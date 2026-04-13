using System.Text.RegularExpressions;

namespace CountITBCALCop.Helpers;


public partial class HelperFunctions
{

    public static bool ContainsWhiteSpace(string str) =>
    WhiteSpaceRegex().IsMatch(str);

    public static bool ContainsSpecialCharacters(string str) =>
        NonAlphanumericOrUnderscoreRegex().IsMatch(str); // allows letters, digits, underscore
    
    public static string RemoveAlpanumericCharacters(string name)
    {
        return NonAlphaNumericalRegex().Replace(name, "");
    }

    // removes all leading uppercase letters except the last one -> First Letter of Object name
    public static string RemovePrefix(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        var match = AfterPrefixCaptureRegex().Match(name);
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return name;
    }

    [GeneratedRegex("[^a-zA-Z0-9]", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericalRegex();

    [GeneratedRegex("[^a-zA-Z0-9_]", RegexOptions.Compiled)]
    private static partial Regex NonAlphanumericOrUnderscoreRegex();

    [GeneratedRegex("\\s+", RegexOptions.Compiled)]
    private static partial Regex WhiteSpaceRegex();

    // ^[A-Z0-9]{3,} = from the start match ≥3 uppercase letters or Numbers at start
    // ([A-Z].+) = match 1 uppercase Letter and everything else
    [GeneratedRegex(@"^[A-Z0-9]{3,}([A-Z].+)", RegexOptions.Compiled)]
    private static partial Regex AfterPrefixCaptureRegex();

    [GeneratedRegex(@"^([A-Z0-9]{3,})[A-Z].+", RegexOptions.Compiled)]
    private static partial Regex PrefixCaptureRegex();
}
