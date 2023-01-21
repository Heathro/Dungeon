using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Movement;
using Combat;
using Attributes;
using Stats;
using UI.Ingame;
using UI.HUD;
using Utils;
using Core;
using Saving;
using Inventories;
using Animations;
using Abilities;
using UI;
using Audio;

namespace Control
{
    public class PlayerController : MonoBehaviour, IController, ISaveable
    {
        // CONFIG
        
        [SerializeField] int startActionPoints = 4;
        [SerializeField] int maxActionPoints = 6;
        [SerializeField] float delayAfterTurn = 1f;
        [SerializeField] CursorMapping[] cursorMappings = null;
        [SerializeField] float navMeshHitTolerance = 1f;
        [SerializeField] float maxNavMeshPathLength = 40f;

        [SerializeField] TargetMarker targetMarker;
        [SerializeField] StatusBar statusBar;

        [System.Serializable]
        struct CursorMapping
        {
            public CursorType type;
            public Texture2D texture;
            public Vector2 hotspot;
        }

        // CACHE

        Fighter fighter;
        Mover mover;
        Health health;
        NavMeshAgent navMeshAgent;
        NavMeshObstacle navMeshObstacle;
        BattleMarker battleMarker;
        APDisplay actionPointsDisplay;
        BaseStats baseStats;
        FollowCamera followCamera;
        Interactor interactor;
        ActionStore actionStore;
        PauseHub pauseMenu;
        ControlSwitcher controlSwitcher;
        AnimationController animationController;
        BuffStore buffStore;
        BuffStoreHub buffStoreHub;
        ShowHideUI showHideUI;
        AudioController audioController;
        AudioHub audioHub;

        // STATE

        public event Action onActionPointsUsageUpdate;

        bool takingTurn = false;
        int actionPoints = 0;
        int savedPoints = 0;
        int delayedPoints = 0;
        int usingActionPoints = 0;
        IRaycastable currentRaycastable = null;
        float mileage = -1f;
        int companionIndex = -1;

        bool finalLooting = false;

        static int vanishedLayerNumber = 8;
        static int vanishedLayerMask = 1 << 8;

        // LIFECYCLE

        void Awake()
        {
            fighter = GetComponent<Fighter>();
            mover = GetComponent<Mover>();
            health = GetComponent<Health>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshObstacle = GetComponent<NavMeshObstacle>();
            battleMarker = GetComponentInChildren<BattleMarker>();
            baseStats = GetComponent<BaseStats>();
            interactor = GetComponent<Interactor>();
            actionStore = GetComponent<ActionStore>();
            animationController = GetComponent<AnimationController>();
            buffStore = GetComponent<BuffStore>();
            audioController = GetComponent<AudioController>();
        }

        void Start()
        {
            actionPointsDisplay = FindObjectOfType<APDisplay>();
            followCamera = FindObjectOfType<FollowCamera>();
            pauseMenu = FindObjectOfType<PauseHub>();
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            buffStoreHub = controlSwitcher.GetComponent<BuffStoreHub>();
            showHideUI = FindObjectOfType<ShowHideUI>();
            audioHub = FindObjectOfType<AudioHub>();

            fighter.onWeaponChange += DisableTargetMarker;
            fighter.onWeaponSwitchStart += (() => EnableControl(false));
            fighter.onWeaponSwitchEnd += (() => EnableControl(true));
        }

        void Update()
        {
            ResetActionPointsDisplay();
            CheckInputKeys();
            if (InteractWithUI()) return;
            if (IsPlayerDead()) return;
            if (IsDragging()) return;
            if (InteractWithComponent()) return;
            ResetRaycastable();
            if (InteractWithMovement()) return;
            ResetTargetMarker();
            SetDefaultCursor();
        }

        // PUBLIC

        public void SetupCompanionRole(Transform companionPosition, int companionIndex)
        {
            mover.SetCompanionPosition(companionPosition);
            this.companionIndex = companionIndex;
            EnableControl(companionIndex == -1);
        }

        public int GetCompanionIndex()
        {
            return companionIndex;
        }

        public void ResetSavedPoints()
        {
            savedPoints = 0;
        }

        public bool IsLooting()
        {
            return finalLooting;
        }

