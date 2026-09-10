using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    // MUST be public so the button can find it
    [SerializeField] private string gameplaySceneName = "PreBattleScene";
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private DialogueTypewriter narrativeScreen;
    [SerializeField] private TextAsset introductionLore;

    public void OnStartClick()
    {
        if (narrativeScreen != null && introductionLore != null)
        {
            narrativeScreen.PlayNarrative(introductionLore, () => 
            {
                Debug.Log("Intro done!!");
                LoadPreBattleDialogue();
            });
        }
        else
        {
            // fallback if none of the above are assigned
            LoadPreBattleDialogue();
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
        Application.Quit();
    }

    public void OnCredits()
    {
        // names of everyone!!!
        
    }

    private void LoadPreBattleDialogue()
    {
        // The screen is already fully covered by NarrativeOverlay's black backdrop,
        // so loading directly prevents any flash of the menu.
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }
}
