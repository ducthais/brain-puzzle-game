using System;
using UnityEngine;

/// <summary>
/// Điểm truy cập duy nhất cho tiến trình người chơi (màn đã mở khóa, số sao, màn đã lưu).
/// Mặc định lưu bằng LocalProgressStore; có thể thay bằng store khác (Cloud) qua UseStore().
/// </summary>
public static class ProgressService
{
    private static IProgressStore store;
    private static ProgressData data;

    /// <summary>Phát ra mỗi khi tiến trình thay đổi (thắng màn, reset, gộp dữ liệu cloud).</summary>
    public static event Action<ProgressData> Changed;

    public static IProgressStore Store => store ??= new LocalProgressStore();

    private static ProgressData Data => data ??= Store.Load() ?? new ProgressData();

    public static int MaxUnlockedLevel => Data.maxUnlockedLevel;
    public static string SavedLevel => Data.savedLevel;

    /// <summary>Bản sao dữ liệu hiện tại (sửa bản sao không ảnh hưởng tiến trình).</summary>
    public static ProgressData Snapshot => Data.Clone();

    public static bool IsUnlocked(int levelId) => levelId <= Data.maxUnlockedLevel;
    public static bool IsCompleted(int levelId) => levelId < Data.maxUnlockedLevel;
    public static int GetStars(int levelId) => Data.GetStars(levelId);

    /// <summary>Đổi nơi lưu trữ (VD: CloudProgressStore). Dữ liệu được nạp lại từ store mới.</summary>
    public static void UseStore(IProgressStore newStore)
    {
        store = newStore;
        data = null;
    }

    /// <summary>Bỏ bộ nhớ đệm, lần truy cập sau sẽ đọc lại từ store.</summary>
    public static void Reload() => data = null;

    /// <summary>
    /// Ghi nhận thắng màn: mở khóa màn kế tiếp, lưu số sao (chỉ tăng), lưu màn để chơi tiếp.
    /// </summary>
    public static void CompleteLevel(int levelId, string nextLevelName, int stars)
    {
        ProgressData current = Data;
        if (levelId >= current.maxUnlockedLevel) current.maxUnlockedLevel = levelId + 1;
        current.SetStarsIfHigher(levelId, stars);
        if (!string.IsNullOrEmpty(nextLevelName)) current.savedLevel = nextLevelName;
        Commit();
    }

    /// <summary>Mở khóa tới màn chỉ định (dùng cho tool test).</summary>
    public static void UnlockUpTo(int levelId)
    {
        Data.maxUnlockedLevel = Mathf.Max(1, levelId);
        Commit();
    }

    /// <summary>Chơi lại từ đầu: chỉ còn Level 1, xóa sao. Không ảnh hưởng Premium.</summary>
    public static void ResetProgress()
    {
        data = new ProgressData();
        Commit();
    }

    /// <summary>Gộp dữ liệu từ nơi khác (VD: cloud) theo luật của ProgressMerger rồi lưu lại.</summary>
    public static void MergeFrom(ProgressData remote)
    {
        data = ProgressMerger.Merge(Data, remote);
        Commit();
    }

    private static void Commit()
    {
        data.updatedAtUtc = DateTime.UtcNow.ToString("o");
        Store.Save(data);
        Changed?.Invoke(data.Clone());
    }

    // Reset trạng thái static khi vào Play mode (kể cả khi tắt Domain Reload)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        store = null;
        data = null;
        Changed = null;
    }
}
