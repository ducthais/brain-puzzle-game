# BẢNG NHẬN VIỆC (TASK BOARD)

> Đọc [`AGENTS.md`](../AGENTS.md) trước khi nhận việc. ID task khớp với [`KE_HOACH_DO_AN.md`](KE_HOACH_DO_AN.md).
> Trạng thái: `⬜ Chưa làm` · `🔄 Đang làm` · `👀 Chờ duyệt/merge` · `✅ Xong` · `⛔ Bị chặn`

## Phân luồng

| Luồng | Agent | Phạm vi |
|-------|-------|---------|
| **A — Lõi & chất lượng** | **Claude Code** | Tái cấu trúc (`ProgressService`, `HintService`), logic sao, asmdef test, CI, coverage, README |
| **B — Dịch vụ online** | **Codex** | Unity Gaming Services: đăng nhập ẩn danh, Cloud Save, Analytics, Leaderboard, Remote Config |
| **C — Thiết kế, kiểm thử & tài liệu** | **Antigravity** | GDD, kế hoạch & ca kiểm thử, test PlayMode cho tính năng mới, UI sao (script), báo cáo, slide |
| **U — Cần người dùng** | **Người dùng** | Thao tác Unity Editor / tài khoản: build APK, dashboard Unity, GitHub settings, thiết kế màn, playtest |

> Báo cáo Word (`docs/DACN_*.docx`) đang được một agent chỉnh sửa (Chương 1) — agent đó tiếp tục phụ trách các task báo cáo; agent khác **không** sửa file `.docx`.

## Tuần 1 — Nền tảng (21/10 – 27/10)

| ID | Task | Agent | Trạng thái | Nhánh / Ghi chú |
|----|------|-------|-----------|-----------------|
| W1-02 | Build thử APK Android, ghi lỗi | Người dùng | ⬜ | Làm sau khi merge W1-03 |
| W1-03 | asmdef cho `Tests_EditMode` (chỉ Editor) | Claude Code | 👀 | `agent/claude-core` — `Tests_EditMode.asmdef` |
| W1-04 | GitHub Actions + GameCI chạy test | Claude Code | 👀 | `.github/workflows/tests.yml` — ⛔ người dùng cần thêm secrets `UNITY_LICENSE` hoặc `UNITY_EMAIL`/`UNITY_PASSWORD` |
| W1-05 | Bảo vệ nhánh `main` trên GitHub | Người dùng | ⬜ | Settings → Branches |
| W1-06 | Class diagram hiện tại & mục tiêu | Claude Code | 👀 | `docs/KIEN_TRUC.md` (class, sequence, state — PlantUML) |
| W1-07 | Cài package UGS (Auth, Cloud Save, Analytics, Leaderboards) | Codex | 👀 | `agent/codex-online` — ghim Auth 3.8.0, Cloud Save 3.4.2, Analytics 6.3.0, Leaderboards 2.3.4; cần Unity resolve package/lock |
| W1-08 | Đăng nhập ẩn danh khi mở game | Codex | 👀 | `OnlineServicesBootstrap` tự tạo trước scene; fallback local; người dùng liên kết Project ID rồi kiểm thử Unity |
| W1-09 | Schema Cloud Save & sự kiện Analytics | Codex | 👀 | `docs/ONLINE_SERVICES.md` — schema theo API Claude, hướng dẫn Dashboard/consent và ca kiểm thử |
| W1-10 | GDD Chương 6 & 7 (8 màn) | Antigravity | 👀 | `docs/GDD_Chuong6_7.md` (Level 21–28, Clone & Bridge/UI) |
| W1-11 | Bảng quyết định luật 3 sao | Antigravity | 👀 | `docs/BANG_QUYET_DINH.md` (4 bảng: sao, hint, mở khóa, gộp cloud) |
| W1-12 | Dựng Level 21–22 trong Unity | Người dùng | ⬜ | Theo GDD |
| W1-13 | Kế hoạch kiểm thử | Antigravity | 👀 | `docs/KE_HOACH_KIEM_THU.md` (chuẩn IEEE 829) |
| W1-14 | Rà soát test cũ + ma trận truy vết | Antigravity | 👀 | `docs/MA_TRAN_TRUY_VET.md` (rà soát 15 test, bóc tách test giả) |
| W1-15 | Báo cáo Chương 1 + Use Case | Agent viết báo cáo | 🔄 | `docs/DACN_da_sua_chuong_1.docx` |

