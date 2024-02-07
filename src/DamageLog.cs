using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DamageLog
{
    public sealed class DamageLog
    {
        public readonly string targetDisplayName;
        public readonly int targetDiscriminator;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        private string targetName => (targetDiscriminator == 0) ? targetDisplayName : $"{targetDisplayName} {targetDiscriminator}";
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character
        public readonly bool isBoss = false;
        private readonly Dictionary<string, DamageSource> entries = [];
        private readonly CharacterBody targetBody;
        private float timeOfDeath = -1;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        public float time => (timeOfDeath > 0) ? timeOfDeath : Time.time;
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        public DamageLog(NetworkUser user, CharacterBody body)
        {
            if (user == null || body == null) return;

            targetBody = body;
            targetDisplayName = user.userName;

            Plugin.Data.AddUserLog(user, Track(body));
        }

        public DamageLog(CharacterBody body)
        {
            if (body == null) return;
            if (IsIgnoredBossSubtitle(body.subtitleNameToken)) return;

            targetBody = body;
            targetDisplayName = Util.GetBestBodyName(body.gameObject);
            targetDiscriminator = Plugin.Data.EncounterBody(body.baseNameToken);
            isBoss = true;

            int key = body.GetInstanceID();
            Plugin.Data.AddBossLog(key, Track(body));
        }

        private DamageLog Track(CharacterBody body)
        {
            GlobalEventManager.onClientDamageNotified += Record;
            body.master.onBodyDestroyed += Cease;
            Log.Debug($"Tracking {targetName}.");
            return this;
        }

        internal void Cease(CharacterBody _ = null)
        {
            if (timeOfDeath <= 0) timeOfDeath = Time.time;
            GlobalEventManager.onClientDamageNotified -= Record;
            if (targetBody?.master != null) targetBody.master.onBodyDestroyed -= Cease;
            else Log.Warning($"Could not unsubscribe {targetName} {nameof(CharacterMaster.onBodyDestroyed)}.");

            var caller = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            Log.Debug($"Untracking {targetName}. | {caller.DeclaringType}::{caller.Name}");
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

            if (entries.Count > Plugin.Config.EntryMaxCount) Prune();
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
            for (int i = 0; i < orderedEntries.Count; i++) {
                if (i >= Plugin.Config.EntryMaxCount) {
                    entries.Remove(orderedEntries[i].identifier);
                }
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
