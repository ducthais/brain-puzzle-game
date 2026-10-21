using NUnit.Framework;

/// <summary>
/// Kiểm thử luật gộp tiến trình local ↔ cloud: không bao giờ làm mất tiến trình.
/// </summary>
public class ProgressMergerTests
{
    private static ProgressData Make(int maxUnlocked, string savedLevel, string updatedAt, params (int level, int stars)[] stars)
    {
        ProgressData data = new ProgressData { maxUnlockedLevel = maxUnlocked, savedLevel = savedLevel, updatedAtUtc = updatedAt };
        foreach (var (level, value) in stars) data.SetStarsIfHigher(level, value);
        return data;
    }

    [Test]
    public void Merge_TakesHigherMaxUnlockedLevel()
    {
        ProgressData result = ProgressMerger.Merge(Make(5, "Level 5", ""), Make(9, "Level 9", ""));
        Assert.AreEqual(9, result.maxUnlockedLevel);
    }

    [Test]
    public void Merge_SavedLevelFollowsSideWithMoreProgress()
    {
        Assert.AreEqual("Level 9", ProgressMerger.Merge(Make(5, "Level 5", ""), Make(9, "Level 9", "")).savedLevel);
        Assert.AreEqual("Level 7", ProgressMerger.Merge(Make(7, "Level 7", ""), Make(3, "Level 3", "")).savedLevel);
    }

    [Test]
    public void Merge_EqualProgress_KeepsLocalSavedLevel()
    {
        Assert.AreEqual("Level 4 local", ProgressMerger.Merge(Make(4, "Level 4 local", ""), Make(4, "Level 4 cloud", "")).savedLevel);
    }

    [Test]
    public void Merge_TakesHigherStarsPerLevel()
    {
        ProgressData local = Make(5, "Level 5", "", (1, 3), (2, 1));
        ProgressData remote = Make(5, "Level 5", "", (1, 2), (2, 3), (3, 2));

        ProgressData result = ProgressMerger.Merge(local, remote);

        Assert.AreEqual(3, result.GetStars(1));
        Assert.AreEqual(3, result.GetStars(2));
        Assert.AreEqual(2, result.GetStars(3));
    }

    [Test]
    public void Merge_TakesLaterUpdatedAt()
    {
        ProgressData result = ProgressMerger.Merge(
            Make(1, "Level 1", "2026-11-01T10:00:00.0000000Z"),
            Make(1, "Level 1", "2026-11-02T08:00:00.0000000Z"));
        Assert.AreEqual("2026-11-02T08:00:00.0000000Z", result.updatedAtUtc);
    }

    [Test]
    public void Merge_NullSides_ReturnsCopyOfOther()
    {
        ProgressData local = Make(6, "Level 6", "", (2, 2));
        Assert.AreEqual(6, ProgressMerger.Merge(local, null).maxUnlockedLevel);
        Assert.AreEqual(6, ProgressMerger.Merge(null, local).maxUnlockedLevel);
        Assert.AreEqual(1, ProgressMerger.Merge(null, null).maxUnlockedLevel);
    }

    [Test]
    public void Merge_DoesNotModifyInputs()
    {
        ProgressData local = Make(2, "Level 2", "", (1, 1));
        ProgressData remote = Make(8, "Level 8", "", (1, 3));

        ProgressMerger.Merge(local, remote);

        Assert.AreEqual(2, local.maxUnlockedLevel);
        Assert.AreEqual(1, local.GetStars(1));
    }
}
