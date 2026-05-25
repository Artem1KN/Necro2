using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject loadingPanel;

    [Header("Progress")]
    [SerializeField] private Image progressFill;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text tipText;

    [Header("Tips")]
    [SerializeField] private string[] tips;

    [Header("Pacing")]
    [Tooltip("Minimum time the loading screen is shown (seconds), even for tiny scenes.")]
    [SerializeField] private float minDisplaySeconds = 0.6f;

    [Tooltip("Lerp speed for the progress bar.")]
    [SerializeField] private float fillLerpSpeed = 4f;

    private void Awake()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[LoadingScreen] Scene name is empty.", this);
            return;
        }

        StartCoroutine(LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (tipText != null && tips != null && tips.Length > 0)
            tipText.text = tips[Random.Range(0, tips.Length)];

        float displayed = 0f;
        float startedAt = Time.unscaledTime;

        var op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null) yield break;
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float target = Mathf.Clamp01(op.progress / 0.9f);
            displayed = Mathf.MoveTowards(displayed, target, fillLerpSpeed * Time.unscaledDeltaTime);

            if (progressFill != null) progressFill.fillAmount = displayed;
            if (progressText != null) progressText.text = $"{Mathf.RoundToInt(displayed * 100f)}%";

            if (op.progress >= 0.9f
                && displayed >= 0.999f
                && Time.unscaledTime - startedAt >= minDisplaySeconds)
            {
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
