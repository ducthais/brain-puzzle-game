# GAME DESIGN DOCUMENT (GDD) — CHƯƠNG 6 & CHƯƠNG 7
## TÀI LIỆU THIẾT KẾ MÀN CHƠI MỞ RỘNG (LEVEL 21 – LEVEL 28)

> **Dự án:** Mr. Eggy Puzzle Game — Đồ án chuyên ngành IT6122  
> **Nhiệm vụ:** Task W1-10 (Agent Antigravity phụ trách)  
> **Nguyên tắc thiết kế tối thượng:** CHỈ sử dụng các cơ chế và component C# đã có sẵn trong `Assets/_Project/Scripts/`. Không phát sinh script cơ chế mới ngoài tầm kiểm soát.

---

## 1. Giới thiệu tổng quan 2 Chương mở rộng

Trò chơi "Mr. Eggy" thu hút người chơi nhờ phong cách giải đố phi quy ước (thinking outside the box), đánh lừa trực giác và tạo ra những tiếng cười sảng khoái khi phát hiện ra mánh khóe ("troll"). 
Hai chương mở rộng tiếp nối tinh thần đó:

```
[Hành trình 28 màn chơi]
├── Chương 1-5 (Level 1–20): Làm quen, Nhảy vô hạn, Kéo đất, Hộp vật lý, Bẫy troll cơ bản.
├── Chương 6 (Level 21–24): "Ảo Ảnh Song Trùng & Thân Xác Thí Mạng" (Clone Mechanics).
└── Chương 7 (Level 25–28): "Bẻ Cong Thứ Nguyên & Phá Vỡ Bức Tường Thứ Tư" (Reality & UI Breaker).
```

- **Chương 6 (Level 21–24) — Cơ chế chủ đạo: Nhân bản & Thân xác thế mạng (`CloneManager.cs`).** Biến cái chết hoặc lệnh reset trở thành một công cụ giải đố. Người chơi phải chủ động hy sinh các phiên bản của chính mình để tạo bậc thang, làm khiên chắn hoặc đè nút công tắc.
- **Chương 7 (Level 25–28) — Cơ chế chủ đạo: Cầu xoay trọng lực (`RotatableBridge.cs`) & Tương tác phá vỡ UI (`DraggableButton.cs`, `FakeWin_Trigger.cs`).** Bẻ gãy các quy tắc giao diện người dùng, biến các phần tử HUD thành đồ chơi vật lý và đối mặt với những cái bẫy chiến thắng giả mạo.

---

## 2. Thiết kế chi tiết Chương 6: "Ảo Ảnh Song Trùng" (Level 21 – Level 24)

### Level 21: Bậc Thang Thân Xác (The Human Ladder)
- **Độ khó dự kiến:** `2/5` (Màn nhập môn cơ chế nhân bản).
- **Ý tưởng "troll" & Tâm lý người chơi:** Trước mặt là một hố chông tử thần cực rộng, nhảy cỡ nào cũng rơi xuống hố chết. Người chơi theo thói quen sẽ cố tìm cách nhảy xa hơn hoặc tìm đường vòng. Nhưng bí mật là: mỗi lần chết/reset, một xác bản sao (`PlayerClone`) sẽ hóa đá nằm lại ngay vị trí đó.
- **Sơ đồ bố cục (ASCII Map):**
```
+-------------------------------------------------------------+
|                                                             |
|   [P]                                                  [FLAG]|
|  ====                                                 ======|
|      \                                               /      |
|       |     # # # # # # # # # # # # # # # # # #     |       |
|       |     # # # [CHÔNG TỬ THẦN - TRAP]  # # #     |       |
|       +---------------------------------------------+       |
|             (Cần 3 xác clone để làm cầu bắc qua)            |
+-------------------------------------------------------------+
```
- **Bố cục chi tiết:**
  - Vị trí Player [P]: Đứng trên mỏm đá bên trái `(x: -6, y: 0)`.
  - Hố chông: Tag `Trap`, chiều rộng 8 mét, rơi xuống chạm vào là chết ngay.
  - Cờ đích [FLAG]: Đứng trên mỏm đá bên phải `(x: 6, y: 0)`.
  - Bộ quản lý: GameObject chứa `CloneManager` nối với nút Reset trên HUD.
