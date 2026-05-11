using System.Text;

/// <summary>
/// Utilities for displaying numbers correctly in Persian RTL UI.
/// </summary>
public static class PersianTextUtils
{
    // Regular comma wrapped in RLM (U+200F) on both sides.
    // RLM tells the BiDi algorithm the comma lives in an RTL run → no digit reordering.
    // This works even if the font has no U+066C glyph.
    private const string ThousandsSep = "‏,‏";

    /// <summary>
    /// Formats an integer with a comma every 3 digits (RTL-safe).
    /// Groups are built least-significant-first so RTLTextMeshPro's reversal
    /// renders them in the correct visual order.
    /// Example: 28000 → "۲۸,۰۰۰"
    /// </summary>
    public static string FormatNumber(long amount)
    {
        if (amount < 0)
            return "‏-" + FormatNumber(-amount);

        string digits = amount.ToString();
        int len = digits.Length;
        if (len <= 3)
            return ToPersianDigits(digits);

        // Collect groups from least significant to most significant
        var groups = new System.Collections.Generic.List<string>();
        int firstGroup = len % 3;
        if (firstGroup > 0)
            groups.Add(digits.Substring(0, firstGroup));
        for (int i = firstGroup; i < len; i += 3)
            groups.Add(digits.Substring(i, 3));

        // Reverse so RTLTextMeshPro's character-reversal yields correct visual order
        groups.Reverse();

        var sb = new StringBuilder();
        for (int i = 0; i < groups.Count; i++)
        {
            if (i > 0) sb.Append(ThousandsSep);
            sb.Append(groups[i]);
        }
        return ToPersianDigits(sb.ToString());
    }

    /// <summary>
    /// Fixes a pre-formatted price string for RTLTextMeshPro.
    /// RTLTextMeshPro reverses group order, so "۱۱۹,۰۰۰ ت" displays as "۰۰۰,۱۱۹ ت".
    /// This method reverses the digit groups in-place so after RTL rendering they appear correctly.
    /// Example: "۱۱۹,۰۰۰ ت" → "۰۰۰,۱۱۹ ت" (stored) → "ت ۱۱۹,۰۰۰" (displayed) ✅
    /// </summary>
    public static string FixRTLPriceLabel(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Match a number that contains at least one comma (e.g. ۱۱۹,۰۰۰ or 119,000)
        return System.Text.RegularExpressions.Regex.Replace(
            input,
            @"[\d۰-۹٠-٩]+(,[\d۰-۹٠-٩]+)+",
            m =>
            {
                var parts = m.Value.Split(',');
                System.Array.Reverse(parts);
                return string.Join(ThousandsSep, parts);
            }
        );
    }

    /// <summary>Converts ASCII digits 0-9 to Persian digits ۰-۹.</summary>
    public static string ToPersianDigits(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sb = new StringBuilder(input.Length);
        foreach (char c in input)
            sb.Append(c >= '0' && c <= '9' ? (char)(c - '0' + '۰') : c);
        return sb.ToString();
    }
}
