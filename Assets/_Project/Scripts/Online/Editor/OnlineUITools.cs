using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>Dựng UI trong scene đang mở; không sửa scene bằng text hay tự lưu scene.</summary>
public static class OnlineUITools
{
    [MenuItem("Tools/Online/Tạo bảng xếp hạng")]
    public static void CreateLeaderboard()
    {
        if (!EnsureScene("LevelSelect", "OnlineLeaderboardCanvas")) return;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Tạo bảng xếp hạng");
        GameObject root = CreateCanvas("OnlineLeaderboardCanvas", 100);
        GameObject panel = Box("LeaderboardPanel", root.transform, new Color(.06f, .09f, .16f, .98f));
        Stretch(panel, new Vector2(.1f, .04f), new Vector2(.9f, .96f));
        var controller = Undo.AddComponent<LeaderboardPanel>(panel);
        Label("Title", panel.transform, "BẢNG XẾP HẠNG", 34, new Vector2(0, -38), new Vector2(900, 48));
        TMP_Text status = Label("Status", panel.transform, "Tổng sao cao nhất", 22,
            new Vector2(0, -82), new Vector2(900, 40));
        TMP_Text[] rows = new TMP_Text[10];
        for (int i = 0; i < rows.Length; i++)
            rows[i] = Label("Rank" + (i + 1), panel.transform, "", 24,
                new Vector2(0, -126 - i * 36), new Vector2(900, 34));
        TMP_Text own = Label("OwnRank", panel.transform, "Hạng của bạn: —", 24,
            new Vector2(0, -510), new Vector2(900, 48));
        ButtonAt("Retry", panel.transform, "Thử lại", new Vector2(-130, -584), controller.Refresh);
        ButtonAt("Close", panel.transform, "Đóng", new Vector2(130, -584), controller.Close);
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("statusText").objectReferenceValue = status;
        serialized.FindProperty("ownRankText").objectReferenceValue = own;
        SerializedProperty rowRefs = serialized.FindProperty("rows");
        rowRefs.arraySize = rows.Length;
        for (int i = 0; i < rows.Length; i++) rowRefs.GetArrayElementAtIndex(i).objectReferenceValue = rows[i];
        serialized.ApplyModifiedProperties();
        Button open = ButtonAt("OpenLeaderboard", root.transform, "Xếp hạng", Vector2.zero, controller.Show);
        RectTransform openRect = (RectTransform)open.transform;
        openRect.anchorMin = openRect.anchorMax = new Vector2(1, 0);
        openRect.anchoredPosition = new Vector2(-145, 55);
        panel.SetActive(false);
        Finish(root, group);
    }

    [MenuItem("Tools/Online/Tạo popup đồng ý Analytics")]
    public static void CreateConsent()
    {
        if (!EnsureScene("StartScreen", "OnlineConsentCanvas")) return;
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Tạo popup đồng ý Analytics");
        GameObject root = CreateCanvas("OnlineConsentCanvas", 200);
        var prompt = Undo.AddComponent<AnalyticsConsentPrompt>(root);
        GameObject panel = Box("ConsentPanel", root.transform, new Color(0, 0, 0, .8f));
        Stretch(panel, Vector2.zero, Vector2.one);
        GameObject card = Box("Card", panel.transform, new Color(.06f, .09f, .16f, 1));
        Place(card, new Vector2(.5f, .5f), Vector2.zero, new Vector2(1000, 500));
        Label("Title", card.transform, "GIÚP MR. EGGY CẢI THIỆN", 32,
            new Vector2(0, -55), new Vector2(940, 60));
        TMP_Text body = Label("Explanation", card.transform,
            "Bạn có đồng ý gửi dữ liệu phân tích để cải thiện game không?\n\n"
            + "Dữ liệu gồm màn chơi, thời gian chơi, số lần chết, gợi ý, sao và các sự kiện thiết bị/phiên chơi của Unity Analytics.\n\n"
            + "Bạn có thể từ chối và tiếp tục chơi bình thường. Bạn có thể thu hồi lựa chọn qua chức năng Đồng ý Analytics khi mở lại popup.",
            25, new Vector2(0, -245), new Vector2(900, 290));
        body.alignment = TextAlignmentOptions.TopLeft;
        body.overflowMode = TextOverflowModes.Overflow;
        ButtonAt("Grant", card.transform, "Đồng ý", new Vector2(-140, -438), prompt.Grant);
        ButtonAt("Revoke", card.transform, "Từ chối", new Vector2(140, -438), prompt.Revoke);
        // Cho phép mở lại popup để đổi/thu hồi lựa chọn sau lần đầu.
        Button settings = ButtonAt("AnalyticsChoice", root.transform, "Đồng ý Analytics", Vector2.zero, null);
        UnityEventTools.AddBoolPersistentListener(settings.onClick, panel.SetActive, true);
        settings.transform.SetAsFirstSibling();
        RectTransform settingsRect = (RectTransform)settings.transform;
        settingsRect.anchorMin = settingsRect.anchorMax = new Vector2(0, 0);
        settingsRect.anchoredPosition = new Vector2(155, 50);
        var serialized = new SerializedObject(prompt);
        serialized.FindProperty("consentPanel").objectReferenceValue = panel;
        serialized.ApplyModifiedProperties();
        Finish(root, group);
    }

