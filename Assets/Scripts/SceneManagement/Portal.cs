using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Control;
using UI.HUD;
using Utils;
using Stats;
using Attributes;
using Quests;
using Audio;

namespace SceneManagement
{
    public class Portal : MonoBehaviour, IRaycastable
    {
        // CONFIG

        [SerializeField] int sceneToLoad = -1;
        [SerializeField] Transform leaderSpawnPoint;
        [SerializeField] Transform[] companionSpawnPoints;
        [SerializeField] Transform portal;
        [SerializeField] Address destination;
        [SerializeField] float transitionTime = 1f;
        [SerializeField] int interactCost = 1;
        [SerializeField] float interactRange = 2;
        [SerializeField] HintDisplay hintDisplay;
        [SerializeField] string hintText = "";

        enum Address { A, B, C, D, E, F, G, H }

        // CACHE

        SavingWrapper savingWrapper = null;
        AudioHub audioHub = null;

        // LIFECYCLE

        void Start()
        {
            hintDisplay.SetText(hintText);
            savingWrapper = FindObjectOfType<SavingWrapper>();
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PUBLIC

        public bool HandleRaycast(PlayerController callingController, bool actionAvailable)
        {
            EnableHintDisplay();
            if (actionAvailable && Input.GetMouseButtonDown(0))
            {
                callingController.SetupInteraction(this);
            }
            return true;
        }

        public void Interact(PlayerController callingController)
        {
            StartCoroutine(Transition());
        }

        public Vector3 GetPosition()
        {
            return portal.position;
        }

        public CursorType GetCursorType(PlayerController callingController)
        {
            return CursorType.Door;
        }

        public int GetInteractCost(PlayerController callingController)
        {
            return callingController.IsTakingTurn() ? interactCost : 0;
        }

        public float GetInteractRange(PlayerController callingController)
        {
            return interactRange;
        }

        public void EnableStatusBar()
        {
            
        }

        public void SetupStatusBar()
        {

        }

        public void EnableHintDisplay()
        {
            hintDisplay.EnableHint();
        }

        public void EnableObstacle(bool isEnable)
        {

        }

        public void EnableBattleMarker(bool isEnable, bool isSelfTarget = false)
        {
            
        }

        // PRIVATE

        IEnumerator Transition()
        {
            DontDestroyOnLoad(gameObject);

            audioHub.PlayDoor(true);

            Cursor.visible = false;

            DisablePlayerControls();

            if (sceneToLoad == 0)
            {
                savingWrapper.Save();
                savingWrapper.LoadMainMenu();
                yield break;
            }

            Fader fader = FindObjectOfType<Fader>();
            yield return fader.FadeOut();

            savingWrapper.Save();

            yield return SceneManager.LoadSceneAsync(sceneToLoad);

            DisablePlayerControls();

            savingWrapper.Load();

            DisablePlayerControls();

            Portal newPortal = GetOtherPortal();

            UpdatePlayerPositions(newPortal);

            savingWrapper.Save();

            yield return new WaitForSeconds(transitionTime);

            RemoveDeadInOtherScene();

            audioHub.PlayDoor(false);

            if (FindObjectOfType<Explorer>().GainExperience(sceneToLoad))
            {
                QuestCompletion questCompletion = newPortal.GetComponent<QuestCompletion>();
                if (questCompletion != null) questCompletion.CompleteObjective();

                yield return fader.FadeIn(true);
            }
            else
            {
                yield return fader.FadeIn();
            }

            savingWrapper.Save();

            Cursor.visible = true;

            Destroy(gameObject);
        }

        Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue;
                if (portal.destination != this.destination) continue;
                return portal;
            }
            return null;
        }

        void UpdatePlayerPositions(Portal otherPortal)
        {
            GameObject players = GameObject.FindWithTag("Player");
            PlayerController leader = players.GetComponentInParent<ControlSwitcher>().GetActivePlayer();

            foreach (Transform player in players.transform)
            {
                if (player.GetComponent<Health>().IsDead()) continue;

                player.GetComponent<NavMeshAgent>().enabled = false;

                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController == leader)
                {
                    playerController.transform.position = otherPortal.leaderSpawnPoint.position;
                    playerController.transform.rotation = otherPortal.leaderSpawnPoint.rotation;
                }
                else
                {
                    int companionIndex = playerController.GetCompanionIndex();
                    playerController.transform.position = otherPortal.companionSpawnPoints[companionIndex].position;
                    playerController.transform.rotation = otherPortal.companionSpawnPoints[companionIndex].rotation;
                }

                player.GetComponent<NavMeshAgent>().enabled = true;
            }
        }

        void DisablePlayerControls()
        {
            GameObject players = GameObject.FindWithTag("Player");
            
            foreach (Transform player in players.transform)
            {
                player.GetComponent<PlayerController>().EnableControl(false);
            }
        }

        void RemoveDeadInOtherScene()
        {
            GameObject players = GameObject.FindWithTag("Player");
            foreach (Transform player in players.transform)
            {
                Health health = player.GetComponent<Health>();
                int deathSceneIndex = health.GetDeathSceneIndex();
                int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

                if (health.IsDead() && deathSceneIndex != currentSceneIndex)
                {
                    player.gameObject.SetActive(false);
                }
            }
        }

        public BaseStats GetBaseStats()
        {
            return null;
        }

        public Transform GetTransform()
        {
            return transform;
        }
    }
}