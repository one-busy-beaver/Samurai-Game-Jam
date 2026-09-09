using System.Collections;
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
    [SerializeField] private UIFader boxFader; // Reference to the UIFader on text_box

    [Header("Optional Scene Transition")]
    [SerializeField] private string nextSceneOnComplete = "";
    [SerializeField] private float fadeDuration = 1f;

    private Story currentStory;
    private bool isStoryActive = false;
    private bool isEnding = false; // Prevents spamming clicks during the fade out

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
        isEnding = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        // Ensure dialogue box is visible and interactable at start
        if (boxFader != null)
            boxFader.FadeIn(0.2f);

        AdvanceDialogue();
    }

    private void Update()
    {
        if (PauseController.IsGamePaused) return;
        if (!isStoryActive || isEnding) return;

        bool keyPressed = Keyboard.current != null && 
            (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);
            
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (keyPressed || mouseClicked)
        {
            AdvanceDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        if (currentStory.canContinue)
        {
            string nextLine = currentStory.Continue().Trim();
            string speakerName = ParseSpeakerTag(currentStory.currentTags);

            nameText.text = speakerName;
            dialogueText.text = nextLine;
        }
        else
        {
            StartCoroutine(EndStoryRoutine());
        }
    }

    private string ParseSpeakerTag(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            if (tag.StartsWith("speaker:"))
            {
                return tag.Substring("speaker:".Length).Trim();
            }
        }
        return "";
    }

    private IEnumerator EndStoryRoutine()
    {
        isEnding = true;
        isStoryActive = false;

        // 1. Fade out the dialogue box using UIFader
        if (boxFader != null)
        {
            boxFader.FadeOut(fadeDuration);
            yield return new WaitForSecondsRealtime(fadeDuration);
        }

        // 2. Hide the panel cleanly
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // 3. Trigger scene transition if one is set
        if (!string.IsNullOrEmpty(nextSceneOnComplete))
        {
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.FadeAndLoad(nextSceneOnComplete, fadeDuration);
            }
            else
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneOnComplete);
            }
        }
    }
}