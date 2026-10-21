# DANH SÁCH CA KIỂM THỬ CHI TIẾT (TEST CASES SPECIFICATION)

> **Dự án:** Mr. Eggy Puzzle Game — Đồ án chuyên ngành IT6122  
> **Tài liệu bàn giao:** Task W2-10 (Agent Antigravity phụ trách)  
> **Cấu trúc mỗi ca kiểm thử:** Mã ID, Mô tả, Điều kiện đầu (Preconditions), Các bước thực hiện (Steps), Dữ liệu kiểm thử (Test Data), Kết quả mong đợi (Expected Results), Loại kỹ thuật áp dụng (Testing Technique).

---

## 1. Nhóm ca kiểm thử: Luật tính 3 sao (TC-STAR)

| Mã ID | Mô tả ca kiểm thử | Điều kiện đầu | Các bước thực hiện | Dữ liệu thử | Kết quả mong đợi | Loại kỹ thuật |
|---|---|---|---|---|---|:---:|
| **TC-STAR-01** | Qua màn hoàn hảo: Không chết, không xem gợi ý | Người chơi ở Level 1, phiên chơi mới (`Attempt = 1`, `Deaths = 0`, `HintsUsed = 0`) | 1. Di chuyển nhân vật né tránh chướng ngại vật.<br>2. Chạm vào lá cờ đích vàng (`YellowFlag`). | `completed = true`<br>`skipped = false`<br>`deaths = 0`<br>`hints = 0` | - Hiển thị WinPanel.<br>- Đạt **3 sao** rực rỡ.<br>- `LevelManager.LastStars = 3`.<br>- Lưu 3 sao vào `ProgressService`. | Bảng quyết định (Rule 3) |
| **TC-STAR-02** | Qua màn có chết: Chết đúng 1 lần, không xem gợi ý | Vào Level 1 | 1. Nhảy vào chướng ngại vật để chết 1 lần (`Deaths = 1`).<br>2. Scene nạp lại, chơi tiếp đến khi chạm cờ đích. | `completed = true`<br>`skipped = false`<br>`deaths = 1`<br>`hints = 0` | - Hiển thị WinPanel.<br>- Mất sao không chết -> Đạt **2 sao**.<br>- `LevelManager.LastStars = 2`. | Phân tích giá trị biên (`deaths = 1`) |
| **TC-STAR-03** | Qua màn có chết nhiều lần: Chết 5 lần, không xem gợi ý | Vào Level 1 | 1. Cố tình chết 5 lần liên tiếp.<br>2. Ở lần thứ 6, chạm cờ đích qua màn. | `completed = true`<br>`skipped = false`<br>`deaths = 5`<br>`hints = 0` | - Đạt **2 sao** (vẫn giữ được sao không dùng hint).<br>- `LevelManager.LastStars = 2`. | Phân vùng tương đương (`deaths > 0`) |
| **TC-STAR-04** | Qua màn có xem gợi ý: Không chết, xem gợi ý 1 lần | Vào Level 1 | 1. Bấm nút Hint trên HUD -> Xem nội dung gợi ý.<br>2. Đóng bảng gợi ý, tự giải đố chạm cờ đích. | `completed = true`<br>`skipped = false`<br>`deaths = 0`<br>`hints = 1` | - Mất sao không hint -> Đạt **2 sao**.<br>- `LevelManager.LastStars = 2`. | Bảng quyết định (Rule 4) |
| **TC-STAR-05** | Qua màn vừa chết vừa xem gợi ý | Vào Level 1 | 1. Cố tình chết 2 lần.<br>2. Bấm mở xem nội dung gợi ý 1 lần.<br>3. Chạm cờ đích qua màn. | `completed = true`<br>`skipped = false`<br>`deaths = 2`<br>`hints = 1` | - Mất cả 2 sao điều kiện -> Đạt **1 sao** danh dự.<br>- `LevelManager.LastStars = 1`. | Bảng quyết định (Rule 6) |
| **TC-STAR-06** | Qua màn bằng nút "Nhảy" (Bỏ qua màn) | Đang chơi Level 1, chưa chết | 1. Bấm nút Hint.<br>2. Trên Canvas 2, bấm nút "Nhảy" (`OnClickNhay_Canvas2`). | `completed = true`<br>`skipped = true`<br>`deaths = 0`<br>`hints = 0` | - Thắng ngay lập tức nhưng chỉ nhận **1 sao**.<br>- `LevelManager.LastStars = 1`. | Bảng quyết định (Rule 2) |
| **TC-STAR-07** | Chơi lại màn cũ với kết quả thấp hơn | Level 1 đã đạt kỷ lục 3 sao trong `ProgressService` | 1. Chơi lại Level 1.<br>2. Cố tình chết 3 lần và xem hint rồi qua màn. | `oldStars = 3`<br>`newRunStars = 1` | - `LevelManager.LastStars = 1` (phản ánh đúng lượt chơi).<br>- `ProgressService.GetStars(1)` **vẫn giữ nguyên 3 sao** (không bị hạ thành tích). | Phân tích tính bền vững (Immutability) |

