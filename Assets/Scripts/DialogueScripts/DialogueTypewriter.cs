using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using Ink.Runtime;

public class DialogueTypewriter : MonoBehaviour
{
    [Header("Ink Narrative File")]
    [Tooltip("Drop your compiled Ink .json file here")]
    [SerializeField] private TextAsset inkJSONAsset;
    [SerializeField] private bool playOnStart = true;

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private UIFader fader;
    [SerializeField] private TextMeshProUGUI narrativeText;

    [Header("Typewriter Timing")]
    [SerializeField] private float charactersPerSecond = 35f;
    [SerializeField] private float punctuationPause = 0.2f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Optional Scene Transition")]
    [Tooltip("Leave blank if this typewriter doesn't change scenes on complete")]
    [SerializeField] private string nextSceneOnComplete = "";
    // [SerializeField] private float sceneFadeDuration = 1f; // console complains that this is never used

    [Header("Color Palette Dictionary")]
    [SerializeField] private List<ColorMapping> colorPalette = new List<ColorMapping>
    {
        new ColorMapping("white",  new Color32(255, 255, 255, 255)),
        new ColorMapping("red",    new Color32(235, 64, 52, 255)),
        new ColorMapping("yellow", new Color32(255, 242, 121, 255)),
        new ColorMapping("cyan",   new Color32(80, 220, 255, 255)),
        new ColorMapping("gold",   new Color32(255, 195, 0, 255))
    };

    [System.Serializable]
    public struct ColorMapping
    {
        public string key;
        public Color32 color;
        public ColorMapping(string key, Color32 color)
        {
            this.key = key;
            this.color = color;
        }
    }

    private Story currentStory;
    private Coroutine typewriterRoutine;
    private bool isTyping = false;
    private bool isEnding = false;
    private Color32 activeTextColor = Color.white;
    private Action onCompleteCallback;

    private void Awake()
    {
        // Start completely hidden and unclickable
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private void Start()
    {
        if (playOnStart && inkJSONAsset != null)
        {
            Debug.Log("NARRATIVE PLAYING");
            PlayNarrative(inkJSONAsset);
        }
    }

    public void PlayNarrative(TextAsset storyAsset = null, Action onComplete = null)
    {
        TextAsset assetToLoad = storyAsset != null ? storyAsset : inkJSONAsset;
        if (assetToLoad == null)
        {
            Debug.LogWarning("No Ink JSON Asset assigned to DialogueTypewriter!");
            return;
        }

        gameObject.SetActive(true); // turns on canvas group

        onCompleteCallback = onComplete;
        currentStory = new Story(assetToLoad.text);
        isEnding = false;

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        if (fader != null)
        {
            fader.FadeIn(fadeDuration);
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        ContinueStory();
    }

    private void Update()
    {
        if (currentStory == null || isEnding) return;

        bool interactPressed = (Keyboard.current != null && 
            (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (interactPressed)
        {
            if (isTyping)
            {
                FinishTypingImmediately();
            }
            else
            {
                ContinueStory();
            }
        }
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            string line = currentStory.Continue().Trim();
            List<string> tags = currentStory.currentTags;

            // Resolve color from tags (# color: red) or Ink VAR (text_color)
            ResolveColor(tags);

            if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
            typewriterRoutine = StartCoroutine(TypewriterSequence(line, activeTextColor));
        }
        else
        {
            StartCoroutine(EndNarrativeRoutine());
        }
    }

    private IEnumerator EndNarrativeRoutine()
    {
        isEnding = true;

        // If we are changing scenes (via field or callback), keep screen black!
        bool isTransitioning = !string.IsNullOrEmpty(nextSceneOnComplete) || onCompleteCallback != null;

        if (!isTransitioning)
        {
            if (fader != null)
            {
                fader.FadeOut(fadeDuration);
                yield return new WaitForSecondsRealtime(fadeDuration);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }
        else
        {
            if (narrativeText != null) narrativeText.text = "";
        }

        currentStory = null;
        onCompleteCallback?.Invoke();

        // Load scene if specified in the Inspector
        if (!string.IsNullOrEmpty(nextSceneOnComplete))
        {
            Time.timeScale = 1f; // Safety reset in case time was paused
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.FadeAndLoad(nextSceneOnComplete, 0.5f);
            }
            else
            {
                SceneManager.LoadScene(nextSceneOnComplete);
            }
        }
    }

    private void ResolveColor(List<string> tags)
    {
        string colorTag = ParseTagValue(tags, "color");
        if (!string.IsNullOrEmpty(colorTag))
        {
            activeTextColor = GetColorFromKey(colorTag);
            return;
        }

        // Optional: check for VAR text_color declared inside the ink file
        try
        {
            if (currentStory.variablesState["text_color"] != null)
            {
                string inkVarColor = currentStory.variablesState["text_color"].ToString();
                activeTextColor = GetColorFromKey(inkVarColor);
                return;
            }
        }
        catch
        {
            // Variable wasn't declared in this ink file, safely ignore
        }

        activeTextColor = GetColorFromKey("white");
    }

    private Color32 GetColorFromKey(string key)
    {
        key = key.Trim().ToLower();
        for (int i = 0; i < colorPalette.Count; i++)
        {
            if (colorPalette[i].key.ToLower() == key)
                return colorPalette[i].color;
        }

        if (ColorUtility.TryParseHtmlString(key.StartsWith("#") ? key : "#" + key, out Color parsedColor))
            return parsedColor;

        return Color.white;
    }

    private string ParseTagValue(List<string> tags, string prefix)
    {
        for (int i = 0; i < tags.Count; i++)
        {
            string t = tags[i].Trim();
            if (t.StartsWith(prefix + ":", StringComparison.OrdinalIgnoreCase))
            {
                return t.Substring(prefix.Length + 1).Trim();
            }
        }
        return "";
    }

    private IEnumerator TypewriterSequence(string rawText, Color32 color)
    {
        isTyping = true;
        string hexColor = ColorUtility.ToHtmlStringRGBA(color);
        narrativeText.text = $"<color=#{hexColor}>{rawText}</color>";

        narrativeText.maxVisibleCharacters = 0;
        narrativeText.ForceMeshUpdate();

        TMP_TextInfo textInfo = narrativeText.textInfo;
        int totalCharacters = textInfo.characterCount;
        float charDelay = 1f / Mathf.Max(1f, charactersPerSecond);

        for (int visibleCount = 1; visibleCount <= totalCharacters; visibleCount++)
        {
            narrativeText.maxVisibleCharacters = visibleCount;
            char currentChar = textInfo.characterInfo[visibleCount - 1].character;

            if (currentChar == '.' || currentChar == '?' || currentChar == '!' || currentChar == ',')
                yield return new WaitForSeconds(punctuationPause);
            else
                yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
    }

    private void FinishTypingImmediately()
    {
        if (typewriterRoutine != null) StopCoroutine(typewriterRoutine);
        narrativeText.maxVisibleCharacters = narrativeText.textInfo.characterCount;
        isTyping = false;
    }
}