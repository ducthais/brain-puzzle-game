using NUnit.Framework;

/// <summary>
/// Kiểm thử ProgressService với store trong bộ nhớ (không đụng PlayerPrefs thật).
/// </summary>
public class ProgressServiceTests
{
    private InMemoryProgressStore store;

    [SetUp]
    public void Setup()
    {
        store = new InMemoryProgressStore();
        ProgressService.UseStore(store);
    }

    [TearDown]
    public void Teardown()
    {
        ProgressService.UseStore(null); // Trả về LocalProgressStore mặc định
    }

    [Test]
    public void NewPlayer_OnlyLevel1Unlocked()
    {
        Assert.AreEqual(1, ProgressService.MaxUnlockedLevel);
        Assert.IsTrue(ProgressService.IsUnlocked(1));
        Assert.IsFalse(ProgressService.IsUnlocked(2));
        Assert.IsFalse(ProgressService.IsCompleted(1));
    }

    [Test]
    public void CompleteLevel_UnlocksNextLevel_AndSaves()
    {
        ProgressService.CompleteLevel(1, "Level 2", 3);

        Assert.AreEqual(2, ProgressService.MaxUnlockedLevel);
        Assert.IsTrue(ProgressService.IsCompleted(1));
        Assert.AreEqual("Level 2", ProgressService.SavedLevel);
        Assert.AreEqual(3, ProgressService.GetStars(1));
        Assert.AreEqual(1, store.SaveCount);
        Assert.AreEqual(2, store.Load().maxUnlockedLevel);
    }

    [Test]
    public void ReplayingOldLevel_DoesNotLowerMaxUnlocked()
    {
        ProgressService.UnlockUpTo(10);

        ProgressService.CompleteLevel(3, "Level 4", 1);

        Assert.AreEqual(10, ProgressService.MaxUnlockedLevel);
    }

    [Test]
    public void CompleteLevel_WithFewerStars_KeepsBestStars()
    {
        ProgressService.CompleteLevel(1, "Level 2", 3);
        ProgressService.CompleteLevel(1, "Level 2", 1);

        Assert.AreEqual(3, ProgressService.GetStars(1));
    }

    [Test]
    public void ResetProgress_BackToLevel1_ClearsStars()
    {
        ProgressService.CompleteLevel(1, "Level 2", 3);
        ProgressService.CompleteLevel(2, "Level 3", 2);

        ProgressService.ResetProgress();

        Assert.AreEqual(1, ProgressService.MaxUnlockedLevel);
        Assert.AreEqual(ProgressData.DefaultSavedLevel, ProgressService.SavedLevel);
        Assert.AreEqual(0, ProgressService.GetStars(1));
    }

    [Test]
    public void MergeFrom_AppliesCloudProgress()
    {
        ProgressService.CompleteLevel(1, "Level 2", 2);
        ProgressData cloud = new ProgressData { maxUnlockedLevel = 7, savedLevel = "Level 7" };
        cloud.SetStarsIfHigher(1, 3);

        ProgressService.MergeFrom(cloud);

        Assert.AreEqual(7, ProgressService.MaxUnlockedLevel);
        Assert.AreEqual(3, ProgressService.GetStars(1));
        Assert.AreEqual(7, store.Load().maxUnlockedLevel);
    }

    [Test]
    public void Changed_IsRaisedOnEveryCommit()
    {
        int raised = 0;
        void Handler(ProgressData _) => raised++;
        ProgressService.Changed += Handler;
        try
        {
            ProgressService.CompleteLevel(1, "Level 2", 3);
            ProgressService.ResetProgress();
        }
        finally
        {
            ProgressService.Changed -= Handler;
        }

        Assert.AreEqual(2, raised);
    }

    [Test]
    public void Snapshot_IsACopy()
    {
        ProgressData snapshot = ProgressService.Snapshot;
        snapshot.maxUnlockedLevel = 99;

        Assert.AreEqual(1, ProgressService.MaxUnlockedLevel);
    }

    [Test]
    public void UseStore_LoadsExistingDataFromNewStore()
    {
        ProgressService.UseStore(new InMemoryProgressStore(new ProgressData { maxUnlockedLevel = 15 }));

        Assert.AreEqual(15, ProgressService.MaxUnlockedLevel);
    }
}