- **Các bước giải (Walkthrough):**
  1. Người chơi bước xuống hố chông lần 1 -> Bấm Reset -> Một clone hóa đá xuất hiện ngay mép chông.
  2. Người chơi nhảy lên lưng clone 1, nhảy tiếp về phía trước rồi bấm Reset -> Clone thứ 2 xuất hiện ở giữa hố chông.
  3. Lặp lại bước 3 -> Tạo đủ cây cầu bằng 3 thân xác clone.
  4. Người chơi thật ung dung đi trên lưng các clone bước sang bờ bên kia chạm cờ vàng.
- **Script / Prefab sử dụng:** [`CloneManager.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/CloneManager.cs), `PlayerClone.prefab`, [`PlayerDeath.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/PlayerDeath.cs), `YellowFlag`.
- **Nội dung Hint:** *"Thất bại là mẹ thành công, mà nhiều cái xác thì thành cây cầu!"*

---

### Level 22: Người Anh Em Nặng Ký (The Heavy Twin)
- **Độ khó dự kiến:** `3/5`.
- **Ý tưởng "troll" & Tâm lý người chơi:** Lá cờ vàng nằm ngay trước mắt nhưng lại có xích khóa (`YellowFlag.isLocked = true`). Cách đó một đoạn có một nút bấm vật lý (`PhysicalButton`). Khi người chơi giẫm lên nút thì cờ mở khóa, nhưng vừa bước chân rời nút để chạy lại cờ thì cờ lập tức khóa lại!
- **Sơ đồ bố cục (ASCII Map):**
```
+-------------------------------------------------------------+
|                                                             |
|   [P]                                                       |
|  ======                                                     |
|       |             [CÔNG TẮC]             [CỜ KHÓA]        |
|       |             (PHYSICAL)             (LOCKED)         |
|       |             +--------+             +-------+        |
|       |             | BUTTON |             | FLAG  |        |
|  =========================================================  |
+-------------------------------------------------------------+
```
- **Bố cục chi tiết:**
  - Vị trí Player: `(x: -5, y: 1)`.
  - Nút bấm `PhysicalButton`: Tọa độ `(x: 0, y: -1)`, có lò xo đàn hồi. Khi có vật thể đè lên thì gọi `flag.UnlockFlag()`.
  - Cờ đích `YellowFlag`: Tọa độ `(x: 5, y: -1)`, mặc định `isLocked = true`.
- **Các bước giải (Walkthrough):**
  1. Người chơi chạy đến đứng lên nút bấm `PhysicalButton` -> Cờ phát sáng mở khóa.
  2. Tại vị trí đứng trên nút, người chơi kích hoạt kỹ năng tạo bản sao (`CloneManager.CreateCloneAndRespawn`).
  3. Một clone xuất hiện đứng đè vững chắc trên nút bấm. Player thật được dịch chuyển về điểm xuất phát.
  4. Vì clone liên tục đè nút, lá cờ giữ nguyên trạng thái mở khóa. Player thật chỉ việc chạy thẳng đến chạm cờ chiến thắng.
