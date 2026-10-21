using System;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UnityConsent;

/// <summary>Chưa chọn/từ chối đều không thu thập; lựa chọn Analytics độc lập với Cloud Save.</summary>
public static class AnalyticsConsent
{
    public const string PreferenceKey = "AnalyticsConsent";
    public static bool HasDecided => PlayerPrefs.GetInt(PreferenceKey, -1) == 0 || IsGranted;
    public static bool IsGranted => PlayerPrefs.GetInt(PreferenceKey, -1) == 1;

    public static void Grant() => SetDecision(true);
    public static void Revoke() => SetDecision(false);

    private static void SetDecision(bool granted)
    {
        PlayerPrefs.SetInt(PreferenceKey, granted ? 1 : 0);
        PlayerPrefs.Save();
        Apply();
    }

    /// <summary>Gọi trước khởi tạo UGS và sau khi khởi tạo xong để gắn reporter.</summary>
    public static void Apply()
    {
        GameAnalytics.UseReporter(new NullAnalyticsReporter());
        try
        {
            // Unity 6000.6 + Analytics 6.3: dùng Developer Data, không dùng StartDataCollection.
            ConsentState state = EndUserConsent.GetConsentState();
            state.AnalyticsIntent = IsGranted ? ConsentStatus.Granted : ConsentStatus.Denied;
            EndUserConsent.SetConsentState(state);
            if (IsGranted && UnityServices.State == ServicesInitializationState.Initialized)
                GameAnalytics.UseReporter(new UnityAnalyticsReporter());
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"⚠ Chưa áp dụng được lựa chọn Analytics ({exception.GetType().Name}); game tiếp tục với reporter rỗng.");
        }
    }
}
