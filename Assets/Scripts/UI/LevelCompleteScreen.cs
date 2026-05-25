using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// Full-screen panel shown when the player reaches a LevelEndZone.
/// Provides a "Next Level" button that loads the configured next scene via
/// LoadingScreenController (if assigned) or directly via SceneManager.
public class LevelCompleteScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Next Scene")]
    [Tooltip("Scene name loaded when Next Level is pressed. Must be in Build Settings.")]
    public string nextSceneName;

    [Tooltip("Scene name loaded when Main Menu is pressed.")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Optional")]
    [SerializeField] private LoadingScreenController loadingScreen;

    private void Awake()
    {
        if (root == null) root = gameObject;
        root.SetActive(false);

        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(LoadNext);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(LoadMainMenu);
    }

    public void Show()
    {
        if (root != null) root.SetActive(true);
        if (titleText != null) titleText.text = "LEVEL COMPLETE";
        if (nextLevelButton != null) nextLevelButton.interactable = !string.IsNullOrEmpty(nextSceneName);
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    private void LoadNext()
    {
        if (string.IsNullOrEmpty(nextSceneName)) return;
        Time.timeScale = 1f;
        if (loadingScreen != null) loadingScreen.LoadScene(nextSceneName);
        else SceneManager.LoadScene(nextSceneName);
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        if (loadingScreen != null) loadingScreen.LoadScene(mainMenuSceneName);
        else SceneManager.LoadScene(mainMenuSceneName);
    }
}
