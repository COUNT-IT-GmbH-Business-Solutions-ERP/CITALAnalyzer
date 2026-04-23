using System.Collections.Immutable;
using CountITBCALCop.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace CountITBCALCop.Design;

[DiagnosticAnalyzer]
public class Rule0001VariableNamesOnlyContainPrefixIfNecessary : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.Rule0001VariableNamesOnlyContainPrefixIfNecessary);

    public override void Initialize(AnalysisContext context)
    {
        context.RegisterCompilationStartAction(startContext =>
        {
            var variablesByContainer = new Dictionary<ISymbol, List<IVariableSymbol>>();

            startContext.RegisterSymbolAction(symbolContext =>
            {
                if (symbolContext.Symbol is not IVariableSymbol variable)
                    return;

                if (symbolContext.IsObsoletePendingOrRemoved())
                    return;

                if (variable.Type.IsErrorType() ||
                    variable.Type.GetNavTypeKindSafe() == NavTypeKind.DotNet)
                    return;

                var container = variable.ContainingSymbol;
                if (container == null)
                    return;

                if (!variablesByContainer.TryGetValue(container, out var list))
                {
                    list = new List<IVariableSymbol>();
                    variablesByContainer[container] = list;
                }

                list.Add(variable);

            }, SymbolKind.GlobalVariable, SymbolKind.LocalVariable);

            startContext.RegisterCompilationEndAction(endContext =>
            {
                foreach (var group in variablesByContainer.Values)
                {
                    AnalyzeGroup(group, endContext);
                }
            });
        });
    }

    private static void AnalyzeGroup(List<IVariableSymbol> variables, CompilationAnalysisContext ctx)
    {
        var map = new Dictionary<string, List<IVariableSymbol>>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var v in variables)
        {
            var unprefixed = HelperFunctions.RemovePrefix(v.Name);

            if (!map.TryGetValue(unprefixed, out var list))
            {
                list = new List<IVariableSymbol>();
                map[unprefixed] = list;
            }

            list.Add(v);
        }

        foreach (var v in variables)
        {
            if (!HelperFunctions.HasPrefix(v.Name))
                continue;

            var unprefixed = HelperFunctions.RemovePrefix(v.Name);

            if (map.TryGetValue(unprefixed, out var list) && list.Count == 1)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0001VariableNamesOnlyContainPrefixIfNecessary,
                    v.GetLocation()));
            }
        }
    }
}