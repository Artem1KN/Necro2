using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// Builds a full-screen Game Over overlay programmatically. Subscribes to
/// PlayerHealth.onDeath and shows the screen on death. After
/// autoReloadDelaySeconds the current scene is reloaded (countdown shown on
/// the screen). Manual buttons: Restart and Main Menu.
///
/// Attach to any GameObject in the gameplay scene and assign PlayerHealth.
/// If PlayerHealth is left empty the screen searches for one on Start.
public class GameOverScreen : MonoBehaviour
{
    [Header("Refs")]
    public PlayerHealth playerHealth;

    [Header("Reload")]
    [Tooltip("Seconds before the current scene is automatically reloaded after the player dies.")]
    public float autoReloadDelaySeconds = 5f;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Colors")]
    public Color titleColor = new(1f, 0.2f, 0.2f);
    public Color countdownColor = new(0.95f, 0.95f, 0.95f);

    private Canvas canvas;
    private GameObject root;
    private TMP_Text countdownText;
    private bool shown;

    private void Start()
    {
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null) playerHealth.onDeath += HandleDeath;

        BuildUI();
        root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (playerHealth != null) playerHealth.onDeath -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (shown) return;
        shown = true;
        root.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        StartCoroutine(CountdownThenReload());
    }

    private IEnumerator CountdownThenReload()
    {
        float remaining = autoReloadDelaySeconds;
        while (remaining > 0f)
        {
            if (countdownText != null)
                countdownText.text = $"Перезапуск через {Mathf.CeilToInt(remaining)}…";
            yield return null;
            remaining -= Time.unscaledDeltaTime;
        }
        Restart();
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void BuildUI()
    {
        var go = new GameObject("GameOver_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        root = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        var panelRt = root.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.85f);

        CreateText("Title", root.transform, new Vector2(0, 0.55f), new Vector2(1, 0.75f),
            "GAME OVER", 96, FontStyles.Bold, titleColor);

        countdownText = CreateText("Countdown", root.transform, new Vector2(0, 0.42f), new Vector2(1, 0.5f),
            "Перезапуск через 5…", 32, FontStyles.Normal, countdownColor);

        var restart = CreateButton("Restart", root.transform, new Vector2(0.5f, 0.32f), "RESTART",
            new Color(0.2f, 0.6f, 0.9f));
        restart.onClick.AddListener(Restart);

        var menu = CreateButton("MainMenu", root.transform, new Vector2(0.5f, 0.22f), "MAIN MENU",
            new Color(0.35f, 0.35f, 0.35f));
        menu.onClick.AddListener(LoadMainMenu);
    }

    private static TMP_Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        string text, float fontSize, FontStyles style, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = Vector2.zero;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Button CreateButton(string name, Transform parent, Vector2 anchorCenter, string label, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorCenter;
        rt.anchorMax = anchorCenter;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(360, 70);
        var img = go.GetComponent<Image>();
        img.color = color;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        var lRt = labelGo.GetComponent<RectTransform>();
        lRt.anchorMin = Vector2.zero;
        lRt.anchorMax = Vector2.one;
        lRt.sizeDelta = Vector2.zero;
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return go.GetComponent<Button>();
    }
}
