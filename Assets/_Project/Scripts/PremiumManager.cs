using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Unity.Services.Core;

public class PremiumManager : MonoBehaviour
{
    public static PremiumManager Instance { get; private set; }

    // Đồng nhất duy nhất một Key theo chuẩn báo cáo
    private const string PremiumKey = "isPremium";

    [Header("--- ID SẢN PHẨM GOOGLE PLAY ---")]
    public string noAdsProductId = "com.yourstudio.mreggy.noads";

    [Header("--- SHOP UI PANELS ---")]
    public GameObject panelNotPremium;
    public GameObject panelPremiumOwned;

    private StoreController storeController;
    private bool isInitialized = false;

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await InitializeIAPAsync();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RefreshShopUI();
    }

    // Cho phép các Script khác đọc trạng thái công khai
    public bool IsPremium
    {
        get => PlayerPrefs.GetInt(PremiumKey, 0) == 1;
        private set
        {
            PlayerPrefs.SetInt(PremiumKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    // HÀM MỚI: Gọi hàm này để kích hoạt Premium (Dùng cho cả MoMo và IAP)
    public void GrantPremium()
    {
        IsPremium = true;
        RefreshShopUI();
        Debug.Log("[PremiumManager] Đã cấp quyền Premium vĩnh viễn cho tài khoản!");
    }

    public void RefreshShopUI()
    {
        bool hasPremium = IsPremium;
        if (panelNotPremium != null) panelNotPremium.SetActive(!hasPremium);
        if (panelPremiumOwned != null) panelPremiumOwned.SetActive(hasPremium);
    }

    // =======================================
    // LOGIC UNITY IAP v5 (GIỮ NGUYÊN CODE GỐC)
    // =======================================
    private async Task InitializeIAPAsync()
    {
        if (isInitialized) return;
        try
        {
            await UnityServices.InitializeAsync();
            storeController = UnityIAPServices.StoreController();

            // ĐĂNG KÝ ĐẦY ĐỦ CÁC SỰ KIỆN ĐỂ XÓA CẢNH BÁO MÀU VÀNG
            storeController.OnProductsFetched += OnProductsFetched;
            storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            storeController.OnPurchasePending += OnPurchasePending;
            storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            storeController.OnPurchaseFailed += OnPurchaseFailed;
            storeController.OnPurchasesFetched += OnPurchasesFetched;

            // Bổ sung 3 dòng này để sửa lỗi báo thiếu Callback trong ảnh của bạn
            storeController.OnStoreConnected += OnStoreConnected;
            storeController.OnStoreDisconnected += OnStoreDisconnected;
            storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed; // Hàm này xử lý lỗi lịch sử mua

            await storeController.Connect();
            var products = new List<ProductDefinition> { new ProductDefinition(noAdsProductId, ProductType.NonConsumable) };
            storeController.FetchProducts(products);

            isInitialized = true;
        }
        catch (System.Exception ex) { Debug.LogError($"[PremiumManager] Lỗi khởi tạo IAP: {ex.Message}"); }
    }

    // Bổ sung thêm các hàm trống này ở cuối file PremiumManager.cs để bắt sự kiện:
    // =======================================
    // BỔ SUNG CÁC HÀM BẮT SỰ KIỆN CÒN THIẾU CỦA IAP v5
    // =======================================

    private void OnStoreConnected()
    {
        Debug.Log("[PremiumManager] Đã kết nối tới Store thành công.");
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.Log($"[PremiumManager] Ngắt kết nối Store. Lý do: {description.message}");
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription description)
    {
        Debug.Log($"[PremiumManager] Lấy lịch sử mua thất bại. Lý do: {description.message}");
    }
    public void BuyPremium()
    {
        if (isInitialized && storeController != null) storeController.PurchaseProduct(noAdsProductId);
    }

    private void OnProductsFetched(List<Product> products) => storeController.FetchPurchases();
    private void OnProductsFetchFailed(ProductFetchFailed failure) { }
    private void OnPurchasesFetched(Orders orders)
    {
        foreach (var order in orders.ConfirmedOrders)
        {
            foreach (var pInfo in order.Info.PurchasedProductInfo)
            {
                if (pInfo.productId == noAdsProductId) { GrantPremium(); }
            }
        }
    }

    private void OnPurchasePending(PendingOrder pendingOrder)
    {
        foreach (var pInfo in pendingOrder.Info.PurchasedProductInfo)
        {
            if (pInfo.productId == noAdsProductId)
            {
                GrantPremium();
                storeController.ConfirmPurchase(pendingOrder);
            }
        }
    }
    private void OnPurchaseConfirmed(Order order) { }
    private void OnPurchaseFailed(FailedOrder failedOrder) { }
}