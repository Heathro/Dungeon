using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SceneManagement;
using Audio;

namespace UI.Menu
{
    public class LoadUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text title;
        [SerializeField] Button loadButton;
        [SerializeField] Button deleteButton;

        // STATE

        Action update;
        SavingWrapper savingWrapper = null;
        AudioHub audioHub = null;

        // LIFECYCLE

        void Start()
        {
            loadButton.onClick.AddListener(Load);
            deleteButton.onClick.AddListener(Delete);
        }

        // PUBLIC

        public void Setup(string title, Action update, AudioHub audioHub, SavingWrapper savingWrapper)
        {
            this.title.text = title;
            this.update = update;
            this.audioHub = audioHub;
            this.savingWrapper = savingWrapper;
        }

        // PRIVATE

        void Load()
        {
            Time.timeScale = 1f;
            audioHub.PlayClick();
            savingWrapper.LoadGame(title.text);
        }

        void Delete()
        {
            audioHub.PlayClick();
            savingWrapper.Delete(title.text);
            update();
        }
    }
}