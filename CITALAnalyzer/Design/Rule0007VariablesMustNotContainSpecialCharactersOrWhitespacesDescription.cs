using System.Collections.Immutable;
using CountITBCALCop.Helpers;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

// CIG0007 - Variable Naming
// "Names of variables must always include the AL object and must not contain special characters."

// Name of the AL Object is in this case the name without special characters or whitespaces

// issue: cannto remove prefix properly because it is not standarized

namespace CountITBCALCop.Design;

[DiagnosticAnalyzer]
public class Rule0007VariablesNameContainsALObjectAndNoSpecialChars : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(DiagnosticDescriptors.Rule0007VariablesNameContainsALObjectAndNoSpecialChars);

        public override void Initialize(AnalysisContext ctx) =>
            ctx.RegisterSymbolAction(new Action<SymbolAnalysisContext>(this.CheckVariableNames), 
                SymbolKind.GlobalVariable, 
                SymbolKind.LocalVariable);

        private void CheckVariableNames(SymbolAnalysisContext ctx)
        {
            if(ctx.IsObsoletePendingOrRemoved() || ctx.Symbol is not IVariableSymbol symbol)
                return;

            if (symbol.Type.IsErrorType() || symbol.Type.GetNavTypeKindSafe() == NavTypeKind.DotNet)
                return;

            if (HelperFunctions.ContainsWhiteSpace(symbol.Name) || HelperFunctions.ContainsSpecialCharacters(symbol.Name))
            ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0007VariablesNameContainsALObjectAndNoSpecialChars,
                    symbol.GetLocation()));

            var typeKind = symbol.Type.GetNavTypeKindSafe();
            if (!(typeKind == NavTypeKind.Record ||
                  typeKind == NavTypeKind.Page ||
                  typeKind == NavTypeKind.Report ||
                  typeKind == NavTypeKind.Codeunit ||
                  typeKind == NavTypeKind.Query ||
                  typeKind == NavTypeKind.XmlPort ||
                  typeKind == NavTypeKind.Enum))
            {
                return;
            }

            string objectName = symbol.Type.Name; // e.g. "Gen. Jnl.-Post Line"
            string abbreviation = HelperFunctions.RemoveAlpanumericCharacters(objectName); // e.g. "GenJnlPostLine"
            abbreviation = HelperFunctions.RemovePrefix(abbreviation);

            if (!symbol.Name.ToLower().Contains(abbreviation.ToLower()))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.Rule0007VariablesNameContainsALObjectAndNoSpecialChars,
                    symbol.GetLocation()));
            }
    }
        
}