# CHECKLIST KIỂM THỬ TRÊN THIẾT BỊ DI ĐỘNG ANDROID (DEVICE REGRESSION CHECKLIST)

> **Dự án:** Mr. Eggy Puzzle Game — Đồ án chuyên ngành IT6122  
> **Tài liệu bàn giao:** Task W4-05 (Agent Antigravity phụ trách luồng C)  
> **Áp dụng cho:** Bản kiểm thử Alpha v0.5, Beta v0.9 và Release v1.0 trên thiết bị vật lý thật  
> **Tài liệu tham chiếu:** [`docs/KE_HOACH_DO_AN.md`](KE_HOACH_DO_AN.md) (Mục 8, 11) · [`docs/KIEM_THU_HOI_QUY.md`](KIEM_THU_HOI_QUY.md)

---

## 1. Mục tiêu & Nguyên tắc kiểm thử Thiết bị thật

### 1.1. Mục tiêu
1. Đảm bảo bản build APK chạy ổn định trên ít nhất **2 dòng điện thoại Android khác biệt** về cấu hình phần cứng, phiên bản hệ điều hành (Android OS), độ phân giải và tỉ lệ màn hình.
2. Kiểm chứng các chỉ số phi chức năng then chốt: **Tốc độ khung hình (FPS ≥ 30/60)**, **Thời gian nạp scene (< 1.5s)**, **Dung lượng bộ cài APK (< 60MB)**, và **Mức tiêu hao bộ nhớ RAM (< 250MB)**.
3. Xác nhận giao diện hiển thị chuẩn xác, không bị che khuất bởi tai thỏ (Notch), nốt ruồi camera đục lỗ, hay thanh điều hướng cử chỉ (Gesture Navigation Bar).
4. Đảm bảo ứng dụng xử lý an toàn các tình huống gián đoạn vòng đời: nhận cuộc gọi đến, bấm nút Home, đa nhiệm, khóa màn hình và rớt kết nối mạng đột ngột.

### 1.2. Danh sách Hồ sơ Thiết bị Thử nghiệm (Target Device Profiles)

| Thông số kỹ thuật | Thiết bị 1 (Thiết bị chính - Phổ thông) | Thiết bị 2 (Thiết bị phụ - Cấu hình yếu/Cũ) | Thiết bị 3 (Tùy chọn - Tỉ lệ màn hình khác) |
|---|---|---|---|
| **Tên thiết bị** | **Xiaomi Redmi Note 11** (hoặc Samsung Galaxy A52) | **Samsung Galaxy A12** (hoặc Xiaomi Redmi 9A / Vsmart Joy 3) | **Tablet / Máy 16:9** (Samsung Tab A / Sony Xperia) |
| **Phiên bản Android** | Android 12 / 13 (MIUI / OneUI) | Android 10 / 11 (OneUI Core) | Android 11 / 12 |
| **Chipset (SoC)** | Qualcomm Snapdragon 680 (8 nhân) | MediaTek Helio P35 / G25 (8 nhân giá rẻ) | Exynos / Snapdragon |
| **Dung lượng RAM** | 4 GB / 6 GB | 3 GB (khả dụng thực tế ~1.2 GB) | 3 GB / 4 GB |
| **Độ phân giải (Resolution)** | Full HD+ (1080 x 2400 pixels) | HD+ (720 x 1600 pixels) | Full HD (1080 x 1920) hoặc WXGA (1280 x 800) |
| **Tỉ lệ khung hình (Aspect Ratio)** | **20 : 9** (Màn hình dài hiện đại) | **20 : 9** (Phân khúc giá rẻ) | **16 : 9** hoặc **16 : 10** (Tỉ lệ chuẩn) |
| **Đặc điểm viền màn hình (Cutout)** | Nốt ruồi ở giữa (Punch-hole) · 90 Hz | Giọt nước ở giữa (Waterdrop Notch) · 60 Hz | Viền đều không khuyết tật · 60 Hz |
| **Mật độ điểm ảnh (DPI)** | ~395 ppi (xxhdpi) | ~270 ppi (tvdpi / xhdpi) | ~280 - 400 ppi |

