﻿using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace DamageLog
{
    public class DamageLog
    {
        private readonly CharacterBody targetBody;
        private readonly string targetName;

        private readonly Dictionary<string, DamageSource> entries = [];
        private float timeOfDeath = -1;
        public float Time => (timeOfDeath > 0) ? timeOfDeath : UnityEngine.Time.time;

#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
        public virtual string displayName => targetName;
        public virtual string loggingName => targetName;
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

        public DamageLog(NetworkUser user, CharacterBody body) : this(body, user.userName) {}

        protected DamageLog(CharacterBody targetBody, string targetName)
        {
            this.targetBody = targetBody;
            this.targetName = targetName;

            Track(targetBody);
        }

        private DamageLog Track(CharacterBody body)
        {
            GlobalEventManager.onClientDamageNotified += Record;
            body.master.onBodyDestroyed += Cease;
            Plugin.Logger.LogDebug($"Tracking {loggingName}.");
            return this;
        }

        internal void Untrack()
        {
            if (timeOfDeath <= 0) timeOfDeath = UnityEngine.Time.time;
            GlobalEventManager.onClientDamageNotified -= Record;
            if (targetBody?.master != null) targetBody.master.onBodyDestroyed -= Cease;
            else Plugin.Logger.LogWarning($"Could not unsubscribe {nameof(RoR2)}.{nameof(CharacterMaster)}::{nameof(CharacterMaster.onBodyDestroyed)} for {loggingName}.");
        }

        private void Cease(CharacterBody _)
        {
            Untrack();
            Plugin.Logger.LogDebug($"Untracking {loggingName} (body destroyed).");
        }

        private void Record(DamageDealtMessage e)
        {
            if (targetBody == null || e.victim != targetBody.gameObject) return;

            string key = e.GenerateIdentifier();
            if (entries.TryGetValue(key, out DamageSource latest) && !IsExpired(Time - latest.time)) {
                latest.Add(e);
            }
            else {
                entries.Remove(key);
                latest = new DamageSource(e);
                entries.Add(key, latest);
            }

            if (latest.remainingHpPercent <= 0f) timeOfDeath = UnityEngine.Time.time;

            Decay();
        }

        private void Decay()
        {
            float now = Time;
            foreach (DamageSource src in GetEntries()) {
                if (IsExpired(now - src.time)) {
                    entries.Remove(src.identifier);
                }
            }
        }

        public virtual bool IsExpired(float elapsedTime)
            => (elapsedTime > Plugin.Config.EntryMaxRetainTime);

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
    }
}
