# Dịch vụ online — Mr. Eggy (luồng B)

Phạm vi: W1-07, W1-08, W1-09, W2-05, W2-06, W2-07. Mã ở
`Assets/_Project/Scripts/Online/`, assembly `MainGameScripts.Online` tham chiếu
`MainGameScripts`, `Unity.Services.Core`, `Unity.Services.Authentication`,
`Unity.Services.CloudSave`, `Unity.Services.Analytics`. Không thay assembly lõi hay scene/prefab.

## 1. Package và mức độ xác minh

| Package | Phiên bản ghim | Mức độ chắc chắn |
|---|---|---|
| `com.unity.services.authentication` | `3.8.0` | Cao về phiên bản: [Unity liệt kê released cho 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-services-authentication) |
| `com.unity.services.cloudsave` | `3.4.2` | Cao về phiên bản: [released cho 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-services-cloudsave) |
| `com.unity.services.analytics` | `6.3.0` | Cao về phiên bản: [released cho 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-services-analytics) |
| `com.unity.services.leaderboards` | `2.3.4` | Cao về phiên bản: [released cho 6000.6](https://docs.unity.com/en-us/engine/6000.6/manual/packages-list/packages-all/pack-safe/com-unity-services-leaderboards); chỉ cài package, chưa triển khai W3-05 |

UGS Core là phụ thuộc do Package Manager giải quyết. Chưa có Unity và DLL UGS
trong môi trường triển khai này: **chưa xác nhận biên dịch/tích hợp thật với Unity 6000.6.6f1**.
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

Chưa chắc chắn ở mức tích hợp thực tế: tên/đồ thị assembly sau resolve,
kiểu dữ liệu Dashboard khớp SDK, hành vi consent và đăng nhập lại sau token hết hạn
trên Android. Các chữ ký trên có nguồn tài liệu, nhưng chưa được kiểm chứng bằng DLL và dịch vụ thật.

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
Reset local có thể được cloud khôi phục bởi luật max; xóa/reset cloud toàn bộ chưa thuộc đợt này.

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

### Gắn popup trong Unity (người dùng/agent UI thực hiện)

1. Tạo Canvas/Panel đồng ý ở StartScreen bằng Editor. Nội dung cần nói rõ thu thập
   màn chơi, thời gian, số lần chết, gợi ý/sao và sự kiện thiết bị/phiên của SDK để cải thiện game;
   cho phép từ chối và tiếp tục chơi.
2. Gắn `AnalyticsConsentPrompt` vào một controller luôn active, **nằm ngoài panel**;
   kéo panel vào field `consentPanel`. `Start()` chỉ hiện nếu `!HasDecided`.
3. Nút Đồng ý → `OnClick` → `AnalyticsConsentPrompt.Grant()`;
   nút Từ chối → `AnalyticsConsentPrompt.Revoke()`. Hai cách đều đóng panel.
4. Trong Settings, thêm nút thu hồi gọi `Revoke()`; để đồng ý lại gọi `Grant()` sau
   khi hiển thị giải thích. Không tự đồng ý khi người chơi đóng popup.
5. Chưa gắn UI: mặc định không thu thập, game/Cloud Save vẫn chạy.

## 6. Liên kết Unity Cloud và khai báo event

1. Đăng nhập Unity Hub bằng tài khoản có quyền project. Trên Unity Cloud Dashboard,
   chọn đúng Organization và tạo/chọn project Mr. Eggy, ghi lại Project ID.
2. Trong Unity Editor mở **Edit → Project Settings → Services**, chọn Organization,
   liên kết project hiện có đúng Project ID (hoặc tạo project mới nếu chưa có).
   Kiểm tra Project ID tại đây; không đặt token/service account secret vào source.
3. Mở **Window → Package Manager**, chờ bốn package ở mục 1 resolve.
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

Kiểm tra tại worktree: 14 ca dùng assertion NUnit đã pass bằng trình chạy reflection .NET 10
với Unity/UGS giả lập. Mô phỏng bổ sung đã pass: lưu local ngay, giữ tiến trình khi tải lỗi,
retry, gộp cloud kèm write lock, thay đổi khi đang upload, cloud hỏng, mở lại và consent/5 event.
Harness tạm nằm trong `Temp/OnlineChecks/` (Git bỏ qua); phép kiểm tra này chỉ xác minh
logic C# trên mô hình SDK, không xác nhận chữ ký DLL, JsonUtility/PlayerPrefs hay dịch vụ thật.
Chưa chạy Unity Test Runner, PlayMode, Android hay gọi UGS thật; không coi task đã nghiệm thu.

Yêu cầu Claude: duyệt/commit luồng B sau khi chạy các kiểm tra trên; cập nhật Phụ lục B
của kế hoạch theo schema hiện tại; thống nhất reset khi cloud bật nếu cần xóa thật từ đầu.
Không cần đổi interface hay sửa các điểm phát sự kiện gameplay hiện tại.
