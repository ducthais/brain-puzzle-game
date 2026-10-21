# Dịch vụ online — Mr. Eggy (luồng B)

Phạm vi: W1-07, W1-08, W1-09, W2-05, W2-06, W2-07, W3-05, W3-06. Mã ở
`Assets/_Project/Scripts/Online/`, assembly `MainGameScripts.Online` tham chiếu
`MainGameScripts`, `Unity.Services.Core`, `Unity.Services.Authentication`,
`Unity.Services.CloudSave`, `Unity.Services.Analytics`, `Unity.Services.Leaderboards`,
`Unity.Services.RemoteConfig`, `Unity.TextMeshPro`, `UnityEngine.UI`.
W3 thêm `Core/GameConfig.cs`, `Services/GameConfigService.cs`, nối cấu hình vào `AdManager`
và các điểm mở hint/skip trong `LevelManager`; không sửa scene/prefab bằng text.

## 1. Package và mức độ xác minh

| Package | Phiên bản ghim | Mức độ chắc chắn |
|---|---|---|
| `com.unity.services.authentication` | `3.8.0` | Cao về phiên bản: [Unity liệt kê released cho 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-services-authentication) |
| `com.unity.services.cloudsave` | `3.4.2` | Cao về phiên bản: [released cho 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-services-cloudsave) |
| `com.unity.services.analytics` | `6.3.0` | Cao về phiên bản: [released cho 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-services-analytics) |
| `com.unity.services.leaderboards` | `2.3.4` | W3-05 đã triển khai; kiểm tra biên dịch bằng DLL thật của project |
| `com.unity.remote-config` | `4.2.5` | Đã thêm manifest; [tài liệu package 4.2.5](https://docs.unity3d.com/Packages/com.unity.remote-config@4.2/api/index.html); cần Unity resolve dependency runtime |

UGS Core là phụ thuộc do Package Manager giải quyết. Theo xác nhận của người dùng,
luồng B tuần 1–2 đã được merge và biên dịch thành công trong Unity 6000.6.6f1,
bao gồm API `UnityConsent.EndUserConsent`. Đợt W3 đã kiểm tra C# bằng DLL Unity/UGS
thật có sẵn, ngoại trừ Remote Config phải dùng stub vì chưa có package trong cache và
môi trường chặn tải package. Chưa coi đây là chạy Unity Test Runner hoặc kiểm thử UGS thật.
Không sửa tay `packages-lock.json`: mở Unity để resolve, kiểm tra dependency và để Unity cập nhật lock.

API đã đối chiếu tài liệu chính thức:

- `UnityServices.InitializeAsync()`, `AuthenticationService.Instance.SignInAnonymouslyAsync()`,
  `IsAuthorized`: [quản lý phiên đăng nhập](https://docs.unity.com/en-us/authentication/session-management).
  Khi token hết hạn vẫn có thể `IsSignedIn == true`, nên bootstrap kiểm tra `IsAuthorized`.
- `CloudSaveService.Instance.Data.Player.LoadAsync(HashSet<string>)`,
  `item.Value.GetAs<string>()`: [hướng dẫn SDK](https://docs.unity.com/en-us/cloud-save/tutorials/unity-sdk).
- `SaveAsync(Dictionary<string, object>)` để tạo key, `SaveAsync(Dictionary<string, SaveItem>)`
  để cập nhật có khóa: [API Player Data 3.4](https://docs.unity3d.com/Packages/com.unity.services.cloudsave@3.4/api/Unity.Services.CloudSave.Internal.IPlayerDataService.html),
  [SaveItem(object, string)](https://docs.unity3d.com/Packages/com.unity.services.cloudsave@3.4/api/Unity.Services.CloudSave.Models.SaveItem.html).
- `CustomEvent` + `AnalyticsService.Instance.RecordEvent(...)`:
  [API Analytics 6.3](https://docs.unity3d.com/Packages/com.unity.services.analytics@6.3/api/Unity.Services.Analytics.CustomEvent.html).
- `UnityEngine.UnityConsent.EndUserConsent.GetConsentState()/SetConsentState(...)`,
  `ConsentState.AnalyticsIntent`, `ConsentStatus.Granted/Denied`:
  [API Unity 6000.6](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UnityConsent.EndUserConsent.html),
  [hướng dẫn chuyển consent](https://docs.unity.com/en-us/analytics/sdks-and-apis/sdk61-migration-guide).
  Unity 6.2+ với SDK 6.1+ dùng Developer Data; không gọi API cũ `StartDataCollection/StopDataCollection`.

Còn cần xác nhận tích hợp thực tế: dependency Remote Config sau resolve, cấu hình
Dashboard, hành vi consent/đăng nhập lại trên Android và thao tác hai tool UI trong Editor.

## 2. Khởi tạo và lưu tiến trình

1. `SubsystemRegistration` xóa tham chiếu bootstrap static khi vào Play Mode.
2. `BeforeSceneLoad` tạo `OnlineServicesBootstrap`, gọi `DontDestroyOnLoad`.
3. `Awake` tạo `CloudProgressStore`, đăng ký bằng `ProgressService.UseStore(store)`.
   `Load()` chỉ gọi `LocalProgressStore.Load()` và trả ngay; mở scene không phải chờ mạng.
4. Áp dụng lựa chọn Analytics trước khi khởi tạo SDK. Chưa chọn đồng nghĩa không thu thập.
5. Thiếu `Application.cloudProjectId`: cảnh báo một lần, giữ store local hoạt động;
   liên kết project trong Editor rồi mở lại Play Mode/game.
6. Có cấu hình và mạng: `InitializeAsync()` rồi `SignInAnonymouslyAsync()`.
   Token lưu sẵn được SDK dùng lại. Không log token hay Player ID.
7. Vòng lặp kiểm tra mỗi giây bằng `Task.Delay`, không phụ thuộc `Time.timeScale`.
   Cloud đọc rồi gộp bằng `ProgressService.MergeFrom(remote)`; UI nhận `Changed` sẵn có.

Mỗi lần `Save(data)` luôn lưu local trước, đánh dấu `CloudProgressPending = 1` trong PlayerPrefs.
Store nghe `ProgressService.Changed` để lùi thời điểm gửi tới 2 giây sau thay đổi cuối.
`CloudSyncState` là lớp C# thuần chứa revision, debounce, trạng thái đang gửi và retry.
Chỉ một yêu cầu đồng bộ được chạy; thay đổi xảy ra trong lúc upload giữ cờ chờ cho lượt sau.

Mỗi lượt đồng bộ đều đọc cloud trước khi ghi. Với key có sẵn, dùng `item.WriteLock`
để phát hiện dữ liệu đã bị thay đổi trên thiết bị khác. Xung đột giữ local/cờ chờ,
lần thử tiếp theo đọc và gộp lại; không ép ghi đè bỏ khóa.
Đối với key chưa tồn tại, SDK tạo bằng lệnh không khóa theo quy ước UGS.
[Cơ chế write lock](https://docs.unity.com/en-us/cloud-save/concepts/write-locks).

## 3. Schema Cloud Save

Player Data, **Default Access Class**, key **`progress`**, value là **chuỗi JSON**
do `JsonUtility.ToJson(ProgressData)` tạo. Khi sửa bằng Dashboard, lưu kiểu string chứa JSON,
không đổi thành một object Cloud Save khác kiểu với `GetAs<string>()`.

```json
{
  "version": 1,
  "maxUnlockedLevel": 12,
  "savedLevel": "Level 12",
  "resetCount": 0,
  "stars": [
    { "levelId": 1, "stars": 3 },
    { "levelId": 2, "stars": 2 }
  ],
  "updatedAtUtc": "2026-10-21T10:15:00.0000000Z"
}
```

`stars` là `List<LevelStars>`; **không có `isPremium`**. Premium do IAP/PremiumManager
quản lý riêng. Schema này theo API hiện tại và ghi chú Claude, thay ví dụ cũ ở Phụ lục B
(`stars` dạng dictionary, `updatedAt`, `isPremium`). Không tự sửa kế hoạch thuộc phần điều phối.

Luật gộp do `ProgressMerger` quyết định: max màn mở khóa, max sao từng màn;
`savedLevel` theo bên mở khóa xa hơn, hòa thì giữ local. Merger lấy timestamp muộn hơn;
`ProgressService.MergeFrom` sau đó ghi timestamp mới khi commit. Không thay luật trong luồng B.
`ValidateRemote` từ chối phiên bản chưa hỗ trợ, màn/sao không hợp lệ hoặc phần tử null;
gặp dữ liệu lỗi thì giữ local, không ghi đè cloud, cảnh báo và thử lại sau.

## 4. Mất mạng và khôi phục

- `internetReachability` chỉ là tín hiệu thiết bị có đường mạng. Wi-Fi không có Internet,
  lỗi DNS, timeout, giới hạn dịch vụ, lỗi xác thực đều được bắt exception.
- Khởi tạo/đăng nhập và đồng bộ có khoảng chờ thử lại riêng: 5, 10, 20, 40, tối đa 60 giây.
  Thay đổi mới không vượt qua khoảng chờ lỗi. Không phát request trong mỗi frame.
- Khi từ mất mạng sang có mạng hoặc resume app: bỏ thời gian chờ lỗi cloud;
  vẫn tôn trọng debounce. Vòng khởi tạo cũng thử lại khi phát hiện mạng khôi phục.
- `CloudProgressPending` chỉ về 0 khi upload thành công và không có revision mới.
  Không trông chờ một request lúc thoát app; local đã được ghi trước đó.
- Mỗi lần mở game đều đánh dấu cần đối chiếu lại dù cờ cũ bằng 0. Vì vậy cờ cũ bị thiếu
  hoặc app chết sau khi ghi local cũng không làm bỏ sót đồng bộ phiên sau.
- Hủy bootstrap hủy vòng lặp và tháo đăng ký `Changed`; phản hồi mạng đến muộn
  không được gộp vào phiên mới. SDK request đang chạy không bị ép hủy ở tầng HTTP.

Giới hạn hiện tại: tài khoản ẩn danh dùng cache trên thiết bị; cài lại/xóa dữ liệu có thể
mất tài khoản, chưa có liên kết tài khoản để chơi cùng Player ID trên hai máy.
[Giới hạn anonymous sign-in](https://docs.unity.com/en-us/authentication/use-anon-sign-in).
Hai thiết bị tạo key `progress` lần đầu đúng cùng lúc vẫn có khe tranh chấp vì chưa có
write lock; cần Cloud Code phía server nếu muốn bảo đảm nguyên tử cho trường hợp này.
`resetCount` lớn hơn thắng nguyên bản dữ liệu; chỉ gộp max khi hai bên cùng số lần reset.
Dữ liệu cũ thiếu trường này tương đương 0. New Game vẫn giữ kỷ lục leaderboard theo Keep Best.

## 5. Analytics và consent

| Sự kiện | Điểm gameplay hiện có | Tham số và kiểu C# / Dashboard |
|---|---|---|
| `level_start` | Load `Level N` | `level_id`: int / Integer; `attempt`: int / Integer |
| `level_complete` | Thắng màn | `level_id`: int / Integer; `duration_sec`: float / Float; `deaths`: int / Integer; `hints_used`: int / Integer; `stars`: int / Integer |
| `player_death` | Nhân vật chết | `level_id`: int / Integer; `time_in_level_sec`: float / Float |
| `hint_used` | Mở gợi ý | `level_id`: int / Integer; `via_ad`: bool / Boolean |
| `level_skip` | Bỏ qua màn | `level_id`: int / Integer; `time_in_level_sec`: float / Float |

Giữ đúng chữ thường và dấu gạch dưới. `UnityAnalyticsReporter` chỉ được đăng ký
bằng `GameAnalytics.UseReporter(...)` khi `AnalyticsConsent.IsGranted` và UGS đã khởi tạo.
Reporter kiểm tra lại consent trước mỗi lệnh gửi; lỗi SDK không được làm hỏng gameplay.
Không xếp hàng/phát lại các sự kiện gameplay xảy ra khi chưa đồng ý.
SDK có thể phát các sự kiện nền tiêu chuẩn **sau khi** được đồng ý.

`AnalyticsConsent` lưu duy nhất key `AnalyticsConsent`: không có key = chưa quyết định;
`1` = đồng ý, `0` = từ chối/thu hồi. `HasDecided` giúp UI biết cần hỏi lần đầu.
`Grant()`/`Revoke()` lưu lựa chọn và cập nhật Developer Data ngay;
`Revoke()` thay reporter bằng `NullAnalyticsReporter` và đặt AnalyticsIntent = Denied.
Không đồng ý Analytics không ảnh hưởng Auth/Cloud Save. Thu hồi không đồng nghĩa xóa
dữ liệu đã gửi; yêu cầu xóa dữ liệu là thao tác riêng nếu sản phẩm cần triển khai sau này.

### Dựng popup trong Unity

1. Dừng Play mode, mở scene **StartScreen**, chạy **Tools → Online → Tạo popup đồng ý Analytics**.
2. Tool tạo Canvas, panel có giải thích tiếng Việt, hai nút **Đồng ý/Từ chối**,
   `AnalyticsConsentPrompt` trên Canvas ngoài panel; gán `consentPanel` và nối
   persistent onClick `Grant()`/`Revoke()` bằng `UnityEventTools.AddPersistentListener`.
3. Nút **Đồng ý Analytics** ở góc trái dưới mở lại popup để đổi/thu hồi lựa chọn.
   `Start()` chỉ hiện popup tự động nếu chưa quyết định. Hai lựa chọn đều đóng panel.
4. Kiểm tra bố cục, font tiếng Việt và lưu scene (Ctrl+S). Ctrl+Z hoàn tác cả lần tạo.
   Tool không lưu scene tự động, không tạo trùng và không chạy khi đang/vừa chuẩn bị Play.
5. Chưa tạo UI: mặc định không thu thập, Auth/Cloud Save/Leaderboard/Remote Config vẫn chạy.

## 6. Liên kết Unity Cloud và khai báo event

1. Đăng nhập Unity Hub bằng tài khoản có quyền project. Trên Unity Cloud Dashboard,
   chọn đúng Organization và tạo/chọn project Mr. Eggy, ghi lại Project ID.
2. Trong Unity Editor mở **Edit → Project Settings → Services**, chọn Organization,
   liên kết project hiện có đúng Project ID (hoặc tạo project mới nếu chưa có).
   Kiểm tra Project ID tại đây; không đặt token/service account secret vào source.
3. Mở **Window → Package Manager**, chờ các package ở mục 1 resolve.
   Kiểm tra Console và `packages-lock.json` do Unity sinh; giữ Unity `6000.6.6f1`.
4. Trong Dashboard project bật/cấu hình Authentication (anonymous), Cloud Save, Analytics
   theo màn hình onboarding; dùng cùng environment. Code hiện tại dùng environment mặc định,
   không gọi `SetEnvironmentName`. Xác minh environment mặc định trong Dashboard.
5. Analytics → **Event Manager → Add New → Custom Event**, tạo đủ 5 event ở bảng trên.
   Tạo Custom Parameter và gắn đúng kiểu cho mỗi event, dùng lại parameter chung
   `level_id`, `time_in_level_sec`; bật event trong environment dùng thử.
   [Hướng dẫn tạo custom event](https://docs.unity.com/en-us/analytics/events/custom-event),
   [Event Manager](https://docs.unity.com/en-us/analytics/events/event-manager).
6. Chạy game, đồng ý qua popup, chơi thử; xem Analytics Event Browser/Debug và tình trạng
   valid/invalid của event. Cloud Save → Player Data: kiểm tra key `progress` sau khi thắng màn.
   Tên vị trí menu có thể thay đổi; cần kiểm chứng trong Dashboard của project thực tế.

## 7. Kiểm thử và bàn giao

`Tests_EditMode/Online/` có asmdef chỉ Editor, define `UNITY_INCLUDE_TESTS`, tham chiếu
hai assembly game và TestRunner/NUnit riêng. Chạy **Window → General → Test Runner → EditMode**
sau khi Unity resolve package. Các test kiểm tra debounce, retry tăng dần/có giới hạn,
thay đổi trong lúc upload, phục hồi mạng và dữ liệu cloud không hợp lệ.

Kiểm tra thủ công bắt buộc trong Editor và trên Android:

| Ca | Thao tác | Kết quả cần xác nhận |
|---|---|---|
| Chưa liên kết | Play khi thiếu Project ID | Cảnh báo, chơi và lưu local được |
| Khởi động offline | Tắt mạng, thắng màn, đóng/mở game | Màn/sao còn nguyên, cờ chờ = 1 |
| Mạng trở lại | Bật mạng, chờ bootstrap + debounce | Đăng nhập, đọc/gộp, cập nhật `progress`, cờ = 0 |
| Upload gián đoạn | Cắt mạng giữa gửi, chơi tiếp | Local giữ cả thay đổi mới; đồng bộ đủ khi kết nối lại |
| Cloud tốt hơn | Sửa dữ liệu hợp lệ trên Dashboard trước lần đồng bộ | Mở khóa/sao được gộp theo max, UI nhận Changed |
| Xung đột write lock | Đổi cloud trong khoảng đọc–ghi | Lệnh thất bại an toàn, thử lại bằng bản cloud mới |
| Consent mặc định | Xóa key consent trên thiết bị test, mở game | HasDecided=false; reporter rỗng, AnalyticsIntent=Denied |
| Consent bật/tắt | Đồng ý, phát 5 event, thu hồi, phát tiếp | Đúng schema khi đồng ý; ngừng thu thập/gửi sau thu hồi |
| Phiên hết hạn | Để token hết hạn khi offline rồi nối mạng | Đăng nhập lại bằng cache, không tạo nhầm tài khoản khi cache còn |
| Cloud lỗi | Dữ liệu null, sao ngoài 0–3 hoặc version tương lai | Cảnh báo, không ghi đè cloud, local chơi được |
| Play Mode lặp lại | Bật/tắt Domain Reload, vào/thoát Play nhiều lần | Một bootstrap, không callback/store cũ |

Kiểm tra đợt tuần 1–2 tại worktree: 14 ca dùng assertion NUnit đã pass bằng trình chạy reflection .NET 10
với Unity/UGS giả lập. Mô phỏng bổ sung đã pass: lưu local ngay, giữ tiến trình khi tải lỗi,
retry, gộp cloud kèm write lock, thay đổi khi đang upload, cloud hỏng, mở lại và consent/5 event.
Harness tạm nằm trong `Temp/OnlineChecks/` (Git bỏ qua); phép kiểm tra này chỉ xác minh
logic C# trên mô hình SDK, không xác nhận chữ ký DLL, JsonUtility/PlayerPrefs hay dịch vụ thật.
Chưa chạy Unity Test Runner, PlayMode, Android hay gọi UGS thật; không coi task đã nghiệm thu.

Yêu cầu Claude: duyệt/commit luồng B sau khi chạy các kiểm tra trên; cập nhật Phụ lục B
của kế hoạch theo schema hiện tại; thống nhất reset khi cloud bật nếu cần xóa thật từ đầu.
Không cần đổi interface hay sửa các điểm phát sự kiện gameplay hiện tại.

## 8. Leaderboard tổng sao (W3-05)

Hằng số `LeaderboardScoreService.LeaderboardId = "total_stars"`. Điểm là tổng
`ProgressData.stars[*].stars`, không phải số màn mở khóa. `LeaderboardSyncState` là C# thuần,
chỉ gửi khi tổng hiện tại lớn hơn lần đã gửi thành công. Tổng 0 chưa tạo mục xếp hạng.
Chỉ thay đổi sao mới lùi debounce 2 giây; thay đổi âm thanh/tiến trình không liên quan
không lùi thời điểm gửi. Lỗi giữ điểm chờ, retry 5/10/20/40/60 giây; cập nhật trong lúc
request chạy được giữ cho lượt sau. Khôi phục mạng bỏ thời gian chờ lỗi, vẫn giữ debounce.

`LeaderboardScoreService` nghe `ProgressService.Changed` và chạy qua vòng lặp bootstrap
sau khi `OnlineServicesBootstrap.IsSignedIn` xác nhận SDK đã khởi tạo và `IsAuthorized`.
Điểm xác nhận lưu ở PlayerPrefs riêng `LeaderboardTotalStars.<PlayerId>`; không đụng key
tiến trình. Lỗi/đóng app trước khi xác nhận sẽ gửi lại an toàn nhờ Keep Best. Xóa cache
có thể gửi lại cùng điểm; New Game không xóa kỷ lục đã gửi. Nếu xóa/reset leaderboard
trên Dashboard để thử lại, cũng xóa cache điểm tương ứng trên thiết bị test.

### Việc cần làm trên Dashboard

1. Chọn đúng project và environment mặc định đang dùng cho Auth/Cloud Save.
2. Bật **Leaderboards**, tạo leaderboard có **ID `total_stars`** (đúng chữ thường).
3. **Sort Order: Descending**, **Update Type: Keep Best / Best Score**.
   Dùng bảng toàn cục, không bật bucket/tier, không lên lịch reset cho bảng kỷ lục này.
4. Lưu/tạo cấu hình theo màn hình Dashboard. Thắng một màn để có sao, chờ gửi;
   xác nhận điểm là tổng sao và chơi lại ít sao hơn không hạ kỷ lục.

### Dựng màn hình tự động

1. Dừng Play mode, mở **LevelSelect**, chạy **Tools → Online → Tạo bảng xếp hạng**.
2. Tool tạo Canvas, nút **Xếp hạng** ở góc phải dưới, panel ban đầu ẩn,
   10 hàng TextMeshPro, hạng riêng, trạng thái, nút **Thử lại/Đóng**; nối sẵn mọi tham chiếu.
3. Bấm **Xếp hạng** trong Play: lấy top 10 (`Offset=0`, `Limit=10`) và điểm cá nhân.
   Rank SDK bắt đầu từ 0 nên UI cộng 1. Ưu tiên `PlayerName` do UGS trả về;
   thiếu tên thì hiện `Eggy <8 ký tự đầu PlayerId>`. Tên là văn bản thuần, không chạy rich text.
4. Chưa đăng nhập → thông báo và tự tải khi đăng nhập xong. Chưa gửi điểm → thông báo
   chưa có hạng; chỉ coi `EntryNotFound` là chưa có điểm. Lỗi dịch vụ/cấu hình vẫn hiện lỗi.
   Đóng panel hoặc đổi tài khoản khiến phản hồi cũ không cập nhật UI.
5. Kiểm tra vị trí nút trên UI hiện có và font tiếng Việt, rồi Ctrl+S. Ctrl+Z hỗ trợ hoàn tác.
   Chạy lại menu sẽ chọn bản đã có và cảnh báo thay vì tạo trùng.

Hai tool nằm trong `Scripts/Online/Editor/`, assembly **MainGameScripts.Online.Editor**,
chỉ Editor, tham chiếu Online/MainGameScripts/UI/TMP và Input System để tạo EventSystem
phù hợp `activeInputHandler=1` khi scene thiếu. Không cần sửa assembly Editor của luồng A.
Nếu project chưa có TMP Essential Resources, import qua menu TextMeshPro trước khi dùng.

API đã đối chiếu [hướng dẫn top scores](https://docs.unity.com/en-us/leaderboards/tutorials/unity-sdk/get-score),
[hạng cá nhân](https://docs.unity.com/en-us/leaderboards/tutorials/unity-sdk/get-player-score) và
[LeaderboardEntry 2.3.4](https://docs.unity3d.com/Packages/com.unity.services.leaderboards@2.3/api/Unity.Services.Leaderboards.Models.LeaderboardEntry.html).
`AddPlayerScoreAsync`, `GetScoresAsync`, `GetScoresOptions`, `GetPlayerScoreAsync`,
`LeaderboardEntry.PlayerName/PlayerId/Rank/Score` và `LeaderboardsException.Reason/EntryNotFound`
đã biên dịch với DLL **2.3.4** thật; chưa kiểm chứng bằng tài khoản/dịch vụ thật.

## 9. Remote Config (W3-06)

`GameConfig` là C# thuần, bất biến. Constructor validate khoảng, `Parse` đọc các giá trị
chuỗi để test độc lập; key thiếu/sai parse dùng mặc định riêng cho key đó.
`GameConfigService.Current` giữ cấu hình và phát `Changed`; SubsystemRegistration
reset cả cấu hình lẫn subscriber khi vào Play mode, kể cả tắt Domain Reload.

| Key (phân biệt hoa/thường) | Kiểu Dashboard | Mặc định | Hợp lệ / ý nghĩa |
|---|---|---|---|
| `interstitial_levels` | Int | `2` | Số nguyên 1–20; quảng cáo sau từng số màn hoàn thành |
| `interstitial_deaths` | Int | `9` | Số nguyên 1–20; quảng cáo sau từng số lần chết |
| `hint_requires_ad` | Bool | `true` | `true`/`false`; false mở gợi ý trực tiếp |
| `skip_enabled` | Bool | `true` | `true`/`false`; false chặn hành động Nhảy |

Giá trị mặc định 2/9 lấy từ prefab `AdManager` (`levelsPerAd = 2`, `deathsPerAd = 9`).
Remote Config áp dụng thay cấu hình Inspector của hai ngưỡng này, không đổi tên field.
Premium vẫn bỏ quảng cáo theo hành vi cũ. `hint_requires_ad=false` chỉ miễn quảng cáo
để mở hint; quảng cáo dùng cho Nhảy giữ hành vi hiện tại. `skip_enabled=false` chặn
ở handler, không sửa hay dựng lại HUD/logic sao/tiến trình.

### Việc cần làm trên Dashboard

1. Bật **Remote Config** trong cùng project/environment với các dịch vụ khác.
2. Tạo 4 key trên ở cấu hình mặc định (`settings`), chọn đúng **Int/Bool**, rồi **Save/Publish/Deploy**
   theo giao diện Dashboard. Thử trước với các mặc định trong bảng.
3. Đổi từng key, mở lại game để fetch phiên mới; thử hint miễn quảng cáo và chặn Nhảy.
   Thử ngưỡng 0/21 để xác nhận key quay về 2; không dùng 0 để tắt quảng cáo.

`RemoteGameConfigService` tải sau đăng nhập, chạy độc lập với cloud/leaderboard qua
`Task.WhenAll`, thành công một lần trong mỗi phiên chạy. Không cần Analytics consent.
Mỗi phiên bắt đầu bằng mặc định; lỗi mạng hoặc SDK chỉ trả cache thì giữ mặc định,
retry cùng khoảng 5–60 giây. Chỉ áp `RuntimeConfig.origin == ConfigOrigin.Remote`;
`GetInt/GetBool` có fallback, constructor tiếp tục validate khoảng số.
Hủy bootstrap khiến kết quả muộn không áp vào phiên sau. Không sửa `packages-lock.json`:
Unity phải resolve `com.unity.remote-config` **4.2.5** và dependency runtime.

API tham khảo: [tích hợp SDK](https://docs.unity.com/en-us/remote-config/code-integration),
[FetchConfigsAsync](https://docs.unity3d.com/Packages/com.unity.remote-config-runtime@4.0/api/Unity.Services.RemoteConfig.RemoteConfigService.html),
[RuntimeConfig](https://docs.unity3d.com/Packages/com.unity.remote-config-runtime@4.0/api/Unity.Services.RemoteConfig.RuntimeConfig.html).
Tài liệu runtime xác nhận assembly `Unity.Services.RemoteConfig`, overload hai struct trả
`Task<RuntimeConfig>`, `origin`, `GetInt(string,int)`, `GetBool(string,bool)`.
**Chưa xác nhận bằng DLL dependency thực tế của 4.2.5**; đây là phần cần kiểm tra lại khi Unity resolve.

## 10. Kết quả kiểm tra W3 và bàn giao

- **26 ca NUnit pass** bằng reflection runner .NET 10: 18 ca mới cho GameConfig/LeaderboardSyncState,
  cộng 8 ca CloudSyncState hiện có. Dùng mã C# thuần thật, không stub Unity/UGS ở các ca này.
- Build .NET Standard 2.1/C# 9 của MainGameScripts, Online và Online.Editor pass với DLL Unity
  **6000.6.6f1**, UGS hiện có (gồm Leaderboards **2.3.4**), TMP/UI/Input System thật.
  **Riêng Remote Config dùng stub** theo chữ ký tài liệu vì cache chưa có và mạng bị chặn tải package;
  không gọi đây là xác nhận biên dịch đầy đủ trong Unity.
- Harness/log nằm ở `Temp/W3Checks/` (Git bỏ qua); không đưa stub vào Assets hay commit.
- Không chạy Unity Test Runner/PlayMode/Android hoặc hai menu UI trong phiên này.
  Người dùng cần resolve package, chạy EditMode/PlayMode, xác nhận Console không lỗi đỏ,
  thao tác menu/Undo, kiểm tra TMP font/bố cục và UGS thật theo checklist dưới đây.

| Ca bổ sung | Kết quả cần xác nhận |
|---|---|
| Thắng nhiều màn / cải thiện sao | Điểm bằng tổng sao; chỉ tăng mới gửi, gom 2 giây |
| Offline → online / đóng mở game | Không crash, tiến trình còn, điểm chờ gửi lại |
| Tăng sao trong lúc gửi | Không mất lần tăng mới; gửi lượt sau |
| Chưa có điểm / ngoài top 10 | Top vẫn tải; hạng riêng đúng hoặc báo chưa có điểm |
| Tắt/mở panel khi request chạy | Phản hồi cũ không ghi vào panel vừa mở lại |
| Chưa đăng nhập / lỗi ID trên Dashboard | Trạng thái tiếng Việt rõ ràng; có thể Thử lại |
| Hai menu lúc Play / tạo trùng / Undo | Bị chặn / chọn bản cũ / hoàn tác toàn bộ |
| Consent lần đầu / mở lại / thu hồi | Hai nút nối persistent đúng; Analytics tôn trọng lựa chọn |
| Config hợp lệ / thiếu key / ngoài khoảng | Áp giá trị hợp lệ; thiếu/lỗi giữ mặc định |
| Config offline ngay từ đầu | 2/2/true/true; khi mạng trở lại tải được cấu hình |
| Tắt Domain Reload, vào Play nhiều lần | Cấu hình reset, không subscriber hoặc bootstrap thừa |
