using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Movement;
using Attributes;
using Core;
using Stats;
using UI.HUD;
using Utils;
using Combat;
using Quests;
using Audio;

namespace Control
{
    public class FightScheduler : MonoBehaviour
    {
        // CONFIG 

        [SerializeField] Transform players;
        [SerializeField] Transform fightQueueUI;
        [SerializeField] GameObject largeAttributeIcon;
        [SerializeField] GameObject smallAttributeIcon;
        [SerializeField] GameObject queueEndPrefab;
        [SerializeField] Button delayTurnButton;
        [SerializeField] Button endTurnButton;
        [SerializeField] StatusBar statusBar;

        // CACHE
        
        FollowCamera followCamera;
        FightAlert fightAlert;
        ControlSwitcher controlSwitcher;
        AudioPlayer audioPlayer;
        AudioHub audioHub;

        // STATE

        public event Action onFightStart;
        public event Action onFightFinish;
        public event Action onEnemyAggro;
        public event Action onQueueEnd;
        public event Action enemyTurn;
        public event Action playerTurn;

        List<IController> fighters = new List<IController>();
        List<AIController> allEnemies = new List<AIController>();
        Transform enemies;
        bool fightRunning = false;
        Coroutine fightQueue;
        float experienceReward = 0f;
        int currentFighter = 0;

        bool delayed = false;

        enum Winner { None, Players, Enemies }

        // LIFECYCLE

        void Start()
        {
            followCamera = FindObjectOfType<FollowCamera>();
            fightAlert = FindObjectOfType<FightAlert>();
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            audioPlayer = FindObjectOfType<AudioPlayer>();
            audioHub = FindObjectOfType<AudioHub>();

            EnableFightButtons(false);

            delayTurnButton.onClick.AddListener(DelayPlayerTurn);
            endTurnButton.onClick.AddListener(EndPlayerTurn);
        }

        void Update()
        {
            if (!fightRunning) return;

            SetEndButtonInteractability(controlSwitcher.GetActivePlayer().enabled);
            SetDelayButtonInteractability(IsDelayPossible());
        }

        // PUBLIC

        public void StartFight(Transform enemyGroup)
        {
            if (enemyGroup == enemies) return;
            enemies = enemyGroup; 

            if (fightRunning)
            {
                AddMoreEnemies(enemyGroup);
                RedrawFightQueueUI(currentFighter);
            }
            else
            {
                fightRunning = true;

                audioPlayer.PlayBattleTheme();

                if (onEnemyAggro != null)
                {
                    onEnemyAggro();
                }   

                AddPlayers();
                AddEnemies(enemyGroup);
                fighters = SortSeparatelyByInitiative(fighters);
                fightQueue = StartCoroutine(FightQueue());
            }
        }

        public void AddResurrected(IController iController)
        {
            if (!fighters.Contains(iController))
            {
                Health health = iController.GetHealth();
                health.onDeath += UpdateState;

                fighters.Add(iController);
                health.GetComponent<PlayerController>().ResetSavedPoints();
            }

            RedrawFightQueueUI(currentFighter);
        }

        public bool IsFightRunning()
        {
            return fightRunning;
        }

        public StatusBar GetStatusBar()
        {
            return statusBar;
        }

        public List<Health> GetAliveEnemies()
        {
            List<Health> aliveEnemies = new List<Health>();

            foreach (AIController aiController in allEnemies)
            {
                Health health = aiController.GetHealth();
                if (!health.IsDead())
                {
                    aliveEnemies.Add(health);
                }
            }

            return aliveEnemies;
        }

        // PRIVATE  

        void AddPlayers()
        {
            foreach (Transform player in players)
            {
                Health health = player.GetComponent<Health>();
                if (health.IsDead()) continue;
                health.onDeath += UpdateState;

                fighters.Add(player.GetComponent<IController>());
                player.GetComponent<ActionScheduler>().CancelCurrentAction();
                player.GetComponent<Mover>().EnableCompanionMode(false);

                PlayerController playerController = player.GetComponent<PlayerController>();
                playerController.SetBattleMarker(FighterType.Companion);
                playerController.DisableTargetMarker();
                playerController.ResetSavedPoints();
            }
        }

