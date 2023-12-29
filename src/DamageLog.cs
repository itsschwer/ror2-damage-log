﻿using RoR2;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DamageLog
{
    public class DamageLog
    {
        public static readonly Dictionary<NetworkUser, DamageLog> Logs = [];

        public static void ClearAll()
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
            if (user?.GetCurrentBody() == null) return;

            this.user = user;
            this.body = user.GetCurrentBody();

            if (Logs.TryGetValue(user, out DamageLog log)) log.Cease();
            Logs[user] = this;

            GlobalEventManager.onCharacterDeathGlobal += OnDeath;
            GlobalEventManager.onClientDamageNotified += Record;
            body.master.onBodyDestroyed += Cease;
            Log.Debug($"{Plugin.GUID}> tracking {user.masterController.GetDisplayName()}.");
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
            GlobalEventManager.onClientDamageNotified -= Record;
            if (body?.master != null) body.master.onBodyDestroyed -= Cease;
            Log.Debug($"{Plugin.GUID}> untracking {user.masterController.GetDisplayName()}.");
        }

        public List<DamageSource> GetEntries()
        {
            List<DamageSource> list = entries.Values.ToList();
            if (list.Count > 1)
                list.Sort((a, b) => System.Math.Sign(b.time - a.time)); // Newest first
            return list;
        }

        private void Record(DamageDealtMessage e)
        {
            if (e.victim != body.gameObject) return;

            string key = DamageSource.GenerateIdentifier(e);
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

        public class DamageSource
        {
            public readonly string identifier;

            public readonly Texture attackerPortrait;
            public readonly string attackerName;

            public readonly bool isFallDamage;
            public readonly bool isVoidFogDamage;

            public readonly float timeStart;
#pragma warning disable IDE1006 // Naming rule violation: must begin with upper case character
            public float time { get; private set; }
            public int hits { get; private set; }
            public float damage { get; private set; }
            public float damagePercent { get; private set; }
            public float hpPercent { get; private set; }
#pragma warning restore IDE1006 // Naming rule violation: must begin with upper case character

            private static Texture _PlanetPortrait;
            public static Texture PlanetPortrait {
                get {
                    if (_PlanetPortrait == null) {
                        _PlanetPortrait = LegacyResourcesAPI.Load<Texture>("Textures/BodyIcons/texUnidentifiedKillerIcon");
                    }
                    return _PlanetPortrait;
                }
            }

            public DamageSource(DamageDealtMessage e)
            {
                identifier = GenerateIdentifier(e);

                isFallDamage = e.damageType.HasFlag(DamageType.FallDamage);
                isVoidFogDamage = IsVoidFogDamage(e);

                // RoR2.UI.GameEndReportPanelController.SetPlayerInfo()
                attackerName = Language.GetString("UNIDENTIFIED_KILLER_NAME");
                attackerPortrait = PlanetPortrait;

                if (e.attacker) {
                    string name = Util.GetBestBodyName(e.attacker);
                    Texture portrait = e.attacker?.GetComponent<CharacterBody>()?.portraitIcon;

                    if (!string.IsNullOrEmpty(name)) attackerName = name;
                    if (portrait != null) attackerPortrait = portrait;
                }
                else if (isFallDamage) {
                    attackerName = "The Ground";
                    attackerPortrait = RoR2Content.Artifacts.weakAssKneesArtifactDef.smallIconSelectedSprite.texture;
                }
                else if (isVoidFogDamage) {
                    attackerName = Language.GetString("VOIDCAMPCENTER_NAME");
                    attackerPortrait = RoR2Content.Buffs.VoidFogMild.iconSprite.texture;
                }

                timeStart = Time.time;
                time = timeStart;
                hits = 1;
                damage = e.damage;
                UpdateHpDamagePercent(e.victim);
            }

            public DamageSource Add(DamageDealtMessage e)
            {
                time = Time.time;
                hits++;
                damage += e.damage;
                UpdateHpDamagePercent(e.victim);
                return this;
            }

            private void UpdateHpDamagePercent(GameObject victim)
            {
                HealthComponent health = victim?.GetComponent<HealthComponent>();
                if (health != null) {
                    hpPercent = health.combinedHealthFraction;
                    damagePercent = damage / health.fullCombinedHealth;
                }
            }

            public static bool IsVoidFogDamage(DamageDealtMessage e)
            {
                // RoR2.FogDamageController.FixedUpdate()
                return (e.damageType == (DamageType.BypassArmor | DamageType.BypassBlock)
                    && e.damageColorIndex == DamageColorIndex.Void
                    && e.attacker == null);
                // Could use position to differentiate void fog instances
            }

            public static string GenerateIdentifier(DamageDealtMessage e)
            {
                if (e.attacker != null) return e.attacker.GetInstanceID().ToString();
                if (e.damageType.HasFlag(DamageType.FallDamage)) return "fall_damage";
                if (IsVoidFogDamage(e)) return "void_fog_damage";
                return "??";
            }
        }
    }
}
