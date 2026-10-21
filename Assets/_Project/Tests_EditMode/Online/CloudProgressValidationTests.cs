using System;
using NUnit.Framework;

public class CloudProgressValidationTests
{
    [Test]
    public void SchemaHienTai_DuocChapNhan()
    {
        var data = new ProgressData();
        data.SetStarsIfHigher(1, 3);
        Assert.DoesNotThrow(() => CloudProgressStore.ValidateRemote(data));
    }

    [Test]
    public void PhienBanTuongLai_KhongGhiDeCloud()
    {
        var data = new ProgressData { version = ProgressData.CurrentVersion + 1 };
        Assert.Throws<ArgumentException>(() => CloudProgressStore.ValidateRemote(data));
    }

    [TestCase(-1)]
    [TestCase(4)]
    public void SoSaoNgoaiMien_BiTuChoi(int stars)
    {
        var data = new ProgressData();
        data.stars.Add(new LevelStars { levelId = 1, stars = stars });
        Assert.Throws<ArgumentException>(() => CloudProgressStore.ValidateRemote(data));
    }

    [Test]
    public void PhanTuNull_BiTuChoiTruocKhiGop()
    {
        var data = new ProgressData();
        data.stars.Add(null);
        Assert.Throws<ArgumentException>(() => CloudProgressStore.ValidateRemote(data));
    }

    [Test]
    public void SceneSaiQuyUoc_BiTuChoi()
    {
        var data = new ProgressData { savedLevel = "LevelSelect" };
        Assert.Throws<ArgumentException>(() => CloudProgressStore.ValidateRemote(data));
    }
}
