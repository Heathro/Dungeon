using UnityEngine;
using UnityEngine.UI;
using SceneManagement;
using Audio;

namespace UI.Menu
{
    public class DeathMenuUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] GameObject mainMenu;
        [SerializeField] Button quickLoadButton;
        [SerializeField] Button loadButton;
        [SerializeField] Button mainMenuButton;
        [SerializeField] Button quitButton;

        [SerializeField] GameObject loadMenu;
        [SerializeField] Transform loadList;
        [SerializeField] LoadUI loadFieldPrefab;
        [SerializeField] Button closeLoadButton;

        // CACHE

        ShowHideUI showHideUI;
        SavingWrapper savingWrapper;
        AudioHub audioHub;

        // LIFECYCLE

        void Start()
        {
            showHideUI = FindObjectOfType<ShowHideUI>();
            savingWrapper = FindObjectOfType<SavingWrapper>();
            audioHub = FindObjectOfType<AudioHub>();

            quickLoadButton.onClick.AddListener(QuickLoad);
            loadButton.onClick.AddListener(OpenLoadMenu);
            mainMenuButton.onClick.AddListener(MainMenu);
            quitButton.onClick.AddListener(Quit);
            closeLoadButton.onClick.AddListener(OpenMainMenu);
        }

        // PUBLIC

        public void OpenMainMenu()
        {
            audioHub.PlayClick();
            mainMenu.SetActive(true);
            loadMenu.SetActive(false);

            quickLoadButton.interactable = PlayerPrefs.HasKey("currentSaveFileName") && savingWrapper.SaveFileExist();
        }

        // PRIVATE

        void OpenLoadMenu()
        {
            audioHub.PlayClick();
            mainMenu.SetActive(false);
            loadMenu.SetActive(true);
            UpdateLoadList();
        }

        void QuickLoad()
        {
            audioHub.PlayClick();
            showHideUI.ClosePauseMenu();
            savingWrapper.LoadGame();
        }

        void MainMenu()
        {
            audioHub.PlayClick();
            showHideUI.CloseDeathMenu();
            savingWrapper.LoadMainMenu();
        }

        void Quit()
        {
            audioHub.PlayClick();
            Application.Quit();
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