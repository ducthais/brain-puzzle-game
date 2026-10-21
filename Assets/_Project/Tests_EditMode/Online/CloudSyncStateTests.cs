using NUnit.Framework;

public class CloudSyncStateTests
{
    [Test]
    public void ChuaThayDoi_KhongGuiCloud()
    {
        var state = new CloudSyncState();
        Assert.That(state.TryBegin(100), Is.False);
    }

    [Test]
    public void NhieuThayDoi_GomSauHaiGiayKeTuThayDoiCuoi()
    {
        var state = new CloudSyncState();
        state.MarkChanged(0);
        state.MarkChanged(1);
        Assert.That(state.TryBegin(2.99), Is.False);
        Assert.That(state.TryBegin(3), Is.True);
        Assert.That(state.TryBegin(10), Is.False);
    }

    [Test]
    public void ThanhCong_KhongConChoDongBo()
    {
        var state = new CloudSyncState();
        state.MarkChanged(0);
        state.TryBegin(2);
        state.Succeed(state.Revision);
        Assert.That(state.NeedsSync, Is.False);
        Assert.That(state.IsInFlight, Is.False);
    }

    [Test]
    public void ThayDoiTrongLucDangGui_KhongBiXoaCoCho()
    {
        var state = new CloudSyncState();
        state.MarkChanged(0);
        state.TryBegin(2);
        long sent = state.Revision;
        state.MarkChanged(3);
        state.Succeed(sent);
        Assert.That(state.NeedsSync, Is.True);
        Assert.That(state.TryBegin(4.99), Is.False);
        Assert.That(state.TryBegin(5), Is.True);
    }

    [Test]
    public void Loi_GiuCoChoVaKhongSpamKeCaCoThayDoiMoi()
    {
        var state = new CloudSyncState();
        state.MarkChanged(0);
        state.TryBegin(2);
        state.Fail(2);
        state.MarkChanged(3);
        Assert.That(state.NeedsSync, Is.True);
        Assert.That(state.TryBegin(6.99), Is.False);
        Assert.That(state.TryBegin(7), Is.True);
        state.Fail(7);
        Assert.That(state.TryBegin(16.99), Is.False);
        Assert.That(state.TryBegin(17), Is.True);
    }

    [Test]
    public void ThuLai_KhoangChoKhongVuotQuaSauMuoiGiay()
    {
        var state = new CloudSyncState();
        state.MarkChanged(0);
        double now = 2;
        foreach (double delay in new double[] { 5, 10, 20, 40, 60, 60 })
        {
            Assert.That(state.TryBegin(now), Is.True);
            state.Fail(now);
            Assert.That(state.TryBegin(now + delay - 0.01), Is.False);
            now += delay;
        }
        Assert.That(state.TryBegin(now), Is.True);
    }

    [Test]
    public void CoMangLai_BoChoLoiNhungVanGiuDebounce()
    {
        var state = new CloudSyncState();
        state.MarkChanged(0);
        state.TryBegin(2);
        state.Fail(2);
        state.MarkChanged(3);
        state.NetworkRestored();
        Assert.That(state.TryBegin(4), Is.False);
        Assert.That(state.TryBegin(5), Is.True);
    }

    [Test]
    public void ThanhCong_DatLaiKhoangChoLoiVeNamGiay()
    {
        var state = new CloudSyncState();
        state.MarkChanged(0);
        state.TryBegin(2);
        state.Fail(2);
        state.TryBegin(7);
        state.Succeed(state.Revision);
        state.MarkChanged(10);
        state.TryBegin(12);
        state.Fail(12);
        Assert.That(state.TryBegin(16.99), Is.False);
        Assert.That(state.TryBegin(17), Is.True);
    }
}
