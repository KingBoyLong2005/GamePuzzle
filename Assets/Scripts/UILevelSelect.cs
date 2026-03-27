using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ────────────────────────────────────────────────────────────
//  UILevelSelect  —  Hiển thị grid các level card
//  Mỗi card cho biết: số level, tên, đã hoàn thành chưa (dấu sao)
// ────────────────────────────────────────────────────────────

public class UILevelSelect : MonoBehaviour
{
    Canvas    canvas;
    GameObject root;
    GameObject gridContainer;
    List<GameObject> cards = new();

    void Awake() => BuildShell();

    // Tạo khung cố định (canvas + header + scroll), chưa có cards
    void BuildShell()
    {
        var canvasGO = new GameObject("LevelSelectCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(540, 960);
        canvasGO.AddComponent<GraphicRaycaster>();
        root = canvasGO;

        // Full-screen BG
        var bg = MakeImage(canvasGO, "BG", Vector2.zero, Vector2.zero,
            new Color(0.10f, 0.14f, 0.22f, 1f));
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        // Header
        var header = MakeImage(canvasGO, "Header", new Vector2(0, 100), new Vector2(540, 100),
            new Color(0.08f, 0.12f, 0.20f, 1f));

        var title = MakeTMP(header, "Title", new Vector2(0, 0), new Vector2(400, 60));
        title.text      = "Select Level";
        title.fontSize  = 30;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color     = Color.white;

        // Back button (top-left)
        MakeButton(canvasGO, "<", new Vector2(-220, 105), new Vector2(60, 60),
            new Color(0.20f, 0.25f, 0.38f), () => GameManager.Instance.ShowMainMenu());

        // Scroll view for level cards
        BuildScrollView(canvasGO);

        root.SetActive(false);
    }

    void BuildScrollView(GameObject parent)
    {
        // Scroll viewport
        var scrollGO = new GameObject("Scroll");
        scrollGO.transform.SetParent(parent.transform, false);
        var scrollRt = scrollGO.AddComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 1);
        scrollRt.offsetMin = new Vector2(0, 0);
        scrollRt.offsetMax = new Vector2(0, -110);   // leave room for header

        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        // Viewport mask
        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollGO.transform, false);
        var vpRt = viewport.AddComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>().color = new Color(0,0,0,0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content container (grows with cards)
        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0,1);
        contentRt.anchorMax = new Vector2(1,1);
        contentRt.pivot     = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0, 0);

        var glg = content.AddComponent<GridLayoutGroup>();
        glg.cellSize        = new Vector2(130, 130);
        glg.spacing         = new Vector2(20, 20);
        glg.padding         = new RectOffset(30, 30, 30, 30);
        glg.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 2;
        glg.startCorner     = GridLayoutGroup.Corner.UpperLeft;
        glg.startAxis       = GridLayoutGroup.Axis.Horizontal;
        glg.childAlignment  = TextAnchor.UpperCenter;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = vpRt;
        scrollRect.content  = contentRt;
        scrollRect.scrollSensitivity = 30f;

        gridContainer = content;
    }

    // ── Public API ────────────────────────────────────────────

    public void Show(LevelDataSO[] levels)
    {
        root.SetActive(true);
        RefreshCards(levels);
    }

    public void Hide() => root?.SetActive(false);

    void RefreshCards(LevelDataSO[] levels)
    {
        // Xoá cards cũ
        foreach (var c in cards) if (c) Destroy(c);
        cards.Clear();

        for (int i = 0; i < levels.Length; i++)
        {
            var level   = levels[i];
            int idx     = i;
            bool done   = SaveManager.Instance.IsCompleted(level);
            var card    = BuildCard(gridContainer, level, idx + 1, done);
            cards.Add(card);
        }
    }

    // ── Card ─────────────────────────────────────────────────

    GameObject BuildCard(GameObject parent, LevelDataSO level, int number, bool completed)
    {
        var card = new GameObject($"Card_{number}");
        card.transform.SetParent(parent.transform, false);
        card.AddComponent<RectTransform>();

        // Card background
        var bg = card.AddComponent<Image>();
        bg.color  = completed
            ? new Color(0.12f, 0.28f, 0.18f, 1f)   // đã hoàn thành → xanh lá đậm
            : new Color(0.16f, 0.20f, 0.32f, 1f);   // chưa → xanh tím
        bg.sprite = MakeRoundedSprite();
        bg.type   = Image.Type.Sliced;

        // Border highlight khi completed
        if (completed)
        {
            var border = new GameObject("Border");
            border.transform.SetParent(card.transform, false);
            var bRt = border.AddComponent<RectTransform>();
            bRt.anchorMin = Vector2.zero; bRt.anchorMax = Vector2.one;
            bRt.sizeDelta = new Vector2(4, 4);
            var bImg = border.AddComponent<Image>();
            bImg.color  = new Color(0.25f, 0.85f, 0.40f, 0.7f);
            bImg.sprite = MakeRoundedSprite();
            bImg.type   = Image.Type.Sliced;
        }

        // Level number
        var numLabel = MakeTMP(card, "Num", new Vector2(0, 50), new Vector2(200, 60));
        numLabel.text      = $"{number}";
        numLabel.fontSize  = 42;
        numLabel.fontStyle = FontStyles.Bold;
        numLabel.alignment = TextAlignmentOptions.Center;
        numLabel.color     = completed ? new Color(0.3f,0.9f,0.45f) : new Color(0.7f,0.85f,1f);

        // Level display name
        var nameLabel = MakeTMP(card, "Name", new Vector2(0, 0), new Vector2(210, 40));
        nameLabel.text      = string.IsNullOrEmpty(level.displayName) ? $"Level {number}" : level.displayName;
        nameLabel.fontSize  = 15;
        nameLabel.alignment = TextAlignmentOptions.Center;
        nameLabel.color     = new Color(1f, 1f, 1f, 0.65f);

        // Grid size label
        var sizeLabel = MakeTMP(card, "Size", new Vector2(0, -28), new Vector2(200, 32));
        sizeLabel.text      = $"{level.rows} × {level.cols}";
        sizeLabel.fontSize  = 13;
        sizeLabel.alignment = TextAlignmentOptions.Center;
        sizeLabel.color     = new Color(1f, 1f, 1f, 0.40f);

        // Color mode badge
        if (level.colorMode)
        {
            var badge = new GameObject("Badge");
            badge.transform.SetParent(card.transform, false);
            var bRt = badge.AddComponent<RectTransform>();
            bRt.anchorMin = bRt.anchorMax = new Vector2(0.5f, 0.5f);
            bRt.anchoredPosition = new Vector2(0, -58);
            bRt.sizeDelta = new Vector2(90, 24);
            var bImg = badge.AddComponent<Image>();
            bImg.color  = new Color(0.70f, 0.25f, 0.95f, 0.85f);
            bImg.sprite = MakeRoundedSprite();
            bImg.type   = Image.Type.Sliced;

            var bTxt = MakeTMP(badge, "Txt", Vector2.zero, new Vector2(90,24));
            bTxt.text      = "COLOR";
            bTxt.fontSize  = 11;
            bTxt.fontStyle = FontStyles.Bold;
            bTxt.alignment = TextAlignmentOptions.Center;
            bTxt.color     = Color.white;
        }

        // Star (completed indicator)
        var star = MakeTMP(card, "Star", new Vector2(72, 82), new Vector2(40, 40));
        star.text      = completed ? "★" : "☆";
        star.fontSize  = 24;
        star.alignment = TextAlignmentOptions.Center;
        star.color     = completed ? new Color(1f, 0.85f, 0.1f) : new Color(1f,1f,1f,0.18f);

        // Click handler
        var btn = card.AddComponent<Button>();
        btn.targetGraphic = bg;
        var nav = Navigation.defaultNavigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        btn.onClick.AddListener(() => GameManager.Instance.StartLevel(level));

        return card;
    }

    // ── UI Helpers ────────────────────────────────────────────

    TextMeshProUGUI MakeTMP(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go.AddComponent<TextMeshProUGUI>();
    }

    void MakeButton(GameObject parent, string label, Vector2 pos, Vector2 size,
                    Color bg, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject($"Btn_{label}");
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = bg;
        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        var nav = Navigation.defaultNavigation; nav.mode = Navigation.Mode.None;
        btn.navigation = nav;
        btn.onClick.AddListener(onClick);
        var txt = MakeTMP(go, "Txt", Vector2.zero, size);
        txt.text = label; txt.fontSize = 22; txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center; txt.color = Color.white;
    }

    GameObject MakeImage(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        go.AddComponent<Image>().color = color;
        return go;
    }

    static Sprite _sprite;
    Sprite MakeRoundedSprite()
    {
        if (_sprite != null) return _sprite;
        int s = 64; float r = 12f;
        var tex = new Texture2D(s, s);
        var pixels = new Color[s * s];
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            float cx = Mathf.Min(x, s-1-x), cy = Mathf.Min(y, s-1-y);
            bool inside = cx >= r || cy >= r || (cx-r)*(cx-r)+(cy-r)*(cy-r) <= r*r;
            pixels[y*s+x] = inside ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels); tex.Apply();
        _sprite = Sprite.Create(tex, new Rect(0,0,s,s), new Vector2(0.5f,0.5f), s,
            0, SpriteMeshType.FullRect, new Vector4(r,r,r,r));
        return _sprite;
    }
}
