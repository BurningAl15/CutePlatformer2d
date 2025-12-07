using UnityEngine;

public class FinishSpline : MonoBehaviour
{
    [SerializeField] private ObjectAction objectAction;


    public void Finish()
    {
        objectAction.FinishAnimation();
    }
}