- **Script / Prefab sử dụng:** [`CloneManager.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/CloneManager.cs), [`PhysicalButton.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/PhysicalButton.cs), [`YellowFlag.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/YellowFlag.cs).
- **Nội dung Hint:** *"Một người giữ cửa, một người đi qua. Bạn có dám để lại bản sao của mình?"*

---

### Level 23: Cơn Mưa Bẫy Rơi & Khiên Chắn Thịt (Falling Meat-Shield)
- **Độ khó dự kiến:** `3/5`.
- **Ý tưởng "troll" & Tâm lý người chơi:** Một hành lang hẹp dẫn tới cờ. Ngay phía trên trần nhà là 3 bẫy chông sắt `FallingTrap` chực chờ rơi tự do xuống với vận tốc khủng khiếp khi người chơi đi ngang qua. Tốc độ di chuyển của nhân vật không thể nào chạy thoát trước khi bẫy sập.
- **Sơ đồ bố cục (ASCII Map):**
```
+-------------------------------------------------------------+
|        [BẪY 1]            [BẪY 2]            [BẪY 3]        |
|        |FALL |            |FALL |            |FALL |        |
|        V TRAPV            V TRAPV            V TRAPV        |
|                                                             |
|   [P]                                                  [FLAG]|
|  ========================================================== |
|  (Tạo clone trên không để hứng bẫy rơi làm ô dù che chở)    |
+-------------------------------------------------------------+
```
- **Bố cục chi tiết:**
  - Các bẫy rơi `FallingTrap`: Đặt tại các tọa độ `x = -2, 1, 4` trên trần cao `y = 4`. Script `FallingTrap` có `fallSpeed = 10f`.
  - Cờ đích: Đặt ở cuối hành lang `x = 6`.
- **Các bước giải (Walkthrough):**
  1. Người chơi nhảy lên không trung ngay dưới quỹ đạo rơi của Bẫy 1 -> Kích hoạt tạo Clone giữa không trung.
  2. Bẫy 1 rơi xuống đập trúng đầu Clone và bị kẹt lại trên đầu Clone (Collider chặn nhau), tạo thành một cái mái che vững chắc.
  3. Lặp lại với các bẫy tiếp theo hoặc căn nhịp bẫy đã bị vô hiệu hóa để ung dung bước qua dưới gầm bẫy.
- **Script / Prefab sử dụng:** [`FallingTrap.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/FallingTrap.cs), [`CloneManager.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/CloneManager.cs), [`YellowFlag.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/YellowFlag.cs).
- **Nội dung Hint:** *"Đầu đội trời, chân đạp đất, clone đội bẫy chông!"*

---

### Level 24: Nghịch Lý Không Gian & Cầu Đảo Chiều (The Temporal Paradox — Màn kết Chương 6)
- **Độ khó dự kiến:** `4/5` (Đỉnh cao kết hợp cơ chế).
- **Ý tưởng "troll" & Tâm lý người chơi:** Màn chơi chia làm 2 tầng ngăn cách bởi vực thẳm. Ở giữa vực là một cây cầu xoay `RotatableBridge`. Phía trên trần có bẫy rơi, dưới sàn có công tắc khóa cờ, và cờ nằm trên đỉnh tháp cao. Người chơi phải phối hợp cả xoay cầu, tạo clone giữ cầu và né bẫy.
- **Sơ đồ bố cục (ASCII Map):**
```
+-------------------------------------------------------------+
|                                                  [FLAG-LOCK]|
|   [P]                                            +---------+|
|  =====                                           | THÁP CAO||
|       \       [ROTATABLE BRIDGE]                 |         ||
|        |         /===========/                   |         ||
|        |        /  (CẦU XOAY)                    |         ||
|        +-----------------------------------------+         ||
|             [NÚT MỞ CỜ]         [BẪY RƠI TRÊN TRẦN]        ||
|             +---------+         +-----------------+        ||
|  ========================================================== |
+-------------------------------------------------------------+
```
- **Bố cục chi tiết:**
  - `RotatableBridge`: Cây cầu xoay bắc ngang vực, người chơi có thể dùng ngón tay vuốt để xoay nghiêng góc.
  - `CloneManager`: Dùng để tạo bản sao đứng đè lên nút mở cờ ở tầng đáy.
  - `FallingTrap`: Bẫy sập bảo vệ lối leo lên tháp.
- **Các bước giải (Walkthrough):**
  1. Người chơi nhảy xuống tầng đáy, kích hoạt bẫy rơi từ xa rồi lùi lại né bẫy.
  2. Đứng lên nút bấm ở tầng đáy mở khóa cờ -> Bấm tạo Clone để Clone ở lại đè nút.
  3. Người chơi respawn về bờ trái -> Vuốt xoay cây cầu `RotatableBridge` tạo thành một con dốc nghiêng 30 độ nối thẳng từ bờ trái lên đỉnh tháp.
  4. Chạy vọt lên con dốc cầu xoay và nhảy thẳng vào lá cờ vàng đã mở khóa trên đỉnh tháp.
- **Script / Prefab sử dụng:** [`RotatableBridge.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/RotatableBridge.cs), [`CloneManager.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/CloneManager.cs), [`PhysicalButton.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/PhysicalButton.cs), [`YellowFlag.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/YellowFlag.cs).
- **Nội dung Hint:** *"Xoay cầu làm dốc, hy sinh bản sao, đỉnh tháp vẫy gọi!"*

