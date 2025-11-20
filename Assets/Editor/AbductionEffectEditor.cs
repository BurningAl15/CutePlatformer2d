using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(AbductionEffect))]
public class AbductionEffectEditor : Editor
{
    private AbductionEffect abductionEffect;
    private bool showTimeline = true;
    private bool isPlaying = false;
    private float currentTime = 0f;
    private Coroutine previewCoroutine;

    private int draggingBlock = -1;
    private bool isDraggingPosition = false;
    private bool isResizingLeft = false;
    private bool isResizingRight = false;

    private static class Colors
    {
        public static readonly Color Duration = new Color(0.3f, 0.8f, 1f);
        public static readonly Color StartTime = new Color(0.3f, 1f, 0.5f);
        public static readonly Color Block1 = new Color(1f, 0.9f, 0.3f, 0.8f);
        public static readonly Color Block2 = new Color(0.3f, 0.6f, 1f, 0.8f);
        public static readonly Color Block3 = new Color(0.3f, 1f, 0.5f, 0.8f);
        public static readonly Color Block4 = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        public static readonly Color Block5 = new Color(1f, 0.6f, 0.3f, 0.8f);
        public static readonly Color Block6 = new Color(0.6f, 0.3f, 1f, 0.8f);
    }

    private void OnEnable()
    {
        abductionEffect = (AbductionEffect)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawColorLegend();
        
        EditorGUILayout.Space(10);
        
        DrawPropertiesWithColors();
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Abduction Timeline Tool", EditorStyles.boldLabel);
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

    private void DrawColorLegend()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        GUIStyle legendStyle = new GUIStyle(GUI.skin.label);
        legendStyle.fontStyle = FontStyle.Bold;
        legendStyle.fontSize = 10;
        
        EditorGUILayout.LabelField("Property Color Legend:", legendStyle, GUILayout.Width(140));
        
        DrawColorBox(Colors.StartTime, 15);
        EditorGUILayout.LabelField("Start Time", GUILayout.Width(70));
        
        DrawColorBox(Colors.Duration, 15);
        EditorGUILayout.LabelField("Duration", GUILayout.Width(60));
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawColorBox(Color color, float size)
    {
        Rect boxRect = GUILayoutUtility.GetRect(size, size);
        EditorGUI.DrawRect(boxRect, color);
        Handles.color = Color.black;
        Handles.DrawSolidRectangleWithOutline(boxRect, Color.clear, Color.black);
    }

    private void DrawPropertiesWithColors()
    {
        EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abductionPosition"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abductionMask"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abductionLight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("globalLight"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Raycast Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundAndPlayerLayer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRaycastDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundOffset"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Timeline - Absolute Start Times", EditorStyles.boldLabel);
        DrawColoredProperty("abductionLightUpStartTime", "Abd Light Up Start", Colors.StartTime);
        DrawColoredProperty("landingStartTime", "Landing Start", Colors.StartTime);
        DrawColoredProperty("globalLightStartTime", "Global Light Start", Colors.StartTime);
        DrawColoredProperty("waitStartTime", "Wait Start", Colors.StartTime);
        DrawColoredProperty("abductionLightDownStartTime", "Abd Light Down Start", Colors.StartTime);
        DrawColoredProperty("retractionStartTime", "Retraction Start", Colors.StartTime);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
        
        DrawColoredProperty("landingDuration", "Landing Duration", Colors.Duration);
        DrawColoredProperty("retractionDuration", "Retraction Duration", Colors.Duration);
        DrawColoredProperty("scaleMultiplier", "Scale Multiplier", Color.white);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animCurve_Land"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animCurve_Abduction"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Abduction Light", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abductionLightMaxIntensity"));
        DrawColoredProperty("abductionLightFadeDuration", "Fade Duration", Colors.Duration);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("abductionLightCurve"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Global Light Animation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animateGlobalLight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("darkIntensity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("normalIntensity"));
        DrawColoredProperty("globalLightFadeDuration", "Fade Duration", Colors.Duration);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("globalLightFadeCurve"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
        DrawColoredProperty("waitAfterLanding", "Wait After Landing", Colors.Duration);
    }

    private void DrawColoredProperty(string propertyName, string label, Color color)
    {
        SerializedProperty prop = serializedObject.FindProperty(propertyName);
        if (prop == null) return;

        EditorGUILayout.BeginHorizontal();
        
        Rect colorRect = GUILayoutUtility.GetRect(4, EditorGUIUtility.singleLineHeight);
        EditorGUI.DrawRect(colorRect, color);
        
        EditorGUILayout.PropertyField(prop, new GUIContent(label));
        
        EditorGUILayout.EndHorizontal();
    }

    private void DrawInteractiveTimeline()
    {
        SerializedProperty abductionLightUpStartTimeProp = serializedObject.FindProperty("abductionLightUpStartTime");
        SerializedProperty landingStartTimeProp = serializedObject.FindProperty("landingStartTime");
        SerializedProperty globalLightStartTimeProp = serializedObject.FindProperty("globalLightStartTime");
        SerializedProperty waitStartTimeProp = serializedObject.FindProperty("waitStartTime");
        SerializedProperty abductionLightDownStartTimeProp = serializedObject.FindProperty("abductionLightDownStartTime");
        SerializedProperty retractionStartTimeProp = serializedObject.FindProperty("retractionStartTime");
        
        SerializedProperty landingDurationProp = serializedObject.FindProperty("landingDuration");
        SerializedProperty retractionDurationProp = serializedObject.FindProperty("retractionDuration");
        SerializedProperty abductionLightFadeDurationProp = serializedObject.FindProperty("abductionLightFadeDuration");
        SerializedProperty globalLightFadeDurationProp = serializedObject.FindProperty("globalLightFadeDuration");
        SerializedProperty waitAfterLandingProp = serializedObject.FindProperty("waitAfterLanding");

        float t0_start = abductionLightUpStartTimeProp.floatValue;
        float t0_duration = abductionLightFadeDurationProp.floatValue;
        float t0_end = t0_start + t0_duration;
        
        float t1_start = landingStartTimeProp.floatValue;
        float t1_duration = landingDurationProp.floatValue;
        float t1_end = t1_start + t1_duration;
        
        float t2_start = globalLightStartTimeProp.floatValue;
        float t2_duration = globalLightFadeDurationProp.floatValue;
        float t2_end = t2_start + t2_duration;
        
        float t3_start = waitStartTimeProp.floatValue;
        float t3_duration = waitAfterLandingProp.floatValue;
        float t3_end = t3_start + t3_duration;
        
        float t4_start = abductionLightDownStartTimeProp.floatValue;
        float t4_duration = abductionLightFadeDurationProp.floatValue;
        float t4_end = t4_start + t4_duration;
        
        float t5_start = retractionStartTimeProp.floatValue;
        float t5_duration = retractionDurationProp.floatValue;
        float t5_end = t5_start + t5_duration;
        
        float maxTime = Mathf.Max(t0_end, t1_end, t2_end, t3_end, t4_end, t5_end, 1f);

        EditorGUILayout.HelpBox(
            "🖱️ Drag center to move | Drag edges to resize\n" +
            "Left edge = change start time | Right edge = change duration | Center = move entire block", 
            MessageType.Info
        );

        float timelineWidth = EditorGUIUtility.currentViewWidth - 40;
        float timelineHeight = 180;
        
        Rect timelineRect = GUILayoutUtility.GetRect(timelineWidth, timelineHeight);
        
        EditorGUI.DrawRect(timelineRect, new Color(0.15f, 0.15f, 0.15f));
        
        DrawGrid(timelineRect, maxTime);
        
        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        
        HandleBlockInteraction(timelineRect, maxTime, e, controlID,
            t0_start, t0_end, t1_start, t1_end, t2_start, t2_end, 
            t3_start, t3_end, t4_start, t4_end, t5_start, t5_end,
            abductionLightUpStartTimeProp, landingStartTimeProp, globalLightStartTimeProp,
            waitStartTimeProp, abductionLightDownStartTimeProp, retractionStartTimeProp,
            landingDurationProp, retractionDurationProp, abductionLightFadeDurationProp,
            globalLightFadeDurationProp, waitAfterLandingProp);
        
        DrawDraggableBlock(timelineRect, t0_start, t0_end, Colors.Block1, "Abd Light ↑", maxTime, 0, 25);
        DrawDraggableBlock(timelineRect, t1_start, t1_end, Colors.Block2, "Landing", maxTime, 1, 50);
        DrawDraggableBlock(timelineRect, t2_start, t2_end, Colors.Block3, "Global ↑", maxTime, 2, 75);
        DrawDraggableBlock(timelineRect, t3_start, t3_end, Colors.Block4, "Wait", maxTime, 3, 100);
        DrawDraggableBlock(timelineRect, t4_start, t4_end, Colors.Block5, "Abd Light ↓", maxTime, 4, 125);
        DrawDraggableBlock(timelineRect, t5_start, t5_end, Colors.Block6, "Retract", maxTime, 5, 150);
        
        DrawTimeMarker(timelineRect, 0f, "0.0s", Color.green, maxTime);
        DrawTimeMarker(timelineRect, maxTime, $"{maxTime:F1}s", Color.red, maxTime);
        
        if (isPlaying)
        {
            DrawTimeMarker(timelineRect, currentTime, "●", new Color(1f, 0.2f, 0.2f), maxTime);
        }
        
        DrawTimeLabels(timelineRect, maxTime);
        
        EditorGUILayout.Space(5);
        
        DrawTimingInfo(t0_start, t0_end, t1_start, t1_end, t2_start, t2_end,
                      t3_start, t3_end, t4_start, t4_end, t5_start, t5_end);
    }

    private void HandleBlockInteraction(Rect timeline, float maxTime, Event e, int controlID,
        float t0_start, float t0_end, float t1_start, float t1_end, 
        float t2_start, float t2_end, float t3_start, float t3_end,
        float t4_start, float t4_end, float t5_start, float t5_end,
        SerializedProperty abductionLightUpStartTimeProp, SerializedProperty landingStartTimeProp,
        SerializedProperty globalLightStartTimeProp, SerializedProperty waitStartTimeProp,
        SerializedProperty abductionLightDownStartTimeProp, SerializedProperty retractionStartTimeProp,
        SerializedProperty landingDurationProp, SerializedProperty retractionDurationProp,
        SerializedProperty abductionLightFadeDurationProp,
        SerializedProperty globalLightFadeDurationProp, SerializedProperty waitAfterLandingProp)
    {
        switch (e.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (e.button == 0 && timeline.Contains(e.mousePosition))
                {
                    float clickTime = ((e.mousePosition.x - timeline.x) / timeline.width) * maxTime;
                    
                    if (TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 0, t0_start, t0_end) ||
                        TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 1, t1_start, t1_end) ||
                        TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 2, t2_start, t2_end) ||
                        TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 3, t3_start, t3_end) ||
                        TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 4, t4_start, t4_end) ||
                        TryStartBlockInteraction(timeline, maxTime, clickTime, e.mousePosition.x, 5, t5_start, t5_end))
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
                    
