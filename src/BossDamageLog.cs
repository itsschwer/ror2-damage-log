using RoR2;

namespace DamageLog
{
    internal sealed class BossDamageLog(CharacterBody body) : DamageLog(body, Util.GetBestBodyName(body.gameObject))
    {
        public readonly uint targetNetId = body.netId.Value;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        private string targetNetIdHex => targetNetId.ToString("x8");
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        public override string displayName => $"<style=cStack>{targetNetIdHex}</style> <style=cIsHealth>{base.displayName}</style>";
        public override string loggingName => $"{base.loggingName} <{targetNetIdHex}>";

        protected override void Prune() {}
        public override bool IsExpired(float elapsedTime) => false;

        public static bool IsIgnoredBossSubtitle(string subtitleNameToken)
        {
            if (string.IsNullOrEmpty(subtitleNameToken)) return true;

            switch (subtitleNameToken) {
                default: return false;
                case "NULL_SUBTITLE":               // "Horde of Many"
                case "LUNARWISP_BODY_SUBTITLE":     // "Zenith Designs"
                case "LUNARGOLEM_BODY_SUBTITLE":    // "Zenith Designs"
                case "LUNAREXPLODER_BODY_SUBTITLE": // "Zenith Designs"
                    return true;
            }
        }
    }
}
