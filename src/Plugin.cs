using BepInEx;
using RoR2;

namespace DamageLog
{
    [BepInPlugin(GUID, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = Author + "." + Name;
        public const string Author = "itsschwer";
        public const string Name = "DamageIndicator";
        public const string Version = "0.0.0";

        private void Awake()
        {
            Log.Init(Logger);
            Log.Message($"{Plugin.GUID}> awake.");
        }

        private void OnEnable()
        {
            CharacterBody.onBodyStartGlobal += TrackBody;
            Log.Message($"{Plugin.GUID}> enabled.");
        }

        private void OnDisable()
        {
            DamageLog.ClearAll();
            CharacterBody.onBodyStartGlobal -= TrackBody;
            Log.Message($"{Plugin.GUID}> disabled.");
        }

        private static void TrackBody(CharacterBody body)
        {
            if (!body.isPlayerControlled) return;

            new DamageLog(FindBodyUser(body));
        }

        public static NetworkUser FindBodyUser(CharacterBody body)
        {
            if (body == null) return null;

            foreach (NetworkUser user in NetworkUser.readOnlyInstancesList) {
                if (user.GetCurrentBody() == body) return user;
            }

            return null;
        }
    }
}
