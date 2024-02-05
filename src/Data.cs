using System.Collections.Generic;

namespace DamageLog
{
    internal sealed class Data
    {
        internal readonly Dictionary<RoR2.NetworkUser, DamageLog> UserLogs = [];
        internal readonly Dictionary<int, DamageLog> BossLogs = [];

        public void Clear<TKey>(Dictionary<TKey, DamageLog> dictionary)
        {
            foreach (DamageLog log in dictionary.Values) log.Cease();
            dictionary.Clear();
        }

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
    }
}
