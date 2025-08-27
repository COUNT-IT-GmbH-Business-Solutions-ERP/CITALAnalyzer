using CountITBCALCop.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0005 - Using procedures as Source Expressions
// "Procedures should be specified directly as a Source Expression in a Page Field, not in OnAfterGetRecord()."

namespace CountITBCALCop.Design;

[DiagnosticAnalyzer]
public class Rule0015DefineProcedureAsSourceExpressionInPageField : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    ImmutableArray.Create(DiagnosticDescriptors.Rule0015DefineProcedureAsSourceExpressionInPageField);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(new Action<SyntaxNodeAnalysisContext>(AnalyzePageForOnAfterGetRecord), SyntaxKind.PageObject);
    }

    private static void AnalyzePageForOnAfterGetRecord(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved())
            return;

        var model = ctx.SemanticModel;
        var ct = ctx.CancellationToken;

        // find OnAfterGetRecord
        var onAfterGetRecord = ctx.Node
            .DescendantNodes()
            .OfType<TriggerDeclarationSyntax>()
            .FirstOrDefault(t =>
            {
                var name = t.Name?.Identifier.ValueText;
                return !string.IsNullOrEmpty(name) && SemanticFacts.IsSameName(name, "OnAfterGetRecord");
            });
        if (onAfterGetRecord is null)
            return;

        // globals of the page
        var pageSymbol = model.GetDeclaredSymbol(ctx.Node, ct) as IApplicationObjectTypeSymbol;
        if (pageSymbol is null)
            return;

        var pageGlobals = pageSymbol
            .GetMembers()
            .OfType<IVariableSymbol>()
            .Where(v => v.Kind == SymbolKind.GlobalVariable)
            .ToList();
        if (pageGlobals.Count == 0)
            return;

        // map local procedures (for recursion)
        var localMethods = ctx.Node.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        var methodBySymbol = new Dictionary<IMethodSymbol, MethodDeclarationSyntax>();
        foreach (var m in localMethods)
        {
            var msym = model.GetDeclaredSymbol(m, ct) as IMethodSymbol;
            if (msym != null && !methodBySymbol.ContainsKey(msym))
                methodBySymbol.Add(msym, m);
        }

        // collect globals modified in OnAfterGetRecord and its callees (recursive)
        var modifiedGlobals = new HashSet<IVariableSymbol>();
        var visited = new HashSet<IMethodSymbol>();

        CollectModifiedGlobalsInNode(onAfterGetRecord, model, ct, pageGlobals, methodBySymbol, visited, modifiedGlobals);

        if (modifiedGlobals.Count == 0)
            return;

        // report fields whose SourceExpr (ONLY the header argument, not body/properties) references modified globals
        foreach (var pageField in ctx.Node.DescendantNodes().OfType<PageFieldSyntax>())
        {
            // Determine the position of the opening brace of the field body, if any.
            // We only consider identifiers that occur BEFORE this brace (i.e., in "field(Name; <SourceExpr>)").
            var openBraceToken = pageField.OpenBraceToken;
            var openBracePos = openBraceToken.IsMissing ? int.MaxValue : openBraceToken.FullSpan.Start;

            // All identifier candidates under the field...
            var idCandidates = pageField.DescendantNodes().OfType<IdentifierNameSyntax>();

            foreach (var id in idCandidates)
            {
                // Skip anything in the field BODY/properties (e.g., StyleExpr = StyleExprTxt;)
                if (id.FullSpan.Start >= openBracePos)
                    continue;

                // Only care about identifiers that actually resolve to GLOBAL variables
                var sym = model.GetSymbolInfo(id, ct).Symbol as IVariableSymbol;
                if (sym is null || sym.Kind != SymbolKind.GlobalVariable)
                    continue;

                if (!modifiedGlobals.Contains(sym))
                    continue;

                // At this point, a modified global is being used directly in the field header (SourceExpr)
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0015DefineProcedureAsSourceExpressionInPageField,
                    id.GetLocation()));

                break;
            }
        }
    }

    // recursively walk: collect writes to globals and follow local procedure calls
    private static void CollectModifiedGlobalsInNode(
        SyntaxNode node,
        SemanticModel model,
        System.Threading.CancellationToken ct,
        List<IVariableSymbol> pageGlobals,
        Dictionary<IMethodSymbol, MethodDeclarationSyntax> methodBySymbol,
        HashSet<IMethodSymbol> visited,
        HashSet<IVariableSymbol> modifiedGlobals)
    {
        // ":=" to a global
        foreach (var assign in node.DescendantNodes().OfType<AssignmentStatementSyntax>())
        {
            if (assign.Target is IdentifierNameSyntax targetId)
            {
                var sym = model.GetSymbolInfo(targetId, ct).Symbol as IVariableSymbol;
                if (sym != null && sym.Kind == SymbolKind.GlobalVariable && pageGlobals.Contains(sym))
                    modifiedGlobals.Add(sym);
            }
        }

        // "+=" to a global
        foreach (var comp in node.DescendantNodes().OfType<CompoundAssignmentStatementSyntax>())
        {
            if (comp.AssignmentToken.Kind != SyntaxKind.AssignPlusToken)
                continue;

            if (comp.Target is IdentifierNameSyntax id)
            {
                var sym = model.GetSymbolInfo(id, ct).Symbol as IVariableSymbol;
                if (sym != null && sym.Kind == SymbolKind.GlobalVariable && pageGlobals.Contains(sym))
                    modifiedGlobals.Add(sym);
            }
        }

        // follow local procedure calls
        foreach (var inv in node.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var targetExpr = inv.Expression;
            var msym = model.GetSymbolInfo(targetExpr, ct).Symbol as IMethodSymbol;
            if (msym == null)
                continue;

            if (!methodBySymbol.TryGetValue(msym, out var decl))
                continue; // skip external/codeunit methods

            if (visited.Contains(msym))
                continue;

            visited.Add(msym);
            CollectModifiedGlobalsInNode(decl, model, ct, pageGlobals, methodBySymbol, visited, modifiedGlobals);
        }
    }
}