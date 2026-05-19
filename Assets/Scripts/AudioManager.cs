using UnityEngine;
using UnityEngine.SceneManagement;

// Gắn vào GameManager hoặc 1 GO riêng trong scene
// Tự detect scene đang chạy và play nhạc phù hợp
public class AudioManager : MonoBehaviour
{
    void Start()
    {
        if (SoundManager.instance == null) return;
        XacDinhVaPlayNhac();
    }

    void XacDinhVaPlayNhac()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (scene == "MainMenu" || scene.StartsWith("Level_1_"))
            SoundManager.instance.PlayNhacMenuCh1();
        // Các chương khác sẽ thêm sau khi có nhạc
    }
}