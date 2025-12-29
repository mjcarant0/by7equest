using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Game Flow Manager - Controls progression through all difficulty modes
/// 
/// PERSISTENCE: Uses DontDestroyOnLoad singleton pattern to persist across ALL scenes
/// 
/// NO HEART SYSTEM: Player progresses through all modes regardless of performance
/// 
/// GAME LOOP LOGIC:
/// Easy/Medium/Hard Modes:
///   - Each mode requires exactly 6 minigames
///   - After 6th game, triggers TempTransition to next mode
///   - Counter resets when advancing to next mode
/// 
/// God Mode:
///   - Plays exactly 10 minigames
///   - After 10th game, game is complete
///   - Goes directly to NameInput scene
/// 
/// SCENE FLOW:
/// Normal: GameEnd → GameStart (next minigame)
/// Mode Complete: GameEnd → TempTransition → GameStart (new mode)
/// Game Complete: GameEnd → NameInput (save score)
/// </summary>
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

    /// <summary>
    /// Main minigame resolution handler - Called when a minigame ends
    /// 
    /// NO HEART SYSTEM: Success/failure only affects score, not progression
    /// SCORING: Calculates score based on success, time bonus, and current mode
    /// FLOW: Triggers PostGameSequence which handles all subsequent logic
    /// </summary>
    /// <param name="success">Did the player win the minigame?</param>
    /// <param name="timeBonus">Bonus points for completing quickly</param>
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

    /// <summary>
    /// Post-Game Sequence - Controls entire game flow after minigame completion
    /// 
    /// FLOW LOGIC:
    /// 1. Load GameEnd scene (shows result)
    /// 2. Increment minigamesCompletedInMode counter
    /// 3. For Easy/Medium/Hard: Check if 6 games completed
    ///    - YES: Show TempTransition → Advance mode → GameStart
    ///    - NO: Load GameStart (continue current mode)
    /// 4. For God Mode: Check if 10 games completed
    ///    - YES: Game Complete! → Load NameInput scene
    ///    - NO: Load GameStart (continue God mode)
    /// </summary>
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
                Debug.Log("[GameModeManager] God mode complete! Game finished. Going to NameInput scene.");
                SceneManager.LoadScene(nameInputScene);
                yield break;
            }
            else
            {
                // Easy/Medium/Hard complete - advance to next mode
                GameMode previousMode = currentMode;
                minigamesCompletedInMode = 0; // Reset counter for new mode
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
