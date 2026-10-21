using System;

/// <summary>
/// Gộp tiến trình local và cloud khi hai bên khác nhau.
/// Luật: lấy giá trị "tốt hơn" cho người chơi → không bao giờ làm mất tiến trình.
/// | Trường           | Luật gộp                                              |
/// |------------------|-------------------------------------------------------|
/// | resetCount khác  | lấy nguyên bên có resetCount lớn hơn (reset mới nhất) |
/// | maxUnlockedLevel | max(local, remote)                                    |
/// | stars[level]     | max(local, remote) cho từng màn                       |
/// | savedLevel       | lấy theo bên có maxUnlockedLevel lớn hơn (hòa → local)|
/// | updatedAtUtc     | thời điểm muộn hơn (chuỗi ISO 8601 so sánh được)       |
/// </summary>
public static class ProgressMerger
{
    public static ProgressData Merge(ProgressData local, ProgressData remote)
    {
        if (local == null && remote == null) return new ProgressData();
        if (local == null) return remote.Clone();
        if (remote == null) return local.Clone();

        // Một bên đã "New Game" sau bên kia → tiến trình cũ không được gộp lại
        if (remote.resetCount != local.resetCount)
            return remote.resetCount > local.resetCount ? remote.Clone() : local.Clone();

        ProgressData result = local.Clone();
        result.version = Math.Max(local.version, remote.version);
        result.maxUnlockedLevel = Math.Max(local.maxUnlockedLevel, remote.maxUnlockedLevel);
        result.savedLevel = remote.maxUnlockedLevel > local.maxUnlockedLevel ? remote.savedLevel : local.savedLevel;

        if (remote.stars != null)
        {
            foreach (LevelStars entry in remote.stars)
                result.SetStarsIfHigher(entry.levelId, entry.stars);
        }

        result.updatedAtUtc = string.CompareOrdinal(remote.updatedAtUtc ?? "", local.updatedAtUtc ?? "") > 0
            ? remote.updatedAtUtc
            : local.updatedAtUtc;

        return result;
    }
}
