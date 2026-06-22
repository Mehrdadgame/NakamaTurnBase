namespace Nakama.Helpers
{
    /// <summary>
    /// Text chat payload relayed between real players (OperationCode.SendChat = 14).
    /// Serialized with Newtonsoft (PascalCase) — the server relay reads <c>Text</c> for
    /// its length clamp, so the field name must stay "Text".
    /// </summary>
    public class ChatData
    {
        public string ID;    // sender userId — the receiver ignores its own echo
        public string Text;  // message body (already profanity-masked client-side)
        public string Kind;  // "preset" | "free" (for optional styling/analytics)
    }
}
