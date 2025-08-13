using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Data;
using System.Text.Json;

// Namespace structure check
// "Namespace should follow the pattern 'CIG.ProductName.Area.Feature'."

// for Extensions orient on the Standart Namespace (e.g. SalesHeader TableExtension = CIG.{Product}.Sales.Document)
// Feature is optional but recommended when another capsulation is needed

// Comapny = 'CIG'
// Product = 'CF1' (extracted from the settings.json file - interpretation)
// Area = Name of the top folder or Extended Object (interpretation)
// Feature = custom?

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0000CheckNamespaceStructure : DiagnosticAnalyzer
{
    private const string Company = "CIG";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(DiagnosticDescriptors.Rule0000CheckNamespaceStructure);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxTreeAction(new Action<SyntaxTreeAnalysisContext>(AnalyzeSyntaxTree));
    }

    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext ctx)
    {
        var root = ctx.Tree.GetRoot(ctx.CancellationToken);
        if (root is null)
            return;

        var nsToken = root
            .DescendantTokens()
            .FirstOrDefault(t => t.Kind == SyntaxKind.NamespaceKeyword);


        var nsNode = nsToken.Parent ?? (SyntaxNode)root;

        var candidate = nsNode;
        foreach (var anc in nsNode.AncestorsAndSelf())
        {
            if (anc.DescendantTokens().Any(t => t.Kind == SyntaxKind.SemicolonToken))
            {
                candidate = anc;
                break;
            }
        }

        var rawText = candidate.ToString();
        var idxKw = rawText.IndexOf("namespace", System.StringComparison.OrdinalIgnoreCase);
        if (idxKw < 0)
            return;

        var afterKw = rawText.Substring(idxKw + "namespace".Length);
        var semi = afterKw.IndexOf(';');
        var nsText = (semi >= 0 ? afterKw.Substring(0, semi) : afterKw).Trim();
        if (string.IsNullOrWhiteSpace(nsText))
            return;

        var parts = nsText.Split('.', System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        if (!string.Equals(parts[0], Company, System.StringComparison.OrdinalIgnoreCase))
        {
            ctx.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptors.Rule0000CheckNamespaceStructure,
                                nsToken.GetLocation(),
                                nsText));
            return;
        }

        if (parts.Length >= 2)
        {
            var settingsPath = FindSettingsJson(ctx);
            if (settingsPath != null)
            {
                var productName = ExtractProductNameFromSettings(settingsPath, Company);
                if (!string.IsNullOrWhiteSpace(productName))
                {
                    var nsProduct = parts[1];
                    if (!string.Equals(nsProduct, productName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(
                                            DiagnosticDescriptors.Rule0000CheckNamespaceStructure,
                                            nsToken.GetLocation(),
                                            nsText));
                        return;
                    }
                }
            }
        }

        if (TryGetAreaFromPath(ctx.Tree.FilePath, out var areaFromPath))
        {
            var areaSegs = areaFromPath.Split('.', System.StringSplitOptions.RemoveEmptyEntries);
            if (areaSegs.Length > 0)
            {
                if (parts.Length < 3)
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                                            DiagnosticDescriptors.Rule0000CheckNamespaceStructure,
                                            nsToken.GetLocation(),
                                            nsText));
                    return;
                }

                var nsAreaCombined = string.Join('.', parts.Skip(2).Take(areaSegs.Length));

                if (!string.Equals(nsAreaCombined, areaFromPath, System.StringComparison.OrdinalIgnoreCase))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(
                                            DiagnosticDescriptors.Rule0000CheckNamespaceStructure,
                                            nsToken.GetLocation(),
                                            nsText));
                    return;
                }
            }
        }
    }

    private static string? FindSettingsJson(SyntaxTreeAnalysisContext ctx)
    {
        var currentDir = Path.GetDirectoryName(ctx.Tree.FilePath);
        if (string.IsNullOrEmpty(currentDir))
            return null;

        while (!string.IsNullOrEmpty(currentDir))
        {
            var settingsPath = Path.Combine(currentDir, ".AL-Go", "settings.json");
            if (File.Exists(settingsPath))
                return settingsPath;

            currentDir = Path.GetDirectoryName(currentDir);
        }

        return null;
    }

    private static string? ExtractProductNameFromSettings(string settingsPath, string company)
    {
        if (!File.Exists(settingsPath))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(settingsPath));
            var root = doc.RootElement;

            if (root.TryGetProperty("appSourceCopMandatoryAffixes", out var affixesArray) &&
                affixesArray.ValueKind == JsonValueKind.Array &&
                affixesArray.GetArrayLength() > 0)
            {
                foreach (var el in affixesArray.EnumerateArray())
                {
                    if (el.ValueKind != JsonValueKind.String)
                        continue;

                    var s = el.GetString() ?? string.Empty;
                    if (s.StartsWith(company + " ", System.StringComparison.OrdinalIgnoreCase))
                        return s.Substring(company.Length + 1).Trim();
                }
            }
        }
        catch (JsonException)
        {
            // ignore malformed JSON
        }

        return null;
    }

    private static readonly HashSet<string> ObjectFolders =
        new HashSet<string>(new[]
        {
                    "Codeunit","Codeunits",
                    "Table","Tables","TableExt","TableExtension","TableExtensions",
                    "Page","Pages","PageExt","PageExtension","PageExtensions",
                    "Report","Reports",
                    "Query","Queries",
                    "XmlPort","XmlPorts",
                    "Enum","Enums","EnumExt","EnumExtension","EnumExtensions",
                    "Permissions","PermissionSet","PermissionSets",
                    "Interface","Interfaces",
                    "ControlAddIn","ControlAddIns",
                    "Profile","Profiles","ProfileExtension","ProfileExtensions",
                    "RequestPage","RequestPages",
                    "TestPage","TestPages","TestRequestPage","TestRequestPages"
        }, System.StringComparer.OrdinalIgnoreCase);

    private static bool TryGetAreaFromPath(string filePath, out string area)
    {
        area = string.Empty;
        if (string.IsNullOrEmpty(filePath))
            return false;

        var fileDir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(fileDir))
            return false;

        var appRoot = FindAppRoot(new DirectoryInfo(fileDir));
        if (appRoot is null)
            return false;

        var rel = Path.GetRelativePath(appRoot.FullName, fileDir);
        if (string.IsNullOrEmpty(rel))
            return false;

        var segs = rel.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                             System.StringSplitOptions.RemoveEmptyEntries);
        if (segs.Length == 0)
            return false;

        if (!segs[0].Equals("Microsoft", System.StringComparison.OrdinalIgnoreCase))
        {
            area = segs[0];
            return true;
        }

        var parts = new List<string> { "Microsoft" };
        for (int i = 1; i < segs.Length; i++)
        {
            if (ObjectFolders.Contains(segs[i]))
                break;
            parts.Add(segs[i]);
        }

        area = string.Join('.', parts);
        return true;
    }

    private static DirectoryInfo? FindAppRoot(DirectoryInfo? start)
    {
        var dir = start;
        while (dir != null)
        {
            var appJson = Path.Combine(dir.FullName, "app.json");
            if (File.Exists(appJson))
                return dir;

            dir = dir.Parent;
        }
        return null;
    }

}