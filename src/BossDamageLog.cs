using RoR2;

namespace DamageLog
{
    internal sealed class BossDamageLog(CharacterBody body) : DamageLog(body, Util.GetBestBodyName(body.gameObject))
    {
        public readonly uint targetNetId = body.netId.Value;

        public override string displayName => $"<style=cStack>{targetNetId:x8}</style> <style=cIsHealth>{base.displayName}</style>";
        public override string loggingName => $"{base.loggingName} <{targetNetId:x8}>";

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