        public Coroutine TakeTurn()
        {
            takingTurn = true;
            finalLooting = false;
            actionPoints = GetCorrectPoints();
            SetActionPointsUsage(0);

            mileage = -1f;
            return StartCoroutine(TakingTurn());
        }

        public void EnableControl(bool isEnable)
        {
            enabled = isEnable;

            if (!isEnable)
            {
                DisableStatusBar();
                SetCursor(CursorType.MovementUp, true);
            }
        }

        public void EnableAgent(bool isEnable)
        {
            if (isEnable && navMeshObstacle.enabled) return;
            navMeshAgent.enabled = isEnable;
        }

        public void EnableObstacle(bool isEnable)
        {
            if (isEnable && navMeshAgent.enabled) return;
            navMeshObstacle.enabled = isEnable;
        }

        public bool IsEnemy()
        {
            return false;
        }

        public Health GetHealth()
        {
            return health;
        }

        public void EndTurn()
        {
            savedPoints = actionPoints;
            EnableControl(false);
            DisableTargetMarker();
            takingTurn = false;
        }

        public void DelayTurn()
        {
            delayedPoints = actionPoints;
            EndTurn();
        }

        public bool IsPointsOver()
        {
            return actionPoints == 0;
        }

        public bool IsTakingTurn()
        {
            return takingTurn;
        }

        public void StartAttack(CombatTarget target, bool friendlyAttack = false)
        {
            audioHub.PlayClick();

            if (takingTurn)
            {
                EnableControl(false);
                fighter.StartAttackAction(target.gameObject, actionPoints, UseActionPoints, ActionFinished, mileage, SetMileage);
            }
            else
            {
                DisableTargetMarker();
                animationController.ResetUnsheathTime();
                animationController.Unsheath(() => fighter.StartAttackAction(target.gameObject, friendlyAttack));
            }
        }

        public void SetupInteraction(IRaycastable target, bool isDialogue = false)
        {
            if (target == null)
            {
                interactor.CancelAction();
                return;
            }
            
            audioHub.PlayClick();

            if (takingTurn)
            {
                EnableControl(false);
                interactor.StartInteractAction(target, actionPoints, UseActionPoints, ActionFinished, mileage, SetMileage);
            }
            else
            {
                DisableTargetMarker();
                animationController.ResetUnsheathTime();
                interactor.StartInteractAction(target, isDialogue);
            }
        }

        public void SetBattleMarker(FighterType fighterType)
        {
            battleMarker.SetBattleMarker(fighterType);
        }

        public void DisableTargetMarker()
        {
            targetMarker.SetTargetMarker(MarkerType.None);
        }

        public void ResetTargetMarker()
        {
            targetMarker.DisablePathLine();
            if (!takingTurn) return;
            DisableTargetMarker();
        }

        public void HideOnDrag()
        {
            ResetTargetMarker();
            ResetActionPointsDisplay();
        }

        public void EnableTargetSystem(bool isEnable)
        {
            controlSwitcher.EnableTargetSystem(isEnable);
        }

        public void SetActionPointsDisplay(bool inRange, float distance, int points, string message = "Out of range")
        {
            SetActionPointsUsage(points);

            if (inRange)
            {
                actionPointsDisplay.SetPoints(false);
                actionPointsDisplay.SetDistance(true, distance);
                actionPointsDisplay.SetMessage(false);
            }
            else
            {
                actionPointsDisplay.SetPoints(false);
                actionPointsDisplay.SetDistance(true, distance);
                actionPointsDisplay.SetMessage(true, message, Color.red);
            }
        }

        public void SetActionPointsDisplay(float distance, string message)
        {
            actionPointsDisplay.SetAccuracy(false);
            actionPointsDisplay.SetPoints(false);
            actionPointsDisplay.SetDistance(true, distance);
            actionPointsDisplay.SetMessage(true, message, Color.yellow);
        }

        public void SetAccuracyDisplay(BaseStats casterStats, BaseStats targetStats)
        {
            if (casterStats == null || targetStats == null)
            {
                actionPointsDisplay.SetAccuracy(false);
            }
            else if (object.ReferenceEquals(casterStats, targetStats))
            {
                actionPointsDisplay.SetAccuracy(false);
            }    
            else
            {
                actionPointsDisplay.SetAccuracy(true, CalculateHitChance(casterStats, targetStats));
            }
        }

