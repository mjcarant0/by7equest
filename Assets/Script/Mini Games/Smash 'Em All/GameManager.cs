using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int sliceGoal = 10;
    private int currentSlices;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void FruitSliced()
    {
        currentSlices++;

        if (currentSlices >= sliceGoal)
            WinGame();
    }

    void WinGame()
    {
        Time.timeScale = 0f;
        Debug.Log("YOU WIN");
    }
}