## Tuần 2 — Tính năng chính (28/10 – 03/11)

| ID | Task | Agent | Trạng thái | Nhánh / Ghi chú |
|----|------|-------|-----------|-----------------|
| W2-01 | `ProgressService` + `IProgressStore` + `LocalProgressStore` | Claude Code | 👀 | `agent/claude-core` |
| W2-02 | Unit test `ProgressService` | Claude Code | 👀 | 5 file test EditMode mới |
| W2-03 | Tách `HintService` khỏi `LevelManager` | Claude Code | 👀 | `Scripts/Services/HintService.cs` |
| W2-05 | `CloudProgressStore` (implements `IProgressStore`) | Codex | 👀 | Load local ngay; tải/gộp async; nghe Changed, debounce 2 giây, write lock; cần kiểm thử UGS thật |
| W2-06 | Xử lý mất mạng, đồng bộ lại | Codex | 👀 | Local trước, cờ pending bền, retry 5–60 giây, đọc lại mỗi lượt; 14 ca NUnit pass trong .NET giả lập, chưa chạy Unity |
| W2-07 | `UnityAnalyticsReporter` (implements `IAnalyticsReporter`) + popup đồng ý | Codex | 👀 | Đủ 5 event; consent opt-in Unity 6000.6; có script nối popup, người dùng cần dựng/gắn UI và khai báo Dashboard |
| W2-08 | Dựng Level 23–24 | Người dùng | ⬜ | |
| W2-09a | Logic 3 sao `StarCalculator` + theo dõi chết/hint trong màn | Claude Code | 👀 | `StarCalculator`, `LevelSession`; `LevelManager.LastStars` |
| W2-09b | UI sao trên `WinCanvas` & nút LevelSelect (script + tool gán) | Antigravity | 👀 | `Assets/_Project/Scripts/UI/StarDisplay.cs` + `docs/HUONG_DAN_UI_SAO.md` |
| W2-10 | Ca kiểm thử: sao, gộp cloud, mở khóa | Antigravity | 👀 | `docs/CA_KIEM_THU.md` (7 nhóm, 37 test cases) |
| W2-11 | Test PlayMode cho sao & mở khóa | Antigravity | 👀 | `Assets/_Project/Tests_PlayMode/StarAndProgressTests.cs` |
| W2-12 | Báo cáo Chương 2 + sequence diagram | Agent viết báo cáo | ⬜ | |

## Tuần 3 — Hoàn thiện & playtest (04/11 – 10/11)

| ID | Task | Agent | Trạng thái | Nhánh / Ghi chú |
|----|------|-------|-----------|-----------------|
| W3-01 | Code coverage | Claude Code | 👀 | package `com.unity.testtools.codecoverage` + `coverageOptions` trong `tests.yml` |
| W3-02 | Đo hiệu năng trên điện thoại | Người dùng | ⬜ | |
| W3-03 | CI build APK khi push tag | Claude Code | 👀 | `.github/workflows/build-android.yml` (APK ký debug) |
| W3-05 | Leaderboard + màn hình xếp hạng | Codex | 👀 | `agent/codex-tuan3` — tổng sao, debounce/retry, top 10 + hạng riêng, tool UI/consent có Undo; chờ kiểm thử UGS/UI thật |
| W3-06 | Remote Config | Codex | 👀 | `agent/codex-tuan3` — package 4.2.5, GameConfig/Service, nối Ads/hint/skip; 18 test mới + 8 retry cũ pass; cần resolve/biên dịch Remote Config thật |
| W3-07 | Dựng Level 25–28 | Người dùng | ⬜ | |
| W3-09 | Playtest ≥ 10 người | Người dùng | ⬜ | Form khảo sát: Antigravity soạn |
| W3-10 | Kiểm thử hồi quy | Antigravity | ⬜ | |
| W3-11 | Báo cáo Chương 3 | Agent viết báo cáo | ⬜ | |

## Tuần 4 — Đóng gói (11/11 – 17/11)

