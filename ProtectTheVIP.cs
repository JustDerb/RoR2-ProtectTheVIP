using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// TODO:
// Fix AI to follow better
// Teleport VIP if you get too far from them??
// Ping to make AI follow??
namespace ProtectTheVIP
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInPlugin(GUID, ModName, Version)]
    [R2APISubmoduleDependency(nameof(CommandHelper))]
    [R2APISubmoduleDependency(nameof(R2API.LanguageAPI))]
    [R2APISubmoduleDependency(nameof(R2API.ResourcesAPI))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    public class ProtectTheVIP : BaseUnityPlugin
    {
        public const string GUID = "com.justinderby.protectthevip";
        public const string ModName = "Protect The VIP";
        public const string Version = "1.0.6";

        //private GameObject VIPRunPrefab;

        // This is only used for ConCommands, since they need to be static...
        public static ProtectTheVIP Instance;

        public static ConfigEntry<bool> LogSpawnCards { get; set; }
        public static ConfigEntry<string> CustomSpawnCards { get; set; }

        public void Awake()
        {
            Instance = SingletonHelper.Assign(Instance, this);

            CommandHelper.AddToConsoleWhenReady();

            LogSpawnCards = Config.Bind("SpawnCards", "LogToConsole", false, "Log all available spawn cards from \"SpawnCards/CharacterSpawnCards/\"");
            CustomSpawnCards = Config.Bind("SpawnCards", "CustomSpawnCards",
                "SpawnCards/CharacterSpawnCards/cscJellyfish; SpawnCards/CharacterSpawnCards/cscRoboBallMini",
                "Semi-colon seperated (;) entries of Spawn Cards");

            if (LogSpawnCards.Value)
            {
                foreach (var resource in Resources.LoadAll<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/"))
                {
                    Debug.LogError(resource.name);
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
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscBeetle"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscLemurian"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscLemurianBruiser"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscMiniMushroom"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscParent"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscVulture"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscBison"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscBell"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscClayBruiser"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscGolem"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscImp"));
            characterAllies.Add(Resources.Load<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards/cscScav"));

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
                    CharacterSpawnCard csc = Resources.Load<CharacterSpawnCard>(spawnCardSanitized);
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
