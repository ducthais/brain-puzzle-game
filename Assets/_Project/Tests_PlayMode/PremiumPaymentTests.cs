using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class PremiumPaymentTests
{
    private GameObject shopObj;
    private ShopManager shopManager;

    [SetUp]
    public void Setup()
    {
        // Khởi tạo cụm đối tượng để phục vụ môi trường kiểm thử độc lập
        shopObj = new GameObject("ShopRoot");
        shopManager = shopObj.AddComponent<ShopManager>();

        // Cấp phát các GameObject ảo gán vào ShopManager để tránh lỗi bẻ gãy luồng NullReferenceException
        shopManager.panelShopMain = new GameObject("Panel_ShopMain");
        shopManager.panelIsPremium = new GameObject("Panel_IsPremium");
        shopManager.panelQRCode = new GameObject("Panel_QRCode");

        shopManager.panelShopMain.SetActive(false);
        shopManager.panelIsPremium.SetActive(false);
        shopManager.panelQRCode.SetActive(false);

        // Tạo thực thể PremiumManager Singleton nếu ngữ cảnh Test Editor chưa khởi tạo
        if (GameObject.FindObjectOfType<PremiumManager>() == null)
        {
            GameObject pmObj = new GameObject("PremiumManager");
            pmObj.AddComponent<PremiumManager>();
        }

        // Khởi tạo mặc định: Ép trạng thái ban đầu về tài khoản Free thường
        PlayerPrefs.SetInt("isPremium", 0);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void Teardown()
    {
        // Thu hồi tài nguyên làm sạch bộ nhớ Unity Editor sau mỗi ca test
        if (shopObj != null) Object.DestroyImmediate(shopObj);

        var pm = GameObject.FindObjectOfType<PremiumManager>();
        if (pm != null) Object.DestroyImmediate(pm.gameObject);

        PlayerPrefs.DeleteKey("isPremium");
        PlayerPrefs.Save();
    }

    // Test cho TC_BQ_4.6_01 & 02: Xác minh tài khoản Free khi mở cửa hàng phải hiển thị Panel_ShopMain
    [Test]
    public void Test_OpenShop_FreeUser_DisplaysShopMainPanel()
    {
        // Thực thi mở shop khi tài khoản đang là Free thường
        shopManager.OpenShop();

        Assert.IsTrue(shopManager.panelShopMain.activeSelf, "Lỗi: Tài khoản Free khi vào Shop phải hiển thị giao diện Non-Premium!");
        Assert.IsFalse(shopManager.panelIsPremium.activeSelf, "Lỗi: Tài khoản Free không được phép kích hoạt hiển thị Panel_IsPremium!");
    }

    // Test cho TC_BQ_4.6_03: Xác minh khi nhận tín hiệu đồng bộ VIP thành công, Shop tự động cập nhật sang Panel_IsPremium
    [Test]
    public void Test_GrantPremium_SyncsShopToPremiumPanel()
    {
        // Giả lập luồng kích hoạt VIP thành công sau khi MoMo Sandbox phản hồi lệnh SUCCESS
        if (PremiumManager.Instance != null)
        {
            PremiumManager.Instance.GrantPremium();
        }

        // Gọi hàm đồng bộ làm mới lại giao diện của ShopManager
        shopManager.OpenShop();

        Assert.IsTrue(shopManager.panelIsPremium.activeSelf, "Lỗi: Đã kích hoạt Premium thành công nhưng Shop chưa chuyển sang giao diện Is-Premium!");
        Assert.IsFalse(shopManager.panelShopMain.activeSelf, "Lỗi: Đã lên đặc quyền VIP nhưng màn hình quảng cáo Non-Premium cũ vẫn hiển thị công khai!");
    }

    // Test cho TC_BQ_4.6_04: Xác minh khi bấm hủy thanh toán quét mã QR -> Hệ thống đóng QR và đưa UI an toàn về màn Shop gốc
    [Test]
    public void Test_CancelAndCloseQR_SafelyReturnsToShopMainPanel()
    {
        // Giả lập bối cảnh người chơi đang đứng tại giao diện quét mã QR MoMo
        shopManager.SwitchPanel("Panel_QRCode");
        Assert.IsTrue(shopManager.panelQRCode.activeSelf, "Lỗi thiết lập: Giao diện QR chưa thể kích hoạt thành công!");

        // Thực thi hành động bấm nút X/Hủy bỏ giao dịch thanh toán
        shopManager.CancelAndCloseQR();

        Assert.IsFalse(shopManager.panelQRCode.activeSelf, "Lỗi: Giao diện hiển thị mã QR MoMo chưa được đóng sau khi người dùng ra lệnh Hủy!");
        Assert.IsTrue(shopManager.panelShopMain.activeSelf, "Lỗi: Sau khi hủy quét mã QR, hệ thống không tự động chuyển hướng trả UI về màn hình Shop gốc!");
    }
}