        public void DisableAPDisplay()
        {
            actionPointsDisplay.SetAccuracy(false);
            actionPointsDisplay.SetPoints(false);
            actionPointsDisplay.SetDistance(false);
            actionPointsDisplay.SetMessage(false);
        }

        public void SetSimpleCursor()
        {
            SetCursor(CursorType.MovementUp, true);
        }

        public void SetDefaultCursor()
        {
            SetCursor(CursorType.None, true);
        }

        public void SetCursor(CursorType type, bool actionAvailable)
        {
            if (!actionAvailable) type = CursorType.None;

            CursorMapping mapping = GetCursorMapping(type);
            Cursor.SetCursor(mapping.texture, mapping.hotspot, CursorMode.Auto);
        }

        public void SetTargeting(bool isEnable)
        {
            pauseMenu.SetPauseEnable(!isEnable);
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public float GetInitiative()
        {
            return baseStats.GetStat(CharacterStat.Initiative);
        }

        public static Ray GetMouseRay()
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }

        public static RaycastHit[] RaycastAllSorted()
        {
            RaycastHit[] hits = Physics.RaycastAll(GetMouseRay());
            float[] distances = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
            {
                distances[i] = hits[i].distance;
            }
            Array.Sort(distances, hits);
            return hits;
        }

        public void UseActionPoints(int spend)
        {
            actionPoints -= spend;
            SetActionPointsUsage(0);
        }

        public void AddActionPoints(int newPoints)
        {
            actionPoints = Mathf.Min(actionPoints + newPoints, maxActionPoints);
            SetActionPointsUsage(0);
        }

        public void SetActionPointsUsage(int usingActionPoints)
        {
            this.usingActionPoints = usingActionPoints > actionPoints ? 0 : usingActionPoints;

            if (onActionPointsUsageUpdate != null)
            {
                onActionPointsUsageUpdate();
            }
        }

        public int GetActionPoints()
        {
            return actionPoints;
        }

        public int GetUsingActionPoints()
        {
            return usingActionPoints;
        }

        public void ActionFinished()
        {
            if (!takingTurn) return;

            if (actionPoints <= 0)
            {
                if (IsLootingInProcess())
                {
                    finalLooting = true;
                    StartCoroutine(FinishingLooting());
                }
                else
                {
                    EndTurn();
                }
            }
            else if (animationController.IsAnimationFinished())
            {
                EnableControl(true);
            }
        }

        public void UpdateBuffs()
        {
            buffStore.UpdateFightTimers();
        }

        public void ApplyBuffEffect()
        {
            buffStore.ApplyBuffEffect();
        }

        public void StopAllActions()
        {
            fighter.CancelAction();
            mover.CancelAction();
            interactor.CancelAction();
            DisableTargetMarker();
        }

        public void DisableStatusBar()
        {
            statusBar.EnableStatusBar(false);
        }

        // PRIVATE

        bool IsLootingInProcess()
        {
            return showHideUI.GetContainer().activeSelf;
        }

        IEnumerator FinishingLooting()
        {
            while (IsLootingInProcess())
            {
                yield return null;
            }
            finalLooting = false;
            EndTurn();
        }

        int GetCorrectPoints()
        {
            int points;

            if (delayedPoints > 0)
            {
                points = delayedPoints;
                delayedPoints = 0;
            }
            else
            {
                points = Mathf.Min(startActionPoints + savedPoints, maxActionPoints);
            }

            return points;
        }

        IEnumerator TakingTurn()
        {
            while (takingTurn)
            {
                yield return null;
            }
            yield return new WaitForSeconds(delayAfterTurn);
        }

        void SetMileage(float mileage)
        {
            this.mileage = mileage;
        }

        bool IsDragging()
        {
            if (!followCamera.IsDragging()) return false;

            HideOnDrag();
            SetCursor(CursorType.Drag, true);
            return true;
        }

