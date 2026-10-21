using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("--- CƠ SỞ DỮ LIỆU ---")]
    public LevelDatabase levelDatabase;

    [Header("--- CÀI ĐẶT UI CƠ BẢN ---")]
    public GameObject winPanel;
    public GameObject pauseCanvas;
    public GameObject shopPanel;

    [Header("--- HỆ THỐNG HINT ---")]
    [Tooltip("Popup hỏi: Bạn có muốn xem quảng cáo lấy mẹo không?")]
    public GameObject hintCanvas1;
    [Tooltip("Bảng hiển thị nội dung mẹo (Gợi ý thực sự)")]
    public GameObject hintCanvas2;
    public TextMeshProUGUI hintText2;

    [Header("--- CẤU HÌNH TÊN SCENE ---")]
    public string levelSelectSceneName = "LevelSelect";
    public string nextLevelName = "Level 2";

    private bool isGameWon = false;

    // BIẾN MỚI: Theo dõi trạng thái đã mở khóa Hint ở màn chơi hiện tại hay chưa
    private bool isHintUnlockedCurrentLevel = false;

    /// <summary>Số sao đạt được ở lần thắng gần nhất (UI hiển thị sao đọc giá trị này).</summary>
    public int LastStars { get; private set; }

    private void Start()
    {
        // Khi nạp vào màn chơi mới, luôn đặt trạng thái chưa mở khóa Hint
        isHintUnlockedCurrentLevel = false;

        // Bắt đầu (hoặc tiếp tục sau khi chết) lượt chơi để đếm số lần chết / xem gợi ý
        if (LevelNaming.TryParseLevelNumber(SceneManager.GetActiveScene().name, out int levelId))
        {
            LevelSession session = LevelSession.Begin(levelId, Time.realtimeSinceStartup);
            GameAnalytics.Reporter.LevelStart(levelId, session.Attempt);
        }
    }

    // Ấn nút Hint trên HUD màn hình chơi
    public void OnClickHintButton()
    {
        if (isGameWon) return;

        Time.timeScale = 0f; // Tạm dừng thế giới vật lý của game

        // ĐÃ SỬA: Nếu là tài khoản Premium HOẶC đã xem quảng cáo mở khóa Hint ở màn này rồi thì mở thẳng luôn
        bool isPremium = PremiumManager.Instance != null && PremiumManager.Instance.IsPremium;
        if (!GameConfigService.Current.HintRequiresAd
            || HintService.CanOpenHintDirectly(isPremium, isHintUnlockedCurrentLevel))
        {
            OpenHintCanvas2(viaAd: false); // Xem luôn gợi ý, bỏ qua bước xem quảng cáo
        }
        else
        {
            if (hintCanvas1 != null) hintCanvas1.SetActive(true); // Chưa mở khóa: Hiện bảng hỏi xem Ads
            if (hintCanvas2 != null) hintCanvas2.SetActive(false);
        }
    }

    public void CloseHintCanvas1()
    {
        if (hintCanvas1 != null) hintCanvas1.SetActive(false);
        Time.timeScale = 1f;
    }

    // Người chơi bấm "CÓ" chấp nhận xem Quảng cáo trên Hint Canvas 1
    public void OnClickCo_Canvas1()
    {
        if (hintCanvas1 != null) hintCanvas1.SetActive(false);

        if (!GameConfigService.Current.HintRequiresAd)
        {
            OpenHintCanvas2(viaAd: false);
            return;
        }

        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowRewardedAdForHint(
                onGranted: () =>
                {
                    // ĐÃ SỬA: Đánh dấu đã mở khóa thành công cho màn này vĩnh viễn (cho đến khi thắng)
                    isHintUnlockedCurrentLevel = true;
                    OpenHintCanvas2(viaAd: true);
                },
                onFailed: () =>
                {
                    Time.timeScale = 1f;
                    Debug.Log("Lỗi kích hoạt quảng cáo!");
                }
            );
        }
        else
        {
            isHintUnlockedCurrentLevel = true;
            OpenHintCanvas2(viaAd: false);
        }
    }

    private void OpenHintCanvas2(bool viaAd)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (hintText2 != null)
        {
            hintText2.text = HintService.GetHint(levelDatabase, currentSceneName);
        }

        LevelSession session = LevelSession.Current;
        if (session != null)
        {
            session.RecordHint();
            GameAnalytics.Reporter.HintUsed(session.LevelId, viaAd);
        }

        if (hintCanvas1 != null) hintCanvas1.SetActive(false);
        if (hintCanvas2 != null) hintCanvas2.SetActive(true);
    }

    public void CloseHintCanvas2()
    {
        if (hintCanvas2 != null) hintCanvas2.SetActive(false);
        Time.timeScale = 1f;
    }

    // ĐÃ NÂNG CẤP: Nút "NHẢY" (Bỏ qua màn) nằm trên Hint Canvas 2 đòi hỏi xem Ads ảo
    public void OnClickNhay_Canvas2()
    {
        if (!GameConfigService.Current.SkipEnabled) return;
        if (hintCanvas2 != null) hintCanvas2.SetActive(false);

        // Gọi hệ thống quảng cáo đếm ngược trước khi cho phép người chơi ăn gian qua màn
        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowRewardedAdForHint(
                onGranted: () =>
                {
                    Time.timeScale = 1f; // Trả lại thời gian thực trước khi xử lý thắng cuộc
                    SkipLevel();         // Xem xong Ads ảo -> Thắng màn ngay lập tức
                },
                onFailed: () =>
                {
                    Time.timeScale = 1f;
                    Debug.Log("Không thể bỏ qua màn chơi do lỗi hệ thống quảng cáo!");
                }
            );
        }
        else
        {
            // Dự phòng an toàn nếu trong Scene thiếu Prefab AdManager
            Time.timeScale = 1f;
            SkipLevel();
        }
    }

    public void WinGame()
    {
        CompleteLevel(skipped: false);
    }

    // Bỏ qua màn bằng nút "NHẢY" → vẫn tính là qua màn nhưng chỉ được 1 sao
    private void SkipLevel()
    {
        LevelSession session = LevelSession.Current;
        if (session != null)
            GameAnalytics.Reporter.LevelSkip(session.LevelId, session.Elapsed(Time.realtimeSinceStartup));

        CompleteLevel(skipped: true);
    }

    private void CompleteLevel(bool skipped)
    {
        if (isGameWon) return;
        isGameWon = true;

        // === TÍNH SAO + LƯU TIẾN TRÌNH: Mở khóa level tiếp theo ===
        // Làm trước khi bật WinPanel để UI hiển thị sao đọc được LastStars mới
        int currentLevel = GetCurrentLevelNumber();
        LevelSession session = LevelSession.Current != null && LevelSession.Current.LevelId == currentLevel
            ? LevelSession.Current
            : null;
        int deaths = session?.Deaths ?? 0;
        int hintsUsed = session?.HintsUsed ?? 0;
        float duration = session?.Elapsed(Time.realtimeSinceStartup) ?? 0f;

        LastStars = StarCalculator.Calculate(completed: true, skipped, deaths, hintsUsed);
        ProgressService.CompleteLevel(currentLevel, nextLevelName, LastStars);
        GameAnalytics.Reporter.LevelComplete(currentLevel, duration, deaths, hintsUsed, LastStars);
        session?.Finish();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.winSound);

        if (AdManager.Instance != null) AdManager.Instance.OnLevelCompleted();

        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // Trích số level từ tên Scene hiện tại (VD: "Level 5" -> 5)
    private int GetCurrentLevelNumber()
    {
        return LevelNaming.TryParseLevelNumber(SceneManager.GetActiveScene().name, out int levelNumber)
            ? levelNumber
            : 1; // Mặc định nếu không parse được
    }

    // ==========================================
    // CÁC HÀM QUẢN LÝ KHÁC (GIỮ NGUYÊN)
    // ==========================================
    public void GoToNextLevel() { Time.timeScale = 1f; SceneManager.LoadScene(nextLevelName); }
    public void ReplayLevel() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void TogglePause() { if (!isGameWon) { if (pauseCanvas != null) pauseCanvas.SetActive(true); Time.timeScale = 0f; } }
    public void ResumeGame() { if (pauseCanvas != null) pauseCanvas.SetActive(false); Time.timeScale = 1f; }
    public void ShowShop() { if (shopPanel != null) shopPanel.SetActive(true); }
    public void CloseShop() { if (shopPanel != null) shopPanel.SetActive(false); }
    public void GoToLevelSelect() { Time.timeScale = 1f; SceneManager.LoadScene(levelSelectSceneName); }
}
