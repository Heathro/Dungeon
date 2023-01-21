using UnityEngine;
using Combat;
using Attributes;
using Inventories;
using UI.HUD;
using UI.Inventories;
using Utils;
using Dialogues;
using Stats;
using UI;
using Animations;

namespace Control
{
    public class CombatTarget : MonoBehaviour, IRaycastable
    {
        // CONFIG

        [SerializeField] bool friendly = false;
        [SerializeField] int interactCost = 1;
        [SerializeField] float interactRange = 2f;
        [SerializeField] float companionDialogueRange = 4f;
        [SerializeField] HintDisplay hintDisplay;

        // CACHE

        Health health;
        AIController aiController;
        Inventory inventory;
        IController iController;
        AIConversant aiConversant;
        ShowHideUI showHideUI;
        ContainerUI containerUI;
        StatusBar statusBar;
        FightScheduler fightScheduler;

        // LIFECYCLE

        void Awake()
        {
            health = GetComponent<Health>();
            aiController = GetComponent<AIController>();
            inventory = GetComponent<Inventory>();
            iController = GetComponent<IController>();
            aiConversant = GetComponent<AIConversant>();

            hintDisplay.SetText(aiConversant.GetAIName());
        }

        void Start()
        {
            showHideUI = FindObjectOfType<ShowHideUI>();
            containerUI = showHideUI.GetContainer().GetComponent<ContainerUI>();
            fightScheduler = FindObjectOfType<FightScheduler>();
            statusBar = fightScheduler.GetStatusBar();
        }

        // PUBLIC

        public bool IsFriendly()
        {
            return friendly;
        }

        public bool HandleRaycast(PlayerController callingController, bool actionAvailable)
        {
            if (friendly)
            {
                if (callingController == GetComponent<PlayerController>())
                {
                    return true;
                }
                else if (health.IsDead())
                {
                    if (actionAvailable && Input.GetMouseButtonDown(0))
                    {
                        callingController.SetupInteraction(this, true);
                    }
                    return true;
                }
                else if (callingController.GetComponent<AnimationController>().IsUnsheated())
                {
                    if (actionAvailable && Input.GetMouseButtonDown(0))
                    {
                        callingController.GetComponentInParent<ControlSwitcher>().DisableLeaderPositionUpdate();
                        EnableObstacle(true);
                        callingController.StartAttack(this, true);
                        callingController.SetupInteraction(null);
                    }
                    return true;
                }
                else
                {
                    if (actionAvailable && Input.GetMouseButtonDown(0))
                    {
                        callingController.SetupInteraction(this, true);
                    }
                    return true;
                }
            }   
            
            if (health.IsDead())
            {
                if (actionAvailable && Input.GetMouseButtonDown(0))
                {
                    callingController.SetupInteraction(this);
                }
                return true;
            }

            if (!callingController.GetComponent<AnimationController>().IsUnsheated() && !aiController.IsAggressive())
            {
                if (actionAvailable && Input.GetMouseButtonDown(0))
                {
                    callingController.SetupInteraction(this);
                }
                return true;
            }

            if (!callingController.GetComponent<Fighter>().CanAttack(gameObject))
            {
                return false;
            }

            if (actionAvailable && Input.GetMouseButtonDown(0))
            {
                if (aiController.IsFighting())
                {
                    EnableObstacle(true);
                }
                callingController.SetupInteraction(null);
                callingController.StartAttack(this);
            }

            return true;
        }

        public void Interact(PlayerController callingController)
        {
            if (friendly)
            {
                if (!health.IsDead())
                {
                    PlayerConversant playerConversant = callingController.GetComponent<PlayerConversant>();
                    Dialogue dialogue = aiConversant.GetDialogue();

                    playerConversant.StartDialogue(dialogue, aiConversant);
                }
            }
            else
            {
                if (health.IsDead())
                {
                    callingController.transform.LookAt(transform);
                    showHideUI.OpenContainer();
                    containerUI.SetupContainer(inventory);
                }
                else if (!callingController.IsTakingTurn() && !aiController.IsAggressive())
                {
                    PlayerConversant playerConversant = callingController.GetComponent<PlayerConversant>();
                    Dialogue dialogue = aiConversant.GetDialogue();

                    playerConversant.StartDialogue(dialogue, aiConversant);
                }
            }
        }
        
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public CursorType GetCursorType(PlayerController callingController)
        {
            if (friendly)
            {
                if (health.IsDead() || callingController == GetComponent<PlayerController>())
                {
                    return CursorType.MovementUp;
                }
                return callingController.GetComponent<AnimationController>().IsUnsheated() ? CursorType.Combat : CursorType.Dialogue;
            }

            if (health.IsDead())
            {
                return CursorType.Loot;
            }

            if (!callingController.GetComponent<AnimationController>().IsUnsheated() && !aiController.IsAggressive())
            {
                return CursorType.Dialogue;
            }
            else
            {
                return CursorType.Combat;
            }
        }

        public int GetInteractCost(PlayerController callingController)
        {
            if (!callingController.IsTakingTurn()) return 0;

            if (health.IsDead()) return interactCost;

            return callingController.GetComponent<Fighter>().GetAttackPrice();
        }

        public float GetInteractRange(PlayerController callingController)
        {
            if (health.IsDead()) return interactRange;

            if (callingController.IsTakingTurn())
            {
                return callingController.GetComponent<Fighter>().GetAttackRange();
            }

            if (friendly) return companionDialogueRange;
            if (!aiController.IsAggressive()) return interactRange;
            
            return callingController.GetComponent<Fighter>().GetAttackRange();
        }

        public void EnableStatusBar()
        {
            statusBar.EnableStatusBar(true);
        }

        public void SetupStatusBar()
        {
            statusBar.SetUpStatusBar(health);
        }

        public void EnableHintDisplay()
        {
            if (health.IsDead()) hintDisplay.EnableHint();
        }

        public void EnableObstacle(bool isEnable)
        {
            if (friendly)
            {
                if (!GetComponent<PlayerController>().IsTakingTurn())
                {
                    iController.EnableObstacle(isEnable);
                }
                return;
            }

            if (isEnable && !aiController.IsFighting())
            {
                return;
            }

            iController.EnableObstacle(isEnable);
        }

        public void EnableBattleMarker(bool isEnable, bool isSelfTarget = false)
        {
            if (friendly)
            {
                if (isEnable && fightScheduler.IsFightRunning())
                {
                    iController.SetBattleMarker(isSelfTarget ? FighterType.Control : FighterType.Companion);
                }
                else
                {
                    iController.SetBattleMarker(FighterType.None);
                }

                return;
            }

            if (isEnable && !aiController.IsFighting())
            {
                return;
            }

            iController.SetBattleMarker(isEnable ? FighterType.Enemy : FighterType.None);
        }

        public BaseStats GetBaseStats()
        {
            return GetComponent<BaseStats>();
        }

        public Transform GetTransform()
        {
            return transform;
        }
    }
}