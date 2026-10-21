using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class UpdateLevelSelectScene
{
    [MenuItem("Tools/Cập nhật Giao diện LevelSelect")]
    public static void UpdateScene()
    {
        // Đảm bảo đang mở scene LevelSelect
        UnityEngine.SceneManagement.Scene currentScene = EditorSceneManager.GetActiveScene();
        if (!currentScene.name.Equals("LevelSelect"))
        {
            if (EditorUtility.DisplayDialog("Sai Scene", "Vui lòng mở Scene 'LevelSelect' trước khi chạy công cụ này.", "Mở Scene LevelSelect", "Hủy"))
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/LevelSelect.unity");
            }
            else
            {
                return;
            }
        }

        // Tải sub-sprite lớn nhất (đúng) từ mỗi file Multiple Sprite
        Sprite green = LoadLargestSubSprite("Assets/_Project/Sprites/Selection/Green.png");
        Sprite correctGreen = LoadLargestSubSprite("Assets/_Project/Sprites/Selection/CorrectGreen.png");
        Sprite hidelGreen = LoadLargestSubSprite("Assets/_Project/Sprites/Selection/HidelGreen.png");

        Sprite purple = LoadLargestSubSprite("Assets/_Project/Sprites/Selection/Purple.png");
        Sprite correctPurple = LoadLargestSubSprite("Assets/_Project/Sprites/Selection/CorrectPurple.png");
        Sprite hidePurple = LoadLargestSubSprite("Assets/_Project/Sprites/Selection/HidePurple.png");

        Sprite chapterButtonSprite = LoadLargestSubSprite("Assets/_Project/Sprites/Selection/chapterButon.png");
        Sprite mapOriginSprite = LoadLargestSubSprite("Assets/_Project/Sprites/Selection/mapOrigin.jpg");

        // Kiểm tra từng sprite
        if (green == null) Debug.LogError("Không tìm thấy sub-sprite Green!");
        if (correctGreen == null) Debug.LogError("Không tìm thấy sub-sprite CorrectGreen!");
        if (hidelGreen == null) Debug.LogError("Không tìm thấy sub-sprite HidelGreen!");
        if (purple == null) Debug.LogError("Không tìm thấy sub-sprite Purple!");
        if (correctPurple == null) Debug.LogError("Không tìm thấy sub-sprite CorrectPurple!");
        if (hidePurple == null) Debug.LogError("Không tìm thấy sub-sprite HidePurple!");
        if (chapterButtonSprite == null) Debug.LogError("Không tìm thấy sub-sprite chapterButon!");
        if (mapOriginSprite == null) Debug.LogError("Không tìm thấy sub-sprite mapOrigin!");

        if (green == null || purple == null || chapterButtonSprite == null || mapOriginSprite == null ||
            correctGreen == null || correctPurple == null || hidelGreen == null || hidePurple == null)
        {
            Debug.LogError("Thiếu tài nguyên! Kiểm tra Console để biết chi tiết.");
            return;
        }

        Debug.Log($"✅ Loaded sprites: Green({green.rect.width}x{green.rect.height}), " +
                  $"CorrectPurple({correctPurple.rect.width}x{correctPurple.rect.height}), " +
                  $"mapOrigin({mapOriginSprite.rect.width}x{mapOriginSprite.rect.height})");

        bool hasChanges = false;

        // 1. Cập nhật LevelSelectManager (Sử dụng API mới)
        LevelSelectManager manager = Object.FindAnyObjectByType<LevelSelectManager>();
        if (manager != null)
        {
            manager.greenUnlocked = green;
            manager.greenCompleted = correctGreen;
            manager.greenLocked = hidelGreen;

            manager.purpleUnlocked = purple;
            manager.purpleCompleted = correctPurple;
            manager.purpleLocked = hidePurple;

            EditorUtility.SetDirty(manager);
            hasChanges = true;
            Debug.Log("✅ Đã cập nhật các field Sprite trong LevelSelectManager.");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy LevelSelectManager trong Scene.");
        }

        // 2. Cập nhật các nút Chapter → dùng chapterButon.png
        if (manager != null && manager.chapterButtons != null)
        {
            foreach (Image btn in manager.chapterButtons)
            {
                if (btn != null)
                {
                    btn.sprite = chapterButtonSprite;
                    btn.color = Color.white;
                    btn.preserveAspect = true;
                    EditorUtility.SetDirty(btn);
                    hasChanges = true;
                }
            }
            Debug.Log("✅ Đã cập nhật Sprite cho các nút Chapter.");
        }

        // 3. XÓA HorizontalLayoutGroup trên TopBar → cho phép di chuyển/scale nút Chapter tự do
        GameObject topBarObj = GameObject.Find("TopBar");
        if (topBarObj != null)
        {
            HorizontalLayoutGroup hlg = topBarObj.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                // Phải xóa hoàn toàn component này, vì dù tắt childControl nó vẫn tự sắp xếp con
                Object.DestroyImmediate(hlg);
                EditorUtility.SetDirty(topBarObj);
                hasChanges = true;
                Debug.Log("✅ Đã XÓA HorizontalLayoutGroup trên TopBar → nút Chapter giờ di chuyển/scale tự do.");
            }
            else
            {
                Debug.Log("TopBar đã không còn HorizontalLayoutGroup (đã xóa trước đó).");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy TopBar.");
        }

        // 4. Cập nhật tất cả Level Button: preserveAspect + sprite đúng
        if (manager != null && manager.levelButtons != null)
        {
            int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
            for (int i = 0; i < manager.levelButtons.Length; i++)
            {
                if (manager.levelButtons[i] == null) continue;

                int levelId = i + 1;
                bool isUnlocked = levelId <= maxUnlocked;
                bool isCompleted = levelId < maxUnlocked;
                bool isPurple = (levelId % 4 == 0);

                Image btnImage = manager.levelButtons[i].GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = Color.white;
                    btnImage.preserveAspect = true;

                    if (isPurple)
                    {
                        if (isCompleted) btnImage.sprite = correctPurple;
                        else if (isUnlocked) btnImage.sprite = purple;
                        else btnImage.sprite = hidePurple;
                    }
                    else
                    {
                        if (isCompleted) btnImage.sprite = correctGreen;
                        else if (isUnlocked) btnImage.sprite = green;
                        else btnImage.sprite = hidelGreen;
                    }

                    // Giữ interactable = true (không làm xám)
                    manager.levelButtons[i].interactable = true;

                    // Sửa ColorBlock để mọi trạng thái đều hiện màu trắng
                    ColorBlock cb = manager.levelButtons[i].colors;
                    cb.disabledColor = Color.white;
                    manager.levelButtons[i].colors = cb;

                    EditorUtility.SetDirty(btnImage);
                    EditorUtility.SetDirty(manager.levelButtons[i]);
                    hasChanges = true;
                }
            }
            Debug.Log("✅ Đã cập nhật Sprite + preserveAspect cho tất cả nút Level.");
        }

        // 5. Ẩn tất cả Arrow (đường nối đen giữa các màn) sử dụng API mới
        int arrowCount = 0;
        Image[] allImages = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Image img in allImages)
        {
            if (img.gameObject.name.Contains("Arrow"))
            {
                img.gameObject.SetActive(false);
                EditorUtility.SetDirty(img.gameObject);
                arrowCount++;
                hasChanges = true;
            }
        }
        Debug.Log($"✅ Đã ẩn {arrowCount} Arrow (đường nối đen).");

        // 6. Cập nhật Background cho tất cả Panel_Chapter
        int bgCount = 0;
        foreach (Image img in allImages)
        {
            if (img.gameObject.name == "Image")
            {
                Transform parent = img.transform.parent;
                if (parent != null)
                {
                    string parentName = parent.gameObject.name;
                    if (parentName.Contains("Panel_Chapter") || parentName.Contains("Canvas") || parentName.Contains("PanelSetting"))
                    {
                        img.sprite = mapOriginSprite;
                        img.color = Color.white;
                        img.preserveAspect = false;
                        img.type = Image.Type.Simple;
                        EditorUtility.SetDirty(img);
                        bgCount++;
                        hasChanges = true;
                        Debug.Log($"  → Đã đổi background trên: {parentName}/{img.gameObject.name}");
                    }
                }
            }
        }

        if (bgCount == 0)
        {
            foreach (Image img in allImages)
            {
                RectTransform rect = img.rectTransform;
                if (rect.sizeDelta.x > 1000 && rect.sizeDelta.y > 500 &&
                    !img.gameObject.name.Contains("Level") &&
                    !img.gameObject.name.Contains("Chapter") &&
                    !img.gameObject.name.Contains("Arrow") &&
                    !img.gameObject.name.Contains("Panel"))
                {
                    img.sprite = mapOriginSprite;
                    img.color = Color.white;
                    img.type = Image.Type.Simple;
                    EditorUtility.SetDirty(img);
                    bgCount++;
                    hasChanges = true;
                    Debug.Log($"  → Đã đổi background (kích thước lớn) trên: {img.gameObject.name} (size: {rect.sizeDelta})");
                }
            }
        }

        if (bgCount > 0)
            Debug.Log($"✅ Đã cập nhật {bgCount} Background thành mapOrigin.");
        else
            Debug.LogWarning("⚠ Không tìm thấy Background tự động. Hãy kéo mapOrigin vào Image nền thủ công.");

        if (hasChanges)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("🎉 Quá trình cập nhật hoàn tất! Hãy lưu Scene (Ctrl+S).");
            EditorUtility.DisplayDialog("Thành công",
                $"Đã cập nhật giao diện thành công!\n" +
                $"- Sprite Level (sub-sprite lớn nhất): ✅\n" +
                $"- Chapter Button: ✅\n" +
                $"- TopBar Layout tắt ChildControl: ✅\n" +
                $"- Arrow ẩn: {arrowCount}\n" +
                $"- Background: {bgCount}\n\n" +
                "Nút Chapter giờ có thể chỉnh scale độc lập!\n" +
                "Hãy bấm Ctrl+S để lưu Scene.", "OK");
        }
    }

    /// <summary>
    /// Load sub-sprite có kích thước lớn nhất từ file Multiple Sprite.
    /// Vì các file sprite trong Selection đều được chia thành nhiều sub-sprite,
    /// trong đó có 1 sprite nhỏ vô nghĩa (5x6px) và 1 sprite đúng (lớn).
    /// </summary>
    private static Sprite LoadLargestSubSprite(string assetPath)
    {
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        Sprite largest = null;
        float maxArea = 0;

        foreach (Object asset in allAssets)
        {
            if (asset is Sprite sprite)
            {
                float area = sprite.rect.width * sprite.rect.height;
                if (area > maxArea)
                {
                    maxArea = area;
                    largest = sprite;
                }
            }
        }

        if (largest != null)
        {
            Debug.Log($"  Loaded: {assetPath} → {largest.name} ({largest.rect.width}x{largest.rect.height})");
        }
        else
        {
            Debug.LogError($"  Không tìm thấy Sprite nào trong: {assetPath}");
        }

        return largest;
    }
}