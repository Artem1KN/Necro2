using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Scenes")]
    [Tooltip("Scene name to load via LoadingScreenController. Must be in Build Settings.")]
    [SerializeField] private string firstLevelScene = "Level_01";

    [SerializeField] private LoadingScreenController loadingScreen;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsPressed);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitPressed);

        ShowMain();
    }

    private void OnPlayPressed()
    {
        if (loadingScreen != null)
            loadingScreen.LoadScene(firstLevelScene);
        else
            SceneManager.LoadScene(firstLevelScene);
    }

    private void OnSettingsPressed()
    {
        ShowSettings();
    }

    private void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
}