---

## 2. Nhóm ca kiểm thử: Tiến trình mở khóa màn chơi (TC-PROG)

| Mã ID | Mô tả ca kiểm thử | Điều kiện đầu | Các bước thực hiện | Dữ liệu thử | Kết quả mong đợi | Loại kỹ thuật |
|---|---|---|---|---|---|:---:|
| **TC-PROG-01** | Trạng thái người chơi mới mở game lần đầu | Ứng dụng mới cài đặt hoặc vừa xóa sạch dữ liệu | 1. Khởi động game.<br>2. Vào màn hình `LevelSelect`. | Dữ liệu trắng | - `ProgressService.MaxUnlockedLevel = 1`.<br>- Chỉ có nút Level 1 được mở (sáng màu xanh/tím).<br>- Các nút từ Level 2 trở đi đều ở trạng thái Khóa (hiện ổ khóa). | Kiểm thử giá trị khởi tạo |
| **TC-PROG-02** | Mở khóa tuần tự màn tiếp theo khi thắng | Đang ở Level 1, `MaxUnlockedLevel = 1` | 1. Chơi và hoàn thành Level 1.<br>2. Quay về màn hình `LevelSelect`. | Thắng Level 1 | - `ProgressService.MaxUnlockedLevel = 2`.<br>- Nút Level 2 chuyển từ Khóa sang Mở khóa.<br>- Nút Level 1 chuyển sang trạng thái Đã hoàn thành (Completed). | Kiểm thử chuyển trạng thái |
| **TC-PROG-03** | Lưu trữ tên màn chơi tiếp theo (`savedLevel`) | Đang ở Level 3 (`nextLevelName = "Level 4"`) | 1. Hoàn thành Level 3.<br>2. Đóng ứng dụng hoàn toàn.<br>3. Mở lại ứng dụng, bấm nút "Chơi tiếp" ở Menu chính. | `nextLevelName = "Level 4"` | - Game tự động nạp thẳng vào scene `Level 4`.<br>- `ProgressService.SavedLevel = "Level 4"`. | Kiểm thử tính bền vững lưu trữ |
| **TC-PROG-04** | Chơi lại màn cũ không làm thụt lùi tiến trình | `MaxUnlockedLevel = 10` | 1. Chọn chơi lại Level 2.<br>2. Hoàn thành Level 2.<br>3. Kiểm tra lại `ProgressService.MaxUnlockedLevel`. | Level hiện tại = 2 | - `MaxUnlockedLevel` **vẫn giữ nguyên là 10**.<br>- Không bị reset về 3. | Phân tích biên logic |
| **TC-PROG-05** | Ngăn chặn truy cập vào màn chưa được mở khóa | Đang mở khóa tới Level 3 | 1. Tại `LevelSelect`, bấm vào nút `LevelBtn_5`. | Bấm vào nút bị khóa | - Game không nạp scene `Level 5`.<br>- Console xuất log: *"Level 5 chưa được mở khóa!"*.<br>- Người chơi vẫn ở lại màn hình LevelSelect. | Kiểm thử biên bảo vệ |
| **TC-PROG-06** | Xóa tiến trình qua menu Test Editor | Đang mở khóa đến Level 20, có nhiều sao | 1. Trong Unity Editor, chọn `Tools -> Test -> Xóa tiến trình chơi`.<br>2. Vào màn hình `LevelSelect`. | Lệnh xóa tiến trình | - Toàn bộ PlayerPrefs tiến trình bị xóa.<br>- `MaxUnlockedLevel = 1`, `SavedLevel = "Level 1"`.<br>- Tất cả số sao đưa về 0. | Kiểm thử tính năng tiện ích (Dev Tools) |

