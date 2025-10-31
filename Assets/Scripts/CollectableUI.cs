using UnityEngine;
using TMPro;

public class CollectableUI : MonoBehaviour
{
    public static CollectableUI instance;

    [SerializeField] private TextMeshProUGUI collectableText;

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

    private void Start()
    {
        if (LevelManager.instance != null)
        {
            UpdateDisplay(0, LevelManager.instance.TotalCollectables);
        }
    }

    public void UpdateDisplay(int current, int total)
    {
        if (collectableText != null)
        {
            collectableText.text = $"{current}/{total}";
        }
    }
}