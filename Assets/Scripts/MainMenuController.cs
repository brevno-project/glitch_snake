using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Game";

    // Will load the Game scene in a later step.
    public void PlayGame()
    {
    }

    // Will quit the application in a later step.
    public void QuitGame()
    {
    }
}
