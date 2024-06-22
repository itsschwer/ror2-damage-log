using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DamageLog
{
    public sealed class DamageLog
    {
        private readonly CharacterBody targetBody;
        public readonly string targetName;
        public readonly bool isBoss = false;
        public readonly uint targetNetId = 0;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        public string targetNetIdHex => targetNetId.ToString("x8");
        private string targetDisplayName => (targetNetId == 0) ? targetName : $"{targetName} {targetNetIdHex}";
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        private readonly Dictionary<string, DamageSource> entries = [];
        private float timeOfDeath = -1;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        public float time => (timeOfDeath > 0) ? timeOfDeath : Time.time;
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        public DamageLog(NetworkUser user, CharacterBody body)
        {
            if (user == null || body == null) return;

            targetBody = body;
            targetName = user.userName;

            Plugin.Data.AddUserLog(user, Track(body));
        }

        public DamageLog(CharacterBody body)
        {
            if (body == null) return;
            if (IsIgnoredBossSubtitle(body.subtitleNameToken)) return;

            targetBody = body;
            targetName = Util.GetBestBodyName(body.gameObject);
            isBoss = true;
            targetNetId = targetBody.netId.Value;

            uint key = body.netId.Value;
            Plugin.Data.AddBossLog(key, Track(body));
        }

        private DamageLog Track(CharacterBody body)
        {
            GlobalEventManager.onClientDamageNotified += Record;
            body.master.onBodyDestroyed += Cease;
            Log.Debug($"Tracking {targetDisplayName}.");
            return this;
        }

        internal void Cease(CharacterBody _ = null)
        {
            if (timeOfDeath <= 0) timeOfDeath = Time.time;
            GlobalEventManager.onClientDamageNotified -= Record;
            if (targetBody?.master != null) targetBody.master.onBodyDestroyed -= Cease;
            else Log.Warning($"Could not unsubscribe {nameof(RoR2)}.{nameof(CharacterMaster)}::{nameof(CharacterMaster.onBodyDestroyed)} for {targetDisplayName}.");

            var caller = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            Log.Debug($"Untracking {targetDisplayName}. | {caller.DeclaringType}::{caller.Name}");
        }

        private void Record(DamageDealtMessage e)
        {
            if (targetBody == null || e.victim != targetBody.gameObject) return;

            string key = DamageSource.GenerateIdentifier(e.attacker, DamageSource.IsFallDamage(e), DamageSource.IsVoidFogDamage(e));
            if (entries.TryGetValue(key, out DamageSource latest) && !IsExpired(time - latest.time)) {
                latest.Add(e);
            }
            else {
                entries.Remove(key);
                latest = new DamageSource(e);
                entries.Add(key, latest);
            }

            if (latest.remainingHpPercent <= 0f) timeOfDeath = Time.time;

            if (entries.Count > Plugin.Config.EntryMaxCount && !isBoss) Prune();
        }

        private void Prune()
        {
            Decay();
            Displace();
        }

        private void Decay()
        {
            float now = time;
            foreach (DamageSource src in GetEntries()) {
                if (IsExpired(now - src.time)) {
                    entries.Remove(src.identifier);
                }
            }
        }

        private void Displace()
        {
            List<DamageSource> orderedEntries = GetEntries();
            for (int i = Plugin.Config.EntryMaxCount; i < orderedEntries.Count; i++) {
                entries.Remove(orderedEntries[i].identifier);
            }
        }

        public bool IsExpired(float elapsedTime)
            => (!isBoss && elapsedTime > Plugin.Config.EntryMaxRetainTime);

        /// <returns>
        /// The <see cref="DamageSource"/>s contained in the <see cref="DamageLog"/>,
        /// ordered by decreasing recency.
        /// </returns>
        public List<DamageSource> GetEntries()
        {
            List<DamageSource> list = entries.Values.ToList();
            if (list.Count > 1)
                list.Sort((a, b) => System.Math.Sign(b.time - a.time)); // Newest first
            return list;
        }




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
