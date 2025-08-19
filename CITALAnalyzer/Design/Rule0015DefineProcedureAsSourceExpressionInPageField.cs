using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0005 - Prozdeuren als Source Expression verwenden
// "Procedures should be specified directly as a Source Expression in a Page Field, not in OnAfterGetRecord()."

//
// extend to procedure calls procedure

namespace CITALAnalyzer.Design;

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

        // return if no OnAfterGetRecord trigger
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

        // return if no global variables
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

        // collect globals set with result of procedure call in InAfterGetReccord Trigger
        var globalsAssignedFromCall = new HashSet<IVariableSymbol>();
        foreach (var assign in onAfterGetRecord.DescendantNodes().OfType<AssignmentStatementSyntax>())
        {
            if (assign.Target is not IdentifierNameSyntax targetId)
                continue;

            if (model.GetSymbolInfo(targetId, ct).Symbol is not IVariableSymbol targetSym)
                continue;

            if (targetSym.Kind != SymbolKind.GlobalVariable || !pageGlobals.Contains(targetSym))
                continue;

            bool rhsHasCall =
                assign.Source is InvocationExpressionSyntax
                || assign.Source.DescendantNodes().OfType<InvocationExpressionSyntax>().Any();

            if (rhsHasCall)
                globalsAssignedFromCall.Add(targetSym);
        }

        if (globalsAssignedFromCall.Count == 0)
            return; 

        // check if gloabls are used as SourceExpression
        foreach (var pageField in ctx.Node.DescendantNodes().OfType<PageFieldSyntax>())
        {
            var idCandidates = pageField.DescendantNodes().OfType<IdentifierNameSyntax>();

            foreach (var id in idCandidates)
            {
                var sym = model.GetSymbolInfo(id, ct).Symbol as IVariableSymbol;
                if (sym is null || sym.Kind != SymbolKind.GlobalVariable)
                    continue;

                if (!globalsAssignedFromCall.Contains(sym))
                    continue;

                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0015DefineProcedureAsSourceExpressionInPageField,
                    id.GetLocation()));

                break;
            }
        }
    }

}
