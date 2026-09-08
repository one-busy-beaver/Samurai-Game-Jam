using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DeathScreenController : MonoBehaviour
{
    [SerializeField] private GameObject promptText;
    [SerializeField] private string targetSceneName = "MainMenu"; // TODO: levels
    [SerializeField] private UIFader deathPanelFader;
    [SerializeField] private float promptDelay = 5f;
    [SerializeField] private float fadeDuration = 1f;

    private bool canRestart = false;
    private bool isTransitioning = false;

    void Start()
    {
        if (promptText != null) // hide text
        {
            promptText.SetActive(false);
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

    public void OnPlayerDefeated() // I'm getting an error :(((
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
            promptText.SetActive(true);
        }

        canRestart = true;
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