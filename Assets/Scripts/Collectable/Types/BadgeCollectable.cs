using UnityEngine;

public class BadgeCollectable : CollectableBase
{
    [Header("Badge Specific")]
    [SerializeField] private string badgeID;
    [SerializeField] private Sprite badgeIcon;
    [SerializeField] private bool isRare = false;

    protected override void OnBeforeCollect()
    {
        base.OnBeforeCollect();
        
        if (isRare)
        {
            Debug.Log($"[BadgeCollectable] RARE badge collected: {badgeID}");
        }
        else
        {
            Debug.Log($"[BadgeCollectable] Badge collected: {badgeID}");
        }
    }
}