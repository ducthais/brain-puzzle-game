/// <summary>
/// Gửi sự kiện phân tích hành vi người chơi (xem Phụ lục A của kế hoạch).
/// </summary>
public interface IAnalyticsReporter
{
    void LevelStart(int levelId, int attempt);
    void LevelComplete(int levelId, float durationSec, int deaths, int hintsUsed, int stars);
    void PlayerDeath(int levelId, float timeInLevelSec);
    void HintUsed(int levelId, bool viaAd);
    void LevelSkip(int levelId, float timeInLevelSec);
}

/// <summary>
/// Bản mặc định: không gửi gì (khi chưa tích hợp Unity Analytics hoặc khi chạy test).
/// </summary>
public class NullAnalyticsReporter : IAnalyticsReporter
{
    public void LevelStart(int levelId, int attempt) { }
    public void LevelComplete(int levelId, float durationSec, int deaths, int hintsUsed, int stars) { }
    public void PlayerDeath(int levelId, float timeInLevelSec) { }
    public void HintUsed(int levelId, bool viaAd) { }
    public void LevelSkip(int levelId, float timeInLevelSec) { }
}
