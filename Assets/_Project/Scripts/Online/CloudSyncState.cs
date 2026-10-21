using System;

/// <summary>Trạng thái thuần C#: gom thay đổi, chống gửi chồng và chờ thử lại khi lỗi.</summary>
public sealed class CloudSyncState
{
    public const double DebounceSeconds = 2;
    public bool NeedsSync { get; private set; }
    public bool IsInFlight { get; private set; }
    public long Revision { get; private set; }
    private double dueAt;
    private double retryAt;
    private double retryDelay = 5;

    public void MarkChanged(double now)
    {
        Revision++;
        NeedsSync = true;
        Postpone(now);
    }

    public void Postpone(double now) => dueAt = now + DebounceSeconds;

    public bool TryBegin(double now)
    {
        if (!NeedsSync || IsInFlight || now < Math.Max(dueAt, retryAt)) return false;
        IsInFlight = true;
        return true;
    }

    public void Succeed(long uploadedRevision)
    {
        IsInFlight = false;
        NeedsSync = uploadedRevision != Revision;
        retryAt = 0;
        retryDelay = 5;
    }

    public void Fail(double now)
    {
        IsInFlight = false;
        NeedsSync = true;
        retryAt = now + retryDelay;
        retryDelay = Math.Min(60, retryDelay * 2);
    }

    /// <summary>Có mạng trở lại: bỏ thời gian chờ lỗi nhưng vẫn giữ debounce.</summary>
    public void NetworkRestored() => retryAt = 0;
}
