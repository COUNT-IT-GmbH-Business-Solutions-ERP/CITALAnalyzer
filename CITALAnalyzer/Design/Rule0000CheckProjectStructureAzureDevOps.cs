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

        // Determine expected kind folder (Codeunit, EventSub, PageExt, ...)
        var expectedKindFolder = GetExpectedKindFolder(ctx);
        if (expectedKindFolder is null)
            return;

        var anchorName = appMatch.Groups[1].Value;           // "App" | "Test"
        var anchorSeg = "/" + anchorName + "/";

        var (projectName, currentRel) = ProjectRelative(fullPath, anchorSeg);

        var idxAnchor = IndexOf(fullPath, anchorSeg);
        if (idxAnchor < 0) return;
        var after = fullPath.Substring(idxAnchor + anchorSeg.Length);
        var parts = after.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return; // at least "<Kind>/<File>" expected at the end

        // Immediate parent must be the kind folder
        var fileName = parts[^1];
        var parentFolder = parts[^2];
        var parentIsKind = parentFolder.Equals(expectedKindFolder, System.StringComparison.OrdinalIgnoreCase);

        // There must be at least one folder between App/Test and the kind (Area required, Feature optional)
        var parentIndex = parts.Length - 2;       
        var hasArea = parentIndex >= 1;        

        // Derive Area (and optional Feature) from namespace: company.project.area[.feature[...]]
        var (area, feature) = GetAreaFeatureFromNamespace(ctx);

        // Validate namespace-mapped folders if provided:
        // - Area must appear somewhere before the kind
        // - Feature (if provided) must appear after Area and before the kind
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
            // Build expected project-relative pattern using namespace-derived segments when available
            string expectedRel;
            if (projectName is not null)
            {
                if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(feature))
                    expectedRel = $"{projectName}/{anchorName}/{area}/{feature}/.../{expectedKindFolder}/";
                else if (!string.IsNullOrEmpty(area))
                    expectedRel = $"{projectName}/{anchorName}/{area}/.../{expectedKindFolder}/";
                else
                    expectedRel = $"{projectName}/{anchorName}/<Area>/.../{expectedKindFolder}/";
            }
            else
            {
                if (!string.IsNullOrEmpty(area) && !string.IsNullOrEmpty(feature))
                    expectedRel = $"<Project>/{anchorName}/{area}/{feature}/.../{expectedKindFolder}/";
                else if (!string.IsNullOrEmpty(area))
                    expectedRel = $"<Project>/{anchorName}/{area}/.../{expectedKindFolder}/";
                else
                    expectedRel = $"<Project>/{anchorName}/<Area>/.../{expectedKindFolder}/";
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

    private static string? GetExpectedKindFolder(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node.Kind == SyntaxKind.CodeunitObject)
        {
            // treat codeunits with [EventSubscriber] anywhere as EventSub
            var containsSubscriber = ctx.Node
                .DescendantNodes()
                .Any(n => n.Kind == SyntaxKind.MemberAttribute &&
                          n.ToString().IndexOf("EventSubscriber", System.StringComparison.OrdinalIgnoreCase) >= 0);

            return containsSubscriber ? "EventSub" : "Codeunit";
        }

        return ctx.Node.Kind switch
        {
            SyntaxKind.TableObject => "Table",
            SyntaxKind.TableExtensionObject => "TableExt",
            SyntaxKind.PageObject => "Page",
            SyntaxKind.PageExtensionObject => "PageExt",
            SyntaxKind.ReportObject => "Report",
            SyntaxKind.QueryObject => "Query",
            SyntaxKind.XmlPortObject => "XmlPort",
            SyntaxKind.ControlAddInObject => "ControlAddIn",
            SyntaxKind.ReportExtensionObject => "ReportExtension",
            SyntaxKind.ProfileObject => "Profile",
            SyntaxKind.ProfileExtensionObject => "ProfileExtension",
            SyntaxKind.PageCustomizationObject => "PageCustomization",
            SyntaxKind.Interface => "Interface",
            SyntaxKind.EnumType => "Enum",
            SyntaxKind.EnumExtensionType => "EnumExtension",
            _ => null
        };
    }

    private static string KindToLabel(SyntaxKind kind) => kind switch
    {
        SyntaxKind.TableObject => "Table",
        SyntaxKind.TableExtensionObject => "TableExtension",
        SyntaxKind.PageObject => "Page",
        SyntaxKind.PageExtensionObject => "PageExtension",
        SyntaxKind.ReportObject => "Report",
        SyntaxKind.QueryObject => "Query",
        SyntaxKind.XmlPortObject => "XmlPort",
        SyntaxKind.ControlAddInObject => "ControlAddIn",
        SyntaxKind.ReportExtensionObject => "ReportExtension",
        SyntaxKind.ProfileObject => "Profile",
        SyntaxKind.ProfileExtensionObject => "ProfileExtension",
        SyntaxKind.PageCustomizationObject => "PageCustomization",
        SyntaxKind.CodeunitObject => "Codeunit",
        SyntaxKind.Interface => "Interface",
        SyntaxKind.EnumType => "Enum",
        SyntaxKind.EnumExtensionType => "EnumExtension",
        _ => "Object"
    };

    private static int IndexOf(string hay, string needle) =>
        CultureInfo.InvariantCulture.CompareInfo.IndexOf(hay, needle, CompareOptions.IgnoreCase);

    private static int IndexOfPart(string[] parts, string value, int start, int endExclusive)
    {
        for (int i = start; i < endExclusive && i < parts.Length; i++)
            if (parts[i].Equals(value, System.StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private static Location FileLevelLocation(SyntaxNodeAnalysisContext ctx)
    {
        var tree = ctx.Node.SyntaxTree!;
        var text = tree.GetText();
        // EOF, zero-length → Problems entry only, no squiggle
        return Location.Create(tree, new TextSpan(text.Length, 0));
    }

    /// Extracts Area and optional Feature from the first namespace:
    /// company.project.area[.feature[...]] → returns (area, featureOrNull)
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

    /// Returns (projectName, projectRelativePath) where projectRelativePath starts at the project folder.
    /// Falls back to anchor-relative if project name cannot be found.
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
