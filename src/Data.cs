﻿using System.Collections.Generic;
using System.Linq;

namespace DamageLog
{
    internal sealed class Data
    {
        private readonly Dictionary<RoR2.NetworkUser, DamageLog> UserLogs = [];
        private readonly Dictionary<int, DamageLog> BossLogs = [];
        public bool HasBossLogs => BossLogs.Count > 0;

        internal void AddUserLog(RoR2.NetworkUser user, DamageLog log)
            => Add(UserLogs, user, log);

        internal void AddBossLog(int instanceID, DamageLog log)
            => Add(BossLogs, instanceID, log);

        internal bool TryGetUserLog(RoR2.NetworkUser user, out DamageLog log)
            => UserLogs.TryGetValue(user, out log);

        internal bool TryGetBossLog(int index, out DamageLog log)
            => TryGetDamageLog(index, BossLogs, out log);

        internal void ClearUserLogs()
        {
            Log.Debug("Clearing user damage logs.");
            Clear(UserLogs);
        }

        internal void ClearBossLogs()
        {
            Log.Debug("Clearing boss damage logs.");
            Clear(BossLogs);
        }

        internal void ClearAll()
        {
            ClearUserLogs();
            ClearBossLogs();
        }




        internal RoR2.NetworkUser CycleUser(RoR2.NetworkUser current, bool reverse)
        {
            if (UserLogs.Count <= 0) return null;

            int i = (current == null) ? 0 : RoR2.NetworkUser.readOnlyInstancesList.IndexOf(current);
            i = CycleCollectionIndex(i, RoR2.NetworkUser.readOnlyInstancesList, reverse);
            RoR2.NetworkUser user = RoR2.NetworkUser.readOnlyInstancesList[i];

            if (UserLogs.ContainsKey(user)) return user;
            // Probably fine
            return CycleUser(user, reverse);
        }

        internal int CycleBossIndex(int current, bool reverse)
            => CycleCollectionIndex(current, BossLogs, reverse);

        internal static int CycleCollectionIndex(int index, System.Collections.ICollection collection, bool reverse)
        {
            if (reverse) index--;
            else index++;

            if (index < 0) index = collection.Count - 1;
            else if (index >= collection.Count) index = 0;

            return index;
        }




        public static void Add<TKey>(Dictionary<TKey, DamageLog> logs, TKey key, DamageLog newLog)
        {
            if (logs.TryGetValue(key, out DamageLog log)) {
                log.Cease();
                Log.Debug("Replacing existing damage log.");
            }
            logs[key] = newLog;
        }

        public static void Clear<TKey>(Dictionary<TKey, DamageLog> logs)
        {
            foreach (DamageLog log in logs.Values) log.Cease();
            logs.Clear();
        }

        public static bool TryGetDamageLog<TKey>(int index, Dictionary<TKey, DamageLog> dictionary, out DamageLog log)
        {
            log = null;
            if (index < 0) return false;
            if (index >= dictionary.Count) return false;

            log = dictionary.ElementAt(index).Value;
            return true;
        }
    }
}
