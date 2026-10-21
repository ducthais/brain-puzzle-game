using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models;
using UnityEngine;

/// <summary>Local luôn sẵn sàng; cloud chỉ chạy bất đồng bộ trên luồng chính Unity.</summary>
public sealed class CloudProgressStore : IProgressStore, IDisposable
{
    public const string ProgressKey = "progress";
    public const string PendingKey = "CloudProgressPending";
    private readonly LocalProgressStore local = new LocalProgressStore();
    private readonly CloudSyncState sync = new CloudSyncState();
    private bool disposed;

    public bool NeedsSync => sync.NeedsSync;
    private static double Now => Time.realtimeSinceStartupAsDouble;

    public CloudProgressStore()
    {
        // Mỗi phiên đều đối chiếu cloud, kể cả lần trước đã đẩy thành công.
        // Cờ trên đĩa còn lại nếu ứng dụng bị đóng giữa yêu cầu mạng.
        sync.MarkChanged(Now);
        SetPending(true);
        ProgressService.Changed += OnProgressChanged;
    }

    public ProgressData Load() => local.Load();

    public void Save(ProgressData data)
    {
        if (data == null) return;
        local.Save(data);
        sync.MarkChanged(Now);
        SetPending(true);
    }

    private void OnProgressChanged(ProgressData data) => sync.Postpone(Now);

    public void NetworkRestored() => sync.NetworkRestored();

    /// <summary>Bootstrap chỉ gọi sau khi đăng nhập; không chặn scene hay gameplay.</summary>
    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        if (disposed || cancellationToken.IsCancellationRequested || !sync.TryBegin(Now)) return;
        try
        {
            // Luôn đọc lại khi thử lại: có thể lần ghi trước đã tới server nhưng mất phản hồi.
            var items = await CloudSaveService.Instance.Data.Player.LoadAsync(
                new HashSet<string> { ProgressKey });
            cancellationToken.ThrowIfCancellationRequested();
            if (disposed) return;

            string writeLock = null;
            if (items.TryGetValue(ProgressKey, out var item))
            {
                ProgressData remote = JsonUtility.FromJson<ProgressData>(item.Value.GetAs<string>());
                ValidateRemote(remote);
                writeLock = item.WriteLock;
                if (string.IsNullOrEmpty(writeLock))
                    throw new InvalidOperationException("Cloud thiếu write lock; giữ nguyên dữ liệu để tránh ghi đè.");
                ProgressService.MergeFrom(remote);
            }

            // Chụp sau MergeFrom để bao gồm mọi thay đổi local trong lúc chờ tải cloud.
            string json = JsonUtility.ToJson(ProgressService.Snapshot);
            long revision = sync.Revision;
            if (writeLock == null)
            {
                // UGS yêu cầu bỏ write lock khi tạo key lần đầu.
                await CloudSaveService.Instance.Data.Player.SaveAsync(
                    new Dictionary<string, object> { { ProgressKey, json } });
            }
            else
            {
                await CloudSaveService.Instance.Data.Player.SaveAsync(
                    new Dictionary<string, SaveItem> { { ProgressKey, new SaveItem(json, writeLock) } });
            }
            cancellationToken.ThrowIfCancellationRequested();
            if (disposed) return;
            sync.Succeed(revision);
            SetPending(sync.NeedsSync);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Cờ chờ vẫn nằm trên đĩa; lần mở sau đối chiếu lại từ local.
            if (!disposed) sync.Fail(Now);
        }
        catch (Exception exception)
        {
            if (disposed || cancellationToken.IsCancellationRequested) return;
            sync.Fail(Now);
            SetPending(true);
            Debug.LogWarning($"⚠ Đồng bộ cloud chưa thành công ({exception.GetType().Name}); tiến trình local vẫn được giữ, sẽ thử lại.");
        }
    }

    /// <summary>Chặn dữ liệu hỏng/phiên bản tương lai trước khi đi vào ProgressMerger.</summary>
    public static void ValidateRemote(ProgressData data)
    {
        if (data == null || data.version != ProgressData.CurrentVersion || data.maxUnlockedLevel < 1
            || !LevelNaming.TryParseLevelNumber(data.savedLevel, out _) || data.stars == null)
            throw new ArgumentException("Dữ liệu cloud không hợp lệ hoặc chưa hỗ trợ phiên bản này.");
        foreach (LevelStars entry in data.stars)
        {
            if (entry == null || entry.levelId < 1 || entry.stars < 0 || entry.stars > 3)
                throw new ArgumentException("Dữ liệu sao trên cloud không hợp lệ.");
        }
    }

    private static void SetPending(bool pending)
    {
        int value = pending ? 1 : 0;
        if (PlayerPrefs.GetInt(PendingKey, -1) == value) return;
        PlayerPrefs.SetInt(PendingKey, value);
        PlayerPrefs.Save();
    }

    public void Dispose()
    {
        disposed = true;
        ProgressService.Changed -= OnProgressChanged;
    }
}
