using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Game";

    // Called by the Play button.
    public void PlayGame()
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