---

## 3. Nhóm ca kiểm thử: Hệ thống Gợi ý (TC-HINT)

| Mã ID | Mô tả ca kiểm thử | Điều kiện đầu | Các bước thực hiện | Dữ liệu thử | Kết quả mong đợi | Loại kỹ thuật |
|---|---|---|---|---|---|:---:|
| **TC-HINT-01** | Người chơi Free bấm nút Hint lần đầu trong màn | Tài khoản Free (`isPremium = false`), chưa mở hint màn này | 1. Vào màn chơi bất kỳ.<br>2. Bấm vào biểu tượng bóng đèn trên HUD. | `isPremium = false`<br>`unlockedThisLevel = false` | - Trục thời gian đóng băng (`Time.timeScale = 0`).<br>- `hintCanvas1` (hỏi xem Ads) hiện lên.<br>- `hintCanvas2` vẫn ẩn. | Bảng quyết định (Rule 1) |
| **TC-HINT-02** | Người chơi Free từ chối xem quảng cáo | Đang mở `hintCanvas1` | 1. Bấm nút dấu "X" (Đóng) trên `hintCanvas1`. | Bấm nút đóng | - `hintCanvas1` đóng lại (`activeSelf = false`).<br>- Thời gian phục hồi bình thường (`Time.timeScale = 1.0`). | Kiểm thử giao diện luồng hủy |
| **TC-HINT-03** | Người chơi Free đồng ý xem quảng cáo thành công | Đang mở `hintCanvas1` | 1. Bấm nút "CÓ" (`OnClickCo_Canvas1`).<br>2. Chờ quảng cáo hoàn thành và cấp thưởng. | Xem Ads thành công | - `hintCanvas1` đóng.<br>- `hintCanvas2` hiện lên với nội dung câu đố tương ứng của màn.<br>- Đánh dấu `isHintUnlockedCurrentLevel = true`.<br>- Bộ đếm `LevelSession.HintsUsed` tăng 1. | Kiểm thử luồng thành công (Happy Path) |
| **TC-HINT-04** | Người chơi Free bấm lại Hint lần 2 trong cùng màn | Đã xem Ads mở khóa thành công ở bước trước | 1. Đóng `hintCanvas2` để chơi tiếp.<br>2. Bấm lại nút Hint trên HUD một lần nữa. | `unlockedThisLevel = true` | - **Mở thẳng `hintCanvas2`**, không còn hiện bảng hỏi xem Ads `hintCanvas1` nữa. | Bảng quyết định (Rule 3) |
| **TC-HINT-05** | Tài khoản Premium bấm nút Hint | Đã mua Premium (`isPremium = true`) | 1. Bấm nút Hint trên HUD ở bất kỳ màn nào. | `isPremium = true` | - Bỏ qua hoàn toàn phễu quảng cáo.<br>- **Mở thẳng `hintCanvas2` ngay lập tức**. | Bảng quyết định (Rule 2) |
| **TC-HINT-06** | Hiển thị nội dung gợi ý chuẩn xác theo tên Scene | Cấu hình database có hint cho "Level 2" | 1. Vào scene "Level 2".<br>2. Mở `hintCanvas2`. | Tên scene = "Level 2" | - Text hiển thị đúng chuỗi `hintMessage` trong `GameHintData.asset`.<br>- Nếu scene không có trong database -> hiện chuỗi mặc định `HintService.DefaultHint`. | Kiểm thử ánh xạ dữ liệu |

---

## 4. Nhóm ca kiểm thử: Gộp tiến trình Cloud Save (TC-SYNC)

