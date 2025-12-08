using UnityEngine;

public class StarCollectableTarget : MonoBehaviour
{
    public static StarCollectableTarget Instance;
    public Transform Target;
    
    
    private void Awake()
    {
        Instance = this;
        Target = this.transform;
    }
}