---

## 2. Tiêu chuẩn Đánh giá Phần cứng (Hardware Quality Benchmarks)

| Chỉ số kỹ thuật | Công cụ đo lường | Ngưỡng Tối thiểu (Chấp nhận được) | Ngưỡng Mục tiêu (Tốt nhất) |
|---|---|:---:|:---:|
| **Dung lượng file APK** | File Explorer / GitHub Release | < 75 MB | **< 50 MB** |
| **Dung lượng sau khi cài đặt** | Cài đặt ứng dụng Android (App Info) | < 150 MB | **< 100 MB** |
| **Thời gian Khởi động lạnh (Cold Start)** | Bấm giờ từ icon đến StartScreen | < 4.0 giây | **< 2.5 giây** |
| **Thời gian Khởi động ấm (Warm Start)** | Bật lại từ trạng thái Background | < 1.5 giây | **< 0.8 giây** |
| **Thời gian tải màn (Scene Transition)** | Chuyển giữa LevelSelect và Màn chơi | < 2.0 giây | **< 1.0 giây** |
| **Tốc độ khung hình (FPS - Máy yếu)** | Unity Profiler / GameBench / In-game FPS | **≥ 30 FPS** (ổn định, không giật) | **≥ 45 FPS** |
| **Tốc độ khung hình (FPS - Máy thường)** | Unity Profiler / GameBench / In-game FPS | **≥ 55 FPS** | **60 FPS** (khóa VSync) |
| **Mức tiêu hao RAM (Memory Footprint)** | Android Studio Profiler / ADB Shell | < 300 MB | **< 200 MB** |
| **Mức nhiệt độ pin / máy** | CPU-Z / Pin Android sau 30 phút chơi | < 42°C | **< 38°C** (ấm nhẹ) |

---

## 3. Checklist Kiểm thử Chi tiết trên Thiết bị

*Quy ước đánh dấu:*  
`[P]` = Đạt (Pass) · `[F]` = Thất bại (Fail) · `[N/A]` = Không áp dụng · `[B]` = Phát sinh lỗi cần log bug

### Hạng mục 1: Cài đặt, Khởi động & Quản lý Dung lượng

| ID | Nội dung kiểm tra | Tiêu chuẩn chấp nhận | Thiết bị 1 | Thiết bị 2 | Ghi chú kỹ thuật |
|:---:|---|---|:---:|:---:|---|
| **DEV-01** | Cài đặt APK từ file ngoài (Side-loading) | Trình cài đặt Android phân tích cú pháp gói (parsing package) trơn tru, không báo lỗi "App not installed" hay lỗi chữ ký số. | `[ ]` | `[ ]` | Bật "Cài từ nguồn không xác định" |
| **DEV-02** | Quyền hạn ứng dụng (App Permissions) | Ứng dụng chỉ xin quyền tối thiểu (Internet/Network state cho UGS); không đòi hỏi các quyền nhạy cảm (Camera, Danh bạ, Vị trí...). | `[ ]` | `[ ]` | Tuân thủ Google Play Store Policy |
| **DEV-03** | Khởi động lần đầu (First Launch) | Icon ứng dụng sắc nét; tên game hiển thị đúng `"Mr. Eggy"`; màn hình Splash xuất hiện không bị méo và chuyển mượt vào `StartScreen`. | `[ ]` | `[ ]` | Kiểm tra độ phân giải Adaptive Icon |
| **DEV-04** | Kiểm tra kích thước bộ cài APK | Dung lượng file APK bản Release sau khi build nén đạt chuẩn đề ra. | `[ ]` | `[ ]` | Kích thước file: _____ MB |

---

### Hạng mục 2: Tương thích Màn hình, Tai thỏ (Notch) & Vùng an toàn (SafeArea)