        void AddEnemies(Transform enemyGroup)
        {
            foreach (Transform enemy in enemyGroup)
            {
                experienceReward += enemy.GetComponent<BaseStats>().GetStat(CharacterStat.ExperienceReward);
                
                AIController AIcontroller = enemy.GetComponent<AIController>();
                Health health = enemy.GetComponent<Health>();

                AIcontroller.SetBattleMarker(FighterType.Enemy);
                allEnemies.Add(AIcontroller);
                AIcontroller.Aggrevate();
                AIcontroller.SetInFight(true);

                if (health.IsDead()) continue;

                health.onDeath += UpdateState;

                fighters.Add(enemy.GetComponent<IController>());
            }
        }

        void AddMoreEnemies(Transform enemyGroup)
        {
            List<IController> newEnemies = new List<IController>();

            foreach (Transform enemy in enemyGroup)
            {   
                experienceReward += enemy.GetComponent<BaseStats>().GetStat(CharacterStat.ExperienceReward);

                AIController AIcontroller = enemy.GetComponent<AIController>();
                Health health = enemy.GetComponent<Health>();

                AIcontroller.SetBattleMarker(FighterType.Enemy);
                AIcontroller.Aggrevate();
                AIcontroller.EnableControl(false);
                AIcontroller.EnableAgent(false);
                AIcontroller.EnableObstacle(true);
                AIcontroller.SetInFight(true);
                allEnemies.Add(AIcontroller);

                if (health.IsDead()) continue;
                health.onDeath += UpdateState;

                newEnemies.Add(enemy.GetComponent<IController>());
                enemy.GetComponent<Fighter>().Unsheath();
            }

            foreach (IController newEnemy in SortByInitiative(newEnemies))
            {
                fighters.Add(newEnemy);
            }
        }

        List<IController> SortByInitiative(List<IController> input)
        {
            return input.OrderByDescending(x => x.GetInitiative()).ToList();
        }

        List<IController> SortSeparatelyByInitiative(List<IController> input)
        {
            List<IController> players = new List<IController>();
            List<IController> enemies = new List<IController>();

            foreach (IController iController in input)
            {
                if (iController.IsEnemy())
                {
                    enemies.Add(iController);
                }
                else
                {
                    players.Add(iController);
                }
            }

            players = SortByInitiative(players);
            enemies = SortByInitiative(enemies);

            List<IController> output = new List<IController>();

            if (players[0].GetInitiative() > enemies[0].GetInitiative())
            {
                while (players.Count > 0 || enemies.Count > 0)
                {
                    if (players.Count > 0)
                    {
                        output.Add(players[0]);
                        players.RemoveAt(0);
                    }
                    if (enemies.Count > 0)
                    {
                        output.Add(enemies[0]);
                        enemies.RemoveAt(0);
                    }
                }
            }
            else
            {
                while (players.Count > 0 || enemies.Count > 0)
                {
                    if (enemies.Count > 0)
                    {
                        output.Add(enemies[0]);
                        enemies.RemoveAt(0);
                    }
                    if (players.Count > 0)
                    {
                        output.Add(players[0]);
                        players.RemoveAt(0);
                    }                    
                }
            }

            return output;
        }

