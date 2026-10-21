# Mr. Eggy — Game giải đố 2D trên Unity

[![Unity Tests](https://github.com/ducthais/brain-puzzle-game/actions/workflows/tests.yml/badge.svg)](https://github.com/ducthais/brain-puzzle-game/actions/workflows/tests.yml)

Game giải đố 2D thể loại "troll": đưa nhân vật Mr. Eggy tới lá cờ vàng bằng cách di chuyển, nhảy và **tương tác với chính giao diện** — kéo thả mặt đất, lá cờ, nút điều khiển, nhân bản nhân vật làm cầu...
Đồ án chuyên ngành IT6122 — Trường CNTT & TT, Đại học Công nghiệp Hà Nội.

- **Nền tảng:** Android (thử nghiệm trong Unity Editor)
- **Engine:** Unity `6000.6.6f1` (Unity 6) · C#
- **Nội dung:** 21+ màn, chia chương (4 màn/chương), hệ thống gợi ý, 3 sao mỗi màn, Premium (thử nghiệm)

## Yêu cầu

| Công cụ | Ghi chú |
|---------|---------|
| [Unity Hub](https://unity.com/download) + Unity **6000.6.6f1** | Bắt buộc đúng phiên bản; tích module **Android Build Support** nếu cần build APK |
| [Git](https://git-scm.com/) + [Git LFS](https://git-lfs.com/) | Ảnh, âm thanh, model lưu bằng Git LFS — thiếu LFS thì Unity không mở được asset |

## Cài đặt & chạy

```bash
git lfs install
git clone https://github.com/ducthais/brain-puzzle-game.git
```

1. Unity Hub → **Projects → Add → Add project from disk** → chọn thư mục vừa clone (lần đầu import mất vài phút).
2. Mở scene `Assets/_Project/Scenes/StartScreen.unity` → bấm **▶ Play**.
3. Muốn thử nhanh 1 màn: mở thẳng `Assets/_Project/Scenes/Level N.unity` → **▶ Play** (không cần mở khóa).

## Build APK

- **Trong Unity:** File → Build Profiles → Android → Switch Platform → Build.
- **Tự động (CI):** push tag `v*` (VD `git tag v1.0 && git push origin v1.0`) → workflow `Build Android` build APK và đính kèm vào GitHub Release.

## Công cụ trong Unity Editor (menu **Tools**)

| Menu | Chức năng |
|------|-----------|
| **Tools → Tạo Level mới** | Copy scene mẫu thành `Level N`, thêm vào Build Profiles, nối `nextLevelName`, thêm gợi ý, tạo nút màn (và chương mới) trong LevelSelect |
| **Tools → Sửa thanh Chương (cuộn ngang)** | Chuyển thanh chương ở LevelSelect sang dạng cuộn ngang khớp khung nền |
| **Tools → Test → Mở khóa tất cả Level** | Mở khóa mọi màn để test |
| **Tools → Test → Xóa tiến trình chơi** | Về trạng thái chỉ mở Level 1, xóa sao |

> Các lệnh sửa scene không chạy được khi đang ở Play mode.

## Kiểm thử

- Unity → **Window → General → Test Runner** → tab **EditMode** / **PlayMode** → **Run All**.
- `Assets/_Project/Tests_EditMode/` — unit test logic thuần (tiến trình, luật 3 sao, gộp dữ liệu cloud...).
- `Assets/_Project/Tests_PlayMode/` — test cơ chế gameplay, UI, Premium.
- CI (`.github/workflows/tests.yml`) chạy toàn bộ test + đo coverage mỗi Pull Request. Cần secrets `UNITY_EMAIL` + `UNITY_PASSWORD` (hoặc `UNITY_LICENSE`) — xem [GameCI activation](https://game.ci/docs/github/activation).

## Cấu trúc thư mục

```
Assets/_Project/
├── Scenes/            StartScreen, LevelSelect, Level 1 … Level N
├── Scripts/           Gameplay & UI (MonoBehaviour) — assembly MainGameScripts
│   ├── Core/          Logic C# thuần: ProgressData, StarCalculator, ProgressMerger, LevelSession...
│   ├── Services/      ProgressService, HintService, GameAnalytics, LocalProgressStore
│   └── Editor/        Tool Editor (Tạo Level mới, công cụ test)
├── GameData/          GameHintData.asset — gợi ý theo màn
├── Prefabs/ Sprites/ Sounds/ Animations/
├── Tests_EditMode/    Unit test
└── Tests_PlayMode/    Test PlayMode
docs/                  Kế hoạch, kiến trúc, kiểm thử, báo cáo
```

Kiến trúc chi tiết (sơ đồ lớp, tuần tự, trạng thái): [`docs/KIEN_TRUC.md`](docs/KIEN_TRUC.md).

## Thêm màn chơi mới

1. **Tools → Tạo Level mới** → chọn scene mẫu, số màn, nhập gợi ý → **Tạo**.
2. Mở scene `Level N` vừa tạo, xóa/sửa vật thể của màn mẫu và thiết kế bản đồ mới.
3. Kiểm tra: **Tools → Test → Mở khóa tất cả Level** → Play từ `StartScreen`.

## Tài liệu

| Tài liệu | Nội dung |
|----------|----------|
| [`docs/KE_HOACH_DO_AN.md`](docs/KE_HOACH_DO_AN.md) | Kế hoạch 4 tuần, phạm vi, rủi ro, KPI |
| [`docs/TASK_BOARD.md`](docs/TASK_BOARD.md) | Bảng phân việc & trạng thái |
| [`docs/KIEN_TRUC.md`](docs/KIEN_TRUC.md) | Kiến trúc mã nguồn |
| [`AGENTS.md`](AGENTS.md) | Quy ước code & phối hợp |

## Nhóm thực hiện

Nhóm 1 — Lớp 20261IT6122001 · GVHD: TS. Nguyễn Bá Nghiễn
