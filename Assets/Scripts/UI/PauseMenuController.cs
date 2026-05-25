using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    public bool IsPaused { get; private set; }

    private InputAction pauseAction;

    private void OnEnable()
    {
        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Keyboard>/p");
        pauseAction.performed += OnPausePressed;
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.performed -= OnPausePressed;
            pauseAction.Disable();
            pauseAction.Dispose();
            pauseAction = null;
        }
    }

    private void Start()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        SetPaused(false);
    }

    private void OnPausePressed(InputAction.CallbackContext _)
    {
        SetPaused(!IsPaused);
    }

    public void Resume() => SetPaused(false);

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        if (pausePanel != null) pausePanel.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }
}
