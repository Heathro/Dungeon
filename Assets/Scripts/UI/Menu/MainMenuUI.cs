using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;
using SceneManagement;
using Audio;

namespace UI.Menu
{
    public class MainMenuUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] GameObject mainMenu;
        [SerializeField] Button continueButton;
        [SerializeField] Button newGameButton;
        [SerializeField] Button loadGameButton;
        [SerializeField] Button quitGameButton;

        [SerializeField] GameObject createMenu;
        [SerializeField] TMP_InputField inputField;
        [SerializeField] Button createButton;
        [SerializeField] Button closeCreateButton;

        [SerializeField] GameObject loadMenu;
        [SerializeField] Button closeLoadButton;
        [SerializeField] Transform saveList;
        [SerializeField] LoadUI savePrefab;

        [SerializeField] Texture2D cursoreTexture;
        [SerializeField] Vector2 cursorHotspot;

        // CACHE

        LazyValue<SavingWrapper> savingWrapper;
        LazyValue<AudioPlayer> audioPlayer;
        LazyValue<AudioHub> audioHub;

        // LIFECYCLE

        void Awake()
        {
            Cursor.SetCursor(cursoreTexture, cursorHotspot, CursorMode.Auto);

            savingWrapper = new LazyValue<SavingWrapper>(GetSavingWrapper);
            audioPlayer = new LazyValue<AudioPlayer>(GetAudioPlayer);
            audioHub = new LazyValue<AudioHub>(GetAudioHub);

            continueButton.onClick.AddListener(Continue);
            newGameButton.onClick.AddListener(NewGameMenu);
            loadGameButton.onClick.AddListener(LoadGameMenu);
            quitGameButton.onClick.AddListener(QuitGame);

            createButton.onClick.AddListener(Create);
            closeCreateButton.onClick.AddListener(MainMenu);

            closeLoadButton.onClick.AddListener(MainMenu);

            FirstLoadMainMenu();
        }

        void Start()
        {
            audioPlayer.Value.PlayMenuTheme();
        }

        void Update()
        {
            createButton.interactable = !string.IsNullOrEmpty(inputField.text);
            if (createButton.interactable && Input.GetKeyDown(KeyCode.Return))
            {
                Create();
            }
        }

        // PRIVATE

        void Continue()
        {
            audioHub.Value.PlayClick();
            savingWrapper.Value.ContinueGame();
        }

        void FirstLoadMainMenu()
        {
            mainMenu.SetActive(true);
            createMenu.SetActive(false);
            loadMenu.SetActive(false);

            continueButton.interactable = PlayerPrefs.HasKey("currentSaveFileName") && savingWrapper.Value.SaveFileExist();
        }

        void MainMenu()
        {
            audioHub.Value.PlayClick();

            mainMenu.SetActive(true);
            createMenu.SetActive(false);
            loadMenu.SetActive(false);

            continueButton.interactable = PlayerPrefs.HasKey("currentSaveFileName") && savingWrapper.Value.SaveFileExist();
        }

        void NewGameMenu()
        {
            audioHub.Value.PlayClick();
            mainMenu.SetActive(false);
            createMenu.SetActive(true);
            loadMenu.SetActive(false);

            inputField.text = "";
        }

        void Create()
        {
            audioHub.Value.PlayClick();
            savingWrapper.Value.NewGame(inputField.text);
        }

        void LoadGameMenu()
        {
            audioHub.Value.PlayClick();
            mainMenu.SetActive(false);
            createMenu.SetActive(false);
            loadMenu.SetActive(true);

            UpdateSaveList();
        }

        void UpdateSaveList()
        {
            foreach (Transform save in saveList)
            {
                Destroy(save.gameObject);
            }

            foreach (string file in savingWrapper.Value.GetSaveList())
            {
                LoadUI loadUI = Instantiate(savePrefab, saveList);
                loadUI.Setup(file, UpdateSaveList, audioHub.Value, savingWrapper.Value);
            }
        }

        void QuitGame()
        {
            audioHub.Value.PlayClick();
            Application.Quit();
        }

        SavingWrapper GetSavingWrapper()
        {
            return FindObjectOfType<SavingWrapper>();
        }

        AudioPlayer GetAudioPlayer()
        {
            return FindObjectOfType<AudioPlayer>();
        }

        AudioHub GetAudioHub()
        {
            return FindObjectOfType<AudioHub>();
        }
    }
}