using UnityEngine;

public class GameUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        // Initial UI state setup will be implemented in a later step.
    }

    public void SetScore(int score)
    {
        // Score text update will be implemented later.
    }

    public void ShowGameOver(bool show)
    {
        // Game over panel visibility will be implemented later.
    }
}
