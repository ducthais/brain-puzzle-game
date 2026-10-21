/// <summary>
/// Nơi lưu tiến trình. Bản Local dùng PlayerPrefs; bản Cloud (Unity Cloud Save) cài đặt cùng interface này.
/// </summary>
public interface IProgressStore
{
    ProgressData Load();
    void Save(ProgressData data);
}

/// <summary>
/// Lưu tiến trình trong bộ nhớ — dùng cho kiểm thử.
/// </summary>
public class InMemoryProgressStore : IProgressStore
{
    private ProgressData data;

    public InMemoryProgressStore(ProgressData initial = null)
    {
        data = initial?.Clone();
    }

    public int SaveCount { get; private set; }

    public ProgressData Load() => data?.Clone() ?? new ProgressData();

    public void Save(ProgressData newData)
    {
        data = newData?.Clone();
        SaveCount++;
    }
}