| Mã ID | Mô tả ca kiểm thử | Điều kiện đầu | Các bước thực hiện | Dữ liệu thử | Kết quả mong đợi | Loại kỹ thuật |
|---|---|---|---|---|---|:---:|
| **TC-SYNC-01** | Gộp khi Cloud tiến xa hơn Local (Đổi máy mới) | Local mới mở Level 1; Cloud đã chơi đến Level 8 (được 20 sao) | 1. Đăng nhập ẩn danh trên thiết bị mới.<br>2. Tải dữ liệu đám mây về.<br>3. Gọi `ProgressService.MergeFrom(cloudData)`. | `local.max = 1`<br>`remote.max = 8` | - `ProgressService.MaxUnlockedLevel` nhảy lên 8.<br>- Toàn bộ 20 sao của các màn được đồng bộ về máy.<br>- Bắn sự kiện `Changed` cập nhật giao diện. | Bảng quyết định (Rule 3) |
| **TC-SYNC-02** | Gộp khi Local chơi offline tiến xa hơn Cloud | Máy chơi offline lên Level 12; Cloud lưu cũ ở Level 7 | 1. Bật mạng kết nối lại dịch vụ.<br>2. Tải cloud và kích hoạt lệnh gộp. | `local.max = 12`<br>`remote.max = 7` | - `MaxUnlockedLevel` giữ nguyên là 12.<br>- `savedLevel` giữ nguyên của Local.<br>- Cập nhật tiến trình mới này ngược lên Cloud. | Bảng quyết định (Rule 4) |
| **TC-SYNC-03** | Gộp sao đan xen từng màn (Cross-Level Stars Merge) | Local: L1=3 sao, L2=1 sao.<br>Remote: L1=2 sao, L2=3 sao, L3=2 sao. | 1. Kích hoạt `ProgressMerger.Merge(local, remote)`. | Dữ liệu đan xen | Kết quả sau gộp lấy max từng màn:<br>- Level 1 = **3 sao** (từ Local).<br>- Level 2 = **3 sao** (từ Remote).<br>- Level 3 = **2 sao** (từ Remote). | Kiểm thử ma trận dữ liệu |
| **TC-SYNC-04** | Xung đột hòa tiến trình (Equal Progress Conflict) | Local: Level 5, `savedLevel = "Level 5 Local"`.<br>Remote: Level 5, `savedLevel = "Level 5 Cloud"`. | 1. Tiến hành gộp dữ liệu. | `local.max == remote.max = 5` | - Luật hòa: Giữ nguyên `savedLevel` của **Local** ("Level 5 Local"). | Bảng quyết định (Rule 5) |
| **TC-SYNC-05** | Xử lý an toàn khi dữ liệu 1 bên bị Null | Một bên truyền vào `null` do lỗi mạng hoặc tài khoản mới | 1. Gọi `ProgressMerger.Merge(local, null)` hoặc `Merge(null, remote)`. | Có tham số `null` | - Hàm không văng Exception NullReferenceException.<br>- Trả về bản sao an toàn của bên còn lại. | Kiểm thử tính bền bỉ (Robustness) |

---

## 5. Nhóm ca kiểm thử: Mất mạng và Phục hồi Offline (TC-NET)

| Mã ID | Mô tả ca kiểm thử | Điều kiện đầu | Các bước thực hiện | Dữ liệu thử | Kết quả mong đợi | Loại kỹ thuật |
|---|---|---|---|---|---|:---:|
| **TC-NET-01** | Chơi game hoàn toàn khi không có mạng (Offline Mode) | Thiết bị tắt toàn bộ Wi-Fi và Dữ liệu di động (4G) | 1. Mở game từ màn hình điện thoại.<br>2. Chơi từ Level 1 đến Level 3. | Không có kết nối Internet | - Game khởi động bình thường, không bị treo ở màn hình Splash/Start.<br>- Hệ thống tự động chuyển sang dùng `LocalProgressStore`.<br>- Vượt màn vẫn mở khóa Level 2, Level 3 mượt mà trong Local. | Kiểm thử chế độ ngắt kết nối |
| **TC-NET-02** | Đang chơi bị rớt mạng đột ngột lúc thắng màn | Đang chơi màn, kết nối Wi-Fi bị ngắt | 1. Chạm cờ thắng màn ngay khi mạng vừa mất.<br>2. Nhận sao và bấm sang màn tiếp theo. | Mất mạng tức thời | - WinPanel vẫn hiện bình thường.<br>- Tiến trình được ghi an toàn vào PlayerPrefs local.<br>- Không xảy ra lỗi crash ứng dụng. | Kiểm thử đột biến môi trường |
| **TC-NET-03** | Phục hồi kết nối mạng và tự động đồng bộ bù | Đã chơi offline vượt qua 3 màn | 1. Bật lại Wi-Fi.<br>2. Chuyển scene hoặc bấm vào LevelSelect. | Có mạng trở lại | - Game phát hiện kết nối mạng thành công.<br>- Dữ liệu local tự động đẩy lên Cloud Save mà không làm mất 3 màn vừa vượt qua. | Kiểm thử tự phục hồi (Recovery) |
| **TC-NET-04** | Gọi Analytics khi không có mạng | Đang offline | 1. Nhân vật chết hoặc qua màn. | Offline | - Các lệnh gọi `GameAnalytics.Reporter.*` được bọc an toàn, không văng lỗi đỏ lên màn hình.<br>- Dữ liệu sự kiện được lưu tạm vào hàng đợi (Queue) chờ gửi lại khi có mạng. | Kiểm thử chịu lỗi (Fault-Tolerance) |

