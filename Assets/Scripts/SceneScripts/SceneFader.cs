using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [SerializeField] private Image image;
    [SerializeField] private float defaultFadeDuration = 1f;
    [SerializeField] private bool fadeInOnStart = false; // Check this ONLY if you want the scene to fade in from black

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (fadeInOnStart)
        {
            StartCoroutine(FadeInRoutine(defaultFadeDuration));
        }
        else
        {
            // Ensure the screen starts completely clear
            Color c = image.color;
            c.a = 0f;
            image.color = c;
            image.raycastTarget = false;
        }
    }

    public void FadeAndLoad(string sceneName, float duration)
    {
        StartCoroutine(FadeOutRoutine(sceneName, duration));
    }

    public void FadeAndLoad(string sceneName)
    {
        StartCoroutine(FadeOutRoutine(sceneName, defaultFadeDuration));
    }

    private IEnumerator FadeInRoutine(float duration)
    {
        image.raycastTarget = true;
        float t = 0f;
        Color c = image.color;
        c.a = 1f;
        image.color = c;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(1f - (t / duration));
            image.color = c;
            yield return null;
        }

        c.a = 0f;
        image.color = c;
        image.raycastTarget = false;
    }

    private IEnumerator FadeOutRoutine(string sceneName, float duration)
    {
        image.raycastTarget = true;
        float t = 0f;
        Color c = image.color;
        c.a = 0f;
        image.color = c;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(t / duration);
            image.color = c;
            yield return null;
        }

        c.a = 1f;
        image.color = c;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}