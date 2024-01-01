#if DEBUG
using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace DamageLog
{
    public static class Director
    {
        public static void SpawnNearBody(SpawnCard spawnCard, CharacterBody body, TeamIndex teamIndexOverride = TeamIndex.Void)
        {
            DirectorPlacementRule placement = new DirectorPlacementRule {
                spawnOnTarget = body.coreTransform,
                placementMode = DirectorPlacementRule.PlacementMode.NearestNode
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
            if (spawn.skipSpawnWhenSacrificeArtifactEnabled)
            {
                // Create modified copy
                spawn = ScriptableObject.Instantiate(spawn);
                spawn.skipSpawnWhenSacrificeArtifactEnabled = false;
            }

            return spawn;
        }

        // https://github.com/Goorakh/RiskOfChaos/blob/149f6e103588a66ae83c5539f6f778fd2d405915/RiskOfChaos/EffectDefinitions/World/Spawn/SpawnRandomInteractable.cs#L47-L63
        public static InteractableSpawnCard CreateCauldronSpawnCard(string assetPath = "RoR2/Base/LunarCauldrons/LunarCauldron, GreenToRed Variant.prefab") {
            int lastSlashIndex = assetPath.LastIndexOf('/');
            string cardName = assetPath.Substring(lastSlashIndex + 1, assetPath.LastIndexOf('.') - lastSlashIndex - 1);

            InteractableSpawnCard spawn = ScriptableObject.CreateInstance<InteractableSpawnCard>();
            spawn.prefab = Addressables.LoadAssetAsync<GameObject>(assetPath).WaitForCompletion();
            spawn.name = cardName;
            spawn.orientToFloor = true;
            spawn.hullSize = HullClassification.Golem;
            spawn.requiredFlags = NodeFlags.None;
            spawn.forbiddenFlags = NodeFlags.NoChestSpawn;
            spawn.occupyPosition = true;
            spawn.sendOverNetwork = true;

            return spawn;
        }

        public static void SpawnInteractable(Interactable interactable, CharacterBody body)
        {
            string[] paths = [
                "RoR2/Base/Scrapper/iscScrapper.asset",
                "RoR2/Base/ShrineChance/iscShrineChanceSnowy.asset",
                "RoR2/Base/ShrineBlood/iscShrineBloodSnowy.asset",
                "RoR2/DLC1/VoidChest/iscVoidChest.asset",
                "RoR2/DLC1/VoidTriple/iscVoidTriple.asset"
            ];

            SpawnNearBody(LoadInteractableSpawnCard(paths[(int)interactable]), body);
        }

        public enum Interactable
        {
            Scrapper,
            ShrineChance,
            ShrineBlood,
            VoidChest,
            VoidTriple
        }
    }
}
#endif
