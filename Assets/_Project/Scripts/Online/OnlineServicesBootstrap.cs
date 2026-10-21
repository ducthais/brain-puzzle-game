using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

/// <summary>Tự tạo trước scene đầu tiên, giữ qua các scene; không cần gán Inspector.</summary>
public sealed class OnlineServicesBootstrap : MonoBehaviour
{
    private static OnlineServicesBootstrap instance;
    private CloudProgressStore store;
    private LeaderboardScoreService leaderboard;
    private RemoteGameConfigService remoteConfig;
    private CancellationTokenSource lifetime;

    public static bool IsSignedIn => instance != null
        && UnityServices.State == ServicesInitializationState.Initialized
        && AuthenticationService.Instance.IsAuthorized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (instance == null) new GameObject("OnlineServicesBootstrap").AddComponent<OnlineServicesBootstrap>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        lifetime = new CancellationTokenSource();
        store = new CloudProgressStore();
        ProgressService.UseStore(store);
        leaderboard = new LeaderboardScoreService();
        remoteConfig = new RemoteGameConfigService();
        AnalyticsConsent.Apply();
    }

    private async void Start()
    {
        if (instance != this) return;
        CancellationToken token = lifetime.Token;
        double retryAt = 0;
        double retryDelay = 5;
        bool wasOffline = false;
        bool consentAppliedAfterInitialization = false;
        try
        {
            if (string.IsNullOrWhiteSpace(Application.cloudProjectId))
            {
                Debug.LogWarning("⚠ Chưa liên kết Unity Cloud Project ID; game dùng tiến trình local. Liên kết project rồi mở lại game để bật online.");
                return;
            }
            while (!token.IsCancellationRequested)
            {
                bool offline = Application.internetReachability == NetworkReachability.NotReachable;
                if (offline && !wasOffline)
                    Debug.LogWarning("⚠ Thiết bị đang mất mạng; game tiếp tục lưu local và sẽ đồng bộ khi có mạng.");
                if (!offline && wasOffline)
                {
                    retryAt = 0;
                    store.NetworkRestored();
                    leaderboard.NetworkRestored();
                    remoteConfig.NetworkRestored();
                }
                wasOffline = offline;

                if (!offline && Time.realtimeSinceStartupAsDouble >= retryAt)
                {
                    try
                    {
                        if (UnityServices.State != ServicesInitializationState.Initialized)
                            await UnityServices.InitializeAsync();
                        token.ThrowIfCancellationRequested();
                        if (!consentAppliedAfterInitialization)
                        {
                            AnalyticsConsent.Apply();
                            consentAppliedAfterInitialization = true;
                        }
                        // IsSignedIn vẫn có thể true khi token hết hạn; cần kiểm tra IsAuthorized.
                        if (!AuthenticationService.Instance.IsAuthorized)
                            await AuthenticationService.Instance.SignInAnonymouslyAsync();
                        token.ThrowIfCancellationRequested();
                        if (!AuthenticationService.Instance.IsAuthorized)
                            throw new InvalidOperationException("Phiên đăng nhập chưa được xác thực; chờ SDK khôi phục.");
                        await Task.WhenAll(store.SynchronizeAsync(token),
                            leaderboard.SynchronizeAsync(token), remoteConfig.SynchronizeAsync(token));
                        retryDelay = 5;
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"⚠ Chưa kết nối được dịch vụ online ({exception.GetType().Name}); game vẫn lưu local, sẽ thử lại.");
                        retryAt = Time.realtimeSinceStartupAsDouble + retryDelay;
                        retryDelay = Math.Min(60, retryDelay * 2);
                    }
                }
                await Task.Delay(1000, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (Exception exception)
        {
            Debug.LogWarning($"⚠ Dịch vụ online đã dừng ({exception.GetType().Name}); tiến trình vẫn được lưu local.");
        }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) PlayerPrefs.Save();
        else
        {
            store?.NetworkRestored();
            leaderboard?.NetworkRestored();
            remoteConfig?.NetworkRestored();
        }
    }

    private void OnDestroy()
    {
        lifetime?.Cancel();
        lifetime?.Dispose();
        store?.Dispose();
        leaderboard?.Dispose();
        if (instance == this)
        {
            instance = null;
            GameAnalytics.UseReporter(new NullAnalyticsReporter());
        }
    }
}
