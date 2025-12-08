using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CollectableMoveToUIEffect : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 0.8f;
    [SerializeField] private Ease moveEase = Ease.InOutQuad;
    
    [SerializeField] private bool useScale = true;
    [SerializeField] private float targetScale = 0.5f;
    [SerializeField] private Ease scaleEase = Ease.InBack;
    
    [SerializeField] private bool useFade = false;
    [SerializeField] private float targetAlpha = 0f;

    [Header("Follow Settings")]
    [SerializeField] private bool followTarget = true;
    [SerializeField] private float followSpeed = 12f;
    
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private bool isMovingToTarget = false;
    private Transform currentTarget;

    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public IEnumerator MoveToTarget(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("[CollectableMoveToUIEffect] Target is null");
            yield break;
        }

        currentTarget = targetTransform;

        if (followTarget)
        {
            yield return MoveToTargetDynamic();
        }
        else
        {
            yield return MoveToTargetFixed();
        }
    }

    private IEnumerator MoveToTargetDynamic()
    {
        isMovingToTarget = true;
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);
            float easedT = DOVirtual.EasedValue(0f, 1f, t, moveEase);

            if (currentTarget != null)
            {
                Vector3 targetWorldPosition = GetUIWorldPosition(currentTarget);
                transform.position = Vector3.Lerp(startPosition, targetWorldPosition, easedT);
            }

            if (useScale)
            {
                float scaleT = DOVirtual.EasedValue(0f, 1f, t, scaleEase);
                transform.localScale = Vector3.Lerp(startScale, Vector3.one * targetScale, scaleT);
            }

            if (useFade && spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, targetAlpha, t);
                spriteRenderer.color = color;
            }

            yield return null;
        }

        if (currentTarget != null)
        {
            transform.position = GetUIWorldPosition(currentTarget);
        }

        if (useScale)
        {
            transform.localScale = Vector3.one * targetScale;
        }

        isMovingToTarget = false;
    }

    private IEnumerator MoveToTargetFixed()
    {
        Vector3 targetWorldPosition = GetUIWorldPosition(currentTarget);

        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(transform.DOMove(targetWorldPosition, moveDuration).SetEase(moveEase));
        
        if (useScale)
        {
            sequence.Join(transform.DOScale(targetScale, moveDuration).SetEase(scaleEase));
        }
        
        if (useFade && spriteRenderer != null)
        {
            sequence.Join(spriteRenderer.DOFade(targetAlpha, moveDuration));
        }

        yield return sequence.WaitForCompletion();
    }

    public IEnumerator DefaultCollectAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        
        sequence.Append(transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutQuad));
        sequence.Append(transform.DOScale(0f, 0.25f).SetEase(Ease.InBack));
        
        if (spriteRenderer != null)
        {
            sequence.Join(spriteRenderer.DOFade(0f, 0.25f).SetDelay(0.15f));
        }

        yield return sequence.WaitForCompletion();
    }

    private Vector3 GetUIWorldPosition(Transform uiTransform)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        RectTransform rectTransform = uiTransform.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, rectTransform.position);
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 10f));
                return worldPos;
            }
            else if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position);
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, mainCamera.nearClipPlane + 10f));
                return worldPos;
            }
        }

        return uiTransform.position;
    }

    private void OnDestroy()
    {
        isMovingToTarget = false;
        currentTarget = null;
    }
}
