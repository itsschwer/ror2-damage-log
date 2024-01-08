using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DamageLog
{
    public sealed class DamageLog
    {
        public static readonly Dictionary<NetworkUser, DamageLog> Logs = [];

        internal static void ClearAll()
        {
            foreach (DamageLog log in Logs.Values) log.Cease();
            Logs.Clear();
        }

        private readonly Dictionary<string, DamageSource> entries = [];

        public readonly NetworkUser user;

#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        private CharacterBody _body;
        private CharacterBody body {
            get {
                if (_body == null) Cease();
                return _body;
            }
            set { _body = value; }
        }

        public float timeOfDeath { get; private set; }  = -1;
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        public DamageLog(NetworkUser user, CharacterBody body)
        {
            if (user == null || body == null) return;

            this.user = user;
            this.body = body;

            if (Logs.TryGetValue(user, out DamageLog log)) log.Cease();
            Logs[user] = this;

            GlobalEventManager.onClientDamageNotified += Record;
            body.master.onBodyDestroyed += Cease;
            Log.Debug($"Tracking {user.userName}.");
        }

        private void Cease(CharacterBody body = null)
        {
            if (timeOfDeath <= 0) timeOfDeath = Time.time;
            GlobalEventManager.onClientDamageNotified -= Record;
            if (body?.master != null) body.master.onBodyDestroyed -= Cease;
            Log.Debug($"Untracking {user.userName}.");
        }

        private void Record(DamageDealtMessage e)
        {
            if (e.victim != body.gameObject) return;

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
            if (i > Plugin.Config.EntryMaxCount || (endTime - src.time >= Plugin.Config.EntryMaxRetainTime)) {
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
