public enum GameLanguage
{
    English = 0,
    Persian = 1,
}

/// <summary>
/// The game is Persian-only. This shim exists because gameplay code calls
/// Localization.L(fa, en) at many call sites; it always returns the Persian
/// string, so behavior is identical to the original hard-coded Persian texts.
/// </summary>
public static class Localization
{
    public static GameLanguage Current => GameLanguage.Persian;

    public static bool IsPersian => true;

    /// <summary>Always picks the Persian string.</summary>
    public static string L(string fa, string en) => fa;
}
