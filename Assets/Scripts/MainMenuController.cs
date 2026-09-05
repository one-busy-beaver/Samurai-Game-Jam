using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
    // MUST be public so the button can find it

    public void OnStartClick()
    {
        SceneManager.LoadScene("SampleScene");
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


}
