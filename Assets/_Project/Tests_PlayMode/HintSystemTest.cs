using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Kiểm tra hệ thống gợi ý hiển thị đúng nội dung lấy từ LevelDatabase theo tên scene đang chơi.
/// </summary>
public class HintSystemTests
{
    private GameObject root;
    private LevelManager levelManager;
    private LevelDatabase database;
    private TextMeshProUGUI hintText;

    [SetUp]
    public void Setup()
    {
        ProgressService.UseStore(new InMemoryProgressStore());
        LevelSession.Clear();

        database = ScriptableObject.CreateInstance<LevelDatabase>();

        root = new GameObject("Test_HintRoot");
        levelManager = root.AddComponent<LevelManager>();
        levelManager.levelDatabase = database;
        levelManager.hintCanvas1 = new GameObject("HintCanvas1");
        levelManager.hintCanvas2 = new GameObject("HintCanvas2");
        levelManager.hintCanvas1.transform.SetParent(root.transform);
        levelManager.hintCanvas2.transform.SetParent(root.transform);
        levelManager.hintCanvas1.SetActive(false);
        levelManager.hintCanvas2.SetActive(false);

        GameObject textObj = new GameObject("HintText2", typeof(RectTransform));
        textObj.transform.SetParent(root.transform);
        hintText = textObj.AddComponent<TextMeshProUGUI>();
        levelManager.hintText2 = hintText;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(root);
        Object.Destroy(database);
        Time.timeScale = 1f;
        ProgressService.UseStore(null);
        LevelSession.Clear();
    }

    [UnityTest]
    public IEnumerator HintSystem_ShowHint_DisplaysCorrectText()
    {
        // Arrange: gợi ý khai báo cho đúng scene đang chạy test
        string expectedHint = "Sức người có hạn, trọng lực thì miễn phí!";
        database.allLevels = new[]
        {
            new LevelDatabase.LevelData { levelName = "Level khác", hintMessage = "Gợi ý của màn khác" },
            new LevelDatabase.LevelData { levelName = SceneManager.GetActiveScene().name, hintMessage = expectedHint }
        };
        yield return null;

        // Act: người chơi bấm Hint → "Có" (không có AdManager nên mở khóa ngay)
        levelManager.OnClickHintButton();
        levelManager.OnClickCo_Canvas1();

        // Assert
        Assert.IsTrue(levelManager.hintCanvas2.activeSelf, "Bảng gợi ý chưa được mở!");
        Assert.AreEqual(expectedHint, hintText.text, "Hệ thống gợi ý đang hiển thị sai nội dung!");
    }

    [UnityTest]
    public IEnumerator HintSystem_LevelWithoutHint_ShowsDefaultHint()
    {
        database.allLevels = new[]
        {
            new LevelDatabase.LevelData { levelName = "Level khác", hintMessage = "Gợi ý của màn khác" }
        };
        yield return null;

        levelManager.OnClickHintButton();
        levelManager.OnClickCo_Canvas1();

        Assert.AreEqual(HintService.DefaultHint, hintText.text, "Màn chưa khai báo gợi ý phải hiện gợi ý mặc định!");
    }

    [UnityTest]
    public IEnumerator HintSystem_CloseHint_ResumesGame()
    {
        yield return null;

        levelManager.OnClickHintButton();
        Assert.AreEqual(0f, Time.timeScale, "Mở gợi ý phải tạm dừng game!");

        levelManager.OnClickCo_Canvas1();
        levelManager.CloseHintCanvas2();

        Assert.IsFalse(levelManager.hintCanvas2.activeSelf);
        Assert.AreEqual(1f, Time.timeScale, "Đóng gợi ý phải cho game chạy lại!");
    }
}
