using NUnit.Framework;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Kiểm thử luồng Gợi ý / Nhảy màn của LevelManager (bảng quyết định mục 4.5).
/// EditMode không chạy Awake() → gán singleton PremiumManager/AdManager bằng reflection
/// để code thật nhận diện đúng tài khoản Premium/Free.
/// </summary>
public class LevelManagerTests
{
    private GameObject gameObj;
    private LevelManager levelManager;
    private PremiumManager premiumManager;
    private bool hadPremiumKey;
    private int premiumBackup;

    private static void SetInstance<T>(T value) where T : MonoBehaviour
    {
        typeof(T).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
            .GetSetMethod(true)
            .Invoke(null, new object[] { value });
    }

    private static void SetPremium(bool isPremium)
    {
        PlayerPrefs.SetInt("isPremium", isPremium ? 1 : 0);
    }

    [SetUp]
    public void Setup()
    {
        // Sao lưu Premium thật của Editor để trả lại sau test
        hadPremiumKey = PlayerPrefs.HasKey("isPremium");
        premiumBackup = PlayerPrefs.GetInt("isPremium", 0);

        // Tiến trình lưu trong bộ nhớ → WinGame() không ghi đè tiến trình thật trong PlayerPrefs
        ProgressService.UseStore(new InMemoryProgressStore());
        LevelSession.Clear();

        gameObj = new GameObject("TestRoot");
        levelManager = gameObj.AddComponent<LevelManager>();
        levelManager.nextLevelName = "Level 2";

        levelManager.hintCanvas1 = new GameObject("HintCanvas1");
        levelManager.hintCanvas2 = new GameObject("HintCanvas2");
        levelManager.winPanel = new GameObject("WinPanel");
        levelManager.hintCanvas1.SetActive(false);
        levelManager.hintCanvas2.SetActive(false);
        levelManager.winPanel.SetActive(false);

        premiumManager = new GameObject("PremiumManager").AddComponent<PremiumManager>();
        SetInstance(premiumManager);
        SetInstance<AdManager>(null);
    }

    [TearDown]
    public void Teardown()
    {
        SetInstance<PremiumManager>(null);
        AdManager ad = AdManager.Instance;
        SetInstance<AdManager>(null);
        if (ad != null) Object.DestroyImmediate(ad.gameObject);
        if (premiumManager != null) Object.DestroyImmediate(premiumManager.gameObject);

        if (levelManager != null)
        {
            Object.DestroyImmediate(levelManager.hintCanvas1);
            Object.DestroyImmediate(levelManager.hintCanvas2);
            Object.DestroyImmediate(levelManager.winPanel);
        }
        if (gameObj != null) Object.DestroyImmediate(gameObj);

        Time.timeScale = 1f;
        ProgressService.UseStore(null);
        LevelSession.Clear();

        if (hadPremiumKey) PlayerPrefs.SetInt("isPremium", premiumBackup);
        else PlayerPrefs.DeleteKey("isPremium");
        PlayerPrefs.Save();
    }

    // ID màn không tồn tại: không mở khóa, không có sao, tên scene sai không parse được, gợi ý dùng mặc định
    [Test]
    public void InvalidLevelId_IsHandledSafely()
    {
        LevelDatabase emptyDb = ScriptableObject.CreateInstance<LevelDatabase>();
        try
        {
            Assert.IsFalse(ProgressService.IsUnlocked(99));
            Assert.AreEqual(0, ProgressService.GetStars(99));
            Assert.IsFalse(LevelNaming.TryParseLevelNumber("Level 0", out _));
            Assert.IsFalse(LevelNaming.TryParseLevelNumber("LevelSelect", out _));
            Assert.AreEqual(HintService.DefaultHint, HintService.GetHint(emptyDb, "Level 99"));
        }
        finally
        {
            Object.DestroyImmediate(emptyDb);
        }
    }

