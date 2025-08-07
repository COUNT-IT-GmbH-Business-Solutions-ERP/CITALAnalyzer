using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIG0008 - EventSub File Name
// "Event subscribers must always be placed in a codeunit that has the same name as the object to which the event belongs."

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0008EventSubsInCorrectCodeunit : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.Rule0008EventSubsInCorrectCodeunit);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSymbolAction(AnalyzeCodeunitForEventSubscribers, SymbolKind.Codeunit);

    private void AnalyzeCodeunitForEventSubscribers(SymbolAnalysisContext context)
    {
        if (context.Symbol is not ICodeunitTypeSymbol codeunitSymbol || context.IsObsoletePendingOrRemoved())
            return;

        var eventPublisherNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in codeunitSymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol || !methodSymbol.IsEventSubscriber())
                continue;

            var eventSubscriberAttribute = methodSymbol.Attributes
                .FirstOrDefault(attr => attr.AttributeKind == AttributeKind.EventSubscriber);

            if (eventSubscriberAttribute is null || eventSubscriberAttribute.Arguments.Length < 2)
                continue;

            var publisherObjectName = eventSubscriberAttribute.Arguments[1].ValueText;
            if (string.IsNullOrWhiteSpace(publisherObjectName))
                continue;

            eventPublisherNames.Add(HelperFunctions.RemoveAlpanumericCharacters(publisherObjectName));
        }

        if (eventPublisherNames.Count > 1)
        {
            foreach (var member in codeunitSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                if (!member.IsEventSubscriber())
                    continue;

                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0008EventSubsInCorrectCodeunit,
                    member.GetLocation(),
                    codeunitSymbol.Name));
            }
        }
    }
}