using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0001 - SetLoadFields
// "SetLoadFields must always be used before fetching a record from the database."

// which methods should use a setLoadFields before they are used?

namespace CITALAnalyzer.Design
{
    [DiagnosticAnalyzer]
    public class Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecords : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DiagnosticDescriptors.Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecords);


        // set of methods that require a SetLoadField() before beeing used
        private static readonly HashSet<string> FetchMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "FindSet",
            "FindFirst",
            "FindLast",
            "Find", 
            "Get"
        };

        private static readonly HashSet<string> LoadFieldSetters = new(StringComparer.OrdinalIgnoreCase)
        {
            "SetLoadFields",
            "AddLoadFields"
        };

        private static readonly HashSet<string> LoadFieldClearers = new(StringComparer.OrdinalIgnoreCase)
        {
            "ClearLoadFields"
        };

        public override void Initialize(AnalysisContext context) =>
            context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeCodeBlock));

        private void AnalyzeCodeBlock(CodeBlockAnalysisContext ctx)
        {
            if (ctx.IsObsoletePendingOrRemoved() || ctx.CodeBlock is not MethodDeclarationSyntax methodSyntax)
                return;

            var semanticModel = ctx.SemanticModel;
            var cancellationToken = ctx.CancellationToken;

            var hasSetLoadFields = new Dictionary<IVariableSymbol, bool>();

            if (methodSyntax.Body == null)
                return;

            foreach (var statement in methodSyntax.Body.Statements)
            {
                // Check all method invocations
                foreach (var invocation in statement.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    if (semanticModel.GetOperation(invocation, cancellationToken) is not IInvocationExpression op)
                        continue;

                    if (op.TargetMethod.MethodKind != MethodKind.BuiltInMethod)
                        continue;

                    if (!TryResolveRecordInstance(op, invocation, out var recVar, out var recType, semanticModel, cancellationToken))
                        continue;

                    if (recType.Temporary)
                        continue;

                    if (!hasSetLoadFields.ContainsKey(recVar))
                        hasSetLoadFields[recVar] = false;

                    var methodName = op.TargetMethod.Name;

                    if (LoadFieldSetters.Contains(methodName))
                    {
                        hasSetLoadFields[recVar] = true;
                        continue;
                    }

                    if (LoadFieldClearers.Contains(methodName))
                    {
                        hasSetLoadFields[recVar] = false;
                        continue;
                    }

                    if (FetchMethods.Contains(methodName))
                    {
                        if (!hasSetLoadFields[recVar])
                        {
                            ctx.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptors.Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecords,
                                invocation.GetLocation(),
                                recVar.Name,
                                methodName));
                        }
                    }
                }

                // Reset if record is reassigned
                foreach (var assign in statement.DescendantNodes().OfType<AssignmentStatementSyntax>())
                {
                    if (assign.Target is IdentifierNameSyntax idName)
                    {
                        var symbol = semanticModel.GetSymbolInfo(idName, cancellationToken).Symbol;
                        if (symbol is IVariableSymbol v && hasSetLoadFields.ContainsKey(v))
                            hasSetLoadFields[v] = false;
                    }
                }
            }
        }

        private static bool TryResolveRecordInstance(
            IInvocationExpression op,
            InvocationExpressionSyntax syntax,
            out IVariableSymbol recVar,
            out IRecordTypeSymbol recType,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken)
        {
            recVar = null!;
            recType = null!;

            if (op.Instance?.GetSymbol() is IVariableSymbol { Type: IRecordTypeSymbol rt1 } v1)
            {
                recVar = v1;
                recType = rt1;
                return true;
            }

            var withStmt = syntax.FirstAncestorOrSelf<WithStatementSyntax>();
            if (withStmt != null)
            {
                var exprSymbol = semanticModel.GetSymbolInfo(withStmt.WithId, cancellationToken).Symbol;
                if (exprSymbol is IVariableSymbol { Type: IRecordTypeSymbol rt2 } v2)
                {
                    recVar = v2;
                    recType = rt2;
                    return true;
                }
            }

            return false;
        }
    }
}
