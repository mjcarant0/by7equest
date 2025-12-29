using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode { Easy, Medium, Hard, God }

    [Header("Player Stats - Persistent Across Scenes")]
    [Tooltip("HEART SYSTEM: Player health, starts at 3. Persists across all scenes.")]
    public int lives = 3;
    
    [Tooltip("Total accumulated score across all minigames and modes")]
    public int score = 0;
    
    [Tooltip("Score earned in the most recent minigame (for display purposes)")]
    public int lastMinigameScore = 0;

    [Header("Difficulty Progression - Game Loop Control")]
    [Tooltip("Current difficulty mode (Easy→Medium→Hard→God)")]
    public GameMode currentMode = GameMode.Easy;
    
    [Tooltip("Tracks completed minigames in current mode. Resets to 0 when advancing modes.")]
    public int minigamesCompletedInMode = 0;
    
    [Tooltip("Required games per mode for Easy/Medium/Hard (default: 6). God mode has no limit.")]
    public int gamesPerMode = 6;

    [Header("Timer")]
    public float timer;
    private bool timerRunning;

    [Header("Scene Names")]
    public string transitionScene = "TempTransition";
    public string gameStartScene = "GameStart"; 
    public string gameEndScene = "GameEnd";
    public string scoreScene = "ScoreScene";
    public string closingScene = "ClosingScene";
    public string landingPage = "LandingPage";

    [Header("References")]
    public HeartUIHandler heartUIHandler;

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

    private void Start()
    {
        StartCoroutine(InitializeHeartUI());
    }

    private IEnumerator InitializeHeartUI()
    {
        // Wait one frame to ensure all objects are loaded
        yield return null;
        
        if (heartUIHandler == null) // To Fix: Remove heart ui handler to Landing Page
        {
            heartUIHandler = FindObjectOfType<HeartUIHandler>();
            if (heartUIHandler == null)
            {
                Debug.LogWarning("[GameModeManager] HeartUIHandler not found in scene! Please add HeartUIHandler component to your UI.");
            }
        }
        
        if (heartUIHandler != null)
        {
            heartUIHandler.HideHearts();
            heartUIHandler.UpdateHearts(lives);
            Debug.Log("[GameModeManager] Heart UI initialized successfully");
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
        
        // Life loss or heart update
        if (!success)
        {
            lives--;
            Debug.Log($"[GameModeManager] Player lost a life. Remaining lives: {lives}");
            if (heartUIHandler != null)
            {
                heartUIHandler.UpdateHearts(lives);
            }
        }

        // Calculate score for this minigame
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

        // Check if game is over (no lives left)
        if (lives <= 0)
        {
            if (heartUIHandler != null)
            {
                heartUIHandler.HideHearts();
            }
            
            Debug.Log("[GameModeManager] Game Over! Loading Score Scene then Closing Scene");
            
            // Show Score Scene only at final game over
            SceneManager.LoadScene(scoreScene);
            yield return new WaitForSecondsRealtime(sceneDelay);
            
            SceneManager.LoadScene(closingScene);
            yield break;
        }

        minigamesCompletedInMode++;
        Debug.Log($"[GameModeManager] Completed {minigamesCompletedInMode}/{gamesPerMode} games in {currentMode} mode");

        // Check if we need to advance difficulty
        if (minigamesCompletedInMode >= gamesPerMode)
        {
            if (currentMode != GameMode.God)
            {
                // Advance to next difficulty
                GameMode previousMode = currentMode;
                minigamesCompletedInMode = 0; // Reset counter for new mode
                AdvanceDifficulty();
                Debug.Log($"[GameModeManager] Completed {gamesPerMode} games in {previousMode}, advancing to {currentMode}");
                
                // Show transition, then it will load Game Start
                yield return StartCoroutine(ShowModeTransition(currentMode));
                yield break;
            }
            else
            {
                // Already in God mode, just continue (no game limit in God mode)
                Debug.Log($"[GameModeManager] In God mode, continuing...");
            }
        }

        // Continue in current mode - go directly to Game Start
        Debug.Log($"[GameModeManager] Continuing in {currentMode} mode ({minigamesCompletedInMode}/{gamesPerMode}), loading Game Start");
        SceneManager.LoadScene(gameStartScene);
    }

    private IEnumerator ShowModeTransition(GameMode newMode) // fix transition controller 
    {
        Debug.Log($"[GameModeManager] Showing transition to {newMode}");
        
        // Load transition scene
        SceneManager.LoadScene(transitionScene);
        yield return new WaitForSecondsRealtime(0.5f);

        float waitTime = 0f;
        float maxWaitTime = 3f;
        while (TempTransitionController.Instance == null && waitTime < maxWaitTime)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            waitTime += 0.1f;
        }
        
        if (TempTransitionController.Instance == null)
        {
            Debug.LogError("[GameModeManager] TempTransitionController not found after waiting! Skipping transition.");
            SceneManager.LoadScene(gameStartScene);
            yield break;
        }
        
        if (TempTransitionController.Instance.backgroundImage == null)
        {
            Debug.LogError("[GameModeManager] TempTransitionController backgroundImage not assigned! Skipping transition.");
            SceneManager.LoadScene(gameStartScene);
            yield break;
        }

        bool transitionComplete = false;
        TempTransitionController.Instance.ShowTransition(newMode, () =>
        {
            transitionComplete = true;
        });

        // Wait for transition with timeout
        float transitionWaitTime = 0f;
        float transitionTimeout = 5f;
        while (!transitionComplete && transitionWaitTime < transitionTimeout)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            transitionWaitTime += 0.1f;
        }
        
        if (!transitionComplete)
        {
            Debug.LogWarning("[GameModeManager] Transition did not complete in time, continuing anyway");
        }
        
        // After mode transition, go directly to Game Start
        Debug.Log("[GameModeManager] Transition complete, loading Game Start");
        SceneManager.LoadScene(gameStartScene);
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
        lives = 3;
        score = 0;
        lastMinigameScore = 0;
        minigamesCompletedInMode = 0;
        currentMode = GameMode.Easy;
        
        if (heartUIHandler != null)
        {
            heartUIHandler.UpdateHearts(lives);
            heartUIHandler.HideHearts();
        }
    }

    public void StartTimerExternally()
    {
        if (heartUIHandler == null)
        {
            heartUIHandler = FindObjectOfType<HeartUIHandler>();
        }
        
        if (heartUIHandler != null)
        {
            heartUIHandler.ShowHearts();
            heartUIHandler.UpdateHearts(lives);
        }
        else
        {
            Debug.LogWarning("[GameModeManager] HeartUIHandler not found when starting timer!");
        }
        
        timerRunning = true;
        Debug.Log($"[GameModeManager] Timer started: {timer}s for {currentMode} mode with {lives} lives");
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
            case GameMode.Easy: return 30f;
            case GameMode.Medium: return 25f;
            case GameMode.Hard: return 20f;
            case GameMode.God: return 15f;
            default: return 30f;
        }
    }

    public void LoadGameStartScene()
    {
        Debug.Log("[GameModeManager] Loading Game Start Scene");
        SceneManager.LoadScene(gameStartScene);
    }
}
