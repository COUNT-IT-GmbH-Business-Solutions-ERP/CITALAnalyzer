using CountITBCALCop.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

// CIGP0002 - SetAutoCalcFields vs. CalcFields
// "Always use SetAutoCalcFields instead of CalcFields."

namespace CountITBCALCop.Design;

[DiagnosticAnalyzer]
public class Rule0012UseSetAutoCalcFieldsInsteadOfCalcFields : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(DiagnosticDescriptors.Rule0012UseSetAutoCalcFieldsInsteadOfCalcFields);

    public override void Initialize(AnalysisContext context) =>
            context.RegisterOperationAction(new Action<OperationAnalysisContext>(AnalyzeInvocation), OperationKind.InvocationExpression);

    private static void AnalyzeInvocation(OperationAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved() || ctx.Operation is not IInvocationExpression op)
            return;

        if (op.TargetMethod?.MethodKind != MethodKind.BuiltInMethod)
            return;

        if (!string.Equals(op.TargetMethod.Name, "CalcFields", StringComparison.OrdinalIgnoreCase))
            return;

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.Rule0012UseSetAutoCalcFieldsInsteadOfCalcFields,
            op.Syntax.GetLocation()));
    }
}
