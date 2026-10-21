using System;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

/// <summary>Năm sự kiện theo Phụ lục A; không giữ sự kiện phát sinh trước khi đồng ý.</summary>
public sealed class UnityAnalyticsReporter : IAnalyticsReporter
{
    public void LevelStart(int levelId, int attempt) => Record(new CustomEvent("level_start")
    {
        { "level_id", levelId }, { "attempt", attempt }
    });

    public void LevelComplete(int levelId, float durationSec, int deaths, int hintsUsed, int stars)
        => Record(new CustomEvent("level_complete")
    {
        { "level_id", levelId }, { "duration_sec", durationSec }, { "deaths", deaths },
        { "hints_used", hintsUsed }, { "stars", stars }
    });

    public void PlayerDeath(int levelId, float timeInLevelSec) => Record(new CustomEvent("player_death")
    {
        { "level_id", levelId }, { "time_in_level_sec", timeInLevelSec }
    });

    public void HintUsed(int levelId, bool viaAd) => Record(new CustomEvent("hint_used")
    {
        { "level_id", levelId }, { "via_ad", viaAd }
    });

    public void LevelSkip(int levelId, float timeInLevelSec) => Record(new CustomEvent("level_skip")
    {
        { "level_id", levelId }, { "time_in_level_sec", timeInLevelSec }
    });

    private static void Record(CustomEvent analyticsEvent)
    {
        // Kiểm tra lại ở điểm gửi để tham chiếu reporter cũ cũng tôn trọng Revoke().
        if (!AnalyticsConsent.IsGranted || UnityServices.State != ServicesInitializationState.Initialized) return;
        try
        {
            AnalyticsService.Instance.RecordEvent(analyticsEvent);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"⚠ Không ghi được sự kiện Analytics ({exception.GetType().Name}); gameplay tiếp tục.");
        }
    }
}