---

## 6. Nhóm ca kiểm thử: Thanh chương cuộn ngang & Giao diện LevelSelect (TC-SEL)

| Mã ID | Mô tả ca kiểm thử | Điều kiện đầu | Các bước thực hiện | Dữ liệu thử | Kết quả mong đợi | Loại kỹ thuật |
|---|---|---|---|---|---|:---:|
| **TC-SEL-01** | Bấm chọn chuyển đổi giữa các Chương (Chapter 1 -> Chapter 7) | Đang ở màn hình `LevelSelect`, mặc định chọn Chương 1 | 1. Bấm nút `ChapterBtn_2`.<br>2. Bấm tiếp `ChapterBtn_6`.<br>3. Bấm tiếp `ChapterBtn_7`. | Chọn chương 1 -> 7 | - Panel chương được chọn chuyển sang `activeSelf = true`, các panel khác tắt đi.<br>- Nút chương được chọn chuyển sang màu sáng (`activeColor = Trắng`).<br>- Các nút chương còn lại chuyển sang màu xám tối (`inactiveColor`). | Kiểm thử tương tác giao diện |
| **TC-SEL-02** | Hiển thị màu sắc nút Level theo quy chuẩn xanh / tím | Có 28 màn (7 chương x 4 màn) | 1. Mở xem lần lượt các chương. | Màn thường vs Màn kết chương | - Các màn 4, 8, 12, 16, 20, 24, 28 (chia hết cho 4): Dùng bộ Sprite **Tím** (`purpleLocked`, `purpleUnlocked`, `purpleCompleted`).<br>- Các màn còn lại: Dùng bộ Sprite **Xanh lá** (`greenLocked`, `greenUnlocked`, `greenCompleted`). | Kiểm thử kiểm tra quy tắc hiển thị |
| **TC-SEL-03** | Ẩn toàn bộ các đường nối đen (Arrows) khi vào màn hình | Trong Scene có chứa các Image tên "Arrow" | 1. Mở Scene `LevelSelect`. | Tự động chạy trong `Start()` | - Tất cả GameObject Image có tên chứa chữ "Arrow" đều bị tắt (`activeSelf = false`), giao diện sạch sẽ, không lộ vết nối thô. | Kiểm thử tính toàn vẹn UI |
| **TC-SEL-04** | Cập nhật tức thời số sao trên nút Level khi có thay đổi | Đang đứng ở màn hình `LevelSelect` | 1. Dữ liệu cloud đồng bộ về làm thay đổi số sao của Level 1 từ 1 sao lên 3 sao. | Bắn sự kiện `ProgressService.Changed` | - Cụm component `StarDisplay` trên nút Level 1 lập tức chuyển sang sáng 3 sao mà **không cần người chơi phải thoát ra vào lại scene**. | Kiểm thử tính phản ứng (Event-Driven UI) |
| **TC-SEL-05** | Bật/Tắt cửa sổ Shop từ thanh Menu trên LevelSelect | Đang ở `LevelSelect` | 1. Bấm nút Shop trên TopBar.<br>2. Bấm nút Đóng trên cửa sổ Shop. | Toggle Shop | - Bước 1: `shopCanvas` kích hoạt hiện lên màn hình.<br>- Bước 2: `shopCanvas` đóng an toàn, trả lại màn hình LevelSelect. | Kiểm thử Popup Modal |
| **TC-SEL-06** | Bật/Tắt bảng Cài đặt (Settings) | Đang ở `LevelSelect` | 1. Bấm biểu tượng bánh răng Cài đặt.<br>2. Bấm nút Tắt Cài đặt. | Toggle Settings | - `panelSetting` bật/tắt chính xác, các nút Bật/Tắt Âm thanh và Nhạc phản hồi đúng trạng thái. | Kiểm thử Popup Modal |

