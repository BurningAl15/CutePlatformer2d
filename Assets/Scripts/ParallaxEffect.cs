using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Header("Parallax Layers")]
    [SerializeField] private ParallaxLayer[] layers;

    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        [Range(0f, 1f)] public float parallaxMultiplier = 0.5f;
    }

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    private void FixedUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        foreach (ParallaxLayer layer in layers)
        {
            if (layer.layerTransform != null)
            {
                Vector3 newPosition = layer.layerTransform.position + new Vector3(
                    deltaMovement.x * layer.parallaxMultiplier,
                    deltaMovement.y * layer.parallaxMultiplier,
                    0f
                );

                layer.layerTransform.position = newPosition;
            }
        }

        lastCameraPosition = cameraTransform.position;
    }
}