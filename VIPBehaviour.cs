using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace ProtectTheVIP
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Run))]
    class VIPBehaviour : MonoBehaviour
    {
        private bool IsOnVIPSelectionStage
        {
            get {
                return run.stageClearCount == 0 && SceneCatalog.GetSceneDefForCurrentScene().baseSceneName == "limbo";
            }
        }

        private Run run;
        private SceneCollection initialStartingScenes;

        public List<CharacterSpawnCard> characterAllies = new List<CharacterSpawnCard>();
        
        private List<GameObject> spawnedBarrels;

        private CombatSquad allySquad;
        private List<CharacterMaster> allies;

        protected void Awake()
        {
            run = GetComponent<Run>();
            initialStartingScenes = run.startingSceneGroup;
            //run.startingSceneGroup = ScriptableObject.CreateInstance<SceneCollection>();
            //run.startingSceneGroup.SetSceneEntries(new SceneCollection.SceneEntry[] {
            //    new SceneCollection.SceneEntry {
            //        sceneDef = SceneCatalog.GetSceneDefFromSceneName("limbo"),
            //        weight = 1,
            //    },
            //});

            allySquad = gameObject.AddComponent<CombatSquad>();
            allySquad.onMemberLost += AllySquad_onMemberLost;
            allies = new List<CharacterMaster>();

            On.RoR2.Run.PickNextStageScene += Run_ForceVIPScene;

            IL.RoR2.TeamComponent.SetupIndicator += TeamComponent_SetupIndicator;

            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            Run.onServerGameOver += Run_onServerGameOver;
            On.RoR2.Run.OnServerBossAdded += Run_OnServerBossAdded;
            On.RoR2.RunReport.Generate += RunReport_Generate;
            On.RoR2.BarrelInteraction.OnInteractionBegin += BarrelInteraction_OnInteractionBegin;
        }

        private void Run_ForceVIPScene(On.RoR2.Run.orig_PickNextStageScene orig, Run self, WeightedSelection<SceneDef> choices)
        {
            Debug.Log("Forcing scene to 'limbo'");
            self.nextStageScene = SceneCatalog.GetSceneDefFromSceneName("limbo");
            // Remove ourselves
            On.RoR2.Run.PickNextStageScene -= Run_ForceVIPScene;
        }

        protected void OnDestroy()
        {
            IL.RoR2.TeamComponent.SetupIndicator -= TeamComponent_SetupIndicator;

            Stage.onStageStartGlobal -= Stage_onStageStartGlobal;
            On.RoR2.Run.OnServerBossAdded -= Run_OnServerBossAdded;
            On.RoR2.RunReport.Generate -= RunReport_Generate;
            On.RoR2.BarrelInteraction.OnInteractionBegin -= BarrelInteraction_OnInteractionBegin;
            On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter -= Phase1_OnEnter;
        }

        private void TeamComponent_SetupIndicator(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(x => x.MatchCallvirt(typeof(CharacterBody), "get_isPlayerControlled"));
            cursor.Next.OpCode = OpCodes.Nop;
            cursor.EmitDelegate<Func<CharacterBody, bool>>((body) =>
            {
                return body.isPlayerControlled || body.master.GetComponent<AllyMaster>() != null;
            });
        }

        private void AllySquad_onMemberLost(CharacterMaster master)
        {
            string[] standardDeathQuoteTokens = (string[])typeof(GlobalEventManager)
                        .GetField("standardDeathQuoteTokens", BindingFlags.Static | BindingFlags.NonPublic)
                        .GetValue(null);
            Chat.SendBroadcastChat(new Chat.PlayerDeathChatMessage
            {
                subjectAsCharacterBody = master.GetBody(),
                baseToken = standardDeathQuoteTokens[Random.Range(0, standardDeathQuoteTokens.Length)]
            });

            if (allySquad.memberCount == 0)
            {
                foreach (var controller in PlayerCharacterMasterController.instances)
                {
                    CharacterMaster characterMaster = controller.master;
                    if (!characterMaster.IsDeadAndOutOfLivesServer())
                    {
                        CharacterBody characterBody = characterMaster.GetBody();
                        if (characterBody)
                        {
                            EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/BrittleDeath"), new EffectData
                            {
                                origin = characterBody.corePosition,
                                rotation = Quaternion.identity,
                                scale = characterBody.bestFitRadius * 2f
                            }, true);
                        }
                        characterMaster.TrueKill(null, null, DamageType.VoidDeath);
                    }
                }
                TriggerGameOver();
            }
        }

        private void Run_onServerGameOver(Run arg1, GameEndingDef arg2)
        {
            allies.Clear();
            allySquad.onMemberLost -= AllySquad_onMemberLost;
        }

        private RunReport RunReport_Generate(On.RoR2.RunReport.orig_Generate orig, Run run, GameEndingDef gameEnding)
        {
            // Temporarily add the allies as a "player"
            // Sadly, singleplayer runs don't make this show up still...
            try
            {
                foreach (var ally in allies)
                {
                    if (ally.gameObject.GetComponent<PlayerCharacterMasterController>() == null)
                    {
                        ally.gameObject.AddComponent<PlayerCharacterMasterController>();
                    }
                    if (ally.gameObject.GetComponent<PlayerStatsComponent>() == null)
                    {
                        ally.gameObject.AddComponent<PlayerStatsComponent>();
                    }
                }
                return orig(run, gameEnding);
            }
            finally
            {
                foreach (var ally in allies)
                {
                    PlayerCharacterMasterController controller = ally.gameObject.GetComponent<PlayerCharacterMasterController>();
                    if (controller)
                    {
                        PlayerStatsComponent stats = controller.gameObject.GetComponent<PlayerStatsComponent>();
                        if (stats)
                        {
                            Destroy(stats);
                        }
                        Destroy(controller);
                    }
                }
            }
        }

        private void TriggerGameOver()
        {
            run.BeginGameOver(RoR2Content.GameEndings.StandardLoss);
        }

        private void Stage_onStageStartGlobal(Stage stage)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (IsOnVIPSelectionStage)
            {
                spawnedBarrels = new List<GameObject>(characterAllies.Count);
                foreach (var masterController in PlayerCharacterMasterController.instances)
                {
                    SetGodMode(masterController.master, true);
                    masterController.master.money = 0;
                }

                foreach (var masterController in PlayerCharacterMasterController.instances)
                {
                    if (masterController.master.GetBody())
                    {
                        SpawnVIPSelections(masterController.master);
                        StartCoroutine(SendChat(6f,
                            new Chat.SimpleChatMessage()
                            {
                                baseToken = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>Protect the VIP:</color> " +
                                    $"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>Select an ally you would like to " +
                                    $"protect for the run. Select the portal slot to choose no ally (disabling the mod).</color>"
                            }));
                        break;
                    }
                }
            }
            else if (run.stageClearCount == 0)
            {
                foreach (var masterController in PlayerCharacterMasterController.instances)
                {
                    SetGodMode(masterController.master, false);
                    masterController.master.GiveMoney(run.ruleBook.startingMoney);
                }
                foreach (var master in allies)
                {
                    SetGodMode(master, false);
                }
                spawnedBarrels.Clear();

                Debug.Log("Unhooking from RoR2.Run.OnServerBossAdded");
                On.RoR2.Run.OnServerBossAdded -= Run_OnServerBossAdded;
            }

            StartCoroutine(TeleportAlliesToFirstAvailablePlayerRoutine(null, 3));

            if (SceneCatalog.GetSceneDefForCurrentScene().baseSceneName == "moon" && allies.Count != 0)
            {
                // Stage with Mithrix, so we need to set up extra stuff for the AI
                StartCoroutine(SendChat(3f,
                    new Chat.SimpleChatMessage()
                    {
                        baseToken = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>Protect the VIP:</color> " +
                            $"The AI is broken on this stage... your ally will try to teleport to you when you reach the final area."
                    }));
                On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1_OnEnter;
            }
        }

        private IEnumerator SendChat(float delay, ChatMessageBase message)
        {
            yield return new WaitForSeconds(delay);
            Chat.SendBroadcastChat(message);
            yield break;
        }

        private void Phase1_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase1 self)
        {
            orig(self);

            bool teleportedAllies = false;
            if (!TeleportAlliesToFirstAvailablePlayer())
            {
                CharacterBody body = null;
                foreach (var masterController in PlayerCharacterMasterController.instances)
                {
                    if (masterController.master.GetBody())
                    {
                        body = masterController.master.GetBody();
                        break;
                    }
                }

                // Try one more time with a more direct approach
                if (body) {
                    Transform bodyTransform = getTransform(body);
                    Vector3 position = new Vector3(
                        bodyTransform.position.x + Random.Range(-5f, 5f),
                        bodyTransform.position.y + 10f,
                        bodyTransform.position.z + Random.Range(-5f, 5f));
                    if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 30f, LayerIndex.world.mask))
                    {
                        position = hit.point;
                    }
                    if (!TeleportAlliesToFirstAvailablePlayer(position))
                    {
                        Debug.LogWarning("Couldn't teleport allies to a player body!");
                        Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                        {
                            baseToken = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>Protect the VIP:</color> " +
                                $"Couldn't teleport allies to someone! I guess they'll sit this fight out."
                        });
                    }
                    else
                    {
                        teleportedAllies = true;
                    }
                }
            }
            else
            {
                teleportedAllies = true;
            }

            if (teleportedAllies)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                {
                    baseToken = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>Protect the VIP:</color> " +
                        $"Your ally joins you to fight by your side."
                });
            }

            Debug.Log("Unhooking from EntityStates.Missions.BrotherEncounter.Phase1.OnEnter");
            On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter -= Phase1_OnEnter;
        }

        private IEnumerator TeleportAlliesToFirstAvailablePlayerRoutine(Vector3? forcePosition = null, float delay = 0)
        {
            foreach (var ally in allies)
            {
                CharacterBody allyBody = ally.GetBody();
                if (allyBody)
                {
                    allyBody.AddTimedBuff(RoR2Content.Buffs.Cloak, delay);
                    allyBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, delay);
                }
            }
            yield return new WaitForSeconds(delay);
            if (!TeleportAlliesToFirstAvailablePlayer(forcePosition))
            {
                Debug.LogWarning("Couldn't teleport allies to a player body!");
            }
            yield break;
        }

        private bool TeleportAlliesToFirstAvailablePlayer(Vector3? forcePosition = null)
        {
            if (allies.Count == 0)
            {
                return true;
            }

            CharacterBody body = null;
            foreach (var masterController in PlayerCharacterMasterController.instances)
            {
                if (masterController.master.GetBody())
                {
                    body = masterController.master.GetBody();
                    break;
                }
            }

            if (!body)
            {
                return false;
            }

            bool teleportedAtLeastOneAlly = false;
            foreach (var ally in allies)
            {
                CharacterBody allyBody = ally.GetBody();
                if (!allyBody)
                {
                    continue;
                }
                Vector3? position = forcePosition;
                if (!forcePosition.HasValue)
                {
                    position = FindSafeTeleportDestination(body.footPosition, allyBody, RoR2Application.rng);
                }
                if (position.HasValue)
                {
                    TeleportHelper.TeleportBody(allyBody, position.Value);

                    allyBody.AddTimedBuff(RoR2Content.Buffs.Immune, 3f);
                    foreach (EntityStateMachine entityStateMachine in allyBody.GetComponents<EntityStateMachine>())
                    {
                        entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                    }

                    GameObject respawnEffect = Resources.Load<GameObject>("Prefabs/Effects/HippoRezEffect");
                    if (respawnEffect)
                    {
                        Transform bodyTransform = getTransform(allyBody);
                        EffectManager.SpawnEffect(respawnEffect, new EffectData
                        {
                            origin = position.Value,
                            rotation = bodyTransform.rotation,
                            scale = allyBody.bestFitRadius * 2f
                        }, true);
                    }
                    if (ally.GetBodyObject())
                    {
                        Util.PlaySound("Play_item_proc_extraLife", ally.GetBodyObject());
                    }

                    teleportedAtLeastOneAlly = true;
                }
            }

            return teleportedAtLeastOneAlly;
        }

        // Copy from TeleporterHelper, but uses a different strategy so they don't spawn on top of the person
        public static Vector3? FindSafeTeleportDestination(Vector3 characterFootPosition, CharacterBody characterBodyOrPrefabComponent, Xoroshiro128Plus rng)
        {
            Vector3? result = null;
            SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            spawnCard.hullSize = characterBodyOrPrefabComponent.hullClassification;
            //spawnCard.nodeGraphType = MapNodeGroup.GraphType.Ground;
            spawnCard.prefab = Resources.Load<GameObject>("SpawnCards/HelperPrefab");
            DirectorPlacementRule placementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                position = characterFootPosition
            };
            DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Close,
                out placementRule.minDistance, out placementRule.maxDistance);
            GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, placementRule, rng));
            if (gameObject)
            {
                result = new Vector3?(gameObject.transform.position);
                Destroy(gameObject);
            }
            Destroy(spawnCard);
            return result;
        }

        private void Shuffle<T>(List<T> list)
        {
            // Knuth shuffle algorithm
            for (int t = 0; t < list.Count; t++)
            {
                T tmp = list[t];
                int r = UnityEngine.Random.Range(t, list.Count);
                list[t] = list[r];
                list[r] = tmp;
            }
        }

        private void SpawnVIPSelections(CharacterMaster master)
        {
            Shuffle(characterAllies);

            int numSpawns = characterAllies.Count;
            float radius = Mathf.Max(30f, (numSpawns + 1) * 3f);
            Vector3 center = getTransform(master.GetBody()).position;

            // Disable Swarm if enabled, as that spawns multiple enemies, which we don't want
            bool swarmsWasEnabled = false;
            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef))
            {
                RunArtifactManager.instance.SetArtifactEnabledServer(RoR2Content.Artifacts.swarmsArtifactDef, false);
                swarmsWasEnabled = true;
            }
            try
            {
                for (int i = 0; i < numSpawns; ++i)
                {
                    // Add one more to make a slot for skipping Ally selection
                    float angle = i * (360f / (numSpawns + 1));
                    GameObject barrel = SpawnBarrel(center, angle, radius).spawnedInstance;
                    BarrelInteraction interaction = barrel.GetComponent<BarrelInteraction>();
                    interaction.goldReward = 0;
                    interaction.expReward = 0;
                    GameObject allyDummy = SpawnAllyDummy(characterAllies[i], center, angle, radius + 5f).spawnedInstance;
                    ApplyDummyAI(allyDummy.GetComponent<BaseAI>(), master.GetBodyObject());
                    SetGodMode(allyDummy.GetComponent<CharacterMaster>(), true);
                    ApplyPurchaseLogic(barrel, allyDummy);
                }

                // Add barrel to skip ally interaction
                float noAllyAngle = numSpawns * (360f / (numSpawns + 1));
                GameObject noAllybarrel = SpawnBarrel(center, noAllyAngle, radius).spawnedInstance;
                BarrelInteraction noAllyInteraction = noAllybarrel.GetComponent<BarrelInteraction>();
                noAllyInteraction.goldReward = 0;
                noAllyInteraction.expReward = 0;
                GameObject portal = SpawnPortal(center, noAllyAngle, radius + 5f).spawnedInstance;
                GenericInteraction portalInteraction = portal.GetComponent<GenericInteraction>();
                portalInteraction.SetInteractabilityDisabled();
                portalInteraction.onActivation.RemoveAllListeners();
                //portalInteraction.onActivation.AddListener((interactor) =>
                //{
                //    portalInteraction.SetInteractabilityDisabled();
                //    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                //    {
                //        baseToken = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>Protect the VIP:</color> " +
                //            $"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>Starting run with no ally.</color>"
                //    });
                //    GoToNextLevel();
                //});
                ApplyPurchaseLogic(noAllybarrel, null);
            }
            finally
            {
                if (swarmsWasEnabled)
                {
                    RunArtifactManager.instance.SetArtifactEnabledServer(RoR2Content.Artifacts.swarmsArtifactDef, true);
                }
            }
        }

        private SpawnCard.SpawnResult SpawnPortal(Vector3 center, float angle, float radius = 30f)
        {
            InteractableSpawnCard spawnCard = Resources.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscMSPortal");
            spawnCard.slightlyRandomizeOrientation = false;
            spawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;
            DirectorPlacementRule rule = new DirectorPlacementRule()
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct
            };
            DirectorSpawnRequest request = new DirectorSpawnRequest(spawnCard, rule, run.runRNG);
            Vector3 spawnPosition = FindGroundPosition(center, angle, radius);
            // FIXME: Rotation doesn't seem to work?
            return spawnCard.DoSpawn(spawnPosition, Quaternion.LookRotation((center - spawnPosition).normalized), request);
        }

        private SpawnCard.SpawnResult SpawnBarrel(Vector3 center, float angle, float radius = 30f)
        {
            InteractableSpawnCard spawnCard = Resources.Load<InteractableSpawnCard>("SpawnCards/InteractableSpawnCard/iscBarrel1");
            spawnCard.slightlyRandomizeOrientation = false;
            spawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;
            DirectorPlacementRule rule = new DirectorPlacementRule()
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct
            };
            DirectorSpawnRequest request = new DirectorSpawnRequest(spawnCard, rule, run.runRNG);
            Vector3 spawnPosition = FindGroundPosition(center, angle, radius);
            return spawnCard.DoSpawn(spawnPosition, Quaternion.LookRotation((center - spawnPosition).normalized), request);
        }

        private SpawnCard.SpawnResult SpawnAllyDummy(CharacterSpawnCard spawnCard, Vector3 center, float angle, float radius = 35f)//, GameObject summonerBody = null)
        {
            spawnCard.noElites = true;
            DirectorPlacementRule rule = new DirectorPlacementRule()
            {
                placementMode = DirectorPlacementRule.PlacementMode.Direct
            };
            DirectorSpawnRequest request = new DirectorSpawnRequest(spawnCard, rule, run.runRNG)
            {
                teamIndexOverride = TeamIndex.Neutral,
                ignoreTeamMemberLimit = true,
                //summonerBodyObject = summonerBody,
            };
            Vector3 spawnPosition = FindGroundPosition(center, angle, radius);
            return spawnCard.DoSpawn(spawnPosition, Quaternion.LookRotation((center - spawnPosition).normalized), request);
        }

        private Vector3 FindGroundPosition(Vector3 center, float angle, float radius)
        {
            Vector3 position = new Vector3(
                    center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad),
                    center.y + 10f,
                    center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad));
            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.up, out hit, 30f, LayerIndex.world.mask))
            {
                position = hit.point;
            }
            else if (Physics.Raycast(position, Vector3.down, out hit, 30f, LayerIndex.world.mask))
            {
                position = hit.point;
            }
            return position;
        }

        private void SetGodMode(CharacterMaster master, bool enabled)
        {
            FieldInfo godMode = typeof(CharacterMaster).GetField("godMode", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo UpdateBodyGodMode = typeof(CharacterMaster).GetMethod("UpdateBodyGodMode", BindingFlags.Instance | BindingFlags.NonPublic);

            godMode.SetValue(master, enabled);
            UpdateBodyGodMode.Invoke(master, new object[0]);
        }

        private void ApplyDummyAI(BaseAI baseAI, GameObject leader)
        {
            baseAI.leader.gameObject = leader;
            baseAI.neverRetaliateFriendlies = true;

            AISkillDriver waitNearLeaderDefault = baseAI.gameObject.AddComponent<AISkillDriver>();
            waitNearLeaderDefault.customName = "WaitNearLeaderDefault";
            waitNearLeaderDefault.skillSlot = SkillSlot.None;
            waitNearLeaderDefault.requiredSkill = null;
            waitNearLeaderDefault.requireSkillReady = false;
            waitNearLeaderDefault.requireEquipmentReady = false;
            waitNearLeaderDefault.minUserHealthFraction = float.NegativeInfinity;
            waitNearLeaderDefault.maxUserHealthFraction = float.PositiveInfinity;
            waitNearLeaderDefault.minTargetHealthFraction = float.NegativeInfinity;
            waitNearLeaderDefault.maxTargetHealthFraction = float.PositiveInfinity;
            waitNearLeaderDefault.minDistance = 0f;
            waitNearLeaderDefault.maxDistance = float.PositiveInfinity;
            waitNearLeaderDefault.selectionRequiresTargetLoS = false;
            waitNearLeaderDefault.selectionRequiresOnGround = false;
            waitNearLeaderDefault.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            waitNearLeaderDefault.activationRequiresTargetLoS = false;
            waitNearLeaderDefault.activationRequiresAimConfirmation = false;
            waitNearLeaderDefault.movementType = AISkillDriver.MovementType.Stop;
            waitNearLeaderDefault.moveInputScale = 1f;
            waitNearLeaderDefault.aimType = AISkillDriver.AimType.AtCurrentLeader;
            waitNearLeaderDefault.ignoreNodeGraph = true;
            waitNearLeaderDefault.shouldSprint = false;
            waitNearLeaderDefault.shouldFireEquipment = false;
            waitNearLeaderDefault.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            waitNearLeaderDefault.driverUpdateTimerOverride = -1f;
            waitNearLeaderDefault.resetCurrentEnemyOnNextDriverSelection = false;
            waitNearLeaderDefault.noRepeat = false;
            waitNearLeaderDefault.nextHighPriorityOverride = null;

            PropertyInfo skillDriversProperty = typeof(BaseAI).GetProperty("skillDrivers", BindingFlags.Instance | BindingFlags.Public);
            if (skillDriversProperty != null)
            {
                skillDriversProperty.SetValue(baseAI, new AISkillDriver[] { waitNearLeaderDefault }, null);
            }

            typeof(BaseAI)
                .GetField("skillDriverUpdateTimer", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(baseAI, 0f);
            typeof(BaseAI)
                .GetField("targetRefreshTimer", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(baseAI, 0f);
        }

        private void ApplyNormalAI(BaseAI baseAI, GameObject leader)
        {
            baseAI.leader.gameObject = leader;
            baseAI.neverRetaliateFriendlies = true;

            AISkillDriver returnToOwnerLeash = baseAI.gameObject.AddComponent<AISkillDriver>();
            returnToOwnerLeash.customName = "ReturnToOwnerLeash";
            returnToOwnerLeash.skillSlot = SkillSlot.None;
            returnToOwnerLeash.requiredSkill = null;
            returnToOwnerLeash.requireSkillReady = false;
            returnToOwnerLeash.requireEquipmentReady = false;
            returnToOwnerLeash.minUserHealthFraction = float.NegativeInfinity;
            returnToOwnerLeash.maxUserHealthFraction = float.PositiveInfinity;
            returnToOwnerLeash.minTargetHealthFraction = float.NegativeInfinity;
            returnToOwnerLeash.maxTargetHealthFraction = float.PositiveInfinity;
            returnToOwnerLeash.minDistance = 50f;
            returnToOwnerLeash.maxDistance = float.PositiveInfinity;
            returnToOwnerLeash.selectionRequiresTargetLoS = false;
            returnToOwnerLeash.selectionRequiresOnGround = false;
            returnToOwnerLeash.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            returnToOwnerLeash.activationRequiresTargetLoS = false;
            returnToOwnerLeash.activationRequiresAimConfirmation = false;
            returnToOwnerLeash.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            returnToOwnerLeash.moveInputScale = 1f;
            returnToOwnerLeash.aimType = AISkillDriver.AimType.AtCurrentLeader;
            returnToOwnerLeash.ignoreNodeGraph = false;
            returnToOwnerLeash.shouldSprint = true;
            returnToOwnerLeash.shouldFireEquipment = false;
            returnToOwnerLeash.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            returnToOwnerLeash.driverUpdateTimerOverride = 3f;
            returnToOwnerLeash.resetCurrentEnemyOnNextDriverSelection = true;
            returnToOwnerLeash.noRepeat = false;
            returnToOwnerLeash.nextHighPriorityOverride = null;

            AISkillDriver returnToLeaderDefault = baseAI.gameObject.AddComponent<AISkillDriver>();
            returnToLeaderDefault.customName = "ReturnToLeaderDefault";
            returnToLeaderDefault.skillSlot = SkillSlot.None;
            returnToLeaderDefault.requiredSkill = null;
            returnToLeaderDefault.requireSkillReady = false;
            returnToLeaderDefault.requireEquipmentReady = false;
            returnToLeaderDefault.minUserHealthFraction = float.NegativeInfinity;
            returnToLeaderDefault.maxUserHealthFraction = float.PositiveInfinity;
            returnToLeaderDefault.minTargetHealthFraction = float.NegativeInfinity;
            returnToLeaderDefault.maxTargetHealthFraction = float.PositiveInfinity;
            returnToLeaderDefault.minDistance = 15f;
            returnToLeaderDefault.maxDistance = float.PositiveInfinity;
            returnToLeaderDefault.selectionRequiresTargetLoS = false;
            returnToLeaderDefault.selectionRequiresOnGround = false;
            returnToLeaderDefault.moveTargetType = AISkillDriver.TargetType.CurrentLeader;
            returnToLeaderDefault.activationRequiresTargetLoS = false;
            returnToLeaderDefault.activationRequiresAimConfirmation = false;
            returnToLeaderDefault.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            returnToLeaderDefault.moveInputScale = 1f;
            returnToLeaderDefault.aimType = AISkillDriver.AimType.AtMoveTarget;
            returnToLeaderDefault.ignoreNodeGraph = false;
            returnToLeaderDefault.shouldSprint = true;
            returnToLeaderDefault.shouldFireEquipment = false;
            returnToLeaderDefault.buttonPressType = AISkillDriver.ButtonPressType.Hold;
            returnToLeaderDefault.driverUpdateTimerOverride = -1f;
            returnToLeaderDefault.resetCurrentEnemyOnNextDriverSelection = false;
            returnToLeaderDefault.noRepeat = false;
            returnToLeaderDefault.nextHighPriorityOverride = null;

            PropertyInfo skillDriversProperty = typeof(BaseAI).GetProperty("skillDrivers", BindingFlags.Instance | BindingFlags.Public);
            if (skillDriversProperty != null)
            {
                skillDriversProperty.SetValue(baseAI, baseAI.gameObject.GetComponents<AISkillDriver>(), null);
            }

            baseAI.currentEnemy.Reset();
            typeof(BaseAI)
                .GetField("skillDriverUpdateTimer", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(baseAI, 0f);
            typeof(BaseAI)
                .GetField("targetRefreshTimer", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(baseAI, 0f);
        }

        private void ApplyPurchaseLogic(GameObject barrel, GameObject allyDummy)
        {
            spawnedBarrels.Add(barrel);

            BarrelInteraction interaction = barrel.GetComponent<BarrelInteraction>();
            interaction.expReward = 0;
            interaction.goldReward = 0;

            AllyDetails details = interaction.gameObject.AddComponent<AllyDetails>();
            details.ally = allyDummy;
        }

        private void GiveHelpingHand(CharacterMaster master)
        {
            master.inventory.GiveItem(RoR2Content.Items.BoostHp, 10);
            master.inventory.GiveItem(RoR2Content.Items.BoostDamage, 4);
            master.inventory.GiveItem(RoR2Content.Items.HealWhileSafe, 2);
            master.inventory.GiveItem(RoR2Content.Items.Medkit, 2);
        }

        private void BarrelInteraction_OnInteractionBegin(On.RoR2.BarrelInteraction.orig_OnInteractionBegin orig, BarrelInteraction self, Interactor activator)
        {
            if (!NetworkServer.active)
            {
                orig(self, activator);
                return;
            }

            if (IsOnVIPSelectionStage)
            {
                if (self.Networkopened)
                {
                    orig(self, activator);
                    return;
                }

                foreach (var barrel in spawnedBarrels)
                {
                    BarrelInteraction interaction = barrel.GetComponent<BarrelInteraction>();
                    if (self != interaction)
                    {
                        interaction.Networkopened = true;
                    }
                }

                AllyDetails details = self.GetComponent<AllyDetails>();
                if (details && details.ally)
                {
                    CharacterMaster summonCharacterMaster = activator.GetComponent<CharacterBody>().master;
                    CharacterMaster allyCharacterMaster = details.ally.GetComponent<CharacterMaster>();

                    AllyMaster allyMaster = allyCharacterMaster.gameObject.AddComponent<AllyMaster>();
                    allyMaster.NameToken = "VIP";

                    GiveHelpingHand(allyCharacterMaster);
                    ApplyNormalAI(
                        details.ally.GetComponent<BaseAI>(),
                        summonCharacterMaster.GetBodyObject());

                    allies.Add(allyCharacterMaster);
                    allySquad.AddMember(allyCharacterMaster);

                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                    {
                        baseToken = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>Protect the VIP:</color> " +
                            $"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>Protect the ally at all costs.</color> " +
                            $"<color=#{ColorUtility.ToHtmlStringRGB(Color.red)}>If it dies, you die.</color>"
                    });
                }
                else if (details && !details.ally)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
                    {
                        baseToken = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>Protect the VIP:</color> " +
                            $"<color=#{ColorUtility.ToHtmlStringRGB(Color.gray)}>Starting run with no ally.</color>"
                    });
                }
                else
                {
                    Debug.LogError("No details attached to barrel :( Starting normal run...");
                }

                GoToNextLevel();

                Debug.Log("Unhooking from RoR2.BarrelInteraction.OnInteractionBegin");
                On.RoR2.BarrelInteraction.OnInteractionBegin -= BarrelInteraction_OnInteractionBegin;
            }

            orig(self, activator);
        }

        private void GoToNextLevel()
        {
            if (NetworkServer.active && !SceneExitController.isRunning)
            {
                SceneCollection startingScenes = initialStartingScenes;
                if (startingScenes == null || startingScenes.isEmpty)
                {
                    startingScenes = ScriptableObject.CreateInstance<SceneCollection>();
                    startingScenes.SetSceneEntries(new SceneCollection.SceneEntry[] {
                        new SceneCollection.SceneEntry {
                            sceneDef = SceneCatalog.GetSceneDefFromSceneName("blackbeach"),
                            weight = 1,
                        },
                        new SceneCollection.SceneEntry {
                            sceneDef = SceneCatalog.GetSceneDefFromSceneName("golemplains"),
                            weight = 1,
                        },
                    });
                }

                WeightedSelection<SceneDef> sceneDefs = new WeightedSelection<SceneDef>();
                startingScenes.AddToWeightedSelection(sceneDefs);
                run.PickNextStageScene(sceneDefs);
                SceneExitController exitController = gameObject.AddComponent<SceneExitController>();
                exitController.useRunNextStageScene = true;
                Debug.Log("Advancing to next stage...");
                exitController.Begin();
            }
        }

        private void Run_OnServerBossAdded(On.RoR2.Run.orig_OnServerBossAdded orig, Run self, BossGroup bossGroup, CharacterMaster characterMaster)
        {
            OnServerBossAdded(bossGroup, characterMaster);
            orig(self, bossGroup, characterMaster);
        }

        public void OnServerBossAdded(BossGroup _, CharacterMaster characterMaster)
        {
            if (IsOnVIPSelectionStage)
            {
                // FIXME: Need to kill BossGroup too
                Debug.Log("Killing boss in VIP selection stage");
                //BaseAI baseAI = characterMaster.gameObject.GetComponent<BaseAI>();
                //if (baseAI)
                //{
                //    Destroy(baseAI);
                //}
                characterMaster.DestroyBody();
                Destroy(characterMaster);
            }
        }

        private Transform getTransform(CharacterBody body)
        {
            FieldInfo transformField = typeof(CharacterBody).GetField("transform", BindingFlags.Instance | BindingFlags.NonPublic);
            return (Transform)transformField.GetValue(body);
        }
    }
}
