using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AbductionEffect))]
public class AbductionEffectEditor : Editor
{
    private AbductionEffect effect;
    private bool showTimeline = true;
    private bool isPlaying = false;
    private float currentTime = 0f;
    private Coroutine previewCoroutine;

    private int draggingBlock = -1;
    private bool isDraggingPosition = false;
    private bool isResizingLeft = false;
    private bool isResizingRight = false;

    private static class TimelineColors
    {
        public static readonly Color Block1 = new Color(1f, 0.9f, 0.3f, 0.8f);
        public static readonly Color Block2 = new Color(0.3f, 0.6f, 1f, 0.8f);
        public static readonly Color Block3 = new Color(0.3f, 1f, 0.5f, 0.8f);
    }

    private void OnEnable()
    {
        effect = (AbductionEffect)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Timeline Editor (Landing Sequence Only)", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        showTimeline = EditorGUILayout.Foldout(showTimeline, "Interactive Timeline", true);
        
        if (showTimeline)
        {
            DrawInteractiveTimeline();
        }

        EditorGUILayout.Space(10);
        DrawTestButtons();
        
        if (isPlaying)
        {
            DrawPlaybackInfo();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawInteractiveTimeline()
    {
        SerializedProperty normalDeployProp = serializedObject.FindProperty("normalDeployDuration");
        SerializedProperty retractProp = serializedObject.FindProperty("retractDuration");
        SerializedProperty lightFadeProp = serializedObject.FindProperty("lightFadeDuration");
        SerializedProperty waitProp = serializedObject.FindProperty("waitAfterLanding");

        if (normalDeployProp == null || retractProp == null || lightFadeProp == null)
        {
            EditorGUILayout.HelpBox("Timeline properties not found. Make sure AbductionEffect has the correct serialized fields.", MessageType.Warning);
            return;
        }

        float t0_start = 0f;
        float t0_duration = lightFadeProp.floatValue;
        float t0_end = t0_start + t0_duration;
        
        float t1_start = 0.5f;
        float t1_duration = normalDeployProp.floatValue;
        float t1_end = t1_start + t1_duration;
        
        float t2_start = t1_end;
        float t2_duration = waitProp.floatValue;
        float t2_end = t2_start + t2_duration;
        
        float maxTime = Mathf.Max(t0_end, t1_end, t2_end, 3f);

        EditorGUILayout.HelpBox(
            "🖱️ Drag blocks to adjust timing:\n" +
            "• Yellow = Beam Light Fade\n" +
            "• Blue = Deploy Beam (Landing)\n" +
            "• Green = Wait After Landing", 
            MessageType.Info
        );

        float timelineWidth = EditorGUIUtility.currentViewWidth - 40;
        float timelineHeight = 120;
        
        Rect timelineRect = GUILayoutUtility.GetRect(timelineWidth, timelineHeight);
        
        EditorGUI.DrawRect(timelineRect, new Color(0.15f, 0.15f, 0.15f));
        
        DrawGrid(timelineRect, maxTime);
        
        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        
        HandleBlockInteraction(timelineRect, maxTime, e, controlID,
            t0_start, t0_end, t1_start, t1_end, t2_start, t2_end,
            lightFadeProp, normalDeployProp, waitProp);
        
        DrawDraggableBlock(timelineRect, t0_start, t0_end, TimelineColors.Block1, "Light Fade", maxTime, 0, 20);
        DrawDraggableBlock(timelineRect, t1_start, t1_end, TimelineColors.Block2, "Deploy Beam", maxTime, 1, 50);
        DrawDraggableBlock(timelineRect, t2_start, t2_end, TimelineColors.Block3, "Wait", maxTime, 2, 80);
        
        DrawTimeMarker(timelineRect, 0f, "0.0s", Color.green, maxTime);
        DrawTimeMarker(timelineRect, maxTime, $"{maxTime:F1}s", Color.red, maxTime);
        
        if (isPlaying)
        {
            DrawTimeMarker(timelineRect, currentTime, "●", new Color(1f, 0.2f, 0.2f), maxTime);
        }
        
        DrawTimeLabels(timelineRect, maxTime);
        
        EditorGUILayout.Space(5);
        DrawTimingInfo(normalDeployProp.floatValue, retractProp.floatValue, lightFadeProp.floatValue, waitProp.floatValue);
    }

    private void HandleBlockInteraction(Rect timeline, float maxTime, Event e, int controlID,
        float t0_start, float t0_end, float t1_start, float t1_end, float t2_start, float t2_end,
        SerializedProperty lightFadeProp, SerializedProperty normalDeployProp, SerializedProperty waitProp)
    {
        switch (e.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (e.button == 0 && timeline.Contains(e.mousePosition))
                {
                    float clickTime = ((e.mousePosition.x - timeline.x) / timeline.width) * maxTime;
                    
                    if (TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 0, t0_start, t0_end) ||
                        TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 1, t1_start, t1_end) ||
                        TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 2, t2_start, t2_end))
                    {
                        GUIUtility.hotControl = controlID;
                        e.Use();
                        Repaint();
                    }
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID && draggingBlock >= 0)
                {
                    float deltaX = e.delta.x;
                    float deltaTime = (deltaX / timeline.width) * maxTime;
                    
                    ApplyBlockModification(draggingBlock, deltaTime, lightFadeProp, normalDeployProp, waitProp);
                    
                    serializedObject.ApplyModifiedProperties();
                    e.Use();
                    Repaint();
                }
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                    draggingBlock = -1;
                    isDraggingPosition = false;
                    isResizingLeft = false;
                    isResizingRight = false;
                    e.Use();
                    Repaint();
                }
                break;

