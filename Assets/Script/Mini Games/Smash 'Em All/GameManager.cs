using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int sliceGoal = 10;
    private int currentSlices;

    public float timeLimit = 15f;
    private float timer;

    private bool gameEnded = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentSlices = 0;
        gameEnded = false;

        if (GameModeManager.Instance != null)
            timer = timeLimit;
    }

    void Update()
    {
        if (gameEnded || GameModeManager.Instance == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
            LoseGame();
    }

    public void FruitSliced()
    {
        if (gameEnded) return;

        currentSlices++;

        if (currentSlices >= sliceGoal)
            WinGame();
    }

    void WinGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        GameModeManager.Instance.MinigameCompleted();
    }

    public void LoseGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        GameModeManager.Instance.MinigameFailed();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
