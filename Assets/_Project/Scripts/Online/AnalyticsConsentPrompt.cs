using UnityEngine;

/// <summary>Gắn vào controller đang bật; gán panel riêng và nối hai nút trong Inspector.</summary>
public sealed class AnalyticsConsentPrompt : MonoBehaviour
{
    [Header("--- Đồng ý Analytics ---")]
    [SerializeField] private GameObject consentPanel = null;

    private void Start() => ShowIfNeeded();

    public void ShowIfNeeded()
    {
        if (consentPanel != null) consentPanel.SetActive(!AnalyticsConsent.HasDecided);
    }

    public void Grant()
    {
        AnalyticsConsent.Grant();
        if (consentPanel != null) consentPanel.SetActive(false);
    }

    public void Revoke()
    {
        AnalyticsConsent.Revoke();
        if (consentPanel != null) consentPanel.SetActive(false);
    }
}