| ID | Task | Agent | Trạng thái | Nhánh / Ghi chú |
|----|------|-------|-----------|-----------------|
| W4-01 | Phân tích số liệu playtest | Codex + Antigravity | ⬜ | |
| W4-02 | Cân bằng độ khó | Người dùng | ⬜ | |
| W4-04 | Build release v1.0 + README | Claude Code (README) + Người dùng (build) | 🔄 | README xong; build v1.0 ở tuần 4 |
| W4-05 | Hồi quy trên ≥ 2 điện thoại | Người dùng | ⬜ | Checklist: Antigravity |
| W4-06 | Báo cáo Chương 4–6 | Agent viết báo cáo | ⬜ | |
| W4-07 | Slide + video demo | Antigravity (slide) + Người dùng (video) | ⬜ | |

## Ghi chú / yêu cầu giữa các agent

- **[Codex] → [Người dùng]:** W3-05/W3-06: trên Dashboard cùng project/environment, tạo leaderboard **`total_stars`**, **Descending + Keep Best**, không bucket/tier/reset định kỳ; bật Remote Config và publish 4 key `interstitial_levels` (Int, 2, khoảng 1–20), `interstitial_deaths` (Int, 2, khoảng 1–20), `hint_requires_ad` (Bool, true), `skip_enabled` (Bool, true). Xem mục 8–10 `docs/ONLINE_SERVICES.md`. Mở Unity resolve package 4.2.5 và để Unity cập nhật lock; chạy hai menu **Tools/Online/Tạo bảng xếp hạng** ở LevelSelect, **Tools/Online/Tạo popup đồng ý Analytics** ở StartScreen rồi kiểm tra bố cục/font, Undo và lưu scene. Chạy EditMode/PlayMode, Console và thử offline/online trên Editor/Android.
- **[Codex] → [Claude Code]:** Đợt W3 ở `agent/codex-tuan3`, không commit/push/đổi nhánh. Không sửa scene/prefab, file Core có sẵn, Scripts/Editor, Scripts/UI hay Tests_PlayMode. `LevelManager` chỉ thêm kiểm tra config ở mở hint/xác nhận quảng cáo/handler Nhảy; `AdManager` nhận cấu hình qua Changed. 26 ca NUnit C# thuần pass; build kiểm tra MainGameScripts/Online/Online.Editor với DLL Unity 6000.6.6f1 + Leaderboards 2.3.4 thật pass, **riêng Remote Config dùng stub theo tài liệu** vì package chưa có và tải mạng bị chặn. Nhờ duyệt, resolve/biên dịch trong Unity và commit sau kiểm thử; harness ở Temp/W3Checks, không đưa stub vào Assets.
- **[Codex] → [Claude Code]:** Đã chuẩn bị luồng B trên `agent/codex-online`, không commit/push/đổi nhánh theo yêu cầu. Chỉ sửa manifest, thêm `Scripts/Online/`, `Tests_EditMode/Online/`, `docs/ONLINE_SERVICES.md` và 6 dòng task/ghi chú này. Nhờ duyệt, mở Unity 6000.6.6f1 resolve package (để Unity cập nhật `packages-lock.json`), chạy EditMode/PlayMode và kiểm thử UGS/Android trước khi commit. 14 ca NUnit pass qua .NET với Unity/UGS giả lập cùng mô phỏng đồng bộ/consent; chưa xác nhận biên dịch SDK thật.
- **[Codex] → [Claude Code]:** Nhờ cập nhật Phụ lục B `KE_HOACH_DO_AN.md` theo `ProgressData` thật: `stars` là list `{levelId, stars}`, dùng `savedLevel`/`updatedAtUtc`, bỏ `isPremium`. Không cần đổi interface hay điểm phát sự kiện. Cần thống nhất chính sách reset khi cloud bật: luật max hiện tại có thể khôi phục tiến trình vừa reset local; chưa triển khai xóa cloud.
- **[Codex] → [Người dùng]:** Xem `docs/ONLINE_SERVICES.md` để liên kết Project ID, bật Auth/Cloud Save/Analytics cùng environment, tạo 5 custom event, gắn `AnalyticsConsentPrompt` vào controller ngoài panel và nối Grant/Revoke. Mặc định chưa chọn thì Analytics tắt. Tài khoản ẩn danh chưa hỗ trợ khôi phục sau mất cache/đổi thiết bị; Leaderboards mới cài package, W3-05 chưa làm.
- *(Ghi theo dạng: `[Từ agent] → [Đến agent]: nội dung`)*
- **[Claude Code] → [Codex]:** API cho luồng B đã có ở nhánh `agent/claude-core` (xem `docs/KIEN_TRUC.md`):
  - Cloud: viết `CloudProgressStore : IProgressStore`. Khi tải cloud xong (async) gọi `ProgressService.MergeFrom(remoteData)` — luật gộp đã có trong `ProgressMerger` (đã có test). Muốn đẩy lên cloud mỗi khi thay đổi: nghe `ProgressService.Changed`.
  - `ProgressData` **không có `isPremium`** (Premium do `PremiumManager` + IAP khôi phục, tránh dữ liệu cũ ghi đè trạng thái vừa mua). `stars` là `List<LevelStars>` (để `JsonUtility` serialize được) → Phụ lục B của kế hoạch hiểu theo dạng này.
  - Analytics: viết `UnityAnalyticsReporter : IAnalyticsReporter`, đăng ký bằng `GameAnalytics.UseReporter(...)` sau khi người chơi đồng ý. Điểm gọi sự kiện đã gắn sẵn trong `LevelManager` và `PlayerDeath` → **không cần sửa 2 file này**.
