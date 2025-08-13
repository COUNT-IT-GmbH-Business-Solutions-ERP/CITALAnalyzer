using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

// CIG0005 - EventSub Parameter
// "In an EventSubscriber, only the parameters that are actually needed should be passed."

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0005CheckForUnnecessaryParamsInEventSub : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(DiagnosticDescriptors.Rule0005CheckForUnnecessaryParamsInEventSub);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeEventSubscriberForUnusedParameters));
    }

    private void AnalyzeEventSubscriberForUnusedParameters(CodeBlockAnalysisContext ctx)
    {
        if (ctx.CodeBlock is not MethodDeclarationSyntax methodSyntax)
            return;

        var eventSubscriberAttribute = methodSyntax.Attributes
            .FirstOrDefault(attr =>
                SemanticFacts.IsSameName(attr.GetIdentifierOrLiteralValue() ?? "", "EventSubscriber"));

        if (eventSubscriberAttribute is null)
            return;

        if (methodSyntax.ParameterList == null || methodSyntax.ParameterList.Parameters.Count == 0)
            return;

        // Collect all identifiers in method body
        var identifiersInBody = methodSyntax.Body?.DescendantNodes()
            .OfType<IdentifierNameSyntax>() 
            .Select(id => id.Identifier.ValueText)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string?>();

        foreach (var parameter in methodSyntax.ParameterList.Parameters)
        {

            string? parameterName = parameter?.GetIdentifierOrLiteralValue() ?? parameter?.Name?.ToString();

            if (!string.IsNullOrWhiteSpace(parameterName) &&
                !identifiersInBody.Contains(parameterName))
            {
                if (parameter is null)
                    continue; 

                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0005CheckForUnnecessaryParamsInEventSub,
                    parameter.GetLocation(),
                    parameterName));
            }
        }
    }
}