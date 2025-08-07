using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CITALAnalyzer.Helpers;


public class HelperFunctions
{
    public static bool MethodImplementsInterfaceMethod(IMethodSymbol methodSymbol)
    {
        return MethodImplementsInterfaceMethod(methodSymbol.GetContainingApplicationObjectTypeSymbol(), methodSymbol);
    }

    public static bool MethodImplementsInterfaceMethod(IApplicationObjectTypeSymbol? objectSymbol, IMethodSymbol methodSymbol)
    {
        if (objectSymbol is not ICodeunitTypeSymbol codeunitSymbol)
        {
            return false;
        }

        foreach (var implementedInterface in codeunitSymbol.ImplementedInterfaces)
        {
            if (implementedInterface.GetMembers().OfType<IMethodSymbol>().Any(interfaceMethodSymbol => MethodImplementsInterfaceMethod(methodSymbol, interfaceMethodSymbol)))
            {
                return true;
            }
        }

        return false;
    }

    public static bool MethodImplementsInterfaceMethod(IMethodSymbol methodSymbol, IMethodSymbol interfaceMethodSymbol)
    {
        if (methodSymbol.Name != interfaceMethodSymbol.Name)
        {
            return false;
        }
        if (methodSymbol.Parameters.Length != interfaceMethodSymbol.Parameters.Length)
        {
            return false;
        }
        var methodReturnValType = methodSymbol.ReturnValueSymbol?.ReturnType.NavTypeKind ?? NavTypeKind.None;
        var interfaceMethodReturnValType = interfaceMethodSymbol.ReturnValueSymbol?.ReturnType.NavTypeKind ?? NavTypeKind.None;
        if (methodReturnValType != interfaceMethodReturnValType)
        {
            return false;
        }
        for (int i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            var methodParameter = methodSymbol.Parameters[i];
            var interfaceMethodParameter = interfaceMethodSymbol.Parameters[i];

            if (methodParameter.IsVar != interfaceMethodParameter.IsVar)
            {
                return false;
            }
            if (!methodParameter.ParameterType.Equals(interfaceMethodParameter.ParameterType))
            {
                return false;
            }
        }
        return true;
    }

    public static bool ContainsWhiteSpace(string str) =>
    Regex.IsMatch(str, "\\s+", RegexOptions.Compiled);

    public static bool ContainsSpecialCharacters(string str) =>
        Regex.IsMatch(str, "[^a-zA-Z0-9_]", RegexOptions.Compiled); // allows letters, digits, underscore

    public static string RemoveAlpanumericCharacters(string name)
    {
        return Regex.Replace(name, "[^a-zA-Z0-9]", "");
    }

    public static bool HasPrefix(string name) =>
        Regex.IsMatch(name, @"^[A-Z]{3}");

    public static string GetPrefix(string name) =>
        HasPrefix(name) ? name.Substring(0, 3) : "";

    public static string RemovePrefix(string name) =>
        HasPrefix(name) ? name.Substring(3) : name;

}
