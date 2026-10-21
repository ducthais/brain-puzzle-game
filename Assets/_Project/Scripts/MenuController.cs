using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Dùng để load màn chơi sau này

public class MenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loadingPanel;
    public GameObject startPanel;
    public GameObject confirmDialogPanel; // Màn hình xác nhận (board.png)

    [Header("Loading UI")]
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText;

    [Header("Settings")]
    public float loadTime = 3f; // Thời gian chạy thanh loading (giả lập 3 giây)

    void Start()
    {
        // Khi vừa mở game: Hiện Loading, Ẩn Start và Confirm
        loadingPanel.SetActive(true);
        startPanel.SetActive(false);
        if (confirmDialogPanel != null) confirmDialogPanel.SetActive(false);

        // Bắt đầu chạy tiến trình load
        StartCoroutine(SimulateLoading());
    }

    // Coroutine giả lập thời gian tải game
    IEnumerator SimulateLoading()
    {
        float timer = 0f;

        while (timer < loadTime)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / loadTime); // Tính % từ 0 đến 1

            // Cập nhật thanh Slider và Text
            loadingSlider.value = progress;
            loadingText.text = "LOADING... " + (progress * 100f).ToString("F0") + "%";

            yield return null; // Đợi frame tiếp theo
        }

        // Khi load xong 100%
        loadingSlider.value = 1f;
        loadingText.text = "LOADING... 100%";

        yield return new WaitForSeconds(0.5f); // Dừng lại nửa giây cho người chơi nhìn thấy 100%

        // Ẩn thanh Loading, Hiện nút PLAY
        loadingPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    // Hàm này được gọi khi bấm nút CONTINUE
    public void OnContinueButtonClicked()
    {
        Debug.Log("Đang tải LevelSelect với tiến trình đã lưu");
        // Tải Scene LevelSelect (tiến trình được lưu qua ProgressService)
        SceneManager.LoadScene("LevelSelect");
    }

    // Hàm này được gọi khi bấm nút NEW GAME
    public void OnNewGameButtonClicked()
    {
        // Hiện màn hình xác nhận New Game
        if (confirmDialogPanel != null)
        {
            confirmDialogPanel.SetActive(true);
        }
    }

    // Hàm này được gọi khi bấm "Có" trong bảng xác nhận New Game
    public void OnConfirmYesClicked()
    {
        // Reset toàn bộ tiến trình game (giữ nguyên Premium)
        ProgressService.ResetProgress();
        LevelSession.Clear();

        Debug.Log("Đã reset tiến trình game. Đang tải LevelSelect");

        // Ẩn bảng xác nhận (để gọn gàng trước khi load)
        if (confirmDialogPanel != null) confirmDialogPanel.SetActive(false);

        // Tải Scene LevelSelect
        SceneManager.LoadScene("LevelSelect");
    }

    // Hàm này được gọi khi bấm "Không" trong bảng xác nhận New Game
    public void OnConfirmNoClicked()
    {
        // Ẩn màn hình xác nhận, quay lại startPanel
        if (confirmDialogPanel != null)
        {
            confirmDialogPanel.SetActive(false);
        }
    }
}