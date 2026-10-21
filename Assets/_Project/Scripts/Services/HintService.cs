using System.Linq;

/// <summary>
/// Logic của hệ thống gợi ý, tách khỏi LevelManager để kiểm thử độc lập.
/// </summary>
public static class HintService
{
    public const string DefaultHint = "Hãy thử tương tác với giao diện xem sao!";

    /// <summary>Lấy nội dung gợi ý theo tên scene; không có thì trả về gợi ý mặc định.</summary>
    public static string GetHint(LevelDatabase database, string levelName)
    {
        if (database == null || database.allLevels == null) return DefaultHint;

        LevelDatabase.LevelData levelData = database.allLevels.FirstOrDefault(l => l != null && l.levelName == levelName);
        return levelData != null && !string.IsNullOrEmpty(levelData.hintMessage) ? levelData.hintMessage : DefaultHint;
    }

    /// <summary>Premium hoặc đã mở khóa gợi ý ở màn này (xem quảng cáo) → mở thẳng bảng gợi ý.</summary>
    public static bool CanOpenHintDirectly(bool isPremium, bool unlockedThisLevel) => isPremium || unlockedThisLevel;
}
