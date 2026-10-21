# AGENTS.md — Hướng dẫn cho AI agent (Claude Code, Codex, Antigravity)

Dự án: game giải đố 2D **Mr. Eggy** (Unity `6000.6.6f1`, C#). Đồ án chuyên ngành IT6122.
Kế hoạch tổng: [`docs/KE_HOACH_DO_AN.md`](docs/KE_HOACH_DO_AN.md) · Bảng nhận việc: [`docs/TASK_BOARD.md`](docs/TASK_BOARD.md)

## 1. Quy tắc phối hợp giữa các agent

1. **Trước khi làm:** đọc `docs/TASK_BOARD.md`, chỉ nhận task có cột *Agent* là mình (hoặc `—` chưa ai nhận). Đổi trạng thái sang `🔄 Đang làm` và commit ngay để agent khác thấy.
2. **Làm trên nhánh + worktree riêng**, không làm thẳng trên `main` và không làm trong thư mục chính (người dùng đang mở Unity ở đó):
   ```bash
   git worktree add ../MrEggy-<agent> -b agent/<agent>-<chu-de>   # ví dụ agent/codex-online
   ```
3. **Không sửa file thuộc task của agent khác.** Cần thay đổi ở đó → ghi vào mục *Ghi chú / yêu cầu* của task trong `TASK_BOARD.md`.
4. **Xong task:** test pass → đổi trạng thái `✅ Xong`, ghi tên nhánh/commit → báo người dùng merge vào `main`.
5. **Hợp đồng interface** (mục 4) là ranh giới giữa các phần. Muốn đổi chữ ký interface phải ghi đề xuất vào `TASK_BOARD.md` trước.

## 2. Cấu trúc dự án

| Đường dẫn | Nội dung |
|-----------|----------|
| `Assets/_Project/Scripts/` | Code game — assembly `MainGameScripts` (asmdef) |
| `Assets/_Project/Scripts/Core/` | Logic thuần C# (không phụ thuộc MonoBehaviour), dễ unit test |
| `Assets/_Project/Scripts/Editor/` | Tool Editor — assembly `MainGameScripts.Editor` (chỉ Editor) |
| `Assets/_Project/Tests_EditMode/` | Unit test EditMode |
| `Assets/_Project/Tests_PlayMode/` | Test PlayMode — assembly `Tests_PlayMode` |
| `Assets/_Project/Scenes/` | `StartScreen`, `LevelSelect`, `Level 1` … `Level N` |
| `Assets/_Project/GameData/GameHintData.asset` | Gợi ý theo màn (`LevelDatabase`) |
| `docs/` | Kế hoạch, báo cáo, tài liệu kiểm thử |

## 3. Quy ước code

- C#, comment và log **bằng tiếng Việt**, theo phong cách code sẵn có (header `[Header("--- ... ---")]`, log có emoji ✅ ⚠ ❌ ở tool Editor).
- Logic nghiệp vụ (tiến trình, tính sao, gộp dữ liệu cloud) viết thành **class C# thường** trong `Scripts/Core/` để test EditMode được.
- **Không đổi tên field `public` / `[SerializeField]`** đang được gán trong Inspector (sẽ mất tham chiếu trong scene).
- **Không sửa trực tiếp file `.unity` / `.prefab` bằng text** trừ khi bắt buộc; ưu tiên viết tool Editor (`Tools/...`) để người dùng bấm chạy trong Unity.
- Tên scene màn chơi bắt buộc dạng `Level <số>` (code parse số từ tên).
- PlayerPrefs keys đang dùng: `MaxUnlockedLevel`, `SavedLevel`, `CurrentLevel`, `isPremium`, `MusicMuted`, `SfxMuted` — truy cập tiến trình qua `ProgressService`, không gọi PlayerPrefs trực tiếp ở code mới.
- Commit message: `<loại>: <mô tả>` (`feat`, `fix`, `test`, `docs`, `refactor`, `ci`, `level`).

## 4. Hợp đồng interface (ranh giới giữa các phần)

```csharp
// Lưu trữ tiến trình (Claude: Local; Codex: Cloud)
public interface IProgressStore
{
    ProgressData Load();
    void Save(ProgressData data);
}

// Gửi sự kiện phân tích (Claude: NullAnalyticsReporter; Codex: bản Unity Analytics)
public interface IAnalyticsReporter
{
    void LevelStart(int levelId, int attempt);
    void LevelComplete(int levelId, float durationSec, int deaths, int hintsUsed, int stars);
    void PlayerDeath(int levelId, float timeInLevelSec);
    void HintUsed(int levelId, bool viaAd);
    void LevelSkip(int levelId, float timeInLevelSec);
}
```

Chi tiết `ProgressData`, luật gộp dữ liệu, luật tính sao: xem code trong `Scripts/Core/` và Phụ lục A, B của kế hoạch.

## 5. Kiểm tra trước khi báo xong

- Code biên dịch được (mở Unity hoặc `dotnet build` với DLL Unity tham chiếu).
- Test EditMode/PlayMode liên quan pass.
- Không còn lỗi đỏ trong Console Unity.
- Việc cần người dùng làm trong Unity (gán Inspector, chạy tool, build APK) → ghi rõ trong task.
