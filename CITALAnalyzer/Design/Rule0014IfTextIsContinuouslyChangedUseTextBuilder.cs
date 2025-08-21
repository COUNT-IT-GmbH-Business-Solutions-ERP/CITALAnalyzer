using CountITBCALCop.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0004 – Text. Vs TextBuilder
// "If a text is continuously changed, the data type TextBuilder must be used."

// issue: only checks in methods, needs to check all repeat/for/while loops 
// eg not checking triggers

namespace CountITBCALCop.Design;

[DiagnosticAnalyzer]
public class Rule0014IfTextIsContinuouslyChangedUseTextBuilder : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(DiagnosticDescriptors.Rule0014IfTextIsContinuouslyChangedUseTextBuilder);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeMethod));

    private static void AnalyzeMethod(CodeBlockAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved() || ctx.CodeBlock is not MethodDeclarationSyntax method || method.Body == null) // method is not enough
            return;

        var model = ctx.SemanticModel;
        var ct = ctx.CancellationToken;

        foreach (var loop in method.Body.DescendantNodes().OfType<ForStatementSyntax>())
            MarkTextUsagesIn(loop, model, ct, ctx);
        foreach (var loop in method.Body.DescendantNodes().OfType<WhileStatementSyntax>())
            MarkTextUsagesIn(loop, model, ct, ctx);
        foreach (var loop in method.Body.DescendantNodes().OfType<RepeatStatementSyntax>())
            MarkTextUsagesIn(loop, model, ct, ctx);
    }

    private static void MarkTextUsagesIn(
        SyntaxNode loopNode,
        SemanticModel model,
        System.Threading.CancellationToken ct,
        CodeBlockAnalysisContext ctx)
    {
        // "+="
        foreach (var comp in loopNode.DescendantNodes().OfType<CompoundAssignmentStatementSyntax>())
        {
            if (comp.AssignmentToken.Kind != SyntaxKind.AssignPlusToken)
                continue;
            if (comp.Target is not IdentifierNameSyntax id)
                continue;

            if (model.GetSymbolInfo(id, ct).Symbol is IVariableSymbol vsym && vsym.Type.IsTextType())
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0014IfTextIsContinuouslyChangedUseTextBuilder,
                    comp.GetLocation(),
                    vsym.Name));
            }
        }

        // ":=" (append only: textVar := textVar + 'Text')
        foreach (var assign in loopNode.DescendantNodes().OfType<AssignmentStatementSyntax>())
        {
            if (assign.AssignmentToken.Kind != SyntaxKind.AssignToken || assign.Target is not IdentifierNameSyntax targetId)
                continue;

            if (model.GetSymbolInfo(targetId, ct).Symbol is not IVariableSymbol targetSym || !targetSym.Type.IsTextType())
                continue;

            if (assign.Source is BinaryExpressionSyntax bin && bin.Kind == SyntaxKind.AddExpression)
            {
                if (!IsSameIdentifier(targetId, bin.Left))
                    continue;

                if (!IsLiteral(bin.Right))
                    continue;

                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0014IfTextIsContinuouslyChangedUseTextBuilder,
                    assign.GetLocation(),
                    targetSym.Name));
            }
        }
    }

    private static bool IsLiteral(SyntaxNode node) =>
        node is LiteralExpressionSyntax;

    private static bool IsSameIdentifier(IdentifierNameSyntax target, SyntaxNode expr) =>
        expr is IdentifierNameSyntax id && id.Identifier.ValueText == target.Identifier.ValueText;
}