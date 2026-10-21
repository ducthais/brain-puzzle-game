using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Kiểm thử lưu/đọc tiến trình bằng PlayerPrefs. Sao lưu và khôi phục key thật để không làm mất dữ liệu Editor.
/// </summary>
public class LocalProgressStoreTests
{
    private static readonly string[] IntKeys = { LocalProgressStore.MaxUnlockedLevelKey };
    private static readonly string[] StringKeys =
    {
        LocalProgressStore.SavedLevelKey, LocalProgressStore.LevelStarsKey, LocalProgressStore.UpdatedAtKey
    };

    private int? backupMaxUnlocked;
    private readonly string[] backupStrings = new string[StringKeys.Length];
    private readonly bool[] hadString = new bool[StringKeys.Length];

    [SetUp]
    public void Setup()
    {
        backupMaxUnlocked = PlayerPrefs.HasKey(IntKeys[0]) ? PlayerPrefs.GetInt(IntKeys[0]) : (int?)null;
        for (int i = 0; i < StringKeys.Length; i++)
        {
            hadString[i] = PlayerPrefs.HasKey(StringKeys[i]);
            backupStrings[i] = PlayerPrefs.GetString(StringKeys[i], "");
        }
        LocalProgressStore.DeleteAll();
    }

    [TearDown]
    public void Teardown()
    {
        LocalProgressStore.DeleteAll();
        if (backupMaxUnlocked.HasValue) PlayerPrefs.SetInt(IntKeys[0], backupMaxUnlocked.Value);
        for (int i = 0; i < StringKeys.Length; i++)
        {
            if (hadString[i]) PlayerPrefs.SetString(StringKeys[i], backupStrings[i]);
        }
        PlayerPrefs.Save();
    }

    [Test]
    public void Load_WhenEmpty_ReturnsDefaults()
    {
        ProgressData data = new LocalProgressStore().Load();

        Assert.AreEqual(1, data.maxUnlockedLevel);
        Assert.AreEqual(ProgressData.DefaultSavedLevel, data.savedLevel);
        Assert.AreEqual(0, data.stars.Count);
    }

    [Test]
    public void SaveThenLoad_RoundTripsAllFields()
    {
        ProgressData saved = new ProgressData { maxUnlockedLevel = 12, savedLevel = "Level 12", updatedAtUtc = "2026-11-01T00:00:00Z" };
        saved.SetStarsIfHigher(1, 3);
        saved.SetStarsIfHigher(5, 2);

        new LocalProgressStore().Save(saved);
        ProgressData loaded = new LocalProgressStore().Load();

        Assert.AreEqual(12, loaded.maxUnlockedLevel);
        Assert.AreEqual("Level 12", loaded.savedLevel);
        Assert.AreEqual("2026-11-01T00:00:00Z", loaded.updatedAtUtc);
        Assert.AreEqual(3, loaded.GetStars(1));
        Assert.AreEqual(2, loaded.GetStars(5));
    }

    // Tương thích dữ liệu cũ: người chơi đã có key "MaxUnlockedLevel" từ phiên bản trước
    [Test]
    public void Load_ReadsLegacyKeys()
    {
        PlayerPrefs.SetInt("MaxUnlockedLevel", 8);
        PlayerPrefs.SetString("SavedLevel", "Level 8");

        ProgressData data = new LocalProgressStore().Load();

        Assert.AreEqual(8, data.maxUnlockedLevel);
        Assert.AreEqual("Level 8", data.savedLevel);
    }

    [Test]
    public void Load_CorruptedStars_FallsBackToEmpty()
    {
        PlayerPrefs.SetString(LocalProgressStore.LevelStarsKey, "{not json");

        ProgressData data = null;
        Assert.DoesNotThrow(() => data = new LocalProgressStore().Load());
        Assert.AreEqual(0, data.GetStars(1));
    }

    [Test]
    public void Load_InvalidMaxUnlocked_ClampedTo1()
    {
        PlayerPrefs.SetInt("MaxUnlockedLevel", 0);

        Assert.AreEqual(1, new LocalProgressStore().Load().maxUnlockedLevel);
    }

    [Test]
    public void Save_DoesNotTouchPremiumKey()
    {
        bool hadPremium = PlayerPrefs.HasKey("isPremium");
        int premium = PlayerPrefs.GetInt("isPremium", 0);

        new LocalProgressStore().Save(new ProgressData { maxUnlockedLevel = 3 });

        Assert.AreEqual(hadPremium, PlayerPrefs.HasKey("isPremium"));
        Assert.AreEqual(premium, PlayerPrefs.GetInt("isPremium", 0));
    }
}
