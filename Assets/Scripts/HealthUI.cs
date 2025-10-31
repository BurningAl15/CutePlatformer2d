using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    public static HealthUI instance;

    [SerializeField] private Sprite fullLifeSprite;
    [SerializeField] private Sprite emptyLifeSprite;
    [SerializeField] private List<Image> lifeIcons = new List<Image>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateDisplay(int currentLives, int maxLives)
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (i < currentLives)
            {
                if (fullLifeSprite != null)
                {
                    lifeIcons[i].sprite = fullLifeSprite;
                }
                lifeIcons[i].enabled = true;
            }
            else
            {
                if (emptyLifeSprite != null)
                {
                    lifeIcons[i].sprite = emptyLifeSprite;
                }
                else
                {
                    lifeIcons[i].enabled = false;
                }
            }
        }
    }
}