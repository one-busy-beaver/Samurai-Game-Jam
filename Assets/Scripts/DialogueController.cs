using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Ink.Runtime;

public class DialogueController : MonoBehaviour
{
    [Header("Ink File")]
    [SerializeField] private TextAsset inkJSONAsset;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Optional Scene Transition")]
    [SerializeField] private string nextSceneOnComplete = "";
    [SerializeField] private float fadeDuration = 1f;

    private Story currentStory;
    private bool isStoryActive = false;

    private void Start()
    {
        if (inkJSONAsset != null)
        {
            StartStory();
        }
    }

    public void StartStory()
    {
        currentStory = new Story(inkJSONAsset.text);
        isStoryActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        AdvanceDialogue();
    }

    private void Update()
    {
        if (!isStoryActive) return;

        // Advance line when space, enter, or mouse click is tapped
        if ((Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        if (currentStory.canContinue)
        {
            // 1. Fetch the raw dialogue line
            string nextLine = currentStory.Continue().Trim();

            // 2. Extract the speaker from Ink tags
            string speakerName = ParseSpeakerTag(currentStory.currentTags);

            // 3. Update the UI
            nameText.text = speakerName;
            dialogueText.text = nextLine;
        }
        else
        {
            EndStory();
        }
    }

    private string ParseSpeakerTag(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            // Expecting tags written as: #speaker: Name
            if (tag.StartsWith("speaker:"))
            {
                return tag.Substring("speaker:".Length).Trim();
            }
        }

        return ""; // Leave name blank if no speaker tag was added
    }

    private void EndStory()
    {
        isStoryActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Transition out if a target scene is defined
        if (!string.IsNullOrEmpty(nextSceneOnComplete))
        {
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.FadeAndLoad(nextSceneOnComplete, fadeDuration);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneOnComplete);
            }
        }
    }
}