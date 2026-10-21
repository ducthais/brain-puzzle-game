using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// =======================================================
// CÁC CLASS ĐỂ BỌC/MỞ GÓI DỮ LIỆU JSON (MOMO API)
// =======================================================
[System.Serializable]
public class MoMoRequestData
{
    public string partnerCode;
    public string requestId;
    public string amount;
    public string orderId;
    public string orderInfo;
    public string redirectUrl;
    public string ipnUrl;
    public string requestType;
    public string extraData;
    public string signature;
}

[System.Serializable]
public class MoMoResponseData
{
    public int resultCode;
    public string message;
    public string payUrl;
    public string qrCodeUrl;
    public string deeplink;
}

[System.Serializable]
public class MoMoQueryRequestData
{
    public string partnerCode;
    public string requestId;
    public string orderId;
    public string signature;
}

[System.Serializable]
public class MoMoQueryResponseData
{
    public int resultCode;
    public string message;
}

// =======================================================
// SCRIPT QUẢN LÝ CỬA HÀNG VÀ THANH TOÁN MOMO
// =======================================================
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("=== CẤU HÌNH MOMO SANDBOX ===")]
    public string partnerCode = "MOMO";
    public string accessKey = "YOUR_ACCESS_KEY";
    public string secretKey = "YOUR_SECRET_KEY";

    private string createOrderUrl = "https://test-payment.momo.vn/v2/gateway/api/create";
    private string queryStatusUrl = "https://test-payment.momo.vn/v2/gateway/api/query";

    [Header("=== GIAO DIỆN CỬA HÀNG ===")]
    public GameObject panelShopMain;
    public GameObject panelIsPremium;
    public GameObject panelQRCode;
    public RawImage qrRawImage;

    private bool isCheckingPayment = false;
    private Coroutine paymentPollingCoroutine; // Lưu trữ tiến trình kiểm tra thanh toán

    private void Awake()
    {
        // Xóa DontDestroyOnLoad để Canvas UI load lại trơn tru khi Replay
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Chỉ chạy lần đầu khi Scene được load
        RefreshShopUI();
    }

    // =======================================================
    // HÀM MỞ SHOP TỪ SẢNH (Gắn vào nút Shop)
    // =======================================================
    public void OpenShop()
    {
        RefreshShopUI();
    }

    // =======================================================
    // HÀM HỦY GIAO DỊCH (Gắn vào nút X/Hủy ở Panel QR)
    // =======================================================
    public void CancelAndCloseQR()
    {
        // Dừng việc liên tục hỏi Server MoMo
        isCheckingPayment = false;
        if (paymentPollingCoroutine != null)
        {
            StopCoroutine(paymentPollingCoroutine);
        }

        // Kiểm tra lại và quay về màn hình phù hợp
        RefreshShopUI();
    }

    // Hàm hỗ trợ tự động nhận diện tài khoản để hiển thị panel đúng
    private void RefreshShopUI()
    {
        if (PremiumManager.Instance != null && PremiumManager.Instance.IsPremium)
        {
            SwitchPanel("Panel_IsPremium");
        }
        else
        {
            SwitchPanel("Panel_ShopMain");
        }
    }

    public void OnClickBuyPremium()
    {
        if (PremiumManager.Instance != null && PremiumManager.Instance.IsPremium)
        {
            Debug.Log("Tài khoản của bạn đã sở hữu gói Premium từ trước!");
            return;
        }

        if (qrRawImage != null) qrRawImage.texture = null;

        string orderId = "EGGY_" + DateTime.Now.Ticks.ToString();
        string requestId = "REQ_" + DateTime.Now.Ticks.ToString();
        string amount = "25000";
        string orderInfo = "Nang cap tai khoan Premium - Mr.Eggy";

        StartCoroutine(CreateMoMoOrderRoutine(orderId, requestId, amount, orderInfo));
    }

    private IEnumerator CreateMoMoOrderRoutine(string orderId, string requestId, string amount, string orderInfo)
    {
        string redirectUrl = "https://momo.vn";
        string ipnUrl = "https://momo.vn";
        string requestType = "captureWallet";
        string extraData = "";

        string rawSignature = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={ipnUrl}" +
                              $"&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}" +
                              $"&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";

        string signature = ComputeHmacSha256(rawSignature, secretKey);

        MoMoRequestData requestData = new MoMoRequestData
        {
            partnerCode = partnerCode,
            requestId = requestId,
            amount = amount,
            orderId = orderId,
            orderInfo = orderInfo,
            redirectUrl = redirectUrl,
            ipnUrl = ipnUrl,
            requestType = requestType,
            extraData = extraData,
            signature = signature
        };

        string jsonPayload = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(createOrderUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Lỗi kết nối Server cổng MoMo: " + request.error);
                RefreshShopUI();
            }
            else
            {
                MoMoResponseData response = JsonUtility.FromJson<MoMoResponseData>(request.downloadHandler.text);

                if (response.resultCode == 0)
                {
                    Debug.Log("Tạo link đơn hàng thành công v2.");

                    // CHỈ KHI THÀNH CÔNG MỚI BẬT PANEL QR LÊN
                    SwitchPanel("Panel_QRCode");

#if UNITY_EDITOR
                    string dataToEncode = string.IsNullOrEmpty(response.deeplink) ? response.payUrl : response.deeplink;
                    string qrApiUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={UnityWebRequest.EscapeURL(dataToEncode)}";
                    StartCoroutine(DownloadQRImage(qrApiUrl));
#else
                    string targetLink = string.IsNullOrEmpty(response.deeplink) ? response.payUrl : response.deeplink;
                    Application.OpenURL(targetLink);
#endif
                    isCheckingPayment = true;
                    // Lưu lại tiến trình Coroutine để có thể dừng bằng nút Cancel
                    paymentPollingCoroutine = StartCoroutine(CheckPaymentStatusRoutine(orderId, requestId));
                }
                else
                {
                    Debug.LogError("MoMo phản hồi lỗi tham số: " + response.message);
                    RefreshShopUI();
                }
            }
        }
    }

    // =======================================================
    // HÀM TẢI ẢNH QR MÀ BẠN ĐÃ BỊ THIẾU
    // =======================================================
    private IEnumerator DownloadQRImage(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (qrRawImage != null)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    qrRawImage.texture = texture; // Áp texture QR vào RawImage trên màn hình PC
                }
            }
            else
            {
                Debug.LogError("Không thể tải hình ảnh mã hóa QR: " + request.error);
            }
        }
    }

    private IEnumerator CheckPaymentStatusRoutine(string orderId, string requestId)
    {
        int maxAttempts = 60;
        int attempts = 0;

        while (isCheckingPayment && attempts < maxAttempts)
        {
            yield return new WaitForSecondsRealtime(5f);
            attempts++;

            string rawSignature = $"accessKey={accessKey}&orderId={orderId}&partnerCode={partnerCode}&requestId={requestId}";
            string signature = ComputeHmacSha256(rawSignature, secretKey);

            MoMoQueryRequestData queryData = new MoMoQueryRequestData
            {
                partnerCode = partnerCode,
                requestId = requestId,
                orderId = orderId,
                signature = signature
            };

            string jsonPayload = JsonUtility.ToJson(queryData);

            using (UnityWebRequest request = new UnityWebRequest(queryStatusUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    MoMoQueryResponseData response = JsonUtility.FromJson<MoMoQueryResponseData>(request.downloadHandler.text);
                    if (response.resultCode == 0)
                    {
                        isCheckingPayment = false;
                        OnPaymentSuccess();
                        yield break;
                    }
                }
            }
        }

        // Hết thời gian chờ (Timeout)
        if (isCheckingPayment)
        {
            isCheckingPayment = false;
            RefreshShopUI();
        }
    }

    private void OnPaymentSuccess()
    {
        if (PremiumManager.Instance != null)
        {
            PremiumManager.Instance.GrantPremium();
        }
        RefreshShopUI();
    }

    public void SwitchPanel(string panelName)
    {
        if (panelShopMain != null) panelShopMain.SetActive(panelName == "Panel_ShopMain");
        if (panelIsPremium != null) panelIsPremium.SetActive(panelName == "Panel_IsPremium");
        if (panelQRCode != null) panelQRCode.SetActive(panelName == "Panel_QRCode");
    }

    // =======================================================
    // HÀM TẠO CHỮ KÝ BẢO MẬT BẠN ĐÃ CÓ TỪ ĐẦU
    // =======================================================
    private string ComputeHmacSha256(string message, string secretKey)
    {
        byte[] keyByte = Encoding.UTF8.GetBytes(secretKey);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        using (var hmacsha256 = new HMACSHA256(keyByte))
        {
            byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
            StringBuilder hex = new StringBuilder(hashmessage.Length * 2);
            foreach (byte b in hashmessage) hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}