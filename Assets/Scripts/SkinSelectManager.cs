using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class SkinSelectManager : MonoBehaviour
{
    [Header("=== UI ===")]
    public Image imgPreview;
    public TextMeshProUGUI txtTenSkin;
    public TextMeshProUGUI txtMoTa;
    public TextMeshProUGUI txtDieuKien;
    public Button nutTrai;
    public Button nutPhai;
    public Button nutChon;
    public Button nutBack;
    public Image[] cacDot;

    [Header("=== CÀI ĐẶT ===")]
    public string tenSceneMainMenu = "MainMenu";

    private List<SkinManager.Skin> danhSach;
    private int indexHienTai = 0;

    // Màu dot
    private readonly Color mauDotActive = new Color(0.96f, 0.78f, 0.26f);
    private readonly Color mauDotInactive = new Color(0.2f, 0.2f, 0.2f);
    private readonly Color mauDotLock = new Color(0.12f, 0.12f, 0.12f);

    void Start()
    {
        if (SkinManager.instance == null)
        {
            Debug.LogWarning("SkinManager chưa có — load từ MainMenu trước!");
            return;
        }

        danhSach = SkinManager.instance.LayDanhSach();

        // TMP text mặc định có raycastTarget = true → chặn click nút bên dưới
        if (txtTenSkin   != null) txtTenSkin.raycastTarget   = false;
        if (txtMoTa      != null) txtMoTa.raycastTarget      = false;
        if (txtDieuKien  != null) txtDieuKien.raycastTarget  = false;

        // Tự wire lại tất cả buttons theo tên — bỏ qua giá trị Inspector để tránh gán sai
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
            {
                switch (btn.gameObject.name)
                {
                    case "Btn_Trai": nutTrai = btn; break;
                    case "Btn_Phai": nutPhai = btn; break;
                    case "Btn_Chon": nutChon = btn; break;
                    case "Btn_Back": nutBack = btn; break;
                }
            }
        }

        WireButton(nutTrai, NutTrai);
        WireButton(nutPhai, NutPhai);
        WireButton(nutChon, NutChon);
        WireButton(nutBack, NutBack);

        string skinDangDung = SkinManager.instance.LaySkinDangDung();
        indexHienTai = danhSach.FindIndex(s => s.id == skinDangDung);
        if (indexHienTai < 0) indexHienTai = 0;

        HienThiSkin(indexHienTai);
    }

    void HienThiSkin(int idx)
    {
        if (danhSach == null || danhSach.Count == 0) return;
        if (SkinManager.instance == null) return;

        var skin = danhSach[idx];

        // Preview — check null trước
        if (imgPreview != null)
        {
            Sprite sp = SkinManager.instance.LaySpriteSkin(skin.id);
            if (sp != null) imgPreview.sprite = sp;
            imgPreview.color = skin.daUnlock
                ? Color.white
                : new Color(0.2f, 0.2f, 0.2f);
        }

        if (txtTenSkin != null)
        {
            txtTenSkin.text = skin.ten;
            txtTenSkin.color = skin.daUnlock
                ? new Color(0.91f, 0.91f, 0.82f)
                : new Color(0.3f, 0.3f, 0.3f);
        }

        if (txtMoTa != null)
            txtMoTa.text = skin.daUnlock ? skin.moTa : "???";

        if (txtDieuKien != null)
        {
            if (skin.daUnlock)
            {
                string dangDungId = SkinManager.instance != null
                    ? SkinManager.instance.LaySkinDangDung()
                    : "";
                txtDieuKien.text = dangDungId == skin.id ? "Đang dùng" : "";
                txtDieuKien.color = new Color(0.96f, 0.78f, 0.26f);
            }
            else
            {
                txtDieuKien.text = skin.dieuKienMo;
                txtDieuKien.color = new Color(0.3f, 0.3f, 0.3f);
            }
        }

        if (nutChon != null)
        {
            nutChon.interactable = skin.daUnlock;
            nutChon.GetComponent<Image>().color = skin.daUnlock
                ? new Color(0.96f, 0.78f, 0.26f)
                : new Color(0.16f, 0.16f, 0.16f);
            var txt = nutChon.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
                txt.color = skin.daUnlock
                    ? new Color(0.1f, 0.1f, 0.1f)
                    : new Color(0.3f, 0.3f, 0.3f);
        }

        CapNhatMuiTen(idx);
        CapNhatDots(idx);
    }

    void CapNhatMuiTen(int idx)
    {
        if (nutTrai != null)
        {
            bool coTrai = idx > 0;
            nutTrai.interactable = coTrai;
            CanvasGroup cg = nutTrai.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = coTrai ? 1f : 0.25f;
        }
        if (nutPhai != null)
        {
            bool coPhai = idx < danhSach.Count - 1;
            nutPhai.interactable = coPhai;
            CanvasGroup cg = nutPhai.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = coPhai ? 1f : 0.25f;
        }

        // Đảm bảo nutBack luôn hiển thị, độc lập với alpha của Btn_Trai parent
        if (nutBack != null)
        {
            nutBack.interactable = true;
            var img = nutBack.GetComponent<Image>();
            if (img != null) img.color = new Color(0.16f, 0.16f, 0.16f, 1f);

            // Thêm CanvasGroup riêng nếu chưa có, set ignoreParentGroups
            // để không bị ảnh hưởng bởi alpha của Btn_Trai
            var cg2 = nutBack.GetComponent<CanvasGroup>();
            if (cg2 == null) cg2 = nutBack.gameObject.AddComponent<CanvasGroup>();
            cg2.alpha = 1f;
            cg2.ignoreParentGroups = true;
            cg2.interactable = true;
            cg2.blocksRaycasts = true;
        }
    }

    void CapNhatDots(int idx)
    {
        if (cacDot == null || cacDot.Length == 0 || danhSach == null) return;

        int total      = danhSach.Count;
        int soHienThi  = cacDot.Length; // thường là 5

        // Cửa sổ cuộn: căn giữa idx, kẹp cạnh
        int start = Mathf.Clamp(idx - soHienThi / 2, 0, Mathf.Max(0, total - soHienThi));

        for (int i = 0; i < soHienThi; i++)
        {
            if (cacDot[i] == null) continue;
            int skinIdx = start + i;

            if (skinIdx >= total)
            {
                cacDot[i].gameObject.SetActive(false);
                continue;
            }
            cacDot[i].gameObject.SetActive(true);

            if (skinIdx == idx)
                cacDot[i].color = mauDotActive;
            else if (danhSach[skinIdx].daUnlock)
                cacDot[i].color = mauDotInactive;
            else
                cacDot[i].color = mauDotLock;
        }
    }

    void WireButton(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        // Nếu đã có persistent listener → không thêm runtime để tránh gọi 2 lần
        if (btn.onClick.GetPersistentEventCount() == 0)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
        }
    }

    private readonly MenuKeyHold holdTrai = new MenuKeyHold(UnityEngine.InputSystem.Key.A, UnityEngine.InputSystem.Key.LeftArrow);
    private readonly MenuKeyHold holdPhai = new MenuKeyHold(UnityEngine.InputSystem.Key.D, UnityEngine.InputSystem.Key.RightArrow);

    void Update()
    {
        if (danhSach == null) return;

        float dt    = Time.unscaledDeltaTime;
        bool diTrai = holdTrai.Update(dt);
        bool diPhai = holdPhai.Update(dt);
        bool ok     = GameInput.instance != null && GameInput.instance.ConfirmDown;
        bool back   = GameInput.instance != null && GameInput.instance.EscapeDown;

        if (diTrai) NutTrai();
        if (diPhai) NutPhai();
        if (ok)     NutChon();
        if (back)   NutBack();
    }

    public void NutTrai()
    {
        if (indexHienTai > 0)
        {
            indexHienTai--;
            HienThiSkin(indexHienTai);
        }
    }

    public void NutPhai()
    {
        if (indexHienTai < danhSach.Count - 1)
        {
            indexHienTai++;
            HienThiSkin(indexHienTai);
        }
    }

    public void NutChon()
    {
        if (danhSach == null || indexHienTai >= danhSach.Count) return;
        string id = danhSach[indexHienTai].id;
        SkinManager.instance.ChonSkin(id);
        // Refresh để hiện "✓ Đang dùng"
        HienThiSkin(indexHienTai);
    }

    public void NutBack()
    {
        SceneManager.LoadScene(tenSceneMainMenu);
    }
}