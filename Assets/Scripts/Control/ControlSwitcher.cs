using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Core;
using Saving;
using Movement;
using UI;
using UI.HUD;
using UI.Ingame;
using Animations;
using Attributes;
using Utils;
using Combat;
using Audio;

namespace Control
{
    public class ControlSwitcher : MonoBehaviour, ISaveable
    {
        // CONFIG

        [SerializeField] PlayerController[] players;
        [SerializeField] Health tank;
        [SerializeField] Transform leaderPosition;
        [SerializeField] Transform[] companionPositions;
        [SerializeField] FollowCamera followCamera;
        [SerializeField] float deathDelay = 3f;

        [SerializeField] Button unsheathButton;
        [SerializeField] KeyCode unsheathKey = KeyCode.C;

        [SerializeField] float lookAroundRadius = 15f;
        [SerializeField] KeyCode lookAroundKey = KeyCode.Space;
        [SerializeField] Button lookAroundButton;
        [SerializeField] ButtonOnHold buttonHold;

        [SerializeField] FightScheduler fightScheduler;
        [SerializeField] ShowHideUI showHideUI;
        [SerializeField] TargetMarker targetMarker;

        // CACHE

        AudioHub audioHub = null;

        // STATE

        public event Action onControlChange;
        public event Action onPartyDeath;
        int currentPlayerIndex = 0;
        bool targetSystemEnabled = true;
        bool shouldUpdateLeaderPosition = true;

        // LIFECYCLE

        void Start()
        {
            audioHub = FindObjectOfType<AudioHub>();

            foreach (PlayerController player in players)
            {
                player.EnableControl(false);
            }

            followCamera.SetTarget(players[currentPlayerIndex].transform);

            buttonHold.SetOnHold(LookAround, audioHub.PlayClick);

            fightScheduler.onFightStart += DisableUnsheathButton;
            fightScheduler.onFightFinish += EnableUnsheathButton;

            foreach (PlayerController playerController in players)
            {
                playerController.GetComponent<Health>().onDeath += CheckPartyDeath;
                playerController.GetComponent<Mover>().SetLeaderControlEnable(ReturnCompanionRole);
                playerController.GetComponent<Fighter>().SetLeaderControlEnable(ReturnCompanionRole);
                playerController.GetComponent<Interactor>().SetLeaderControlEnable(ReturnCompanionRole);
            }
        }