        bool InteractWithComponent()
        {
            targetMarker.DisablePathLine();

            Vector3 target;

            foreach (RaycastHit hit in RaycastAllSorted())
            {
                if (hit.transform.GetComponent<IRaycastable>() == null && hit.transform.gameObject.layer != vanishedLayerNumber)
                {
                    DisableStatusBar();
                    return false;
                }

                foreach (IRaycastable raycastable in hit.transform.GetComponents<IRaycastable>())
                {
                    VanishingObject vanishingComponent = raycastable.GetTransform().GetComponent<VanishingObject>();
                    if (vanishingComponent != null && vanishingComponent.IsVanished()) continue;

                    SetupRaycastable(raycastable);

                    int interactCost = raycastable.GetInteractCost(this);
                    float interactRange = raycastable.GetInteractRange(this);
                    float distance = Vector3.Distance(raycastable.GetPosition(), transform.position);

                    bool actionAvailable = true;

                    if (distance <= interactRange)
                    {
                        if (takingTurn)
                        {
                            if (raycastable == GetComponent<IRaycastable>())
                            {
                                DisableAPDisplay();
                            }
                            else if (raycastable is CombatTarget && !fighter.IsMelee() && 
                                     !Scanner.IsTargetVisible(gameObject, raycastable.GetBaseStats().gameObject) &&
                                     !raycastable.GetTransform().GetComponent<Health>().IsDead())
                            {
                                actionPointsDisplay.SetAccuracy(false);
                                actionPointsDisplay.SetPoints(false);
                                actionPointsDisplay.SetDistance(false);
                                actionPointsDisplay.SetMessage(true, "Out of View", Color.red);

                                actionAvailable = false;
                            }
                            else if (actionPoints < interactCost)
                            {
                                SetActionPointsUsage(interactCost);

                                actionPointsDisplay.SetAccuracy(false);
                                actionPointsDisplay.SetPoints(true, interactCost, false);
                                actionPointsDisplay.SetDistance(false);
                                actionPointsDisplay.SetMessage(true, "Not enought AP", Color.yellow);

                                actionAvailable = false;
                            }
                            else
                            {
                                SetActionPointsUsage(interactCost);

                                if (raycastable is CombatTarget)
                                {
                                    if (!raycastable.GetTransform().GetComponent<Health>().IsDead())
                                    {
                                        actionPointsDisplay.SetAccuracy(true, CalculateHitChance(baseStats, raycastable.GetBaseStats()));
                                    }
                                    else
                                    {
                                        actionPointsDisplay.SetAccuracy(false);
                                    }   
                                }
                                actionPointsDisplay.SetPoints(true, interactCost, true);
                                actionPointsDisplay.SetDistance(false);
                                actionPointsDisplay.SetMessage(false);
                            }
                        }
                        else
                        {   
                            if (raycastable is CombatTarget && raycastable.GetTransform().GetComponent<Health>().IsDead())
                            {
                                actionPointsDisplay.SetAccuracy(false);
                                actionPointsDisplay.SetPoints(false);
                                actionPointsDisplay.SetDistance(false);
                                actionPointsDisplay.SetMessage(false);
                            }
                            else if (raycastable is CombatTarget && !fighter.IsMelee() && !Scanner.IsTargetVisible(gameObject, raycastable.GetBaseStats().gameObject))
                            {
                                AIController aiController = raycastable.GetBaseStats().GetComponent<AIController>();
                                if (animationController.IsUnsheated() || (aiController != null && aiController.IsAggressive()))
                                {
                                    actionPointsDisplay.SetAccuracy(false);
                                    actionPointsDisplay.SetPoints(false);
                                    actionPointsDisplay.SetDistance(false);
                                    actionPointsDisplay.SetMessage(true, "Out of View", Color.red);

                                    actionAvailable = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (takingTurn)
                        {
                            if (raycastable == GetComponent<IRaycastable>())
                            {
                                DisableAPDisplay();
                            }
                            else if (buffStoreHub.IsOverweight())
                            {
                                actionPointsDisplay.SetMessage(true, "Overweight", Color.red);

                                actionAvailable = false;
                            }
                            else if (raycastable is CombatTarget && !fighter.IsMelee() && !raycastable.GetTransform().GetComponent<Health>().IsDead())
                            {
                                SetActionPointsUsage(0);

                                actionPointsDisplay.SetAccuracy(false);
                                actionPointsDisplay.SetPoints(false);

                                if (!Scanner.IsTargetVisible(gameObject, raycastable.GetBaseStats().gameObject))
                                {
                                    actionPointsDisplay.SetDistance(false);
                                    actionPointsDisplay.SetMessage(true, "Out of View", Color.red);
                                }
                                else
                                {
                                    actionPointsDisplay.SetDistance(true, distance);
                                    actionPointsDisplay.SetMessage(true, "Out of Range", Color.yellow);
                                }

                                actionAvailable = false;
                            }
                            else if (!RaycastNavMesh(raycastable.GetPosition(), out target, navMeshHitTolerance + 1, interactCost, interactRange))
                            {
                                actionAvailable = false;
                            }
                            else if (raycastable is CombatTarget)
                            {
                                if (!raycastable.GetTransform().GetComponent<Health>().IsDead())
                                {
                                    actionPointsDisplay.SetAccuracy(true, CalculateHitChance(baseStats, raycastable.GetBaseStats()));
                                }
                                else
                                {
                                    actionPointsDisplay.SetAccuracy(false);
                                }
                            }
                        }
                        else
                        {
                            if (!RaycastNavMesh(raycastable.GetPosition(), out target, navMeshHitTolerance + 1, interactCost, interactRange))
                            {
                                actionAvailable = false;
                            }
                            else if (buffStoreHub.IsOverweight())
                            {
                                actionPointsDisplay.SetMessage(true, "Overweight", Color.red);

                                actionAvailable = false;
                            }
                            else if (raycastable is CombatTarget && !fighter.IsMelee() && 
                                     !Scanner.IsTargetVisible(gameObject, raycastable.GetBaseStats().gameObject) &&
                                     !raycastable.GetTransform().GetComponent<Health>().IsDead())
                            {
                                AIController aiController = raycastable.GetBaseStats().GetComponent<AIController>();
                                if (animationController.IsUnsheated() || (aiController != null && aiController.IsAggressive()))
                                {
                                    actionPointsDisplay.SetAccuracy(false);
                                    actionPointsDisplay.SetPoints(false);
                                    actionPointsDisplay.SetDistance(false);
                                    actionPointsDisplay.SetMessage(true, "Out of View", Color.red);

                                    actionAvailable = false;
                                }
                            }
                        }
                    }

                    if (raycastable.HandleRaycast(this, actionAvailable))
                    {
                        SetupTagretMarker(raycastable, actionAvailable);
                        SetCursor(raycastable.GetCursorType(this), actionAvailable);

                        if (Input.GetMouseButtonDown(0))
                        {
                            ResetActionPointsDisplay();
                            if (distance > interactRange && !(raycastable is CombatTarget && !fighter.IsMelee()))
                            {
                                followCamera.ResetCameraTarget();
                            }

                            targetMarker.DisablePathLine();
                            ResetRaycastable();
                            ResetTargetMarker();
                            DisableStatusBar();
                        }

                        return true;
                    }
                }
            }

            DisableStatusBar();
            return false;
        }

        bool InteractWithMovement()
        {
            Vector3 target;
            
            if (RaycastNavMesh(Vector3.zero, out target, navMeshHitTolerance, 0, 0))
            {
                if (takingTurn)
                {                    
                    targetMarker.SetTargetMarker(MarkerType.Movement);
                    targetMarker.SetPosition(target);
                }

                if (buffStoreHub.IsOverweight())
                {
                    if (takingTurn)
                    {
                        SetCursor(CursorType.None, true);
                        actionPointsDisplay.SetMessage(true, "Overweight", Color.red);
                    }
                    else if (Input.GetMouseButton(0))
                    {
                        SetCursor(CursorType.None, true);
                        actionPointsDisplay.SetMessage(true, "Overweight", Color.red);
                    }
                    else
                    {
                        SetCursor(CursorType.MovementUp, true);
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        followCamera.ResetCameraTarget();
                        targetMarker.DisablePathLine();
                        targetMarker.SetTargetMarker(MarkerType.Movement);
                        targetMarker.SetPosition(target);
                        targetMarker.SetInstantiator(this.transform);
                        SetupInteraction(null);

                        if (takingTurn)
                        {
                            EnableControl(false);
                            ResetActionPointsDisplay();
                            mover.StartMoveAction(target, actionPoints, UseActionPoints, ActionFinished, null, mileage, SetMileage, 0);
                        }
                        else
                        {
                            animationController.ResetUnsheathTime();
                            mover.StartMoveAction(target);
                        }

                        audioHub.PlayClick();
                    }

                    if (Input.GetMouseButton(0))
                    {
                        SetCursor(CursorType.MovementDown, true);
                    }
                    else
                    {
                        SetCursor(CursorType.MovementUp, true);
                    }
                }

                return true;
            }
            return false;
        }

        bool RaycastNavMesh(Vector3 source, out Vector3 target, float distanceTolerance, int interactCost, float interactRange)
        {
            target = new Vector3();
            bool distanceNeeded = false;

            if (source == Vector3.zero)
            {
                distanceNeeded = true;
                RaycastHit hit;
                bool hasHit = Physics.Raycast(GetMouseRay(), out hit, 1000f, ~vanishedLayerMask);
                if (!hasHit) return false;
                source = hit.point;
            }

            NavMeshHit navMeshHit;
            bool hasNavMeshHit = NavMesh.SamplePosition(source, out navMeshHit, distanceTolerance, NavMesh.AllAreas);
            if (!hasNavMeshHit) return false;

            target = navMeshHit.position;

            NavMeshPath path = new NavMeshPath();
            bool hasPath = NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            if (!hasPath) return false;

            if (path.status != NavMeshPathStatus.PathComplete) return false;

            float pathLength = GetPathLength(path) + 0.1f;
            float availableMove = GetMaxMoveDistance(interactCost, interactRange);
            CalculateActionPoints(pathLength, availableMove, interactRange, interactCost, distanceNeeded);            

            if (pathLength > availableMove) return false;
            if (takingTurn) targetMarker.DrawPathLine(path);
            
            return true;
        }

        float GetPathLength(NavMeshPath path)
        {
            float total = 0;
            if (path.corners.Length < 2) return total;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }
            return total;
        }

        float GetMaxMoveDistance(float interactCost, float interactRange)
        {
            if (takingTurn)
            {
                float total = 0;
                total += (actionPoints - interactCost) * baseStats.GetStat(CharacterStat.Speed);
                if (mileage > 0)
                {
                    total += baseStats.GetStat(CharacterStat.Speed) - mileage;
                }
                return total + interactRange;
            }
            return maxNavMeshPathLength;
        }

        void CalculateActionPoints(float pathLength, float availableMove, float interactRange, int interactCost, bool distanceNeeded)
        {
            if (!takingTurn)
            {
                SetActionPointsUsage(0);
                DisableAPDisplay();
                return;
            }

            float mileageLeft = 0;
            if (mileage > 0)
            {
                mileageLeft += baseStats.GetStat(CharacterStat.Speed) - mileage;
            }

            float mileageToWalk = pathLength - interactRange - mileageLeft;
            if (mileageToWalk < 0) mileageToWalk = 0f;

            int actionPointsNeeded = (int)Math.Ceiling(mileageToWalk / baseStats.GetStat(CharacterStat.Speed)) + interactCost;

            SetActionPointsUsage(actionPointsNeeded);

            actionPointsDisplay.SetPoints(true, actionPointsNeeded, actionPointsNeeded <= actionPoints);
            actionPointsDisplay.SetDistance(distanceNeeded, pathLength);
            actionPointsDisplay.SetMessage(pathLength > availableMove, "Not enought AP", Color.yellow);
        }

        float CalculateHitChance(BaseStats attacker, BaseStats target)
        {
            float targetDodge = target.GetStat(CharacterStat.Dodging);
            float attackerAccuracy = attacker.GetStat(CharacterStat.Accuracy);

            int attackerLevel = attacker.GetLevel();
            int targetLevel = target.GetLevel();

            if (attackerLevel > targetLevel)
            {
                int difference = attackerLevel - targetLevel;
                attackerAccuracy += (difference * 5f);
            }
            else if (targetLevel > attackerLevel)
            {
                int difference = targetLevel - attackerLevel;
                attackerAccuracy -= (difference * 5f);
            }

            float chance = (1 - targetDodge / 100) * attackerAccuracy / 100;
            return chance * 100;
        }

        bool InteractWithUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                DisableStatusBar();
                ResetTargetMarker();
                
                if (Input.GetMouseButton(0))
                {
                    SetCursor(CursorType.MovementDown, true);
                }
                else
                {
                    SetCursor(CursorType.MovementUp, true);
                }

                return true;
            }
            return false;
        }