        IEnumerator FightQueue()
        {
            yield return new WaitForSeconds(0.1f);
            if (WasOneShot()) yield break;

            fightAlert.TriggerAlert();
            Unsheath();

            EnableControls(false);
            EnableAgents(false);
            yield return new WaitForSeconds(1f);
            EnableObstacles(true);

            if (onFightStart != null)
            {
                onFightStart();
            }

            while (fightRunning)
            {
                for (int i = 0; i < fighters.Count; i++)
                {
                    if (fighters[i].GetHealth().IsDead()) continue;
                    currentFighter = i;

                    RedrawFightQueueUI(i);

                    fighters[i].EnableObstacle(false);

                    yield return new WaitForSeconds(0.5f);

                    followCamera.ResetCameraTarget();
                    followCamera.SetTarget(fighters[i].GetTransform());

                    fighters[i].ApplyBuffEffect();

                    SwitchPlayersPanel(fighters[i]);

                    fighters[i].SetBattleMarker(FighterType.Control);
                    fighters[i].EnableAgent(true);
                    fighters[i].EnableControl(true);

                    KeepTrackOfLeader(fighters[i]);

                    yield return fighters[i].TakeTurn();

                    fighters[i].EnableControl(false);
                    fighters[i].EnableAgent(false);
                    yield return new WaitForSeconds(0.5f);

                    fighters[i].UpdateBuffs();

                    fighters[i].SetBattleMarker(FighterType.Companion);
                    fighters[i].EnableObstacle(true);

                    if (MoveDelayed()) i--;
                }

                RemoveDead();        
                
                if (onQueueEnd != null)
                {
                    onQueueEnd();
                }

                fighters = SortSeparatelyByInitiative(fighters);
            }

            FinishFight();
        }

        bool WasOneShot()
        {
            Winner winner = IsFinished();
            if (winner == Winner.None) return false;

            if (winner == Winner.Players)
            {
                GainExperience();
            }

            FinishFight();

            return true;
        }

        void UpdateState()
        {
            if (!fightRunning) return;

            Winner winner = IsFinished();
            if (winner == Winner.None) return;

            if (winner == Winner.Players)
            {
                GainExperience();
            }

            FinishFight();
        }

        Winner IsFinished()
        {
            int enemiesLeft = 0;
            int playersLeft = 0;

            foreach (IController fighter in fighters)
            {
                if (fighter.IsEnemy() && !fighter.GetHealth().IsDead())
                {
                    enemiesLeft++;
                }
                else if (!fighter.IsEnemy() && !fighter.GetHealth().IsDead())
                {
                    playersLeft++;
                }
            }

            if (enemiesLeft == 0) return Winner.Players;
            if (playersLeft == 0) return Winner.Enemies;
            return Winner.None;
        }

        bool IsDelayPossible()
        {
            if (currentFighter == fighters.Count - 1)
            {
                return false;
            }
            if (!controlSwitcher.GetActivePlayer().enabled)
            {
                return false;
            }
            if (controlSwitcher.GetActivePlayer().IsPointsOver())
            {
                return false;
            }
            return true;
        }

        void DelayPlayerTurn()
        {
            audioHub.PlayClick();
            delayed = true;
            controlSwitcher.GetActivePlayer().DelayTurn();
        }

        bool MoveDelayed()
        {
            if (!delayed) return false;
            delayed = false;

            IController iController = fighters[currentFighter];
            fighters.Remove(iController);
            fighters.Add(iController);

            return true;
        }

        void EndPlayerTurn()
        {
            audioHub.PlayClick();
            fighters[currentFighter].EndTurn();
        }

        void RemoveDead()
        {
            List<IController> fightersToCheck = new List<IController>(fighters);

            foreach (IController fighter in fightersToCheck)
            {
                if (fighter.GetHealth().IsDead())
                {
                    fighter.GetHealth().onDeath -= UpdateState;
                    fighters.Remove(fighter);
                }
            }
        }

        void KeepTrackOfLeader(IController fighter) 
        {
            if (!fighter.IsEnemy())
            {
                controlSwitcher.UpdateControl(fighter.GetTransform().GetComponent<PlayerController>());
            }
        }

        void GiveControlToLeader()
        {
            controlSwitcher.ReturnControlToLeader();
        }

        void EnableControls(bool isEnable)
        {
            foreach (IController fighter in fighters)
            {
                if (fighter.GetHealth().IsDead()) continue;

                fighter.EnableControl(isEnable);
            }
        }

        void EnableAgents(bool isEnable)
        {
            foreach (IController fighter in fighters)
            {
                if (fighter.GetHealth().IsDead()) continue;

                fighter.EnableAgent(isEnable);
            }
        }

        void EnableObstacles(bool isEnable)
        {
            foreach (IController fighter in fighters)
            {
                if (fighter.GetHealth().IsDead()) continue;

                fighter.EnableObstacle(isEnable);
            }
        }

