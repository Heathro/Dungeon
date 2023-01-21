using UnityEngine;
using UnityEngine.UI;
using Control;
using UI.HUD;
using Animations;

namespace UI.Inventories
{
    public class CharacterSwitcher : MonoBehaviour
    {
        // CONFIG

        [SerializeField] GameObject[] players;
        [SerializeField] GameObject[] attributeBars;

        // STATE

        ControlSwitcher controlSwitcher;
        FightScheduler fightScheduler;
        int currentPlayer;

        // LIFECYCLE

        void Start()
        {
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            controlSwitcher.onControlChange += SwitchPlayer;
            SwitchPlayer();

            fightScheduler = FindObjectOfType<FightScheduler>();

            fightScheduler.playerTurn += EnableAttributeBar;
            fightScheduler.enemyTurn += DisableAttributeBar;

            fightScheduler.onFightStart += DisableAttributeBar;
            fightScheduler.onFightFinish += EnableAttributeBar;

            foreach (GameObject attributeBar in attributeBars)
            {
                attributeBar.GetComponent<AttributeBar>().SubscribeAPBars(fightScheduler);
            }    
        }

        // PRIVATE

        void SwitchPlayer()
        {
            currentPlayer = controlSwitcher.GetActivePlayerIndex();
            for (int i = 0; i < players.Length; i++)
            {
                players[i].SetActive(i == currentPlayer);
                attributeBars[i].SetActive(i == currentPlayer);
            }
        }

        void EnableAttributeBar()
        {
            SwitchPlayer();
        }

        void DisableAttributeBar()
        {
            for (int i = 0; i < players.Length; i++)
            {
                attributeBars[i].SetActive(false);
            }
        }
    }
}