        CursorMapping GetCursorMapping(CursorType type)
        {
            foreach (CursorMapping mapping in cursorMappings)
            {
                if (mapping.type == type)
                {
                    return mapping;
                }
            }
            return cursorMappings[0];
        }

        void SetupRaycastable(IRaycastable raycastable)
        {
            raycastable.EnableStatusBar(); 
            raycastable.SetupStatusBar();

            if (currentRaycastable != raycastable)
            {            
                if (takingTurn) ResetRaycastable();

                currentRaycastable = raycastable;
            }

            if (takingTurn)
            {
                raycastable.EnableBattleMarker(false);
                raycastable.EnableObstacle(false);
            }
        }

        void ResetRaycastable()
        {
            if (currentRaycastable != null)
            {
                currentRaycastable.EnableObstacle(true);
                currentRaycastable.EnableBattleMarker(true, object.ReferenceEquals(baseStats, currentRaycastable.GetBaseStats()));
            }
        }

        void ResetActionPointsDisplay()
        {
            SetActionPointsUsage(0);
            DisableAPDisplay();
        }

        void SetupTagretMarker(IRaycastable raycastable, bool isAvailable)
        {
            if (takingTurn)
            {
                if (isAvailable)
                {
                    if (raycastable is CombatTarget)
                    {
                        targetMarker.SetTargetMarker(MarkerType.Attack);
                    }
                    else
                    {
                        targetMarker.SetTargetMarker(MarkerType.Interact);
                    }
                }
                else
                {
                    targetMarker.SetTargetMarker(MarkerType.NotAvailable);
                }
                targetMarker.SetPosition(raycastable.GetPosition());
            }
        }