        void Unsheath()
        {
            foreach (IController fighter in fighters)
            {
                if (fighter.GetHealth().IsDead()) continue;

                fighter.GetHealth().GetComponent<Fighter>().Unsheath();
            }
        }

        void Sheath()
        {
            foreach (IController fighter in fighters)
            {
                if (fighter.GetHealth().IsDead()) continue;

                fighter.GetHealth().GetComponent<Fighter>().Sheath();
            }
        }

        void EnableFightButtons(bool isEnable)
        {
            delayTurnButton.gameObject.SetActive(isEnable);
            endTurnButton.gameObject.SetActive(isEnable);
        }

        void SetEndButtonInteractability(bool isEnable)
        {
            endTurnButton.interactable = isEnable;
        }

        void SetDelayButtonInteractability(bool isEnable)
        {
            delayTurnButton.interactable = isEnable;
        }

        void SwitchPlayersPanel(IController fighter)
        {
            if (fighter.IsEnemy())
            {
                EnableFightButtons(false);
                if (enemyTurn != null)
                {
                    enemyTurn();
                }
            }
            else
            {
                EnableFightButtons(true);
                if (playerTurn != null)
                {
                    playerTurn();
                }
            }
        }

        void DisableMarkers()
        {
            foreach (Transform player in players)
            {
                player.GetComponent<PlayerController>().SetBattleMarker(FighterType.None);
            }
            foreach (AIController AIcontroller in allEnemies)
            {
                AIcontroller.SetBattleMarker(FighterType.None);
                AIcontroller.SetInFight(false);
            }
            allEnemies.Clear();
        }

        void EndTurn()
        {
            foreach (IController fighter in fighters)
            {
                fighter.EndTurn();
            }
        }

        void FinishFight()
        {
            if (fightQueue != null)
            {
                StopCoroutine(fightQueue);
            }
            StartCoroutine(FinishingRoutine());
        }

        IEnumerator FinishingRoutine()
        {
            audioPlayer.PlayCivilTheme();

            EndTurn();

            fightRunning = false;
            if (onFightFinish != null)
            {
                onFightFinish();
            }

            CompleteObjectives();

            DisableMarkers();

            EnableFightButtons(false);

            EnableObstacles(false);
            yield return new WaitForSeconds(0.2f);
            EnableAgents(true);

            yield return new WaitForSeconds(1f);
            Sheath();
            yield return new WaitForSeconds(1f);
            GiveControlToLeader();
            
            ClearFightersQueue();
            RedrawFightQueueUI(0);
        }

        void CompleteObjectives()
        {
            foreach (AIController enemy in allEnemies)
            {
                QuestCompletion questCompletion = enemy.GetComponent<QuestCompletion>();
                if (questCompletion != null && questCompletion.IsTargetToKill()) questCompletion.CompleteObjective();
            }
        }

        void ClearFightersQueue()
        {
            foreach (IController fighter in fighters)
            {
                fighter.GetHealth().onDeath -= UpdateState;
            }
            fighters.Clear();
        }

        void GainExperience()
        {
            foreach (Transform player in players)
            {
                player.GetComponent<Experience>().GainExperience(experienceReward);
            }
            experienceReward = 0f;
        }

        void RedrawFightQueueUI(int currentFighter)
        {
            foreach (Transform child in fightQueueUI)
            {
                Destroy(child.gameObject);
            }

            if (fighters.Count == 0) return;

            for (int i = currentFighter; i < fighters.Count; i++)
            {
                if (fighters[i].GetHealth().IsDead()) continue;

                GameObject fighterIcon = Instantiate(i == currentFighter ? largeAttributeIcon : smallAttributeIcon, fightQueueUI);
                fighterIcon.GetComponent<AttributeIcon>().SetOwner(fighters[i].GetHealth());
            }

            Instantiate(queueEndPrefab, fightQueueUI);

            for (int i = 0; i < currentFighter; i++)
            {
                if (fighters[i].GetHealth().IsDead()) continue;

                GameObject fighterIcon = Instantiate(smallAttributeIcon, fightQueueUI);
                fighterIcon.GetComponent<AttributeIcon>().SetOwner(fighters[i].GetHealth());
            }
        }
    }
}