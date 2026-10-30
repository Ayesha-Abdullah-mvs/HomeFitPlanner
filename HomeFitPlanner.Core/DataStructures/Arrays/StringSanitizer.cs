namespace HomeFitPlanner.Core.DataStructures.Arrays;

/// <summary>
/// Hand-written string processing and sanitization utility without regex or heavy external helpers.
/// Fulfills the 'cleaning of text input' topic from Table 8 of the project proposal.
/// </summary>
public static class StringSanitizer
{
    public static string ManualTrim(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        int start = 0;
        while (start < input.Length && char.IsWhiteSpace(input[start]))
        {
            start++;
        }

        int end = input.Length - 1;
        while (end >= start && char.IsWhiteSpace(input[end]))
        {
            end--;
        }

        if (start > end) return string.Empty;

        char[] chars = new char[end - start + 1];
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = input[start + i];
        }
        return new string(chars);
    }

    public static string ManualToLower(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        char[] chars = new char[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c >= 'A' && c <= 'Z')
            {
                chars[i] = (char)(c + 32);
            }
            else
            {
                chars[i] = c;
            }
        }
        return new string(chars);
    }

    public static bool ManualContains(string? source, string? target, bool ignoreCase = true)
    {
        if (source == null || target == null) return false;
        if (target.Length == 0) return true;
        if (target.Length > source.Length) return false;

        string s = ignoreCase ? ManualToLower(source) : source;
        string t = ignoreCase ? ManualToLower(target) : target;

        for (int i = 0; i <= s.Length - t.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < t.Length; j++)
            {
                if (s[i + j] != t[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return true;
        }
        return false;
    }

    public static string[] ManualSplit(string? input, char delimiter)
    {
        if (string.IsNullOrEmpty(input)) return Array.Empty<string>();

        int count = 1;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == delimiter) count++;
        }

        string[] tokens = new string[count];
        int tokenIdx = 0;
        int start = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == delimiter)
            {
                tokens[tokenIdx++] = Substring(input, start, i - start);
                start = i + 1;
            }
        }
        tokens[tokenIdx] = Substring(input, start, input.Length - start);
        return tokens;
    }

    private static string Substring(string s, int start, int length)
    {
        if (length <= 0) return string.Empty;
        char[] buffer = new char[length];
        for (int i = 0; i < length; i++)
        {
            buffer[i] = s[start + i];
        }
        return new string(buffer);
    }

    public static string CleanAlphaNumericAndSpaces(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        char[] temp = new char[input.Length];
        int writeIdx = 0;
        bool lastWasSpace = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '(' || c == ')' || c == ',' || c == '.')
            {
                temp[writeIdx++] = c;
                lastWasSpace = false;
            }
            else if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace && writeIdx > 0)
                {
                    temp[writeIdx++] = ' ';
                    lastWasSpace = true;
                }
            }
        }

        // Trim trailing space
        if (writeIdx > 0 && temp[writeIdx - 1] == ' ')
        {
            writeIdx--;
        }

        char[] result = new char[writeIdx];
        for (int i = 0; i < writeIdx; i++)
        {
            result[i] = temp[i];
        }
        return new string(result);
    }
}

