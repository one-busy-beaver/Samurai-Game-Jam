using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class DeathScreenController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string targetSceneName = "MainMenu"; // TODO: levels
    [SerializeField] private UIFader deathPanelFader;
    [SerializeField] private float promptDelay = 2f;
    [SerializeField] private float fadeDuration = 1f;

    private float flashMultiplier = 2f;
    SpriteRenderer sr;
    Color32 yellow = new Color32(255, 242, 121, 255);
    Color32 transparent = new Color32(255, 242, 121, 0);

    private bool canRestart = false;
    private bool isTransitioning = false;

    void Start()
    {
        if (promptText != null) // hide text
        {
            promptText.gameObject.SetActive(false);
        }

        StartCoroutine(ShowPrompt());
    }

    void Update()
    {
        if (canRestart && !isTransitioning)
        {
            if(Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                TriggerTransition();
            }
        }
    }

    public void OnPlayerDefeated()
    {
        if (deathPanelFader != null)
        {
            deathPanelFader.FadeIn(2f);
        }
    }

    private IEnumerator ShowPrompt()
    {
        yield return new WaitForSecondsRealtime(promptDelay);

        if (promptText != null) // show text
        {
            promptText.gameObject.SetActive(true);
        }

        canRestart = true;
        
        // flashing text
        float timer = 0f;
        while (!isTransitioning)
        {
            timer += Time.unscaledDeltaTime;
            float flashT = Mathf.PingPong(timer * flashMultiplier, 1f);

            if (promptText != null)
            {
                promptText.color = Color.Lerp(yellow, transparent, flashT);
            }

            yield return null;
        }
    }

    private void TriggerTransition()
    {
        isTransitioning = true; // prevent multiple key spam triggers

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeAndLoad(targetSceneName, fadeDuration);
        }
        else
        {
            Time.timeScale = 1; // lol, I forgot this last time
            SceneManager.LoadScene(targetSceneName);
        }
    }
}