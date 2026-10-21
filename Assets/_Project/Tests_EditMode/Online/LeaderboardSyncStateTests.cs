using NUnit.Framework;

public class LeaderboardSyncStateTests
{
    private static ProgressData Stars(int value)
    {
        var data = new ProgressData();
        data.SetStarsIfHigher(1, value);
        return data;
    }

    [Test]
    public void TongSao_CongTatCaMan_VaChiuDuocDuLieuTrong()
    {
        ProgressData data = Stars(3);
        data.SetStarsIfHigher(2, 2);
        data.stars.Add(null);
        Assert.That(LeaderboardSyncState.TotalStars(data), Is.EqualTo(5));
        Assert.That(LeaderboardSyncState.TotalStars(null), Is.Zero);
        Assert.That(LeaderboardSyncState.TotalStars(new ProgressData { stars = null }), Is.Zero);
    }

    [Test]
    public void ChiGuiKhiTangSoVoiMocDaGui_KeCaSauReset()
    {
        var state = new LeaderboardSyncState(2);
        state.Observe(Stars(2), 0);
        Assert.That(state.TryBegin(2, out _), Is.False);
        state.Observe(Stars(1), 3);
        Assert.That(state.TryBegin(5, out _), Is.False);
        state.Observe(Stars(3), 6);
        Assert.That(state.TryBegin(8, out int score), Is.True);
        Assert.That(score, Is.EqualTo(3));
        state.Succeed();
        Assert.That(state.LastSubmitted, Is.EqualTo(3));
        state.Observe(Stars(1), 9);
        Assert.That(state.TryBegin(20, out _), Is.False);
    }

    [Test]
    public void Debounce_KhongBiLuiBoiThayDoiKhongLienQuanDenSao()
    {
        var state = new LeaderboardSyncState();
        state.Observe(Stars(1), 0);
        state.Observe(Stars(2), 1);
        state.Observe(Stars(2), 2);
        Assert.That(state.TryBegin(2.99, out _), Is.False);
        Assert.That(state.TryBegin(3, out int score), Is.True);
        Assert.That(score, Is.EqualTo(2));
        Assert.That(state.TryBegin(10, out _), Is.False);
    }

    [Test]
    public void TangSaoKhiDangGui_GiuDiemMoiChoLanSau()
    {
        var state = new LeaderboardSyncState();
        state.Observe(Stars(1), 0);
        state.TryBegin(2, out _);
        state.Observe(Stars(3), 3);
        state.Succeed();
        Assert.That(state.LastSubmitted, Is.EqualTo(1));
        Assert.That(state.TryBegin(4.99, out _), Is.False);
        Assert.That(state.TryBegin(5, out int score), Is.True);
        Assert.That(score, Is.EqualTo(3));
        state.Succeed();
        Assert.That(state.TryBegin(100, out _), Is.False);
    }

    [Test]
    public void Loi_RetryTangDan_ToiDa60Giay_KhongXacNhanDiemSom()
    {
        var state = new LeaderboardSyncState();
        state.Observe(Stars(3), 0);
        double now = 2;
        foreach (double delay in new double[] { 5, 10, 20, 40, 60, 60 })
        {
            Assert.That(state.TryBegin(now, out _), Is.True);
            state.Fail(now);
            Assert.That(state.LastSubmitted, Is.Zero);
            Assert.That(state.TryBegin(now + delay - .01, out _), Is.False);
            now += delay;
        }
        Assert.That(state.TryBegin(now, out _), Is.True);
        state.Succeed();
        Assert.That(state.LastSubmitted, Is.EqualTo(3));
    }

    [Test]
    public void CoMangLai_GiuDebounce_VaGuiGiaTriMoiNhat()
    {
        var state = new LeaderboardSyncState();
        state.Observe(Stars(1), 0);
        state.TryBegin(2, out _);
        state.Fail(2);
        state.Observe(Stars(2), 3);
        state.NetworkRestored();
        Assert.That(state.TryBegin(4.99, out _), Is.False);
        Assert.That(state.TryBegin(5, out int score), Is.True);
        Assert.That(score, Is.EqualTo(2));
    }

    [Test]
    public void KhongCoSao_KhongTaoDiemKhong()
    {
        var state = new LeaderboardSyncState();
        state.Observe(new ProgressData(), 0);
        Assert.That(state.TryBegin(100, out _), Is.False);
    }
}
