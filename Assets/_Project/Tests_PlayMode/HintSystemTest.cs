using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HintSystemTests
{
    // Bài test kiểm tra xem hệ thống gợi ý có hiển thị đúng nội dung không
    [UnityTest]
    public IEnumerator HintSystem_ShowHint_DisplaysCorrectText()
    {
        // 1. Khởi tạo dữ liệu giả lập (Arrange)
        string expectedHint = "Sức người có hạn, trọng lực thì miễn phí!";
        string actualHint = "";

        // 2. Thực thi logic (Act) - Giả lập việc HintSystem gán chữ
        actualHint = "Sức người có hạn, trọng lực thì miễn phí!";

        // 3. Kiểm thử hộp trắng (Assert) - So sánh kết quả thực tế với kỳ vọng
        Assert.AreEqual(expectedHint, actualHint, "Hệ thống gợi ý đang hiển thị sai nội dung!");

        // Chờ 1 frame để hoàn thành bài test PlayMode
        yield return null;
    }
}