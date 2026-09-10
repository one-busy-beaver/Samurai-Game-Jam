using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Ink.Runtime;

public class DialogueController : MonoBehaviour
{
    public enum TimeScaleMode
    {
        NormalTime,
        FreezeTimeDuringDialogue
    }

    [System.Serializable]
    public class CharacterSlot
    {
        public string characterName;         // Matches "# speaker: Name"
        public string defaultTitle;          // e.g. "Commander", "Haruto", "Lead Engineer"
        public RectTransform portraitRect;    // The RectTransform of this portrait
        public Image portraitImage;          // The Image component for visuals
        [HideInInspector] public Vector2 defaultAnchoredPos;
        [HideInInspector] public Coroutine hopRoutine;
    }

    [Header("Ink File")]
    [SerializeField] private TextAsset inkJSONAsset;

    [Header("Time Settings")]
    [Tooltip("FreezeTimeDuringDialogue sets Time.timeScale to 0 while reading, then restores to 1 when finished.")]
    [SerializeField] private TimeScaleMode timeMode = TimeScaleMode.NormalTime;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI titleText;      // Context, surname, or rank/role
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private UIFader boxFader;

    [Header("Character Portrait Slots")]
    [SerializeField] private CharacterSlot leftSlot;   // Char 1 (Left side)
    [SerializeField] private CharacterSlot rightSlot;  // Char 2 (Right side)

    [Header("Speaking Bounce Settings")]
    [SerializeField] private float bounceHeight = 25f;
    [SerializeField] private float bounceDuration = 0.15f;

    [Header("Optional Scene Transition")]
    [SerializeField] private string nextSceneOnComplete = "";
    [SerializeField] private float fadeDuration = 1f;

    private Story currentStory;
    private bool isStoryActive = false;
    private bool isEnding = false;
    private string lastSpeaker = "";

    public static bool IsDialogueActive { get; private set; }

    private void Awake()
    {
        // Cache original anchored positions so bounce routines return accurately
        if (leftSlot.portraitRect != null)
            leftSlot.defaultAnchoredPos = leftSlot.portraitRect.anchoredPosition;

        if (rightSlot.portraitRect != null)
            rightSlot.defaultAnchoredPos = rightSlot.portraitRect.anchoredPosition;
    }

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
        lastSpeaker = "";

        // Track dialogue state globally
        IsDialogueActive = true;

