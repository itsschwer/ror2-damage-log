using RoR2;

namespace DamageLog
{
    internal sealed class PlayerDamageLog(NetworkUser user, CharacterBody body) : DamageLog(body, user.userName)
    {
    }
}
