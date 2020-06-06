namespace MoreRareAnimals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Harmony;
    using JetBrains.Annotations;
    using UnityEngine;
    using Random = UnityEngine.Random;

    [ModTitle("MoreRareAnimals")]
    [ModDescription("Gives you the ability to change the general and rare animal spawn rate.")]
    [ModAuthor("janniksam")]
    [ModIconUrl("https://raw.githubusercontent.com/janniksam/RaftMod.MoreRareAnimals/master/morerareanimals.png")]
    [ModWallpaperUrl("https://raw.githubusercontent.com/janniksam/RaftMod.MoreRareAnimals/master/morerareanimals.png")]
    [ModVersionCheckUrl("https://www.raftmodding.com/api/v1/mods/morerareanimals/version.txt")]
    [ModVersion("1.2")]
    [RaftVersion("Update 11 (4677160)")]
    [ModIsPermanent(false)]
    public class MoreRareAnimals : Mod
    {
        private HarmonyInstance m_harmony;
        private static AssetBundle m_assetBundle;

        private const string HarmonyId = "com.janniksam.raftmods.morerareanimals";
        private const string ModNamePrefix = "<color=#42a7f5>MoreRare</color><color=#FF0000>Animals</color>";

        private const string MoreAnimalsArgumentsAreInvalid = "moreanimals: Needs one parameter.\n" +
                                                              "e.g. use \"moreanimals 4\" to let 4 times the amount of domestic animals spawn on each large island";
        private const string MoreAnimalsArgumentAreOutOfRange = "moreanimals: The spawn rate needs to be between 1 and 50.";

        private const string MoreAnimalScalesArgumentsAreInvalid = "moreanimalscales: Needs two parameters.\n" +
                                                              "e.g. use \"moreanimalscales 0.5 2.5\" lets animals spawn, that scale between 50% and 250% of its normal size.\n" +
                                                              "Default scale factors are 0.8 to 1.2.";
        private const string MoreAnimalScalesArgumentAreOutOfRange = "moreanimalscales: The factor needs to be between 0.5 and 3.";


        private const string MoreRareAnimalsArgumentsAreInvalid = "morerareanimals: Needs three parameters.\n" +
                                                                   "e.g. use \"morerareanimals 20 50 30\" to increase the chances for rare skin 1 to 50% and rare skin 2 to 30%";
        private const string MoreRareAnimalsArgumentsDoNotSumUpTo100 = "morerareanimals: Needs all arguments to have a total sum of exactly 100.";
        private const string MoreRareAnimalsSomeArgumentsAreNegative = "morerareanimals: Needs all arguments to be positive.";

        [UsedImplicitly]
        public IEnumerator Start()
        {
            var request = AssetBundle.LoadFromFileAsync("mods/ModData/MoreRareAnimals/llama_animator_fix.assets");
            yield return request;

            m_assetBundle = request.assetBundle;

            m_harmony = HarmonyInstance.Create(HarmonyId);
            m_harmony.PatchAll(Assembly.GetExecutingAssembly());
            RConsole.registerCommand(typeof(MoreRareAnimals),
                "Gives you the ability to change the rare animal spanwn rate.",
                "morerareanimals",
                SetRareProbabilities);

            RConsole.registerCommand(typeof(MoreRareAnimals),
                "Gives you the ability to change the domestic animal spawn rate.",
                "moreanimals",
                SetSpawnRate);

            RConsole.registerCommand(typeof(MoreRareAnimals),
                "Gives you the ability to change the default animal scale factors.",
                "moreanimalscales",
                SetAnimalScales);

            RConsole.Log(string.Format("{0} has been loaded!", ModNamePrefix));
        }

        [UsedImplicitly]
        public void OnModUnload()
        {
            m_assetBundle.Unload(true);
            m_harmony.UnpatchAll(HarmonyId);
            RConsole.Log(string.Format("{0} has been unloaded!", ModNamePrefix));
            Destroy(gameObject);
        }


        private void SetSpawnRate()
        {
            var args = RConsole.lcargs;
            if (args.Length != 2)
            {
                RConsole.Log(MoreAnimalsArgumentsAreInvalid);
                return;
            }


            int spawnRate;
            if ((!int.TryParse(args[1], out spawnRate)))
            {
                RConsole.Log(MoreAnimalsArgumentsAreInvalid);
                return;
            }

            if (spawnRate < 1 || spawnRate > 50)
            {
                RConsole.Log(MoreAnimalsArgumentAreOutOfRange);
                return;
            }

            ObjectSpawnerLandmarkEditPatch.DomesticSpawnFactor = spawnRate;
            RConsole.Log(string.Format("{0}: Animal factor set to {1} successfully.", ModNamePrefix, spawnRate));
        }

        private static void SetAnimalScales()
        {
            var args = RConsole.lcargs;
            if (args.Length != 3)
            {
                RConsole.Log(MoreAnimalScalesArgumentsAreInvalid);
                return;
            }

            float minimum;
            if ((!float.TryParse(args[1], out minimum)))
            {
                RConsole.Log(MoreAnimalScalesArgumentAreOutOfRange);
                return;
            }

            float maximum;
            if ((!float.TryParse(args[2], out maximum)))
            {
                RConsole.Log(MoreAnimalScalesArgumentAreOutOfRange);
                return;
            }

            if (minimum < 0.5 ||
                maximum > 3 ||
                minimum > maximum)
            {
                RConsole.Log(MoreAnimalScalesArgumentAreOutOfRange);
                return;
            }

            ScaleEditPatch.MinimumScaleFactor = minimum;
            ScaleEditPatch.MaximumScaleFactor = maximum;
            RConsole.Log(string.Format("{0}: Animal scale factor set to {1} min / {2} max successfully.", ModNamePrefix, minimum, maximum));
        }

        private static void SetRareProbabilities()
        {
            var args = RConsole.lcargs;
            if (args.Length != 4)
            {
                RConsole.Log(MoreRareAnimalsArgumentsAreInvalid);
                return;
            }

            int defaultSkin;
            if ((!int.TryParse(args[1], out defaultSkin)))
            {
                RConsole.Log(MoreRareAnimalsArgumentsAreInvalid);
                return;
            }

            int rareSkin1;
            if ((!int.TryParse(args[2], out rareSkin1)))
            {
                RConsole.Log(MoreRareAnimalsArgumentsAreInvalid);
                return;
            }

            int rareSkin2;
            if ((!int.TryParse(args[3], out rareSkin2)))
            {
                RConsole.Log(MoreRareAnimalsArgumentsAreInvalid);
                return;
            }

            if (rareSkin1 < 0 || rareSkin2 < 0 || defaultSkin < 0)
            {
                RConsole.Log(MoreRareAnimalsSomeArgumentsAreNegative);
                return;
            }

            if (rareSkin1 + rareSkin2 + defaultSkin != 100)
            {
                RConsole.Log(MoreRareAnimalsArgumentsDoNotSumUpTo100);
                return;
            }

            RareMaterialEditPatch.DefaultSkinProbability = defaultSkin;
            RareMaterialEditPatch.RareSkin1Probability = rareSkin1;
            RareMaterialEditPatch.RareSkin2Probability = rareSkin2;
            RConsole.Log("Rare animal probabilities set successfully.");
        }

        [HarmonyPatch(typeof(AI_NetworkBehaviour_Domestic)), HarmonyPatch("SetNetworkIndexes")]
        [UsedImplicitly]
        public class ScaleEditPatch
        {
            internal static float MinimumScaleFactor = -1;
            internal static float MaximumScaleFactor = -1;

            [UsedImplicitly]
            public static bool Prefix(
                // ReSharper disable InconsistentNaming
                AI_NetworkBehaviour_Animal __instance
                // ReSharper restore InconsistentNaming
                )
            {
                if (MinimumScaleFactor < 0 ||
                    MaximumScaleFactor < 0)
                {
                    return true;
                }

                __instance.localScaleInterval = new Interval_Float(MinimumScaleFactor, MaximumScaleFactor);

                var aiNetworkBehaviourLlama = __instance as AI_NetworkBehaviour_Llama;
                if (aiNetworkBehaviourLlama == null)
                {
                    return true;
                }

                var fixedLlamaAnimator = m_assetBundle.LoadAsset<RuntimeAnimatorController>("Llama_Animator");
                var componentInParent = aiNetworkBehaviourLlama.GetComponentInParent<Animator>();
                componentInParent.runtimeAnimatorController = fixedLlamaAnimator;
                return true;
            }
        }

        [HarmonyPatch(typeof(AI_Movement)), HarmonyPatch("ChangeMovementSpeedTowards")]
        [UsedImplicitly]
        public class ScaledSpeedEditPatch
        {
            [UsedImplicitly]
            public static bool Prefix(
                // ReSharper disable InconsistentNaming
                AI_Movement __instance,
                ref float target,
                ref float speed
                // ReSharper restore InconsistentNaming
                )
            {
                var aiNetworkBehaviourDomestic = __instance.GetComponentInParent(typeof(AI_NetworkBehaviour_Domestic)) as AI_NetworkBehaviour_Domestic;
                if (aiNetworkBehaviourDomestic == null)
                {
                    return true;
                }

                var speedFactor = aiNetworkBehaviourDomestic.scaledSize;
                var aiStateMachineDomestic = __instance.GetComponentInParent(typeof(AI_StateMachine_Domestic)) as AI_StateMachine_Domestic;

                // is running
                const int stateRunning = 1;
                if (aiStateMachineDomestic != null &&
                    aiStateMachineDomestic.currentState != null &&
                    aiStateMachineDomestic.currentState.stateIndex == stateRunning &&
                    speedFactor > 3)
                {
                    speedFactor = 3;
                }

                target *= speedFactor;
                speed *= speedFactor;
                return true;
            }
        }

        [HarmonyPatch(typeof(RareMaterial)), HarmonyPatch("AssignRandomRareMaterial")]
        [UsedImplicitly]
        public class RareMaterialEditPatch
        {
            internal static int DefaultSkinProbability = -1;
            internal static int RareSkin1Probability = -1;
            internal static int RareSkin2Probability = -1;

            [UsedImplicitly]
            public static bool Prefix(
                    // ReSharper disable InconsistentNaming
                    // ReSharper disable SuggestBaseTypeForParameter
                    RareMaterial __instance)
            // ReSharper restore SuggestBaseTypeForParameter
            // ReSharper restore InconsistentNaming
            {
                if (!Semih_Network.IsHost)
                {
                    return true;
                }

                if (DefaultSkinProbability < 0 ||
                    RareSkin1Probability < 0 ||
                    RareSkin2Probability < 0)
                {
                    return true;
                }

                var aiNetworkBehaviourLlama = __instance.GetComponentInParent<AI_NetworkBehaviour_Llama>();
                var aiNetworkBehaviourGoat = __instance.GetComponentInParent<AI_NetworkBehaviour_Goat>();
                var aiNetworkBehaviourChicken = __instance.GetComponentInParent<AI_NetworkBehaviour_Chicken>();
                if (aiNetworkBehaviourLlama == null && aiNetworkBehaviourGoat == null && aiNetworkBehaviourChicken == null)
                {
                    // Not a child of a supported animal
                    return true;
                }

                var randomItems = (RandomItem[])Traverse.Create(__instance.randomizer).Field("items").GetValue();
                if (randomItems == null ||
                    randomItems.Length != 3 ||
                    Math.Abs(randomItems[0].weight - 22) > 0.1)
                {
                    // Does not have exactly 3 random items connected or does not match a known type
                    return true;
                }

                randomItems[0].weight = DefaultSkinProbability;
                randomItems[0].spawnChance = DefaultSkinProbability + "%";
                randomItems[1].weight = RareSkin1Probability;
                randomItems[1].spawnChance = RareSkin1Probability + "%";
                randomItems[2].weight = RareSkin2Probability;
                randomItems[2].spawnChance = RareSkin2Probability + "%";

                return true;
            }
        }


        [HarmonyPatch(typeof(LandmarkEntitySpawner)), HarmonyPatch("CreateEntity")]
        [UsedImplicitly]
        public class ObjectSpawnerLandmarkEditPatch
        {
            private static bool m_isInPatchMode;
            private static readonly Queue<uint> m_disposableAnimals = new Queue<uint>();
            private static readonly List<string> m_usableItemNames = new List<string>()
            {
                "Palm",
                "Pineapple",
                "Watermelon",
                "Mango",
                "Flower",
                "Berry",
                "Mushroom"
            };

            private static readonly AI_NetworkBehaviourType[] m_domesticTypes =
            {
                AI_NetworkBehaviourType.Llama,
                AI_NetworkBehaviourType.Chicken,
                AI_NetworkBehaviourType.Goat
            };

            public static int DomesticSpawnFactor = 1;

            [UsedImplicitly]
            public static bool Prefix(
                    // ReSharper disable InconsistentNaming
                    // ReSharper disable SuggestBaseTypeForParameter
                    LandmarkEntitySpawner __instance)
            // ReSharper restore SuggestBaseTypeForParameter
            // ReSharper restore InconsistentNaming
            {
                if (!Semih_Network.IsHost)
                {
                    return true;
                }

                if (!m_domesticTypes.Contains(__instance.entityType))
                {
                    return true;
                }

                if (m_isInPatchMode)
                {
                    return true;
                }

                try
                {
                    m_isInPatchMode = true;

                    var usableLocations = __instance.landmark.GetComponentsInChildren<MonoBehaviour>()
                        .Where(p => m_usableItemNames.Any(u => p.name.Contains(u)))
                        .ToArray();

                    __instance.landmark.OnLandmarkRemoved += OnLandmarkRemoved;
                    for (var i = 1; i < DomesticSpawnFactor; i++)
                    {
                        var rangePosition = Random.Range(0, usableLocations.Length);
                        var rangeDomesticTypes = Random.Range(0, m_domesticTypes.Length);

                        var position = usableLocations[rangePosition].transform.position;
                        var domesticType = m_domesticTypes[rangeDomesticTypes];

                        var behaviour = SpawnAnimal(domesticType, position);
                        m_disposableAnimals.Enqueue(behaviour.ObjectIndex);
                    }
                }
                finally
                {
                    m_isInPatchMode = false;
                }

                return true;
            }

            private static AI_NetworkBehaviour SpawnAnimal(AI_NetworkBehaviourType animalType, Vector3 position)
            {
                if (!Semih_Network.IsHost)
                {
                    return null;
                }

                return ComponentManager<Network_Host_Entities>.Value.CreateAINetworkBehaviour(animalType, position);
            }

            private static void OnLandmarkRemoved()
            {
                while (m_disposableAnimals.Count > 0)
                {
                    var index = m_disposableAnimals.Dequeue();
                    NetworkIDManager.SendIDBehaviourDead(index, typeof(AI_NetworkBehaviour), true);
                }
            }
        }
    }
}
