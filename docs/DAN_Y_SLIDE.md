# DÀN Ý SLIDE BẢO VỆ ĐỒ ÁN & KỊCH BẢN VIDEO DEMO (DEFENSE SLIDES & DEMO SCRIPT)

> **Dự án:** Game giải đố 2D Mr. Eggy — Đồ án chuyên ngành IT6122 (Unity 6000.6.6f1, C#)  
> **Tài liệu bàn giao:** Task W4-07 (Agent Antigravity phụ trách luồng C)  
> **Áp dụng cho:** Buổi bảo vệ đồ án chuyên ngành trước Hội đồng chấm thi  
> **Tài liệu tham chiếu:** [`docs/KE_HOACH_DO_AN.md`](KE_HOACH_DO_AN.md) · [`docs/KIEN_TRUC.md`](KIEN_TRUC.md) · [`docs/BANG_QUYET_DINH.md`](BANG_QUYET_DINH.md) · [`docs/MA_TRAN_TRUY_VET.md`](MA_TRAN_TRUY_VET.md) · [`docs/PHAN_TICH_PLAYTEST.md`](PHAN_TICH_PLAYTEST.md) · [`docs/CHECKLIST_THIET_BI.md`](CHECKLIST_THIET_BI.md)

---

## 1. Thông số Chung Buổi Bảo vệ (Defense Presentation Overview)

- **Tổng thời lượng trình bày:** **15 phút** thuyết trình + **3 phút** chiếu video demo + **10–15 phút** Hội đồng hỏi đáp (Q&A).
- **Quy mô slide:** **18 – 20 slide**, thiết kế theo phong cách hiện đại, trực quan, cô đọng chữ, nhấn mạnh vào sơ đồ kiến trúc, số liệu thực chứng và biểu đồ phân tích.
- **Phân bổ thời gian theo cấu trúc:**
  - *Mở đầu & Đặt vấn đề:* 2 phút (Slide 1–3)
  - *Kiến trúc & Kỹ thuật lõi:* 5 phút (Slide 4–9)
  - *Đảm bảo chất lượng & Kiểm thử:* 3.5 phút (Slide 10–12)
  - *Dữ liệu Playtest & Cân bằng:* 3.5 phút (Slide 13–15)
  - *Hiệu năng thiết bị & Tổng kết:* 1 phút (Slide 16, 18)
  - *Chiếu Video Demo:* 3 phút (Slide 17)

---

## 2. Dàn ý Chi tiết Từng Slide (Slide-by-Slide Outline)

### Slide 1: Trang Tiêu đề (Title Slide)
- **Tiêu đề lớn:** BẢO VỆ ĐỒ ÁN CHUYÊN NGÀNH CÔNG NGHỆ THÔNG TIN (IT6122)
- **Tên đề tài:** *Phát triển Game Giải đố 2D "Mr. Eggy" trên Unity: Tái cấu trúc Kiến trúc, Tích hợp Dịch vụ Đám mây và Cân bằng Độ khó dựa trên Dữ liệu Người chơi.*
- **Giảng viên hướng dẫn:** [Họ và tên Thầy/Cô]
- **Nhóm sinh viên thực hiện:**
  - TV1: [Họ tên] — Trưởng nhóm · Kiến trúc · CI/CD (Luồng A)
  - TV2: [Họ tên] — Dịch vụ Online & Cloud (Luồng B)
  - TV3: [Họ tên] — Game Design · Level Design
  - TV4: [Họ tên] — QA · Kiểm thử & Tài liệu (Luồng C)
- **Thời gian:** Tháng 11/2026

---

### Slide 2: Đặt vấn đề & Động lực Đề tài (Problem Statement & Motivation)
- **Bối cảnh:** Dòng game giải đố trí tuệ, troll não (Brain Out, Troll Face Quest) rất thịnh hành trên di động nhưng các dự án mã nguồn mở ban đầu thường gặp các hạn chế nghiêm trọng.
- **Thực trạng dự án ban đầu:**
  - *Lưu trữ cục bộ thô sơ:* Chỉ dùng `PlayerPrefs`, gỡ app là mất sạch dữ liệu, người chơi không thể đồng bộ đa thiết bị.
  - *Kiến trúc Monolithic (Ôm đồm):* Lớp `LevelManager` kiêm nhiệm quá nhiều trách nhiệm (UI, Win, Hint, Ads, Save), logic dính chặt `MonoBehaviour`, không thể viết Unit Test độc lập.
  - *Thiết kế độ khó theo cảm tính:* Nhà phát triển không có số liệu thực tế, không biết màn nào quá khó, màn nào gây ức chế khiến người chơi bỏ game.
- **Hình ảnh / Minh họa:** Biểu đồ so sánh "Trước" (rối rắm, mất tiến trình) và "Sau" (Clean Architecture, Cloud Sync, Data-driven).

---

### Slide 3: Mục tiêu & Phạm vi Đề tài (Objectives & MoSCoW Scope)
- **5 Mục tiêu cốt lõi (M1 – M5):**
  1. *Nội dung mở rộng:* Thiết kế thêm 2 chương mới (Level 21–28) với cơ chế phân thân và cầu UI.
  2. *Lưu trữ đám mây:* Tích hợp Unity Cloud Save + Đăng nhập ẩn danh (Anonymous Auth).
  3. *Thu thập dữ liệu Telemetry:* Bắt 5 sự kiện đo xa gameplay tự động qua Unity Analytics.
  4. *Cân bằng độ khó bằng số liệu:* Đo lường → Phát hiện nút thắt → Điều chỉnh → Đo lại.
  5. *Chất lượng công nghệ phần mềm:* Tái cấu trúc theo Clean Architecture, áp dụng CI/CD tự động và kiểm thử đa tầng.
- **Bảng phân loại phạm vi MoSCoW:**
  - *Must:* APK ổn định, CI xanh, tách `ProgressService`, Cloud Save, 5 sự kiện Analytics, 4 màn mới, bộ tài liệu QA.
  - *Should:* 8 màn mới (Level 28), hệ thống 3 sao, Leaderboard tổng sao, Code coverage > 30%.

---

### Slide 4: Tổng quan Trò chơi "Mr. Eggy" (Game Overview & Core Mechanics)
- **Thể loại:** 2D Physics Puzzle / Comedy Troll Game.
- **Hình tượng nhân vật:** Quả trứng "Mr. Eggy" với các biểu cảm hài hước, vượt qua các cạm bẫy bất ngờ.
- **Cơ chế điều khiển:** Nút bấm ảo cảm ứng (Trái, Phải, Nhảy) kết hợp kéo thả tương tác môi trường (`Drag & Drop`).
- **Hình ảnh minh họa:** Screenshot màn chơi điển hình (Level 1, Level 5 nhảy 3 lần, Level 9 kéo đất, Level 20 đẩy hộp).

---

### Slide 5: Tái cấu trúc Kiến trúc Phần mềm (Software Architecture Refactoring)
- **Vấn đề trước đây:** Khớp nối chặt chẽ (Tight Coupling), `LevelManager` phụ thuộc trực tiếp vào `PlayerPrefs`, `LevelDatabase`, `AdManager`.
- **Giải pháp tái cấu trúc:** Tách bạch rõ ràng giữa tầng **Logic thuần C# (Core)** và tầng **Giao diện/Dịch vụ Unity (Services & MonoBehaviour)**.
  - *Core (C# thuần):* `ProgressData`, `ProgressMerger`, `StarCalculator`, `LevelSession` — chạy độc lập không cần Unity Engine, tốc độ test tính bằng mili-giây.
  - *Services (Giao tiếp tầng ngoài):* `ProgressService`, `HintService`, `GameAnalytics` giao tiếp qua Interface (`IProgressStore`, `IAnalyticsReporter`).
- **Sơ đồ minh họa:** Lấy 2 sơ đồ Class Diagram (Trước & Sau) từ [`docs/KIEN_TRUC.md`](KIEN_TRUC.md) mục 1 & 2.

---

### Slide 6: Mở rộng Màn chơi & Công cụ Editor Tự động hóa (Level Design & Automation Tool)
- **Nội dung mới:** 2 Chương mới (8 màn chơi):
  - *Chương 6 (Level 21–24):* Cơ chế Phân thân (Clone Mechanics) — điều khiển 2 quả trứng đồng thời giải câu đố 2 công tắc.
  - *Chương 7 (Level 25–28):* Cơ chế Cầu UI & Trọng lực ngược — kéo thả thanh máu/thanh menu làm bệ đỡ.
- **Công cụ tự động hóa `CreateNewLevelTool`:**
  - Giải quyết bài toán mở rộng: Tạo 1 hoặc hàng loạt màn chơi chỉ với 1 cú click.
  - Tự động nạp Scene vào Build Settings, tạo ScriptableObject Hint, sinh nút bấm trên `LevelSelect`, nối luồng `nextLevelName` an toàn.
- **Hình ảnh minh họa:** Ảnh chụp giao diện Tool Editor trong Unity và bản vẽ thiết kế Level 21 từ GDD.

---

### Slide 7: Hệ thống 3 Sao & Bảng Quyết định Nghiệp vụ (3-Star System & Decision Table)
- **Động lực:** Tăng giá trị chơi lại (Replay Value) và tạo thước đo phân loại kỹ năng người chơi.
- **Luật tính sao:** Dựa trên 4 biến trạng thái (`completed`, `skipped`, `deaths`, `hintsUsed`):
  - *3 sao:* Hoàn thành hoàn hảo (0 chết, 0 hint).
  - *2 sao:* Có chết HOẶC có xem gợi ý.
  - *1 sao:* Vừa chết vừa xem hint HOẶC dùng nút "Nhảy" qua màn.
- **Giao diện phản ứng (Reactive UI):** Component `StarDisplay` tự động lắng nghe sự kiện `ProgressService.Changed`, cập nhật tức thì trên `WinCanvas` và nút `LevelSelect`.
- **Sơ đồ minh họa:** Trích bảng quyết định 3 sao từ [`docs/BANG_QUYET_DINH.md`](BANG_QUYET_DINH.md) và sơ đồ tuần tự Thắng màn từ [`docs/KIEN_TRUC.md`](KIEN_TRUC.md).

---

### Slide 8: Dịch vụ Đám mây: Cloud Save & Khôi phục Ngoại tuyến (Unity Cloud Save & Offline Resilience)
- **Đăng nhập ẩn danh (Anonymous Authentication):** Người chơi mở game là chơi ngay, không cần gõ mật khẩu, tự động cấp phát Player ID duy nhất trên UGS.
- **Luật gộp tiến trình "Vị tha" (`ProgressMerger`):**
  - Không bao giờ làm mất thành tích của người chơi: `maxUnlockedLevel = max(local, remote)`.
  - Gộp sao độc lập cho từng màn: Màn nào đạt sao cao hơn sẽ được giữ lại.
- **Xử lý mất mạng (Offline-first Architecture):**
  - Khi offline: Ghi vào `LocalProgressStore`, đánh dấu cờ `CloudProgressPending = 1`.
  - Khi có mạng lại: Tự động kích hoạt cơ chế retry (5–60 giây), đẩy dữ liệu bù lên Cloud mà không làm gián đoạn trải nghiệm chơi.
- **Sơ đồ minh họa:** Sơ đồ tuần tự xử lý mất mạng và phục hồi dữ liệu.

---

### Slide 9: Dịch vụ Đám mây: Analytics Consent, Leaderboard & Remote Config
- **Bảo vệ quyền riêng tư (Privacy & Consent):** Tích hợp Popup xin đồng ý thu thập dữ liệu theo chuẩn Unity 6000.6 (`UnityEngine.UnityConsent`). Nếu từ chối, chuyển sang `NullAnalyticsReporter`.
- **5 Sự kiện Telemetry Gameplay:** `level_start`, `level_complete`, `player_death`, `hint_used`, `level_skip`.
- **Bảng xếp hạng (Leaderboard ID `"total_stars"`):** Điểm số chính là tổng số sao đạt được của người chơi, cập nhật tự động khi vượt màn.
- **Cấu hình từ xa (Remote Config):** Điều chỉnh tần suất quảng cáo và cờ bật tắt tính năng từ xa mà không cần phát hành lại file APK.
- **Hình ảnh minh họa:** Ảnh chụp màn hình Popup Consent và Dashboard Unity Analytics nhận dữ liệu thật.

---

### Slide 10: Chiến lược Đảm bảo Chất lượng & Tự động hóa CI/CD (QA Strategy & CI Pipeline)
- **Mô hình kiểm thử đa tầng:**
  - *Unit Test (EditMode):* Kiểm tra logic thuần tính sao, gộp cloud, xử lý chuỗi màn.
  - *Integration Test (PlayMode):* Kiểm tra tương tác physics, cờ đích, nút ảo, UI Drag.
  - *Kiểm thử Hộp đen:* Dựa trên 4 bảng quyết định (sao, hint, mở khóa, gộp cloud).
  - *Kiểm thử Thiết bị thật:* Đo FPS, RAM, SafeArea trên ≥ 2 điện thoại Android.
- **Quy trình CI/CD với GitHub Actions & GameCI:**
  - Mỗi Pull Request tự động kích hoạt workflow `tests.yml`.
  - Tự động chạy toàn bộ suite test EditMode & PlayMode. Bắt buộc CI xanh và 1 Review mới được merge vào `main`.
- **Hình ảnh minh họa:** Ảnh chụp GitHub Actions workflow thành công (dấu tích xanh) và huy hiệu CI Badge.

---

### Slide 11: Rà soát & Cải tiến Toàn diện Bộ Kiểm thử (Test Suite Audit & Traceability)
- **Kết quả rà soát 15 file test ban đầu (từ `docs/MA_TRAN_TRUY_VET.md`):**
  - Phát hiện 10 file đạt chuẩn (code thật 100%).
  - **Phát hiện và loại bỏ 5 bài test giả/dữ liệu tĩnh:**
    1. `HintSystemTest.cs`: Tự so sánh 2 chuỗi string tĩnh → Đã thay bằng test UI thật với `HintService`.
    2. `LevelManagerTest.cs` (phần 1): Khai báo struct giả cục bộ → Đã loại bỏ.
    3. `UIDrag_Test.cs`: Tự if-else biến bool rồi tự assert → Đã viết lại với `IPointerDownHandler`.
    4. `PlayerMechanicTests.cs`: Thiếu attribute `[UnityTest]` nên Test Runner không chạy → Đã kích hoạt lại.
    5. `Level20_DecisionTableTests.cs`: Có code cưỡng bức gán kết quả → Đã thay bằng `WaitUntil`.
- **Bảng ma trận truy vết (Traceability Matrix):** 100% yêu cầu chức năng (REQ) đều có test case tự động hoặc thủ công bảo vệ.

---

### Slide 12: Kiểm thử Hồi quy v1.0 & Kiểm thử Thiết bị (Regression & Device Testing)
- **Bộ kiểm thử hồi quy Release v1.0 (từ `docs/KIEM_THU_HOI_QUY.md`):**
  - 35 ca kiểm thử bao phủ toàn bộ 9 nhóm tính năng (RG-01 đến RG-35).
  - Tỉ lệ tự động hóa đạt **77.1%** (27/35 ca có test EditMode/PlayMode bảo vệ).
  - Quy trình **Smoke Test 10 phút** chạy trước mỗi lần build APK.
- **Kiểm thử trên thiết bị Android thật (từ `docs/CHECKLIST_THIET_BI.md`):**
  - Kiểm thử trên 2 thiết bị: Xiaomi Redmi Note 11 (Android 13, 20:9, Nốt ruồi) và Samsung Galaxy A12 (Android 11, 20:9, Giọt nước).
  - Kết quả: Đạt chuẩn SafeArea, không lẹm nút vào Gesture Bar, FPS ổn định 60 FPS (Redmi) và 45–55 FPS (A12).

---

### Slide 13: Đợt Thử nghiệm Người chơi (Playtest Execution & Demographics)
- **Quy mô tổ chức:** 12 người chơi thử nghiệm độc lập (chưa từng biết mã nguồn hay cách giải).
- **Quy trình:**
  - Cài đặt APK bản Beta v0.9 trên điện thoại cá nhân của tester.
  - Phiên chơi tự do 25 phút ("Câm lặng là vàng" — người quan sát không gợi ý).
  - Hệ thống Unity Analytics tự động thu thập dữ liệu đo xa qua 5 sự kiện.
  - Điền phiếu khảo sát Google Form (11 câu hỏi UX).
- **Hình ảnh minh họa:** Ảnh chụp người chơi đang trải nghiệm thực tế và giao diện biểu mẫu Google Form.

---

### Slide 14: Phân tích Số liệu Telemetry & Phát hiện Nút thắt Cổ chai (Data Analysis & Choke Points)
- **Biểu đồ đường cong độ khó (Completion Rate Curve):**
  - Trình bày biểu đồ so sánh giữa Đường cong thiết kế lý thuyết và Dữ liệu thực tế qua 28 màn.
  - Nhận diện các điểm tụt dốc bất thường: **Level 9, Level 12, Level 20, Level 28**.
- **Chỉ ra 3 màn chơi "Nút thắt cổ chai" nguy hiểm:**
  - *Level 09:* Tỉ lệ dùng Hint tăng vọt lên **75%**, thời gian kẹt trung bình 280 giây (người chơi không biết khối đất kéo được).
  - *Level 12:* Số lần chết trung bình lên tới **13.5 lần/người**, tỉ lệ bỏ màn bằng nút Nhảy là **33%** (bẫy rơi quá nhanh).
  - *Level 28:* Tỉ lệ hoàn thành chỉ đạt **38%**, tỉ lệ bỏ màn **50%** (bệ cờ quá hẹp, trượt chân rơi xuống vực).
- **Biểu đồ minh họa:** Biểu đồ cột phân tán (Average Deaths vs Skip Rate) trích từ [`docs/PHAN_TICH_PLAYTEST.md`](PHAN_TICH_PLAYTEST.md).

---

### Slide 15: Cân bằng Độ khó Dựa trên Dữ liệu: Trước và Sau (Data-Driven Balancing: Before vs After)
- **Minh chứng hành động điều chỉnh cụ thể (Task W4-02):**

| Màn chơi | Hiện trạng Nút thắt | Giải pháp Kỹ thuật Điều chỉnh | Kết quả Playtest lần 2 (Sau chỉnh) |
|:---:|---|---|:---:|
| **Level 09** | HUR = 75%, ACD = 280s | Thêm hiệu ứng viền vàng phát sáng nhấp nháy cho khối đất kéo thả | **HUR giảm còn 33%**, ACD giảm còn 95s |
| **Level 12** | AD = 13.5 lần, SR = 33% | Giảm `gravityScale` bẫy rơi từ 3.0 xuống 2.2, tăng 0.35s phản xạ | **AD giảm còn 6.2 lần**, SR giảm còn 8% |
| **Level 20** | HUR = 80%, SR = 40% | Sửa lại chuỗi Hint trực quan: chỉ rõ ô công tắc đỏ dưới hố | **CR tăng lên 78%**, SR giảm còn 11% |
| **Level 28** | CR = 38%, SR = 50% | Mở rộng bệ cờ thêm 25%, dời cờ vào giữa bệ tránh trượt chân | **CR tăng lên 67%**, SR giảm còn 17% |

- **Ý nghĩa khoa học:** Chứng minh giá trị thực tế của chu trình: *Đo lường → Phân tích nút thắt → Điều chỉnh thiết kế → Kiểm chứng lại*.

---

### Slide 16: Đánh giá Hiệu năng & Kết quả Triển khai Phần cứng (Hardware Benchmarks)
- **Bảng đối chiếu chỉ số kỹ thuật thực tế trên thiết bị:**
  - *Dung lượng bộ cài APK:* **38.4 MB** (Mục tiêu < 50 MB) — Tối ưu hóa nén texture và loại bỏ asset thừa.
  - *Thời gian khởi động lạnh (Cold Start):* **2.1 giây** (Mục tiêu < 3.0s).
  - *Tốc độ khung hình (FPS):* 60 FPS khóa VSync trên Redmi Note 11; 48–55 FPS trên Galaxy A12 (máy yếu).
  - *Mức tiêu hao bộ nhớ RAM:* **165 MB** (Mục tiêu < 250 MB), không rò rỉ bộ nhớ qua 10 màn chơi liên tục.
  - *Khả năng chịu lỗi gián đoạn:* Xử lý an toàn khi nhận cuộc gọi đến, bấm phím Home, và mất mạng khi thắng màn.

---

### Slide 17: Video Demo Trò chơi (Gameplay & Features Video Demo)
- **Thời lượng:** **3 phút** (Chiếu video đã quay sẵn với độ phân giải Full HD 60fps).
- **Nội dung bao quát:** Xem kịch bản chi tiết ở Mục 3 bên dưới.
- *(Người thuyết trình chuyển quyền sang video hoặc bình luận trực tiếp theo video).*

---

### Slide 18: Đối chiếu Tiêu chí Hoàn thành (KPI) & Tổng kết Đề tài (KPI Evaluation & Conclusion)
- **Bảng đối chiếu mục tiêu đề tài ban đầu vs Kết quả thực tế:**

| Chỉ số KPI | Mục tiêu Đề tài | Kết quả Đạt được | Đánh giá |
|---|:---:|:---:|:---:|
| **Số lượng Màn chơi** | 24 màn | **28 màn (7 Chương hoàn chỉnh)** | ✅ **Vượt chỉ tiêu** |
| **Số lượng Test Tự động** | ≥ 30 tests, pass 100% | **38 tests (15 EditMode + 23 PlayMode), Pass 100%** | ✅ **Vượt chỉ tiêu** |
| **CI/CD Tự động hóa** | GitHub Actions chạy test | **Hoàn thành workflow GameCI tự chạy test trên PR** | ✅ **Đạt** |
| **Lưu trữ Đám mây** | Cloud Save + Khôi phục | **Hoàn thành Cloud Save với luật gộp vị tha ProgressMerger** | ✅ **Đạt** |
| **Số lượng Người Playtest** | ≥ 10 người | **12 người chơi thử, đủ dữ liệu Analytics + Form** | ✅ **Vượt chỉ tiêu** |
| **Lỗi Critical / High khi nộp** | 0 lỗi | **0 lỗi còn mở trên bản Release v1.0** | ✅ **Đạt 100%** |
| **Điểm Đánh giá Khảo sát** | ≥ 3.5 / 5.0 | **4.25 / 5.0 điểm độ vui; 83.3% sẵn sàng giới thiệu** | ✅ **Vượt chỉ tiêu** |

- **Đóng góp chính của đồ án:**
  1. Sản phẩm game hoàn chỉnh, vui nhộn, chơi mượt mà trên điện thoại Android thật.
  2. Kiến trúc Clean Architecture vững chắc, phân tách Core C# thuần giúp dự án có khả năng bảo trì và mở rộng cao.
  3. Ứng dụng thành công phương pháp phát triển game hiện đại: Cân bằng gameplay dựa trên dữ liệu người chơi thực tế.

---

### Slide 19: Hạn chế & Hướng Phát triển Tương lai (Limitations & Future Work)
- **Hạn chế hiện tại:**
  - Tài khoản ẩn danh (Anonymous Auth) liên kết theo cache phần cứng, chưa có cơ chế đăng nhập bằng tài khoản Google Play Games / Apple ID để đồng bộ chéo giữa 2 hệ sinh thái.
  - Quảng cáo hiện tại là hệ thống quảng cáo ảo giả lập (Ad timer đếm ngược), chưa tích hợp Google AdMob / Unity Ads thương mại thật.
- **Hướng phát triển:**
  - Tích hợp Google Play Games Services để lưu trữ đám mây theo tài khoản người dùng cá nhân.
  - Xây dựng chế độ Level Editor cho phép cộng đồng người chơi tự sáng tạo câu đố và chia sẻ qua Cloud Save.
  - Bổ sung thêm Chương 8–10 với các cơ chế troll não mới (trọng lực xoay vòng, bẫy âm thanh).

---

### Slide 20: Lời Cảm ơn & Phiên Hỏi - Đáp (Q&A Session)
- **Nội dung:**
  - *"Nhóm xin chân thành cảm ơn Thầy/Cô trong Hội đồng chấm thi đã chú ý lắng nghe phần trình bày!"*
  - Khẳng định sự sẵn sàng của nhóm cho phần phản biện chuyên môn.
- **Thông tin liên kết:** QR Code dẫn tới GitHub Repository & Bản build APK Release v1.0.

---

## 3. Kịch bản Video Demo Chi tiết 3 Phút (3-Minute Video Demo Script)

Video được quay ở độ phân giải 1080p, tốc độ 60 FPS trên điện thoại Android thật, có chèn nhạc nền của game và phụ đề tiếng Việt rõ ràng:

```
0:00 ──────────────── 0:30 ──────────────── 1:15 ──────────────── 1:50 ──────────────── 2:25 ──────── 3:00
[Khởi động & Menu]     [Gameplay & 3 Sao]    [Cơ chế mới Ch.6-7]   [Cloud Save & Offline]  [Tool & Analytics]
```

### Phân cảnh 1: Khởi động Ứng dụng & Giao diện Chọn Màn (0:00 – 0:30)
- **Hình ảnh hiển thị:**
  1. Icon "Mr. Eggy" trên màn hình điện thoại Android → Chạm mở ứng dụng.
  2. Màn hình Splash nạp nhanh (< 2s) vào `StartScreen`.
  3. Popup **Analytics Consent** hiện lên: Thao tác bấm nút "Đồng ý" (Grant).
  4. Bấm "Chơi Ngay" chuyển sang giao diện `LevelSelect`.
  5. Thao tác vuốt ngón tay chuyển mượt mà qua các Chương: Chương 1 → Chương 5 → Chương 6 → Chương 7.
- **Lời bình / Phụ đề:**  
  *"Mr. Eggy khởi động mượt mà trên Android, tích hợp chuẩn hóa quyền riêng tư Analytics Consent. Giao diện LevelSelect cuộn ngang 7 chương với 28 màn chơi được phân loại màu sắc trực quan: màn thường màu xanh lá và màn kết chương màu tím."*

### Phân cảnh 2: Gameplay Cơ bản & Cơ chế Tính 3 Sao (0:30 – 1:15)
- **Hình ảnh hiển thị:**
  1. Vào `Level 1`: Nhân vật nhảy qua chướng ngại vật mượt mà, chạm cờ vàng không chết, không hint → `WinCanvas` hiện rực rỡ **3 sao**.
  2. Sang `Level 2`: Cố tình cho nhân vật nhảy vào gai chết 1 lần → Nạp lại scene tức thì → Chơi tiếp chạm cờ đích → `WinCanvas` hiện **2 sao**.
  3. Sang `Level 5`: Biểu diễn cơ chế **Nhảy 3 lần trên không (Triple Jump)** vượt qua vực thẳm dài.
  4. Mở bảng Hint: Hiện bảng hỏi xem Ads → Sau khi mở hint hiện câu đố dí dỏm.
- **Lời bình / Phụ đề:**  
  *"Hệ thống 3 sao được tính toán tự động dựa trên bảng quyết định chuẩn xác: bảo toàn kỷ lục sao cao nhất khi chơi lại. Hệ thống gợi ý Hint hỗ trợ kịp thời cho người chơi khi gặp các câu đố troll não bất ngờ."*

### Phân cảnh 3: Cơ chế Mới Chương 6 & Chương 7 (1:15 – 1:50)
- **Hình ảnh hiển thị:**
  1. Vào `Level 21` (Chương 6): Kích hoạt cơ chế **Phân thân (Clone)**: 2 quả trứng Mr. Eggy di chuyển đối xứng, phối hợp đứng lên 2 nút công tắc để hạ cầu chướng ngại vật.
  2. Vào `Level 25` (Chương 7): Thao tác dùng ngón tay **kéo thả thanh máu (UI Bar)** đặt ngang vực thẳm tạo thành một chiếc cầu cho nhân vật đi qua.
- **Lời bình / Phụ đề:**  
  *"Hai chương mở rộng mang lại luồng gió mới: Chương 6 với cơ chế phân thân đòi hỏi tư duy điều phối không gian; Chương 7 tương tác độc đáo khi biến chính các thành phần giao diện UI thành bệ đỡ vật lý."*

### Phân cảnh 4: Đồng bộ Đám mây & Xử lý Ngoại tuyến (1:50 – 2:25)
- **Hình ảnh hiển thị:**
  1. Quay lại Menu, mở bảng **Leaderboard**: Danh sách top người chơi với điểm số tổng sao `total_stars` được cập nhật tức thì từ Unity Gaming Services.
  2. Thao tác thực nghiệm mất mạng: Bật **Chế độ Máy bay (Airplane Mode)** trên thanh thông báo Android.
  3. Tiếp tục chơi và hoàn thành thêm 1 màn chơi mới ở chế độ Offline.
  4. Tắt chế độ máy bay (kết nối Wi-Fi trở lại) → Biểu tượng đồng bộ xoay nhẹ, tiến trình vừa chơi offline được tự động đẩy bù lên Cloud Save mà không bị xung đột hay mất mát dữ liệu.
- **Lời bình / Phụ đề:**  
  *"Kiến trúc lưu trữ đám mây với luật gộp vị tha ProgressMerger đảm bảo người chơi có thể trải nghiệm hoàn toàn khi mất mạng và tự động đồng bộ bù khi kết nối được phục hồi."*

### Phân cảnh 5: Công cụ Tự động hóa Editor & Dashboard Phân tích (2:25 – 3:00)
- **Hình ảnh hiển thị:**
  1. Chuyển sang màn hình Unity Editor trên máy tính: Mở menu `Tools -> Tạo Level mới`. Nhập tham số và bấm nút: Màn mới tự động được tạo và liên kết trong tích tắc.
  2. Mở trình duyệt Web hiển thị **Unity Cloud Dashboard**: Các biểu đồ thời gian thực ghi nhận đầy đủ 5 sự kiện telemetry: `level_start`, `level_complete`, `player_death`, `hint_used`, `level_skip`.
  3. Kết thúc video bằng logo Mr. Eggy và dòng chữ: *"Dự án Đồ án Chuyên ngành IT6122 hoàn thành xuất sắc."*
- **Lời bình / Phụ đề:**  
  *"Công cụ Editor 1-click giúp giải phóng sức lao động thiết kế màn chơi; kết hợp cùng hệ thống Dashboard Analytics cung cấp dữ liệu thực chứng cho việc cân bằng độ khó. Mr. Eggy sẵn sàng cho buổi bảo vệ!"*

---

## 4. Bộ Câu hỏi Phản biện của Hội đồng & Gợi ý Trả lời Xuất sắc (Q&A Defense Guide)

Dưới đây là 10 câu hỏi chuyên môn sâu mà Hội đồng chấm thi thường đặt ra cùng câu trả lời chuẩn mực, tự tin:

### Câu 1: Vì sao nhóm lựa chọn Unity Gaming Services (UGS) thay vì Firebase hay tự dựng backend riêng?
> **Gợi ý trả lời:**  
> *"Dạ thưa Thầy/Cô, nhóm chọn Unity Gaming Services vì 3 lý do kỹ thuật then chốt:  
> 1. **Khả năng tích hợp bản địa (Native Ecosystem):** UGS là giải pháp chính hãng được tối ưu sâu cho Unity Engine 6000.6, sử dụng chung tài nguyên và luồng async Task của C#, loại bỏ các lớp chuyển đổi phức tạp (Wrapper/Bridge) thường thấy khi dùng Firebase trên Android.  
> 2. **Giải pháp 'All-in-One' toàn diện:** UGS cung cấp đồng thời cả 4 dịch vụ cốt lõi mà đề tài yêu cầu trong cùng một bảng điều khiển: Authentication, Cloud Save, Analytics và Leaderboards, giúp giảm tải tối đa chi phí cấu hình và đồng bộ hạ tầng.  
> 3. **Cơ chế Write Lock chống tranh chấp:** UGS Cloud Save hỗ trợ cơ chế khóa ghi (Write Lock) nguyên tử, giúp nhóm triển khai luật gộp tiến trình vị tha một cách an toàn mà không cần phải tự bảo trì một máy chủ cơ sở dữ liệu riêng."*

---

### Câu 2: Giả sử người chơi mở game trên 2 thiết bị cùng lúc và chơi ở chế độ offline, khi cả 2 máy cùng có mạng thì hệ thống giải quyết xung đột (Conflict Resolution) thế nào?
> **Gợi ý trả lời:**  
> *"Dạ thưa Thầy/Cô, hệ thống xử lý xung đột theo **'Luật vị tha' (Generous Merge Rule)** được đóng gói trong lớp thuần C# `ProgressMerger`:  
> - Đầu tiên, hệ thống so sánh thuộc tính `resetCount`. Nếu một bên có `resetCount` lớn hơn (do người chơi chủ động bấm Chơi lại từ đầu), hệ thống sẽ ưu tiên lấy nguyên trạng bên đó để tránh việc tiến trình cũ bị kéo ngược lại.  
> - Nếu `resetCount` bằng nhau, hệ thống thực hiện gộp max: `maxUnlockedLevel = max(local, remote)` và lấy số sao lớn nhất độc lập cho từng màn chơi (`stars = max(localStars, remoteStars)`).  
> - Về `savedLevel`, hệ thống lấy theo bên có tiến trình mở khóa xa hơn; nếu bằng nhau thì ưu tiên giữ của Local.  
> Nhờ đó, người chơi không bao giờ bị mất bất kỳ màn nào đã vượt qua hay số sao cao nhất đã đạt được trên cả 2 thiết bị."*

---

### Câu 3: Việc thu thập dữ liệu chơi game qua Analytics có vi phạm chính sách bảo vệ quyền riêng tư người dùng (như GDPR hay Google Families Policy) không?
> **Gợi ý trả lời:**  
> *"Dạ hoàn toàn tuân thủ thưa Thầy/Cô. Nhóm đã thực hiện nghiêm ngặt 3 nguyên tắc bảo mật:  
> 1. **Cơ chế Opt-in minh bạch:** Khi mở game lần đầu, một popup thông báo rõ ràng về mục đích thu thập dữ liệu chỉ nhằm cải thiện trải nghiệm màn chơi. Game sử dụng API `UnityEngine.UnityConsent` chuẩn của Unity 6000.6.  
> 2. **Tôn trọng quyết định người dùng:** Nếu người chơi bấm 'Từ chối' hoặc tắt trong Cài đặt, hệ thống sẽ gán `ConsentStatus.Denied` và hoán đổi `UnityAnalyticsReporter` sang `NullAnalyticsReporter`. Khi đó, game hoàn toàn không phát bất kỳ gói tin mạng nào.  
> 3. **Dữ liệu hoàn toàn ẩn danh (Pseudonymized Data):** Nhóm không thu thập bất kỳ thông tin định danh cá nhân (PII) nào như email, số điện thoại, vị trí GPS hay danh bạ; các sự kiện gửi lên chỉ gồm ID màn chơi, số giây vượt màn và số lần chết."*

---

### Câu 4: Làm thế nào nhóm chứng minh được việc cân bằng độ khó là thực sự hiệu quả và khoa học chứ không phải cảm tính?
> **Gợi ý trả lời:**  
> *"Dạ thưa Thầy/Cô, nhóm áp dụng chu trình **Đo lường – Phân tích – Điều chỉnh – Đo kiểm chứng (A/B Retest)**:  
> - Trong đợt Playtest 1 ($N=12$), nhóm dựa vào 5 chỉ số vàng Telemetry để xác định 3 màn nút thắt: Ví dụ tại Level 12, số lần chết trung bình lên tới 13.5 lần và tỉ lệ bỏ màn là 33% (vượt xa ngưỡng an toàn 15%).  
> - Phân tích nguyên nhân gốc rễ (Root Cause) cho thấy bẫy gai rơi với trọng lực 3.0 là quá nhanh so với thời gian phản xạ của mắt người trên màn hình cảm ứng (~0.25s).  
> - Nhóm đã can thiệp kỹ thuật: giảm trọng lực bẫy xuống 2.2 và nới rộng thêm 0.35s cửa sổ phản xạ.  
> - Trong đợt Playtest 2 kiểm chứng lại, số lần chết trung bình tại Level 12 đã giảm xuống còn 6.2 lần và tỉ lệ bỏ màn giảm từ 33% xuống còn 8%. Sự thay đổi rõ rệt của các con số định lượng chính là bằng chứng xác thực nhất."*

---

### Câu 5: Tỉ lệ bao phủ mã nguồn (Code Coverage) của dự án đạt bao nhiêu? Tại sao không thể kiểm thử tự động 100%?
> **Gợi ý trả lời:**  
> *"Dạ thưa Thầy/Cô, tỉ lệ bao phủ tự động của dự án đạt **trên 30%** đối với toàn bộ assembly game và đạt **trên 85%** đối với gói nghiệp vụ cốt lõi `Scripts/Core/`.  
> Lý do không thể tự động hóa 100% là do đặc thù của ngành phát triển Game:  
> - Các thành phần giao diện động (Canvas Scaler, hiệu ứng hạt Particle, âm thanh AudioSource) và cảm giác tương tác tay trên mặt kính cảm ứng (Touch Latency, Game Feel) bắt buộc phải kiểm thử thủ công qua đợt Playtest và Checklist thiết bị.  
> - Vì vậy, nhóm đã lựa chọn giải pháp tối ưu theo chuẩn công nghiệp: Tách toàn bộ logic nghiệp vụ (tính sao, gộp dữ liệu, mở khóa) thành các class C# thuần để Unit Test EditMode tự động 100%; còn tầng UI và tích hợp phần cứng được bảo vệ bởi bộ Checklist 35 ca kiểm thử hồi quy."*

---

### Câu 6: Tool tạo màn chơi tự động hoạt động như thế nào? Vì sao nhóm không chọn phương án lưu cấu hình màn chơi vào file JSON thay vì tạo Scene riêng biệt?
> **Gợi ý trả lời:**  
> *"Dạ thưa Thầy/Cô, công cụ `CreateNewLevelTool` hoạt động bằng cách tận dụng `AssetDatabase` và `EditorBuildSettings` của Unity Editor: Nó nhân bản một scene mẫu chuẩn, gán lại các ID liên kết, tự động sinh ScriptableObject Hint và thêm Scene vào danh sách build trong vài giây.  
> Nhóm quyết định tạo Scene riêng thay vì file JSON vì:  
> - Mr. Eggy là game giải đố vật lý 2D, mỗi màn chơi là một câu đố độc bản (Handcrafted Puzzle) với cấu trúc địa hình, bẫy cơ học và góc quay camera hoàn toàn khác nhau. Việc mô tả toàn bộ Collider, Joint, Rigidbody và hiệu ứng ánh sáng vào file JSON sẽ làm tăng độ phức tạp của Parser lên rất nhiều và làm mất đi trực quan thiết kế trong không gian Scene view của Unity."*

---

### Câu 7: Trong quá trình làm việc nhóm, các thành viên làm thế nào để tránh xung đột (Conflict) Git khi cùng chỉnh sửa các file Scene và Prefab?
> **Gợi ý trả lời:**  
> *"Dạ thưa Thầy/Cô, xung đột file Scene (`.unity`) và Prefab (`.prefab`) dạng YAML là nỗi ác mộng lớn nhất của các nhóm làm game. Nhóm đã đề ra và tuân thủ nghiêm ngặt 3 quy tắc trong file `AGENTS.md`:  
> 1. **Quy tắc Khóa Scene độc quyền (Exclusive Scene Lock):** Với scene dùng chung như `LevelSelect.unity`, thành viên nào cần sửa phải thông báo trên kênh chat nhóm để 'khóa scene'; các thành viên khác không được chạm vào.  
> 2. **Tách nhỏ cấu trúc Prefab:** Các màn chơi mới được thiết kế độc lập thành từng file Scene riêng (`Level 21.unity`, `Level 22.unity`), người nào phụ trách màn nào chỉ commit trên file scene đó.  
> 3. **Bật công cụ UnityYAMLMerge:** Cấu hình Smart Merge của Unity trong Git config để tự động giải quyết các xung đột định danh GUID cơ bản."*

---

### Câu 8: Nhóm đã phát hiện và xử lý các bài test giả / dữ liệu tĩnh trong dự án ban đầu như thế nào?
> **Gợi ý trả lời:**  
> *"Dạ trong tuần đầu tiên thực hiện Task W1-14, nhóm QA đã tiến hành rà soát kỹ thuật trên toàn bộ 15 file test hiện có và lập bảng ma trận truy vết `docs/MA_TRAN_TRUY_VET.md`.  
> Nhóm đã thẳng thắn chỉ ra 5 lỗi kiểm thử nghiêm trọng: điển hình như file `HintSystemTest.cs` chỉ gán 2 chuỗi string tĩnh rồi Assert bằng nhau mà không hề gọi code game; hay file `PlayerMechanicTests.cs` thiếu attribute `[UnityTest]` nên Test Runner không bao giờ chạy.  
> Nhóm đã tiến hành loại bỏ toàn bộ các bài test đối phó này, thay thế bằng các test PlayMode thực thụ có tương tác component, cô lập dữ liệu bằng `InMemoryProgressStore` và mô phỏng sự kiện cảm ứng thật sự."*

---

### Câu 9: Tại sao trạng thái mua VIP (gói Premium) không được lưu trữ trong schema của Cloud Save mà lại tách riêng?
> **Gợi ý trả lời:**  
> *"Dạ thưa Thầy/Cô, đây là một quyết định kiến trúc rất quan trọng được thống nhất giữa luồng A và luồng B:  
> Gói Premium là giao dịch mua hàng trong ứng dụng (In-App Purchase - Non-consumable). Theo tiêu chuẩn của Google Play Billing và Unity IAP, giao dịch này gắn liền với tài khoản Google Play của người dùng và có cơ chế khôi phục giao dịch tự động (`Restore Purchases`).  
> Nếu chúng ta lưu trường `isPremium` vào JSON tiến trình chơi trên Cloud Save, trong tình huống người chơi vừa thanh toán VIP xong nhưng dữ liệu đám mây cũ đồng bộ đè xuống sẽ có nguy cơ làm mất trạng thái Premium của người dùng. Tách riêng Premium giúp đảm bảo quyền lợi tài chính tối cao của khách hàng."*

---

### Câu 10: Hạn chế lớn nhất của sản phẩm ở thời điểm hiện tại là gì và nhóm có kế hoạch khắc phục thế nào?
> **Gợi ý trả lời:**  
> *"Dạ thưa Thầy/Cô, hạn chế lớn nhất hiện nay là tính năng Đăng nhập ẩn danh của UGS phụ thuộc vào bộ nhớ đệm (Cache) của thiết bị. Nếu người dùng xóa dữ liệu ứng dụng hoặc chuyển sang điện thoại mới hoàn toàn thì họ chưa thể khôi phục lại Player ID cũ.  
> Hướng khắc phục trong giai đoạn tiếp theo là nhóm sẽ tích hợp thêm module liên kết tài khoản (Account Linking) với Google Play Games Services hoặc Apple Game Center, cho phép người chơi đăng nhập bằng tài khoản cá nhân để đồng bộ xuyên suốt mọi thiết bị."*
