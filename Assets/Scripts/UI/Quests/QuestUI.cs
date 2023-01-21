using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Quests;
using Utils;
using Audio;

namespace UI.Quests
{
    public class QuestUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] Transform questListUI;
        [SerializeField] Transform descriptionListUI;
        [SerializeField] GameObject questOwner;
        [SerializeField] GameObject questButton;
        [SerializeField] GameObject descriptionHeader;
        [SerializeField] GameObject descriptionText;

        // CACHE

        QuestStatus lastQuest;
        QuestList questList;
        ShowHideUI showHideUI;
        AudioHub audioHub;

        // LIFECYCLE

        void Start()
        {
            showHideUI = FindObjectOfType<ShowHideUI>();
            audioHub = FindObjectOfType<AudioHub>();
        }

        void OnEnable()
        {
            if (questList == null)
            { 
                questList = FindObjectOfType<QuestList>();
            }
            if (lastQuest == null)
            {
                SetDefaultLastQuest();
            }

            RedrawQuestList();
            RedrawDescriptionList(lastQuest);
        }

        void Update()
        {
            if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                showHideUI.CloseQuest();
            }
        }

        // PRIVATE

        void RedrawQuestList()
        {
            foreach (Transform child in questListUI)
            {
                Destroy(child.gameObject);
            }

            DisplayOwnerQuests(QuestOwner.All);
            DisplayOwnerQuests(QuestOwner.Priest);
            DisplayOwnerQuests(QuestOwner.Paladin);
            DisplayOwnerQuests(QuestOwner.Hunter);
            DisplayOwnerQuests(QuestOwner.Mage);
        }

        void DisplayOwnerQuests(QuestOwner owner)
        {
            if (questList.GetStatuses(owner).ToList().Count == 0) return;

            GameObject ownerHeader = Instantiate(questOwner, questListUI);
            ownerHeader.GetComponentInChildren<TMP_Text>().text = questList.GetOwnerText(owner);

            foreach (QuestStatus status in questList.GetStatuses(owner))
            {
                GameObject button = Instantiate(questButton, questListUI);
                button.GetComponentInChildren<TMP_Text>().text = status.GetQuest().GetName();
                button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    audioHub.PlayClick();
                    lastQuest = status;
                    RedrawDescriptionList(status);
                });
            }
        }

        void RedrawDescriptionList(QuestStatus status)
        {
            foreach (Transform child in descriptionListUI)
            {
                Destroy(child.gameObject);
            }

            if (status == null) return;

            GameObject header = Instantiate(descriptionHeader, descriptionListUI);
            header.GetComponent<TMP_Text>().text = status.GetQuest().name;

            GameObject story = Instantiate(descriptionText, descriptionListUI);
            story.GetComponent<TMP_Text>().text = status.GetQuest().GetStory();

            GameObject space = Instantiate(descriptionHeader, descriptionListUI);
            space.GetComponent<TMP_Text>().text = " ";

            foreach (Quest.Objective objective in status.GetQuest().GetObjectives())
            {
                GameObject line = Instantiate(descriptionText, descriptionListUI);
                line.GetComponent<TMP_Text>().text = objective.description;
                if (status.IsObjectiveComplete(objective.reference))
                {
                    line.GetComponent<TMP_Text>().color = Color.green;
                }
            }
        }

        void SetDefaultLastQuest()
        {
            foreach (QuestStatus status in questList.GetStatuses(QuestOwner.All))
            {
                lastQuest = status;
            }
            foreach (QuestStatus status in questList.GetStatuses(QuestOwner.Priest))
            {
                lastQuest = status;
            }
            foreach (QuestStatus status in questList.GetStatuses(QuestOwner.Paladin))
            {
                lastQuest = status;
            }
            foreach (QuestStatus status in questList.GetStatuses(QuestOwner.Hunter))
            {
                lastQuest = status;
            }
            foreach (QuestStatus status in questList.GetStatuses(QuestOwner.Mage))
            {
                lastQuest = status;
            }
        }
    }
}