using NUnit.Framework;

/// <summary>
/// Kiểm thử luật 3 sao theo bảng quyết định trong StarCalculator.
/// </summary>
public class StarCalculatorTests
{
    // Mỗi dòng = 1 luật của bảng quyết định: (qua màn, bỏ qua, số lần chết, số lần hint) → số sao
    [TestCase(false, false, 0, 0, 0, TestName = "R1_ChuaQuaMan_0Sao")]
    [TestCase(true, true, 0, 0, 1, TestName = "R2_BoQuaMan_1Sao")]
    [TestCase(true, false, 0, 0, 3, TestName = "R3_KhongChet_KhongHint_3Sao")]
    [TestCase(true, false, 0, 2, 2, TestName = "R4_KhongChet_CoHint_2Sao")]
    [TestCase(true, false, 3, 0, 2, TestName = "R5_CoChet_KhongHint_2Sao")]
    [TestCase(true, false, 5, 1, 1, TestName = "R6_CoChet_CoHint_1Sao")]
    public void Calculate_FollowsDecisionTable(bool completed, bool skipped, int deaths, int hints, int expected)
    {
        Assert.AreEqual(expected, StarCalculator.Calculate(completed, skipped, deaths, hints));
    }

    // Giá trị biên: chết đúng 1 lần đã mất sao "không chết"
    [Test]
    public void Calculate_OneDeath_LosesNoDeathStar()
    {
        Assert.AreEqual(2, StarCalculator.Calculate(true, false, 1, 0));
    }

    // Dữ liệu bất thường (âm) được coi như 0
    [Test]
    public void Calculate_NegativeCounters_TreatedAsZero()
    {
        Assert.AreEqual(StarCalculator.MaxStars, StarCalculator.Calculate(true, false, -1, -1));
    }

    [Test]
    public void Calculate_NeverExceedsMaxStars()
    {
        for (int deaths = 0; deaths < 3; deaths++)
            for (int hints = 0; hints < 3; hints++)
                Assert.LessOrEqual(StarCalculator.Calculate(true, false, deaths, hints), StarCalculator.MaxStars);
    }
}
