using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIG0009 - Kein "addbefore", "addafter", "movebefore" & "moveafter"
// "It is not allowed to use “addbefore”, “addafter”, “movebefore” or “moveafter” in PageExtensions"

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0009CheckForAddMoveBeforeAfterinPageExt : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(
            DiagnosticDescriptors.Rule0009CheckForAddMoveBeforeAfterinPageExt);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterSyntaxNodeAction(new Action<SyntaxNodeAnalysisContext>(CheckBadLayoutInsert),
            SyntaxKind.PageExtensionObject);
    }

    private void CheckBadLayoutInsert(SyntaxNodeAnalysisContext ctx)
    {
       if (ctx.IsObsoletePendingOrRemoved())
           return;

        var pageExtNode = ctx.Node;

        foreach(var node in pageExtNode.DescendantNodes())
        {
            SyntaxToken keyword;

            switch (node)
            {
                case ControlAddChangeSyntax controlAdd:
                    keyword = controlAdd.ChangeKeyword;
                    break;
                case ControlMoveChangeSyntax controlMove:
                    keyword = controlMove.ChangeKeyword;
                    break;
                case ActionAddChangeSyntax actionAdd:
                    keyword = actionAdd.ChangeKeyword;
                    break;
                case ActionMoveChangeSyntax actionMove:
                    keyword = actionMove.ChangeKeyword;
                    break;
                default:
                    continue;
            }

            if (keyword.IsKind(SyntaxKind.AddBeforeKeyword) ||
                keyword.IsKind(SyntaxKind.AddAfterKeyword) ||
                keyword.IsKind(SyntaxKind.MoveBeforeKeyword) ||
                keyword.IsKind(SyntaxKind.MoveAfterKeyword))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0009CheckForAddMoveBeforeAfterinPageExt,
                    keyword.GetLocation()));
            }
        }
    }
}