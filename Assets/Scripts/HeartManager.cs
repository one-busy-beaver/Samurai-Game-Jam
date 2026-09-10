using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartManager : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Prefab & Container")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartContainer; // can be this.transform

    private List<Image> heartImages = new List<Image>(); // containes hearts displayed on screen

    // call once at game start with the player's max health (which is 5)
    public void InitializeHearts(int maxHealth) // called in PlayerControls
    {
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }
        heartImages.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            Image img = newHeart.GetComponent<Image>();
            img.sprite = fullHeartSprite;
            heartImages.Add(img);
        }
    }

    // call whenever health changes via damage or healing
    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < heartImages.Count; i++)
        {
            // if heart index < current health, show full. Otherwise, empty
            if (i < currentHealth)
            {
                heartImages[i].sprite = fullHeartSprite;
            }
            else
            {
                heartImages[i].sprite = emptyHeartSprite;
            }
        }
    }
}