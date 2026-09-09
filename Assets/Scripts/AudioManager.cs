using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private Slider musicSlider;

    private const string MUSIC_VOL_PARAM = "MusicVol";
    private const string MUSIC_PREF_KEY = "SavedMusicVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load saved volume (default to 0.75 if no save exists)
        float savedVolume = PlayerPrefs.GetFloat(MUSIC_PREF_KEY, 0.75f);

        if (musicSlider != null)
        {
            musicSlider.value = savedVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        SetMusicVolume(savedVolume);
    }

    public void SetMusicVolume(float sliderValue)
    {
        // Slider value ranges from 0.0001 to 1. Log10 maps this smoothly to decibels (-80 dB to 0 dB)
        float clampedValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
        float decibels = Mathf.Log10(clampedValue) * 20f;

        mainMixer.SetFloat(MUSIC_VOL_PARAM, decibels);

        // Save preference
        PlayerPrefs.SetFloat(MUSIC_PREF_KEY, sliderValue);
    }
}