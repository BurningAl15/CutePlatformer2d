using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AbductionEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform abductionPosition;
    [SerializeField] private Transform abductionMask;
    [SerializeField] private Light2D abductionLight;
    [SerializeField] private Light2D globalLight;

    [Header("Raycast Configuration")]
    [SerializeField] private LayerMask groundAndPlayerLayer;
    [SerializeField] private float maxRaycastDistance = 50f;
    [SerializeField] private float groundOffset = 0.5f;
    
    [Header("Timeline - Absolute Start Times")]
    [SerializeField] private float abductionLightUpStartTime = 0f;
    [SerializeField] private float landingStartTime = 0.5f;
    [SerializeField] private float globalLightStartTime = 2.2f;
    [SerializeField] private float waitStartTime = 2.5f;
    [SerializeField] private float abductionLightDownStartTime = 3.5f;
    [SerializeField] private float retractionStartTime = 4.0f;
    
    [Header("Animation Settings")]
    [SerializeField] private float landingDuration = 2f;
    [SerializeField] private float retractionDuration = 0.8f;
    [SerializeField] private float quickLandingDuration = 0.3f;
    [SerializeField] private float scaleMultiplier = 1f;
    [SerializeField] private AnimationCurve animCurve_Land;
    [SerializeField] private AnimationCurve animCurve_Abduction;
    
    [Header("Abduction Light")]
    [SerializeField] private float abductionLightMaxIntensity = 1f;
    [SerializeField] private float abductionLightFadeDuration = 0.5f;
    [SerializeField] private AnimationCurve abductionLightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Global Light Animation")]
    [SerializeField] private bool animateGlobalLight = true;
    [SerializeField] private float darkIntensity = 0.05f;
    [SerializeField] private float normalIntensity = 0.65f;
    [SerializeField] private float globalLightFadeDuration = 0.3f;
    [SerializeField] private AnimationCurve globalLightFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Timing")]
    [SerializeField] private float waitAfterLanding = 1f;
    
    private float yMinScale = 0;
    private float yMaxScale;
    private float yMinPos = 0;
    private float yMaxPos;
    
    private Vector3 groundTargetPosition;

    private Coroutine abductionLightCoroutine;
    private Coroutine globalLightCoroutine;
    private Coroutine landingAnimCoroutine;
    private Coroutine retractionAnimCoroutine;

    public void CalculateAbductionLimits(Vector3? targetPosition = null)
    {
        Vector3 rayOrigin = targetPosition ?? abductionPosition.position;
        
        Debug.Log($"[AbductionEffect] Calculating limits from: {rayOrigin}");
        
        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin, 
            Vector2.down, 
            maxRaycastDistance, 
            groundAndPlayerLayer
        );

        if (hit.collider != null)
        {
            float distanceToGround = hit.distance;
            float distanceToTarget = distanceToGround - groundOffset;
            
            yMaxScale = distanceToTarget * scaleMultiplier;
            yMaxPos = -distanceToTarget;
            
            groundTargetPosition = new Vector3(rayOrigin.x, hit.point.y + groundOffset, rayOrigin.z);
            
            Debug.Log($"[AbductionEffect] Hit: {hit.collider.name} | yMaxPos: {yMaxPos:F2} | yMaxScale: {yMaxScale:F2} | Ground: {groundTargetPosition}");
        }
        else
        {
            Debug.LogWarning("[AbductionEffect] No ground found - using defaults");
            yMaxScale = 10f;
            yMaxPos = -10f;
            groundTargetPosition = new Vector3(rayOrigin.x, rayOrigin.y - 10f, rayOrigin.z);
        }
    }
    
    private void MoveToX(float targetX)
    {
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        Debug.Log($"[AbductionEffect] Moved instantly to X: {targetX}");
    }
    
    private IEnumerator AnimateLanding(AnimationCurve curve, float duration, bool holdPlayer)
    {
        Debug.Log($"[AbductionEffect] LANDING Animation - Duration: {duration:F2}s, HoldPlayer: {holdPlayer}");

        float elapsed = 0;
    
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveValue = curve.Evaluate(t);
        
            float currentY = Mathf.Lerp(yMinPos, yMaxPos, curveValue);
            abductionPosition.localPosition = new Vector3(0, currentY, 0);
        
            float currentScale = Mathf.Lerp(yMinScale, yMaxScale, curveValue);
            abductionMask.localScale = new Vector3(abductionMask.localScale.x, currentScale, abductionMask.localScale.z);
        
            if (holdPlayer && Player2DController.instance != null)
            {
                Player2DController.instance.transform.position = abductionPosition.position;
            }
        
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    
        abductionPosition.localPosition = new Vector3(0, yMaxPos, 0);
        abductionMask.localScale = new Vector3(abductionMask.localScale.x, yMaxScale, abductionMask.localScale.z);
    
        if (holdPlayer && Player2DController.instance != null)
        {
            Player2DController.instance.transform.position = groundTargetPosition;
            Debug.Log($"[AbductionEffect] LANDING complete - Player at: {groundTargetPosition}");
        }
    }

    private IEnumerator AnimatePickup(AnimationCurve curve, float duration, bool holdPlayer)
    {
        Debug.Log($"[AbductionEffect] PICKUP Animation - Duration: {duration:F2}s, HoldPlayer: {holdPlayer}");

        float elapsed = 0;
    
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveValue = curve.Evaluate(t);
        
            float currentY = Mathf.Lerp(yMaxPos, yMinPos, curveValue);
            abductionPosition.localPosition = new Vector3(0, currentY, 0);
        
            float currentScale = Mathf.Lerp(yMaxScale, yMinScale, curveValue);
            abductionMask.localScale = new Vector3(abductionMask.localScale.x, currentScale, abductionMask.localScale.z);
        
            if (holdPlayer && Player2DController.instance != null)
            {
                Player2DController.instance.transform.position = abductionPosition.position;
            }
        
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    
        abductionPosition.localPosition = new Vector3(0, yMinPos, 0);
        abductionMask.localScale = new Vector3(abductionMask.localScale.x, yMinScale, abductionMask.localScale.z);
    
        Debug.Log($"[AbductionEffect] PICKUP complete - Mask at 0");
    }

    private void CapturePlayer()
    {
        if (Player2DController.instance == null) return;
        
        Rigidbody2D playerRb = Player2DController.instance.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.bodyType = RigidbodyType2D.Kinematic;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
        
        Player2DController.instance.CallRespawn(true, Player2DController.instance.transform.position, false);
        Player2DController.instance.ForceReleaseGrabbedObject();
        
        Debug.Log("[AbductionEffect] Player captured and grabbed object released");
    }

    private void ReleasePlayer()
    {
        if (Player2DController.instance == null) return;
        
        Debug.Log($"[AbductionEffect] Releasing player at: {Player2DController.instance.transform.position}");
        
        Rigidbody2D playerRb = Player2DController.instance.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
        
        Player2DController.instance.CallRespawn(false, Player2DController.instance.transform.position, false);
    }

    private void StartAbductionLightFade(float fromIntensity, float toIntensity, float fadeDuration)
    {
        if (abductionLightCoroutine != null)
        {
            StopCoroutine(abductionLightCoroutine);
        }
        abductionLightCoroutine = StartCoroutine(FadeAbductionLight(fromIntensity, toIntensity, fadeDuration));
    }

    private IEnumerator FadeAbductionLight(float fromIntensity, float toIntensity, float fadeDuration)
    {
        Debug.Log($"[AbductionEffect] FadeAbductionLight: {fromIntensity:F2} -> {toIntensity:F2}");
        
        float elapsed = 0;
        
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            float curveValue = abductionLightCurve.Evaluate(t);
            abductionLight.intensity = Mathf.Lerp(fromIntensity, toIntensity, curveValue);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        abductionLight.intensity = toIntensity;
        abductionLightCoroutine = null;
    }

    private void StartGlobalLightFade(float fromIntensity, float toIntensity, float fadeDuration)
    {
        if (globalLightCoroutine != null)
        {
            StopCoroutine(globalLightCoroutine);
        }
        globalLightCoroutine = StartCoroutine(FadeGlobalLight(fromIntensity, toIntensity, fadeDuration));
    }

    private IEnumerator FadeGlobalLight(float fromIntensity, float toIntensity, float fadeDuration)
    {
        Debug.Log($"[AbductionEffect] FadeGlobalLight: {fromIntensity:F2} -> {toIntensity:F2}");
        
        float elapsed = 0;
        
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            float curveValue = globalLightFadeCurve.Evaluate(t);
            globalLight.intensity = Mathf.Lerp(fromIntensity, toIntensity, curveValue);
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        globalLight.intensity = toIntensity;
        globalLightCoroutine = null;
    }

    private void SetGlobalLightImmediate(float intensity)
    {
        globalLight.intensity = intensity;
    }

    private void SetAbductionLightImmediate(float intensity)
    {
        abductionLight.intensity = intensity;
    }

    public void Initialize()
    {
        Debug.Log("[AbductionEffect] Initialize");
        abductionPosition.localPosition = new Vector3(0, yMinPos, 0);
        abductionMask.localScale = new Vector3(abductionMask.localScale.x, yMinScale, abductionMask.localScale.z);
        SetAbductionLightImmediate(0);
    }

    public IEnumerator AbductionAnim_Land()
    {
        Debug.Log("[AbductionEffect] === LAND SEQUENCE START ===");
        
        if (Player2DController.instance != null)
        {
            MoveToX(Player2DController.instance.transform.position.x);
        }
        
        CalculateAbductionLimits();
        SetAbductionLightImmediate(0);
        SetGlobalLightImmediate(darkIntensity);
        
        CapturePlayer();
        
        float elapsedTime = 0f;
        
        bool abductionLightUpStarted = false;
        bool landingStarted = false;
        bool globalLightStarted = false;
        bool waitStarted = false;
        bool abductionLightDownStarted = false;
        bool retractionStarted = false;
        bool playerReleased = false;
        
        float totalDuration = Mathf.Max(
            abductionLightUpStartTime + abductionLightFadeDuration,
            landingStartTime + landingDuration,
            globalLightStartTime + globalLightFadeDuration,
            waitStartTime + waitAfterLanding,
            abductionLightDownStartTime + abductionLightFadeDuration,
            retractionStartTime + retractionDuration
        );
        
        while (elapsedTime < totalDuration)
        {
            if (!abductionLightUpStarted && elapsedTime >= abductionLightUpStartTime)
            {
                StartAbductionLightFade(0, abductionLightMaxIntensity, abductionLightFadeDuration);
                abductionLightUpStarted = true;
            }
            
            if (!landingStarted && elapsedTime >= landingStartTime)
            {
                landingAnimCoroutine = StartCoroutine(AnimateLanding(animCurve_Land, landingDuration, true));
                landingStarted = true;
            }
            
            if (!globalLightStarted && elapsedTime >= globalLightStartTime && animateGlobalLight)
            {
                StartGlobalLightFade(darkIntensity, normalIntensity, globalLightFadeDuration);
                globalLightStarted = true;
            }
            
            if (!playerReleased && landingStarted && elapsedTime >= (landingStartTime + landingDuration))
            {
                ReleasePlayer();
                playerReleased = true;
            }
            
            if (!waitStarted && elapsedTime >= waitStartTime)
            {
                waitStarted = true;
            }
            
            if (!abductionLightDownStarted && elapsedTime >= abductionLightDownStartTime)
            {
                StartAbductionLightFade(abductionLightMaxIntensity, 0, abductionLightFadeDuration);
                abductionLightDownStarted = true;
            }
            
            if (!retractionStarted && elapsedTime >= retractionStartTime)
            {
                retractionAnimCoroutine = StartCoroutine(AnimatePickup(animCurve_Abduction, retractionDuration, false));
                retractionStarted = true;
            }
            
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        
        if (!playerReleased)
        {
            ReleasePlayer();
        }
        
        Debug.Log("[AbductionEffect] === LAND SEQUENCE END ===");
    }

    public IEnumerator AbductionAnim_Abduction(bool holdPlayer)
    {
        Debug.Log($"[AbductionEffect] === ABDUCTION SEQUENCE START (holdPlayer: {holdPlayer}) ===");
        
        if (Player2DController.instance != null)
        {
            MoveToX(Player2DController.instance.transform.position.x);
        }
        
        CalculateAbductionLimits();
        
        Debug.Log("[AbductionEffect] Phase 1: Quick landing (ray deploying)");
        yield return StartCoroutine(AnimateLanding(animCurve_Land, quickLandingDuration, false));
        
        if (holdPlayer && Player2DController.instance != null)
        {
            Debug.Log($"[AbductionEffect] Phase 2: Teleport player to ray position: {groundTargetPosition}");
            Player2DController.instance.transform.position = groundTargetPosition;
            
            CapturePlayer();
        }
        
        Debug.Log("[AbductionEffect] Phase 3: Pickup (pulling player up)");
        yield return StartCoroutine(AnimatePickup(animCurve_Abduction, retractionDuration, holdPlayer));
        
        Debug.Log("[AbductionEffect] === ABDUCTION SEQUENCE END ===");
    }
    
    public IEnumerator AbductionSequence_Respawn(Transform waypoint)
    {
        Debug.Log($"[AbductionEffect] === RESPAWN SEQUENCE START === Waypoint: {waypoint.name}");
        
        CapturePlayer();
        
        if (Player2DController.instance != null)
        {
            MoveToX(Player2DController.instance.transform.position.x);
        }
        
        CalculateAbductionLimits(Player2DController.instance.transform.position);
        
        Player2DController.instance.transform.position = groundTargetPosition;
        abductionPosition.localPosition = new Vector3(0, yMaxPos, 0);
        abductionMask.localScale = new Vector3(abductionMask.localScale.x, yMaxScale, abductionMask.localScale.z);
        
        SetAbductionLightImmediate(0);
        StartAbductionLightFade(0, abductionLightMaxIntensity, abductionLightFadeDuration);
        SetGlobalLightImmediate(darkIntensity);
        
        yield return new WaitForSeconds(abductionLightFadeDuration);
        
        Debug.Log("[AbductionEffect] Phase 1: Pickup player");
        yield return StartCoroutine(AnimatePickup(animCurve_Abduction, retractionDuration, true));
        
        Debug.Log($"[AbductionEffect] Phase 2: Teleport to waypoint X: {waypoint.position.x}");
        MoveToX(waypoint.position.x);
        
        Vector3 teleportPos = new Vector3(waypoint.position.x, waypoint.position.y + 15f, waypoint.position.z);
        Player2DController.instance.transform.position = teleportPos;
        
        CalculateAbductionLimits(teleportPos);
        
        Debug.Log("[AbductionEffect] Phase 3: Landing at waypoint");
        yield return StartCoroutine(AnimateLanding(animCurve_Land, landingDuration, true));
        
        Debug.Log($"[AbductionEffect] Player at: {Player2DController.instance.transform.position}");
        
        ReleasePlayer();
        
        StartGlobalLightFade(darkIntensity, normalIntensity, globalLightFadeDuration);
        
        Debug.Log($"[AbductionEffect] Phase 4: Wait {waitAfterLanding:F2}s");
        yield return new WaitForSeconds(waitAfterLanding);
        
        Debug.Log("[AbductionEffect] Phase 5: Fade out light");
        StartAbductionLightFade(abductionLightMaxIntensity, 0, abductionLightFadeDuration);
        yield return new WaitForSeconds(abductionLightFadeDuration);
        
        Debug.Log("[AbductionEffect] Phase 6: Retraction");
        yield return StartCoroutine(AnimatePickup(animCurve_Abduction, retractionDuration, false));
        
        Debug.Log("[AbductionEffect] === RESPAWN SEQUENCE END ===");
    }
    
    public IEnumerator AbductionSequence_Victory()
    {
        Debug.Log("[AbductionEffect] === VICTORY SEQUENCE START ===");
        
        if (Player2DController.instance != null)
        {
            MoveToX(Player2DController.instance.transform.position.x);
        }
        
        CalculateAbductionLimits(Player2DController.instance.transform.position);
        
        SetAbductionLightImmediate(0);
        StartAbductionLightFade(0, abductionLightMaxIntensity, abductionLightFadeDuration);
        SetGlobalLightImmediate(darkIntensity);
        
        yield return new WaitForSeconds(abductionLightFadeDuration);
        
        Debug.Log("[AbductionEffect] Quick landing (deploying ray)");
        yield return StartCoroutine(AnimateLanding(animCurve_Land, quickLandingDuration, false));
        
        if (Player2DController.instance != null)
        {
            Debug.Log($"[AbductionEffect] Teleport player to ray position: {groundTargetPosition}");
            Player2DController.instance.transform.position = groundTargetPosition;
            CapturePlayer();
        }
        
        Debug.Log("[AbductionEffect] Pickup (pulling player up)");
        yield return StartCoroutine(AnimatePickup(animCurve_Abduction, retractionDuration, true));
        
        Debug.Log("[AbductionEffect] === VICTORY SEQUENCE END ===");
    }

    private void OnDrawGizmosSelected()
    {
        if (abductionPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                abductionPosition.position, 
                abductionPosition.position + Vector3.down * maxRaycastDistance
            );
            
            RaycastHit2D hit = Physics2D.Raycast(
                abductionPosition.position, 
                Vector2.down, 
                maxRaycastDistance, 
                groundAndPlayerLayer
            );
            
            if (hit.collider != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hit.point, 0.3f);
                
                Vector3 targetPoint = hit.point + Vector2.up * groundOffset;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(targetPoint, 0.5f);
                Gizmos.DrawLine(abductionPosition.position, targetPoint);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Landing Animation")]
    public void EditorTestLanding()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(AbductionAnim_Land());
        }
    }

    [ContextMenu("Test Abduction Animation")]
    public void EditorTestAbduction()
    {
        if (Application.isPlaying)
        {
            StartCoroutine(AbductionAnim_Abduction(true));
        }
    }
#endif
}