                    ApplyBlockModification(draggingBlock, deltaTime,
                        abductionLightUpStartTimeProp, landingStartTimeProp, globalLightStartTimeProp,
                        waitStartTimeProp, abductionLightDownStartTimeProp, retractionStartTimeProp,
                        landingDurationProp, retractionDurationProp, abductionLightFadeDurationProp,
                        globalLightFadeDurationProp, waitAfterLandingProp);
                    
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
        
        if (Mathf.Abs(mouseX - startX) < edgeThreshold)
        {
            isResizingLeft = true;
            isDraggingPosition = false;
            isResizingRight = false;
            return true;
        }
        else if (Mathf.Abs(mouseX - endX) < edgeThreshold)
        {
            isResizingRight = true;
            isDraggingPosition = false;
            isResizingLeft = false;
            return true;
        }
        else
        {
            isDraggingPosition = true;
            isResizingLeft = false;
            isResizingRight = false;
            return true;
        }
    }

    private void ApplyBlockModification(int blockIndex, float deltaTime,
        SerializedProperty abductionLightUpStartTimeProp, SerializedProperty landingStartTimeProp,
        SerializedProperty globalLightStartTimeProp, SerializedProperty waitStartTimeProp,
        SerializedProperty abductionLightDownStartTimeProp, SerializedProperty retractionStartTimeProp,
        SerializedProperty landingDurationProp, SerializedProperty retractionDurationProp,
        SerializedProperty abductionLightFadeDurationProp,
        SerializedProperty globalLightFadeDurationProp, SerializedProperty waitAfterLandingProp)
    {
        const float minDuration = 0.1f;
        
        switch (blockIndex)
        {
            case 0:
                if (isDraggingPosition)
                {
                    abductionLightUpStartTimeProp.floatValue = Mathf.Max(0f, abductionLightUpStartTimeProp.floatValue + deltaTime);
                }
                else if (isResizingLeft)
                {
                    float newStart = Mathf.Max(0f, abductionLightUpStartTimeProp.floatValue + deltaTime);
                    float newDuration = abductionLightFadeDurationProp.floatValue - deltaTime;
                    if (newDuration >= minDuration)
                    {
                        abductionLightUpStartTimeProp.floatValue = newStart;
                        abductionLightFadeDurationProp.floatValue = newDuration;
                    }
                }
                else if (isResizingRight)
                {
                    abductionLightFadeDurationProp.floatValue = Mathf.Max(minDuration, abductionLightFadeDurationProp.floatValue + deltaTime);
                }
                break;
            
            case 1:
                if (isDraggingPosition)
                {
                    landingStartTimeProp.floatValue = Mathf.Max(0f, landingStartTimeProp.floatValue + deltaTime);
                }
                else if (isResizingLeft)
                {
                    float newStart = Mathf.Max(0f, landingStartTimeProp.floatValue + deltaTime);
                    float newDuration = landingDurationProp.floatValue - deltaTime;
                    if (newDuration >= minDuration)
                    {
                        landingStartTimeProp.floatValue = newStart;
                        landingDurationProp.floatValue = newDuration;
                    }
                }
                else if (isResizingRight)
                {
                    landingDurationProp.floatValue = Mathf.Max(minDuration, landingDurationProp.floatValue + deltaTime);
                }
                break;
            
            case 2:
                if (isDraggingPosition)
                {
                    globalLightStartTimeProp.floatValue = Mathf.Max(0f, globalLightStartTimeProp.floatValue + deltaTime);
                }
                else if (isResizingLeft)
                {
                    float newStart = Mathf.Max(0f, globalLightStartTimeProp.floatValue + deltaTime);
                    float newDuration = globalLightFadeDurationProp.floatValue - deltaTime;
                    if (newDuration >= minDuration)
                    {
                        globalLightStartTimeProp.floatValue = newStart;
                        globalLightFadeDurationProp.floatValue = newDuration;
                    }
                }
                else if (isResizingRight)
                {
                    globalLightFadeDurationProp.floatValue = Mathf.Max(minDuration, globalLightFadeDurationProp.floatValue + deltaTime);
                }
                break;
            
            case 3:
                if (isDraggingPosition)
                {
                    waitStartTimeProp.floatValue = Mathf.Max(0f, waitStartTimeProp.floatValue + deltaTime);
                }
                else if (isResizingLeft)
                {
                    float newStart = Mathf.Max(0f, waitStartTimeProp.floatValue + deltaTime);
                    float newDuration = waitAfterLandingProp.floatValue - deltaTime;
                    if (newDuration >= 0f)
                    {
                        waitStartTimeProp.floatValue = newStart;
                        waitAfterLandingProp.floatValue = Mathf.Max(0f, newDuration);
                    }
                }
                else if (isResizingRight)
                {
                    waitAfterLandingProp.floatValue = Mathf.Max(0f, waitAfterLandingProp.floatValue + deltaTime);
                }
                break;
            
            case 4:
                if (isDraggingPosition)
                {
                    abductionLightDownStartTimeProp.floatValue = Mathf.Max(0f, abductionLightDownStartTimeProp.floatValue + deltaTime);
                }
                else if (isResizingLeft)
                {
                    float newStart = Mathf.Max(0f, abductionLightDownStartTimeProp.floatValue + deltaTime);
                    float newDuration = abductionLightFadeDurationProp.floatValue - deltaTime;
                    if (newDuration >= minDuration)
                    {
                        abductionLightDownStartTimeProp.floatValue = newStart;
                        abductionLightFadeDurationProp.floatValue = newDuration;
                    }
                }
                else if (isResizingRight)
                {
                    abductionLightFadeDurationProp.floatValue = Mathf.Max(minDuration, abductionLightFadeDurationProp.floatValue + deltaTime);
                }
                break;
            
            case 5:
                if (isDraggingPosition)
                {
                    retractionStartTimeProp.floatValue = Mathf.Max(0f, retractionStartTimeProp.floatValue + deltaTime);
                }
                else if (isResizingLeft)
                {
                    float newStart = Mathf.Max(0f, retractionStartTimeProp.floatValue + deltaTime);
                    float newDuration = retractionDurationProp.floatValue - deltaTime;
                    if (newDuration >= minDuration)
                    {
                        retractionStartTimeProp.floatValue = newStart;
                        retractionDurationProp.floatValue = newDuration;
                    }
                }
                else if (isResizingRight)
                {
                    retractionDurationProp.floatValue = Mathf.Max(minDuration, retractionDurationProp.floatValue + deltaTime);
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
        
        Rect leftHandle = new Rect(barRect.x - 3, barRect.y, 6, barRect.height);
        Rect rightHandle = new Rect(barRect.xMax - 3, barRect.y, 6, barRect.height);
        EditorGUI.DrawRect(leftHandle, new Color(1f, 1f, 1f, 0.6f));
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
        
        GUIStyle timeStyle = new GUIStyle(GUI.skin.label);
        timeStyle.alignment = TextAnchor.MiddleCenter;
        timeStyle.fontSize = 8;
        timeStyle.normal.textColor = Color.yellow;
        
        if (width > 70)
        {
            Rect timeRect = new Rect(barRect.x, barRect.y + 10, barRect.width, 10);
            GUI.Label(timeRect, $"{(endTime - startTime):F2}s", timeStyle);
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

    private void DrawTimingInfo(float t0_start, float t0_end, float t1_start, float t1_end,
                                float t2_start, float t2_end, float t3_start, float t3_end,
                                float t4_start, float t4_end, float t5_start, float t5_end)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Timeline Summary", EditorStyles.boldLabel);
        
        GUIStyle compactStyle = new GUIStyle(GUI.skin.label);
        compactStyle.fontSize = 9;
        compactStyle.padding = new RectOffset(10, 0, 0, 0);
        
        EditorGUILayout.BeginHorizontal();
        DrawColorBox(Colors.Block1, 12);
        EditorGUILayout.LabelField($"Abd Light ↑: {t0_start:F2}s → {t0_end:F2}s (dur: {(t0_end - t0_start):F2}s)", compactStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        DrawColorBox(Colors.Block2, 12);
        EditorGUILayout.LabelField($"Landing: {t1_start:F2}s → {t1_end:F2}s (dur: {(t1_end - t1_start):F2}s)", compactStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        DrawColorBox(Colors.Block3, 12);
        EditorGUILayout.LabelField($"Global ↑: {t2_start:F2}s → {t2_end:F2}s (dur: {(t2_end - t2_start):F2}s)", compactStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        DrawColorBox(Colors.Block4, 12);
        EditorGUILayout.LabelField($"Wait: {t3_start:F2}s → {t3_end:F2}s (dur: {(t3_end - t3_start):F2}s)", compactStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        DrawColorBox(Colors.Block5, 12);
        EditorGUILayout.LabelField($"Abd Light ↓: {t4_start:F2}s → {t4_end:F2}s (dur: {(t4_end - t4_start):F2}s)", compactStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        DrawColorBox(Colors.Block6, 12);
        EditorGUILayout.LabelField($"Retract: {t5_start:F2}s → {t5_end:F2}s (dur: {(t5_end - t5_start):F2}s)", compactStyle);
        EditorGUILayout.EndHorizontal();
        
        float totalDuration = Mathf.Max(t0_end, t1_end, t2_end, t3_end, t4_end, t5_end);
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField($"Total Duration: {totalDuration:F2}s", EditorStyles.boldLabel);
        
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
            previewCoroutine = abductionEffect.StartCoroutine(PreviewLanding());
        }
        
        if (GUILayout.Button("▶ Test Abduction", GUILayout.Height(30)))
        {
            StopPreview();
            previewCoroutine = abductionEffect.StartCoroutine(PreviewAbduction());
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
            abductionEffect.Initialize();
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
        
        IEnumerator landingCoroutine = abductionEffect.AbductionAnim_Land();
        
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
        
        IEnumerator abductionCoroutine = abductionEffect.AbductionAnim_Abduction(true);
        
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
        if (previewCoroutine != null && abductionEffect != null)
        {
            abductionEffect.StopCoroutine(previewCoroutine);
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

