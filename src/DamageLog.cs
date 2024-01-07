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

        public DamageLog(NetworkUser user)
        {
            if (user && user.GetCurrentBody() == null) Log.Warning($"{user.userName} has no body yet?");
            if (user?.GetCurrentBody() == null) return;

            this.user = user;
            this.body = user.GetCurrentBody();

            if (Logs.TryGetValue(user, out DamageLog log)) log.Cease();
            Logs[user] = this;

            GlobalEventManager.onCharacterDeathGlobal += OnDeath;
            GlobalEventManager.onClientDamageNotified += Record;
            body.master.onBodyDestroyed += Cease;
            Log.Debug($"Tracking {user.userName}.");
        }

        private void OnDeath(DamageReport report)
        {
            if (report != null && report.victimBody != body) return;

            timeOfDeath = Time.time;
            GlobalEventManager.onCharacterDeathGlobal -= OnDeath;
        }

        private void Cease(CharacterBody body = null)
        {
            if (timeOfDeath <= 0) timeOfDeath = Time.time;
            GlobalEventManager.onCharacterDeathGlobal -= OnDeath;
            GlobalEventManager.onClientDamageNotified -= Record;
            if (body?.master != null) body.master.onBodyDestroyed -= Cease;
            Log.Debug($"Untracking {user.userName}.");
        }

        private void Record(DamageDealtMessage e)
        {
            if (e.victim != body.gameObject) return;

            string key = DamageSource.GenerateIdentifier(e.attacker, DamageSource.IsFallDamage(e), DamageSource.IsVoidFogDamage(e));
            if (entries.TryGetValue(key, out DamageSource src)) src.Add(e);
            else entries.Add(key, new DamageSource(e));

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
