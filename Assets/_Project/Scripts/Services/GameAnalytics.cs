using UnityEngine;

/// <summary>
/// Điểm truy cập cho việc gửi sự kiện phân tích.
/// Mặc định không gửi gì (NullAnalyticsReporter); bản Unity Analytics đăng ký qua UseReporter().
/// </summary>
public static class GameAnalytics
{
    private static IAnalyticsReporter reporter;

    public static IAnalyticsReporter Reporter => reporter ??= new NullAnalyticsReporter();

    public static void UseReporter(IAnalyticsReporter newReporter) => reporter = newReporter;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => reporter = null;
}
