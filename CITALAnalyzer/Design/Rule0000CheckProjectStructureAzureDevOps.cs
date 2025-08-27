using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CountITBCALCop.Design;

// Rule0000CheckProjectStructureAzureDevOps

[DiagnosticAnalyzer]
public class Rule0000CheckProjectStructureAzureDevOps : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.Rule0000CheckProjectStructureAzureDevOps);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(new Action<SyntaxNodeAnalysisContext>(Analyze), new SyntaxKind[]
        {
            SyntaxKind.CodeunitObject,
            SyntaxKind.TableObject,
            SyntaxKind.TableExtensionObject,
            SyntaxKind.PageObject,
            SyntaxKind.PageExtensionObject,
            SyntaxKind.ReportObject,
            SyntaxKind.XmlPortObject,
            SyntaxKind.QueryObject,
            SyntaxKind.ControlAddInObject,
            SyntaxKind.ReportExtensionObject,
            SyntaxKind.ProfileObject,
            SyntaxKind.ProfileExtensionObject,
            SyntaxKind.PageCustomizationObject,
            SyntaxKind.Interface,
            SyntaxKind.EnumType,
            SyntaxKind.EnumExtensionType
        });
    }

    private static void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        var filePath = ctx.Node.SyntaxTree?.FilePath;
        if (string.IsNullOrEmpty(filePath))
            return;

        var fullPath = filePath.Replace('\\', '/');

        // Only check inside /App/ or /Test/
        var appMatch = Regex.Match(fullPath, @"(?i)/(App|Test)/");
        if (!appMatch.Success)
            return;

        if (IsInfraPath(fullPath))
            return;

        // Determine expected kind folder(s)
        var expectedKindFolders = GetExpectedKindFolders(ctx);
        if (expectedKindFolders is null || expectedKindFolders.Count == 0)
            return;

        var expectedKindDisplay = expectedKindFolders.Count == 1
            ? expectedKindFolders[0]
            : "{" + string.Join("|", expectedKindFolders) + "}";

        var anchorName = appMatch.Groups[1].Value;          
        var anchorSeg = "/" + anchorName + "/";

        var (projectName, currentRel) = ProjectRelative(fullPath, anchorSeg);

        var idxAnchor = IndexOf(fullPath, anchorSeg);
        if (idxAnchor < 0) return;
        var after = fullPath.Substring(idxAnchor + anchorSeg.Length);
        var parts = after.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return; // at least "<Kind>/<File>" expected at the end

        // Immediate parent must be one of the allowed kind folders
        var fileName = parts[^1];
        var parentFolder = parts[^2];
        var parentIsKind = expectedKindFolders.Any(k => parentFolder.Equals(k, StringComparison.OrdinalIgnoreCase));

        var parentIndex = parts.Length - 2;
        var hasArea = parentIndex >= 1;

        // Derive Area (and optional Feature) from namespace: company.project.area[.feature[...]]
        var (area, feature) = GetAreaFeatureFromNamespace(ctx);

        // Validate namespace-mapped folders if provided:
        bool areaMatches = true, featureMatches = true;
        if (!string.IsNullOrEmpty(area))
        {
            var idxArea = IndexOfPart(parts, area!, 0, parentIndex);
            areaMatches = idxArea >= 0;

            if (!string.IsNullOrEmpty(feature))
            {
                var idxFeature = IndexOfPart(parts, feature!, (idxArea >= 0 ? idxArea + 1 : 0), parentIndex);
                featureMatches = idxFeature >= 0;
            }
        }

        var ok = parentIsKind && hasArea && areaMatches && featureMatches;
        if (!ok)
        {
            string expectedRel;
            if (projectName is not null)
            {
                if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(feature))
                    expectedRel = $"{projectName}/{anchorName}/{area}/{feature}/.../{expectedKindDisplay}/";
                else if (!string.IsNullOrEmpty(area))
                    expectedRel = $"{projectName}/{anchorName}/{area}/.../{expectedKindDisplay}/";
                else
                    expectedRel = $"{projectName}/{anchorName}/<Area>/.../{expectedKindDisplay}/";
            }
            else
            {
                if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(feature))
                    expectedRel = $"<Project>/{anchorName}/{area}/{feature}/.../{expectedKindDisplay}/";
                else if (!string.IsNullOrEmpty(area))
                    expectedRel = $"<Project>/{anchorName}/{area}/.../{expectedKindDisplay}/";
                else
                    expectedRel = $"<Project>/{anchorName}/<Area>/.../{expectedKindDisplay}/";
            }

            var location = FileLevelLocation(ctx);
            var objectLabel = KindToLabel(ctx.Node.Kind);

            ctx.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.Rule0000CheckProjectStructureAzureDevOps,
                location,
                objectLabel,
                Path.GetFileName(filePath),
                expectedRel,
                currentRel));
        }
    }

    private static bool IsInfraPath(string path) =>
        Regex.IsMatch(path, @"(?i)/(App|Test)/(\.vscode|\.alpackages|\.altestrunner|\.snapshots|Translations)(/|$)");

    // Returns the set of allowed folder names for the object kind.
    private static List<string>? GetExpectedKindFolders(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node.Kind == SyntaxKind.CodeunitObject)
        {
            // treat codeunits with [EventSubscriber] anywhere as EventSub
            var containsSubscriber = ctx.Node
                .DescendantNodes()
                .Any(n => n.Kind == SyntaxKind.MemberAttribute &&
                          n.ToString().IndexOf("EventSubscriber", StringComparison.OrdinalIgnoreCase) >= 0);

            if (containsSubscriber)
                return new List<string> { "EventSub" };

            return new List<string> { "Codeunit", "Helper" };
        }

        return ctx.Node.Kind switch
        {
            SyntaxKind.TableObject => new List<string> { "Table" },
            SyntaxKind.TableExtensionObject => new List<string> { "TableExt" },
            SyntaxKind.PageObject => new List<string> { "Page", "APIPage" },
            SyntaxKind.PageExtensionObject => new List<string> { "PageExt" },
            SyntaxKind.ReportObject => new List<string> { "Report" },
            SyntaxKind.QueryObject => new List<string> { "Query" },
            SyntaxKind.XmlPortObject => new List<string> { "XmlPort" },
            SyntaxKind.ControlAddInObject => new List<string> { "ControlAddIn" },
            SyntaxKind.ReportExtensionObject => new List<string> { "ReportExt" },
            SyntaxKind.ProfileObject => new List<string> { "Profile" },
            SyntaxKind.ProfileExtensionObject => new List<string> { "ProfileExt" },
            SyntaxKind.PageCustomizationObject => new List<string> { "PageCustomization" },
            SyntaxKind.Interface => new List<string> { "Interface" },
            SyntaxKind.EnumType => new List<string> { "Enum" },
            SyntaxKind.EnumExtensionType => new List<string> { "EnumExt" },
            _ => null
        };
    }

    private static string KindToLabel(SyntaxKind kind) => kind switch
    {
        SyntaxKind.TableObject => "Table",
        SyntaxKind.TableExtensionObject => "TableExt",
        SyntaxKind.PageObject => "Page",
        SyntaxKind.PageExtensionObject => "PageExt",
        SyntaxKind.ReportObject => "Report",
        SyntaxKind.QueryObject => "Query",
        SyntaxKind.XmlPortObject => "XmlPort",
        SyntaxKind.ControlAddInObject => "ControlAddIn",
        SyntaxKind.ReportExtensionObject => "ReportExt",
        SyntaxKind.ProfileObject => "Profile",
        SyntaxKind.ProfileExtensionObject => "ProfileExt",
        SyntaxKind.PageCustomizationObject => "PageCustomization",
        SyntaxKind.CodeunitObject => "Codeunit",
        SyntaxKind.Interface => "Interface",
        SyntaxKind.EnumType => "Enum",
        SyntaxKind.EnumExtensionType => "EnumExt",
        _ => "Object"
    };

    private static int IndexOf(string hay, string needle) =>
        CultureInfo.InvariantCulture.CompareInfo.IndexOf(hay, needle, CompareOptions.IgnoreCase);

    private static int IndexOfPart(string[] parts, string value, int start, int endExclusive)
    {
        for (int i = start; i < endExclusive && i < parts.Length; i++)
            if (parts[i].Equals(value, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private static Location FileLevelLocation(SyntaxNodeAnalysisContext ctx)
    {
        var tree = ctx.Node.SyntaxTree!;
        var text = tree.GetText();
        return Location.Create(tree, new TextSpan(text.Length, 0));
    }

    // Extracts Area and optional Feature from the first namespace:
    private static (string? area, string? feature) GetAreaFeatureFromNamespace(SyntaxNodeAnalysisContext ctx)
    {
        var ns = ctx.Node.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        var name = ns?.Name?.ToString();
        if (string.IsNullOrWhiteSpace(name))
            return (null, null);

        var segments = name.Split('.');
        if (segments.Length >= 3)
        {
            var area = segments[2];
            var feature = segments.Length >= 4 ? segments[3] : null;
            return (area, feature);
        }
        return (null, null);
    }

    // Returns (projectName, projectRelativePath) where projectRelativePath starts at the project folder.
    private static (string? projectName, string relative) ProjectRelative(string fullPath, string appOrTestSeg)
    {
        var idxAnchor = IndexOf(fullPath, appOrTestSeg);
        if (idxAnchor < 0)
            return (null, fullPath);

        // previous '/' before '/App/' or '/Test/'
        var prevSlash = fullPath.LastIndexOf('/', idxAnchor - 1);
        if (prevSlash >= 0)
        {
            var rel = fullPath.Substring(prevSlash + 1); // "<Project>/App/.../File.al"
            var projectName = fullPath.Substring(prevSlash + 1, idxAnchor - (prevSlash + 1)); // "<Project>"
            return (projectName, rel);
        }

        // fallback: from anchor onward
        return (null, fullPath.Substring(idxAnchor + 1));
    }
}