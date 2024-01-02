#if DEBUG
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace DamageLog
{
    public static class Debug
    {
        private static int stageIndex;
        public static void ChangeStage()
        {
            string[] stages = ["artifactworld", "goolake", "frozenwall", "sulfurpools", "voidstage", "arena"];
            stageIndex++; if (stageIndex >= stages.Length) stageIndex = 0;
            Run.instance.GenerateStageRNG();
            NetworkManager.singleton.ServerChangeScene(stages[stageIndex]);
        }

        public static void GiveItem(NetworkUser user, ItemDef item, int count = 1)
        {
            if (user?.master?.inventory == null) return;
            user.master.inventory.GiveItem(item, count);
        }

        public static void SpawnAtBody(SpawnCard spawnCard, CharacterBody body, TeamIndex teamIndexOverride = TeamIndex.Void)
        {
            DirectorPlacementRule placement = new DirectorPlacementRule {
                position = body.footPosition,
                placementMode = DirectorPlacementRule.PlacementMode.Direct
            };
            DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Standard, out placement.minDistance, out placement.maxDistance);
            GameObject obj = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placement, RoR2Application.rng) { teamIndexOverride = teamIndexOverride });
            if (obj != null) NetworkServer.Spawn(obj);
        }

        // https://github.com/Goorakh/RiskOfChaos/blob/149f6e103588a66ae83c5539f6f778fd2d405915/RiskOfChaos/EffectDefinitions/World/Spawn/SpawnRandomInteractable.cs#L23-L35
        public static InteractableSpawnCard LoadInteractableSpawnCard(string assetPath)
        {
            InteractableSpawnCard spawn = Addressables.LoadAssetAsync<InteractableSpawnCard>(assetPath).WaitForCompletion();
            // Ignore Artifact of Sacrifice
            if (spawn.skipSpawnWhenSacrificeArtifactEnabled) {
                // Create modified copy
                spawn = ScriptableObject.Instantiate(spawn);
                spawn.skipSpawnWhenSacrificeArtifactEnabled = false;
            }

            return spawn;
        }

        public enum Interactable {
            ShrineBlood,
            VoidChest,
            VoidTriple
        }

        public static void SpawnInteractable(Interactable interactable, CharacterBody body)
        {
            string[] paths = [
                "RoR2/Base/ShrineBlood/iscShrineBloodSnowy.asset",
                "RoR2/DLC1/VoidChest/iscVoidChest.asset",
                "RoR2/DLC1/VoidTriple/iscVoidTriple.asset"
            ];

            SpawnAtBody(LoadInteractableSpawnCard(paths[(int)interactable]), body);
        }

        public static void SpawnInteractable(NetworkUser user)
        {
            if (user?.GetCurrentBody() == null) return;

            Interactable interactable = (Interactable)Random.Range((int)Interactable.ShrineBlood, (int)Interactable.VoidTriple + 1);
            if (Input.GetKey(KeyCode.B)) interactable = Interactable.ShrineBlood;
            else if (Input.GetKey(KeyCode.V)) interactable = Interactable.VoidChest;
            else if (Input.GetKey(KeyCode.P)) interactable = Interactable.VoidTriple;

            SpawnInteractable(interactable, user.GetCurrentBody());
        }
    }
}
#endif
