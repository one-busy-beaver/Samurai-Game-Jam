using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    // MUST be public so the button can find it
    [SerializeField] private string gameplaySceneName = "SampleScene";
    [SerializeField] private float transitionDuration = 1f;

    public void OnStartClick()
    {
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeAndLoad(gameplaySceneName, transitionDuration);
        }
        else
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
    public void OnExitClick()
    {
        // output to the console
        Debug.Log("Pressed Quit");

        // This will stop Play Mode inside the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        // actually quit
        // Application.Quit();
    }

    public void OnOptions()
    {
        // shit here
    }

    public void OnCredits()
    {
        // names of everyone!!!
        
    }
}
