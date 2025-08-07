using System.Collections.Immutable;
using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

// CIG0004 - EventSub Name
// "The name of the event subscriber must match the name of the event it subscribes to."

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0004NameOfEventSubsriberHasToMatchEvent : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.Rule0004NameOfEventSubscriberHasToMatchEvent);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(this.AnalyzeIdentifiersInEventSubscribers));

    private void AnalyzeIdentifiersInEventSubscribers(CodeBlockAnalysisContext ctx)
    {
        if (ctx.CodeBlock is not MethodDeclarationSyntax methodSyntax)
            return;

        var methodName = methodSyntax.Name.Identifier.ValueText;

        var eventSubscriberAttribute = methodSyntax.Attributes
            .FirstOrDefault(attr =>
                SemanticFacts.IsSameName(attr.GetIdentifierOrLiteralValue() ?? "", "EventSubscriber"));

        if (eventSubscriberAttribute is null || eventSubscriberAttribute.ArgumentList.Arguments.Count < 4)
            return;

        var eventNameArg = eventSubscriberAttribute.ArgumentList.Arguments[2];
        var eventName = eventNameArg.GetIdentifierOrLiteralValue(); // e.g. "OnValidate"

        if (string.IsNullOrWhiteSpace(eventName))
            return;

        var elementNameArg = eventSubscriberAttribute.ArgumentList.Arguments[3];
        var elementName = elementNameArg.GetIdentifierOrLiteralValue();

        string expectedName;

        if (!string.IsNullOrWhiteSpace(elementName))
        {
            var cleanedElementName = HelperFunctions.RemoveAlpanumericCharacters(elementName);
            expectedName = cleanedElementName + "_" + eventName;
        }
        else
        {
            expectedName = eventName;
        }

        if (!string.Equals(methodName, expectedName, StringComparison.OrdinalIgnoreCase))
        {
            ctx.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.Rule0004NameOfEventSubscriberHasToMatchEvent, 
                methodSyntax.Name.GetLocation()));
        }
    }
}