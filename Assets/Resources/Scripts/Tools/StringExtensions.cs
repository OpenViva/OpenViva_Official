using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static string ToHumanReadable(this string variableName)
    {
        if (string.IsNullOrEmpty(variableName))
            return variableName;

        // Handle special cases where the name might already be in human-readable form
        if (variableName.Contains(" "))
            return variableName;

        // Insert spaces before each capital letter, except the first one
        string result = Regex.Replace(variableName, "(\\B[A-Z])", " $1");

        // Capitalize the first letter of the result
        return char.ToUpper(result[0]) + result.Substring(1);
    }
}