| ID | Nội dung kiểm tra | Tiêu chuẩn chấp nhận | Thiết bị 1 | Thiết bị 2 | Ghi chú kỹ thuật |
|:---:|---|---|:---:|:---:|---|
| **DEV-05** | Tránh lẹm vào Khuyết tật màn hình (Notch / Punch-hole) | Các nút chức năng ở góc trên (Nút Cài đặt, Shop, Pause, Nút Hint, Số mạng) không bị che lấp bởi nốt ruồi camera hoặc tai thỏ. | `[ ]` | `[ ]` | Kiểm tra script `SafeArea.cs` |
| **DEV-06** | Tương thích Thanh điều hướng cử chỉ (Gesture Bar) | Dải cảm ứng vuốt về Home của Android ở đáy màn hình không chạm đè vào cụm nút điều khiển di chuyển (Mũi tên Trái, Phải, Nhảy). | `[ ]` | `[ ]` | Thao tác bấm nút không bị kích hoạt thoát app |
| **DEV-07** | Căn chỉnh tỉ lệ khung hình (Aspect Ratio Scaling) | Với cả 2 tỉ lệ màn hình (20:9 và 16:9), thanh chọn chương `LevelSelect` hiển thị trọn vẹn, camera 2D bao quát đủ chiều ngang màn chơi, không bị kéo dãn hình ảnh (Stretching). | `[ ]` | `[ ]` | Canvas Scaler: Match Width or Height |
| **DEV-08** | Khóa hướng màn hình (Orientation Lock) | Game cố định tuyệt đối ở chiều ngang (`Landscape` / `Landscape Left / Right`); khi người dùng xoay đứng máy thì màn hình game không bị xoay dọc làm vỡ UI. | `[ ]` | `[ ]` | Cấu hình trong PlayerSettings |

---

### Hạng mục 3: Hiệu năng Thực thi, FPS & Nhiệt độ Thiết bị

| ID | Nội dung kiểm tra | Tiêu chuẩn chấp nhận | Thiết bị 1 | Thiết bị 2 | Ghi chú kỹ thuật |
|:---:|---|---|:---:|:---:|---|
| **DEV-09** | Độ mượt mà FPS trong màn chơi bình thường | FPS duy trì ổn định, không có hiện tượng giật cục (micro-stuttering) khi nhân vật chạy nhảy qua chướng ngại vật. | `[ ]` | `[ ]` | FPS trung bình: TB1: ____ / TB2: ____ |
| **DEV-10** | Hiệu năng tại màn có nhiều vật thể vật lý (Level 9, Level 20, Level 21) | Khi tương tác kéo thả khối đất, kích hoạt hộp rơi hoặc phân thân (Clone), FPS không bị sụt giảm đột ngột (drop < 5 khung hình). | `[ ]` | `[ ]` | Đoạn cao điểm vật lý 2D |
| **DEV-11** | Thời gian nạp scene giữa các màn chơi | Chuyển cảnh từ `WinCanvas` sang màn tiếp theo hoặc tải lại sau khi chết diễn ra trong nháy mắt (< 1.5 giây), không bị khựng đen màn hình. | `[ ]` | `[ ]` | Thời gian trung bình: ____ giây |
| **DEV-12** | Quản lý bộ nhớ RAM & Không rò rỉ (Memory Leak) | Chơi liên tục từ Level 1 đến Level 10; bộ nhớ RAM chiếm dụng ổn định quanh mức cho phép, không tăng liên tục dẫn đến bị Android Low Memory Killer đóng app. | `[ ]` | `[ ]` | RAM tối đa quan sát: ____ MB |
| **DEV-13** | Tỏa nhiệt và mức tiêu thụ pin | Chơi game liên tục trong 20 phút; thân máy chỉ ấm nhẹ (< 40°C), pin sụt giảm ở mức thông thường của game 2D nhẹ (< 4% / 20 phút). | `[ ]` | `[ ]` | Đánh giá nhiệt độ: Mát / Ấm / Nóng |

