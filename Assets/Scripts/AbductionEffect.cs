using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AbductionEffect : MonoBehaviour
{
    [SerializeField] private Transform abductionPosition;
    [SerializeField] private Transform abductionMask;

    [SerializeField] private float yMinScale = 0;
    [SerializeField] private float yMaxScale = 3.711f;

    [SerializeField] private float yMinPos = 0.454f;
    [SerializeField] private float yMaxPos = -0.467f;

    [SerializeField] private float duration;

    [SerializeField] private AnimationCurve animCurve_Land;
    [SerializeField] private AnimationCurve animCurve_Abduction;

    [SerializeField] private Light2D light;

    [SerializeField] private Light2D globalLight;
    
    private IEnumerator Abduction(AnimationCurve curve, bool holdPlayer = true)
    {
        if (holdPlayer)
        {
            Player2DController.instance.PlayerRigidbody(true);
            globalLight.intensity = 0.05f;
        }

        float abductionTime = 0;
        while (abductionTime < duration)
        {
            float curveValue = curve.Evaluate(abductionTime / duration);
            abductionPosition.localPosition = new Vector3(0, curveValue * yMaxPos, 0);
            if (holdPlayer)
                Player2DController.instance.CallRespawn(true, abductionPosition.position, true);
            abductionMask.localScale = new Vector3(abductionPosition.localScale.x, curveValue * yMaxScale,
                abductionPosition.localScale.z);
            abductionTime += Time.fixedDeltaTime;
            yield return null;
        }

        float lightTime = 0;
        float lightDuration = .2f;
        
        if (holdPlayer)
        {
            Player2DController.instance.PlayerRigidbody(false);
            while (lightTime < lightDuration)
            {
                float curveValue = curve.Evaluate(lightTime / lightDuration);
                globalLight.intensity = 0.05f + curveValue * 0.6f;
                lightTime += Time.fixedDeltaTime;
                yield return null;
            }
        }
        
        yield return new WaitForSeconds(1);
        if (holdPlayer)
        {
            Player2DController.instance.CallRespawn(false,abductionPosition.position);
        }
    }

    public void Initialize()
    {
        abductionPosition.localPosition = new Vector3(0, yMinPos, 0);
        abductionMask.localScale = new  Vector3(abductionPosition.localScale.x,yMinScale,abductionPosition.localScale.z);
    }

    public IEnumerator AbductionAnim_Land()
    {
        light.gameObject.SetActive(true);
        yield return StartCoroutine(Abduction(animCurve_Land));
        light.gameObject.SetActive(false);
        yield return  StartCoroutine(Abduction(animCurve_Abduction,false));
    }

    public IEnumerator AbductionAnim_Abduction(bool holdPlayer)
    {
        yield return StartCoroutine(Abduction(animCurve_Abduction,holdPlayer));
    }
    
}
