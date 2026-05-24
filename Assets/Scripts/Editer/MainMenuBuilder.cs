using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

public class MainMenuBuilder : MonoBehaviour
{
    [MenuItem("NoTrollHere/Build Main Menu UI")]
    static void BuildMainMenuUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas!");
            return;
        }

        if (canvas.GetComponent<CanvasGroup>() == null)
            canvas.gameObject.AddComponent<CanvasGroup>();

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // === BACKGROUND ===
        GameObject bg = TaoPanel("Background", canvas.transform);
        CaiDatStretchFull(bg.GetComponent<RectTransform>());
        bg.GetComponent<Image>().color = HexToColor("1A1A1A");
        bg.transform.SetSiblingIndex(0);

        // === CHALK PARTICLE ===
        GameObject particles = new GameObject("ChalkParticleSystem");
        particles.transform.SetParent(canvas.transform, false);
        CaiDatStretchFull(particles.AddComponent<RectTransform>());
        particles.AddComponent<ChalkParticle>();

        // === PANEL CENTER ===
        GameObject panelCenter = new GameObject("Panel_Center");
        panelCenter.transform.SetParent(canvas.transform, false);
        RectTransform pcRect = panelCenter.AddComponent<RectTransform>();
        pcRect.anchorMin = new Vector2(0.5f, 0.5f);
        pcRect.anchorMax = new Vector2(0.5f, 0.5f);
        pcRect.pivot     = new Vector2(0.5f, 0.5f);
        pcRect.sizeDelta = new Vector2(320f, 480f);
        pcRect.anchoredPosition = Vector2.zero;
        VerticalLayoutGroup vlg = panelCenter.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment      = TextAnchor.MiddleCenter;
        vlg.spacing             = 10f;
        vlg.childControlWidth   = true;
        vlg.childControlHeight  = false;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(0, 0, 0, 0);
        panelCenter.AddComponent<Image>().color = Color.clear;

        TaoTMP("Txt_Pretitle", panelCenter.transform,
            "a troll platformer", 13f, "444444", 30f, TextAlignmentOptions.Center);

        GameObject titleGO = TaoTMP("Txt_Title", panelCenter.transform,
            "No<color=#F5C842>Troll</color>Here", 52f, "E8E8D0", 65f,
            TextAlignmentOptions.Center);
        titleGO.GetComponent<TextMeshProUGUI>().richText  = true;
        titleGO.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject sep = new GameObject("Img_Separator");
        sep.transform.SetParent(panelCenter.transform, false);
        sep.AddComponent<RectTransform>().sizeDelta = new Vector2(60f, 2f);
        sep.AddComponent<Image>().color = HexToColor("333333");
        LayoutElement sepLE = sep.AddComponent<LayoutElement>();
        sepLE.preferredHeight = 2f;
        sepLE.preferredWidth  = 60f;
        sepLE.flexibleWidth   = 0f;

        TaoTMP("Txt_Tagline", panelCenter.transform,
            "there are definitely no traps here", 12f, "444444", 25f,
            TextAlignmentOptions.Center);
        TaoSpacer(panelCenter.transform, 20f);

        // === CÁC NÚT ===
        GameObject btnPlay = TaoNut("Btn_Play", panelCenter.transform,
            "Play", "F5C842", "1A1A1A", 48f, true);
        GameObject btnLevel = TaoNut("Btn_LevelSelect", panelCenter.transform,
            "Level Select", "1A1A1A", "E8E8D0", 44f, false);
        GameObject btnSkins = TaoNut("Btn_Skins", panelCenter.transform,
            "Skins", "1A1A1A", "E8E8D0", 44f, false);
        GameObject btnSettings = TaoNut("Btn_Settings", panelCenter.transform,
            "Settings", "1A1A1A", "888888", 40f, false, true);

        // === FOOTER ===
        GameObject footer = TaoTMP("Txt_Footer", canvas.transform,
            "DEATHS THIS SESSION : 0", 11f, "2E2E2E", 25f,
            TextAlignmentOptions.Center);
        RectTransform footerRect = footer.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0f, 0f);
        footerRect.anchorMax = new Vector2(1f, 0f);
        footerRect.pivot     = new Vector2(0.5f, 0f);
        footerRect.anchoredPosition = new Vector2(0f, 20f);
        footerRect.sizeDelta = new Vector2(0f, 25f);

        // === PANEL SETTINGS ===
        GameObject panelSettings = TaoPanel("Panel_Settings", canvas.transform);
        RectTransform psRect = panelSettings.GetComponent<RectTransform>();
        psRect.anchorMin = new Vector2(0.5f, 0.5f);
        psRect.anchorMax = new Vector2(0.5f, 0.5f);
        psRect.pivot     = new Vector2(0.5f, 0.5f);
        psRect.sizeDelta = new Vector2(400f, 420f);
        psRect.anchoredPosition = Vector2.zero;
        panelSettings.GetComponent<Image>().color = HexToColor("111111");
        Outline psOutline = panelSettings.AddComponent<Outline>();
        psOutline.effectColor    = HexToColor("2E2E2E");
        psOutline.effectDistance = new Vector2(2f, -2f);
        panelSettings.SetActive(false);

        VerticalLayoutGroup psVlg = panelSettings.AddComponent<VerticalLayoutGroup>();
        psVlg.childAlignment      = TextAnchor.MiddleCenter;
        psVlg.spacing             = 16f;
        psVlg.childControlWidth   = true;
        psVlg.childControlHeight  = false;
        psVlg.childForceExpandWidth  = true;
        psVlg.childForceExpandHeight = false;
        psVlg.padding = new RectOffset(30, 30, 24, 24);

        TaoTMP("Txt_SettingsTitle", panelSettings.transform,
            "SETTINGS", 20f, "E8E8D0", 36f, TextAlignmentOptions.Center);
        TaoTMP("Txt_SFX", panelSettings.transform,
            "SFX VOLUME", 12f, "888888", 22f, TextAlignmentOptions.Left);
        GameObject sliderSFXGO = TaoSlider("Slider_SFX", panelSettings.transform);
        TaoTMP("Txt_Music", panelSettings.transform,
            "MUSIC VOLUME", 12f, "888888", 22f, TextAlignmentOptions.Left);
        GameObject sliderMusicGO = TaoSlider("Slider_Music", panelSettings.transform);
        GameObject rowFull = TaoRowToggle("Row_Fullscreen",
            panelSettings.transform, "FULLSCREEN");
        TaoSpacer(panelSettings.transform, 8f);
        GameObject btnClose = TaoNut("Btn_CloseSettings", panelSettings.transform,
            "Close", "1A1A1A", "E8E8D0", 40f, false);

        // === MAIN MENU MANAGER ===
        GameObject mmGO = new GameObject("MainMenuManager");
        mmGO.transform.SetParent(canvas.transform.parent, false);
        MainMenuManager mm = mmGO.AddComponent<MainMenuManager>();
        mm.canvasGroup = canvas.GetComponent<CanvasGroup>();

        // === SETTINGS MENU ===
        GameObject smGO = new GameObject("SettingsMenu");
        smGO.transform.SetParent(canvas.transform.parent, false);
        SettingsMenu sm = smGO.AddComponent<SettingsMenu>();
        sm.panelSettings = panelSettings;
        sm.sliderSFX     = sliderSFXGO.GetComponent<Slider>();
        sm.sliderMusic   = sliderMusicGO.GetComponent<Slider>();
        Toggle tog = rowFull.GetComponentInChildren<Toggle>();
        sm.toggleFullscreen = tog;

        // === DEATH COUNTER ===
        TotalDeathCounterUI dc = footer.AddComponent<TotalDeathCounterUI>();
        dc.deathText = footer.GetComponent<TextMeshProUGUI>();

        // === GẮN ONCLICK ===
        btnPlay.GetComponent<Button>().onClick.AddListener(mm.NutPlay);
        btnLevel.GetComponent<Button>().onClick.AddListener(mm.NutLevelSelect);
        btnSkins.GetComponent<Button>().onClick.AddListener(mm.NutSkinSelect);
        btnSettings.GetComponent<Button>().onClick.AddListener(sm.NutMoSettings);
        btnClose.GetComponent<Button>().onClick.AddListener(sm.NutDongSettings);
        sliderSFXGO.GetComponent<Slider>().onValueChanged
            .AddListener(sm.DoiVolumeSFX);
        sliderMusicGO.GetComponent<Slider>().onValueChanged
            .AddListener(sm.DoiVolumeMusic);
        tog.onValueChanged.AddListener(sm.DoiFullscreen);

        Debug.Log("✅ Main Menu UI đã được tạo thành công!");
        EditorUtility.SetDirty(canvas.gameObject);
    }

    static GameObject TaoSlider(string ten, Transform parent)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(300f, 28f);
        Slider slider = go.AddComponent<Slider>();
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 1f;

        GameObject bgS = new GameObject("Background");
        bgS.transform.SetParent(go.transform, false);
        RectTransform bgSR = bgS.AddComponent<RectTransform>();
        bgSR.anchorMin = new Vector2(0f, 0.25f);
        bgSR.anchorMax = new Vector2(1f, 0.75f);
        bgSR.offsetMin = bgSR.offsetMax = Vector2.zero;
        bgS.AddComponent<Image>().color = HexToColor("2E2E2E");

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform faR = fillArea.AddComponent<RectTransform>();
        faR.anchorMin = new Vector2(0f, 0.25f);
        faR.anchorMax = new Vector2(1f, 0.75f);
        faR.offsetMin = new Vector2(5f, 0f);
        faR.offsetMax = new Vector2(-5f, 0f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillR = fill.AddComponent<RectTransform>();
        fillR.anchorMin = Vector2.zero;
        fillR.anchorMax = Vector2.one;
        fillR.offsetMin = fillR.offsetMax = Vector2.zero;
        fill.AddComponent<Image>().color = HexToColor("F5C842");

        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        RectTransform haR = handleArea.AddComponent<RectTransform>();
        haR.anchorMin = Vector2.zero; haR.anchorMax = Vector2.one;
        haR.offsetMin = haR.offsetMax = Vector2.zero;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        handle.AddComponent<RectTransform>().sizeDelta = new Vector2(16f, 16f);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = HexToColor("E8E8D0");

        slider.fillRect      = fillR;
        slider.handleRect    = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handleImg;

        go.AddComponent<LayoutElement>().preferredHeight = 28f;
        return go;
    }

    static GameObject TaoRowToggle(string ten, Transform parent, string label)
    {
        GameObject row = new GameObject(ten);
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>().sizeDelta = new Vector2(300f, 30f);
        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.spacing = 10f;
        hlg.childControlHeight    = true;
        hlg.childForceExpandWidth = false;
        row.AddComponent<Image>().color = Color.clear;
        row.AddComponent<LayoutElement>().preferredHeight = 30f;

        GameObject lbl = new GameObject("Label");
        lbl.transform.SetParent(row.transform, false);
        lbl.AddComponent<RectTransform>().sizeDelta = new Vector2(220f, 30f);
        TextMeshProUGUI tmp = lbl.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 12f;
        tmp.color = HexToColor("888888");
        tmp.alignment = TextAlignmentOptions.Left;
        LayoutElement lblLE = lbl.AddComponent<LayoutElement>();
        lblLE.preferredWidth = 220f; lblLE.flexibleWidth = 1f;

        GameObject togGO = new GameObject("Toggle");
        togGO.transform.SetParent(row.transform, false);
        togGO.AddComponent<RectTransform>().sizeDelta = new Vector2(24f, 24f);
        Toggle tog = togGO.AddComponent<Toggle>();
        LayoutElement togLE = togGO.AddComponent<LayoutElement>();
        togLE.preferredWidth = 24f; togLE.preferredHeight = 24f;

        GameObject togBG = new GameObject("Background");
        togBG.transform.SetParent(togGO.transform, false);
        CaiDatStretchFull(togBG.AddComponent<RectTransform>());
        togBG.AddComponent<Image>().color = HexToColor("2E2E2E");

        GameObject check = new GameObject("Checkmark");
        check.transform.SetParent(togBG.transform, false);
        RectTransform checkR = check.AddComponent<RectTransform>();
        CaiDatStretchFull(checkR);
        checkR.offsetMin = new Vector2(3f, 3f);
        checkR.offsetMax = new Vector2(-3f, -3f);
        Image checkImg = check.AddComponent<Image>();
        checkImg.color = HexToColor("F5C842");

        tog.targetGraphic = togBG.GetComponent<Image>();
        tog.graphic = checkImg;
        tog.isOn = Screen.fullScreen;
        return row;
    }

    static GameObject TaoPanel(string ten, Transform parent)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>();
        return go;
    }

    static GameObject TaoTMP(string ten, Transform parent,
        string text, float size, string hex, float height,
        TextAlignmentOptions align)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(300f, height);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size;
        tmp.color = HexToColor(hex);
        tmp.alignment = align; tmp.richText = true;
        go.AddComponent<LayoutElement>().preferredHeight = height;
        return go;
    }

    static GameObject TaoNut(string ten, Transform parent,
        string label, string bgHex, string textHex,
        float height, bool solid, bool ghost = false)
    {
        GameObject go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, height);
        Image img = go.AddComponent<Image>();
        if (solid) img.color = HexToColor(bgHex);
        else
        {
            img.color = Color.clear;
            Outline o = go.AddComponent<Outline>();
            o.effectColor    = ghost ? HexToColor("2A2A2A") : HexToColor("3A3A3A");
            o.effectDistance = new Vector2(1f, -1f);
        }
        Button btn = go.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
        cb.pressedColor     = new Color(1f, 1f, 1f, 0.2f);
        btn.colors = cb;

        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        CaiDatStretchFull(txtGO.AddComponent<RectTransform>());
        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = ghost ? 13f : 15f;
        tmp.color     = HexToColor(textHex);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = solid ? FontStyles.Bold : FontStyles.Normal;

        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.preferredWidth  = 200f;
        le.flexibleWidth   = 0f;
        return go;
    }

    static void TaoSpacer(Transform parent, float height)
    {
        GameObject go = new GameObject("Spacer");
        go.transform.SetParent(parent, false);
        LayoutElement le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height; le.minHeight = height;
    }

    static void CaiDatStretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static Color HexToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color c)) return c;
        return Color.white;
    }
}
#endif