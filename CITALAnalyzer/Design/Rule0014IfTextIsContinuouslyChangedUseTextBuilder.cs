using CountITBCALCop.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0004 – Text. Vs TextBuilder
// "If a text is continuously changed, the data type TextBuilder must be used."

namespace CountITBCALCop.Design;

[DiagnosticAnalyzer]
public class Rule0014IfTextIsContinuouslyChangedUseTextBuilder : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(DiagnosticDescriptors.Rule0014IfTextIsContinuouslyChangedUseTextBuilder);

    public override void Initialize(AnalysisContext context)
    {
        // analysing method bodys
        context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeMethod));

        // analysing trigger bodys
        context.RegisterSyntaxNodeAction(new Action<SyntaxNodeAnalysisContext>(AnalyzeTrigger), SyntaxKind.TriggerDeclaration);
    }

    private static void AnalyzeMethod(CodeBlockAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved() || ctx.CodeBlock is not MethodDeclarationSyntax method || method.Body == null)
            return;

        AnalyzeBodyForTextAppendLoops(
            method.Body,
            ctx.SemanticModel,
            ctx.CancellationToken,
            d => ctx.ReportDiagnostic(d));
    }

    private static void AnalyzeTrigger(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved())
            return;

        if (ctx.Node is not TriggerDeclarationSyntax trigger || trigger.Body == null)
            return;

        AnalyzeBodyForTextAppendLoops(
            trigger.Body,
            ctx.SemanticModel,
            ctx.CancellationToken,
            d => ctx.ReportDiagnostic(d));
    }

    // Scans the given body for for/while/repeat loops and reports string-append patterns.
    private static void AnalyzeBodyForTextAppendLoops(
        SyntaxNode body,
        SemanticModel model,
        CancellationToken ct,
        Action<Diagnostic> report)
    {
        foreach (var loop in body.DescendantNodes().OfType<ForStatementSyntax>())
            MarkTextUsagesIn(loop, model, ct, report);

        foreach (var loop in body.DescendantNodes().OfType<WhileStatementSyntax>())
            MarkTextUsagesIn(loop, model, ct, report);

        foreach (var loop in body.DescendantNodes().OfType<RepeatStatementSyntax>())
            MarkTextUsagesIn(loop, model, ct, report);
    }

    private static void MarkTextUsagesIn(
        SyntaxNode loopNode,
        SemanticModel model,
        CancellationToken ct,
        Action<Diagnostic> report)
    {
        // "+="
        foreach (var comp in loopNode.DescendantNodes().OfType<CompoundAssignmentStatementSyntax>())
        {
            if (comp.AssignmentToken.Kind != SyntaxKind.AssignPlusToken)
                continue;

            if (comp.Target is not IdentifierNameSyntax id)
                continue;

            var sym = model.GetSymbolInfo(id, ct).Symbol as IVariableSymbol;
            if (sym is not null && sym.Type.IsTextType())
            {
                report(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0014IfTextIsContinuouslyChangedUseTextBuilder,
                    comp.GetLocation(),
                    sym.Name));
            }
        }

        // ":=" append-only pattern
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

                report(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0014IfTextIsContinuouslyChangedUseTextBuilder,
                    assign.GetLocation(),
                    targetSym.Name));
            }
        }
    }

    private static bool IsLiteral(SyntaxNode node) =>
        node is LiteralExpressionSyntax;

    private static bool IsSameIdentifier(IdentifierNameSyntax target, SyntaxNode expr)
    {
        if (expr is IdentifierNameSyntax id)
        {
            var lhs = target.Identifier.ValueText;
            var rhs = id.Identifier.ValueText;
            return !string.IsNullOrEmpty(lhs) && lhs == rhs;
        }
        return false;
    }
}
