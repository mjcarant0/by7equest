using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode { Easy, Medium, Hard, God }

    [Header("Player Stats - Persistent Across Scenes")]
    [Tooltip("Total accumulated score across all minigames and modes")]
    public int score = 0;

    [Tooltip("Score earned in the most recent minigame (for display purposes)")]
    public int lastMinigameScore = 0;

    [Header("Difficulty Progression - Game Loop Control")]
    [Tooltip("Current difficulty mode (Easy→Medium→Hard→God)")]
    public GameMode currentMode = GameMode.Easy;

    [Tooltip("Tracks completed minigames in current mode. Resets to 0 when advancing modes.")]
    public int minigamesCompletedInMode = 0;

    [Tooltip("Required games per mode for Easy/Medium/Hard (mechanic: 6)")]
    public int gamesPerMode = 6;

    [Tooltip("God mode is unlimited; this is ignored")]
    public int godModeGames = 0;

    [Header("Timer")]
    public float timer;
    private bool timerRunning;
    private bool isGameOver = false;

    [Header("Scene Names")]
    public string transitionScene = "ModeDisplay";
    public string gameStartScene = "GameStart";
    public string gameEndScene = "GameEnd";
    public string scoreScene = "ScoreScene";
    public string closingScene = "ClosingScene";
    public string nameInputScene = "NameInput";
    public string landingPage = "LandingPage";

    public float sceneDelay = 2.5f;

    [Header("Closing Scene Video")]
    [Tooltip("Duration to wait for the closing scene video before loading NameInput")]
    public float closingSceneVideoDuration = 5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[GameModeManager] Instance created and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.LogWarning("[GameModeManager] Duplicate found, destroying");
            Destroy(gameObject);
            return;
        }
    }

    public void ResolveMinigame(bool success, int timeBonus = 0)
    {
        if (Instance == null)
        {
            Debug.LogError("[GameModeManager] Instance is null in ResolveMinigame!");
            return;
        }

        timerRunning = false;

        // Base score per mode
        int baseScore = GetBaseScoreForExternalCall();
        int total = success ? (baseScore + timeBonus) : 0;

        // God mode: score and bonus are doubled
        if (success && currentMode == GameMode.God)
        {
            total *= 2;
        }

        lastMinigameScore = total;
        score += lastMinigameScore;

        Debug.Log($"[GameModeManager] Minigame resolved. Success: {success}, Score added: {lastMinigameScore}, Total score: {score}");

        StartCoroutine(PostGameSequence());
    }

    private IEnumerator PostGameSequence()
    {
        Debug.Log("[GameModeManager] Starting post-game sequence");

        // Show Game End animation
        Debug.Log("[GameModeManager] Loading Game End Scene");
        SceneManager.LoadScene(gameEndScene);
        yield return new WaitForSecondsRealtime(sceneDelay);

        // Check if game is over (no lives left) - this will be set by HeartUIHandler
        if (isGameOver)
        {
            Debug.Log("[GameModeManager] Game Over detected, PostGameSequence stopping - FinalizeSession will handle end sequence");
            yield break;
        }

        minigamesCompletedInMode++;
        Debug.Log($"[GameModeManager] Completed {minigamesCompletedInMode} games in {currentMode} mode");

        // If in God mode, never complete; keep loading next minigame
        if (currentMode == GameMode.God)
        {
            Debug.Log("[GameModeManager] God mode is unlimited, continuing in God mode");
            SceneManager.LoadScene(gameStartScene);
            yield break;
        }

        // Check if current mode is complete (Easy/Medium/Hard)
        int requiredGames = gamesPerMode;
        if (minigamesCompletedInMode >= requiredGames)
        {
            // Mode complete
            GameMode previousMode = currentMode;
            minigamesCompletedInMode = 0;
            AdvanceDifficulty();
            Debug.Log($"[GameModeManager] Completed {requiredGames} games in {previousMode}, advancing to {currentMode}");

            // Show transition, then it will load Game Start
            yield return StartCoroutine(ShowModeTransition(currentMode));
            yield break;
        }

        // Continue in current mode - go directly to Game Start
        Debug.Log($"[GameModeManager] Continuing in {currentMode} mode ({minigamesCompletedInMode}/{requiredGames}), loading Game Start");
        SceneManager.LoadScene(gameStartScene);
    }

    private IEnumerator ShowModeTransition(GameMode newMode)
    {
        Debug.Log($"[GameModeManager] Showing transition to {newMode}");
        SceneManager.LoadScene(transitionScene);
        yield break;
    }

    private IEnumerator ShowEndSequence()
    {
        Debug.Log("[GameModeManager] Starting end sequence: GameEnd → ClosingScene (ClosingScene will handle progression to NameInput)");

        // GameEnd is already loaded by PostGameSequence, wait for it to display
        Debug.Log("[GameModeManager] GameEnd showing (score displayed)");
        yield return new WaitForSecondsRealtime(sceneDelay);

        Debug.Log("[GameModeManager] Loading ClosingScene - video will play and handle NameInput transition");
        SceneManager.LoadScene(closingScene);
        // ClosingScene controller will load NameInput when its video finishes
    }

    private GameMode AdvanceDifficulty()
    {
        switch (currentMode)
        {
            case GameMode.Easy:
                currentMode = GameMode.Medium;
                break;
            case GameMode.Medium:
                currentMode = GameMode.Hard;
                break;
            case GameMode.Hard:
                currentMode = GameMode.God;
                break;
            case GameMode.God:
                // Stay in God mode
                break;
        }
        return currentMode;
    }

    public void LoadNextMinigame()
    {
        if (MinigameRandomizer.Instance == null)
        {
            Debug.LogError("[GameModeManager] MinigameRandomizer not found!");
            return;
        }

        timer = GetTimeLimitForExternalCall();
        MinigameRandomizer.Instance.LoadNextMinigame();
    }

    public void FinalizeSession(string playerName = "")
    {
        if (isGameOver)
        {
            Debug.Log("[GameModeManager] FinalizeSession already called, ignoring duplicate");
            return;
        }

        isGameOver = true;
        Debug.Log($"[GameModeManager] Game Over triggered! Score: {score}, Mode: {currentMode}");

        // Stop any running coroutines to prevent conflicts
        StopAllCoroutines();

        // If called with player name (from NameInput scene), save and return to landing page
        if (!string.IsNullOrEmpty(playerName))
        {
            SaveToDatabase(playerName, score, currentMode.ToString());
            ResetGame();
            SceneManager.LoadScene(landingPage);
        }
        else
        {
            // Called from HeartUIHandler when lives reach 0 - start end sequence
            StartCoroutine(ShowEndSequence());
        }
    }

    private void SaveToDatabase(string playerName, int finalScore, string modeReached)
    {
        Debug.Log($"[GameModeManager] SAVE TO DATABASE: {playerName} - {finalScore} - {modeReached}");
    }

    private void ResetGame()
    {
        Debug.Log("[GameModeManager] Resetting game state");
        score = 0;
        lastMinigameScore = 0;
        minigamesCompletedInMode = 0;
        currentMode = GameMode.Easy;
        isGameOver = false;
    }

    public void StartTimerExternally()
    {
        timerRunning = true;
        Debug.Log($"[GameModeManager] Timer started: {timer}s for {currentMode} mode");
    }

    private void Update()
    {
        if (!timerRunning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Debug.Log("[GameModeManager] Timer expired! Resolving as failure");
            timerRunning = false;
            ResolveMinigame(false);
        }
    }

    public int GetTotalScore()
    {
        return score;
    }

    public GameMode GetCurrentMode()
    {
        return currentMode;
    }

    public int GetBaseScoreForExternalCall()
    {
        switch (currentMode)
        {
            case GameMode.Easy: return 20;
            case GameMode.Medium: return 50;
            case GameMode.Hard: return 100;
            case GameMode.God: return 200;
            default: return 20;
        }
    }

    public float GetTimeLimitForExternalCall()
    {
        switch (currentMode)
        {
            case GameMode.Easy: return 30f;
            case GameMode.Medium: return 25f;
            case GameMode.Hard: return 20f;
            case GameMode.God: return 10f;
            default: return 30f;
        }
    }

    public void LoadGameStartScene()
    {
        Debug.Log("[GameModeManager] Loading Game Start Scene");
        SceneManager.LoadScene(gameStartScene);
    }

    public void LoadScoreSceneFromMenu()
    {
        Debug.Log("[GameModeManager] Loading Score Scene from menu");
        SceneManager.LoadScene(scoreScene);
    }
}