# KẾ HOẠCH ĐỒ ÁN CHUYÊN NGÀNH — MR. EGGY PUZZLE GAME

> **Thời gian:** 4 tuần (21/10/2026 → 17/11/2026) · **Nhân sự:** 4 thành viên
> **Repo:** https://github.com/ducthais/brain-puzzle-game · **Unity:** `6000.6.6f1` (cả nhóm dùng **đúng** phiên bản này)

---

## Mục lục
1. [Mục tiêu & định hướng đề tài](#1-mục-tiêu--định-hướng-đề-tài)
2. [Hiện trạng dự án](#2-hiện-trạng-dự-án)
3. [Phạm vi (MoSCoW)](#3-phạm-vi-moscow)
4. [Phân công vai trò](#4-phân-công-vai-trò)
5. [Lịch trình tổng quan & mốc (Milestone)](#5-lịch-trình-tổng-quan--mốc-milestone)
6. [Kế hoạch chi tiết từng tuần](#6-kế-hoạch-chi-tiết-từng-tuần)
7. [Quy trình làm việc nhóm](#7-quy-trình-làm-việc-nhóm)
8. [Kế hoạch kiểm thử](#8-kế-hoạch-kiểm-thử)
9. [Kế hoạch playtest & thu thập số liệu](#9-kế-hoạch-playtest--thu-thập-số-liệu)
10. [Rủi ro & phương án dự phòng](#10-rủi-ro--phương-án-dự-phòng)
11. [Tiêu chí hoàn thành (KPI)](#11-tiêu-chí-hoàn-thành-kpi)
12. [Cấu trúc báo cáo & phân công viết](#12-cấu-trúc-báo-cáo--phân-công-viết)
13. [Checklist trước khi nộp](#13-checklist-trước-khi-nộp)
14. [Phụ lục kỹ thuật](#14-phụ-lục-kỹ-thuật)

---

## 1. Mục tiêu & định hướng đề tài

**Tên đề tài đề xuất:**
> *Xây dựng game giải đố 2D "Mr. Eggy" trên Unity với hệ thống màn chơi mở rộng, lưu trữ đám mây và cân bằng độ khó dựa trên dữ liệu người chơi.*

**Mục tiêu cụ thể:**

| # | Mục tiêu | Cách chứng minh khi bảo vệ |
|---|----------|----------------------------|
| M1 | Mở rộng nội dung: thêm **2 chương mới (Level 21–28)**, mỗi chương 1 cơ chế chủ đạo | Demo trực tiếp, video |
| M2 | **Lưu tiến trình trên đám mây** + đăng nhập ẩn danh (thay cho chỉ dùng `PlayerPrefs`) | Gỡ app → cài lại → tiến trình vẫn còn |
| M3 | **Thu thập số liệu chơi** (chết, thời gian, dùng gợi ý, bỏ màn) | Dashboard + biểu đồ trong báo cáo |
| M4 | **Cân bằng độ khó dựa trên dữ liệu** (đo → chỉnh → đo lại) | Bảng so sánh trước/sau |
| M5 | **Chất lượng phần mềm:** tái cấu trúc kiến trúc, CI tự động chạy test & build | Badge CI xanh, báo cáo coverage |

**Câu chuyện xuyên suốt của báo cáo:**
*Game có sẵn → phát hiện vấn đề (tiến trình dễ mất, không biết màn nào quá khó, code khó mở rộng) → giải pháp (cloud save, analytics, tái cấu trúc, tool tạo màn) → kiểm chứng (test tự động + playtest có số liệu).*

---

## 2. Hiện trạng dự án

| Hạng mục | Hiện trạng | Vấn đề cần xử lý |
|----------|-----------|------------------|
| Nội dung | 21 màn (`Level 1` → `Level 21`), 6 chương, mỗi chương 4 màn | Level 21 mới là bản copy, chưa thiết kế |
| Tạo màn | Tool `Tools → Tạo Level mới` (tự tạo scene, build, hint, nút LevelSelect) | — |
| Lưu tiến trình | `PlayerPrefs` (`MaxUnlockedLevel`, `SavedLevel`, `isPremium`) | Mất khi gỡ app, dễ sửa gian lận |
| Kiến trúc | `LevelManager` ôm nhiều trách nhiệm (thắng màn, hint, ads, shop, lưu tiến trình) | Khó test, khó mở rộng |
| Quảng cáo | `AdManager` — quảng cáo **ảo** (đếm ngược) | Có thể giữ nguyên, ghi rõ trong báo cáo |
| Thanh toán | `PremiumManager` dùng Unity IAP | Chưa kiểm thử trên Google Play sandbox |
| Kiểm thử | 1 file EditMode + 9 file PlayMode (có test theo bảng quyết định) | `Tests_EditMode` **chưa có asmdef** → đang nằm trong `Assembly-CSharp`, có nguy cơ **lỗi khi build APK**; test EditMode hiện test class giả, chưa test code thật |
| CI/CD | Chưa có | — |
| Build Android | Chưa xác nhận build được | Phải kiểm tra **ngay tuần 1** |

---

## 3. Phạm vi (MoSCoW)

| Mức | Hạng mục | Người phụ trách |
|-----|----------|-----------------|
| **Must** (bắt buộc) | Build APK chạy được trên điện thoại thật | TV1 |
| **Must** | CI (GitHub Actions) tự chạy test khi tạo Pull Request | TV1 |
| **Must** | Tách `LevelManager` → `ProgressService` (+ test đơn vị) | TV1 |
| **Must** | Đăng nhập ẩn danh + Cloud Save tiến trình | TV2 |
| **Must** | Analytics: 5 sự kiện chính (xem [Phụ lục A](#a-thiết-kế-sự-kiện-analytics)) | TV2 |
| **Must** | Tối thiểu **4 màn mới** (Level 21–24, Chương 6) | TV3 |
| **Must** | Kế hoạch kiểm thử + test case + báo cáo + slide | TV4 |
| **Must** | Playtest ≥ 10 người + phân tích số liệu | TV3 + TV4 |
| **Should** (nên có) | Thêm 4 màn nữa (Level 25–28, Chương 7) | TV3 |
| **Should** | Hệ thống **3 sao** mỗi màn (qua màn / không chết / không dùng gợi ý) | TV3 |
| **Should** | Bảng xếp hạng (Leaderboard) tổng số sao | TV2 |
| **Should** | Code coverage report | TV1 |
| **Could** (có thì tốt) | Remote Config (sửa hint / mở khóa màn từ xa) | TV2 |
| **Could** | CI tự build APK và đính kèm vào GitHub Release | TV1 |
| **Won't** (không làm đợt này) | Multiplayer, iOS, quảng cáo thật, hệ thống tiền tệ trong game | — |

> **Nguyên tắc:** Hết tuần 3 **đóng băng tính năng (feature freeze)**. Tuần 4 chỉ sửa lỗi, cân bằng màn, viết báo cáo.

---

## 4. Phân công vai trò

> Nhóm tự điền tên vào cột "Thành viên".

| Mã | Vai trò | Thành viên | Trách nhiệm chính |
|----|---------|-----------|-------------------|
| **TV1** | Trưởng nhóm · Kiến trúc · CI/CD | | Quản lý repo & board, review PR, tái cấu trúc code, CI, build APK |
| **TV2** | Dịch vụ online (Backend) | | Unity Gaming Services: Authentication, Cloud Save, Analytics, Leaderboard, Remote Config |
| **TV3** | Game Design · Level Design | | Thiết kế chương/màn mới, hệ thống sao, âm thanh/hiệu ứng, tổ chức playtest |
| **TV4** | QA · Tài liệu | | Kế hoạch & ca kiểm thử, viết test, quản lý bug, tổng hợp báo cáo, slide |

**Quy tắc chung:**
- Mỗi người **tự viết test** cho phần code của mình; TV4 thiết kế ca kiểm thử và kiểm tra lại.
- Mỗi người **viết phần báo cáo** liên quan đến phần mình làm (xem [mục 12](#12-cấu-trúc-báo-cáo--phân-công-viết)), TV4 ghép và chuẩn hóa.

---

## 5. Lịch trình tổng quan & mốc (Milestone)

```
Tuần 1 (21/10–27/10)  NỀN TẢNG        ██████  → M1: CI xanh · APK chạy được · GDD chốt
Tuần 2 (28/10–03/11)  TÍNH NĂNG CHÍNH ██████  → M2: Bản Alpha (cloud save + analytics + 4 màn)
Tuần 3 (04/11–10/11)  HOÀN THIỆN      ██████  → M3: Feature freeze · Bản Beta · Playtest xong
Tuần 4 (11/11–17/11)  ĐÓNG GÓI        ██████  → M4: Bản Release v1.0 · Báo cáo · Slide · Bảo vệ
```

| Mốc | Ngày | Sản phẩm bàn giao | Điều kiện đạt |
|-----|------|-------------------|---------------|
| **M1** | 27/10 | CI chạy test, APK đầu tiên, GDD chương 6–7, kế hoạch kiểm thử | CI xanh trên `main`; APK mở được trên ≥ 1 điện thoại |
| **M2** | 03/11 | Bản Alpha `v0.5` | Đăng nhập + cloud save + 5 sự kiện analytics hoạt động; Level 21–24 chơi được |
| **M3** | 10/11 | Bản Beta `v0.9` + dữ liệu playtest | Toàn bộ tính năng Must/Should xong; ≥ 10 người đã chơi thử |
| **M4** | 17/11 | Bản Release `v1.0` + báo cáo + slide + video demo | Checklist [mục 13](#13-checklist-trước-khi-nộp) đạt 100% |

---

## 6. Kế hoạch chi tiết từng tuần

Ký hiệu: **ước lượng** tính theo giờ công · **Phụ thuộc** = phải xong việc kia trước.

### Tuần 1 (21/10 – 27/10): NỀN TẢNG

**Mục tiêu:** Mọi người làm việc được song song mà không giẫm chân nhau; xác nhận build được Android.

| ID | Công việc | Người | Ước lượng | Phụ thuộc | Kết quả bàn giao |
|----|-----------|-------|-----------|-----------|------------------|
| W1-01 | Họp khởi động: chốt đề tài, phạm vi, phân công, tạo GitHub Project board | Cả nhóm | 2h | — | Board có đủ task tuần 1–2 |
| W1-02 | **Build thử APK Android** ngay ngày 1–2, ghi lại lỗi (nếu có) | TV1 | 4h | — | APK + danh sách lỗi build |
| W1-03 | Tạo asmdef cho `Tests_EditMode` (chỉ chạy trong Editor), sửa lỗi build nếu có | TV1 | 3h | W1-02 | Build APK không còn lỗi do test |
| W1-04 | Cấu hình GitHub Actions + GameCI chạy EditMode & PlayMode test khi có PR (xem [Phụ lục C](#c-mẫu-cấu-hình-ci-github-actions--gameci)) | TV1 | 8h | — | Workflow `tests.yml`, badge trong README |
| W1-05 | Bật bảo vệ nhánh `main` (bắt buộc PR + 1 review + CI xanh) | TV1 | 1h | W1-04 | — |
| W1-06 | Thiết kế lại kiến trúc: vẽ class diagram **hiện tại** và **mục tiêu** (tách `ProgressService`, `HintService`, `WinFlow`) | TV1 | 4h | — | 2 sơ đồ lớp (draw.io / PlantUML) |
| W1-07 | Tạo project trên Unity Cloud Dashboard, liên kết project ID, cài package: Authentication, Cloud Save, Analytics, Leaderboards | TV2 | 4h | — | Project build được với các package mới |
| W1-08 | Prototype đăng nhập ẩn danh (anonymous sign-in) khi mở game, hiển thị Player ID trong log | TV2 | 4h | W1-07 | Scene/StartScreen đăng nhập thành công |
| W1-09 | Thiết kế schema dữ liệu Cloud Save & sự kiện Analytics (xem [Phụ lục A, B](#14-phụ-lục-kỹ-thuật)) | TV2 | 3h | — | Tài liệu schema (đưa vào báo cáo chương 3) |
| W1-10 | Viết **GDD (Game Design Document)** cho Chương 6 & 7: cơ chế chủ đạo, phác thảo 8 màn, đường cong độ khó | TV3 | 6h | — | `docs/GDD_Chuong6_7.md` + phác thảo |
| W1-11 | Định nghĩa luật **3 sao** (qua màn / không chết / không xem hint) — viết thành bảng quyết định | TV3 + TV4 | 2h | — | Bảng quyết định luật sao |
| W1-12 | Dựng Level 21 & 22 bằng tool `Tools → Tạo Level mới` (bản thô, chơi được) | TV3 | 8h | W1-10 | 2 màn chơi được |
| W1-13 | Viết **Kế hoạch kiểm thử** (phạm vi, chiến lược, môi trường, tiêu chí đạt) | TV4 | 4h | — | `docs/KE_HOACH_KIEM_THU.md` |
| W1-14 | Rà soát 10 file test hiện có: liệt kê test nào đang pass/fail, test nào test code giả → lập bảng truy vết yêu cầu ↔ test | TV4 | 4h | — | Ma trận truy vết (traceability matrix) |
| W1-15 | Dựng khung báo cáo (template theo yêu cầu khoa), viết Chương 1 bản nháp, vẽ Use Case tổng quát | TV4 | 6h | — | File báo cáo khung |

**Cuối tuần 1 — Demo nội bộ (27/10):** CI chạy, APK trên điện thoại, đăng nhập ẩn danh in ra Player ID, Level 21–22 chơi được.

---

### Tuần 2 (28/10 – 03/11): TÍNH NĂNG CHÍNH

**Mục tiêu:** Bản Alpha có đủ các tính năng Must.

| ID | Công việc | Người | Ước lượng | Phụ thuộc | Kết quả bàn giao |
|----|-----------|-------|-----------|-----------|------------------|
| W2-01 | Tạo `ProgressService` (interface `IProgressStore` + `LocalProgressStore` dùng PlayerPrefs); chuyển logic lưu tiến trình ra khỏi `LevelManager` và `LevelSelectManager` | TV1 | 8h | W1-06 | Code mới, hành vi game không đổi |
| W2-02 | Viết test đơn vị cho `ProgressService` (mở khóa màn, màn cuối, dữ liệu hỏng…) | TV1 | 4h | W2-01 | ≥ 8 test EditMode |
| W2-03 | Tách `HintService` khỏi `LevelManager` (giữ nguyên tham chiếu Inspector) | TV1 | 4h | W2-01 | — |
| W2-04 | Review & merge PR của cả nhóm (mỗi ngày) | TV1 | 4h | — | — |
| W2-05 | `CloudProgressStore`: đồng bộ tiến trình lên Cloud Save; **luật gộp** khi local ≠ cloud: lấy **giá trị lớn hơn** cho màn mở khóa và số sao | TV2 | 10h | W2-01, W1-08 | Gỡ app → cài lại → tiến trình còn |
| W2-06 | Xử lý **mất mạng**: vẫn chơi bằng dữ liệu local, đồng bộ lại khi có mạng | TV2 | 4h | W2-05 | — |
| W2-07 | Gửi 5 sự kiện Analytics: `level_start`, `level_complete`, `player_death`, `hint_used`, `level_skip` + popup xin đồng ý thu thập dữ liệu | TV2 | 6h | W1-09 | Sự kiện hiện trên dashboard |
| W2-08 | Dựng Level 23 & 24 (màn kết chương, nút tím) | TV3 | 8h | W1-10 | Chương 6 hoàn chỉnh (4 màn) |
| W2-09 | Hệ thống 3 sao: tính sao khi thắng, hiện sao trên `WinCanvas` và trên nút ở `LevelSelect` | TV3 | 8h | W1-11, W2-01 | Sao lưu được qua `ProgressService` |
| W2-10 | Viết ca kiểm thử (bảng quyết định / phân vùng tương đương / giá trị biên) cho: luật sao, luật gộp cloud, mở khóa màn | TV4 | 6h | W1-11 | Bộ test case dạng bảng |
| W2-11 | Cài đặt test PlayMode cho luật sao & mở khóa màn | TV4 | 6h | W2-09, W2-10 | ≥ 6 test mới |
| W2-12 | Báo cáo: Chương 2 (cơ sở lý thuyết) + sơ đồ tuần tự "Thắng màn" và "Đồng bộ cloud" | TV4 (+TV2 cung cấp nội dung) | 6h | — | — |

**Cuối tuần 2 — Mốc M2 / Bản Alpha `v0.5` (03/11):** tag `v0.5` trên GitHub, APK đính kèm.

---

### Tuần 3 (04/11 – 10/11): HOÀN THIỆN & PLAYTEST

**Mục tiêu:** Xong toàn bộ Must + Should, có dữ liệu từ người chơi thật. **Feature freeze tối 10/11.**

| ID | Công việc | Người | Ước lượng | Phụ thuộc | Kết quả bàn giao |
|----|-----------|-------|-----------|-----------|------------------|
| W3-01 | Bật **code coverage** (package Code Coverage), xuất báo cáo HTML | TV1 | 3h | — | Ảnh chụp % coverage cho báo cáo |
| W3-02 | Đo hiệu năng trên điện thoại (Profiler: FPS, bộ nhớ, thời gian load scene) | TV1 | 4h | — | Bảng số liệu hiệu năng |
| W3-03 | (Could) CI tự build APK khi push tag `v*` | TV1 | 4h | W1-04 | APK trong GitHub Release |
| W3-04 | Sửa lỗi theo danh sách bug (ưu tiên Critical/High) | TV1 + TV2 | 8h | — | — |
| W3-05 | **Leaderboard** tổng số sao + màn hình bảng xếp hạng | TV2 | 8h | W2-09 | — |
| W3-06 | (Could) Remote Config: nội dung hint, cờ "mở khóa tất cả" để demo | TV2 | 4h | — | — |
| W3-07 | Dựng Level 25–28 (Chương 7) | TV3 | 14h | — | 28 màn |
| W3-08 | Hiệu ứng / âm thanh khi được sao, polish UI | TV3 | 4h | W2-09 | — |
| W3-09 | **Tổ chức playtest** (06/11–09/11): ≥ 10 người, phát APK bản Beta, form khảo sát (xem [mục 9](#9-kế-hoạch-playtest--thu-thập-số-liệu)) | TV3 + TV4 | 6h | Bản Beta | Dữ liệu analytics + form |
| W3-10 | Kiểm thử hồi quy toàn bộ (chạy toàn bộ test case thủ công + tự động), ghi bug lên GitHub Issues | TV4 | 8h | — | Báo cáo kết quả kiểm thử lần 1 |
| W3-11 | Báo cáo: Chương 3 (phân tích thiết kế) — use case chi tiết, class diagram, sequence, state diagram màn chơi, schema dữ liệu | TV4 (+TV1, TV2) | 8h | — | — |

**Cuối tuần 3 — Mốc M3 / Bản Beta `v0.9` (10/11):** đóng băng tính năng, tag `v0.9`.

---

### Tuần 4 (11/11 – 17/11): CÂN BẰNG · ĐÓNG GÓI · BẢO VỆ

**Mục tiêu:** Không thêm tính năng. Chỉ sửa lỗi, cân bằng màn theo số liệu, hoàn thiện báo cáo & slide.

| ID | Công việc | Người | Ước lượng | Phụ thuộc | Kết quả bàn giao |
|----|-----------|-------|-----------|-----------|------------------|
| W4-01 | Phân tích số liệu playtest: tỉ lệ bỏ màn, số lần chết trung bình, thời gian trung bình mỗi màn → vẽ biểu đồ | TV2 + TV4 | 6h | W3-09 | Biểu đồ + nhận xét |
| W4-02 | **Cân bằng độ khó** các màn "nút thắt" (tỉ lệ bỏ cao), cập nhật hint | TV3 | 8h | W4-01 | Bảng "trước / sau khi chỉnh" |
| W4-03 | Playtest lần 2 (nhỏ, 5 người) để đo lại các màn đã chỉnh | TV3 | 3h | W4-02 | Số liệu so sánh |
| W4-04 | Sửa lỗi cuối, build **Release `v1.0`**, cập nhật README (cách chạy, cách build, kiến trúc) | TV1 | 6h | — | Tag `v1.0` + APK |
| W4-05 | Kiểm thử hồi quy lần 2 trên ≥ 2 điện thoại Android khác nhau | TV4 | 4h | W4-04 | Báo cáo kết quả kiểm thử cuối |
| W4-06 | Báo cáo: Chương 4 (cài đặt), Chương 5 (kiểm thử & đánh giá), Chương 6 (kết luận) | Mỗi người viết phần mình, TV4 ghép | 12h | — | Báo cáo hoàn chỉnh |
| W4-07 | Slide bảo vệ (15–20 slide) + **video demo 3–5 phút** (phòng khi demo trực tiếp lỗi) | TV3 + TV4 | 6h | W4-04 | Slide + video |
| W4-08 | **Tập dượt bảo vệ** 2 lần, chuẩn bị câu hỏi phản biện (xem [Phụ lục E](#e-câu-hỏi-phản-biện-thường-gặp)) | Cả nhóm | 3h | W4-07 | — |
| W4-09 | Rà checklist nộp bài [mục 13](#13-checklist-trước-khi-nộp) | TV1 | 1h | — | — |

---

## 7. Quy trình làm việc nhóm

### 7.1 Họp & giao tiếp
| Hoạt động | Thời gian | Nội dung |
|-----------|-----------|----------|
| Stand-up online (nhắn tin) | Hằng ngày, trước 22h | 3 câu: *Hôm qua làm gì? Hôm nay làm gì? Có bị vướng gì?* |
| Họp nhóm | Thứ 2 & Thứ 6 (30 phút) | Cập nhật tiến độ, gỡ vướng, phân việc tiếp |
| Demo cuối tuần | Thứ 3 (cuối mỗi tuần dự án) | Chạy bản build mới nhất trên điện thoại, chốt mốc |

### 7.2 Quản lý công việc
- Dùng **GitHub Projects** (board kiểu Kanban): `Todo` → `In Progress` → `Review` → `Done`.
- Mỗi task trong file này = 1 issue, đặt tên theo ID (ví dụ `W2-05 CloudProgressStore`), gán người và nhãn (`feature`, `bug`, `test`, `docs`, `level`).

### 7.3 Git workflow
- `main` **luôn chạy được**, được bảo vệ: chỉ merge qua Pull Request, cần **1 review + CI xanh**.
- Đặt tên nhánh: `feat/<ten>`, `fix/<ten>`, `level/<so-man>`, `docs/<ten>`, `test/<ten>`.
- Commit message: `<loại>: <mô tả ngắn>` — ví dụ `feat: đồng bộ tiến trình lên Cloud Save`, `level: thiết kế Level 23`.
- PR nhỏ, merge thường xuyên (≤ 2 ngày/nhánh) để tránh xung đột.

### 7.4 Quy tắc tránh xung đột trong Unity (rất quan trọng)
File `.unity` (scene) và `.prefab` **rất khó gộp** khi 2 người cùng sửa.

| Tài nguyên | Ai được sửa |
|------------|-------------|
| `LevelSelect.unity` | Chỉ **1 người tại một thời điểm** — báo trong nhóm trước khi sửa ("đang khóa LevelSelect") |
| `Level N.unity` | Người được giao màn đó |
| Prefab dùng chung (`LevelManager`, `WinCanvas`, `HUD`…) | Báo nhóm trước khi sửa |
| `GameHintData.asset`, `ProjectSettings/*` | Báo nhóm trước khi sửa |

- Luôn `git pull` **trước khi mở Unity** và trước khi bắt đầu sửa scene.
- Tạo màn mới **chỉ bằng tool** `Tools → Tạo Level mới` (tool tự sửa LevelSelect, hint, build settings) — mỗi lần chỉ 1 người chạy tool.
- Bật Smart Merge của Unity cho Git (hướng dẫn: tìm "UnityYAMLMerge git" trong Unity Manual).

### 7.5 Định nghĩa "Hoàn thành" (Definition of Done) cho một task
- [ ] Code chạy được trong Editor và trên APK
- [ ] Có test (tự động hoặc ca kiểm thử thủ công) cho phần mới
- [ ] CI xanh, đã được 1 người review
- [ ] Không còn lỗi đỏ / cảnh báo mới trong Console
- [ ] Đã cập nhật tài liệu / ghi chú cho phần báo cáo liên quan

---

## 8. Kế hoạch kiểm thử

| Cấp độ | Công cụ | Phạm vi | Người |
|--------|---------|---------|-------|
| Unit test (EditMode) | Unity Test Framework (NUnit) | `ProgressService`, luật sao, luật gộp cloud, parse tên màn | TV1, TV2 |
| Integration / PlayMode test | Unity Test Framework | Cơ chế màn (cờ, hộp, nút ảo), luồng thắng màn, hint, pause, premium | TV3, TV4 |
| Kiểm thử hộp đen | Bảng quyết định, phân vùng tương đương, giá trị biên | Mở khóa màn, tính sao, mua Premium, đồng bộ cloud khi mất mạng | TV4 |
| Kiểm thử hồi quy | CI (tự động) + checklist thủ công | Toàn bộ game | TV4 |
| Kiểm thử thiết bị | ≥ 2 điện thoại Android khác độ phân giải | Giao diện (SafeArea, thanh chương), hiệu năng | TV4 |
| Kiểm thử chấp nhận | Playtest người dùng thật | Độ khó, trải nghiệm | TV3 |

**Mẫu ca kiểm thử (Test Case):**

| ID | Mô tả | Điều kiện đầu | Các bước | Kết quả mong đợi | Kết quả thực tế | Trạng thái |
|----|-------|---------------|----------|------------------|-----------------|-----------|
| TC-STAR-01 | Qua màn, không chết, không xem hint | Vào Level 5 lần đầu | Chơi và chạm cờ | Nhận 3 sao | | |
| TC-SYNC-03 | Mất mạng khi thắng màn | Tắt Wi-Fi/4G | Thắng Level 3 → bật mạng → mở lại game | Level 4 mở khóa trên cloud | | |

**Mức độ lỗi (bug):** `Critical` (crash, mất dữ liệu) · `High` (không qua được màn) · `Medium` (sai giao diện/logic phụ) · `Low` (thẩm mỹ).
**Tiêu chí phát hành v1.0:** 0 lỗi Critical/High còn mở; 100% test tự động pass.

---

## 9. Kế hoạch playtest & thu thập số liệu

**Đối tượng:** ≥ 10 người (bạn cùng lớp, người thân), chưa từng chơi game.

**Quy trình:**
1. Gửi APK bản Beta + hướng dẫn cài đặt (cho phép cài từ nguồn ngoài).
2. Người chơi chơi **tự do 20–30 phút**, không được nhóm gợi ý.
3. Điền **Google Form** sau khi chơi.
4. Dữ liệu analytics tự gửi về dashboard.

**Câu hỏi khảo sát (thang 1–5 trừ khi ghi khác):**
1. Mức độ vui / hấp dẫn của game
2. Độ khó tổng thể (1 = quá dễ, 5 = quá khó)
3. Màn nào khó nhất? Vì sao? *(trả lời tự do)*
4. Gợi ý (hint) có hữu ích không?
5. Giao diện có dễ hiểu không? (thanh chương, nút điều khiển)
6. Bạn có muốn chơi tiếp các chương sau không?
7. Góp ý khác *(trả lời tự do)*

**Chỉ số phân tích cho mỗi màn:**

| Chỉ số | Công thức | Ngưỡng cần chỉnh |
|--------|-----------|------------------|
| Tỉ lệ hoàn thành | số `level_complete` / số người `level_start` | < 60% |
| Số lần chết trung bình | tổng `player_death` / số người chơi màn | > 10 |
| Thời gian trung bình qua màn | trung bình `duration_sec` của `level_complete` | > 5 phút |
| Tỉ lệ dùng hint | số `hint_used` / số người chơi màn | > 70% |
| Tỉ lệ bỏ qua màn | số `level_skip` / số người chơi màn | > 30% |

---

## 10. Rủi ro & phương án dự phòng

| # | Rủi ro | Khả năng | Ảnh hưởng | Phòng ngừa / Dự phòng |
|---|--------|----------|-----------|-----------------------|
| R1 | Không build được APK (lỗi asmdef, SDK Android, package) | Trung bình | Cao | Build thử **ngày 1–2 tuần 1** (W1-02); cài sẵn Android module trên ≥ 2 máy |
| R2 | GameCI chưa có Docker image cho Unity `6000.6.6f1` / lỗi kích hoạt license | Trung bình | Trung bình | Kiểm tra image trên Docker Hub (`unityci/editor`) ngay tuần 1; nếu không được → chạy test local, chụp kết quả, ghi rõ trong báo cáo |
| R3 | Xung đột scene/prefab khi merge | Cao | Trung bình | Quy tắc [mục 7.4](#74-quy-tắc-tránh-xung-đột-trong-unity-rất-quan-trọng); PR nhỏ, merge thường xuyên |
| R4 | Cấu hình Unity Gaming Services mất nhiều thời gian | Trung bình | Cao | TV2 bắt đầu từ ngày 1; giữ `LocalProgressStore` làm phương án dự phòng (game vẫn chạy offline) |
| R5 | Không đủ người playtest | Thấp | Cao | Hẹn người chơi từ tuần 2; có thể playtest trực tiếp trên lớp |
| R6 | Thành viên bận thi/ốm | Trung bình | Trung bình | Mỗi phần có 1 người "dự bị" nắm được: TV1↔TV2, TV3↔TV4 |
| R7 | Tái cấu trúc làm hỏng màn cũ | Trung bình | Cao | Chạy toàn bộ test PlayMode trước/sau; tách nhỏ PR; không đổi tên field public đang được gán trong Inspector |
| R8 | Ôm quá nhiều tính năng | Cao | Cao | Tuân thủ MoSCoW; feature freeze 10/11; bỏ các mục *Could* trước |
| R9 | Các máy dùng khác phiên bản Unity | Trung bình | Cao | Bắt buộc `6000.6.6f1`; không commit nếu `ProjectVersion.txt` bị đổi |
| R10 | Thiếu dung lượng Git LFS | Thấp | Trung bình | Tránh commit file ảnh/âm thanh nặng không dùng; kiểm tra dung lượng LFS trên GitHub |

---

## 11. Tiêu chí hoàn thành (KPI)

| Chỉ số | Mục tiêu tối thiểu | Mục tiêu tốt |
|--------|-------------------|--------------|
| Số màn chơi | 24 | 28 |
| Số test tự động | ≥ 30, pass 100% | ≥ 45, pass 100% |
| Code coverage (assembly `MainGameScripts`) | ≥ 30% | ≥ 50% |
| Số người playtest | 10 | 15+ |
| Lỗi Critical/High còn mở khi nộp | 0 | 0 |
| Số thiết bị Android đã kiểm thử | 2 | 3+ |
| FPS trên điện thoại tầm trung | ≥ 30 | ≥ 60 |
| Điểm hài lòng trung bình (form) | ≥ 3.5/5 | ≥ 4/5 |

---

## 12. Cấu trúc báo cáo & phân công viết

| Chương | Nội dung | Người viết chính | Hạn nháp |
|--------|----------|------------------|----------|
| Mở đầu | Lý do chọn đề tài, mục tiêu, phạm vi, phương pháp | TV4 | 27/10 |
| **1. Tổng quan** | Thể loại puzzle "troll", khảo sát game tương tự (Brain Out, Troll Face Quest…), giới thiệu Mr. Eggy | TV4 + TV3 | 27/10 |
| **2. Cơ sở lý thuyết** | Unity (vòng đời MonoBehaviour, ScriptableObject, Physics 2D, UI), Unity Gaming Services, kiểm thử phần mềm (bảng quyết định…), CI/CD | TV4 + TV2 + TV1 | 03/11 |
| **3. Phân tích & thiết kế** | Yêu cầu chức năng/phi chức năng, Use Case, Class diagram (trước/sau tái cấu trúc), Sequence (thắng màn, đồng bộ cloud, mua Premium), State diagram (vòng đời màn chơi), thiết kế dữ liệu, GDD | TV1 + TV2 + TV3 | 10/11 |
| **4. Cài đặt** | Kiến trúc thư mục, các cơ chế chính, tool tạo màn, Cloud Save/Analytics, hệ thống sao, CI | Mỗi người phần mình | 13/11 |
| **5. Kiểm thử & đánh giá** | Kế hoạch kiểm thử, test case, kết quả test tự động + coverage, kết quả playtest, biểu đồ & cân bằng độ khó trước/sau, hiệu năng | TV4 + TV3 | 15/11 |
| **6. Kết luận** | Kết quả đạt được (đối chiếu KPI), hạn chế, hướng phát triển | TV1 | 15/11 |
| Phụ lục | Hướng dẫn cài đặt & build, tài liệu tham khảo | TV1 | 16/11 |

---

## 13. Checklist trước khi nộp

**Sản phẩm**
- [ ] Tag `v1.0` trên GitHub, APK đính kèm trong Release
- [ ] APK cài và chạy được trên ≥ 2 điện thoại
- [ ] Chơi được từ Level 1 → màn cuối, không lỗi chặn
- [ ] Đã **xóa tiến trình test** (Tools → Test → Xóa tiến trình chơi) trước khi build
- [ ] Tắt các log debug không cần thiết

**Mã nguồn**
- [ ] `main` CI xanh, 100% test pass
- [ ] README: giới thiệu, ảnh chụp, cách cài Unity/Git LFS, cách chạy, cách build, cách tạo màn mới
- [ ] Không commit file thừa (`Library/`, `Temp/`, file build)

**Tài liệu**
- [ ] Báo cáo đúng mẫu của khoa, có mục lục, danh mục hình/bảng, tài liệu tham khảo
- [ ] Mọi sơ đồ rõ nét, có chú thích
- [ ] Slide 15–20 trang + video demo 3–5 phút
- [ ] Đã tập dượt bảo vệ ≥ 2 lần, mỗi người nắm được phần của người khác

---

## 14. Phụ lục kỹ thuật

### A. Thiết kế sự kiện Analytics

> Cần khai báo các sự kiện custom này trên Unity Cloud Dashboard (Event Manager) trước khi gửi.

| Sự kiện | Khi nào gửi | Tham số |
|---------|-------------|---------|
| `level_start` | Scene `Level N` được load | `level_id` (int), `attempt` (int) |
| `level_complete` | `LevelManager.WinGame()` | `level_id`, `duration_sec` (float), `deaths` (int), `hints_used` (int), `stars` (int) |
| `player_death` | `PlayerDeath` kích hoạt | `level_id`, `time_in_level_sec` (float) |
| `hint_used` | Mở bảng gợi ý (`OpenHintCanvas2`) | `level_id`, `via_ad` (bool) |
| `level_skip` | Bấm "Nhảy" màn (`OnClickNhay_Canvas2`) | `level_id`, `time_in_level_sec` |

### B. Thiết kế dữ liệu Cloud Save

Khóa `progress` — JSON của `ProgressData` (serialize bằng `JsonUtility`, xem `Scripts/Core/ProgressData.cs`):

```json
{
  "version": 1,
  "maxUnlockedLevel": 12,
  "savedLevel": "Level 12",
  "stars": [ { "levelId": 1, "stars": 3 }, { "levelId": 2, "stars": 2 } ],
  "updatedAtUtc": "2026-11-02T10:15:00.0000000Z",
  "resetCount": 0
}
```

- `stars` là **danh sách** (JsonUtility không serialize được Dictionary).
- **Không có `isPremium`**: Premium do `PremiumManager` quản lý, Google Play tự khôi phục giao dịch non-consumable → tránh dữ liệu tiến trình cũ ghi đè trạng thái vừa mua.

**Luật gộp khi dữ liệu local và cloud khác nhau** (`ProgressMerger`):
- `resetCount` khác nhau → lấy **nguyên** bên có `resetCount` lớn hơn (bên đã "New Game" sau) — tránh tiến trình cũ quay lại sau khi chơi lại từ đầu.
- `resetCount` bằng nhau:
  - `maxUnlockedLevel` = max(local, cloud)
  - `stars[level]` = max(local, cloud) cho từng màn
  - `savedLevel` theo bên có `maxUnlockedLevel` lớn hơn (hòa → local)

→ Không bao giờ làm mất tiến trình của người chơi (trừ khi chính họ chọn New Game). Bảng quyết định chi tiết: `docs/BANG_QUYET_DINH.md`.

### C. Mẫu cấu hình CI (GitHub Actions + GameCI)

`.github/workflows/tests.yml` (khung tham khảo — cần thêm secrets `UNITY_LICENSE` hoặc `UNITY_EMAIL` + `UNITY_PASSWORD` theo hướng dẫn kích hoạt license của GameCI):

```yaml
name: Unity Tests
on:
  pull_request:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          lfs: true
      - uses: actions/cache@v4
        with:
          path: Library
          key: Library-${{ hashFiles('Assets/**', 'Packages/**', 'ProjectSettings/**') }}
          restore-keys: Library-
      - uses: game-ci/unity-test-runner@v4
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
          UNITY_EMAIL: ${{ secrets.UNITY_EMAIL }}
          UNITY_PASSWORD: ${{ secrets.UNITY_PASSWORD }}
        with:
          testMode: all
          githubToken: ${{ secrets.GITHUB_TOKEN }}
      - uses: actions/upload-artifact@v4
        if: always()
        with:
          name: Test results
          path: artifacts
```

### D. Kiến trúc mục tiêu sau tái cấu trúc

```
LevelManager (điều phối màn chơi)
├── ProgressService ──► IProgressStore
│                        ├── LocalProgressStore  (PlayerPrefs)
│                        └── CloudProgressStore  (Unity Cloud Save)
├── HintService      ──► LevelDatabase (ScriptableObject)
├── StarCalculator   (luật 3 sao — thuần C#, dễ test)
└── AnalyticsReporter (gửi sự kiện, tắt được khi chạy test)
```

Nguyên tắc: logic thuần (tính sao, gộp dữ liệu, mở khóa màn) viết thành **class C# thường**, không phụ thuộc `MonoBehaviour` → test EditMode nhanh và dễ.

### E. Câu hỏi phản biện thường gặp

1. Vì sao chọn Unity Gaming Services thay vì Firebase / tự viết backend?
2. Nếu người chơi chơi trên 2 máy cùng lúc thì dữ liệu gộp thế nào?
3. Dữ liệu người chơi thu thập những gì? Có xin phép không?
4. Làm sao chứng minh màn chơi đã được cân bằng tốt hơn?
5. Test tự động bao phủ bao nhiêu phần trăm? Phần nào chưa test được, vì sao?
6. Tool tạo màn hoạt động thế nào? Vì sao không làm màn bằng dữ liệu (JSON) thay vì scene?
7. Quy trình làm việc nhóm, xử lý xung đột Git với file scene như thế nào?
8. Hạn chế lớn nhất của sản phẩm và hướng phát triển tiếp theo?

---

*Cập nhật lần cuối: 21/10/2026 — Mọi thay đổi kế hoạch cần được thống nhất trong buổi họp nhóm và ghi lại vào file này.*
