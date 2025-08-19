using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text; // already in your file

// Rule0000CheckProjectStructureAzureDevOps
// Project structure is not following official conventions (AzureDevOps).

namespace CountITBCALCop.Design;

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

        var path = filePath.Replace('\\', '/');

        var appMatch = Regex.Match(path, @"(?i)/(App|Test)/");
        if (!appMatch.Success)
            return;

        if (IsInfraPath(path))
            return;

        var expectedKindFolder = GetExpectedKindFolder(ctx);
        if (expectedKindFolder is null)
            return;

        var expectedSegment = "/" + appMatch.Groups[1].Value + "/";
        var ok = PathHasSegmentOrder(path, expectedSegment, expectedKindFolder);

        if (!ok)
        {
            var location = FileLevelLocation(ctx); // file-level only, no squiggles
            var objectLabel = KindToLabel(ctx.Node.Kind);
            var expected = $"<Project>/(App|Test)/<Area>/<SubArea>/{expectedKindFolder}/";

            ctx.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.Rule0000CheckProjectStructureAzureDevOps,
                location,
                objectLabel,
                expected,
                path));
        }
    }

    private static bool IsInfraPath(string path) =>
        Regex.IsMatch(path, @"(?i)/(App|Test)/(\.vscode|\.alpackages|\.altestrunner|\.snapshots|Translations)(/|$)");

    private static string? GetExpectedKindFolder(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.Node.Kind == SyntaxKind.CodeunitObject)
        {
            // EventSub if any [EventSubscriber] attribute is present
            var containsSubscriber = ctx.Node
                .DescendantNodes()
                .Any(n => n.Kind == SyntaxKind.MemberAttribute &&
                          n.ToString().IndexOf("EventSubscriber", StringComparison.OrdinalIgnoreCase) >= 0);

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

    private static bool PathHasSegmentOrder(string fullPath, string appOrTestSeg, string kindFolder)
    {
        var idxApp = IndexOf(fullPath, appOrTestSeg);
        if (idxApp < 0) return false;

        var after = fullPath.Substring(idxApp + appOrTestSeg.Length);
        var parts = after.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3) return false;

        for (int i = 0; i < parts.Length; i++)
            if (parts[i].Equals(kindFolder, StringComparison.OrdinalIgnoreCase))
                return i >= 2; // need <Area>/<SubArea>/ before the kind folder

        return false;
    }

    private static int IndexOf(string hay, string needle) =>
        CultureInfo.InvariantCulture.CompareInfo.IndexOf(hay, needle, CompareOptions.IgnoreCase);


    private static Location FileLevelLocation(SyntaxNodeAnalysisContext ctx)
    {
        var tree = ctx.Node.SyntaxTree!;
        // 0-length span at file start → shows in Problems, no squiggle in editor
        return Location.Create(tree, new TextSpan(0, 0));
    }

}