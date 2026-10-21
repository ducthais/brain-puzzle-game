using UnityEngine;

/// <summary>
/// Lưu tiến trình bằng PlayerPrefs trên thiết bị.
/// Giữ nguyên các key cũ ("MaxUnlockedLevel", "SavedLevel") để tương thích dữ liệu đã có của người chơi.
/// </summary>
public class LocalProgressStore : IProgressStore
{
    public const string MaxUnlockedLevelKey = "MaxUnlockedLevel";
    public const string SavedLevelKey = "SavedLevel";
    public const string LevelStarsKey = "LevelStars";
    public const string UpdatedAtKey = "ProgressUpdatedAt";
    public const string ResetCountKey = "ProgressResetCount";

    [System.Serializable]
    private class StarsWrapper
    {
        public System.Collections.Generic.List<LevelStars> items = new System.Collections.Generic.List<LevelStars>();
    }

    public ProgressData Load()
    {
        ProgressData data = new ProgressData
        {
            maxUnlockedLevel = Mathf.Max(1, PlayerPrefs.GetInt(MaxUnlockedLevelKey, 1)),
            savedLevel = PlayerPrefs.GetString(SavedLevelKey, ProgressData.DefaultSavedLevel),
            updatedAtUtc = PlayerPrefs.GetString(UpdatedAtKey, ""),
            resetCount = PlayerPrefs.GetInt(ResetCountKey, 0)
        };

        string starsJson = PlayerPrefs.GetString(LevelStarsKey, "");
        if (!string.IsNullOrEmpty(starsJson))
        {
            try
            {
                StarsWrapper wrapper = JsonUtility.FromJson<StarsWrapper>(starsJson);
                if (wrapper?.items != null) data.stars = wrapper.items;
            }
            catch (System.ArgumentException)
            {
                Debug.LogWarning("⚠ Dữ liệu sao trong PlayerPrefs bị hỏng, bỏ qua.");
            }
        }

        return data;
    }

    public void Save(ProgressData data)
    {
        if (data == null) return;

        PlayerPrefs.SetInt(MaxUnlockedLevelKey, data.maxUnlockedLevel);
        PlayerPrefs.SetString(SavedLevelKey, data.savedLevel ?? ProgressData.DefaultSavedLevel);
        PlayerPrefs.SetString(LevelStarsKey, JsonUtility.ToJson(new StarsWrapper { items = data.stars }));
        PlayerPrefs.SetString(UpdatedAtKey, data.updatedAtUtc ?? "");
        PlayerPrefs.SetInt(ResetCountKey, data.resetCount);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Xóa toàn bộ key tiến trình (không đụng tới Premium, âm thanh).
    /// </summary>
    public static void DeleteAll()
    {
        PlayerPrefs.DeleteKey(MaxUnlockedLevelKey);
        PlayerPrefs.DeleteKey(SavedLevelKey);
        PlayerPrefs.DeleteKey(LevelStarsKey);
        PlayerPrefs.DeleteKey(UpdatedAtKey);
        PlayerPrefs.DeleteKey(ResetCountKey);
        PlayerPrefs.Save();
    }
}
