using RoR2;
using RoR2.Stats;
using System.Reflection;
using UnityEngine;

namespace ProtectTheVIP
{
    class AllyMaster : MonoBehaviour
    {
        public string NameToken { get; set; }

        private CharacterMaster master;

        public void Awake()
        {
            master = GetComponent<CharacterMaster>();
            if (master)
            {
                // Flip the Ally to the Player Team
                master.teamIndex = TeamIndex.Player;
                TeamComponent teamComponent = master.GetBodyObject().GetComponent<TeamComponent>();
                teamComponent.teamIndex = TeamIndex.Player;

                master.gameObject.AddComponent<SetDontDestroyOnLoad>();

                // This causes the Ally to not spawn in the next stage
                //if (master.gameObject.GetComponent<PlayerCharacterMasterController>() == null)
                //{
                //    Debug.LogWarning("Adding PlayerCharacterMasterController to ally");
                //    master.gameObject.AddComponent<PlayerCharacterMasterController>();
                //}

                //if (master.gameObject.GetComponent<PlayerStatsComponent>() == null)
                //{
                //    Debug.LogWarning("Adding PlayerStatsComponent to ally");
                //    master.gameObject.AddComponent<PlayerStatsComponent>();
                //}

                ForceNameplate();
            }
        }

        public void LateUpdate()
        {
            if (master)
            {
                ForceNameplate();
            }
        }

        private void ForceNameplate()
        {
            // This forces a character nameplate on the ally - as well as other things :)
            CharacterBody body = master.GetBody();
            if (body && !string.IsNullOrWhiteSpace(NameToken) && !body.baseNameToken.Equals(NameToken))
            {
                body.baseNameToken = NameToken;
            }

            if (body?.isPlayerControlled == false)
            {
                Debug.LogWarning($"Forcing ally {body.GetDisplayName()} to have player nameplate.");
                typeof(CharacterBody)
                    .GetProperty("isPlayerControlled", BindingFlags.Instance | BindingFlags.Public)
                    .SetValue(body, true);

                TeamComponent team = master.GetBodyObject().GetComponent<TeamComponent>();
                if (!team)
                {
                    Debug.LogError("Could not find TeamComponent!");
                    return;
                }
                FieldInfo indicatorField = typeof(TeamComponent)
                    .GetField("indicator", BindingFlags.Instance | BindingFlags.NonPublic);
                GameObject indicator = (GameObject)indicatorField.GetValue(team);
                if (indicator)
                {
                    Destroy(indicator);
                    indicatorField.SetValue(team, null);
                }
                typeof(TeamComponent)
                    .GetMethod("SetupIndicator", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(team, new object[0]);
            }
        }
    }
}
