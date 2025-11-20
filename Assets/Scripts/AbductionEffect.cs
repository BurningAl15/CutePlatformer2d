using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AbductionEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform beamRoot;
    [SerializeField] private Transform beamMask;
    [SerializeField] private Light2D beamLight;
    [SerializeField] private Light2D globalLight;

    [Header("Raycast")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxRaycastDistance = 50f;
    [SerializeField] private float groundOffset = 0.5f;

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve beamDeployCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve beamRetractCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve lightFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Timing")]
    [SerializeField] private float normalDeployDuration = 2f;
    [SerializeField] private float quickDeployDuration = 0.3f;
    [SerializeField] private float retractDuration = 0.8f;
    [SerializeField] private float lightFadeDuration = 0.5f;
    [SerializeField] private float waitAfterLanding = 1f;

    [Header("Lighting")]
    [SerializeField] private float beamLightIntensity = 1f;
    [SerializeField] private float darkIntensity = 0.05f;
    [SerializeField] private float normalIntensity = 0.65f;

    private float beamMinY = 0f;
    private float beamMaxY;
    private float maskMinScale = 0f;
    private float maskMaxScale;
    private Vector3 groundPosition;

    public void Initialize()
    {
        ResetBeamVisuals();
        beamLight.intensity = 0f;
    }

    public IEnumerator PlayLandingSequence()
    {
        Debug.Log("[AbductionEffect] === LANDING SEQUENCE START ===");

        MoveToPlayerX();
        CalculateBeamLimits();
        Player2DController.instance.BeAbducted();

        SetLightImmediate(beamLight, 0f);
        SetLightImmediate(globalLight, darkIntensity);

        yield return FadeLight(beamLight, 0f, beamLightIntensity, lightFadeDuration);

        yield return DeployBeam(normalDeployDuration, followPlayer: true);

        Player2DController.instance.ReleaseFromAbduction();

        yield return FadeLight(globalLight, darkIntensity, normalIntensity, lightFadeDuration);
        yield return new WaitForSeconds(waitAfterLanding);
        
        yield return FadeLight(beamLight, beamLightIntensity, 0f, lightFadeDuration);
        yield return RetractBeam(retractDuration);

        Debug.Log("[AbductionEffect] === LANDING SEQUENCE END ===");
    }

    public IEnumerator PlayAbductionSequence()
    {
        Debug.Log("[AbductionEffect] === ABDUCTION SEQUENCE START ===");

        MoveToPlayerX();
        CalculateBeamLimits();

        yield return DeployBeam(quickDeployDuration, followPlayer: false);

        Player2DController.instance.SetPosition(groundPosition);
        Player2DController.instance.BeAbducted();

        yield return RetractBeam(retractDuration, pullPlayer: true);

        Debug.Log("[AbductionEffect] === ABDUCTION SEQUENCE END ===");
    }

    public IEnumerator PlayRespawnSequence(Transform waypoint)
    {
        Debug.Log($"[AbductionEffect] === RESPAWN SEQUENCE START === Waypoint: {waypoint.name}");

        Player2DController.instance.BeAbducted();
        MoveToPlayerX();
        CalculateBeamLimits();

        Player2DController.instance.SetPosition(groundPosition);
        beamRoot.localPosition = new Vector3(0, beamMaxY, 0);
        beamMask.localScale = new Vector3(beamMask.localScale.x, maskMaxScale, beamMask.localScale.z);

        SetLightImmediate(beamLight, 0f);
        SetLightImmediate(globalLight, darkIntensity);

        yield return FadeLight(beamLight, 0f, beamLightIntensity, lightFadeDuration);

        yield return RetractBeam(retractDuration, pullPlayer: true);

        MoveToX(waypoint.position.x);
        Vector3 waypointSkyPos = new Vector3(waypoint.position.x, waypoint.position.y + 15f, waypoint.position.z);
        Player2DController.instance.SetPosition(waypointSkyPos);
        CalculateBeamLimits(waypointSkyPos);

        yield return DeployBeam(normalDeployDuration, followPlayer: true);

        Player2DController.instance.ReleaseFromAbduction();

        yield return FadeLight(globalLight, darkIntensity, normalIntensity, lightFadeDuration);
        yield return new WaitForSeconds(waitAfterLanding);

        yield return FadeLight(beamLight, beamLightIntensity, 0f, lightFadeDuration);
        yield return RetractBeam(retractDuration);

        Debug.Log("[AbductionEffect] === RESPAWN SEQUENCE END ===");
    }

    public IEnumerator PlayVictorySequence()
    {
        Debug.Log("[AbductionEffect] === VICTORY SEQUENCE START ===");

        MoveToPlayerX();
        CalculateBeamLimits();

        SetLightImmediate(beamLight, 0f);
        SetLightImmediate(globalLight, darkIntensity);

        yield return FadeLight(beamLight, 0f, beamLightIntensity, lightFadeDuration);

        yield return DeployBeam(quickDeployDuration, followPlayer: false);

        Player2DController.instance.SetPosition(groundPosition);
        Player2DController.instance.BeAbducted();

        yield return RetractBeam(retractDuration, pullPlayer: true);

        Debug.Log("[AbductionEffect] === VICTORY SEQUENCE END ===");
    }

    private IEnumerator DeployBeam(float duration, bool followPlayer)
    {
        Debug.Log($"[AbductionEffect] Deploying beam - Duration: {duration:F2}s, FollowPlayer: {followPlayer}");

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curve = beamDeployCurve.Evaluate(t);

            float currentY = Mathf.Lerp(beamMinY, beamMaxY, curve);
            beamRoot.localPosition = new Vector3(0, currentY, 0);

            float currentScale = Mathf.Lerp(maskMinScale, maskMaxScale, curve);
            beamMask.localScale = new Vector3(beamMask.localScale.x, currentScale, beamMask.localScale.z);

            if (followPlayer && Player2DController.instance != null)
            {
                Player2DController.instance.SetPosition(beamRoot.position);
            }

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        beamRoot.localPosition = new Vector3(0, beamMaxY, 0);
        beamMask.localScale = new Vector3(beamMask.localScale.x, maskMaxScale, beamMask.localScale.z);

        if (followPlayer && Player2DController.instance != null)
        {
            Player2DController.instance.SetPosition(groundPosition);
        }

        Debug.Log("[AbductionEffect] Beam deployed");
    }

    private IEnumerator RetractBeam(float duration, bool pullPlayer = false)
    {
        Debug.Log($"[AbductionEffect] Retracting beam - Duration: {duration:F2}s, PullPlayer: {pullPlayer}");

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curve = beamRetractCurve.Evaluate(t);

            float currentY = Mathf.Lerp(beamMaxY, beamMinY, curve);
            beamRoot.localPosition = new Vector3(0, currentY, 0);

            float currentScale = Mathf.Lerp(maskMaxScale, maskMinScale, curve);
            beamMask.localScale = new Vector3(beamMask.localScale.x, currentScale, beamMask.localScale.z);

            if (pullPlayer && Player2DController.instance != null)
            {
                Player2DController.instance.SetPosition(beamRoot.position);
            }

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        beamRoot.localPosition = new Vector3(0, beamMinY, 0);
        beamMask.localScale = new Vector3(beamMask.localScale.x, maskMinScale, beamMask.localScale.z);

        Debug.Log("[AbductionEffect] Beam retracted");
    }

    private IEnumerator FadeLight(Light2D light, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curve = lightFadeCurve.Evaluate(t);
            light.intensity = Mathf.Lerp(from, to, curve);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        light.intensity = to;
    }

    private void SetLightImmediate(Light2D light, float intensity)
    {
        light.intensity = intensity;
    }

    private void CalculateBeamLimits(Vector3? origin = null)
    {
        Vector3 rayOrigin = origin ?? beamRoot.position;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, maxRaycastDistance, groundLayer);

        if (hit.collider != null)
        {
            float distance = hit.distance - groundOffset;
            beamMaxY = -distance;
            maskMaxScale = distance;
            groundPosition = new Vector3(rayOrigin.x, hit.point.y + groundOffset, rayOrigin.z);

            Debug.Log($"[AbductionEffect] Beam limits: MaxY={beamMaxY:F2}, MaxScale={maskMaxScale:F2}, Ground={groundPosition}");
        }
        else
        {
            Debug.LogWarning("[AbductionEffect] No ground detected, using defaults");
            beamMaxY = -10f;
            maskMaxScale = 10f;
            groundPosition = new Vector3(rayOrigin.x, rayOrigin.y - 10f, rayOrigin.z);
        }
    }

    private void MoveToPlayerX()
    {
        if (Player2DController.instance != null)
        {
            MoveToX(Player2DController.instance.GetPosition().x);
        }
    }

    private void MoveToX(float targetX)
    {
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }

    private void ResetBeamVisuals()
    {
        beamRoot.localPosition = new Vector3(0, beamMinY, 0);
        beamMask.localScale = new Vector3(beamMask.localScale.x, maskMinScale, beamMask.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (beamRoot == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(beamRoot.position, beamRoot.position + Vector3.down * maxRaycastDistance);

        RaycastHit2D hit = Physics2D.Raycast(beamRoot.position, Vector2.down, maxRaycastDistance, groundLayer);

        if (hit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hit.point, 0.3f);

            Vector3 targetPoint = hit.point + Vector2.up * groundOffset;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPoint, 0.5f);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Landing")]
    public void TestLanding()
    {
        if (Application.isPlaying)
            StartCoroutine(PlayLandingSequence());
    }

    [ContextMenu("Test Abduction")]
    public void TestAbduction()
    {
        if (Application.isPlaying)
            StartCoroutine(PlayAbductionSequence());
    }
#endif
}
