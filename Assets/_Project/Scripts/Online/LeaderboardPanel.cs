using System;
using System.Globalization;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

/// <summary>Top 10 và hạng cá nhân; phản hồi cũ không cập nhật panel đã đóng hoặc tài khoản khác.</summary>
public sealed class LeaderboardPanel : MonoBehaviour
{
    [Header("--- Bảng xếp hạng ---")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text[] rows = new TMP_Text[10];
    [SerializeField] private TMP_Text ownRankText;
    private int generation;
    private bool loading;
    private string visiblePlayerId;
    private static string SignedInPlayerId => OnlineServicesBootstrap.IsSignedIn
        ? AuthenticationService.Instance.PlayerId : null;

    private void OnEnable()
    {
        visiblePlayerId = SignedInPlayerId;
        Refresh();
    }

    private void Update()
    {
        if (visiblePlayerId == SignedInPlayerId) return;
        visiblePlayerId = SignedInPlayerId;
        generation++;
        loading = false;
        Refresh();
    }

    private void OnDisable()
    {
        generation++;
        loading = false;
    }

    public void Show() => gameObject.SetActive(true);
    public void Close() => gameObject.SetActive(false);

    public async void Refresh()
    {
        if (!isActiveAndEnabled || loading) return;
        int request = ++generation;
        ClearRows();
        SetText(ownRankText, "Hạng của bạn: —");
        string playerId = SignedInPlayerId;
        if (playerId == null)
        {
            SetText(statusText, "Chưa đăng nhập. Game sẽ tự kết nối khi có mạng.");
            return;
        }
        loading = true;
        SetText(statusText, "Đang tải bảng xếp hạng…");
        try
        {
            var topTask = LeaderboardsService.Instance.GetScoresAsync(LeaderboardScoreService.LeaderboardId,
                new GetScoresOptions { Offset = 0, Limit = 10 });
            var ownTask = GetOwnScoreAsync();
            await Task.WhenAll(topTask, ownTask);
            if (this == null || request != generation || playerId != SignedInPlayerId) return;
            var top = await topTask;
            var own = await ownTask;
            for (int i = 0; i < Math.Min(rows.Length, top.Results.Count); i++)
                SetText(rows[i], Format(top.Results[i]));
            SetText(ownRankText, own == null ? "Bạn chưa có điểm. Hãy hoàn thành một màn chơi!"
                : "Bạn: " + Format(own));
            SetText(statusText, top.Results.Count == 0 ? "Chưa có người chơi trong bảng xếp hạng." : "Tổng sao cao nhất");
        }
        catch (Exception exception)
        {
            if (this == null || request != generation) return;
            SetText(statusText, "Không tải được bảng xếp hạng. Kiểm tra mạng rồi bấm Thử lại.");
            Debug.LogWarning($"⚠ Chưa tải được bảng xếp hạng ({exception.GetType().Name}).");
        }
        finally
        {
            if (this != null && request == generation) loading = false;
        }
    }

    private static async Task<LeaderboardEntry> GetOwnScoreAsync()
    {
        try
        {
            return await LeaderboardsService.Instance.GetPlayerScoreAsync(LeaderboardScoreService.LeaderboardId);
        }
        catch (LeaderboardsException exception) when (exception.Reason == LeaderboardsExceptionReason.EntryNotFound)
        {
            return null;
        }
    }

    private static string Format(LeaderboardEntry entry)
    {
        string name = string.IsNullOrWhiteSpace(entry.PlayerName) ? ShortId(entry.PlayerId) : entry.PlayerName;
        return $"{entry.Rank + 1}.  {name}  —  {entry.Score.ToString("0", CultureInfo.InvariantCulture)} sao";
    }

    private static string ShortId(string id) => string.IsNullOrEmpty(id) ? "Người chơi"
        : "Eggy " + id.Substring(0, Math.Min(8, id.Length));

    private void ClearRows()
    {
        foreach (TMP_Text row in rows) SetText(row, "");
    }

    private static void SetText(TMP_Text label, string text)
    {
        if (label == null) return;
        label.richText = false;
        label.text = text;
    }
}
