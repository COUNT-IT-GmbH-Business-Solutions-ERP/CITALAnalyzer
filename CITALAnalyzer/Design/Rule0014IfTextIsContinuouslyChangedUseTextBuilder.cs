using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0004 – Text. Vs TextBuilder
// "If a text is continuously changed, the data type TextBuilder must be used."

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0014IfTextIsContinuouslyChangedUseTextBuilder : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(DiagnosticDescriptors.Rule0014IfTextIsContinuouslyChangedUseTextBuilder);

    public override void Initialize(AnalysisContext context) =>
    context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeMethod));

    private void AnalyzeMethod(CodeBlockAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved() || ctx.CodeBlock is not MethodDeclarationSyntax method || method.Body is null)
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
        foreach (var comp in loopNode.DescendantNodes().OfType<CompoundAssignmentStatementSyntax>())
        {
            // Only "+="
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
    }

}