---

## 3. Thiết kế chi tiết Chương 7: "Bẻ Cong Thứ Nguyên" (Level 25 – Level 28)

### Level 25: Bập Bênh Trọng Lực (Tilt of Faith)
- **Độ khó dự kiến:** `3/5`.
- **Ý tưởng "troll" & Tâm lý người chơi:** Giữa màn chơi là một cây cầu gỗ xoay quanh một trục cố định (`RotatableBridge`). Trên cầu có đặt một tảng đá/hộp nặng. Phía bên kia là một bức tường chông gai bảo vệ cờ. Người chơi không thể tự nhảy qua bức tường chông.
- **Sơ đồ bố cục (ASCII Map):**
```
+-------------------------------------------------------------+
|                                                             |
|   [P]            [HỘP ĐÁ]                         [FLAG]    |
|  =====            [BOX]                           ======    |
|             \==============o==============/       |CHÔNG|   |
|                 (CẦU BẬP BÊNH XOAY - TRỤC O)      |GAI  |   |
|  ========================================================== |
|  (Vuốt màn hình để nghiêng cầu, trượt hộp đá phá hủy tường) |
+-------------------------------------------------------------+
```
- **Bố cục chi tiết:**
  - `RotatableBridge`: Gắn ở tâm màn chơi, có `maxTiltAngle = 45f`, phản hồi cử chỉ vuốt kéo của chuột/ngón tay.
  - Hộp đá `PushableBox`: Có gắn `Rigidbody2D` Dynamic.
  - Tường chông: Có thể bị đẩy vỡ hoặc lấp đầy hố khi hộp đá rơi vào.
- **Các bước giải (Walkthrough):**
  1. Người chơi không vội bước lên cầu. Dùng ngón tay chạm vào nửa bên trái của cầu và vuốt xuống để nâng đầu bên phải lên.
  2. Sau đó vuốt mạnh đầu bên trái vểnh lên để cây cầu dốc ngược sang phải.
  3. Chiếc hộp đá trượt tự do theo gia tốc trọng lực lao thẳng vào tường chông phía trước, san phẳng chướng ngại vật.
  4. Người chơi xoay cầu trở lại vị trí nằm ngang và thong thả đi qua chạm cờ.
- **Script / Prefab sử dụng:** [`RotatableBridge.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/RotatableBridge.cs), [`PushableBox.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/PushableBox.cs), `YellowFlag`.
- **Nội dung Hint:** *"Cây cầu không chỉ để đi, nó còn là bệ phóng trượt dốc!"*

---

### Level 26: Chiếc Hộp Pandora & Cờ Giả Mạo (Pandora's Box & The Traitor Flag)
- **Độ khó dự kiến:** `4/5`.
- **Ý tưởng "troll" & Tâm lý người chơi:** Giữa màn chơi có một lá cờ vàng tuyệt đẹp cắm trên bệ. 99% người chơi sẽ lập tức chạy đến chạm vào lá cờ này. Tuy nhiên, lá cờ này được gắn script `FakeWinTrigger.cs`! Khi chạm vào, bảng Win giả hiện lên kèm tiếng cười trêu chọc rồi biến mất, sàn bên dưới sụp đổ và nhân vật rơi xuống vực chết.
- **Sơ đồ bố cục (ASCII Map):**
```
+-------------------------------------------------------------+
|                                                             |
|               [CỜ GIẢ - FAKE WIN]                           |
|                    [FLAG ẢO]                                |
|   [P]             +---------+              [HỘP BÍ ẨN]      |
|  =====            |BẪY SỤP  |              [MYSTERY BOX]    |
|                   +---------+              +-----------+    |
|  ========================================= | CỜ THẬT Ở |=== |
|                                            | TRONG NÀY |    |
+-------------------------------------------------------------+
```
- **Bố cục chi tiết:**
  - Cờ giả: GameObject mang sprite YellowFlag nhưng gắn component [`FakeWinTrigger.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/FakeWin_Trigger.cs) và bẫy rơi.
  - Hộp bí ẩn: GameObject gắn [`MysteryBox.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/MysteryBox.cs) hoặc [`MysterySimpleBox.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/MysterySimpleBox.cs).
- **Các bước giải (Walkthrough):**
  1. Người chơi phớt lờ lá cờ vàng hào nhoáng ở giữa phòng.
  2. Nhảy qua bệ cờ giả, tiếp cận chiếc hộp bí ẩn `MysteryBox` ở góc phòng.
  3. Dùng đầu cụng từ dưới lên hoặc bấm tương tác vào hộp -> Hộp bung ra và văng ra một lá cờ thật (`YellowFlag.cs`).
  4. Chạm vào lá cờ thật để kích hoạt chiến thắng thực sự của `LevelManager.WinGame()`.
- **Script / Prefab sử dụng:** [`FakeWin_Trigger.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/FakeWin_Trigger.cs), [`MysteryBox.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/MysteryBox.cs), [`YellowFlag.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/YellowFlag.cs).
- **Nội dung Hint:** *"Thấy vậy mà không phải vậy. Đừng tin vào ánh hoàng kim giả tạo!"*

