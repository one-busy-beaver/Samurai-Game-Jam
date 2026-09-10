using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [SerializeField] List<GameObject> waveGroups;
    [SerializeField] float delayBetweenWaves = 1f;

    [Header("End of Waves Scene Transition")]
    [SerializeField] private string nextSceneName = "GoodEnd";
    [SerializeField] private float transitionDuration = 1f;

    void Start()
    {
        // Waves are left active in the editor for visibility, so hide them all first
        for (int i = 0; i < waveGroups.Count; i++)
            waveGroups[i].SetActive(false);

        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        for (int i = 0; i < waveGroups.Count; i++)
        {
            GameObject group = waveGroups[i];
            group.SetActive(true);

            // Shooters destroy themselves once they walk back off screen,
            // so the wave is over when none are left alive in the group.
            yield return new WaitUntil(() => CountShooters(group) == 0);

            group.SetActive(false);
            yield return new WaitForSeconds(delayBetweenWaves);
        }

        // Trigger scene transition once all waves are cleared
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.FadeAndLoad(nextSceneName, transitionDuration);
            }
            else
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    // Includes inactive children so a shooter disabled mid-wave still counts as alive
    int CountShooters(GameObject group)
    {
        return group.GetComponentsInChildren<Shooter>(true).Length;
    }
}