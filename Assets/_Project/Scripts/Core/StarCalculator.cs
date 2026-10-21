/// <summary>
/// Luật tính sao khi kết thúc màn (bảng quyết định):
/// | Qua màn | Bỏ qua (Nhảy) | Không chết | Không xem hint | Sao |
/// |---------|---------------|------------|----------------|-----|
/// | Không   | -             | -          | -              | 0   |
/// | Có      | Có            | -          | -              | 1   |
/// | Có      | Không         | Có         | Có             | 3   |
/// | Có      | Không         | Có         | Không          | 2   |
/// | Có      | Không         | Không      | Có             | 2   |
/// | Có      | Không         | Không      | Không          | 1   |
/// </summary>
public static class StarCalculator
{
    public const int MaxStars = 3;

    public static int Calculate(bool completed, bool skipped, int deaths, int hintsUsed)
    {
        if (!completed) return 0;
        if (skipped) return 1;

        int stars = 1;
        if (deaths <= 0) stars++;
        if (hintsUsed <= 0) stars++;
        return stars;
    }
}
