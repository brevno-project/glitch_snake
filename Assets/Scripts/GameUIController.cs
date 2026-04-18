using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text scoreTextTMP;
    [SerializeField] private Text scoreText;
    [SerializeField] private TMP_Text glitchAnnouncementTextTMP;
    [SerializeField] private Text glitchAnnouncementText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string scorePrefix = "Score: ";
    [SerializeField] private string bestScorePrefix = "Best: ";
    [SerializeField] private string scoreSeparator = "   ";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float glitchAnnouncementDefaultDuration = 0.9f;

    private GameManager gameManager;
    private int currentScore;
    private int bestScore;
    private string statusMessage;
    private float glitchAnnouncementHideAtTime;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
        currentScore = 0;
        bestScore = 0;
        statusMessage = string.Empty;
        glitchAnnouncementHideAtTime = 0f;

        ConfigureScoreLabelsForStatus();
        EnsureGlitchAnnouncementText();
        HideGlitchAnnouncementImmediate();
        UpdateScoreLabel();
        ShowGameOver(false);
    }

    public void SetScore(int score)
    {
        currentScore = score;
        UpdateScoreLabel();
    }

    public void SetBestScore(int score)
    {
        bestScore = Mathf.Max(0, score);
        UpdateScoreLabel();
    }

    public void SetStatusMessage(string message)
    {
        statusMessage = message ?? string.Empty;
        UpdateScoreLabel();
    }

    public void ClearStatusMessage()
    {
        statusMessage = string.Empty;
        UpdateScoreLabel();
    }

    public void ShowGlitchAnnouncement(string message, float duration = -1f)
    {
        string safeMessage = string.IsNullOrWhiteSpace(message) ? string.Empty : message.Trim();
        if (string.IsNullOrEmpty(safeMessage))
        {
            HideGlitchAnnouncementImmediate();
            return;
        }

        float targetDuration = duration > 0f ? duration : glitchAnnouncementDefaultDuration;
        glitchAnnouncementHideAtTime = Time.time + Mathf.Max(0.1f, targetDuration);

        if (glitchAnnouncementTextTMP != null)
        {
            glitchAnnouncementTextTMP.text = safeMessage;
            glitchAnnouncementTextTMP.gameObject.SetActive(true);
        }

        if (glitchAnnouncementText != null)
        {
            glitchAnnouncementText.text = safeMessage;
            glitchAnnouncementText.gameObject.SetActive(true);
        }
    }

    private void UpdateScoreLabel()
    {
        string scoreLabel = scorePrefix + currentScore + scoreSeparator + bestScorePrefix + bestScore;
        if (!string.IsNullOrWhiteSpace(statusMessage))
        {
            scoreLabel += "\n" + statusMessage;
        }

        if (scoreTextTMP != null)
        {
            scoreTextTMP.text = scoreLabel;
        }

        if (scoreText != null)
        {
            scoreText.text = scoreLabel;
        }
    }

    private void ConfigureScoreLabelsForStatus()
    {
        if (scoreTextTMP != null)
        {
            scoreTextTMP.enableWordWrapping = false;
            scoreTextTMP.overflowMode = TextOverflowModes.Overflow;
        }

        if (scoreText != null)
        {
            scoreText.horizontalOverflow = HorizontalWrapMode.Overflow;
            scoreText.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }

    private void Update()
    {
        if (glitchAnnouncementHideAtTime <= 0f || Time.time < glitchAnnouncementHideAtTime)
        {
            return;
        }

        HideGlitchAnnouncementImmediate();
    }

    private void HideGlitchAnnouncementImmediate()
    {
        glitchAnnouncementHideAtTime = 0f;

        if (glitchAnnouncementTextTMP != null)
        {
            glitchAnnouncementTextTMP.gameObject.SetActive(false);
        }

        if (glitchAnnouncementText != null)
        {
            glitchAnnouncementText.gameObject.SetActive(false);
        }
    }

    private void EnsureGlitchAnnouncementText()
    {
        if (glitchAnnouncementTextTMP != null || glitchAnnouncementText != null)
        {
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            return;
        }

        GameObject textObject = new GameObject("GlitchAnnouncementText");
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(1400f, 220f);
        rectTransform.anchoredPosition = Vector2.zero;

        TextMeshProUGUI runtimeText = textObject.AddComponent<TextMeshProUGUI>();
        runtimeText.text = string.Empty;
        runtimeText.alignment = TextAlignmentOptions.Center;
        runtimeText.fontSize = 84f;
        runtimeText.enableWordWrapping = false;
        runtimeText.color = new Color(0.96f, 0.42f, 0.96f, 1f);
        runtimeText.outlineColor = new Color(0.02f, 0.05f, 0.12f, 1f);
        runtimeText.outlineWidth = 0.28f;

        if (scoreTextTMP != null && scoreTextTMP.font != null)
        {
            runtimeText.font = scoreTextTMP.font;
        }

        glitchAnnouncementTextTMP = runtimeText;
    }

    public void ShowGameOver(bool show)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(show);
        }
    }

    public void RestartGame()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    public void BackToMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("GameUIController: mainMenuSceneName is empty.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public GameManager GetGameManager()
    {
        return gameManager;
    }
}