    private static bool EnsureScene(string sceneName, string rootName)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("⚠ Hãy dừng Play mode trước khi tạo UI online.");
            return false;
        }
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != sceneName)
        {
            Debug.LogWarning($"⚠ Hãy mở scene {sceneName} trước khi chạy tool này.");
            return false;
        }
        foreach (GameObject root in scene.GetRootGameObjects())
            if (root.name == rootName)
            {
                Selection.activeGameObject = root;
                Debug.LogWarning("⚠ UI này đã tồn tại. Dùng Undo hoặc xóa bản cũ trước khi tạo lại.");
                return false;
            }
        return true;
    }

    private static GameObject CreateCanvas(string name, int sortingOrder)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(root, "Tạo Canvas online");
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = .5f;
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("OnlineEventSystem", typeof(EventSystem));
            Undo.RegisterCreatedObjectUndo(events, "Tạo EventSystem");
#if ENABLE_INPUT_SYSTEM
            Undo.AddComponent<InputSystemUIInputModule>(events).AssignDefaultActions();
#else
            Undo.AddComponent<StandaloneInputModule>(events);
#endif
        }
        return root;
    }

    private static GameObject Box(string name, Transform parent, Color color)
    {
        var box = new GameObject(name, typeof(RectTransform), typeof(Image));
        box.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(box, "Tạo UI online");
        box.GetComponent<Image>().color = color;
        return box;
    }

    private static TMP_Text Label(string name, Transform parent, string text, int size, Vector2 position, Vector2 dimensions)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(obj, "Tạo chữ online");
        var label = obj.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.font = TMP_Settings.defaultFontAsset;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        label.richText = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
        Place(obj, new Vector2(.5f, 1), position, dimensions);
        return label;
    }

    private static Button ButtonAt(string name, Transform parent, string text, Vector2 position, UnityAction action)
    {
        GameObject obj = Box(name, parent, new Color(.15f, .32f, .52f, 1));
        Place(obj, new Vector2(.5f, 1), position, new Vector2(250, 56));
        Button button = Undo.AddComponent<Button>(obj);
        button.targetGraphic = obj.GetComponent<Image>();
        TMP_Text label = Label("Label", obj.transform, text, 24, Vector2.zero, Vector2.zero);
        Stretch(label.gameObject, Vector2.zero, Vector2.one);
        if (action != null) UnityEventTools.AddPersistentListener(button.onClick, action);
        return button;
    }

    private static void Place(GameObject obj, Vector2 anchor, Vector2 position, Vector2 size)
    {
        var rect = (RectTransform)obj.transform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(GameObject obj, Vector2 min, Vector2 max)
    {
        var rect = (RectTransform)obj.transform;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void Finish(GameObject root, int group)
    {
        root.transform.SetAsLastSibling();
        EditorSceneManager.MarkSceneDirty(root.scene);
        Selection.activeGameObject = root;
        Undo.CollapseUndoOperations(group);
        Debug.Log("✅ Đã dựng và nối UI online. Hãy kiểm tra font tiếng Việt, vị trí nút rồi lưu scene (Ctrl+S).");
    }
}
