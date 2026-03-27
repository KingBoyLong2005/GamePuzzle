using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ────────────────────────────────────────────────────────────
//  UIMainMenu  —  Tự build toàn bộ UI Main Menu bằng code
//  Gán script này lên một Empty GO tên "UIMainMenu"
// ────────────────────────────────────────────────────────────

public class UIMainMenu : MonoBehaviour
{
    Canvas    canvas;
    GameObject root;

    void Awake() => BuildUI();

    void BuildUI()
    {
        // Canvas
        var canvasGO = new GameObject("MainMenuCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode       = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(540, 960);
        canvasGO.AddComponent<GraphicRaycaster>();

        root = canvasGO;

        // Background panel
        var bg = MakePanel(canvasGO, "BG", Vector2.zero, new Vector2(540, 960),
            new Color(0.10f, 0.14f, 0.22f, 1f));
        bg.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        bg.GetComponent<RectTransform>().anchorMax = Vector2.one;
        bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        // Title
        var title = MakeTMP(canvasGO, "Title", new Vector2(0, 180), new Vector2(480, 100));
        title.text      = "BLOCK PUZZLE";
        title.fontSize  = 48;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color     = new Color(1f, 0.85f, 0.2f);

        var sub = MakeTMP(canvasGO, "Sub", new Vector2(0, 110), new Vector2(400, 60));
        sub.text      = "Fill the grid. Beat the puzzle.";
        sub.fontSize  = 20;
        sub.alignment = TextAlignmentOptions.Center;
        sub.color     = new Color(0.7f, 0.8f, 1f, 0.85f);

        // Decorative grid icon (3×3 squares)
        BuildDecorIcon(canvasGO);

        // Play button
        MakeButton(canvasGO, "PLAY", new Vector2(0, 40), new Vector2(260, 72),
            new Color(0.15f, 0.60f, 0.95f), () => GameManager.Instance.ShowLevelSelect());

        // Version label
        var ver = MakeTMP(canvasGO, "Ver", new Vector2(0, -430), new Vector2(300, 36));
        ver.text      = "v1.0";
        ver.fontSize  = 14;
        ver.alignment = TextAlignmentOptions.Center;
        ver.color     = new Color(1f, 1f, 1f, 0.25f);
    }

    void BuildDecorIcon(GameObject parent)
    {
        // 3×3 mini grid as decoration
        float sz = 28f, gap = 6f;
        float total = 3 * sz + 2 * gap;
        float startX = -total / 2f + sz / 2f;
        float startY = -30f;

        Color[] colors = {
            new Color(0.20f,0.55f,1.00f), new Color(0.95f,0.25f,0.25f), new Color(0.20f,0.80f,0.35f),
            new Color(1.00f,0.82f,0.10f), new Color(0.70f,0.25f,0.95f), new Color(1.00f,0.55f,0.10f),
            new Color(0.20f,0.55f,1.00f), new Color(0.20f,0.80f,0.35f), new Color(0.95f,0.25f,0.25f),
        };

        int idx = 0;
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            float x = startX + c * (sz + gap);
            float y = startY - r * (sz + gap);
            var cell = MakePanel(parent, $"deco_{r}_{c}",
                new Vector2(x, y), new Vector2(sz, sz), colors[idx++]);
            cell.GetComponent<Image>().sprite = MakeRoundedSprite(8);
        }
    }

    // ── Show / Hide ───────────────────────────────────────────

    public void Show() => root?.SetActive(true);
    public void Hide() => root?.SetActive(false);

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

        var img = go.AddComponent<Image>();
        img.color  = bg;
        img.sprite = MakeRoundedSprite(16);
        img.type   = Image.Type.Sliced;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var nav = Navigation.defaultNavigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        btn.onClick.AddListener(onClick);

        // Label
        var txt = MakeTMP(go, "Txt", Vector2.zero, size);
        txt.text      = label;
        txt.fontSize  = 24;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color     = Color.white;
    }

    GameObject MakePanel(GameObject parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>(); img.color = color;
        return go;
    }

    static Sprite _r8, _r16;
    Sprite MakeRoundedSprite(int radius)
    {
        ref Sprite cache = ref (radius <= 8 ? ref _r8 : ref _r16);
        if (cache != null) return cache;
        int s = 64;
        var tex = new Texture2D(s, s);
        var pixels = new Color[s * s];
        float r = radius;
        for (int y = 0; y < s; y++)
        for (int x = 0; x < s; x++)
        {
            float cx = Mathf.Min(x, s-1-x), cy = Mathf.Min(y, s-1-y);
            bool inside = cx >= r || cy >= r || (cx-r)*(cx-r)+(cy-r)*(cy-r) <= r*r;
            pixels[y*s+x] = inside ? Color.white : Color.clear;
        }
        tex.SetPixels(pixels); tex.Apply();
        cache = Sprite.Create(tex, new Rect(0,0,s,s), new Vector2(0.5f,0.5f), s,
            0, SpriteMeshType.FullRect, new Vector4(r,r,r,r));
        return cache;
    }
}
