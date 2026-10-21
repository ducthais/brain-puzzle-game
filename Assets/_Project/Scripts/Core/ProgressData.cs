using System;
using System.Collections.Generic;

/// <summary>
/// Dữ liệu tiến trình của người chơi (lưu local và đồng bộ cloud).
/// Dùng List thay cho Dictionary để JsonUtility serialize được.
/// Trạng thái Premium KHÔNG nằm ở đây: do PremiumManager quản lý và IAP tự khôi phục.
/// </summary>
[Serializable]
public class ProgressData
{
    public const int CurrentVersion = 1;
    public const string DefaultSavedLevel = "Level 1";

    public int version = CurrentVersion;
    public int maxUnlockedLevel = 1;
    public string savedLevel = DefaultSavedLevel;
    public List<LevelStars> stars = new List<LevelStars>();
    public string updatedAtUtc = "";

    public int GetStars(int levelId)
    {
        if (stars == null) return 0;
        foreach (LevelStars entry in stars)
        {
            if (entry.levelId == levelId) return entry.stars;
        }
        return 0;
    }

    /// <summary>
    /// Chỉ ghi khi số sao mới cao hơn → không bao giờ làm giảm thành tích đã đạt.
    /// </summary>
    public void SetStarsIfHigher(int levelId, int value)
    {
        if (stars == null) stars = new List<LevelStars>();
        foreach (LevelStars entry in stars)
        {
            if (entry.levelId != levelId) continue;
            if (value > entry.stars) entry.stars = value;
            return;
        }
        if (value > 0) stars.Add(new LevelStars { levelId = levelId, stars = value });
    }

    public ProgressData Clone()
    {
        ProgressData copy = (ProgressData)MemberwiseClone();
        copy.stars = new List<LevelStars>();
        if (stars != null)
        {
            foreach (LevelStars entry in stars)
                copy.stars.Add(new LevelStars { levelId = entry.levelId, stars = entry.stars });
        }
        return copy;
    }
}

[Serializable]
public class LevelStars
{
    public int levelId;
    public int stars;
}
