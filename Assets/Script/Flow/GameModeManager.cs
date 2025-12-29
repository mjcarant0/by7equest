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
    
    [Tooltip("Required games per mode for Easy/Medium/Hard (default: 6)")]
    public int gamesPerMode = 6;
    
    [Tooltip("Required games for God mode (default: 10)")]
    public int godModeGames = 10;

    [Header("Timer")]
    public float timer;
    private bool timerRunning;

    [Header("Scene Names")]
    public string transitionScene = "TempTransition";
    public string gameStartScene = "GameStart"; 
    public string gameEndScene = "GameEnd";
    public string scoreScene = "ScoreScene";
    public string closingScene = "ClosingScene";
    public string nameInputScene = "NameInput";
    public string landingPage = "LandingPage";

    public float sceneDelay = 2.5f;

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
        
        // Calculate score for this minigame (no heart loss)
        lastMinigameScore = success ? (GetBaseScoreForExternalCall() + timeBonus) : 0;
        if (currentMode == GameMode.God && success)
        {
            lastMinigameScore *= 2;
        }
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

        minigamesCompletedInMode++;
        Debug.Log($"[GameModeManager] Completed {minigamesCompletedInMode} games in {currentMode} mode");

        // Check if current mode is complete
        int requiredGames = (currentMode == GameMode.God) ? godModeGames : gamesPerMode;
        
        if (minigamesCompletedInMode >= requiredGames)
        {
            // Mode complete!
            if (currentMode == GameMode.God)
            {
                // God mode complete - GAME IS FINISHED!
                Debug.Log("[GameModeManager] God mode complete! Game finished. Starting end sequence.");
                yield return StartCoroutine(ShowEndSequence());
                yield break;
            }
            else
            {
                // Easy/Medium/Hard complete - advance to next mode
                GameMode previousMode = currentMode;
                minigamesCompletedInMode = 0; 
                AdvanceDifficulty();
                Debug.Log($"[GameModeManager] Completed {requiredGames} games in {previousMode}, advancing to {currentMode}");
                
                // Show transition, then it will load Game Start
                yield return StartCoroutine(ShowModeTransition(currentMode));
                yield break;
            }
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
        Debug.Log("[GameModeManager] Starting end sequence: ScoreScene → ClosingScene → NameInput → LandingPage");
        
        // ScoreScene
        Debug.Log("[GameModeManager] Loading ScoreScene");
        SceneManager.LoadScene(scoreScene);
        yield return new WaitForSecondsRealtime(sceneDelay);
        
        // ClosingScene
        Debug.Log("[GameModeManager] Loading ClosingScene");
        SceneManager.LoadScene(closingScene);
        yield return new WaitForSecondsRealtime(sceneDelay);
        
        // NameInput
        Debug.Log("[GameModeManager] Loading NameInput");
        SceneManager.LoadScene(nameInputScene);
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

    public void FinalizeSession(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("[GameModeManager] Player name is empty, using Anonymous");
            playerName = "Anonymous";
        }

        Debug.Log($"[GameModeManager] Finalizing - Player: {playerName}, Score: {score}, Mode: {currentMode}");
        
        // TODO: Save to database
        SaveToDatabase(playerName, score, currentMode.ToString());
        
        ResetGame();
        
        // Return to landing page
        SceneManager.LoadScene(landingPage);
    }

    private void SaveToDatabase(string playerName, int finalScore, string modeReached)
    {
        // TODO: Implement database save
        Debug.Log($"[GameModeManager] SAVE TO DATABASE: {playerName} - {finalScore} - {modeReached}");
    }

    private void ResetGame()
    {
        Debug.Log("[GameModeManager] Resetting game state");
        score = 0;
        lastMinigameScore = 0;
        minigamesCompletedInMode = 0;
        currentMode = GameMode.Easy;
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
            case GameMode.Easy: return 100;
            case GameMode.Medium: return 200;
            case GameMode.Hard: return 300;
            case GameMode.God: return 500;
            default: return 100;
        }
    }

    public float GetTimeLimitForExternalCall()
    {
        switch (currentMode)
        {
            case GameMode.Easy: return 20f;
            case GameMode.Medium: return 15f;
            case GameMode.Hard: return 12f;
            case GameMode.God: return 10f;
            default: return 20f;
        }
    }

    public void LoadGameStartScene()
    {
        Debug.Log("[GameModeManager] Loading Game Start Scene");
        SceneManager.LoadScene(gameStartScene);
    }
}
