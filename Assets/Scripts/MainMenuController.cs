using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public enum GameMode
{
    Classic,
    Glitch
}

public static class GameModeSelection
{
    private static GameMode selectedMode = GameMode.Classic;

    public static bool HasSelectedMode { get; private set; }

    public static GameMode SelectedMode
    {
        get
        {
            return selectedMode;
        }
        set
        {
            selectedMode = value;
            HasSelectedMode = true;
        }
    }
}

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Mode Toggle")]
    [SerializeField] private TMP_Text modeToggleLabel;
    [SerializeField] private string classicModeLabel = "MODE: CLASSIC";
    [SerializeField] private string glitchModeLabel = "MODE: GLITCH";

    private GameMode selectedMode = GameMode.Classic;

    private void Start()
    {
        SelectMode(GameMode.Classic);
    }

    // Called by the Play button.
    public void PlayGame()
    {
        GameModeSelection.SelectedMode = selectedMode;
        LoadGameScene();
    }

    // Called by the mode toggle button.
    public void ToggleGameMode()
    {
        GameMode nextMode = selectedMode == GameMode.Classic ? GameMode.Glitch : GameMode.Classic;
        SelectMode(nextMode);
    }

    private void SelectMode(GameMode mode)
    {
        selectedMode = mode;
        GameModeSelection.SelectedMode = selectedMode;
        UpdateModeToggleLabel();
    }

    private void UpdateModeToggleLabel()
    {
        if (modeToggleLabel == null)
        {
            return;
        }

        modeToggleLabel.text = selectedMode == GameMode.Glitch ? glitchModeLabel : classicModeLabel;
    }

    private void LoadGameScene()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("MainMenuController: gameSceneName is empty.");
            return;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // Called by the Quit button.
    public void QuitGame()
    {
        Application.Quit();
    }
}
