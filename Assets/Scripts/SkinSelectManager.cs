using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        if (SkinManager.instance == null) return;

        danhSach = SkinManager.instance.LayDanhSach();

        // Bắt đầu ở skin đang dùng
        string skinDangDung = SkinManager.instance.LaySkinDangDung();
        indexHienTai = danhSach.FindIndex(s => s.id == skinDangDung);
        if (indexHienTai < 0) indexHienTai = 0;

        HienThiSkin(indexHienTai);
    }

    void HienThiSkin(int idx)
    {
        if (danhSach == null || danhSach.Count == 0) return;

        var skin = danhSach[idx];

        // Preview
        Sprite sp = SkinManager.instance.LaySpriteSkin(skin.id);
        if (sp != null) imgPreview.sprite = sp;
        imgPreview.color = skin.daUnlock
            ? Color.white
            : new Color(0.2f, 0.2f, 0.2f);

        // Tên
        txtTenSkin.text = skin.ten;
        txtTenSkin.color = skin.daUnlock
            ? new Color(0.91f, 0.91f, 0.82f)
            : new Color(0.3f, 0.3f, 0.3f);

        // Mô tả
        txtMoTa.text = skin.daUnlock ? skin.moTa : "???";

        // Điều kiện
        if (skin.daUnlock)
        {
            // Đang dùng skin này chưa?
            bool dangDung = SkinManager.instance.LaySkinDangDung() == skin.id;
            txtDieuKien.text = dangDung ? "✓ Đang dùng" : "";
            txtDieuKien.color = new Color(0.96f, 0.78f, 0.26f);
        }
        else
        {
            txtDieuKien.text = "🔒 " + skin.dieuKienMo;
            txtDieuKien.color = new Color(0.3f, 0.3f, 0.3f);
        }

        // Nút chọn
        nutChon.interactable = skin.daUnlock;
        nutChon.GetComponent<Image>().color = skin.daUnlock
            ? new Color(0.96f, 0.78f, 0.26f)
            : new Color(0.16f, 0.16f, 0.16f);
        nutChon.GetComponentInChildren<TextMeshProUGUI>().color = skin.daUnlock
            ? new Color(0.1f, 0.1f, 0.1f)
            : new Color(0.3f, 0.3f, 0.3f);

        // Mũi tên
        CapNhatMuiTen(idx);

        // Dots
        CapNhatDots(idx);
    }

    void CapNhatMuiTen(int idx)
    {
        if (nutTrai != null)
        {
            bool coTrai = idx > 0;
            nutTrai.interactable = coTrai;
            CanvasGroup cg = nutTrai.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = coTrai ? 1f : 0.25f;
        }
        if (nutPhai != null)
        {
            bool coPhai = idx < danhSach.Count - 1;
            nutPhai.interactable = coPhai;
            CanvasGroup cg = nutPhai.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = coPhai ? 1f : 0.25f;
        }
    }

    void CapNhatDots(int idx)
    {
        if (cacDot == null) return;
        for (int i = 0; i < cacDot.Length; i++)
        {
            if (cacDot[i] == null) continue;
            if (i >= danhSach.Count)
            {
                cacDot[i].gameObject.SetActive(false);
                continue;
            }
            cacDot[i].gameObject.SetActive(true);

            if (i == idx)
                cacDot[i].color = mauDotActive;
            else if (danhSach[i].daUnlock)
                cacDot[i].color = mauDotInactive;
            else
                cacDot[i].color = mauDotLock;
        }
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