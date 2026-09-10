using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    [SerializeField] public GameObject container;
    [SerializeField] public GameObject optionsPanel;

    public static bool IsGamePaused { get; private set; }

    private void Awake()
    {
        IsGamePaused = false;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) // listening for "Esc"
        {
            TogglePause();   
        }
    }

    private void TogglePause()
    {
        IsGamePaused = !IsGamePaused;
        container.SetActive(IsGamePaused); // toggles GameObject visibility

        if (!IsGamePaused && optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        Time.timeScale = IsGamePaused ? 0 : 1;
    }

    public void OnClickResume()
    {
        TogglePause();
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1;

        IsGamePaused = false;

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeAndLoad("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
        // when player clicks MainMenu
        SceneManager.LoadScene("MainMenu");
    }

    public void OnClickOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }
    }

    public void BackToPauseMenu()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(!optionsPanel.activeSelf);
        }
    }

    public void OnClickQuit()
    {
        // quit never saves
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        // actually quits
        Application.Quit();
    }

}