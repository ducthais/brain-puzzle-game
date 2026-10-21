using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using UnityEngine;

/// <summary>Tải một lần mỗi phiên thành công; lỗi thì giữ mặc định và thử lại có khoảng chờ.</summary>
public sealed class RemoteGameConfigService
{
    private struct UserAttributes { }
    private struct AppAttributes { }
    private readonly CloudSyncState sync = new CloudSyncState();

    public RemoteGameConfigService() => sync.MarkChanged(Time.realtimeSinceStartupAsDouble);
    public void NetworkRestored() => sync.NetworkRestored();

    public async Task SynchronizeAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested || !OnlineServicesBootstrap.IsSignedIn
            || !sync.TryBegin(Time.realtimeSinceStartupAsDouble)) return;
        long revision = sync.Revision;
        try
        {
            var config = await RemoteConfigService.Instance.FetchConfigsAsync(new UserAttributes(), new AppAttributes());
            if (token.IsCancellationRequested) return;
            // SDK có thể trả cache khi HTTP lỗi; chỉ áp cấu hình mới tải thành công.
            if (config.origin != ConfigOrigin.Remote)
                throw new InvalidOperationException("Chưa nhận được cấu hình từ máy chủ.");
            GameConfigService.Apply(new GameConfig(
                config.GetInt(GameConfig.LevelsPerAdKey, GameConfig.DefaultLevelsPerAd),
                config.GetInt(GameConfig.DeathsPerAdKey, GameConfig.DefaultDeathsPerAd),
                config.GetBool(GameConfig.HintRequiresAdKey, true),
                config.GetBool(GameConfig.SkipEnabledKey, true)));
            sync.Succeed(revision);
        }
        catch (Exception exception)
        {
            if (token.IsCancellationRequested) return;
            sync.Fail(Time.realtimeSinceStartupAsDouble);
            Debug.LogWarning($"⚠ Chưa tải được Remote Config ({exception.GetType().Name}); giữ cấu hình mặc định và thử lại sau.");
        }
    }
}