---

### Hạng mục 4: Tương tác Cảm ứng & Nút bấm Đa điểm (Multi-touch)

| ID | Nội dung kiểm tra | Tiêu chuẩn chấp nhận | Thiết bị 1 | Thiết bị 2 | Ghi chú kỹ thuật |
|:---:|---|---|:---:|:---:|---|
| **DEV-14** | Thao tác đa điểm đồng thời (Multi-touch) | Vừa nhấn giữ ngón tay trái trên nút "Di chuyển Phải", vừa dùng ngón tay phải nhấn nút "Nhảy": Nhân vật thực hiện cú nhảy về phía trước mượt mà, không bị nhả nút di chuyển. | `[ ]` | `[ ]` | Rất quan trọng với gameplay platformer |
| **DEV-15** | Độ nhạy cảm ứng & Vùng nhấn nút ảo (Hitbox) | Vùng phản hồi cảm ứng của các nút bấm trên màn hình đủ lớn, bấm nhẹ là ăn ngay, không bị hụt tay khi thao tác nhanh. | `[ ]` | `[ ]` | Kiểm tra kích thước RectTransform nút |
| **DEV-16** | Thao tác Kéo & Thả (Drag & Drop) | Ngón tay kéo vật thể di chuyển trơn tru trên mặt kính; khi di chuyển nhanh vật thể không bị văng khỏi ngón tay; thả tay là nhận diện đúng tọa độ. | `[ ]` | `[ ]` | Kiểm tra trên màn hình 60Hz và 90Hz |

---

### Hạng mục 5: Quản lý Vòng đời Ứng dụng (App Lifecycle Interruptions)

| ID | Nội dung kiểm tra | Tiêu chuẩn chấp nhận | Thiết bị 1 | Thiết bị 2 | Ghi chú kỹ thuật |
|:---:|---|---|:---:|:---:|---|
| **DEV-17** | Nhận cuộc gọi đến hoặc thông báo khẩn | Đang chơi game có cuộc gọi đến (hoặc chuông báo thức): Game tự động tạm dừng âm thanh; sau khi từ chối cuộc gọi quay lại game không bị crash, âm thanh phục hồi đúng trạng thái. | `[ ]` | `[ ]` | Sự kiện `OnApplicationPause(true/false)` |
| **DEV-18** | Nhấn phím Home / Vuốt về màn hình chính | Đang ở giữa màn chơi, bấm Home đưa app về background: Sau 30 giây mở lại game từ menu đa nhiệm (Recent Apps), trạng thái game giữ nguyên, không bị khởi động lại từ đầu. | `[ ]` | `[ ]` | Kiểm tra bảo toàn dữ liệu RAM |
| **DEV-19** | Khóa màn hình thiết bị (Screen Lock) | Bấm nút nguồn khóa máy khi đang chơi: Mở khóa lại, trò chơi vẫn hiển thị đúng khung cảnh cũ, không bị lỗi đen màn hình (Black Screen bug). | `[ ]` | `[ ]` | Đánh giá phục hồi đồ họa OpenGL / Vulkan |

---

### Hạng mục 6: Xử lý Mạng, Mất kết nối & Chuyển vùng (Network Resilience)

