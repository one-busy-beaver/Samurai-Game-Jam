using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseController : MonoBehaviour
{
    public GameObject container;

    private bool isPaused = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) // listening for "Esc"
        {
            TogglePause();   
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        container.SetActive(isPaused); // toggles GameObject visibility
        Time.timeScale = isPaused ? 0 : 1;
    }

    public void OnClickResume()
    {
        TogglePause();
    }

    public void OnClickMainMenu()
    {
        // when player clicks MainMenu
        SceneManager.LoadScene("MainMenu");
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

    // public void OnClickSave()
}