using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food")]
    [SerializeField] private GameObject foodPrefab;

    private GameObject currentFoodInstance;

    public void SpawnFood()
    {
        // Random free-cell spawning will be implemented in a later step.
    }

    public void ClearFood()
    {
        // Cleanup behavior will be implemented later.
    }
}
