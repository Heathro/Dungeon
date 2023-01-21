using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SceneManagement;
using Control;
using Core;
using Audio;

namespace UI.Menu
{
    public class PauseMenuUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] GameObject mainMenu;
        [SerializeField] Button resumeButton;
        [SerializeField] Button quickSaveButton;
        [SerializeField] Button saveButton;
        [SerializeField] Button loadButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button quitButton;

        [SerializeField] GameObject saveMenu;
        [SerializeField] Transform saveList;
        [SerializeField] SaveUI saveFieldPrefab;
        [SerializeField] Button createMenuButton;
        [SerializeField] Button closeSaveButton;

        [SerializeField] GameObject createMenu;
        [SerializeField] TMP_InputField inputField;
        [SerializeField] Button createButton;
        [SerializeField] Button closeCreateButton;

        [SerializeField] GameObject loadMenu;
        [SerializeField] Transform loadList;
        [SerializeField] LoadUI loadFieldPrefab;
        [SerializeField] Button closeLoadButton;

        // CACHE

        ShowHideUI showHideUI;
        SavingWrapper savingWrapper;
        FightScheduler fightScheduler;
        PauseHub pauseHub;
        AudioHub audioHub;

        // LIFECYCLE

        void Start()
        {
            showHideUI = FindObjectOfType<ShowHideUI>();
            savingWrapper = FindObjectOfType<SavingWrapper>();
            fightScheduler = FindObjectOfType<FightScheduler>();
            pauseHub = FindObjectOfType<PauseHub>();
            audioHub = FindObjectOfType<AudioHub>();

            resumeButton.onClick.AddListener(Resume);
            quickSaveButton.onClick.AddListener(QuickSave);
            saveButton.onClick.AddListener(OpenSaveMenu);
            loadButton.onClick.AddListener(OpenLoadMenu);
            mainMenuButton.onClick.AddListener(MainMenu);
            quitButton.onClick.AddListener(Quit);

            createMenuButton.onClick.AddListener(OpenCreateMenu);
            closeSaveButton.onClick.AddListener(OpenMainMenu);

            createButton.onClick.AddListener(CreateNew);
            closeCreateButton.onClick.AddListener(OpenSaveMenu);

            closeLoadButton.onClick.AddListener(OpenMainMenu);
        }

        void Update()
        {
            createButton.interactable = !string.IsNullOrEmpty(inputField.text);
            if (createButton.interactable && Input.GetKeyDown(KeyCode.Return))
            {
                CreateNew();
            }
        }

        // PUBLIC

        public void StartPauseMenu()
        {
            mainMenu.SetActive(true);
            saveMenu.SetActive(false);
            createMenu.SetActive(false);
            loadMenu.SetActive(false);

            if (fightScheduler == null) fightScheduler = FindObjectOfType<FightScheduler>();

            quickSaveButton.interactable = !fightScheduler.IsFightRunning();
            saveButton.interactable = !fightScheduler.IsFightRunning();
        }
        
        public void OpenMainMenu()
        {
            audioHub.PlayClick();
            mainMenu.SetActive(true);
            saveMenu.SetActive(false);
            createMenu.SetActive(false);
            loadMenu.SetActive(false);

            if (fightScheduler == null) fightScheduler = FindObjectOfType<FightScheduler>();

            quickSaveButton.interactable = !fightScheduler.IsFightRunning();
            saveButton.interactable = !fightScheduler.IsFightRunning();
        }        
        
        // PRIVATE

        void OpenSaveMenu()
        {
            audioHub.PlayClick();
            mainMenu.SetActive(false);
            saveMenu.SetActive(true);
            createMenu.SetActive(false);
            loadMenu.SetActive(false);
            UpdateSaveList();
        }

        void OpenCreateMenu()
        {
            audioHub.PlayClick();
            mainMenu.SetActive(false);
            saveMenu.SetActive(false);
            createMenu.SetActive(true);
            loadMenu.SetActive(false);

            inputField.text = "";
        }

        void OpenLoadMenu()
        {
            audioHub.PlayClick();
            mainMenu.SetActive(false);
            saveMenu.SetActive(false);
            createMenu.SetActive(false);
            loadMenu.SetActive(true);
            UpdateLoadList();
        }

        void Resume()
        {
            audioHub.PlayClick();
            showHideUI.ClosePauseMenu();
        }

        void QuickSave()
        {
            audioHub.PlayClick();
            savingWrapper.Save();
            showHideUI.ClosePauseMenu();
        }

        void CreateNew()
        {
            savingWrapper.Save(inputField.text);
            savingWrapper.SetCurrentSaveFile(inputField.text);
            OpenSaveMenu();
        }

        void MainMenu()
        {
            audioHub.PlayClick();
            showHideUI.ClosePauseMenu();
            pauseHub.GoingToMainMenu();
            savingWrapper.LoadMainMenu();
        }

        void Quit()
        {
            audioHub.PlayClick();
            Application.Quit();
        }

        void UpdateSaveList()
        {
            foreach (Transform save in saveList)
            {
                Destroy(save.gameObject);
            }

            foreach (string file in savingWrapper.GetSaveList())
            {
                SaveUI saveUI = Instantiate(saveFieldPrefab, saveList);
                saveUI.Setup(file, UpdateSaveList, OpenMainMenu, audioHub, savingWrapper);
            }
        }

        void UpdateLoadList()
        {
            foreach (Transform save in loadList)
            {
                Destroy(save.gameObject);
            }

            foreach (string file in savingWrapper.GetSaveList())
            {
                LoadUI loadUI = Instantiate(loadFieldPrefab, loadList);
                loadUI.Setup(file, UpdateLoadList, audioHub, savingWrapper);
            }
        }
    }
}