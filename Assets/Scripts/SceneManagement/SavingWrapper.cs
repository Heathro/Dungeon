using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Saving;
using Audio;

namespace SceneManagement
{
    public class SavingWrapper : MonoBehaviour
    {
        // STATE

        [SerializeField] float loadingTime = 1f;
        [SerializeField] int firstSceneIndex = 1;

        // CACHE

        SavingSystem savingSystem;
        Fader fader;
        AudioPlayer audioPlayer;

        // LIFECYCLE

        void Awake()
        {
            savingSystem = GetComponent<SavingSystem>();
        }

        void Start()
        {
            fader = FindObjectOfType<Fader>();
            audioPlayer = FindObjectOfType<AudioPlayer>();
        }

        // PUBLIC

        public void ContinueGame()
        {
            if (!PlayerPrefs.HasKey("currentSaveFileName")) return;
            if (!SaveFileExist()) return;
            StartCoroutine(LoadLastScene());
        }

        public void NewGame(string file)
        {
            if (string.IsNullOrEmpty(file)) return;
            SetCurrentSaveFile(file);
            StartCoroutine(LoadFirstScene());
        }

        public void LoadGame()
        {
            LoadGame(GetCurrentSaveFile());
        }

        public void LoadGame(string file)
        {
            if (string.IsNullOrEmpty(file)) return;
            SetCurrentSaveFile(file);
            ContinueGame();
        }

        public void Save()
        {
            savingSystem.Save(GetCurrentSaveFile());
        }

        public void Save(string file)
        {
            savingSystem.Save(file);
        }

        public void Load()
        {
            savingSystem.Load(GetCurrentSaveFile());
        }

        public void LoadMainMenu()
        {
            StartCoroutine(LoadTitleScene());
        }

        public void Delete()
        {
            savingSystem.Delete(GetCurrentSaveFile());
        }
        public void Delete(string file)
        {
            savingSystem.Delete(file);
        }

        public void SetCurrentSaveFile(string file)
        {
            PlayerPrefs.SetString("currentSaveFileName", file);
        }

        public string GetCurrentSaveFile()
        {
            return PlayerPrefs.GetString("currentSaveFileName");
        }

        public bool SaveFileExist()
        {
            return savingSystem.SaveFileExist(GetCurrentSaveFile());
        }

        public IEnumerable<string> GetSaveList()
        {
            return savingSystem.GetSaveList();
        }

        // PRIVATE

        IEnumerator LoadLastScene()
        {
            Cursor.visible = false;
            yield return fader.FadeOut();

            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                audioPlayer.StopMusic();
            }

            yield return savingSystem.LoadLastScene(GetCurrentSaveFile());

            audioPlayer.PlayCivilTheme();

            yield return new WaitForSeconds(loadingTime);
            yield return fader.FadeIn();
            Cursor.visible = true;
        }

        IEnumerator LoadFirstScene()
        {
            Cursor.visible = false;
            yield return fader.FadeOut();

            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                audioPlayer.StopMusic();
            }

            yield return SceneManager.LoadSceneAsync(firstSceneIndex);

            audioPlayer.PlayCivilTheme();

            yield return new WaitForSeconds(loadingTime);
            yield return fader.FadeIn();
            Cursor.visible = true;
            Save();
        }

        IEnumerator LoadTitleScene()
        {
            Cursor.visible = false;
            yield return fader.FadeOut();

            audioPlayer.StopMusic();

            yield return SceneManager.LoadSceneAsync(0);
            yield return new WaitForSeconds(loadingTime);
            yield return fader.FadeIn();
            Cursor.visible = true;
        }
    }
}