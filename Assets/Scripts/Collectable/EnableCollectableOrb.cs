using UnityEngine;
using UnityEngine.UI;

public class EnableCollectableOrb : MonoBehaviour
{
   [SerializeField] private Image image;
   
   public void CollectOrb()
   {
      image.color = Color.white;
   }
}
