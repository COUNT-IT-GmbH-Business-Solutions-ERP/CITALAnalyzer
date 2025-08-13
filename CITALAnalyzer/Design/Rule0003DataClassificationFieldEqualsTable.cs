using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Text.Json;

// CIG0003 - DataClassification
// "Omit DataClassification, except for AppSource or when it is explicitly required."

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

        if (IsAppSourceApp(ctx))
            return;

        if (ctx.Symbol is not IFieldSymbol && ctx.Symbol is not ITableTypeSymbol)
            return;

        IPropertySymbol? dataClassification = ctx.Symbol.GetProperty(PropertyKind.DataClassification);
        if (dataClassification is null)
            return;

        if (string.Equals(dataClassification.ValueText, "CustomerContent", StringComparison.Ordinal))
        {
            ctx.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.Rule0003CheckIfDefaultDataClassification,
                dataClassification.GetLocation()));
        }
    }

    private static bool IsAppSourceApp(SymbolAnalysisContext ctx)
    {
        var treePath = ctx.Compilation?.SyntaxTrees.FirstOrDefault()?.FilePath;
        if (string.IsNullOrEmpty(treePath))
            return false;

        var dir = Path.GetDirectoryName(treePath);
        while (!string.IsNullOrEmpty(dir))
        {
            var settings = Path.Combine(dir, ".github", "AL-Go-Settings.json");
            if (File.Exists(settings))
                return IsAppSourceType(settings);

            dir = Path.GetDirectoryName(dir);
        }

        return false;
    }

    private static bool IsAppSourceType(string settingsPath)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(settingsPath));
            var root = doc.RootElement;
            if (root.TryGetProperty("type", out var typeProp))
            {
                var val = typeProp.GetString() ?? string.Empty;
                return val.Equals("AppSource App", System.StringComparison.OrdinalIgnoreCase);
            }
        }
        catch (IOException) { /* treat as non-AppSource */ }
        catch (JsonException) { /* treat as non-AppSource */ }

        return false;
    }
}