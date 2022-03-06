using BepInEx;
using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

// Allow scanning for ConCommand, and other stuff for Risk of Rain 2
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace ProtectTheVIP
{
    [BepInPlugin(GUID, ModName, Version)]
    public class ProtectTheVIP : BaseUnityPlugin
    {
        public const string GUID = "com.justinderby.protectthevip";
        public const string ModName = "ProtectTheVIP";
        public const string Version = "1.0.7";

        //private GameObject VIPRunPrefab;

        // This is only used for ConCommands, since they need to be static...
        public static ProtectTheVIP Instance;

        public static ConfigEntry<bool> LogSpawnCards { get; set; }
        public static ConfigEntry<string> CustomSpawnCards { get; set; }

        public void Awake()
        {
            Instance = SingletonHelper.Assign(Instance, this);

            LogSpawnCards = Config.Bind("SpawnCards", "LogToConsole", false, "Log all available spawn cards from \"SpawnCards/CharacterSpawnCards/\"");
            CustomSpawnCards = Config.Bind("SpawnCards", "CustomSpawnCards",
                "SpawnCards/CharacterSpawnCards/cscJellyfish; SpawnCards/CharacterSpawnCards/cscRoboBallMini",
                "Semi-colon seperated (;) entries of Spawn Cards");

            if (LogSpawnCards.Value)
            {
                List<String> allPaths = new List<string>();
                LegacyResourcesAPI.GetAllPaths(allPaths);

                foreach (var resource in allPaths)
                {
                    if (resource.StartsWith("SpawnCards/CharacterSpawnCards/"))
                    {
                        Debug.LogError(resource);
                    }
                }
            }

            // FIXME: Get rid of this hack and just use a prefab
            On.RoR2.NetworkSession.BeginRun += NetworkSession_BeginRun;
            // This is the real way to integrate
            // GameModeCatalog.getAdditionalEntries += RegisterVIPRun;
        }

        private Run NetworkSession_BeginRun(On.RoR2.NetworkSession.orig_BeginRun orig, NetworkSession self, Run runPrefabComponent, RuleBook ruleBook, ulong seed)
        {
            //if (!NetworkServer.active)
            //{
            //    Debug.LogWarning("[Server] function 'RoR2.Run RoR2.NetworkSession::BeginRun(RoR2.Run,RoR2.RuleBook,System.UInt64)' called on client");
            //    return null;
            //}
            //if (!Run.instance)
            //{
            //    GameObject gameObject = Instantiate(runPrefabComponent.gameObject);
            //    // FIXME: Get rid of this hack and just use a prefab
            //    gameObject.SetActive(true);
            //    Run component = gameObject.GetComponent<Run>();
            //    component.SetRuleBook(ruleBook);
            //    component.seed = seed;
            //    Debug.LogError(gameObject);
            //    foreach (var child in gameObject.transform)
            //    {
            //        Debug.LogError($"Child = {child}");
            //    }
            //    NetworkServer.Spawn(gameObject);
            //    return component;
            //}
            //return null;
            Run run = orig(self, runPrefabComponent, ruleBook, seed);
            Debug.Log("Applying Protect the VIP Behaviour");
            VIPBehaviour vip = run.gameObject.AddComponent<VIPBehaviour>();
            SetupVIP(vip.characterAllies);
            return run;
        }

        //private void RegisterVIPRun(List<GameObject> obj)
        //{
        //    GameObject gameObject = new GameObject("VIPRun");
        //    gameObject.SetActive(false);
        //    DontDestroyOnLoad(gameObject);

        //    gameObject.AddComponent<NetworkIdentity>();
        //    gameObject.AddComponent<TeamManager>();
        //    gameObject.AddComponent<RunCameraManager>();

        //    VIPRun run = gameObject.AddComponent<VIPRun>();

        //    if (!gameObject.GetComponent<RunArtifactManager>())
        //    {
        //        Debug.LogError("REGISTERING RunArtifactManager");
        //        gameObject.AddComponent<RunArtifactManager>();
        //    }
        //    if (!gameObject.GetComponent<NetworkRuleBook>())
        //    {
        //        Debug.LogError("REGISTERING NetworkRuleBook");
        //        gameObject.AddComponent<NetworkRuleBook>();
        //    }

        //    run.userPickable = true;
        //    run.nameToken = "GAMEMODE_VIP_RUN_NAME";
        //    run.name = "VIPRun";

        //    SetupVIP(run.characterAllies);

        //    obj.Add(gameObject);
        //}

        private void SetupVIP(List<CharacterSpawnCard> characterAllies)
        {
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscBeetle"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscLemurian"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscLemurianBruiser"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscMiniMushroom"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscParent"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscVulture"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscBison"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscBell"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscClayBruiser"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscGolem"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscImp"));
            characterAllies.Add(LegacyResourcesAPI.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscScav"));

            string spawnCards = CustomSpawnCards.Value;
            if (string.IsNullOrWhiteSpace(spawnCards))
            {
                return;
            }

            foreach (var spawnCard in spawnCards.Split(';'))
            {
                string spawnCardSanitized = spawnCard.Trim();
                try
                {
                    CharacterSpawnCard csc = LegacyResourcesAPI.Load<CharacterSpawnCard>(spawnCardSanitized);
                    if (csc)
                    {
                        characterAllies.Add(csc);
                        Debug.Log($"Loaded custom Spawn Card: {spawnCardSanitized}");
                    }
                    else
                    {
                        Debug.LogError($"Could not load Spawn Card: {spawnCardSanitized}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception loading Spawn Card: {spawnCardSanitized}");
                    Debug.LogException(e);
                }
            }
        }
    }
}
