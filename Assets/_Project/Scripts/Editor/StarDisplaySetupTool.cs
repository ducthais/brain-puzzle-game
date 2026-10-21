using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tự động dựng cụm 3 sao (StarDisplay) cho bảng thắng và các nút chọn màn,
/// thay cho các bước thủ công trong docs/HUONG_DAN_UI_SAO.md.
/// </summary>
public static class StarDisplaySetupTool
{
    private const string WinCanvasPrefabPath = "Assets/_Project/Prefabs/WinCanvas.prefab";
    private const string LevelSelectScenePath = "Assets/_Project/Scenes/LevelSelect.unity";
    private const string StarSpritePath = "Assets/_Project/Sprites/UI/level_button/star.png";

    private const string WinStarsName = "StarsContainer";
    private const string ButtonStarsName = "Stars";

    // Vị trí mặc định trên bảng thắng: giữa khung nội dung và nút "Tiếp tục"
    private static readonly Vector2 WinStarsPosition = new Vector2(0f, -185f);
    private const float WinStarSize = 70f;
    private const float WinStarSpacing = 18f;

    // Sao trên nút chọn màn: nằm sát mép dưới nút, cạnh sao = 28% chiều cao nút
    private const float ButtonStarRatio = 0.28f;
    private const float ButtonStarSpacing = 4f;

    private static readonly Color ActiveColor = new Color(1f, 0.84f, 0f, 1f);          // #FFD700
    private static readonly Color InactiveColor = new Color(0.25f, 0.25f, 0.25f, 0.4f); // #40404066

    [MenuItem("Tools/UI Sao/Gắn sao vào bảng thắng (mọi màn)")]
    public static void SetupWinCanvas()
    {
        if (!EnsureNotPlaying()) return;

        Sprite star = LoadStarSprite();
        if (star == null) return;

        GameObject root = PrefabUtility.LoadPrefabContents(WinCanvasPrefabPath);
        try
        {
            if (root.GetComponentInChildren<StarDisplay>(true) != null)
            {
                Debug.Log("⚠ WinCanvas đã có StarDisplay, bỏ qua. Muốn dựng lại thì xóa StarsContainer trong prefab trước.");
                return;
            }

            Transform panel = root.transform.Find("Panel");
            Transform parent = panel != null ? panel : root.transform;

            StarDisplay display = CreateStarGroup(parent, WinStarsName, WinStarSize, WinStarSpacing, star);
            display.mode = StarDisplay.DisplayMode.WinPanel;

            RectTransform rect = (RectTransform)display.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = WinStarsPosition;

            PrefabUtility.SaveAsPrefabAsset(root, WinCanvasPrefabPath);
            Debug.Log($"✅ Đã gắn 3 sao vào {WinCanvasPrefabPath} — áp dụng cho mọi màn dùng prefab này. " +
                      "Mở prefab để chỉnh vị trí StarsContainer nếu cần.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    [MenuItem("Tools/UI Sao/Gắn sao vào nút chọn màn (LevelSelect)")]
    public static void SetupLevelSelectButtons()
    {
        if (!EnsureNotPlaying()) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Sprite star = LoadStarSprite();
        if (star == null) return;

        var scene = EditorSceneManager.OpenScene(LevelSelectScenePath, OpenSceneMode.Single);
        LevelSelectManager manager = Object.FindAnyObjectByType<LevelSelectManager>(FindObjectsInactive.Include);
        if (manager == null || manager.levelButtons == null)
        {
            Debug.LogError("❌ Không tìm thấy LevelSelectManager (hoặc danh sách levelButtons) trong LevelSelect.");
            return;
        }

        int created = 0, updated = 0;
        for (int i = 0; i < manager.levelButtons.Length; i++)
        {
            Button button = manager.levelButtons[i];
            if (button == null) continue;
            int levelId = i + 1;

            StarDisplay display = button.GetComponentInChildren<StarDisplay>(true);
            if (display == null)
            {
                RectTransform buttonRect = (RectTransform)button.transform;
                float size = Mathf.Max(12f, Mathf.Round(buttonRect.rect.height * ButtonStarRatio));

                display = CreateStarGroup(button.transform, ButtonStarsName, size, ButtonStarSpacing, star);
                RectTransform rect = (RectTransform)display.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, size * 0.25f);
                created++;
            }
            else
            {
                updated++;
            }

            display.mode = StarDisplay.DisplayMode.LevelSelectButton;
            display.levelId = levelId;
            EditorUtility.SetDirty(display);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"✅ LevelSelect: tạo mới {created} cụm sao, cập nhật số màn cho {updated} cụm đã có.");
    }

    private static StarDisplay CreateStarGroup(Transform parent, string name, float starSize, float spacing, Sprite star)
    {
        GameObject group = new GameObject(name, typeof(RectTransform));
        group.layer = parent.gameObject.layer;
        RectTransform groupRect = (RectTransform)group.transform;
        groupRect.SetParent(parent, false);
        groupRect.sizeDelta = new Vector2(starSize * 3f + spacing * 2f, starSize);

        HorizontalLayoutGroup layout = group.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = layout.childControlHeight = false;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;

        StarDisplay display = group.AddComponent<StarDisplay>();
        display.starImages = new Image[3];
        display.starActiveSprite = star;
        display.activeColor = ActiveColor;
        display.inactiveColor = InactiveColor;

        for (int i = 0; i < 3; i++)
        {
            GameObject starObj = new GameObject("Star_" + (i + 1), typeof(RectTransform));
            starObj.layer = group.layer;
            RectTransform starRect = (RectTransform)starObj.transform;
            starRect.SetParent(groupRect, false);
            starRect.sizeDelta = new Vector2(starSize, starSize);

            Image image = starObj.AddComponent<Image>();
            image.sprite = star;
            image.preserveAspect = true;
            image.raycastTarget = false; // Không chặn cú bấm vào nút chọn màn
            image.color = InactiveColor;
            display.starImages[i] = image;
        }

        return display;
    }

    private static Sprite LoadStarSprite()
    {
        Sprite star = AssetDatabase.LoadAllAssetsAtPath(StarSpritePath).OfType<Sprite>().FirstOrDefault();
        if (star == null) Debug.LogError($"❌ Không tìm thấy sprite sao tại {StarSpritePath}.");
        return star;
    }

    private static bool EnsureNotPlaying()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode) return true;

        EditorUtility.DisplayDialog("Đang ở Play mode",
            "Không thể sửa scene/prefab khi game đang chạy.\nHãy bấm nút ▶ Play lần nữa để dừng game, rồi chạy lại lệnh này.", "OK");
        return false;
    }
}
