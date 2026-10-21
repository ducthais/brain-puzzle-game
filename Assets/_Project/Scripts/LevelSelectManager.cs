using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [Header("--- CHAPTER MANAGEMENT ---")]
    [Tooltip("Kéo thả Panel_Chapter_1, Panel_Chapter_2... vào đây")]
    public GameObject[] chapterPanels;

    [Tooltip("Kéo thả Component Image của các nút ChapterBtn_1, ChapterBtn_2... vào đây")]
    public Image[] chapterButtons;

    [Tooltip("Màu của nút khi được chọn (Sáng)")]
    public Color activeColor = Color.white;

    [Tooltip("Màu của nút khi không được chọn (Tối đi)")]
    public Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Màu xám

    [Header("--- LEVEL BUTTON SPRITES ---")]
    public Sprite greenUnlocked;
    public Sprite greenCompleted;
    public Sprite greenLocked;

    public Sprite purpleUnlocked;
    public Sprite purpleCompleted;
    public Sprite purpleLocked;

    [Header("--- LEVEL BUTTONS ---")]
    [Tooltip("Kéo thả TẤT CẢ nút Level theo thứ tự (Level 1 → Level 20) vào đây")]
    public Button[] levelButtons;

    [Header("--- UI PANELS ---")]
    public GameObject shopCanvas;   // Kéo thả object ShopCanvas từ Hierarchy
    public GameObject panelSetting; // Kéo thả object PanelSetting từ Hierarchy

    private void Start()
    {
        // Khi vừa mở màn hình Level Select, mặc định chọn Chương 1
        SelectChapter(1);

        // Đảm bảo Shop và Setting đang tắt lúc mới vào
        if (shopCanvas != null) shopCanvas.SetActive(false);
        if (panelSetting != null) panelSetting.SetActive(false);

        // Ẩn tất cả các đường nối (Arrow) giữa các màn
        HideAllArrows();

        // Cập nhật trạng thái khóa/mở khóa cho tất cả nút Level
        RefreshLevelButtons();
    }

    // ==========================================
    // ẨN TẤT CẢ ARROW (ĐƯỜNG NỐI ĐEN)
    // ==========================================
    private void HideAllArrows()
    {
        // Tìm tất cả Image trong scene, ẩn những object có tên chứa "Arrow"
        Image[] allImages = FindObjectsOfType<Image>(true);
        foreach (Image img in allImages)
        {
            if (img.gameObject.name.Contains("Arrow"))
            {
                img.gameObject.SetActive(false);
            }
        }
    }

    // ==========================================
    // 0. CẬP NHẬT TRẠNG THÁI KHÓA/MỞ KHÓA
    // ==========================================
    public void RefreshLevelButtons()
    {
        // Level cao nhất đã mở khóa (mặc định = 1, tức Level 1 luôn mở)
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;

            int levelId = i + 1; // Mảng bắt đầu từ 0, Level bắt đầu từ 1
            bool isUnlocked = levelId <= maxUnlocked;
            bool isCompleted = levelId < maxUnlocked; // Màn đã hoàn thành

            // KHÔNG dùng interactable = false (vì nó làm xám nút)
            // Giữ interactable = true, kiểm tra khóa trong LoadLevel()
            levelButtons[i].interactable = true;

            // Đổi hình ảnh dựa trên trạng thái
            Image btnImage = levelButtons[i].GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = Color.white; // Reset màu về trắng để hiện đúng sprite
                btnImage.preserveAspect = true; // Giữ tỷ lệ ảnh, không bị méo

                bool isPurple = (levelId % 4 == 0); // Màn 4, 8, 12...

                if (isPurple)
                {
                    if (isCompleted) btnImage.sprite = purpleCompleted;
                    else if (isUnlocked) btnImage.sprite = purpleUnlocked;
                    else btnImage.sprite = purpleLocked;
                }
                else
                {
                    if (isCompleted) btnImage.sprite = greenCompleted;
                    else if (isUnlocked) btnImage.sprite = greenUnlocked;
                    else btnImage.sprite = greenLocked;
                }
            }
        }
    }

    // ==========================================
    // 1. LOGIC CHỌN CHƯƠNG VÀ LÀM SÁNG NÚT
    // ==========================================
    public void SelectChapter(int chapterIndex)
    {
        int arrayIndex = chapterIndex - 1; // Vì mảng bắt đầu từ 0

        for (int i = 0; i < chapterPanels.Length; i++)
        {
            // Bật panel Level tương ứng, tắt các panel khác
            if (chapterPanels[i] != null)
            {
                chapterPanels[i].SetActive(i == arrayIndex);
            }

            // Đổi màu làm sáng nút được chọn và làm tối nút không được chọn
            if (i < chapterButtons.Length && chapterButtons[i] != null)
            {
                chapterButtons[i].color = (i == arrayIndex) ? activeColor : inactiveColor;
            }
        }
    }

    // ==========================================
    // 2. LOGIC LOAD LEVEL
    // ==========================================
    public void LoadLevel(int levelId)
    {
        // Kiểm tra level đã mở khóa chưa trước khi load
        int maxUnlocked = PlayerPrefs.GetInt("MaxUnlockedLevel", 1);
        if (levelId > maxUnlocked)
        {
            Debug.Log("Level " + levelId + " chưa được mở khóa!");
            return; // Không cho phép vào level chưa mở khóa
        }

        string sceneName = "Level " + levelId; // Đảm bảo tên Scene khớp với định dạng này
        SceneManager.LoadScene(sceneName);
    }

    // ==========================================
    // 3. LOGIC SHOP
    // ==========================================
    public void ToggleShop(bool isOpen)
    {
        if (shopCanvas != null)
        {
            shopCanvas.SetActive(isOpen);
            // Lưu ý: Việc bật/tắt Panel_NonPremium, Panel_IsPremium, Panel_QrCode 
            // nên được quản lý bởi một script riêng (VD: PremiumManager) gắn trên ShopCanvas.
        }
    }

    // ==========================================
    // 4. LOGIC SETTINGS
    // ==========================================
    public void ToggleSetting(bool isOpen)
    {
        if (panelSetting != null)
        {
            panelSetting.SetActive(isOpen);
        }
    }

    // Các hàm dành cho nút bên trong PanelSetting
    public void ToggleMusic()
    {
        Debug.Log("Chuyển đổi trạng thái Bật/Tắt Nhạc");
        // Bạn gọi logic AudioManager của bạn ở đây
    }

    public void ToggleSound()
    {
        Debug.Log("Chuyển đổi trạng thái Bật/Tắt Âm thanh (SFX)");
        // Bạn gọi logic AudioManager của bạn ở đây
    }
}