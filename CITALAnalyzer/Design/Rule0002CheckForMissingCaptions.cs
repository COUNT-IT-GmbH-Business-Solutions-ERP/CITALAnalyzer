using System.Collections.Immutable;
using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

// CIG0002 - Table Field Captions
// "Captions must always be defined for table fields, even if the field name is the same as the caption."

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0002CheckForMissingCaptions : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.Rule0002CheckForMissingCaptions);

    private static readonly HashSet<string> _predefinedActionCategoryNames =
        SyntaxFacts.PredefinedActionCategoryNames.Select(x => x.Key.ToLowerInvariant()).ToHashSet();

    public override void Initialize(AnalysisContext context)
        => context.RegisterSymbolAction(new Action<SymbolAnalysisContext>(this.CheckForMissingCaptions), SymbolKind.Field
        );

    private void CheckForMissingCaptions(SymbolAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved() || ctx.Symbol is not IFieldSymbol field)
            return;

        IApplicationObjectTypeSymbol? applicationObject = field.GetContainingApplicationObjectTypeSymbol();
        if (applicationObject is not ITableTypeSymbol || applicationObject.IsObsoletePendingOrRemoved() || field.ContainingSymbol is not ITableTypeSymbol table)
            return;

        if (CaptionIsMissing(field, ctx))
        {
            RaiseCaptionWarning(ctx);
        }
    }

    private bool CaptionIsMissing(ISymbol Symbol, SymbolAnalysisContext context)
    {

        if (Symbol.GetBooleanPropertyValue(PropertyKind.ShowCaption) != false)
            if (Symbol.GetProperty(PropertyKind.Caption) is null && Symbol.GetProperty(PropertyKind.CaptionClass) is null && Symbol.GetProperty(PropertyKind.CaptionML) is null)
                return true;
        return false;
    }

    private void RaiseCaptionWarning(SymbolAnalysisContext context)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.Rule0002CheckForMissingCaptions,
            context.Symbol.GetLocation()));
    }
}