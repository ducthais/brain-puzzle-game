using System;

/// <summary>
/// Theo dõi một lượt chơi của một màn: số lần thử, số lần chết, số lần xem gợi ý, thời gian.
/// Lưu ở biến static vì khi nhân vật chết game nạp lại scene → MonoBehaviour bị hủy.
/// </summary>
public class LevelSession
{
    public static LevelSession Current { get; private set; }

    public int LevelId { get; }
    public float StartTime { get; }
    public int Attempt { get; private set; }
    public int Deaths { get; private set; }
    public int HintsUsed { get; private set; }
    public bool IsFinished { get; private set; }

    private LevelSession(int levelId, float startTime)
    {
        LevelId = levelId;
        StartTime = startTime;
        Attempt = 1;
    }

    /// <summary>
    /// Gọi khi scene màn chơi được nạp. Nếu đang chơi dở cùng màn (chơi lại sau khi chết)
    /// thì giữ nguyên bộ đếm và tăng số lần thử; ngược lại bắt đầu lượt mới.
    /// </summary>
    public static LevelSession Begin(int levelId, float now)
    {
        if (Current != null && Current.LevelId == levelId && !Current.IsFinished)
        {
            Current.Attempt++;
            return Current;
        }

        Current = new LevelSession(levelId, now);
        return Current;
    }

    public static void Clear() => Current = null;

    public void RecordDeath() => Deaths++;

    public void RecordHint() => HintsUsed++;

    public float Elapsed(float now) => Math.Max(0f, now - StartTime);

    public void Finish() => IsFinished = true;
}
