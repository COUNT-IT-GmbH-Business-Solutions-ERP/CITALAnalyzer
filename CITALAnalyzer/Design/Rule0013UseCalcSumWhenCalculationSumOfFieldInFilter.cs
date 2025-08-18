using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0003 – CalcSums vs. Loop
// "When calculating the sum of a field in a filter, CalcSums must always be used."

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(DiagnosticDescriptors.Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeMethod));

    private static void AnalyzeMethod(CodeBlockAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved() || ctx.CodeBlock is not MethodDeclarationSyntax method || method.Body == null)
            return;

        var repeats = method.Body.DescendantNodes().OfType<RepeatStatementSyntax>();
        if (!repeats.Any())
            return;

        foreach (var repeat in repeats)
        {

            // +=
            foreach (var comp in repeat.DescendantNodes().OfType<CompoundAssignmentStatementSyntax>())
            {
                if (comp.AssignmentToken.Kind != SyntaxKind.AssignPlusToken)
                    continue;

                if (IsPureMemberAccess(comp.Source))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter,
                        comp.GetLocation()));
                }
            }

            // :=
            foreach (var assign in repeat.DescendantNodes().OfType<AssignmentStatementSyntax>())
            {
                if (assign.AssignmentToken.Kind != SyntaxKind.AssignToken || assign.Target is not IdentifierNameSyntax targetId)
                    continue;

                if (assign.Source is BinaryExpressionSyntax bin && bin.Kind == SyntaxKind.AddExpression)
                {
                    if (IsSameIdentifier(targetId, bin.Left) && IsPureMemberAccess(bin.Right) ||
                        IsPureMemberAccess(bin.Left) && IsSameIdentifier(targetId, bin.Right))
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter,
                            assign.GetLocation()));
                        continue;
                    }

                }
            }
        }

    }

    private static bool IsPureMemberAccess(SyntaxNode node) =>
            node is MemberAccessExpressionSyntax;

    private static bool IsSameIdentifier(IdentifierNameSyntax target, SyntaxNode expr)
            => expr is IdentifierNameSyntax id && id.Identifier.ValueText == target.Identifier.ValueText;

}