---

### Level 27: Nút Pause Đào Tẩu (The Runaway Pause Button)
- **Độ khó dự kiến:** `4/5`.
- **Ý tưởng "troll" & Tâm lý người chơi:** Một bức tường thành cao chót vót chia đôi bản đồ, nhân vật nhảy tối đa cũng chỉ đạt được 1/3 chiều cao tường. Không có thùng gỗ, không có đất kéo. Điều kỳ lạ là: nút Pause góc trên màn hình HUD có viền vật lý rung rinh! Người chơi có thể dùng tay "bóc" nút Pause khỏi giao diện và ném vào thế giới game.
- **Sơ đồ bố cục (ASCII Map):**
```
+-------------------------------------------------------------+
| [NÚT PAUSE VẬT LÝ] <--- (Có thể kéo thả)                    |
|       \                                                     |
|        \                                           [FLAG]   |
|   [P]   \---> [KÉO NÚT PAUSE VÀO ĐÂY]      ||====  ======   |
|  ====         [LÀM BẬC THANG DẬM NHẢY]     || TƯỜNG         |
|               +----------------------+     || RẤT           |
|                                            || CAO           |
|  ========================================================== |
+-------------------------------------------------------------+
```
- **Bố cục chi tiết:**
  - Nút Pause: Được gắn script [`DraggableButton.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/DraggableButton.cs) hoặc [`DraggableUIPanel.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/DraggableUIPanel.cs), có `Rigidbody2D` Kinematic khi kéo và chuyển thành Dynamic/Solid khi thả vào thế giới game.
  - Bức tường cao 6 mét, chia cách người chơi và lá cờ vàng.
- **Các bước giải (Walkthrough):**
  1. Người chơi thử nhảy vào tường nhưng thất bại.
  2. Dùng ngón tay chạm giữ vào nút Pause trên HUD góc trên bên phải màn hình.
  3. Kéo nút Pause thả xuống vị trí lơ lửng sát chân bức tường thành. Nút Pause biến thành một khối tảng đá vật lý vững chắc.
  4. Người chơi nhảy lên nút Pause, rồi từ bề mặt nút Pause lấy đà nhảy vượt qua bức tường cao chạm cờ vàng.
- **Script / Prefab sử dụng:** [`DraggableButton.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/DraggableButton.cs), [`PlayerController.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/PlayerController.cs), `YellowFlag`.
- **Nội dung Hint:** *"Nút tạm dừng đâu nhất thiết phải nằm trên góc màn hình?"*

---

