using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Kiểm thử theo dõi lượt chơi (LevelSession), quy ước tên màn (LevelNaming) và HintService.
/// </summary>
public class LevelSessionAndNamingTests
{
    [TearDown]
    public void Teardown() => LevelSession.Clear();

    // ---------- LevelSession ----------

    [Test]
    public void Begin_NewLevel_StartsFreshSession()
    {
        LevelSession session = LevelSession.Begin(3, 10f);

        Assert.AreEqual(3, session.LevelId);
        Assert.AreEqual(1, session.Attempt);
        Assert.AreEqual(0, session.Deaths);
        Assert.AreSame(session, LevelSession.Current);
    }

    // Chết → scene nạp lại → Begin cùng màn: giữ bộ đếm, tăng số lần thử
    [Test]
    public void Begin_SameLevelAfterDeath_KeepsCounters()
    {
        LevelSession first = LevelSession.Begin(3, 10f);
        first.RecordDeath();
        first.RecordHint();

        LevelSession second = LevelSession.Begin(3, 25f);

        Assert.AreSame(first, second);
        Assert.AreEqual(2, second.Attempt);
        Assert.AreEqual(1, second.Deaths);
        Assert.AreEqual(1, second.HintsUsed);
        Assert.AreEqual(15f, second.Elapsed(25f), 0.001f);
    }

    [Test]
    public void Begin_AfterFinish_StartsNewSession()
    {
        LevelSession first = LevelSession.Begin(3, 0f);
        first.RecordDeath();
        first.Finish();

        LevelSession second = LevelSession.Begin(3, 50f);

        Assert.AreNotSame(first, second);
        Assert.AreEqual(0, second.Deaths);
    }

    [Test]
    public void Begin_DifferentLevel_StartsNewSession()
    {
        LevelSession.Begin(3, 0f).RecordDeath();

        LevelSession other = LevelSession.Begin(4, 5f);

        Assert.AreEqual(4, other.LevelId);
        Assert.AreEqual(0, other.Deaths);
    }

    [Test]
    public void Elapsed_NeverNegative()
    {
        Assert.AreEqual(0f, LevelSession.Begin(1, 100f).Elapsed(50f));
    }

    // ---------- LevelNaming ----------

    [TestCase("Level 1", true, 1)]
    [TestCase("Level 21", true, 21)]
    [TestCase("LevelSelect", false, 0)]
    [TestCase("StartScreen", false, 0)]
    [TestCase("Level 0", false, 0)]
    [TestCase("Level -2", false, 0)]
    [TestCase("", false, 0)]
    [TestCase(null, false, 0)]
    public void TryParseLevelNumber_Cases(string sceneName, bool expectedOk, int expectedId)
    {
        bool ok = LevelNaming.TryParseLevelNumber(sceneName, out int id);

        Assert.AreEqual(expectedOk, ok);
        if (expectedOk) Assert.AreEqual(expectedId, id);
    }

    [Test]
    public void SceneNameFor_RoundTrips()
    {
        Assert.IsTrue(LevelNaming.TryParseLevelNumber(LevelNaming.SceneNameFor(42), out int id));
        Assert.AreEqual(42, id);
    }

    // ---------- HintService ----------

    [Test]
    public void GetHint_ReturnsMessageOfMatchingLevel()
    {
        LevelDatabase db = ScriptableObject.CreateInstance<LevelDatabase>();
        db.allLevels = new[]
        {
            new LevelDatabase.LevelData { levelName = "Level 1", hintMessage = "Gợi ý 1" },
            new LevelDatabase.LevelData { levelName = "Level 2", hintMessage = "Gợi ý 2" }
        };

        Assert.AreEqual("Gợi ý 2", HintService.GetHint(db, "Level 2"));
        Object.DestroyImmediate(db);
    }

    [Test]
    public void GetHint_MissingLevelOrDatabase_ReturnsDefault()
    {
        LevelDatabase db = ScriptableObject.CreateInstance<LevelDatabase>();
        db.allLevels = new[] { new LevelDatabase.LevelData { levelName = "Level 1", hintMessage = "" } };

        Assert.AreEqual(HintService.DefaultHint, HintService.GetHint(db, "Level 99"));
        Assert.AreEqual(HintService.DefaultHint, HintService.GetHint(db, "Level 1"));
        Assert.AreEqual(HintService.DefaultHint, HintService.GetHint(null, "Level 1"));
        Object.DestroyImmediate(db);
    }

    // Bảng quyết định mở gợi ý: Premium hoặc đã mở khóa ở màn này → mở thẳng
    [TestCase(false, false, false)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(true, true, true)]
    public void CanOpenHintDirectly_DecisionTable(bool isPremium, bool unlocked, bool expected)
    {
        Assert.AreEqual(expected, HintService.CanOpenHintDirectly(isPremium, unlocked));
    }
}
