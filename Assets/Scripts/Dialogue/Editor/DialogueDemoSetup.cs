using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class DialogueDemoSetup
{
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    private const string DemoObjectName = "DialogueTestObject";

    [MenuItem("Tools/Dialogue/Create Main Demo")]
    public static void CreateMainDemo()
    {
        Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

        GameObject existingDemo = GameObject.Find(DemoObjectName);
        if (existingDemo != null)
        {
            Selection.activeGameObject = existingDemo;
            Debug.Log("Dialogue demo already exists in Main scene.", existingDemo);
            return;
        }

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("Dialogue demo setup could not find a GameObject named 'Player'.");
            return;
        }

        CreateDialogueUi();
        CreateDialogueTrigger(player.transform.position + Vector3.right * 2f);
        EnsureEventSystem();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Created DialogueTestObject and dialogue UI in Main scene.");
    }

    private static void CreateDialogueUi()
    {
        GameObject canvasObject = new GameObject(
            "DialogueCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(DialogueManager));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = CreateUiObject("DialoguePanel", canvasObject.transform);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);
        SetRect(panel.GetComponent<RectTransform>(), new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.34f), Vector2.zero, Vector2.zero);

        TMP_Text speaker = CreateText("SpeakerName", panel.transform, 34f, FontStyles.Bold);
        speaker.color = new Color(1f, 0.82f, 0.28f);
        SetRect(speaker.rectTransform, new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.95f), Vector2.zero, Vector2.zero);

        TMP_Text dialogue = CreateText("DialogueText", panel.transform, 30f, FontStyles.Normal);
        dialogue.color = Color.white;
        dialogue.enableWordWrapping = true;
        SetRect(dialogue.rectTransform, new Vector2(0.04f, 0.34f), new Vector2(0.96f, 0.72f), Vector2.zero, Vector2.zero);

        Button optionOne = CreateButton("OptionOne", panel.transform, new Vector2(0.04f, 0.07f), new Vector2(0.48f, 0.30f), out TMP_Text optionOneLabel);
        Button optionTwo = CreateButton("OptionTwo", panel.transform, new Vector2(0.52f, 0.07f), new Vector2(0.96f, 0.30f), out TMP_Text optionTwoLabel);

        DialogueManager manager = canvasObject.GetComponent<DialogueManager>();
        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("dialoguePanel").objectReferenceValue = panel;
        serializedManager.FindProperty("speakerText").objectReferenceValue = speaker;
        serializedManager.FindProperty("dialogueText").objectReferenceValue = dialogue;
        serializedManager.FindProperty("optionOneButton").objectReferenceValue = optionOne;
        serializedManager.FindProperty("optionTwoButton").objectReferenceValue = optionTwo;
        serializedManager.FindProperty("optionOneLabel").objectReferenceValue = optionOneLabel;
        serializedManager.FindProperty("optionTwoLabel").objectReferenceValue = optionTwoLabel;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateDialogueTrigger(Vector3 position)
    {
        GameObject demoObject = new GameObject(DemoObjectName);
        demoObject.transform.position = position;

        CircleCollider2D trigger = demoObject.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 1.25f;

        demoObject.AddComponent<DialogueTrigger>();
        Selection.activeGameObject = demoObject;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject uiObject = new GameObject(name, typeof(RectTransform));
        uiObject.transform.SetParent(parent, false);
        return uiObject;
    }

    private static TMP_Text CreateText(string name, Transform parent, float fontSize, FontStyles style)
    {
        GameObject textObject = CreateUiObject(name, parent);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Left;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        out TMP_Text label)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.16f, 0.2f, 0.29f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.28f, 0.36f, 0.52f, 1f);
        colors.pressedColor = new Color(0.10f, 0.13f, 0.20f, 1f);
        button.colors = colors;

        SetRect(buttonObject.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);

        label = CreateText("Label", buttonObject.transform, 25f, FontStyles.Normal);
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(14f, 6f), new Vector2(-14f, -6f));
        return button;
    }

    private static void SetRect(
        RectTransform rect,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }
}