### Level 28: Hỗn Mang Thứ Nguyên (Multiverse Mayhem — Màn kết toàn bộ Game)
- **Độ khó dự kiến:** `5/5` (Đại kết cục — Thử thách trí tuệ tối thượng).
- **Ý tưởng "troll" & Tâm lý người chơi:** Màn chơi quy tụ và kết hợp TOÀN BỘ các cơ chế đã học từ Level 1 đến Level 27. Người chơi phải phối hợp nhịp nhàng: Thao túng UI, nhân bản thế mạng, cân bằng cầu xoay bập bênh và giải mã cờ khóa.
- **Sơ đồ bố cục (ASCII Map):**
```
+-------------------------------------------------------------+
| [NÚT PAUSE UI]                         [BẪY RƠI TRÊN CAO]   |
|                                        +----------------+   |
|   [P]        [CẦU XOAY ROTATABLE]      | (KHIÊN CLONE)  |   |
|  ====        /==================/      +----------------+   |
|             /                                               |
|       +----+------+                     [CỜ KHÓA DI ĐỘNG]   |
|       |NÚT ĐÈ CLONE|                    [TRÊN ĐẤT KÉO THẢ]  |
|  ========================================================== |
+-------------------------------------------------------------+
```
- **Bố cục chi tiết:**
  - `DraggableButton`: Nút Pause kéo xuống làm vật chặn góc cầu.
  - `RotatableBridge`: Cầu bập bênh xoay để chuyển hướng hộp nặng.
  - `CloneManager`: Tạo clone thế mạng chặn bẫy rơi từ trần cao.
  - `PhysicalButton`: Công tắc mở khóa cờ.
  - `DraggableGround`: Miếng đất cắm cờ có thể kéo sang trái/phải để đón người chơi.
  - `YellowFlag`: Lá cờ đích thực thụ.
- **Các bước giải (Walkthrough):**
  1. **Bước 1 (Vô hiệu bẫy):** Nhảy lên không trung tạo Clone 1 để làm khiên che vòm bẫy rơi `FallingTrap`.
  2. **Bước 2 (Mở cờ):** Đứng vào công tắc `PhysicalButton` tạo Clone 2 để duy trì mở khóa `YellowFlag`.
  3. **Bước 3 (Bắc cầu):** Gỡ nút Pause trên HUD (`DraggableButton`) đặt vào khoảng trống nối đường lên cầu xoay `RotatableBridge`.
  4. **Bước 4 (Kéo đích về):** Dùng ngón tay kéo thanh đất `DraggableGround` có cắm cờ từ tít góc xa kéo lại gần mép cầu.
  5. **Bước 5 (Về đích):** Nhảy qua nút Pause, chạy qua cầu xoay và bước nhẹ lên bờ đất chạm cờ vàng, hoàn thành toàn bộ 28 màn chơi của trò chơi "Mr. Eggy"!
