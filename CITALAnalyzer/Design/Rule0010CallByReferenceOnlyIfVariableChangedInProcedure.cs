using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

// Editing CIG0010 ‐ call by reference, only when modifying variable
// "Call by reference may only be used if the variable is modified within the procedure."

// can be disabled by using a comment with the name of the variable and the word filter

namespace CountITBCALCop.Design;

[DiagnosticAnalyzer]
public class Rule0010CallByReferenceOnlyIfVariableChangedInProcedure : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(DiagnosticDescriptors.Rule0010CallByReferenceOnlyIfVariableChangedInProcedure);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeMethodParametersPassedByReference));
    }

    private void AnalyzeMethodParametersPassedByReference(CodeBlockAnalysisContext ctx)
    {
        if (ctx.CodeBlock is not MethodDeclarationSyntax methodSyntax)
            return;

        if (methodSyntax.ParameterList == null || methodSyntax.ParameterList.Parameters.Count == 0)
            return;

        var semanticModel = ctx.SemanticModel;
        var cancellationToken = ctx.CancellationToken;

        // Collect var parameters
        var varParameters = new Dictionary<ISymbol, ParameterSyntax>();
        foreach (var parameter in methodSyntax.ParameterList.Parameters)
        {
            if (parameter.VarKeyword == default)
                continue;

            var symbol = semanticModel.GetDeclaredSymbol(parameter, cancellationToken);
            if (symbol is not null)
                varParameters[symbol] = parameter;
        }

        if (varParameters.Count == 0)
            return;

        var modifiedSymbols = new HashSet<ISymbol>();

        foreach (var node in methodSyntax.DescendantNodes())
        {
            // Check assignments like Customer."Name 2" := ...
            if (node is AssignmentStatementSyntax assignment)
            {
                var targetExpr = assignment.Target;

                if (targetExpr is MemberAccessExpressionSyntax memberAccess &&
                    memberAccess.Expression is IdentifierNameSyntax rootId2)
                {
                    var rootSymbol = semanticModel.GetSymbolInfo(rootId2, cancellationToken).Symbol;
                    if (rootSymbol != null && varParameters.ContainsKey(rootSymbol))
                        modifiedSymbols.Add(rootSymbol);
                }
                else if (targetExpr is IdentifierNameSyntax targetId)
                {
                    var symbol = semanticModel.GetSymbolInfo(targetId, cancellationToken).Symbol;
                    if (symbol != null && varParameters.ContainsKey(symbol))
                        modifiedSymbols.Add(symbol);
                }
            }

            // Check method calls
            if (node is InvocationExpressionSyntax invocation &&
                invocation.Expression is MemberAccessExpressionSyntax memberAccessExpr &&
                memberAccessExpr.Expression is IdentifierNameSyntax rootId)
            {
                var rootSymbol = semanticModel.GetSymbolInfo(rootId, cancellationToken).Symbol;
                if (rootSymbol != null && varParameters.ContainsKey(rootSymbol))
                    modifiedSymbols.Add(rootSymbol);
            }
        }

        // for disabling rule via comment (eg. "filter has been set on param 'name'")
        var leadingComments = methodSyntax.GetLeadingTrivia()
            .Where(trivia =>
                trivia.IsKind(SyntaxKind.LineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                trivia.IsKind(SyntaxKind.CommentTrivia) ||
                trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia))
            .Select(trivia => trivia.ToString().ToLowerInvariant())
            .ToList();

        foreach (var kvp in varParameters)
        {
            var parameterSyntax = kvp.Value;
            if (parameterSyntax == null)
                continue;

            var parameterName = parameterSyntax.Name?.Identifier.ValueText;
            if (string.IsNullOrEmpty(parameterName))
                continue;

            parameterName = parameterName.ToLowerInvariant();

            // checks for keywords that deactive rule
            bool hasFilterComment = leadingComments.Any(comment =>
                (comment.Contains("filter") || comment.Contains("filtered")) &&
                comment.Contains(parameterName));

            if (!modifiedSymbols.Contains(kvp.Key) && !hasFilterComment)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0010CallByReferenceOnlyIfVariableChangedInProcedure,
                    parameterSyntax.GetLocation()));
            }
        }
    }
}
