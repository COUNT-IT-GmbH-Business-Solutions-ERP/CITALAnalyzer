using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CITALAnalyzer.Helpers;


public class HelperFunctions
{

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

    public static string RemovePrefix(string name) =>
        HasPrefix(name) ? name.Substring(3) : name;

}
