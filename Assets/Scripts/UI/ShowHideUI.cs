using UnityEngine;
using Core;
using Control;
using Audio;

namespace UI
{
    public class ShowHideUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] KeyCode inventoryKey = KeyCode.I;
        [SerializeField] KeyCode characterSheetKey = KeyCode.U;
        [SerializeField] KeyCode questKey = KeyCode.L;
        [SerializeField] KeyCode skillKey = KeyCode.K;
        [SerializeField] KeyCode pauseKey = KeyCode.Escape;

        [SerializeField] GameObject inventory = null;
        [SerializeField] GameObject characterSheet = null;
        [SerializeField] GameObject actionBar = null;
        [SerializeField] GameObject container = null;
        [SerializeField] GameObject dialogue = null;
        [SerializeField] GameObject quest = null;
        [SerializeField] GameObject shop = null;
        [SerializeField] GameObject splitMenu = null;
        [SerializeField] GameObject pauseMenu = null;
        [SerializeField] GameObject deathMenu = null;
        [SerializeField] GameObject skillDeck = null;

        // CACHE

        PauseHub pauseHub;
        ControlSwitcher controlSwitcher;
        AudioHub audioHub;

        // STATE

        bool dialogueRunning = false;
        bool shopWorking = false;
        bool partyDead = false;

        // LIFECYCLE

        void Start()
        {
            pauseHub = FindObjectOfType<PauseHub>();
            audioHub = FindObjectOfType<AudioHub>();
            controlSwitcher = FindObjectOfType<ControlSwitcher>();
            controlSwitcher.onPartyDeath += OpenDeathMenu;

            CloseInventory();
            CloseCharacterSheet();
            CloseDialogue();
            CloseQuest();
            CloseShop();
            CloseContainer();
            CloseSplitMenu();
            ClosePauseMenu();
            CloseDeathMenu();
            CloseSkillDeck();
        }

        void Update()
        {
            if (partyDead) return;

            if (Input.GetKeyDown(pauseKey))
            {
                TogglePauseMenu();
            }            

            if (pauseHub.IsPaused()) return;
            if (dialogueRunning) return;
            if (shopWorking) return;

            if (Input.GetKeyDown(inventoryKey))
            {
                ToggleInventory();
            }
            if (Input.GetKeyDown(characterSheetKey))
            {
                ToggleCharacterSheet();
            }
            if (Input.GetKeyDown(questKey))
            {
                ToggleQuest();
            }
            if (Input.GetKeyDown(skillKey))
            {
                ToggleSkillDeck();
            }
        }

        // PUBLIC

        public AudioHub GetAudioHub()
        {
            return audioHub;
        }

        public void CloseAll()
        {
            CloseInventory();
            CloseCharacterSheet();
            CloseDialogue();
            CloseQuest();
            CloseShop();
            CloseContainer();
            CloseSplitMenu();
            CloseSkillDeck();
            OpenActionBar();
        }

        public void OpenInventory()
        {
            if (dialogueRunning) return;
            if (shopWorking) return;

            CloseQuest();
            CloseSkillDeck();

            inventory.SetActive(true);
        }

        public void CloseInventory(bool sound = false)
        {
            if (sound) audioHub.PlayClick(); 

            CloseSplitMenu();

            inventory.SetActive(false);
        }

        public void ToggleInventory()
        {
            audioHub.PlayClick();
            if (inventory.activeSelf)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }

        public void OpenCharacterSheet()
        {
            if (dialogueRunning) return;
            if (shopWorking) return;

            CloseQuest();
            CloseSkillDeck();

            characterSheet.SetActive(true);
        }

        public void CloseCharacterSheet(bool sound = false)
        {
            if (sound) audioHub.PlayClick();

            characterSheet.SetActive(false);
        }

        public void ToggleCharacterSheet()
        {
            audioHub.PlayClick();
            if (characterSheet.activeSelf)
            {
                CloseCharacterSheet();
            }
            else
            {
                OpenCharacterSheet();
            }
        }

        public void OpenActionBar()
        {
            actionBar.SetActive(true);
        }  
        
        public void CloseActionBar()
        {
            actionBar.SetActive(false);
        }

        public void OpenContainer()
        {
            container.SetActive(true);
            audioHub.PlayOpen();
        } 

        public void CloseContainer(bool sound = false)
        {
            if (sound) audioHub.PlayClick();

            container.SetActive(false);
        }

        public GameObject GetContainer()
        {
            return container;
        }   
        
        public void OpenDialogue()
        {
            dialogueRunning = true;

            CloseQuest();
            CloseInventory();
            CloseCharacterSheet();
            CloseContainer();
            CloseSplitMenu();
            CloseSkillDeck();

            CloseActionBar();
            dialogue.SetActive(true);
        }

        public void CloseDialogue()
        {
            dialogueRunning = false;

            if (!shopWorking) OpenActionBar();

            dialogue.SetActive(false);
        }

        public void OpenQuest()
        {
            if (dialogueRunning) return;
            if (shopWorking) return;

            CloseInventory();
            CloseCharacterSheet();
            CloseContainer();
            CloseSplitMenu();
            CloseSkillDeck();

            quest.SetActive(true);
        }

        public void CloseQuest(bool sound = false)
        {
            if (sound) audioHub.PlayClick();

            quest.SetActive(false);
        }

        public void ToggleQuest()
        {
            audioHub.PlayClick();
            if (quest.activeSelf)
            {
                CloseQuest();
            }
            else
            {
                OpenQuest();
            }
        }

        public void OpenShop()
        {
            shopWorking = true;

            CloseDialogue();
            CloseActionBar();

            shop.SetActive(true);
        }

        public void CloseShop(bool sound = false)
        {
            if (sound) audioHub.PlayClick();

            shopWorking = false;

            CloseSplitMenu();
            OpenActionBar();

            controlSwitcher.GetActivePlayer().EnableControl(true);

            shop.SetActive(false);
        }

        public void OpenSplitMenu()
        {
            splitMenu.SetActive(true);
        }

        public void CloseSplitMenu(bool sound = false)
        {
            if (sound) audioHub.PlayClick();

            splitMenu.SetActive(false);
        }

        public GameObject GetSplitMenu()
        {
            return splitMenu;
        }

        public bool IsSplitMenuActive()
        {
            return splitMenu.activeSelf;
        }

        public void OpenPauseMenu()
        {
            if (!pauseHub.CanPause()) return;

            pauseHub.SetPause(true);
            pauseMenu.SetActive(true);
            pauseMenu.GetComponent<UI.Menu.PauseMenuUI>().StartPauseMenu();
        }

        public void ClosePauseMenu()
        {
            pauseHub.SetPause(false);
            pauseMenu.SetActive(false);
        }

        public void TogglePauseMenu()
        {
            audioHub.PlayClick();
            if (pauseMenu.activeSelf)
            {
                ClosePauseMenu();
            }
            else
            {
                OpenPauseMenu();
            }
        }

        public void OpenDeathMenu()
        {
            partyDead = true;

            pauseHub.SetPause(true);
            deathMenu.SetActive(true);
            deathMenu.GetComponent<UI.Menu.DeathMenuUI>().OpenMainMenu();
        }

        public void CloseDeathMenu()
        {
            pauseHub.SetPause(false);
            deathMenu.SetActive(false);
        }

        public void OpenSkillDeck()
        {
            if (dialogueRunning) return;
            if (shopWorking) return;

            CloseInventory();
            CloseCharacterSheet();
            CloseContainer();
            CloseSplitMenu();
            CloseQuest();

            skillDeck.SetActive(true);
        }

        public void CloseSkillDeck(bool sound = false)
        {
            if (sound) audioHub.PlayClick();

            skillDeck.SetActive(false);
        }

        public void ToggleSkillDeck()
        {
            audioHub.PlayClick();
            if (skillDeck.activeSelf)
            {
                CloseSkillDeck();
            }
            else
            {
                OpenSkillDeck();
            }
        }

        public bool IsSinglePlayerActionRunning()
        {
            return shopWorking || dialogueRunning;
        }
    }
}