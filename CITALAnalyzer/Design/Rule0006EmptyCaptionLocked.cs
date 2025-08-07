using System.Collections.Immutable;
using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

// CIG0006 - Locked Captions
// "For empty captions, the Locked property must be set to true."

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0006EmptyCaptionLocked : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.Rule0006EmptyCaptionLocked);

    // List based on https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/properties/devenv-caption-property
    public override void Initialize(AnalysisContext context) =>
        context.RegisterSyntaxNodeAction(new Action<SyntaxNodeAnalysisContext>(AnalyzeCaptionProperty), new SyntaxKind[] {
            SyntaxKind.TableObject,
            SyntaxKind.Field, // TableField
            SyntaxKind.PageField,
            SyntaxKind.PageGroup,
            SyntaxKind.PageObject,
            SyntaxKind.RequestPage,
            SyntaxKind.PageLabel,
            SyntaxKind.PageGroup,
            SyntaxKind.PagePart,
            SyntaxKind.PageSystemPart,
            SyntaxKind.PageAction,
            SyntaxKind.PageActionSeparator,
            SyntaxKind.PageActionGroup,
            SyntaxKind.XmlPortObject,
            SyntaxKind.ReportObject,
            SyntaxKind.QueryObject,
            SyntaxKind.QueryColumn,
            SyntaxKind.QueryFilter,
            SyntaxKind.ReportColumn,
            SyntaxKind.EnumValue,
            SyntaxKind.PageCustomAction,
#if !LessThenSpring2024
            SyntaxKind.PageSystemAction,
#endif
            SyntaxKind.PageView,
            SyntaxKind.ReportLayout,
            SyntaxKind.ProfileObject,
            SyntaxKind.EnumType,
            SyntaxKind.PermissionSet,
            SyntaxKind.TableExtensionObject,
            SyntaxKind.PageExtensionObject
    });

    private void AnalyzeCaptionProperty(SyntaxNodeAnalysisContext ctx)
    {
        if (ctx.IsObsoletePendingOrRemoved())
            return;

        if (ctx.Node.IsKind(SyntaxKind.EnumValue) && ctx.ContainingSymbol.Kind == SymbolKind.Enum)
            return; // Prevent double raising the rule on EnumValue in a EnumObject

        if (ctx.Node.GetPropertyValue("Caption") is not LabelPropertyValueSyntax captionProperty)
            return;

        if (!string.IsNullOrEmpty(captionProperty.Value.LabelText.GetLiteralValue().ToString()?.Trim()))
            return;

        if (captionProperty.Value.Properties?.Values.Any(prop => prop.Identifier.Text.Equals("Locked", StringComparison.OrdinalIgnoreCase)) == true)
            return;

        ctx.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.Rule0006EmptyCaptionLocked,
            captionProperty.GetLocation()));
    }
}