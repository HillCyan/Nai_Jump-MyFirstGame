using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// 负责开始界面、死亡结算界面以及上面的按钮
// 不需要手动挂到场景里：进入游戏时会自动创建（RuntimeInitializeOnLoadMethod）
public class UIManager : MonoBehaviour
{
    // 下次加载场景时先显示哪个界面
    enum NextScreen { Menu, Game }
    static NextScreen nextScreen = NextScreen.Menu;

    Sprite buttonSprite;    // 按钮背景：others/naiYan
    Sprite menuArtSprite;   // 开始界面立绘：Player_Idle/NaiWa
    Sprite deathBgSprite;   // 死亡界面背景：others/naiDan

    GameObject menuPanel;
    GameObject gameOverPanel;
    Text scoreText;
    Player player;
    Font uiFont;
    bool gameOverShown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        // RuntimeInitializeOnLoadMethod 只在游戏启动时执行一次，
        // 重新开始 / 返回主界面 会重载场景，所以必须监听 sceneLoaded，
        // 否则重载后没有 UIManager，就直接进游戏了。
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (FindObjectOfType<UIManager>() != null) return;
        new GameObject("UIManager").AddComponent<UIManager>();
    }

    void Awake()
    {
        uiFont = FindHudFont();
        buttonSprite = LoadSprite("naiYan");
        menuArtSprite = LoadSprite("NaiWa");
        deathBgSprite = LoadSprite("naiDan");

        BuildUI();
    }

    void Start()
    {
        player = FindObjectOfType<Player>();

        if (nextScreen == NextScreen.Menu)
            EnterMenu();
        else
            EnterGame();
    }

    void EnterMenu()
    {
        menuPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        Time.timeScale = 0f;
        if (player != null) player.enabled = false;
    }

    void EnterGame()
    {
        menuPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
        if (player != null) player.enabled = true;
    }

    void Update()
    {
        if (nextScreen == NextScreen.Menu || gameOverShown) return;

        if (player == null) player = FindObjectOfType<Player>();
        if (player != null && player.IsDead)
        {
            gameOverShown = true;
            StartCoroutine(ShowGameOverAfterDelay());
        }
    }

    // 死亡后稍微等一下，让下坠/死亡表现播一下，再弹结算
    IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(0.6f);

        if (player != null)
            scoreText.text = "分数：" + player.Score;

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    // ---------------- 按钮 ----------------

    // 开始：进入游戏
    void OnStartClicked()
    {
        nextScreen = NextScreen.Game;
        EnterGame();
    }

    // 重新开始：直接开一局新的（不经过开始界面）
    void OnRestartClicked()
    {
        nextScreen = NextScreen.Game;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 返回主界面：回到开始界面（不是开始游戏）
    void OnBackToMenuClicked()
    {
        nextScreen = NextScreen.Menu;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 退出：关闭游戏
    void OnQuitClicked()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------------- 构建界面 ----------------

    void BuildUI()
    {
        GameObject canvasGo = new GameObject("UIRoot", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        BuildMenu(canvasGo.transform);
        BuildGameOver(canvasGo.transform);
    }

    // 开始界面：深色底 + NaiWa 立绘 + 标题「奶jump」+ 开始/退出
    void BuildMenu(Transform parent)
    {
        menuPanel = CreateFullScreen("MenuPanel", parent);
        menuPanel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 1f);

        // 立绘：铺满整个开始界面（按比例放大到盖住整屏，不会变形）
        Sprite art = menuArtSprite != null ? menuArtSprite : FindBackgroundSprite();
        if (art != null)
        {
            GameObject artGo = new GameObject("NaiWa", typeof(Image));
            artGo.transform.SetParent(menuPanel.transform, false);

            RectTransform artRt = artGo.GetComponent<RectTransform>();
            artRt.anchorMin = new Vector2(0.5f, 0.5f);
            artRt.anchorMax = new Vector2(0.5f, 0.5f);
            artRt.anchoredPosition = Vector2.zero;

            Image artImg = artGo.GetComponent<Image>();
            artImg.sprite = art;
            artImg.preserveAspect = true;

            // EnvelopeParent：按原比例放大，直到把整屏盖满
            AspectRatioFitter fitter = artGo.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = art.rect.width / art.rect.height;
        }

        // 标题：正中间，深黄色
        Text title = CreateLabel(menuPanel.transform, "奶jump", new Vector2(1200f, 150f), Vector2.zero, 130, new Color(0.8f, 0.6f, 0.05f, 1f));
        title.fontStyle = FontStyle.Bold;

        Shadow shadow = title.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(4f, -4f);

        CreateButton(menuPanel.transform, "开始", new Vector2(-200f, -440f), OnStartClicked);
        CreateButton(menuPanel.transform, "退出", new Vector2(200f, -440f), OnQuitClicked);
    }

    // 死亡界面：半透明遮罩 + naiDan 背景 + 黑字分数 + 三个按钮
    void BuildGameOver(Transform parent)
    {
        gameOverPanel = CreateFullScreen("GameOverPanel", parent);
        gameOverPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        GameObject box = new GameObject("Box", typeof(Image));
        box.transform.SetParent(gameOverPanel.transform, false);

        RectTransform boxRt = box.GetComponent<RectTransform>();
        boxRt.sizeDelta = new Vector2(720f, 800f);   // 拉高，给下方的文字和按钮留位置
        boxRt.anchoredPosition = Vector2.zero;

        // 原来白色的矩形换成 naiDan
        Image boxImg = box.GetComponent<Image>();
        if (deathBgSprite != null)
        {
            boxImg.sprite = deathBgSprite;
            boxImg.color = Color.white;
        }
        else
        {
            boxImg.color = new Color(1f, 1f, 1f, 0.94f);
        }

        GameObject textGo = new GameObject("ScoreText", typeof(Text));
        textGo.transform.SetParent(box.transform, false);

        RectTransform textRt = textGo.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(660f, 120f);
        textRt.anchoredPosition = new Vector2(0f, 60f);   // 移到 naiDan 下半部分

        scoreText = textGo.GetComponent<Text>();
        scoreText.text = "分数：0";
        scoreText.font = uiFont;
        scoreText.fontSize = 60;
        scoreText.fontStyle = FontStyle.Bold;      // 加粗
        scoreText.alignment = TextAnchor.MiddleCenter;
        scoreText.color = Color.black;             // 加深

        // 再描一圈黑边，让字更重
        Outline outline = textGo.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateButton(box.transform, "重新开始", new Vector2(0f, -80f), OnRestartClicked, false);
        CreateButton(box.transform, "返回主界面", new Vector2(0f, -205f), OnBackToMenuClicked, false);
        CreateButton(box.transform, "退出", new Vector2(0f, -330f), OnQuitClicked, false);

        gameOverPanel.SetActive(false);
    }

    GameObject CreateFullScreen(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(Image));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return go;
    }

    Button CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick, bool useSprite = true)
    {
        GameObject go = new GameObject("Btn_" + label, typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(340f, 118f);
        rt.anchoredPosition = pos;

        // 开始界面用 naiYan，死亡界面用纯色
        Image img = go.GetComponent<Image>();
        if (useSprite && buttonSprite != null)
        {
            img.sprite = buttonSprite;
            img.color = Color.white;
        }
        else
        {
            img.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        }

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        Text t = CreateLabel(go.transform, label, Vector2.zero, Vector2.zero, 40, new Color(0.1f, 0.1f, 0.1f, 1f));

        RectTransform trt = t.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        return btn;
    }

    Text CreateLabel(Transform parent, string content, Vector2 size, Vector2 pos, int fontSize, Color color)
    {
        GameObject go = new GameObject("Label_" + content, typeof(Text));
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        Text t = go.GetComponent<Text>();
        t.text = content;
        t.font = uiFont;
        t.fontSize = fontSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;

        return t;
    }

    // 运行时加载玩家自己放进 Resources 的贴图
    Sprite LoadSprite(string name)
    {
        Sprite s = Resources.Load<Sprite>(name);
        if (s != null) return s;

        Texture2D tex = Resources.Load<Texture2D>(name);
        if (tex != null)
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        return null;
    }

    // 兜底：拿场景里 background 的贴图
    Sprite FindBackgroundSprite()
    {
        SpriteRenderer[] all = FindObjectsOfType<SpriteRenderer>();

        foreach (SpriteRenderer sr in all)
            if (sr.gameObject.name.ToLower() == "background")
                return sr.sprite;

        foreach (SpriteRenderer sr in all)
            if (sr.gameObject.name.ToLower() == "background2")
                return sr.sprite;

        return null;
    }

    // 复用场景里 HUD 的字体；找不到就用内置字体
    Font FindHudFont()
    {
        foreach (Text t in FindObjectsOfType<Text>(true))
            if (t.font != null)
                return t.font;

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }
}
