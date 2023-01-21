using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Control;
using Utils;
using UI.Ingame;
using Stats;

namespace Abilities
{
    [CreateAssetMenu(fileName = "Pointing Targeting", menuName = "Abilities/Targeting/Pointing", order = 0)]
    public class PointingTargeting : TargetingStrategy
    {
        // CONFIG

        [SerializeField] CursorType cursorType = CursorType.Air;
        [SerializeField] LayerMask layerMask;
        [SerializeField] FilterStrategy[] filters;
        [SerializeField] bool selfTargetAvailable = true;

        // STATE

        static int vanishedLayerNumber = 8;

        // PUBLIC

        public override void StartTargeting(AbilityData data, Action finished)
        {
            PlayerController playerController = data.GetUser().GetComponent<PlayerController>();
            playerController.SetCursor(cursorType, true);
            playerController.SetTargeting(true);
            playerController.DisableAPDisplay();
            playerController.StartCoroutine(Targeting(data, playerController, finished));
        }

        // PRIVATE

        IEnumerator Targeting(AbilityData data, PlayerController playerController, Action finished)
        {
            bool leftActionBar = false;
            bool targetAquired = false;

            FightScheduler fightScheduler = FindObjectOfType<FightScheduler>();
            TargetMarker targetMarker = FindObjectOfType<TargetMarker>();
            IRaycastable currentRaycastable = null;
            IRaycastable currentUser = data.GetUser().GetComponent<IRaycastable>();

            while (!data.IsCancelled() && !targetAquired)
            {
                playerController.SetAccuracyDisplay(null, null);
                playerController.SetActionPointsDisplay(false, 0, 0, "");

                if (currentRaycastable != null && fightScheduler.IsFightRunning())
                {
                    currentRaycastable.EnableBattleMarker(true, object.ReferenceEquals(currentRaycastable, currentUser));
                }
                targetMarker.SetTargetMarker(MarkerType.None);

                if (Input.GetKeyDown(KeyCode.Escape) || (leftActionBar && EventSystem.current.IsPointerOverGameObject()))
                {
                    playerController.EnableControl(true);
                    playerController.EnableTargetSystem(true);
                    playerController.SetTargeting(false);
                    data.CancelAction();
                    break;
                }
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    leftActionBar = true;
                }

                bool foundRaycastable = false;
                bool obstacleFound = false;

                foreach (RaycastHit hit in PlayerController.RaycastAllSorted())
                {
                    if (hit.transform.GetComponent<IRaycastable>() == null && hit.transform.gameObject.layer != vanishedLayerNumber)
                    {
                        obstacleFound = true;
                    }

                    foreach (IRaycastable raycastable in hit.transform.GetComponents<IRaycastable>())
                    {
                        if (foundRaycastable || obstacleFound) continue;

                        if (currentRaycastable != null && fightScheduler.IsFightRunning())
                        {
                            currentRaycastable.EnableBattleMarker(true, object.ReferenceEquals(currentUser, currentRaycastable));
                        }
                        currentRaycastable = raycastable;
                        targetMarker.SetTargetMarker(MarkerType.None);
                        targetMarker.SetPosition(raycastable.GetPosition());

                        foundRaycastable = true;
                        bool possible = true;

                        float distance = Vector3.Distance(playerController.transform.position, raycastable.GetPosition());

                        if (raycastable.GetBaseStats() == null)
                        {
                            raycastable.EnableHintDisplay();
                            playerController.DisableStatusBar();
                            playerController.SetCursor(cursorType, false);
                            playerController.SetActionPointsDisplay(false, distance, 0, "Wrong Target");
                        }
                        else
                        {
                            raycastable.EnableStatusBar();
                            raycastable.SetupStatusBar();

                            if (!IsTargetPossible(currentUser, raycastable))
                            {
                                playerController.SetCursor(cursorType, false);
                                possible = false;
                                playerController.SetActionPointsDisplay(false, distance, 0, "Wrong Target");

                                if (fightScheduler.IsFightRunning())
                                {
                                    raycastable.EnableBattleMarker(false);
                                    targetMarker.SetTargetMarker(MarkerType.NotAvailable);
                                }
                            }
                            else if (!Scanner.IsTargetVisible(data.GetUser(), raycastable.GetBaseStats().gameObject))
                            {
                                playerController.SetCursor(cursorType, false);
                                possible = false;
                                playerController.SetActionPointsDisplay(false, distance, 0, "Out of View");

                                if (fightScheduler.IsFightRunning())
                                {
                                    raycastable.EnableBattleMarker(false);
                                    targetMarker.SetTargetMarker(MarkerType.NotAvailable);
                                }
                            }
                            else if (distance > data.GetDistance())
                            {
                                playerController.SetCursor(cursorType, false);
                                possible = false;
                                playerController.SetActionPointsDisplay(false, distance, 0);

                                if (fightScheduler.IsFightRunning())
                                {
                                    raycastable.EnableBattleMarker(false);
                                    targetMarker.SetTargetMarker(MarkerType.NotAvailable);
                                }
                            }
                            else
                            {
                                playerController.SetCursor(cursorType, true);
                                possible = true;
                                playerController.SetActionPointsDisplay(true, distance, data.GetCost());

                                if (raycastable is CombatTarget)
                                {
                                    BaseStats casterStats = data.GetUser().GetComponent<BaseStats>();
                                    BaseStats targetStats = raycastable.GetBaseStats();
                                    playerController.SetAccuracyDisplay(casterStats, targetStats);
                                }

                                if (fightScheduler.IsFightRunning())
                                {
                                    raycastable.EnableBattleMarker(false);
                                    targetMarker.SetTargetMarker(MarkerType.Attack);
                                }
                            }
                        }

                        if (!EventSystem.current.IsPointerOverGameObject() && possible && Input.GetMouseButtonDown(0))
                        {
                            yield return new WaitUntil(() => Input.GetMouseButton(0));
                            data.SetTargetPoint(raycastable.GetPosition());
                            data.SetTargets(new GameObject[] { raycastable.GetBaseStats().gameObject });
                            targetAquired = true;
                            if (currentRaycastable != null && fightScheduler.IsFightRunning())
                            {
                                currentRaycastable.EnableBattleMarker(true, object.ReferenceEquals(currentUser, currentRaycastable));
                            }
                            targetMarker.SetTargetMarker(MarkerType.None);
                            break;
                        }
                        else if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
                        {
                            playerController.EnableControl(true);
                            playerController.EnableTargetSystem(true);
                            playerController.SetTargeting(false);
                            data.CancelAction();
                            if (currentRaycastable != null && fightScheduler.IsFightRunning())
                            {
                                currentRaycastable.EnableBattleMarker(true, object.ReferenceEquals(currentUser, currentRaycastable));
                            }
                            targetMarker.SetTargetMarker(MarkerType.None);
                            break;
                        }
                    }
                }

                if (!foundRaycastable)
                {
                    playerController.DisableStatusBar();

                    RaycastHit hit;
                    if (Physics.Raycast(PlayerController.GetMouseRay(), out hit, 1000, layerMask))
                    {
                        float distance = Vector3.Distance(playerController.transform.position, hit.point);

                        if (distance > data.GetDistance())
                        {
                            if (leftActionBar) playerController.SetCursor(cursorType, false);
                            playerController.SetActionPointsDisplay(false, distance, 0);
                        }
                        else
                        {
                            if (leftActionBar) playerController.SetCursor(cursorType, true);
                            playerController.SetActionPointsDisplay(true, distance, 0);
                        }

                        if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
                        {
                            playerController.EnableControl(true);
                            playerController.EnableTargetSystem(true);
                            playerController.SetTargeting(false);
                            data.CancelAction();
                            break;
                        }
                    }
                }

                yield return null;
            }

            playerController.SetTargeting(false);
            finished();
        }

        bool IsTargetPossible(IRaycastable user, IRaycastable raycastable)
        {            
            if (!selfTargetAvailable && object.ReferenceEquals(user, raycastable)) return false;
            if (filters.Length == 0) return true;

            foreach (FilterStrategy filter in filters)
            {
                if (filter.Filter(new GameObject[]{raycastable.GetBaseStats().gameObject}).ToList().Count == 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}