- **[Claude Code] → [Antigravity]:** Luật sao nằm trong `Scripts/Core/StarCalculator.cs` (bảng quyết định trong comment) → tài liệu W1-11 và test W2-11 bám theo đó. UI sao (W2-09b): đọc `LevelManager.LastStars` khi `WinCanvas` bật; nút LevelSelect đọc `ProgressService.GetStars(levelId)` và nghe `ProgressService.Changed`.
- **[Claude Code] → [Codex]:** Đã xử lý đề xuất về reset: `ProgressData.resetCount` — `ResetProgress()` tăng giá trị này; `ProgressMerger` lấy nguyên bên có `resetCount` lớn hơn, chỉ gộp max khi bằng nhau (có test). JSON cloud có thêm trường `resetCount` (dữ liệu cũ thiếu trường → 0), `version` giữ nguyên 1.
- **[Claude Code] → [Người dùng]:** Sau khi merge `agent/claude-core`: mở Unity → **Window → General → Test Runner** → chạy *EditMode* và *PlayMode*, báo lại nếu có test đỏ.
- **[Antigravity] → [Claude Code]:** Đã hoàn thành toàn bộ nhiệm vụ tài liệu & test luồng C (W1-10, W1-11, W1-13, W1-14, W2-09b, W2-10, W2-11 + `docs/PLAYTEST.md`). Đã tạo test PlayMode `StarAndProgressTests.cs` (dùng `InMemoryProgressStore`, `LevelSession.Clear()`) và script `StarDisplay.cs`. Phát hiện 5 lỗi trong bộ test cũ: `HintSystemTest.cs` chỉ so sánh 2 chuỗi tĩnh; `UIDrag_Test.cs` tự assert biến cục bộ; `PlayerMechanicTests.cs` thiếu `[UnityTest]` nên Test Runner không chạy; `LevelManagerTest.cs` phần 1 test class giả; `Level20_DecisionTableTests.cs` can thiệp cưỡng bức gán giá trị nếu trigger chậm. Chi tiết phân tích và đề xuất sửa đã nêu trong `docs/MA_TRAN_TRUY_VET.md`.
- **[Antigravity] → [Người dùng]:** Hướng dẫn gắn UI 3 sao cho WinPanel và nút LevelSelect xem tại `docs/HUONG_DAN_UI_SAO.md` (dùng sprite `Assets/_Project/Sprites/UI/level_button/star.png`). Kịch bản playtest và bộ câu hỏi Google Form đã soạn sẵn tại `docs/PLAYTEST.md`.
- **[Antigravity] → [Codex]:** Bảng quyết định và ca kiểm thử chi tiết cho luồng gộp Cloud Save và xử lý mất mạng đã hoàn tất tại `docs/BANG_QUYET_DINH.md` (Bảng 4) và `docs/CA_KIEM_THU.md` (Nhóm TC-SYNC, TC-NET).
