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
    [SerializeField] private string firstLevelScene = "sc temp";

    [SerializeField] private LoadingScreenController loadingScreen;

    [Header("Audio")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip buttonClickSfx;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        if (playButton != null) playButton.onClick.AddListener(OnPlayPressed);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsPressed);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitPressed);

        if (menuMusic != null)
            AudioManager.EnsureExists().PlayMusic(menuMusic);

        ShowMain();
    }

    private void PlayClick()
    {
        if (buttonClickSfx != null)
            AudioManager.EnsureExists().PlaySfx2D(buttonClickSfx);
    }

    private void OnPlayPressed()
    {
        PlayClick();
        if (loadingScreen != null)
            loadingScreen.LoadScene(firstLevelScene);
        else
            SceneManager.LoadScene(firstLevelScene);
    }

    private void OnSettingsPressed()
    {
        PlayClick();
        ShowSettings();
    }

    private void OnQuitPressed()
    {
        PlayClick();
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
