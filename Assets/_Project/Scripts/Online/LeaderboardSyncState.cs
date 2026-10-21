using System;

/// <summary>Chỉ gửi tổng sao tăng; tái dùng debounce/retry thuần C# của cloud.</summary>
public sealed class LeaderboardSyncState
{
    private readonly CloudSyncState sync = new CloudSyncState();
    private int total;
    private int sendingScore;
    private long sendingRevision;
    public int LastSubmitted { get; private set; }

    public LeaderboardSyncState(int lastSubmitted = 0) => LastSubmitted = Math.Max(0, lastSubmitted);

    public static int TotalStars(ProgressData data)
    {
        int result = 0;
        if (data?.stars == null) return result;
        foreach (LevelStars entry in data.stars)
            if (entry != null) result = checked(result + entry.stars);
        return result;
    }

    public void Observe(ProgressData data, double now)
    {
        int next = TotalStars(data);
        if (next == total) return;
        total = next;
        sync.MarkChanged(now);
    }

    public bool TryBegin(double now, out int score)
    {
        score = 0;
        if (total <= LastSubmitted || !sync.TryBegin(now)) return false;
        sendingScore = score = total;
        sendingRevision = sync.Revision;
        return true;
    }

    public void Succeed()
    {
        LastSubmitted = Math.Max(LastSubmitted, sendingScore);
        sync.Succeed(sendingRevision);
    }

    public void Fail(double now) => sync.Fail(now);
    public void NetworkRestored() => sync.NetworkRestored();
}
