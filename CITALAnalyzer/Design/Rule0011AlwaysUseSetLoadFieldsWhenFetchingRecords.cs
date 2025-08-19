using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using System.Collections.Immutable;

// CIGP0001 - SetLoadFields
// "SetLoadFields must always be used before fetching a record from the database."

// which methods should use a setLoadFields before they are used?

// Transfer fields - disable rule, 
// if used more than 10 fields - disable rules

namespace CITALAnalyzer.Design
{
    [DiagnosticAnalyzer]
    public class Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecords : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(DiagnosticDescriptors.Rule0011AlwaysUseSetLoadFieldsWhenFetchingRecords);

        // Methods that require SetLoadFields() before use
        private static readonly HashSet<string> FetchMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "FindSet", "FindFirst", "FindLast", "Find", "Get"
        };

        private static readonly HashSet<string> LoadFieldSetters = new(StringComparer.OrdinalIgnoreCase)
        {
            "SetLoadFields", "AddLoadFields"
        };

        private static readonly HashSet<string> LoadFieldClearers = new(StringComparer.OrdinalIgnoreCase)
        {
            "ClearLoadFields"
        };

        private const string TransferFieldsMethod = "TransferFields";
        private const int FieldUsageThreshold = 10; // records using >10 fields => skip rule

        public override void Initialize(AnalysisContext context) =>
            context.RegisterCodeBlockAction(new Action<CodeBlockAnalysisContext>(AnalyzeCodeBlock));

        private void AnalyzeCodeBlock(CodeBlockAnalysisContext ctx)
        {
            if (ctx.IsObsoletePendingOrRemoved() || ctx.CodeBlock is not MethodDeclarationSyntax methodSyntax)
                return;

            var semanticModel = ctx.SemanticModel;
            var cancellationToken = ctx.CancellationToken;

            if (methodSyntax.Body == null)
                return;

            // ---------------------------------------------------------------------
            // PASS 1A: collect all records that participate in TransferFields
            // ---------------------------------------------------------------------
            var recordsUsingTransferFields = new HashSet<IVariableSymbol>();

            foreach (var inv in methodSyntax.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (semanticModel.GetOperation(inv, cancellationToken) is not IInvocationExpression op)
                    continue;

                if (op.TargetMethod.MethodKind != MethodKind.BuiltInMethod)
                    continue;

                if (!string.Equals(op.TargetMethod.Name, TransferFieldsMethod, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Receiver: e.g., NewCust.TransferFields(...)
                if (TryResolveRecordInstance(op, inv, out var recvVar, out var recvType, semanticModel, cancellationToken)
                    && !recvType.Temporary)
                {
                    recordsUsingTransferFields.Add(recvVar);
                }

                // Any record arguments: e.g., TransferFields(SrcCust)
                foreach (var arg in op.Arguments)
                {
                    var valueOp = arg.Value;
                    if (valueOp == null)
                        continue;

                    if (TryResolveRecordFromOperation(valueOp, semanticModel, cancellationToken, out var argVar, out var argType)
                        && !argType.Temporary)
                    {
                        recordsUsingTransferFields.Add(argVar);
                    }
                }
            }

            // ---------------------------------------------------------------------
            // PASS 1B: count distinct field usages per record
            //   - Qualified access: Rec.Field
            //   - WITH blocks:     Field (inside WITH Rec DO ...)
            // ---------------------------------------------------------------------
            var fieldsUsedByRecord = new Dictionary<IVariableSymbol, HashSet<int>>();

            void AddFieldUsage(IVariableSymbol recVar, int fieldId)
            {
                if (!fieldsUsedByRecord.TryGetValue(recVar, out var set))
                {
                    set = new HashSet<int>();
                    fieldsUsedByRecord[recVar] = set;
                }
                set.Add(fieldId);
            }

            // Case 1: Rec.Field
            foreach (var ma in methodSyntax.Body.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
            {
                var instanceSym = semanticModel.GetSymbolInfo(ma.Expression, cancellationToken).Symbol;
                if (instanceSym is not IVariableSymbol { Type: IRecordTypeSymbol recType } recVar)
                    continue;
                if (recType.Temporary)
                    continue;

                var memberSym = semanticModel.GetSymbolInfo(ma.Name, cancellationToken).Symbol;
                if (memberSym is IFieldSymbol fieldSym)
                {
                    AddFieldUsage(recVar, fieldSym.Id);
                }
            }

            // Case 2: WITH Rec DO ... Field ...
            foreach (var id in methodSyntax.Body.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                var sym = semanticModel.GetSymbolInfo(id, cancellationToken).Symbol;
                if (sym is not IFieldSymbol fieldSym)
                    continue;

                var withStmt = id.FirstAncestorOrSelf<WithStatementSyntax>();
                if (withStmt == null)
                    continue;

                var withTargetSym = semanticModel.GetSymbolInfo(withStmt.WithId, cancellationToken).Symbol;
                if (withTargetSym is IVariableSymbol { Type: IRecordTypeSymbol recType } recVar && !recType.Temporary)
                {
                    AddFieldUsage(recVar, fieldSym.Id);
                }
            }

            // records using > threshold fields => skip rule
            var recordsUsingManyFields = new HashSet<IVariableSymbol>();
            foreach (var kvp in fieldsUsedByRecord)
            {
                if (kvp.Value.Count > FieldUsageThreshold)
                    recordsUsingManyFields.Add(kvp.Key);
            }

            // Combine skip reasons
            var recordsToSkip = new HashSet<IVariableSymbol>(recordsUsingTransferFields);
            foreach (var r in recordsUsingManyFields)
                recordsToSkip.Add(r);

            // ---------------------------------------------------------------------
            // PASS 2: apply rule unless the record is in 'recordsToSkip'
            // ---------------------------------------------------------------------
            var hasSetLoadFields = new Dictionary<IVariableSymbol, bool>();

            foreach (var statement in methodSyntax.Body.Statements)
            {
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
                        // Skip if: used in TransferFields OR uses >10 fields
                        if (recordsToSkip.Contains(recVar))
                            continue;

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

                // Reset SetLoadFields state on assignment (do not touch skip sets)
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

        private static bool TryResolveRecordFromOperation(
            IOperation? valueOp,
            SemanticModel semanticModel,
            System.Threading.CancellationToken cancellationToken,
            out IVariableSymbol recVar,
            out IRecordTypeSymbol recType)
        {
            recVar = null!;
            recType = null!;

            var sym = valueOp?.GetSymbol();
            if (sym == null && valueOp != null)
                sym = semanticModel.GetSymbolInfo(valueOp.Syntax, cancellationToken).Symbol;

            if (sym is IVariableSymbol { Type: IRecordTypeSymbol rt } v)
            {
                recVar = v;
                recType = rt;
                return true;
            }

            return false;
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

            // Direct instance: MyRec.Get(); MyRec.TransferFields(...);
            if (op.Instance?.GetSymbol() is IVariableSymbol { Type: IRecordTypeSymbol rt1 } v1)
            {
                recVar = v1;
                recType = rt1;
                return true;
            }

            // WITH MyRec DO ... Get(); TransferFields(...);
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