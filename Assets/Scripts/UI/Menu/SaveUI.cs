using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SceneManagement;
using Audio;

namespace UI.Menu
{
    public class SaveUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] TMP_Text title;
        [SerializeField] Button saveButton;
        [SerializeField] Button deleteButton;

        // STATE

        Action update;
        Action back;
        SavingWrapper savingWrapper = null;
        AudioHub audioHub = null;

        // LIFECYCLE

        void Start()
        {
            saveButton.onClick.AddListener(Save);
            deleteButton.onClick.AddListener(Delete);
        }

        // PUBLIC

        public void Setup(string title, Action update, Action back, AudioHub audioHub, SavingWrapper savingWrapper)
        {
            this.title.text = title;
            this.update = update;
            this.back = back;
            this.savingWrapper = savingWrapper;
            this.audioHub = audioHub;
        }

        // PRIVATE

        void Save()
        {
            savingWrapper.Save(title.text);
            back();
        }

        void Delete()
        {
            audioHub.PlayClick();
            savingWrapper.Delete(title.text);
            update();
        }
    }
}