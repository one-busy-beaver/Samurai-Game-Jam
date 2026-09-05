using UnityEngine;
using UnityEngine.EventSystems;


public class QuitMenuController : MonoBehaviour
{
    // MUST be public so the button can find it
    public void OnExitClick()
    {
        // output to the console
        Debug.Log("Pressed Quit");

        // actually quit
        // Application.Quit();

        // This will stop Play Mode inside the Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

    }
}
