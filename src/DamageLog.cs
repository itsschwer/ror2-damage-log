using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DamageLog
{
    public sealed class DamageLog
    {
        public static readonly Dictionary<NetworkUser, DamageLog> UserLogs = [];
        public static readonly Dictionary<int, DamageLog> BossLogs = [];

        internal static void ClearLogs()
        {
            foreach (DamageLog log in UserLogs.Values) log.Cease();
            UserLogs.Clear();

            ClearBossLogs();
        }

        internal static void ClearBossLogs()
        {
            foreach (DamageLog log in BossLogs.Values) log.Cease();
            BossLogs.Clear();
        }




        public readonly string targetDisplayName;
        public readonly string targetDisplayStyle;
        private readonly bool entriesExpire = true;
        private readonly Dictionary<string, DamageSource> entries = [];
        private readonly CharacterBody targetBody;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        public float timeOfDeath { get; private set; }  = -1;
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        public DamageLog(NetworkUser user, CharacterBody body)
        {
            if (user == null || body == null) return;

            targetBody = body;
            targetDisplayName = user.userName;

            if (UserLogs.TryGetValue(user, out DamageLog log)) log.Cease();
            UserLogs[user] = Track(body);
        }

        public DamageLog(CharacterBody body, BossGroup boss)
        {
            if (body == null) return;
            // Do not track "Horde of Many"
            if (string.IsNullOrEmpty(body.subtitleNameToken) || body.subtitleNameToken == "NULL_SUBTITLE") return;

            targetBody = body;
            targetDisplayName = Util.GetBestBodyName(body.gameObject);
            targetDisplayStyle = "cIsHealth";
            entriesExpire = false;

            // Shrine of the Mountain and "Horde of Many" member count is dynamic, '1' will be omitted
            if (boss.combatSquad != null && boss.combatSquad.memberCount > 1) {
                targetDisplayName += $" <style=cStack>{boss.combatSquad.readOnlyMembersList.IndexOf(body.master) + 1}</style>";
            }

            int key = body.GetInstanceID();
            if (BossLogs.TryGetValue(key, out DamageLog log)) log.Cease();
            BossLogs[key] = Track(body);
        }

        private DamageLog Track(CharacterBody body)
        {
            GlobalEventManager.onClientDamageNotified += Record;
            body.master.onBodyDestroyed += Cease;
            Log.Debug($"Tracking {targetDisplayName}.");
            return this;
        }

        private void Cease(CharacterBody _ = null)
        {
            if (timeOfDeath <= 0) timeOfDeath = Time.time;
            GlobalEventManager.onClientDamageNotified -= Record;
            if (targetBody?.master != null) targetBody.master.onBodyDestroyed -= Cease;
            else Log.Warning($"Could not unsubscribe {targetDisplayName} {nameof(CharacterMaster.onBodyDestroyed)}.");

            var caller = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod();
            Log.Debug($"Untracking {targetDisplayName}. | {caller.DeclaringType}::{caller.Name}");
        }

        private void Record(DamageDealtMessage e)
        {
            if (targetBody == null || e.victim != targetBody.gameObject) return;

            string key = DamageSource.GenerateIdentifier(e.attacker, DamageSource.IsFallDamage(e), DamageSource.IsVoidFogDamage(e));
            if (entries.TryGetValue(key, out DamageSource latest)) {
                latest.Add(e);
            }
            else {
                latest = new DamageSource(e);
                entries.Add(key, latest);
            }

            if (latest.remainingHpPercent <= 0f) timeOfDeath = Time.time;

            if (entries.Count > Plugin.Config.EntryMaxCount) Prune();
        }

        private void Prune()
        {
            int i = 0;
            float endTime = (timeOfDeath > 0) ? timeOfDeath : Time.time;
            foreach (DamageSource src in GetEntries()) {
                TryPrune(src, endTime, i);
                i++;
            }
        }

        public bool TryPrune(DamageSource src, float endTime, int i)
        {
            bool expired = (entriesExpire && (endTime - src.time >= Plugin.Config.EntryMaxRetainTime));
            if (i > Plugin.Config.EntryMaxCount || expired) {
                entries.Remove(src.identifier);
                return true;
            }
            return false;
        }

        public List<DamageSource> GetEntries()
        {
            List<DamageSource> list = entries.Values.ToList();
            if (list.Count > 1)
                list.Sort((a, b) => System.Math.Sign(b.time - a.time)); // Newest first
            return list;
        }
    }
}
