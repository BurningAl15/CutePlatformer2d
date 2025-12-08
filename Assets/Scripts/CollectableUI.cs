using UnityEngine;
using TMPro;

public class CollectableUI : MonoBehaviour
{
    public static CollectableUI instance;

    [Header("Stars (Main Counter)")]
    [SerializeField] private TextMeshProUGUI starCounterText;

    [Header("Optional Counters")]
    [SerializeField] private TextMeshProUGUI orbCounterText;
    [SerializeField] private TextMeshProUGUI badgeCounterText;

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

        AutoFindTextComponents();
    }

    private void AutoFindTextComponents()
    {
        if (starCounterText == null)
        {
            GameObject textObj = GameObject.Find("Canvas/CollectableContainer/Text (TMP)");
            if (textObj != null)
            {
                starCounterText = textObj.GetComponent<TextMeshProUGUI>();
                Debug.Log("[CollectableUI] Auto-found star counter text");
            }
            else
            {
                Debug.LogWarning("[CollectableUI] Could not find star counter text!");
            }
        }
    }

    private void Start()
    {
        if (LevelManager.instance != null)
        {
            UpdateDisplay(0, LevelManager.instance.TotalStars);
        }
    }

    public void UpdateDisplay(int current, int total)
    {
        if (starCounterText != null)
        {
            starCounterText.text = $"{current}/{total}";
        }
        else
        {
            Debug.LogWarning("[CollectableUI] Star counter text is null!");
        }
    }

    public void UpdateOrbCounter(int current, int total)
    {
        if (orbCounterText != null)
        {
            orbCounterText.text = $"{current}/{total}";
        }
    }

    public void UpdateBadgeCounter(int current, int total)
    {
        if (badgeCounterText != null)
        {
            badgeCounterText.text = $"{current}/{total}";
        }
    }

    public void UpdateAllCounters()
    {
        if (LevelManager.instance == null) return;

        UpdateDisplay(
            LevelManager.instance.CollectedStars,
            LevelManager.instance.TotalStars
        );

        UpdateOrbCounter(
            LevelManager.instance.GetCollectedByType("orb"),
            LevelManager.instance.GetTotalByType("orb")
        );

        UpdateBadgeCounter(
            LevelManager.instance.GetCollectedByType("badge"),
            LevelManager.instance.GetTotalByType("badge")
        );
    }
}
