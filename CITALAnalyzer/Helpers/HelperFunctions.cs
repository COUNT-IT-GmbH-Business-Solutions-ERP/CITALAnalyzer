using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CountITBCALCop.Helpers;


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


    // removes all leading uppercase letters except the last one -> First Letter of Object name
    public static string RemovePrefix(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // match ≥3 uppercase letters at start
        var match = Regex.Match(name, @"^([A-Z]{3,})(.*)$");
        if (match.Success)
        {
            string prefix = match.Groups[1].Value;
            string rest = match.Groups[2].Value;

            return prefix[^1] + rest;
        }

        return name;
    }

}
