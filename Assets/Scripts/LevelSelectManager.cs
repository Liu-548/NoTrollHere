using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [Header("=== UI THAM CHIẾU ===")]
    public TextMeshProUGUI txtTenChuong;
    public TextMeshProUGUI txtMoTaChuong;
    public GameObject[] cacNutLevel;
    public Button nutTrai;
    public Button nutPhai;

    [Header("=== CÀI ĐẶT ===")]
    public string tenSceneMainMenu = "MainMenu";

    private readonly string[] tenCacChuong = { "CHAPTER 1", "SPECIAL" };
    private readonly string[] moTaCacChuong = { "choose your suffering", "something special" };
    private readonly string[] prefixCacChuong = { "Level_1_", "Level_S_" };
    private readonly int[] soManCacChuong = { 10, 1 };

    private readonly Color mauNenDaMo = new Color(0.18f, 0.18f, 0.18f, 1f);
    private readonly Color mauNenDangChoi = new Color(0.16f, 0.13f, 0.00f, 1f);
    private readonly Color mauNenLock = new Color(0.11f, 0.11f, 0.11f, 1f);
    private readonly Color mauVienDaMo = new Color(0.23f, 0.23f, 0.23f, 1f);
    private readonly Color mauVienDangChoi = new Color(0.96f, 0.78f, 0.26f, 1f);
    private readonly Color mauVienLock = new Color(0.14f, 0.14f, 0.14f, 1f);
    private readonly Color mauChuDaMo = new Color(0.91f, 0.91f, 0.82f, 1f);
    private readonly Color mauChuDangChoi = new Color(0.96f, 0.78f, 0.26f, 1f);
    private readonly Color mauChuLock = new Color(0.20f, 0.20f, 0.20f, 1f);

    private int chuongHienTai = 0;
    private string levelMoiNhat = "Level_1_1";

    void Start()
    {
        if (GameManager.instance != null)
            levelMoiNhat = GameManager.instance.LayLevelMoiNhat();
        else
            levelMoiNhat = PlayerPrefs.GetString("LatestLevel", "Level_1_1");

        chuongHienTai = levelMoiNhat.StartsWith("Level_S_") ? 1 : 0;
        HienThiChuong(chuongHienTai);
    }

    void HienThiChuong(int idx)
    {
        bool duocXem = KiemTraDuocXem(idx);

        txtTenChuong.text = "— " + tenCacChuong[idx] + " —";
        txtMoTaChuong.text = duocXem ? moTaCacChuong[idx] : "???";

        CapNhatMuiTen(idx);

        string prefix = prefixCacChuong[idx];
        int soMan = soManCacChuong[idx];

        for (int i = 0; i < cacNutLevel.Length; i++)
        {
            bool hienThi = i < soMan;
            cacNutLevel[i].SetActive(hienThi);
            if (!hienThi) continue;

            int soThuTu = i + 1;
            string tenLevel = prefix + soThuTu;

            bool daUnlock = duocXem && KiemTraUnlock(tenLevel);
            bool daPassed = PlayerPrefs.GetInt(tenLevel + "_passed", 0) == 1;
            bool laMoiNhat = tenLevel == levelMoiNhat;

            Image img = cacNutLevel[i].GetComponent<Image>();
            Outline outline = cacNutLevel[i].GetComponent<Outline>();
            Button btn = cacNutLevel[i].GetComponent<Button>();
            TextMeshProUGUI txtSo = cacNutLevel[i]
                .transform.Find("Txt_So")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI txtIcon = cacNutLevel[i]
                .transform.Find("Txt_Icon")?.GetComponent<TextMeshProUGUI>();

            if (!daUnlock)
            {
                SetNut(img, outline, btn, txtSo, txtIcon,
                    mauNenLock, mauVienLock, mauChuLock,
                    soThuTu.ToString(), "X", false);
            }
            else if (daPassed)
            {
                SetNut(img, outline, btn, txtSo, txtIcon,
                    mauNenDaMo, mauVienDaMo, mauChuDaMo,
                    soThuTu.ToString(), "*", true);
                GanOnClick(btn, tenLevel);
            }
            else if (laMoiNhat)
            {
                SetNut(img, outline, btn, txtSo, txtIcon,
                    mauNenDangChoi, mauVienDangChoi, mauChuDangChoi,
                    soThuTu.ToString(), "", true);
                GanOnClick(btn, tenLevel);
            }
            else
            {
                SetNut(img, outline, btn, txtSo, txtIcon,
                    mauNenDaMo, mauVienDaMo, mauChuDaMo,
                    soThuTu.ToString(), "", true);
                GanOnClick(btn, tenLevel);
            }
        }
    }

    void SetNut(Image img, Outline outline, Button btn,
        TextMeshProUGUI txtSo, TextMeshProUGUI txtIcon,
        Color mauNen, Color mauVien, Color mauChu,
        string so, string icon, bool tuongTac)
    {
        if (img != null) img.color = mauNen;
        if (outline != null) outline.effectColor = mauVien;
        btn.interactable = tuongTac;
        if (txtSo != null) { txtSo.text = so; txtSo.color = mauChu; }
        if (txtIcon != null) { txtIcon.text = icon; txtIcon.color = mauChu; }
    }

    void GanOnClick(Button btn, string tenLevel)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SceneManager.LoadScene(tenLevel));
    }

    void CapNhatMuiTen(int idx)
    {
        if (nutTrai != null)
        {
            nutTrai.interactable = idx > 0;
            CanvasGroup cg = nutTrai.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = idx > 0 ? 1f : 0.25f;
        }
        if (nutPhai != null)
        {
            bool coManPhia = idx < tenCacChuong.Length - 1;
            nutPhai.interactable = coManPhia;
            CanvasGroup cg = nutPhai.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = coManPhia ? 1f : 0.25f;
        }
    }

    bool KiemTraDuocXem(int idx)
    {
        if (idx == 0) return true;
        if (idx == 1)
        {
            if (GameManager.instance != null)
                return GameManager.instance.LaChapterSpecialDaMo();
            return PlayerPrefs.GetInt("Chapter_Special_unlocked", 0) == 1;
        }
        return false;
    }

    bool KiemTraUnlock(string tenLevel)
    {
        if (GameManager.instance != null)
            return GameManager.instance.LaLevelDaUnlock(tenLevel);
        return PlayerPrefs.GetInt(tenLevel + "_unlocked", 0) == 1;
    }

    public void NutTrai()
    {
        if (chuongHienTai > 0)
        {
            chuongHienTai--;
            HienThiChuong(chuongHienTai);
        }
    }

    public void NutPhai()
    {
        if (chuongHienTai < tenCacChuong.Length - 1)
        {
            chuongHienTai++;
            HienThiChuong(chuongHienTai);
        }
    }

    public void NutBack()
    {
        SceneManager.LoadScene(tenSceneMainMenu);
    }
}