---

## 7. Nhóm ca kiểm thử: Công cụ tạo màn tự động (TC-TOOL)

| Mã ID | Mô tả ca kiểm thử | Điều kiện đầu | Các bước thực hiện | Dữ liệu thử | Kết quả mong đợi | Loại kỹ thuật |
|---|---|---|---|---|---|:---:|
| **TC-TOOL-01** | Tạo tự động 1 Level mới đơn lẻ (`CreateNewLevelTool`) | Game đang có đến `Level 20` | 1. Mở menu `Tools -> Tạo Level mới`.<br>2. Chọn scene mẫu (Level 20).<br>3. Nhập số màn cần tạo = 1.<br>4. Nhập Hint = "Hãy kéo chiếc hộp!".<br>5. Bấm nút "Tạo 1 màn mới". | `levelCount = 1`<br>Scene mẫu = Level 20 | - Tạo mới file scene `Assets/_Project/Scenes/Level 21.unity`.<br>- Tự động thêm `Level 21` vào `EditorBuildSettings`.<br>- Nối `nextLevelName` của Level 20 thành "Level 21".<br>- Thêm mục Level 21 vào `LevelDatabase.asset` với đúng nội dung hint.<br>- Thêm nút `LevelBtn_21` vào scene `LevelSelect`. | Kiểm thử tự động hóa quy trình (Automation Tool) |
| **TC-TOOL-02** | Tạo hàng loạt nhiều màn mới vượt qua Chương mới | Đang có 20 màn (5 chương) | 1. Nhập số màn cần tạo = 8 (từ Level 21 đến Level 28).<br>2. Thiết lập số màn mỗi chương = 4.<br>3. Bấm nút Tạo màn. | `levelCount = 8`<br>`levelsPerChapter = 4` | - Tạo đủ 8 scene từ `Level 21.unity` đến `Level 28.unity`.<br>- Tự động tạo thêm `Panel_Chapter_6` và `Panel_Chapter_7` trong `LevelSelect`.<br>- Tạo thêm 2 nút chuyển chương `ChapterBtn_6`, `ChapterBtn_7`.<br>- Nối logic `nextLevelName` xuyên suốt từ 20 -> 21 -> ... -> 28 -> LevelSelect. | Kiểm thử tải và mở rộng (Scalability Tooling) |
| **TC-TOOL-03** | Khóa thao tác khi đang ở chế độ PlayMode | Unity Editor đang ấn nút Play (game đang chạy) | 1. Mở menu `Tools -> Tạo Level mới`.<br>2. Thử bấm tạo màn. | `EditorApplication.isPlaying = true` | - Tool hiện thông báo cảnh báo.<br>- Chặn hoàn toàn thao tác tạo màn để tránh làm hỏng scene hoặc rò rỉ bộ nhớ. | Kiểm thử điều kiện an toàn |
| **TC-TOOL-04** | Kiểm tra toàn vẹn liên kết Scene cuối cùng | Vừa tạo xong Level 28 | 1. Mở scene `Level 28.unity`.<br>2. Kiểm tra Inspector của `LevelManager`. | Màn cuối cùng của game | - `nextLevelName` của Level 28 phải được nối về `"LevelSelect"` (không trỏ vào Level 29 không tồn tại). | Kiểm thử liên kết biên (Edge-case Linking) |
| **TC-TOOL-05** | Công cụ mở khóa nhanh toàn bộ Level để test (`DevProgressTools`) | Đang ở Level 1 | 1. Trên thanh Menu chọn `Tools -> Test -> Mở khóa tất cả Level`.<br>2. Vào scene `LevelSelect`. | Lệnh `UnlockAllLevels()` | - `ProgressService.MaxUnlockedLevel` tự động nhảy lên màn cao nhất có trong Build Settings.<br>- Tất cả các nút màn chơi trong game đều được mở khóa để đội ngũ QA kiểm thử tức thì. | Kiểm thử hỗ trợ QA (Testability) |