            case EventType.Repaint:
                if (timeline.Contains(Event.current.mousePosition))
                {
                    MouseCursor cursor = MouseCursor.Arrow;
                    
                    if (isDraggingPosition)
                        cursor = MouseCursor.MoveArrow;
                    else if (isResizingLeft || isResizingRight)
                        cursor = MouseCursor.ResizeHorizontal;
                    
                    EditorGUIUtility.AddCursorRect(timeline, cursor);
                }
                break;
        }
    }

    private bool TryStartBlockInteraction(Rect timeline, float maxTime, float clickTime, float mouseX,
                                          int blockIndex, float startTime, float endTime)
    {
        const float edgeThreshold = 10f;
        
        if (clickTime < startTime || clickTime > endTime)
            return false;
        
        float startX = timeline.x + (startTime / maxTime) * timeline.width;
        float endX = timeline.x + (endTime / maxTime) * timeline.width;
        
        draggingBlock = blockIndex;
        
        if (Mathf.Abs(mouseX - endX) < edgeThreshold)
        {
            isResizingRight = true;
            isDraggingPosition = false;
            isResizingLeft = false;
            return true;
        }
        else
        {
            isDraggingPosition = false;
            isResizingLeft = false;
            isResizingRight = false;
            return false;
        }
    }

    private void ApplyBlockModification(int blockIndex, float deltaTime,
        SerializedProperty lightFadeProp, SerializedProperty normalDeployProp, SerializedProperty waitProp)
    {
        const float minDuration = 0.1f;
        
        switch (blockIndex)
        {
            case 0:
                if (isResizingRight)
                {
                    lightFadeProp.floatValue = Mathf.Max(minDuration, lightFadeProp.floatValue + deltaTime);
                }
                break;
            
            case 1:
                if (isResizingRight)
                {
                    normalDeployProp.floatValue = Mathf.Max(minDuration, normalDeployProp.floatValue + deltaTime);
                }
                break;
            
            case 2:
                if (isResizingRight)
                {
                    waitProp.floatValue = Mathf.Max(0f, waitProp.floatValue + deltaTime);
                }
                break;
        }
    }

    private void DrawGrid(Rect timeline, float maxTime)
    {
        Handles.color = new Color(0.3f, 0.3f, 0.3f);
        
        int numLines = Mathf.CeilToInt(maxTime);
        for (int i = 0; i <= numLines; i++)
        {
            float t = i;
            if (t > maxTime) break;
            
            float x = timeline.x + (t / maxTime) * timeline.width;
            Handles.DrawLine(
                new Vector3(x, timeline.y, 0),
                new Vector3(x, timeline.y + timeline.height - 20, 0)
            );
        }
    }

    private void DrawDraggableBlock(Rect timeline, float startTime, float endTime, Color color, string label, float totalDuration, int blockIndex, float yOffset)
    {
        if (totalDuration <= 0) return;
        
        float startX = timeline.x + (startTime / totalDuration) * timeline.width;
        float width = ((endTime - startTime) / totalDuration) * timeline.width;
        
        width = Mathf.Max(width, 2f);
        
        Rect barRect = new Rect(startX, timeline.y + yOffset, width, 20);
        
        bool isActive = draggingBlock == blockIndex;
        Color drawColor = isActive ? Color.Lerp(color, Color.white, 0.5f) : color;
        EditorGUI.DrawRect(barRect, drawColor);
        
        Handles.color = isActive ? Color.yellow : Color.white;
        float outlineWidth = isActive ? 2f : 1f;
        
        for (int i = 0; i < outlineWidth; i++)
        {
            Rect outlineRect = new Rect(barRect.x - i, barRect.y - i, barRect.width + i * 2, barRect.height + i * 2);
            Handles.DrawLine(new Vector3(outlineRect.xMin, outlineRect.yMin), new Vector3(outlineRect.xMax, outlineRect.yMin));
            Handles.DrawLine(new Vector3(outlineRect.xMin, outlineRect.yMax), new Vector3(outlineRect.xMax, outlineRect.yMax));
            Handles.DrawLine(new Vector3(outlineRect.xMin, outlineRect.yMin), new Vector3(outlineRect.xMin, outlineRect.yMax));
            Handles.DrawLine(new Vector3(outlineRect.xMax, outlineRect.yMin), new Vector3(outlineRect.xMax, outlineRect.yMax));
        }
        
        Rect rightHandle = new Rect(barRect.xMax - 3, barRect.y, 6, barRect.height);
        EditorGUI.DrawRect(rightHandle, new Color(1f, 1f, 1f, 0.6f));
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontSize = 9;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;
        
        if (width > 50)
        {
            GUI.Label(barRect, label, labelStyle);
        }
    }

    private void DrawTimeMarker(Rect timeline, float time, string label, Color color, float totalDuration)
    {
        if (totalDuration <= 0) return;
        
        float x = timeline.x + (time / totalDuration) * timeline.width;
        
        Handles.color = color;
        Handles.DrawLine(
            new Vector3(x, timeline.y + 15, 0),
            new Vector3(x, timeline.y + timeline.height - 20, 0)
        );
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 9;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = color;
        labelStyle.alignment = TextAnchor.UpperCenter;
        
        Rect labelRect = new Rect(x - 30, timeline.y, 60, 15);
        GUI.Label(labelRect, label, labelStyle);
    }

    private void DrawTimeLabels(Rect timeline, float totalTime)
    {
        GUIStyle timeStyle = new GUIStyle(GUI.skin.label);
        timeStyle.fontSize = 8;
        timeStyle.normal.textColor = Color.gray;
        timeStyle.alignment = TextAnchor.UpperCenter;
        
        int numLabels = Mathf.Min(10, Mathf.CeilToInt(totalTime));
        for (int i = 0; i <= numLabels; i++)
        {
            float t = (i / (float)numLabels) * totalTime;
            float x = timeline.x + (t / totalTime) * timeline.width;
            
            Rect labelRect = new Rect(x - 20, timeline.yMax - 18, 40, 15);
            GUI.Label(labelRect, $"{t:F1}s", timeStyle);
        }
    }

    private void DrawTimingInfo(float deployDuration, float retractDuration, float lightFadeDuration, float waitDuration)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Reference", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField($"• Deploy Beam: {deployDuration:F2}s");
        EditorGUILayout.LabelField($"• Retract Beam: {retractDuration:F2}s");
        EditorGUILayout.LabelField($"• Light Fade: {lightFadeDuration:F2}s");
        EditorGUILayout.LabelField($"• Wait After Landing: {waitDuration:F2}s");
        
        EditorGUILayout.EndVertical();
    }

    private void DrawTestButtons()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test animations", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("▶ Test Landing", GUILayout.Height(30)))
        {
            StopPreview();
            previewCoroutine = effect.StartCoroutine(PreviewLanding());
        }
        
        if (GUILayout.Button("▶ Test Abduction", GUILayout.Height(30)))
        {
            StopPreview();
            previewCoroutine = effect.StartCoroutine(PreviewAbduction());
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("⏹ Stop Preview", GUILayout.Height(25)))
        {
            StopPreview();
        }
        
        if (GUILayout.Button("🔄 Reset", GUILayout.Height(25)))
        {
            StopPreview();
            effect.Initialize();
        }
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPlaybackInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"⏱️ Preview Running - Time: {currentTime:F2}s", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
    }

    private IEnumerator PreviewLanding()
    {
        isPlaying = true;
        currentTime = 0f;
        
        Debug.Log("[Editor] Starting Landing Preview");
        
        IEnumerator landingCoroutine = effect.PlayLandingSequence();
        
        while (landingCoroutine.MoveNext())
        {
            currentTime += Time.fixedDeltaTime;
            Repaint();
            yield return landingCoroutine.Current;
        }
        
        isPlaying = false;
        currentTime = 0f;
        previewCoroutine = null;
        Repaint();
        
        Debug.Log("[Editor] Landing Preview Complete");
    }

    private IEnumerator PreviewAbduction()
    {
        isPlaying = true;
        currentTime = 0f;
        
        Debug.Log("[Editor] Starting Abduction Preview");
        
        IEnumerator abductionCoroutine = effect.PlayAbductionSequence();
        
        while (abductionCoroutine.MoveNext())
        {
            currentTime += Time.fixedDeltaTime;
            Repaint();
            yield return abductionCoroutine.Current;
        }
        
        isPlaying = false;
        currentTime = 0f;
        previewCoroutine = null;
        Repaint();
        
        Debug.Log("[Editor] Abduction Preview Complete");
    }

    private void StopPreview()
    {
        if (previewCoroutine != null && effect != null)
        {
            effect.StopCoroutine(previewCoroutine);
            previewCoroutine = null;
        }
        
        isPlaying = false;
        currentTime = 0f;
        Repaint();
        
        Debug.Log("[Editor] Preview Stopped");
    }

    private void OnDisable()
    {
        StopPreview();
    }
}
