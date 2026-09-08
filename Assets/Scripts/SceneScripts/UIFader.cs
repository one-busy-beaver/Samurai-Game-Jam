using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIFader : MonoBehaviour
{
    public enum FadeMode
    {
        None,           // Waits for manual script calls (FadeIn / FadeOut)
        FadeInOnly,     // Starts hidden (alpha 0), fades to visible (alpha 1)
        FadeOutOnly,    // Starts visible (alpha 1), fades to hidden (alpha 0)
        FadeInAndOut    // Fades in, pauses, then fades back out
    }

    [Header("Mode & Triggers")]
    [SerializeField] private FadeMode autoFadeOnStart = FadeMode.None;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 1f;
    [Tooltip("How long to stay fully visible when using FadeInAndOut mode")]
    [SerializeField] private float holdDuration = 2f;

    private CanvasGroup canvasGroup;
    private Coroutine currentFadeRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        switch (autoFadeOnStart)
        {
            case FadeMode.FadeInOnly:
                SetImmediateAlpha(0f, false);
                FadeIn(fadeInDuration);
                break;

            case FadeMode.FadeOutOnly:
                SetImmediateAlpha(1f, true);
                FadeOut(fadeOutDuration);
                break;

            case FadeMode.FadeInAndOut:
                SetImmediateAlpha(0f, false);
                StartFadeInAndOut(fadeInDuration, holdDuration, fadeOutDuration);
                break;

            case FadeMode.None:
                // Leaves current CanvasGroup alpha untouched until triggered via code
                break;
        }
    }

    /* PUBLIC API (Callable from other scripts / buttons) */

    public void FadeIn() => FadeIn(fadeInDuration);
    public void FadeIn(float duration) => StartFade(1f, duration, true);

    public void FadeOut() => FadeOut(fadeOutDuration);
    public void FadeOut(float duration) => StartFade(0f, duration, false);

    public void FadeInAndOut() => StartFadeInAndOut(fadeInDuration, holdDuration, fadeOutDuration);
    public void FadeInAndOut(float inDuration, float hold, float outDuration) => 
        StartFadeInAndOut(inDuration, hold, outDuration);

    /* INTERNAL COROUTINES */

    private void SetImmediateAlpha(float alpha, bool interactable)
    {
        canvasGroup.alpha = alpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }

    private void StartFade(float targetAlpha, float duration, bool enableInteraction)
    {
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration, enableInteraction));
    }

    private void StartFadeInAndOut(float inDuration, float hold, float outDuration)
    {
        if (currentFadeRoutine != null) StopCoroutine(currentFadeRoutine);
        currentFadeRoutine = StartCoroutine(FadeInAndOutRoutine(inDuration, hold, outDuration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool enableInteraction)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        // Block clicks during active transitions
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = enableInteraction;
        canvasGroup.interactable = enableInteraction;
        currentFadeRoutine = null;
    }

    private IEnumerator FadeInAndOutRoutine(float inDuration, float hold, float outDuration)
    {
        // 1. Fade In
        yield return FadeRoutine(1f, inDuration, true);

        // 2. Hold on screen
        yield return new WaitForSecondsRealtime(hold);

        // 3. Fade Out
        yield return FadeRoutine(0f, outDuration, false);
    }
}