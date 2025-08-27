using CountITBCALCop.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0003 – CalcSums vs. Loop
// "When calculating the sum of a field in a filter, CalcSums must always be used."

namespace CountITBCALCop.Design;

[DiagnosticAnalyzer]
public class Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(DiagnosticDescriptors.Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter);

    public override void Initialize(AnalysisContext context)
    {
        // checking method bodys
        context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeMethod));
        // checking trigger bodys
        context.RegisterSyntaxNodeAction(new Action<SyntaxNodeAnalysisContext>(AnalyzeTrigger), SyntaxKind.TriggerDeclaration);
    }

    private static void AnalyzeMethod(CodeBlockAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved() || ctx.CodeBlock is not MethodDeclarationSyntax method || method.Body == null)
            return;

        AnalyzeBodyForCalcSumsPattern(method.Body, d => ctx.ReportDiagnostic(d));
    }

    private static void AnalyzeTrigger(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved())
            return;

        if (ctx.Node is not TriggerDeclarationSyntax trigger || trigger.Body == null)
            return;

        AnalyzeBodyForCalcSumsPattern(trigger.Body, d => ctx.ReportDiagnostic(d));
    }

    private static void AnalyzeBodyForCalcSumsPattern(SyntaxNode body, Action<Diagnostic> report)
    {
        var loops = body.DescendantNodes().Where(n =>
            n is RepeatStatementSyntax ||
            n is WhileStatementSyntax ||
            n is ForStatementSyntax);

        foreach (var loop in loops)
        {
            var recordVars = CollectRecordIterationVariables(loop);
            if (recordVars.Count == 0)
                continue;

            // "+=" compound assignments
            foreach (var comp in loop.DescendantNodes().OfType<CompoundAssignmentStatementSyntax>())
            {
                if (comp.AssignmentToken.Kind != SyntaxKind.AssignPlusToken)
                    continue;

                if (IsPureMemberAccessOnAnyRecord(comp.Source, recordVars))
                {
                    report(Diagnostic.Create(
                        DiagnosticDescriptors.Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter,
                        comp.GetLocation()));
                }
            }

            // ":=" assignments
            foreach (var assign in loop.DescendantNodes().OfType<AssignmentStatementSyntax>())
            {
                if (assign.AssignmentToken.Kind != SyntaxKind.AssignToken || assign.Target is not IdentifierNameSyntax targetId)
                    continue;

                if (assign.Source is BinaryExpressionSyntax bin && bin.Kind == SyntaxKind.AddExpression)
                {
                    bool pattern1 = IsSameIdentifier(targetId, bin.Left) && IsPureMemberAccessOnAnyRecord(bin.Right, recordVars);
                    bool pattern2 = IsPureMemberAccessOnAnyRecord(bin.Left, recordVars) && IsSameIdentifier(targetId, bin.Right);

                    if (pattern1 || pattern2)
                    {
                        report(Diagnostic.Create(
                            DiagnosticDescriptors.Rule0013UseCalcSumWhenCalculationSumOfFieldInFilter,
                            assign.GetLocation()));
                    }
                }
            }
        }
    }

    private static HashSet<string> CollectRecordIterationVariables(SyntaxNode loop)
    {
        var recordVars = new HashSet<string>(StringComparer.Ordinal);

        foreach (var inv in loop.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            // Looking for member calls
            if (inv.Expression is MemberAccessExpressionSyntax ma && ma.Name is IdentifierNameSyntax methodName)
            {
                if (ma.Expression is IdentifierNameSyntax recId)
                {
                    var name = recId.Identifier.ValueText;
                    if (!string.IsNullOrEmpty(name))
                        recordVars.Add(name);
                }
            }
        }

        return recordVars;
    }

    private static bool IsPureMemberAccessOnAnyRecord(SyntaxNode node, HashSet<string> recordVars)
    {
        if (node is not MemberAccessExpressionSyntax ma)
            return false;

        // Require a simple identifier as the base (the record variable)
        if (ma.Expression is IdentifierNameSyntax recId)
        {
            var name = recId.Identifier.ValueText;
            return !string.IsNullOrEmpty(name) && recordVars.Contains(name);
        }

        return false;
    }

    private static bool IsSameIdentifier(IdentifierNameSyntax target, SyntaxNode expr)
        => expr is IdentifierNameSyntax id && id.Identifier.ValueText == target.Identifier.ValueText;
}

