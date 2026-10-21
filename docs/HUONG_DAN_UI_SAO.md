# HƯỚNG DẪN GẮN UI SAO VÀO GAME (STARDISPLAY)

Tài liệu hướng dẫn dành cho **Người dùng** thực hiện thao tác thiết lập giao diện UI hiển thị 3 ngôi sao trên Unity Editor (`6000.6.6f1`).

---

## 1. Giới thiệu Script `StarDisplay.cs`

Script [`Assets/_Project/Scripts/UI/StarDisplay.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/UI/StarDisplay.cs) là một `MonoBehaviour` linh hoạt quản lý cụm 3 ngôi sao đánh giá thành tích người chơi:
- **Chế độ `WinPanel`**: Gắn trên bảng chúc mừng chiến thắng (WinPanel / WinCanvas) của màn chơi. Khi bảng hiện lên (`OnEnable`), script tự động truy vấn `LevelManager.LastStars` để làm sáng số sao tương ứng (1, 2 hoặc 3 sao).
- **Chế độ `LevelSelectButton`**: Gắn trên từng nút chọn màn tại scene `LevelSelect`. Script đọc kỷ lục sao từ `ProgressService.GetStars(levelId)` và tự động cập nhật ngay khi nhận sự kiện `ProgressService.Changed` (ví dụ sau khi đồng bộ Cloud Save).

Hỗ trợ cả 2 cách hiển thị đồ họa:
1. Đổi Sprite giữa sao sáng (`starActiveSprite`) và sao tối (`starInactiveSprite`).
2. Tự động đổi màu Tint (`activeColor = Trắng/Vàng` và `inactiveColor = Xám mờ 40%`) nếu chỉ dùng 1 Sprite duy nhất (`Assets/_Project/Sprites/UI/level_button/star.png`).

---

## 2. Hướng dẫn gắn vào Bảng chiến thắng (WinPanel) trong Scene Màn chơi

Áp dụng cho các Scene từ `Level 1` đến `Level 28` (hoặc mở Prefab `WinCanvas` / `WinPanel` nếu đã đóng gói):

### Bước 2.1: Tạo cụm UI hiển thị 3 ngôi sao trong WinPanel
1. Mở Scene màn chơi bất kỳ (ví dụ `Assets/_Project/Scenes/Level 1.unity`).
2. Trong cửa sổ **Hierarchy**, tìm đến đối tượng `Canvas` -> `WinPanel` (hoặc `WinCanvas`).
3. Chuột phải vào `WinPanel` -> chọn **Create Empty**, đổi tên thành `StarsContainer`.
4. Trên `StarsContainer`, thêm component **Horizontal Layout Group**:
   - *Spacing*: `15` đến `20`.
   - *Child Alignment*: `MiddleCenter`.
   - *Child Force Expand*: Bỏ tick cả `Width` và `Height`.
5. Tạo 3 GameObject con bên trong `StarsContainer`:
   - Chuột phải vào `StarsContainer` -> **UI** -> **Image**, đặt tên lần lượt là `Star_1`, `Star_2`, `Star_3`.
   - Kích thước gợi ý cho mỗi sao: `Width = 60, Height = 60`.
   - Tại trường **Source Image** của mỗi `Image`, kéo sprite: `Assets/_Project/Sprites/UI/level_button/star.png`.

```
Hierarchy ví dụ:
├── Canvas
│   ├── WinPanel (GameObject)
│   │   ├── TitleText ("VICTORY!")
│   │   ├── StarsContainer  <-- [GẮN SCRIPT StarDisplay Ở ĐÂY]
│   │   │   ├── Star_1 (Image)
│   │   │   ├── Star_2 (Image)
│   │   │   └── Star_3 (Image)
│   │   ├── NextButton
│   │   └── ReplayButton
```

### Bước 2.2: Gắn và cấu hình Script `StarDisplay`
1. Chọn GameObject `StarsContainer`.
2. Bấm **Add Component** -> tìm và chọn `StarDisplay`.
3. Cấu hình Inspector:
   - **Mode**: Chọn `WinPanel`.
   - **Star Images**: Thiết lập `Size = 3`:
     - *Element 0*: Kéo `Star_1` vào.
     - *Element 1*: Kéo `Star_2` vào.
     - *Element 2*: Kéo `Star_3` vào.
   - **Star Active Sprite**: Kéo file `Assets/_Project/Sprites/UI/level_button/star.png` vào.
   - **Star Inactive Sprite**: Để trống (script sẽ tự dùng màu `inactiveColor`) hoặc kéo sprite sao rỗng nếu có.
   - **Active Color**: Màu trắng `#FFFFFF` hoặc màu vàng `#FFD700`.
   - **Inactive Color**: Màu xám mờ `#40404066` (R: 64, G: 64, B: 64, Alpha: 100).
4. Lưu Scene (`Ctrl + S`).

---

## 3. Hướng dẫn gắn vào Nút Level trên màn hình LevelSelect

Áp dụng cho scene `Assets/_Project/Scenes/LevelSelect.unity`:

### Bước 3.1: Thêm cụm sao vào từng nút Level
1. Mở Scene `Assets/_Project/Scenes/LevelSelect.unity`.
2. Trong cửa sổ **Hierarchy**, tìm đến các nút màn chơi bên trong các Panel Chương:
   - `Panel_Chapter_1` -> `LevelBtn_1`, `LevelBtn_2`, `LevelBtn_3`, `LevelBtn_4`.
   - Tương tự cho các Chương 2, 3, 4, 5, 6, 7.
3. Bên trong mỗi GameObject `LevelBtn_X`:
   - Tạo một GameObject rỗng con tên là `Stars_Level_X`.
   - Thêm **Horizontal Layout Group** (Spacing: `5`, Alignment: `MiddleCenter`).
   - Đặt vị trí của `Stars_Level_X` nằm ở phía dưới số Level (ví dụ: `PosY = -30`).
   - Tạo 3 Image con nhỏ hơn (`Width = 22, Height = 22`): `Star_1`, `Star_2`, `Star_3`.
   - Gán `Source Image` là `Assets/_Project/Sprites/UI/level_button/star.png`.

```
Hierarchy ví dụ:
├── LevelBtn_1 (Button)
│   ├── Text (TMP) ("1")
│   └── Stars_Level_1  <-- [GẮN SCRIPT StarDisplay Ở ĐÂY]
│       ├── Star_1 (Image nhỏ)
│       ├── Star_2 (Image nhỏ)
│       └── Star_3 (Image nhỏ)
```

### Bước 3.2: Cấu hình Inspector cho từng nút
1. Chọn GameObject `Stars_Level_X`.
2. Bấm **Add Component** -> chọn `StarDisplay`.
3. Cấu hình Inspector:
   - **Mode**: Chọn `LevelSelectButton`.
   - **Level Id**: Điền đúng số thứ tự của màn chơi (ví dụ: nút `LevelBtn_1` điền `1`, `LevelBtn_21` điền `21`).
   - **Star Images**: Kéo 3 Image nhỏ `Star_1`, `Star_2`, `Star_3` vào 3 phần tử.
   - **Star Active Sprite**: Kéo `star.png`.
   - **Active Color**: Trắng/Vàng.
   - **Inactive Color**: Đen mờ / Xám mờ (Alpha 30-40%).
4. Lặp lại cho các nút còn lại và Lưu Scene (`Ctrl + S`).

---

## 4. Kiểm tra hoạt động (Verification Checklist)

1. **Kiểm tra trong Scene Màn chơi**:
   - Nhấn **Play** trong Unity Editor ở `Level 1`.
   - Hoàn thành màn chơi không chết, không xem gợi ý -> Chạm cờ -> WinPanel hiện lên -> **3 ngôi sao đều sáng rực**.
   - Chơi lại, cố tình chết 1 lần -> Vượt qua màn -> WinPanel hiện lên -> **2 ngôi sao sáng, 1 ngôi sao tối**.
   - Bấm gợi ý, sau đó bấm nút **Nhảy** (bỏ qua màn) -> WinPanel hiện lên -> **1 ngôi sao sáng, 2 ngôi sao tối**.
2. **Kiểm tra ở LevelSelect**:
   - Mở scene `LevelSelect` -> Nhấn **Play**.
   - Màn nào chưa chơi: 3 sao đều tối mờ.
   - Màn đã vượt qua: hiển thị đúng số sao kỷ lục cao nhất của màn đó.
   - Sử dụng menu `Tools -> Test -> Xóa tiến trình chơi` -> Ngay lập tức các sao trên tất cả các nút đều chuyển về trạng thái tối mờ (0 sao) mà không cần nạp lại scene!