- **Script / Prefab sử dụng:** [`CloneManager.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/CloneManager.cs), [`RotatableBridge.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/RotatableBridge.cs), [`DraggableButton.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/DraggableButton.cs), [`DraggableGround.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/DraggableGround.cs), [`FallingTrap.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/FallingTrap.cs), [`PhysicalButton.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/PhysicalButton.cs), [`YellowFlag.cs`](file:///D:/Repo/MrEggy-antigravity/Assets/_Project/Scripts/YellowFlag.cs).
- **Nội dung Hint:** *"Hãy vận dụng tất cả những gì bạn đã học được: Clone, Cầu xoay, Nút UI và Kéo đất!"*

---

## 4. Đường cong độ khó toàn Game (Difficulty Curve: Level 1 – Level 28)

### 4.1. Bảng phân phối độ khó và Cơ chế chính từng màn

| Level | Chương | Cơ chế chủ đạo | Độ khó (1–5) | Vai trò nhịp độ (Pacing Role) |
|:---:|:---:|---|:---:|---|
| **L01** | **Ch.1** | Di chuyển cơ bản (Trái/Phải/Nhảy) | 1 | Giới thiệu (Onboarding) |
| **L02** | Ch.1 | Cạm bẫy hố chông thụt thò | 1 | Khám phá (Discovery) |
| **L03** | Ch.1 | Bẫy gai rơi bất ngờ | 2 | Thử thách nhẹ |
| **L04** | Ch.1 | Màn kết chương: Nhảy đà tốc độ | **3** | Cao trào Chương 1 (Boss L1) |
| **L05** | **Ch.2** | Triple Jump (Nhảy 3 lần trên không) | 2 | Giới thiệu cơ chế mới |
| **L06** | Ch.2 | Nhảy liên hoàn qua vực sâu | 2 | Thực hành cơ chế |
| **L07** | Ch.2 | Căn nhịp nhảy tránh bẫy rơi | 3 | Thử thách nâng cao |
| **L08** | Ch.2 | Màn kết chương: Triple Jump + Chông di động | **3.5** | Cao trào Chương 2 |
| **L09** | **Ch.3** | Kéo thả đất (`DraggableGround`) | 2 | Phá vỡ quy tắc: Tương tác chạm |
| **L10** | Ch.3 | Kéo đất làm khiên chắn | 2.5 | Vận dụng kéo đất |
| **L11** | Ch.3 | Kéo đất nối đường trên vực thẳm | 3 | Thử thách khéo léo |
| **L12** | Ch.3 | Màn kết chương: Kéo cờ về phía nhân vật | **3.5** | Màn troll kinh điển |
| **L13** | **Ch.4** | Đẩy hộp & Vác hộp (`PlayerBoxCarrier`) | 2.5 | Giới thiệu vật lý thùng gỗ |
| **L14** | Ch.4 | Xếp chồng 2 hộp làm bậc thang | 3 | Tư duy logic khối |
| **L15** | Ch.4 | Vác hộp chặn đường lăn của đá | 3 | Căn thời gian (Timing) |
| **L16** | Ch.4 | Màn kết chương: Thả hộp vào DropZone kích hoạt cờ | **4** | Cao trào Chương 4 |
| **L17** | **Ch.5** | Bẫy troll camera biên (`PlayerDeath`) | 3 | Troll không gian |
| **L18** | Ch.5 | Cờ di động né tránh nhân vật | 3.5 | Troll đuổi bắt |
| **L19** | Ch.5 | Đảo ngược trọng lực màn chơi | 3.5 | Thay đổi tư duy |
| **L20** | Ch.5 | Màn kết chương: Hộp khổng lồ `GiantBox` vào đích | **4** | Cao trào Chương 5 |
| **L21** | **Ch.6** | Nhân bản: Xác clone làm bậc thang | 2 | Giới thiệu Clone (Thả lỏng) |
| **L22** | Ch.6 | Nhân bản: Clone đè công tắc `PhysicalButton` | 3 | Vận dụng Clone |
| **L23** | Ch.6 | Nhân bản: Clone làm khiên chắn bẫy rơi | 3 | Thử thách kết hợp |
| **L24** | Ch.6 | Màn kết chương: Clone + Cầu xoay + Bẫy | **4** | Cao trào Chương 6 |
| **L25** | **Ch.7** | Cầu bập bênh xoay (`RotatableBridge`) | 3 | Giới thiệu xoay góc |
| **L26** | Ch.7 | Chiếc hộp Pandora + Cờ giả (`FakeWinTrigger`) | 4 | Màn troll tâm lý cực đại |
| **L27** | Ch.7 | Bóc nút Pause UI (`DraggableButton`) làm đệm dậm | 4 | Bẻ gãy bức tường thứ tư |
| **L28** | Ch.7 | Đại kết cục: Phối hợp Clone + Bridge + UI + Ground | **5** | ĐỈNH CAO THỬ THÁCH (FINAL BOSS) |

---

### 4.2. Biểu đồ nhịp độ (Pacing Graph — Dạng sóng hình sin răng cưa)

Đường cong độ khó tuân thủ quy luật sóng răng cưa (Sawtooth Wave Curve): Độ khó tăng dần từ đầu chương đến màn kết chương, sau đó hạ nhiệt ở đầu chương tiếp theo để người chơi làm quen với cơ chế mới (tránh ức chế dồn dập), rồi lại leo thang lên đỉnh cao mới.

```
Độ khó (1 - 5)
 5 |                                                                     [*] L28
   |                                                                /
 4 |                 [*] L16             [*] L20             [*] L24   [*] L26, L27
   |                /                   /                   /         /
 3 |     [*] L04   /         [*] L12   /         [*] L22,L23   [*] L25
   |    /         /         /         /         /
 2 | [*] L01-03 [*] L05-07 [*] L09-10  /       [*] L21 (Làm quen Clone)
   |                                  /
 1 |_________________________________L13-14___________________________________
     L01       L05       L09       L13       L17       L21       L25       L28
     (Chương 1) (Chương 2) (Chương 3) (Chương 4) (Chương 5) (Chương 6) (Chương 7)
```
