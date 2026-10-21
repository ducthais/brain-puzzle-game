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
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string templatePath = AssetDatabase.GetAssetPath(templateScene);
        SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
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
                AddLevelSelectButtons(created);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (previousSetup.Length > 0 && previousSetup.Any(s => !string.IsNullOrEmpty(s.path)))
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
        }

        AssetDatabase.SaveAssets();

        if (created.Count > 0)
        {
            string range = created.Count == 1 ? "Level " + created[0] : $"Level {created[0]} → Level {created[created.Count - 1]}";
            EditorUtility.DisplayDialog("Thành công",
                $"Đã tạo {range}.\n\n" +
                "- Scene + Build Profiles: ✅\n" +
                "- nextLevelName: ✅\n" +
                "- Hint: ✅\n" +
                "- Nút LevelSelect: ✅\n\n" +
                "Hãy mở scene mới để thiết kế bản đồ.\n" +
                "Nếu có Chương mới, hãy kiểm tra vị trí nút Chương trong LevelSelect.", "OK");

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(GetLevelScenePath(created[created.Count - 1])));
            hints = new List<string> { "" };
            levelCount = 1;
            templateScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(GetLevelScenePath(created[created.Count - 1]));
        }
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
        LevelManager levelManager = Object.FindFirstObjectByType<LevelManager>(FindObjectsInactive.Include);
        if (levelManager == null)
        {
            Debug.LogWarning($"⚠ Không tìm thấy LevelManager trong {scenePath}, bỏ qua nextLevelName.");
            return;
        }

        SerializedObject so = new SerializedObject(levelManager);
        so.FindProperty("nextLevelName").stringValue = nextLevelName;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void AddHint(string levelName, string hintMessage)
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
    private void AddLevelSelectButtons(List<int> levelIds)
    {
        var scene = EditorSceneManager.OpenScene(LevelSelectScenePath, OpenSceneMode.Single);
        LevelSelectManager manager = Object.FindFirstObjectByType<LevelSelectManager>(FindObjectsInactive.Include);
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

            while (levelButtons.Count < levelId - 1) levelButtons.Add(null);
            if (levelButtons.Count == levelId - 1) levelButtons.Add(newButton);
            else levelButtons[levelId - 1] = newButton;
        }

        manager.levelButtons = levelButtons.ToArray();
        manager.chapterPanels = chapterPanels.ToArray();
        manager.chapterButtons = chapterButtons.ToArray();
        EditorUtility.SetDirty(manager);

        if (addedChapter) FitChapterButtons(chapterButtons);

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

        // Nút chương: copy nút cuối, dịch sang phải một khoảng bằng khoảng cách giữa 2 nút cuối
        Image lastButton = chapterButtons.Last();
        Image newButton = Object.Instantiate(lastButton, lastButton.transform.parent);
        newButton.name = "ChapterBtn_" + chapterNumber;

        RectTransform lastRect = lastButton.rectTransform;
        float spacing = chapterButtons.Count >= 2
            ? lastRect.anchoredPosition.x - chapterButtons[chapterButtons.Count - 2].rectTransform.anchoredPosition.x
            : lastRect.rect.width;
        newButton.rectTransform.anchoredPosition = lastRect.anchoredPosition + new Vector2(spacing, 0);

        Button button = newButton.GetComponent<Button>();
        if (button != null) SetPersistentIntArgument(button, "SelectChapter", chapterNumber);
        SetLabel(newButton.gameObject, "Chương " + chapterNumber);
        chapterButtons.Add(newButton);

        Debug.Log($"✅ Đã tạo Chương {chapterNumber}");
    }

    /// <summary>
    /// Nếu các nút Chương tràn ra ngoài TopBar thì dàn đều lại và thu nhỏ cho vừa.
    /// </summary>
    private static void FitChapterButtons(List<Image> chapterButtons)
    {
        RectTransform topBar = chapterButtons[0].rectTransform.parent as RectTransform;
        if (topBar == null) return;

        float availableWidth = topBar.rect.width;
        RectTransform last = chapterButtons.Last().rectTransform;
        float rightEdge = last.anchoredPosition.x + last.rect.width * last.localScale.x * (1 - last.pivot.x);
        if (availableWidth <= 0 || rightEdge <= availableWidth) return;

        int count = chapterButtons.Count;
        float slotWidth = availableWidth / count;
        foreach (Image img in chapterButtons)
        {
            float scale = Mathf.Min(img.rectTransform.localScale.x, slotWidth * 0.95f / img.rectTransform.rect.width);
            img.rectTransform.localScale = new Vector3(scale, scale, 1);
        }
        for (int i = 0; i < count; i++)
        {
            RectTransform rect = chapterButtons[i].rectTransform;
            rect.anchoredPosition = new Vector2(slotWidth * (i + 0.5f), rect.anchoredPosition.y);
        }

        Debug.Log("ℹ Các nút Chương bị tràn TopBar → đã dàn đều và thu nhỏ lại. Có thể chỉnh tay nếu muốn.");
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
