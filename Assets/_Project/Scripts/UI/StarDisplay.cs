using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý hiển thị cụm 3 ngôi sao đánh giá thành tích:
/// - Chế độ WinPanel: Tự động đọc LevelManager.LastStars khi bảng chiến thắng được bật lên.
/// - Chế độ LevelSelectButton: Đọc ProgressService.GetStars(levelId) và tự động cập nhật khi có sự kiện ProgressService.Changed.
/// </summary>
public class StarDisplay : MonoBehaviour
{
    public enum DisplayMode
    {
        [Tooltip("Gắn trên WinPanel của màn chơi: đọc số sao vừa đạt được từ LevelManager")]
        WinPanel,

        [Tooltip("Gắn trên nút chọn màn ở LevelSelect: hiển thị số sao kỷ lục đã lưu")]
        LevelSelectButton
    }

    [Header("--- CẤU HÌNH CHẾ ĐỘ HIỂN THỊ ---")]
    [Tooltip("Chọn ngữ cảnh hiển thị: trên bảng chiến thắng (WinPanel) hoặc trên nút chọn màn")]
    public DisplayMode mode = DisplayMode.WinPanel;

    [Tooltip("Số thứ tự màn chơi (chỉ dùng cho chế độ LevelSelectButton, VD: 1 cho Level 1)")]
    public int levelId = 1;

    [Header("--- DANH SÁCH ẢNH SAO (3 NGÔI SAO) ---")]
    [Tooltip("Kéo thả 3 Image tương ứng với Star 1, Star 2, Star 3 vào đây")]
    public Image[] starImages = new Image[3];

    [Header("--- TÙY CHỌN HÌNH ẢNH SPRITE ---")]
    [Tooltip("Sprite ngôi sao sáng (đã đạt được). Gợi ý: Assets/_Project/Sprites/UI/level_button/star.png")]
    public Sprite starActiveSprite;

    [Tooltip("Sprite ngôi sao tối/mờ (chưa đạt). Nếu để trống, script sẽ tự đổi màu activeColor / inactiveColor")]
    public Sprite starInactiveSprite;

    [Header("--- TÙY CHỌN MÀU SẮC (KHI KHÔNG ĐỔI SPRITE) ---")]
    [Tooltip("Màu của sao khi đạt (mặc định: Trắng sáng hoặc Vàng)")]
    public Color activeColor = Color.white;

    [Tooltip("Màu của sao khi chưa đạt (mặc định: Xám mờ / Trong suốt một phần)")]
    public Color inactiveColor = new Color(0.25f, 0.25f, 0.25f, 0.4f);

    private void Start()
    {
        RefreshDisplay();
    }

    private void OnEnable()
    {
        if (mode == DisplayMode.LevelSelectButton)
        {
            ProgressService.Changed += OnProgressChanged;
        }

        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (mode == DisplayMode.LevelSelectButton)
        {
            ProgressService.Changed -= OnProgressChanged;
        }
    }

    private void OnProgressChanged(ProgressData data)
    {
        RefreshDisplay();
    }

    /// <summary>
    /// Làm mới giao diện sao dựa trên chế độ hiện tại.
    /// </summary>
    public void RefreshDisplay()
    {
        int starsCount = 0;

        if (mode == DisplayMode.WinPanel)
        {
            LevelManager levelManager = Object.FindAnyObjectByType<LevelManager>();
            if (levelManager != null)
            {
                starsCount = levelManager.LastStars;
            }
        }
        else if (mode == DisplayMode.LevelSelectButton)
        {
            starsCount = ProgressService.GetStars(levelId);
        }

        SetStars(starsCount);
    }

    /// <summary>
    /// Thiết lập trực tiếp số sao sáng (từ 0 đến 3).
    /// </summary>
    public void SetStars(int count)
    {
        count = Mathf.Clamp(count, 0, 3);

        if (starImages == null || starImages.Length == 0) return;

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;

            bool isActive = (i < count);

            // 1. Cập nhật Sprite nếu có cấu hình
            if (isActive && starActiveSprite != null)
            {
                starImages[i].sprite = starActiveSprite;
            }
            else if (!isActive && starInactiveSprite != null)
            {
                starImages[i].sprite = starInactiveSprite;
            }

            // 2. Cập nhật Màu sắc (Tint Color)
            starImages[i].color = isActive ? activeColor : inactiveColor;
        }
    }
}