        void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Backspace)) SwitchPlayerWithDelay(); // TESTING PURPOSE

            if (shouldUpdateLeaderPosition)
            {
                leaderPosition.position = players[currentPlayerIndex].transform.position;
                leaderPosition.forward = players[currentPlayerIndex].transform.forward;
            }

            if (!targetSystemEnabled)
            {
                targetMarker.DisablePathLine();
                targetMarker.SetTargetMarker(MarkerType.None);
            }

            if (Input.GetKeyDown(unsheathKey) && !fightScheduler.IsFightRunning())
            {
                players[currentPlayerIndex].GetComponent<AnimationController>().ToggleUnsheath();
            }

            if (Input.GetKey(lookAroundKey)) LookAround();
            if (Input.GetKeyDown(lookAroundKey)) audioHub.PlayClick();
        }

        // PUBLIC

        public AudioHub GetAudioHub()
        {
            return audioHub;
        }

        public void ReturnCompanionRole()
        {
            if (shouldUpdateLeaderPosition) return;

            EnableLeaderPositionUpdate();

            for (int i = 0; i < players.Length; i++)
            {
                players[i].GetComponent<Mover>().EnableCompanionMode(true);
            }
        }

        public void EnableLeaderPositionUpdate()
        {
            shouldUpdateLeaderPosition = true;
        }

        public void DisableLeaderPositionUpdate()
        {
            shouldUpdateLeaderPosition = false;
        }

        public void UpdateControl(PlayerController playerController)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (playerController == players[i])
                {
                    currentPlayerIndex = i;
                    break;
                }
            }

            if (onControlChange != null)
            {
                onControlChange();
            }
        }

        public void ReturnControlToLeader()
        {
            SwitchPlayer(currentPlayerIndex);
            ReturnCompanionRole();
        }

        public PlayerController[] GetPlayers()
        {
            return players;
        }

        public Health GetTank()
        {
            return tank;
        }

        public PlayerController GetActivePlayer()
        {
            return players[currentPlayerIndex];
        }

        public int GetActivePlayerIndex()
        {
            return currentPlayerIndex;
        }

        public void EnableTargetSystem(bool isEnabled)
        {
            targetSystemEnabled = isEnabled;
        }

        public void LookAround()
        {
            foreach (RaycastHit hit in Physics.SphereCastAll(GetActivePlayer().transform.position, lookAroundRadius, Vector3.up))
            {
                IRaycastable raycastable = hit.transform.GetComponent<IRaycastable>();

                if (raycastable == null) continue;

                VanishingObject vanishingObject = raycastable.GetTransform().GetComponent<VanishingObject>();

                if (vanishingObject != null && vanishingObject.IsVanished()) continue;
                
                raycastable.EnableHintDisplay();
            }
        }

        public void SwitchPlayerButton(int index)
        {
            audioHub.PlayClick();

            PlayerController playerController = GetActivePlayer();

            if (!playerController.enabled && !playerController.GetComponent<Health>().IsDead()) return;

            leaderPosition.position = players[index].transform.position;

            playerController.StopAllActions();
            SwitchPlayer(index);
        }

        public void SwitchPlayerWithDelay()
        {
            SwitchPlayer(currentPlayerIndex);
        }

        // PRIVATE

        void SwitchPlayer(int index)
        {
            if (DeadInOtherScene(index)) return;
            if (fightScheduler.IsFightRunning()) return;
            if (showHideUI.IsSinglePlayerActionRunning()) return;

            showHideUI.CloseContainer();

            players[currentPlayerIndex].DisableTargetMarker();
            
            DisableLeaderPositionUpdate();
            foreach (PlayerController player in players)
            {
                player.GetComponent<Mover>().EnableCompanionMode(false);
            }

            SetupNewPlayer(index);
            int companionIndex = 0;
            for (int i = 0; i < players.Length; i++)
            {
                if (i != currentPlayerIndex)
                {
                    players[i].SetupCompanionRole(companionPositions[companionIndex], companionIndex);
                    companionIndex++;
                }
            }

            if (onControlChange != null)
            {
                onControlChange();
            }
        }

        void SetupNewPlayer(int index)
        {
            currentPlayerIndex = index;
            players[index].EnableControl(true);
            players[index].SetupCompanionRole(null, -1);
            followCamera.ResetCameraTarget();
            followCamera.SetTarget(players[index].transform);
            unsheathButton.onClick.RemoveAllListeners();
            unsheathButton.onClick.AddListener(players[index].GetComponent<AnimationController>().ToggleUnsheath);
        }

        void CheckPartyDeath()
        {
            foreach (PlayerController playerController in players)
            {
                if (!playerController.GetComponent<Health>().IsDead())
                {
                    return;
                }
            }

            StartCoroutine(DeathRoutine());
        }

        IEnumerator DeathRoutine()
        {
            yield return new WaitForSeconds(deathDelay);
            if (onPartyDeath != null)
            {
                onPartyDeath();
            }
        }

        bool DeadInOtherScene(int index)
        {
            Health health = players[index].GetHealth();
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int deathSceneIndex = health.GetDeathSceneIndex();

            return health.IsDead() && currentSceneIndex != deathSceneIndex;
        }

        void EnableUnsheathButton()
        {
            unsheathButton.gameObject.SetActive(true);
        }

        void DisableUnsheathButton()
        {
            unsheathButton.gameObject.SetActive(false);
        }

        object ISaveable.CaptureState()
        {
            return currentPlayerIndex;
        }

        void ISaveable.RestoreState(object state)
        {
            currentPlayerIndex = (int)state;
        }
    }
}