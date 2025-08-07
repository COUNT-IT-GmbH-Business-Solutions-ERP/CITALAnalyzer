using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text.Json;

// CIG0003 - DataClassification
// "Omit DataClassification, except for AppSource or when it is explicitly required."

// TODO: needs to be tested in context of appsource apps

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0003CheckIfDefaultDataClassification : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.Rule0003CheckIfDefaultDataClassification);

    public override void Initialize(AnalysisContext context) =>
        context.RegisterSymbolAction(new Action<SymbolAnalysisContext>(CheckDataClassificationRedundancy), 
            SymbolKind.Field,
            SymbolKind.Table
            );

    private void CheckDataClassificationRedundancy(SymbolAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved())
            return;

        if (IsAppSourceApp(ctx.Options, ctx.CancellationToken))
            return;

        if (ctx.Symbol is not IFieldSymbol && ctx.Symbol is not ITableTypeSymbol)
            return;

        IPropertySymbol? dataClassification = ctx.Symbol.GetProperty(PropertyKind.DataClassification);
        if (dataClassification is null)
            return;

        if (dataClassification.ValueText == "CustomerContent")
        {
            ctx.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.Rule0003CheckIfDefaultDataClassification,
                dataClassification.GetLocation()));
        }
    }

    private bool IsAppSourceApp(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        foreach (var additionalFile in options.AdditionalFiles)
        {
            var path = additionalFile.Path.Replace("\\", "/");

            if (path.EndsWith("/.github/AL-Go-Settings.json", StringComparison.OrdinalIgnoreCase))
            {
                var text = additionalFile.GetText(cancellationToken);
                if (text == null)
                    continue;

                try
                {
                    using var jsonDoc = JsonDocument.Parse(text.ToString());
                    if (jsonDoc.RootElement.TryGetProperty("type", out var typeProperty))
                    {
                        string typeValue = typeProperty.GetString() ?? "";
                        return typeValue.Equals("AppSource App", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (JsonException)
                {
                    // Invalid JSON – treat as non-AppSource
                }
            }
        }

        return false;
    }
}