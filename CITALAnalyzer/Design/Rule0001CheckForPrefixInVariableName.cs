using CITALAnalyzer.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

// CIG0001 - Variable Name Prefix
// "Variable names must not contain a prefix unless to avoid naming conflicts."

// Prefix = 3 Uppercase letters at the beginning of the variable name

// TODO: needs to be tested with multiple apps (more than one possible prefix)

namespace CITALAnalyzer.Design;

[DiagnosticAnalyzer]
public class Rule0001CheckForPrefixInVariableName : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(DiagnosticDescriptors.Rule0001CheckForPrefixInVariableName);

   public override void Initialize(AnalysisContext ctx) =>
       ctx.RegisterSymbolAction(new Action<SymbolAnalysisContext>(this.CheckVariablePrefix), 
           SymbolKind.GlobalVariable, 
           SymbolKind.LocalVariable);

   private void CheckVariablePrefix(SymbolAnalysisContext ctx)
   {
       if(ctx.IsObsoletePendingOrRemoved() || ctx.Symbol is not IVariableSymbol symbol)
           return;

       if (symbol.Type.IsErrorType() || symbol.Type.GetNavTypeKindSafe() == NavTypeKind.DotNet)
           return;

       if (!HelperFunctions.HasPrefix(symbol.Name))
           return;

       string baseName = HelperFunctions.RemovePrefix(symbol.Name);
       ISymbol? container = symbol.ContainingSymbol;
       IEnumerable<IVariableSymbol> allVariables;

       if (container is IMethodSymbol method)
       {
           allVariables = method.LocalVariables
               .Concat(method.Parameters.OfType<IVariableSymbol>());
       }
       else if (container is IObjectTypeSymbol obj)
       {
           allVariables = obj.GetMembers().OfType<IVariableSymbol>();
       }
       else
       {
           return;
       }

       if (allVariables.Count() <= 1)
       {
           ctx.ReportDiagnostic(Diagnostic.Create(
               DiagnosticDescriptors.Rule0001CheckForPrefixInVariableName,
               symbol.GetLocation()));
           return;
       }

       var sameBaseNameVariables = allVariables
           .Where(v => HelperFunctions.RemovePrefix(v.Name).Equals(baseName, StringComparison.OrdinalIgnoreCase))
           .ToList();

       if (sameBaseNameVariables.Count == 1)
       {
           ctx.ReportDiagnostic(Diagnostic.Create(
               DiagnosticDescriptors.Rule0001CheckForPrefixInVariableName,
               symbol.GetLocation()));
       }
   }
}