using System.Collections.Generic;
using NUnit.Framework;

public class GameConfigTests
{
    [Test]
    public void ThieuCauHinh_GiuHanhViCu()
    {
        GameConfig config = GameConfig.Parse(null);
        Assert.That(config.LevelsPerAd, Is.EqualTo(2));
        Assert.That(config.DeathsPerAd, Is.EqualTo(GameConfig.DefaultDeathsPerAd));
        Assert.That(config.HintRequiresAd, Is.True);
        Assert.That(config.SkipEnabled, Is.True);
    }

    [Test]
    public void CauHinhHopLe_DocDuBonKey()
    {
        GameConfig config = GameConfig.Parse(new Dictionary<string, string>
        {
            { GameConfig.LevelsPerAdKey, "1" }, { GameConfig.DeathsPerAdKey, "20" },
            { GameConfig.HintRequiresAdKey, "false" }, { GameConfig.SkipEnabledKey, "false" }
        });
        Assert.That(config.LevelsPerAd, Is.EqualTo(1));
        Assert.That(config.DeathsPerAd, Is.EqualTo(20));
        Assert.That(config.HintRequiresAd, Is.False);
        Assert.That(config.SkipEnabled, Is.False);
    }

    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("21")]
    [TestCase("1.5")]
    [TestCase("999999999999999999999")]
    [TestCase("không hợp lệ")]
    [TestCase(null)]
    public void GiaTriLoi_ChiKeyDoVeMacDinh(string raw)
    {
        GameConfig config = GameConfig.Parse(new Dictionary<string, string>
        {
            { GameConfig.LevelsPerAdKey, raw }, { GameConfig.DeathsPerAdKey, raw },
            { GameConfig.HintRequiresAdKey, raw }, { GameConfig.SkipEnabledKey, "false" }
        });
        Assert.That(config.LevelsPerAd, Is.EqualTo(2));
        Assert.That(config.DeathsPerAd, Is.EqualTo(GameConfig.DefaultDeathsPerAd));
        Assert.That(config.HintRequiresAd, Is.True);
        Assert.That(config.SkipEnabled, Is.False);
    }

    [TestCase(0, 21)]
    [TestCase(-1, int.MaxValue)]
    public void Constructor_CungChanGiaTriNgoaiKhoang(int levels, int deaths)
    {
        var config = new GameConfig(levels, deaths);
        Assert.That(config.LevelsPerAd, Is.EqualTo(2));
        Assert.That(config.DeathsPerAd, Is.EqualTo(GameConfig.DefaultDeathsPerAd));
    }
}