    // TC_BQ_4.5_01: Premium bấm Hint mở thẳng Canvas 2, bỏ qua Ads
    [Test]
    public void Test_ClickHint_PremiumUser_OpensCanvas2Immediately()
    {
        SetPremium(true);

        levelManager.OnClickHintButton();

        Assert.IsTrue(levelManager.hintCanvas2.activeSelf, "Lỗi: Tài khoản Premium không mở được thẳng bảng gợi ý!");
        Assert.IsFalse(levelManager.hintCanvas1.activeSelf, "Lỗi: Premium lại hiển thị bảng hỏi Ads!");
    }

    // TC_BQ_4.5_02 & 03: Free bấm Hint phải hiện Canvas 1 hỏi xem Ads và dừng game
    [Test]
    public void Test_ClickHint_FreeUser_OpensCanvas1First()
    {
        SetPremium(false);

        levelManager.OnClickHintButton();

        Assert.IsTrue(levelManager.hintCanvas1.activeSelf, "Lỗi: Người chơi Free không hiển thị bảng hỏi xem Ads!");
        Assert.IsFalse(levelManager.hintCanvas2.activeSelf, "Lỗi: Người chơi Free lại xem được trực tiếp gợi ý!");
        Assert.AreEqual(0f, Time.timeScale, "Lỗi: Mở bảng gợi ý mà game không tạm dừng!");
    }

    // Free đã mở khóa gợi ý ở màn này (bấm "Có") → lần bấm Hint sau mở thẳng Canvas 2
    [Test]
    public void Test_FreeUser_AfterUnlockingHint_OpensCanvas2Directly()
    {
        SetPremium(false);

        levelManager.OnClickHintButton();
        levelManager.OnClickCo_Canvas1(); // Không có AdManager → mở khóa ngay
        Assert.IsTrue(levelManager.hintCanvas2.activeSelf, "Lỗi: Bấm Có mà không mở bảng gợi ý!");

        levelManager.CloseHintCanvas2();
        levelManager.OnClickHintButton();

        Assert.IsTrue(levelManager.hintCanvas2.activeSelf, "Lỗi: Đã mở khóa gợi ý nhưng vẫn phải xem lại quảng cáo!");
        Assert.IsFalse(levelManager.hintCanvas1.activeSelf);
    }

    // TC_BQ_4.5_04: Premium bấm "Nhảy" qua màn ngay, chỉ được 1 sao, mở khóa màn sau
    [Test]
    public void Test_ClickNhay_PremiumUser_WinsGameImmediately()
    {
        SetPremium(true);
        SetInstance(new GameObject("AdManager").AddComponent<AdManager>());
        levelManager.hintCanvas2.SetActive(true);

        levelManager.OnClickNhay_Canvas2();

        Assert.IsFalse(levelManager.hintCanvas2.activeSelf, "Lỗi: Bảng Canvas 2 chưa đóng sau khi bấm Nhảy!");
        Assert.IsTrue(levelManager.winPanel.activeSelf, "Lỗi: Hệ thống chưa kích hoạt WinPanel hiển thị cho Premium!");
        Assert.AreEqual(0f, Time.timeScale, "Lỗi: Khi hiện bảng Win thì hệ thống phải đóng băng game (timeScale = 0)!");
        Assert.AreEqual(1, levelManager.LastStars, "Lỗi: Nhảy màn phải chỉ được 1 sao!");
        Assert.IsTrue(ProgressService.IsUnlocked(2), "Lỗi: Nhảy màn không mở khóa màn kế tiếp!");
    }

    // TC_BQ_4.5_05: Free bấm "Nhảy" khi không có AdManager → vẫn qua màn (dự phòng), không kẹt game
    [Test]
    public void Test_ClickNhay_FreeUser_WithoutAdManager_StillSkipsLevel()
    {
        SetPremium(false);
        levelManager.hintCanvas2.SetActive(true);

        levelManager.OnClickNhay_Canvas2();

        Assert.IsTrue(levelManager.winPanel.activeSelf, "Lỗi: Thiếu AdManager làm người chơi kẹt, không qua được màn!");
        Assert.AreEqual(1, levelManager.LastStars);
    }
}