        bool IsPlayerDead()
        {
            if (health.IsDead())
            {
                ResetTargetMarker();
                ResetRaycastable();
                SetupInteraction(null);
                SetCursor(CursorType.None, true);
                EndTurn();
                return true;
            }
            return false;
        }

        void CheckInputKeys()
        {
            if (pauseMenu.IsPaused()) return;
            if (health.IsDead()) return;

            if (Input.GetKeyDown(KeyCode.Alpha0)) actionStore.Use(KeyCode.Alpha0, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha1)) actionStore.Use(KeyCode.Alpha1, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha2)) actionStore.Use(KeyCode.Alpha2, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha3)) actionStore.Use(KeyCode.Alpha3, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha4)) actionStore.Use(KeyCode.Alpha4, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha5)) actionStore.Use(KeyCode.Alpha5, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha6)) actionStore.Use(KeyCode.Alpha6, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha7)) actionStore.Use(KeyCode.Alpha7, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha8)) actionStore.Use(KeyCode.Alpha8, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha9)) actionStore.Use(KeyCode.Alpha9, gameObject);
            if (Input.GetKeyDown(KeyCode.Alpha0)) actionStore.Use(KeyCode.Alpha0, gameObject);
            if (Input.GetKeyDown(KeyCode.Minus))  actionStore.Use(KeyCode.Minus,  gameObject);
            if (Input.GetKeyDown(KeyCode.Equals)) actionStore.Use(KeyCode.Equals, gameObject);
        }

        object ISaveable.CaptureState()
        {
            return companionIndex;
        }

        void ISaveable.RestoreState(object state)
        {
            companionIndex = (int)state;
            if (companionIndex != -1)
            {
                enabled = false;
            }
        }

        // ANIMATION

        void FootR()
        {
            audioController.PlayFootstep(true);
        }

        void FootL()
        {
            audioController.PlayFootstep(false);
        }
    }
}