using System.Text;

namespace Nakama.Helpers
{
    /// <summary>
    /// Lightweight client-side profanity filter for free-text chat.
    /// Persian app stores require player-to-player text to be filtered.
    /// This is a best-effort mask; the server also clamps length. Extend <see cref="Blocked"/>
    /// as needed — matching is case-insensitive and normalizes Arabic kaf/yeh to Persian.
    /// </summary>
    public static class ChatProfanityFilter
    {
        // Keep lowercase. Add/remove freely. (Intentionally modest list.)
        private static readonly string[] Blocked =
        {
            // English
            "fuck", "shit", "bitch", "asshole", "bastard", "dick", "pussy",
            "slut", "whore", "cunt", "nigger", "motherfucker",
            // Persian (common insults)
            "کیر", "کس", "کص", "کون", "کونی", "جنده", "جند", "کسکش", "کسخل",
            "خارکسه", "مادرجنده", "پدرسگ", "بیناموس", "بناموس", "حرومزاده",
            "حروم زاده", "لاشی", "عوضی", "جاکش", "گاییدم", "گایید", "بیشرف", "بیشعور",
        };

        /// <summary>Normalizes Arabic variants (ك/ي) to Persian (ک/ی) and lowercases.</summary>
        private static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace('ي', 'ی')  // Arabic yeh → Persian yeh
                    .Replace('ك', 'ک')  // Arabic kaf → Persian kaf
                    .ToLowerInvariant();
        }

        public static bool ContainsProfanity(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string norm = Normalize(text);
            foreach (var w in Blocked)
                if (!string.IsNullOrEmpty(w) && norm.Contains(w))
                    return true;
            return false;
        }

        /// <summary>Replaces any blocked word with asterisks of the same length.</summary>
        public static string Mask(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string result = text;
            string norm = Normalize(result);

            foreach (var w in Blocked)
            {
                if (string.IsNullOrEmpty(w)) continue;
                int idx;
                int from = 0;
                while ((idx = norm.IndexOf(w, from, System.StringComparison.Ordinal)) >= 0)
                {
                    var sb = new StringBuilder(result);
                    for (int i = 0; i < w.Length && idx + i < sb.Length; i++)
                        sb[idx + i] = '*';
                    result = sb.ToString();
                    norm = Normalize(result);
                    from = idx + w.Length;
                }
            }
            return result;
        }
    }
}
