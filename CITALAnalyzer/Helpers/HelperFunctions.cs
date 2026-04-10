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

        //^[A-Z0-9]{3,} = from the start match ≥3 uppercase letters or Numbers at start
        // ([A-Z].+) = match 1 uppercase Letter and everything else
        var match = Regex.Match(name, @"^[A-Z0-9]{3,}([A-Z].+)", RegexOptions.Compiled);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return name;
    }

}
