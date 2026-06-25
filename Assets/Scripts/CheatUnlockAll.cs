using UnityEngine;

// GẮN LÊN BẤT KỲ GAMEOBJECT NÀO TRONG SCENE MAINMENU
// Ẩn nút gọi hàm này trước khi build chính thức
public class CheatUnlockAll : MonoBehaviour
{
    public void MoKhoaTatCa()
    {
        int chuongToiDa  = GameManager.instance != null ? GameManager.instance.chuongToiDa  : 2;
        int soManMoiChuong = GameManager.instance != null ? GameManager.instance.soManMoiChuong : 8;

        // === LEVELS ===
        PlayerPrefs.SetInt("Level_S_0_unlocked", 1);
        PlayerPrefs.SetInt("Chapter_Special_unlocked", 1);

        string levelCuoiCung = "Level_S_0";
        for (int chuong = 1; chuong <= chuongToiDa; chuong++)
            for (int man = 1; man <= soManMoiChuong; man++)
            {
                string key = $"Level_{chuong}_{man}";
                PlayerPrefs.SetInt(key + "_unlocked", 1);
                PlayerPrefs.SetInt(key + "_passed",   1);
                levelCuoiCung = key;
            }

        PlayerPrefs.SetString("LatestLevel", levelCuoiCung);

        // === SKINS ===
        string[] skins = { "Default", "Ghost", "Forest", "Ember", "Void",
                           "Skeleton", "Golden", "IceMan", "Lava", "Robot" };
        foreach (string id in skins)
            PlayerPrefs.SetInt("Skin_" + id + "_unlocked", 1);

        // === ACHIEVEMENTS (nếu có) ===
        PlayerPrefs.SetInt("TotalDeaths", 500);

        PlayerPrefs.Save();

        // Reload lại SkinManager để nhận unlock mới
        if (SkinManager.instance != null)
            SkinManager.instance.ReloadUnlocks();

        Debug.Log("[Cheat] Đã mở khoá tất cả level và skin!");
    }
}
