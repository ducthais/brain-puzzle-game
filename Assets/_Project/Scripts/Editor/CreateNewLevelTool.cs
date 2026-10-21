using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Công cụ tạo Level mới tự động:
/// 1. Copy scene mẫu thành "Level N"
/// 2. Thêm scene vào Build Profiles
/// 3. Nối nextLevelName: màn cuối cũ → màn mới, màn mới cuối → LevelSelect
/// 4. Thêm mục Hint vào LevelDatabase
/// 5. Thêm nút Level (và Chương mới nếu cần) vào scene LevelSelect
/// </summary>
public class CreateNewLevelTool : EditorWindow
{
    private const string ScenesFolder = "Assets/_Project/Scenes";
    private const string LevelSelectScenePath = ScenesFolder + "/LevelSelect.unity";
    private const string LevelSelectSceneName = "LevelSelect";
    private static readonly Regex LevelNameRegex = new Regex(@"^Level (\d+)$");

    private SceneAsset templateScene;
    private int levelCount = 1;
    private int levelsPerChapter = 4;
    private List<string> hints = new List<string> { "" };
    private Vector2 scroll;

    [MenuItem("Tools/Tạo Level mới")]
    public static void ShowWindow()
    {
        CreateNewLevelTool window = GetWindow<CreateNewLevelTool>("Tạo Level mới");
        window.minSize = new Vector2(420, 360);
    }

    private void OnEnable()
    {
        // Mặc định dùng màn cuối cùng làm mẫu (có sẵn Player, HUD, LevelManager...)
        int lastLevel = GetLastLevelNumber();
        if (templateScene == null && lastLevel > 0)
            templateScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(GetLevelScenePath(lastLevel));
    }

    private void OnGUI()
    {
        int lastLevel = GetLastLevelNumber();

        EditorGUILayout.LabelField("Màn cuối hiện tại", lastLevel > 0 ? "Level " + lastLevel : "(chưa có)");
        EditorGUILayout.Space();

        templateScene = (SceneAsset)EditorGUILayout.ObjectField("Scene mẫu", templateScene, typeof(SceneAsset), false);
        levelCount = Mathf.Clamp(EditorGUILayout.IntField("Số màn cần tạo", levelCount), 1, 50);
        levelsPerChapter = Mathf.Max(1, EditorGUILayout.IntField("Số màn mỗi chương", levelsPerChapter));

        while (hints.Count < levelCount) hints.Add("");
        while (hints.Count > levelCount) hints.RemoveAt(hints.Count - 1);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Gợi ý (Hint) cho từng màn", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll);
        for (int i = 0; i < levelCount; i++)
        {
            EditorGUILayout.LabelField("Level " + (lastLevel + 1 + i));
            hints[i] = EditorGUILayout.TextArea(hints[i], GUILayout.MinHeight(40));
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Tool sẽ copy scene mẫu, thêm vào Build Profiles, nối nextLevelName, thêm Hint " +
            "và tạo nút trong LevelSelect. Các scene liên quan sẽ được lưu tự động.\n" +
            "Sau đó hãy mở scene mới và chỉnh sửa bản đồ.", MessageType.Info);

        using (new EditorGUI.DisabledScope(templateScene == null || lastLevel == 0))
        {
            if (GUILayout.Button("Tạo " + levelCount + " màn mới", GUILayout.Height(32)))
                CreateLevels(lastLevel);
        }
    }