| ID | Nội dung kiểm tra | Tiêu chuẩn chấp nhận | Thiết bị 1 | Thiết bị 2 | Ghi chú kỹ thuật |
|:---:|---|---|:---:|:---:|---|
| **DEV-20** | Khởi động game ở Chế độ Máy bay (Hoàn toàn Offline) | Bật Airplane Mode trước khi mở app: Game khởi động trơn tru, không bị đơ chờ mạng; dữ liệu chơi local hoạt động 100%. | `[ ]` | `[ ]` | Kiểm tra fallback `LocalProgressStore` |
| **DEV-21** | Chuyển đổi mạng khi đang chơi (Wi-Fi ↔ 4G) | Đang chơi game thì tắt Wi-Fi chuyển sang 4G (hoặc ngược lại): Game không bị khựng; hệ thống Cloud Save tự động nhận diện lại kết nối. | `[ ]` | `[ ]` | Không văng Exception mạng |
| **DEV-22** | Mất mạng khi chạm cờ chiến thắng | Đang chơi thì ngắt toàn bộ mạng rồi chạm cờ: WinPanel vẫn hiện bình thường, sao và tiến trình được lưu vào Local; khi bật mạng lại tự đồng bộ lên Cloud. | `[ ]` | `[ ]` | Kiểm tra cờ `CloudProgressPending` |

---

## 4. Mẫu Bảng Ghi nhận Lỗi trên Thiết bị (Device Defect Report Template)

Khi phát hiện sự cố trong quá trình thực hiện checklist trên thiết bị thật, Tester điền thông tin chi tiết vào bảng dưới đây:

### Danh sách Sự cố ghi nhận (Hardware Defect Log)

| Mã Bug | Tên tóm tắt lỗi | Thiết bị & OS | Mức độ | Trạng thái | Người báo | Ngày phát hiện |
|:---:|---|---|:---:|:---:|:---:|:---:|
| **BUG-DEV-01** | *(Ví dụ) Lệch nút Nhảy che lẹm vào phím Home cử chỉ trên Android 13* | Redmi Note 11 (Android 13) | Major | Đang sửa | Antigravity | 21/10/2026 |
| **BUG-DEV-02** | | | | | | |

---

### Phiếu Mô tả Lỗi Chi tiết (Mẫu chuẩn điền kèm logcat)

```markdown
### [BUG-DEV-xx] <Tên sự cố ngắn gọn, súc tích>
- **Thiết bị thử nghiệm:** [Tên máy, ví dụ: Samsung Galaxy A12]
- **Phiên bản hệ điều hành:** Android 11 (OneUI Core 3.1)
- **Độ phân giải & Màn hình:** 720 x 1600 (20:9, Giọt nước)
- **Bản build:** APK Release v1.0 (Commit: `abcdef12`)
- **Mức độ nghiêm trọng (Severity):** 
  - [ ] Blocker (Crash app / Đóng băng hoàn toàn)
  - [ ] Critical (Mất dữ liệu / Không qua được màn)
  - [ ] Major (Ảnh hưởng trải nghiệm chính: drop FPS nặng, lệch UI che nút)
  - [ ] Minor (Lỗi thẩm mỹ / Lệch nhẹ font chữ)
- **Tần suất xuất hiện:** Luôn luôn (100%) / Thỉnh thoảng (~50%) / Hiếm gặp (1 lần)

**1. Các bước tái hiện lỗi (Steps to Reproduce):**
1. Mở ứng dụng từ icon trên màn hình chính.
2. Vào màn hình `LevelSelect`, chọn Chương 6.
3. Bấm vào `Level 21`.
4. Quan sát vị trí cụm nút điều khiển ở góc dưới bên phải.

**2. Kết quả thực tế (Actual Result):**
- Nút Nhảy bị thụt xuống sát cạnh dưới đáy màn hình, khi ngón tay cái bấm nhảy thì hệ thống Android hiểu nhầm là thao tác vuốt thanh Gesture Bar, làm thu nhỏ app về màn hình Home.

**3. Kết quả mong đợi (Expected Result):**
- Cụm nút bấm phải tuân thủ vùng đệm an toàn (`SafeArea`), cách mép dưới màn hình tối thiểu 30-40 pixels để người dùng thao tác thoải mái.

**4. Dữ liệu đính kèm (Evidence):**
- Ảnh chụp màn hình: `docs/screenshots/bug_dev_01_safearea.png`
- Trích xuất Logcat liên quan:
  ```text
  [Android/Input] Touch event intercepted by NavigationGesture
  ```
```
