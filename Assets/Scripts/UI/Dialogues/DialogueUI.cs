using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dialogues;
using Control;
using Audio;

namespace UI.Dialogues
{
    public class DialogueUI : MonoBehaviour
    {
        // CONFIG

        [SerializeField] ShowHideUI UICanvas;
        [SerializeField] Image AIIcon;
        [SerializeField] TMP_Text AIName;
        [SerializeField] TMP_Text AIText;
        [SerializeField] Image playerIcon;
        [SerializeField] Transform choicesParent;
        [SerializeField] GameObject choicePrefab;
        [SerializeField] ControlSwitcher controlSwitcher;

        // CACHE

        PlayerConversant playerConversant;
        AudioHub audioHub;

        // LIFECYCLE

        void Awake()
        {
            foreach (PlayerConversant playerConversant in FindObjectsOfType<PlayerConversant>())
            {
                playerConversant.onDialogueStart += StartDialogue;
            }
        }

        void Start()
        {
            audioHub = FindObjectOfType<AudioHub>();
        }

        // PRIVATE 

        void StartDialogue()
        {
            playerConversant = controlSwitcher.GetActivePlayer().GetComponent<PlayerConversant>();
            playerConversant.GetComponent<PlayerController>().EnableControl(false);

            UICanvas.OpenDialogue();

            UpdateUI();
        }

        void UpdateUI()
        {
            AIIcon.sprite = playerConversant.GetAIIcon();
            AIName.text = playerConversant.GetAIName();
            AIText.text = playerConversant.GetAIText();
            playerIcon.sprite = playerConversant.GetPlayerIcon();

            foreach (Transform choice in choicesParent)
            {
                Destroy(choice.gameObject);
            }

            int number = 1;

            foreach (DialogueNode choice in playerConversant.GetChoices())
            {
                GameObject choiceButton = Instantiate(choicePrefab, choicesParent);

                choiceButton.GetComponentInChildren<TMP_Text>().text = number + ". " + choice.GetText();
                choiceButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    audioHub.PlayClick();
                    playerConversant.TriggerSelectAction(choice);                    

                    if (playerConversant.HasNext(choice))
                    {
                        playerConversant.SelectChoice(choice);
                        UpdateUI();
                    }
                    else
                    {
                        playerConversant.QuitDialogue();

                        if (choice.GetSelectAction() != "StartShop")
                        {
                            playerConversant.GetComponent<PlayerController>().EnableControl(true);
                        }

                        UICanvas.CloseDialogue();
                    }
                });

                number++;
            }
        }  
    }
}