    private void CreateLevels(int lastLevel)
    {
        if (!EnsureNotPlaying()) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
        List<int> created;
        try
        {
            created = CreateLevelsFromTemplate(AssetDatabase.GetAssetPath(templateScene), hints, levelsPerChapter);
        }
        finally
        {
            if (previousSetup.Length > 0 && previousSetup.Any(s => !string.IsNullOrEmpty(s.path)))
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
        }

        if (created.Count > 0)
        {
            string range = created.Count == 1 ? "Level " + created[0] : $"Level {created[0]} → Level {created[created.Count - 1]}";
            EditorUtility.DisplayDialog("Thành công",
                $"Đã tạo {range}.\n\n" +
                "- Scene + Build Profiles: ✅\n" +
                "- nextLevelName + nhãn Level trên HUD: ✅\n" +
                "- Hint: ✅\n" +
                "- Nút LevelSelect: ✅\n\n" +
                "Hãy mở scene mới để thiết kế bản đồ.", "OK");

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(GetLevelScenePath(created[created.Count - 1])));
            hints = new List<string> { "" };
            levelCount = 1;
            templateScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(GetLevelScenePath(created[created.Count - 1]));
        }
    }

    /// <summary>
    /// Tạo các màn tiếp theo sau màn cuối hiện tại từ scene mẫu (mỗi phần tử trong hints là một màn).
    /// Dùng chung cho cửa sổ tool và các tool dựng màn tự động.
    /// </summary>
    public static List<int> CreateLevelsFromTemplate(string templatePath, IList<string> hints, int levelsPerChapter)
    {
        int lastLevel = GetLastLevelNumber();
        int levelCount = hints.Count;
        List<int> created = new List<int>();

        try
        {
            for (int i = 0; i < levelCount; i++)
            {
                int levelId = lastLevel + 1 + i;
                string newPath = GetLevelScenePath(levelId);

                EditorUtility.DisplayProgressBar("Tạo Level mới", "Đang tạo Level " + levelId, (float)i / levelCount);

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(newPath) != null)
                {
                    Debug.LogError($"❌ Scene {newPath} đã tồn tại nhưng chưa có trong Build Profiles. Hãy xóa hoặc thêm thủ công rồi chạy lại.");
                    break;
                }

                // 1. Copy scene mẫu
                if (!AssetDatabase.CopyAsset(templatePath, newPath))
                {
                    Debug.LogError("❌ Không copy được scene mẫu: " + templatePath);
                    break;
                }

                // 2. Thêm vào Build Profiles
                AddSceneToBuild(newPath);

                // 3. Nối nextLevelName
                SetNextLevelName(newPath, LevelSelectSceneName);
                SetNextLevelName(GetLevelScenePath(levelId - 1), "Level " + levelId);

                // 4. Thêm Hint
                AddHint("Level " + levelId, hints[i]);

                created.Add(levelId);
                Debug.Log($"✅ Đã tạo Level {levelId}");
            }

            // 5. Thêm nút trong LevelSelect
            if (created.Count > 0)
            {
                EditorUtility.DisplayProgressBar("Tạo Level mới", "Đang cập nhật LevelSelect", 1f);
                AddLevelSelectButtons(created, levelsPerChapter);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        return created;
    }

    // ==========================================
    // BUILD PROFILES
    // ==========================================
    private static void AddSceneToBuild(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == scenePath)) return;

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static int GetLastLevelNumber()
    {
        int max = 0;
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            Match match = LevelNameRegex.Match(System.IO.Path.GetFileNameWithoutExtension(scene.path));
            if (match.Success) max = Mathf.Max(max, int.Parse(match.Groups[1].Value));
        }
        return max;
    }

    private static string GetLevelScenePath(int levelId)
    {
        string expectedName = "Level " + levelId;
        EditorBuildSettingsScene existing = EditorBuildSettings.scenes
            .FirstOrDefault(s => System.IO.Path.GetFileNameWithoutExtension(s.path) == expectedName);
        return existing != null ? existing.path : $"{ScenesFolder}/{expectedName}.unity";
    }

    // ==========================================
    // LEVEL MANAGER + HINT
    // ==========================================
    private static void SetNextLevelName(string scenePath, string nextLevelName)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>(FindObjectsInactive.Include);
        if (levelManager == null)
        {
            Debug.LogWarning($"⚠ Không tìm thấy LevelManager trong {scenePath}, bỏ qua nextLevelName.");
            return;
        }

        SerializedObject so = new SerializedObject(levelManager);
        so.FindProperty("nextLevelName").stringValue = nextLevelName;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Scene copy từ màn mẫu → nhãn "Level N" trên HUD vẫn là số của màn mẫu
        SetHudLevelLabel(System.IO.Path.GetFileNameWithoutExtension(scenePath));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>Đặt chữ "Level N" trên HUD của scene đang mở.</summary>
    public static void SetHudLevelLabel(string levelName)
    {
        foreach (TextMeshProUGUI label in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include))
        {
            if (label.name != "Level number") continue;
            label.text = levelName;
            EditorUtility.SetDirty(label);
        }
    }

    public static void AddHint(string levelName, string hintMessage)
    {
        string[] guids = AssetDatabase.FindAssets("t:LevelDatabase");
        if (guids.Length == 0)
        {
            Debug.LogWarning("⚠ Không tìm thấy LevelDatabase, bỏ qua Hint.");
            return;
        }

        foreach (string guid in guids)
        {
            LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(AssetDatabase.GUIDToAssetPath(guid));
            List<LevelDatabase.LevelData> levels = (database.allLevels ?? new LevelDatabase.LevelData[0]).ToList();

            LevelDatabase.LevelData data = levels.FirstOrDefault(l => l.levelName == levelName);
            if (data == null)
            {
                data = new LevelDatabase.LevelData { levelName = levelName };
                levels.Add(data);
            }
            if (!string.IsNullOrWhiteSpace(hintMessage) || string.IsNullOrEmpty(data.hintMessage))
                data.hintMessage = string.IsNullOrWhiteSpace(hintMessage) ? "Hãy thử tương tác với giao diện xem sao!" : hintMessage;

            database.allLevels = levels.ToArray();
            EditorUtility.SetDirty(database);
        }
    }

    // ==========================================
    // LEVEL SELECT
    // ==========================================
    private static void AddLevelSelectButtons(List<int> levelIds, int levelsPerChapter)
    {
        var scene = EditorSceneManager.OpenScene(LevelSelectScenePath, OpenSceneMode.Single);
        LevelSelectManager manager = Object.FindAnyObjectByType<LevelSelectManager>(FindObjectsInactive.Include);
        if (manager == null)
        {
            Debug.LogError("❌ Không tìm thấy LevelSelectManager trong LevelSelect.");
            return;
        }

        List<Button> levelButtons = manager.levelButtons.ToList();
        List<GameObject> chapterPanels = manager.chapterPanels.ToList();
        List<Image> chapterButtons = manager.chapterButtons.ToList();
        bool addedChapter = false;

        foreach (int levelId in levelIds)
        {
            if (levelButtons.Count >= levelId && levelButtons[levelId - 1] != null)
            {
                Debug.Log($"Nút Level {levelId} đã có trong LevelSelect, bỏ qua.");
                continue;
            }

            int chapterIndex = (levelId - 1) / levelsPerChapter;

            // Tạo chương mới nếu cần (copy chương cuối)
            while (chapterPanels.Count <= chapterIndex)
            {
                CreateChapter(chapterPanels, chapterButtons);
                addedChapter = true;
            }

            // Copy nút ở cùng vị trí của chương trước (hoặc nút cuối cùng nếu không có)
            int sourceIndex = levelId - 1 - levelsPerChapter;
            Button source = sourceIndex >= 0 && sourceIndex < levelButtons.Count && levelButtons[sourceIndex] != null
                ? levelButtons[sourceIndex]
                : levelButtons.Last(b => b != null);

            Button newButton = Object.Instantiate(source, chapterPanels[chapterIndex].transform);
            newButton.name = "Level " + levelId;
            ((RectTransform)newButton.transform).anchoredPosition = ((RectTransform)source.transform).anchoredPosition;
            SetPersistentIntArgument(newButton, "LoadLevel", levelId);
            SetLabel(newButton.gameObject, levelId.ToString());
            // Nút copy từ nút cũ → cụm sao (nếu có) phải trỏ sang màn mới
            foreach (StarDisplay stars in newButton.GetComponentsInChildren<StarDisplay>(true))
                stars.levelId = levelId;

            while (levelButtons.Count < levelId - 1) levelButtons.Add(null);
            if (levelButtons.Count == levelId - 1) levelButtons.Add(newButton);
            else levelButtons[levelId - 1] = newButton;
        }

        manager.levelButtons = levelButtons.ToArray();
        manager.chapterPanels = chapterPanels.ToArray();
        manager.chapterButtons = chapterButtons.ToArray();
        EditorUtility.SetDirty(manager);

        if (addedChapter) SetupChapterScrollBar(chapterButtons);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✅ Đã cập nhật LevelSelect.");
    }

    private static void CreateChapter(List<GameObject> chapterPanels, List<Image> chapterButtons)
    {
        int chapterNumber = chapterPanels.Count + 1;

        // Panel chương: copy panel cuối, xóa các nút Level cũ bên trong (giữ nền, Arrow...)
        GameObject lastPanel = chapterPanels.Last();
        GameObject newPanel = Object.Instantiate(lastPanel, lastPanel.transform.parent);
        newPanel.name = "Panel_Chapter_" + chapterNumber;
        newPanel.SetActive(false);
        foreach (Button oldButton in newPanel.GetComponentsInChildren<Button>(true))
        {
            if (LevelNameRegex.IsMatch(oldButton.name))
                Object.DestroyImmediate(oldButton.gameObject);
        }
        chapterPanels.Add(newPanel);

        // Nút chương: copy nút cuối (vị trí do thanh cuộn tự sắp xếp)
        Image lastButton = chapterButtons.Last();
        Image newButton = Object.Instantiate(lastButton, lastButton.transform.parent);
        newButton.name = "ChapterBtn_" + chapterNumber;

        Button button = newButton.GetComponent<Button>();
        if (button != null) SetPersistentIntArgument(button, "SelectChapter", chapterNumber);
        SetLabel(newButton.gameObject, "Chương " + chapterNumber);
        chapterButtons.Add(newButton);

        Debug.Log($"✅ Đã tạo Chương {chapterNumber}");
    }

    // Thiết kế gốc: nút Chương scale 0.8, cách lề 68, cách nhau 54 → vừa 5 nút trên màn rộng 1920
    private const float ChapterButtonScale = 0.8f;
    private const int ChapterBarPadding = 68;
    private const float ChapterBarSpacing = 54f;

    // Vị trí khung be (thanh Chương) vẽ sẵn trong ảnh nền mapOrigin.jpg, tính theo % chiều rộng ảnh
    private const float BannerLeftRatio = 0.0433f;
    private const float BannerRightRatio = 0.9593f;
    private const float BannerInset = 8f;

    [MenuItem("Tools/Sửa thanh Chương (cuộn ngang)")]
    public static void SetupChapterScrollBarMenu()
    {
        if (!EnsureNotPlaying()) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != LevelSelectScenePath)
            scene = EditorSceneManager.OpenScene(LevelSelectScenePath, OpenSceneMode.Single);

        LevelSelectManager manager = Object.FindAnyObjectByType<LevelSelectManager>(FindObjectsInactive.Include);
        if (manager == null || manager.chapterButtons == null || manager.chapterButtons.Length == 0)
        {
            Debug.LogError("❌ Không tìm thấy LevelSelectManager hoặc nút Chương.");
            return;
        }

        SetupChapterScrollBar(manager.chapterButtons.Where(b => b != null).ToList());
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    /// <summary>
    /// Biến TopBar thành thanh cuộn ngang:
    /// TopBar (ScrollRect) → Viewport (RectMask2D) → Content (HorizontalLayoutGroup) → các nút Chương.
    /// Chạy lại nhiều lần vẫn an toàn (chỉ tạo Viewport/Content ở lần đầu).
    /// </summary>
    private static void SetupChapterScrollBar(List<Image> chapterButtons)
    {
        Transform parent = chapterButtons[0].transform.parent;
        ScrollRect scrollRect = parent.GetComponentInParent<ScrollRect>(true);
        RectTransform content;

        if (scrollRect != null && scrollRect.content == parent)
        {
            content = scrollRect.content;
        }
        else
        {
            RectTransform topBar = (RectTransform)parent;

            RectTransform viewport = CreateRect("Viewport", topBar);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewport.gameObject.AddComponent<RectMask2D>();

            content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0, 0);
            content.anchorMax = new Vector2(0, 1);
            content.pivot = new Vector2(0, 0.5f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            scrollRect = GetOrAdd<ScrollRect>(topBar.gameObject);
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.viewport = viewport;
            scrollRect.content = content;
        }

        FitViewportToBanner(scrollRect.viewport, (RectTransform)scrollRect.transform);

        HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(content.gameObject);
        layout.padding = new RectOffset(ChapterBarPadding, ChapterBarPadding, 0, 0);
        layout.spacing = ChapterBarSpacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childScaleWidth = true;
        layout.childScaleHeight = true;

        ContentSizeFitter fitter = GetOrAdd<ContentSizeFitter>(content.gameObject);
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        for (int i = 0; i < chapterButtons.Count; i++)
        {
            RectTransform rect = chapterButtons[i].rectTransform;
            rect.SetParent(content, false);
            rect.SetSiblingIndex(i);
            rect.localScale = new Vector3(ChapterButtonScale, ChapterButtonScale, 1);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Debug.Log($"✅ Thanh Chương dạng cuộn ngang: {chapterButtons.Count} chương.");
    }

    /// <summary>
    /// Khung be của thanh Chương nằm trong ảnh nền "BG" (kích thước cố định), còn TopBar co giãn theo màn hình.
    /// → Thu Viewport lại đúng bằng khung be để nút không tràn ra ngoài khi màn hình rộng.
    /// </summary>
    private static void FitViewportToBanner(RectTransform viewport, RectTransform topBar)
    {
        // Image trong suốt để kéo được cả ở khoảng trống giữa các nút
        Image dragArea = GetOrAdd<Image>(viewport.gameObject);
        dragArea.color = Color.clear;

        // Làm mờ dần 2 mép để người chơi biết còn chương để cuộn
        GetOrAdd<RectMask2D>(viewport.gameObject).softness = new Vector2Int(24, 0);

        RectTransform bg = topBar.parent != null ? topBar.parent.Find("BG") as RectTransform : null;
        if (bg == null)
        {
            Debug.LogWarning("⚠ Không tìm thấy ảnh nền 'BG' → Viewport giữ nguyên chiều rộng TopBar.");
            return;
        }

        Rect bgRect = bg.rect;
        Vector3 leftWorld = bg.TransformPoint(new Vector3(bgRect.xMin + bgRect.width * BannerLeftRatio, 0, 0));
        Vector3 rightWorld = bg.TransformPoint(new Vector3(bgRect.xMin + bgRect.width * BannerRightRatio, 0, 0));
        float left = topBar.InverseTransformPoint(leftWorld).x + BannerInset;
        float right = topBar.InverseTransformPoint(rightWorld).x - BannerInset;

        viewport.anchorMin = new Vector2(0.5f, 0);
        viewport.anchorMax = new Vector2(0.5f, 1);
        viewport.pivot = new Vector2(0.5f, 0.5f);
        viewport.sizeDelta = new Vector2(right - left, 0);
        viewport.anchoredPosition = new Vector2((left + right) / 2f - topBar.rect.center.x, 0);
    }

    /// <summary>
    /// Unity không cho mở/lưu scene khi đang Play → báo người dùng thoát Play mode trước.
    /// </summary>
    private static bool EnsureNotPlaying()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;

        EditorUtility.DisplayDialog("Đang ở Play mode",
            "Không thể sửa scene khi game đang chạy.\nHãy bấm nút ▶ Play lần nữa để dừng game, rồi chạy lại lệnh này.", "OK");
        return false;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        RectTransform rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void SetPersistentIntArgument(Button owner, string methodName, int value)
    {
        SerializedObject so = new SerializedObject(owner);
        SerializedProperty calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        for (int i = 0; i < calls.arraySize; i++)
        {
            SerializedProperty call = calls.GetArrayElementAtIndex(i);
            if (call.FindPropertyRelative("m_MethodName").stringValue == methodName)
                call.FindPropertyRelative("m_Arguments.m_IntArgument").intValue = value;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetLabel(GameObject root, string text)
    {
        TextMeshProUGUI label = root.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.text = text;
            EditorUtility.SetDirty(label);
        }
    }
}
