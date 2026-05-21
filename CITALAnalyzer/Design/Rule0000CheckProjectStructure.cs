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

// Rule0000CheckProjectStructure
// Enforces folder structure INSIDE apps (App/Test), independent of repo or project layout.
//
// Expected Area and Feature are derived from the namespace:
// CIG.{Product}.{Area}.{Feature}
//
// APP structure requires a type folder:
// App/Sales/Posting/Codeunit/MyCodeunit.al
//
// TEST structure allows omitting the type folder:
// Test/Sales/Posting/MyTest.al
// Test/Sales/MyTest.al

[DiagnosticAnalyzer]
public class Rule0000CheckProjectStructure : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.Rule0000CheckProjectStructure);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(
            Analyze,
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
            SyntaxKind.EnumExtensionType);
    }

    private static void Analyze(SyntaxNodeAnalysisContext ctx)
    {
        var filePath = ctx.Node.SyntaxTree?.FilePath;
        if (string.IsNullOrEmpty(filePath))
            return;

        var fullPath = filePath.Replace('\\', '/');

        // Match:
        // <Anything>/<AppName>/(App|Test)/...
        var match = Regex.Match(fullPath, @"(?i)/([^/]+)/(App|Test)/");
        if (!match.Success)
            return;

        // Ignore infrastructure folders
        if (IsInfraPath(fullPath))
            return;

        var appName = match.Groups[1].Value;
        var anchorName = match.Groups[2].Value;
        var isTestApp = anchorName.Equals("Test", StringComparison.OrdinalIgnoreCase);

        var anchorSeg = $"/{appName}/{anchorName}/";

        var idxAnchor = IndexOf(fullPath, anchorSeg);
        if (idxAnchor < 0)
            return;

        // Relative path inside App/Test root
        var after = fullPath.Substring(idxAnchor + anchorSeg.Length);

        var parts = after.Split('/', System.StringSplitOptions.RemoveEmptyEntries);

        // Need at least:
        // <Area>/<File>
        if (parts.Length < 2)
            return;

        var expectedKindFolders = GetExpectedKindFolders(ctx);
        if (expectedKindFolders is null || expectedKindFolders.Count == 0)
            return;

        var expectedKindDisplay = expectedKindFolders.Count == 1
            ? expectedKindFolders[0]
            : "{" + string.Join("|", expectedKindFolders) + "}";

        string parentFolder;
        int parentIndex;
        bool parentIsKind;
        bool hasArea;

        if (isTestApp)
        {
            // TEST:
            // Allow:
            // <Area>/<File>
            // <Area>/<Feature>/<File>

            parentFolder = parts[^2];

            // Entire path before file can contain Area/Feature
            parentIndex = parts.Length - 1;

            // Type folder is optional in tests
            parentIsKind = true;

            hasArea = parts.Length >= 2;
        }
        else
        {
            // APP:
            // Require:
            // <Area>/<Kind>/<File>
            // <Area>/<Feature>/<Kind>/<File>

            parentFolder = parts[^2];

            parentIndex = parts.Length - 2;

            parentIsKind = expectedKindFolders
                .Any(k => parentFolder.Equals(k, StringComparison.OrdinalIgnoreCase));

            hasArea = parentIndex >= 1;
        }

        // Extract Area/Feature from namespace
        var (area, feature) = GetAreaFeatureFromNamespace(ctx);

        bool areaMatches = true;
        bool featureMatches = true;

        if (!string.IsNullOrEmpty(area))
        {
            var idxArea = IndexOfPart(parts, area!, 0, parentIndex);

            areaMatches = idxArea >= 0;

            if (!string.IsNullOrEmpty(feature))
            {
                var idxFeature = IndexOfPart(
                    parts,
                    feature!,
                    (idxArea >= 0 ? idxArea + 1 : 0),
                    parentIndex);

                featureMatches = idxFeature >= 0;
            }
        }

        // Base validation
        var ok = hasArea && areaMatches && featureMatches;

        // Only APP requires kind folder
        if (!isTestApp)
            ok &= parentIsKind;

        if (ok)
            return;

        // Build expected relative path
        string expectedRel;

        if (isTestApp)
        {
            if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(feature))
                expectedRel = $"{appName}/{anchorName}/{area}/{feature}/";
            else if (!string.IsNullOrEmpty(area))
                expectedRel = $"{appName}/{anchorName}/{area}/";
            else
                expectedRel = $"{appName}/{anchorName}/<Area>/[<Feature>]/";
        }
        else
        {
            if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(feature))
                expectedRel = $"{appName}/{anchorName}/{area}/{feature}/{expectedKindDisplay}/";
            else if (!string.IsNullOrEmpty(area))
                expectedRel = $"{appName}/{anchorName}/{area}/{expectedKindDisplay}/";
            else
                expectedRel = $"{appName}/{anchorName}/<Area>/[<Feature>]/{expectedKindDisplay}/";
        }

        var currentRel = fullPath.Substring(idxAnchor + 1);

        var location = FileLevelLocation(ctx);

        var objectLabel = KindToLabel(ctx.Node.Kind);

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.Rule0000CheckProjectStructure,
            location,
            objectLabel,
            Path.GetFileName(filePath),
            expectedRel,
            currentRel));
    }

    private static bool IsInfraPath(string path) =>
        Regex.IsMatch(
            path,
            @"(?i)/(App|Test)/(\.vscode|\.alpackages|\.altestrunner|\.snapshots|Translations)(/|$)");

    private static List<string>? GetExpectedKindFolders(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node.Kind == SyntaxKind.CodeunitObject)
        {
            var isEventSub = ctx.Node
                .DescendantNodes()
                .Any(n =>
                    n.Kind == SyntaxKind.MemberAttribute &&
                    n.ToString().IndexOf(
                        "EventSubscriber",
                        StringComparison.OrdinalIgnoreCase) >= 0);

            if (isEventSub)
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
        CultureInfo.InvariantCulture.CompareInfo.IndexOf(
            hay,
            needle,
            CompareOptions.IgnoreCase);

    private static int IndexOfPart(
        string[] parts,
        string value,
        int start,
        int endExclusive)
    {
        for (int i = start; i < endExclusive && i < parts.Length; i++)
        {
            if (parts[i].Equals(value, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private static Location FileLevelLocation(SyntaxNodeAnalysisContext ctx)
    {
        var tree = ctx.Node.SyntaxTree!;

        var text = tree.GetText();

        return Location.Create(tree, new TextSpan(text.Length, 0));
    }

    private static (string? area, string? feature) GetAreaFeatureFromNamespace(
        SyntaxNodeAnalysisContext ctx)
    {
        var root = ctx.Node.SyntaxTree.GetRoot();

        var ns = root
            .DescendantNodes()
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault();

        var name = ns?.Name?.ToString();

        if (string.IsNullOrWhiteSpace(name))
            return (null, null);

        var segments = name.Split('.', StringSplitOptions.RemoveEmptyEntries);

        // CIG.{Product}.{Area}.{Feature}[.*]
        if (segments.Length >= 3)
        {
            var area = segments[2];

            var feature = segments.Length >= 4
                ? segments[3]
                : null;

            return (area, feature);
        }

        return (null, null);
    }
}