        if (timeMode == TimeScaleMode.FreezeTimeDuringDialogue)
        {
            Time.timeScale = 0f;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

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
            List<string> tags = currentStory.currentTags;

            string speakerName = ParseTag(tags, "speaker");
            string showDirective = ParseTag(tags, "show");
            
            // Check for dynamic title tag (# title: ... or # context: ...)
            string customTitle = ParseTag(tags, "title");
            if (string.IsNullOrEmpty(customTitle))
            {
                customTitle = ParseTag(tags, "context");
            }

            // Update UI
            nameText.text = speakerName;
            dialogueText.text = nextLine;
            UpdateTitleUI(speakerName, customTitle);

            // Handle showing/hiding portraits
            HandlePortraitVisibility(speakerName, showDirective);

            // Trigger bounce indication if a new speaker takes the turn
            if (!string.IsNullOrEmpty(speakerName) && speakerName != lastSpeaker)
            {
                TriggerSpeakerBounce(speakerName);
                lastSpeaker = speakerName;
            }
        }
        else
        {
            StartCoroutine(EndStoryRoutine());
        }
    }

    private void UpdateTitleUI(string speaker, string explicitTitle)
    {
        if (titleText == null) return;

        // 1. If Ink explicitly gave a # title: or # context: tag, use that
        if (!string.IsNullOrEmpty(explicitTitle))
        {
            titleText.text = explicitTitle;
            titleText.gameObject.SetActive(true);
            return;
        }

        // 2. Check if the matching CharacterSlot has a default title configured
        string fallbackTitle = GetDefaultTitle(speaker);
        if (!string.IsNullOrEmpty(fallbackTitle))
        {
            titleText.text = fallbackTitle;
            titleText.gameObject.SetActive(true);
            return;
        }

        // 3. Otherwise hide/clear the field so it doesn't show stale text
        titleText.text = "";
        titleText.gameObject.SetActive(false);
    }

    private string GetDefaultTitle(string speaker)
    {
        if (IsSlotSpeaker(leftSlot, speaker)) return leftSlot.defaultTitle;
        if (IsSlotSpeaker(rightSlot, speaker)) return rightSlot.defaultTitle;
        return "";
    }

    private void HandlePortraitVisibility(string speaker, string showDirective)
    {
        // 1. Explicit tag overrides: # show: both / left / right / none
        if (!string.IsNullOrEmpty(showDirective))
        {
            switch (showDirective.ToLower())
            {
                case "both":
                    SetSlotActive(leftSlot, true);
                    SetSlotActive(rightSlot, true);
                    return;
                case "left":
                    SetSlotActive(leftSlot, true);
                    SetSlotActive(rightSlot, false);
                    return;
                case "right":
                    SetSlotActive(leftSlot, false);
                    SetSlotActive(rightSlot, true);
                    return;
                case "none":
                case "neither":
                    SetSlotActive(leftSlot, false);
                    SetSlotActive(rightSlot, false);
                    return;
            }
        }

        // 2. Default behavior: auto-reveal speaker slot if assigned
        if (IsSlotSpeaker(leftSlot, speaker))
            SetSlotActive(leftSlot, true);

        if (IsSlotSpeaker(rightSlot, speaker))
            SetSlotActive(rightSlot, true);
    }

    private void TriggerSpeakerBounce(string speaker)
    {
        if (IsSlotSpeaker(leftSlot, speaker))
            PlayBounce(leftSlot);
        else if (IsSlotSpeaker(rightSlot, speaker))
            PlayBounce(rightSlot);
    }

    private bool IsSlotSpeaker(CharacterSlot slot, string speaker)
    {
        return !string.IsNullOrEmpty(slot.characterName) && 
               string.Equals(slot.characterName.Trim(), speaker.Trim(), System.StringComparison.OrdinalIgnoreCase);
    }

    private void SetSlotActive(CharacterSlot slot, bool active)
    {
        if (slot.portraitRect != null)
            slot.portraitRect.gameObject.SetActive(active);
    }

    private void PlayBounce(CharacterSlot slot)
    {
        if (slot.portraitRect == null || !slot.portraitRect.gameObject.activeInHierarchy) return;

        if (slot.hopRoutine != null)
            StopCoroutine(slot.hopRoutine);

        slot.hopRoutine = StartCoroutine(BounceRoutine(slot));
    }

    private IEnumerator BounceRoutine(CharacterSlot slot)
    {
        RectTransform rt = slot.portraitRect;
        Vector2 originalPos = slot.defaultAnchoredPos;
        Vector2 targetPos = originalPos + new Vector2(0f, bounceHeight);

        // 1. Hop up
        float elapsed = 0f;
        float halfDuration = bounceDuration * 0.5f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            rt.anchoredPosition = Vector2.Lerp(originalPos, targetPos, elapsed / halfDuration);
            yield return null;
        }

        // 2. Fall down
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            rt.anchoredPosition = Vector2.Lerp(targetPos, originalPos, elapsed / halfDuration);
            yield return null;
        }

        rt.anchoredPosition = originalPos;
        slot.hopRoutine = null;
    }

    private string ParseTag(List<string> currentTags, string key)
    {
        string prefix = key + ":";
        foreach (string tag in currentTags)
        {
            string trimmed = tag.Trim();
            if (trimmed.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring(prefix.Length).Trim();
            }
        }
        return "";
    }

private IEnumerator EndStoryRoutine()
    {
        isEnding = true;
        isStoryActive = false;
        IsDialogueActive = false; // Mark dialogue as finished

        SetSlotActive(leftSlot, false);
        SetSlotActive(rightSlot, false);

        if (titleText != null)
            titleText.gameObject.SetActive(false);

        if (boxFader != null)
        {
            boxFader.FadeOut(fadeDuration);
            yield return new WaitForSecondsRealtime(fadeDuration);
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Only restore time if game isn't currently paused in PauseController
        if (timeMode == TimeScaleMode.FreezeTimeDuringDialogue && !PauseController.IsGamePaused)
        {
            Time.timeScale = 1f;
        }

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

    private void OnDisable()
    {
        IsDialogueActive = false;

        if (timeMode == TimeScaleMode.FreezeTimeDuringDialogue && Time.timeScale == 0f && !PauseController.IsGamePaused)
        {
            Time.timeScale = 1f;
        }
    }
}