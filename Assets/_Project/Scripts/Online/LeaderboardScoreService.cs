using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using UnityEngine;

/// <summary>Bootstrap điều phối đăng nhập; dịch vụ không làm gián đoạn gameplay khi mạng lỗi.</summary>
public sealed class LeaderboardScoreService : IDisposable
{
    public const string LeaderboardId = "total_stars";
    private LeaderboardSyncState state;
    private string playerId;
    private bool disposed;
    private static double Now => Time.realtimeSinceStartupAsDouble;
    private string CacheKey => "LeaderboardTotalStars." + playerId;

    public LeaderboardScoreService() => ProgressService.Changed += OnChanged;
    private void OnChanged(ProgressData data) => state?.Observe(data, Now);
    public void NetworkRestored() => state?.NetworkRestored();

    public async Task SynchronizeAsync(CancellationToken token)
    {
        if (disposed || token.IsCancellationRequested || !OnlineServicesBootstrap.IsSignedIn) return;
        string currentId = AuthenticationService.Instance.PlayerId;
        if (playerId != currentId)
        {
            playerId = currentId;
            state = new LeaderboardSyncState(PlayerPrefs.GetInt(CacheKey, 0));
            state.Observe(ProgressService.Snapshot, Now);
        }
        LeaderboardSyncState requestState = state;
        string requestKey = CacheKey;
        if (!requestState.TryBegin(Now, out int score)) return;
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, score);
            if (disposed || token.IsCancellationRequested) return;
            requestState.Succeed();
            PlayerPrefs.SetInt(requestKey, requestState.LastSubmitted);
            PlayerPrefs.Save();
        }
        catch (Exception exception)
        {
            if (disposed || token.IsCancellationRequested) return;
            requestState.Fail(Now);
            Debug.LogWarning($"⚠ Chưa gửi được tổng sao ({exception.GetType().Name}); sẽ thử lại khi kết nối ổn định.");
        }
    }

    public void Dispose()
    {
        disposed = true;
        ProgressService.Changed -= OnChanged;
    }
}
