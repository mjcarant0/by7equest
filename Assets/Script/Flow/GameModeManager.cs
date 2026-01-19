using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// Tree Node for Difficulty Progression
public class DifficultyNode
{
    public string name;
    public int baseScore;
    public float timeLimit;
    public DifficultyNode parent;
    public List<DifficultyNode> children;

    public DifficultyNode(string name, int baseScore, float timeLimit)
    {
        this.name = name;
        this.baseScore = baseScore;
        this.timeLimit = timeLimit;
        this.children = new List<DifficultyNode>();
        this.parent = null;
    }

    public void AddChild(DifficultyNode child)
    {
        children.Add(child);
        child.parent = this;
    }
}

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public enum GameMode { Easy, Medium, Hard, God }

    [Header("Player Stats - Persistent Across Scenes")]
    [Tooltip("Total accumulated score across all minigames and modes")]
    public int score = 0;

    [Tooltip("Score earned in the most recent minigame (for display purposes)")]
    public int lastMinigameScore = 0;

    [Tooltip("Base score from the most recent minigame (before bonus)")]
    public int lastMinigameBaseScore = 0;

    [Tooltip("Time bonus from the most recent minigame")]
    public int lastMinigameBonus = 0;

    [Header("Difficulty Progression - Game Loop Control")]
    [Tooltip("Current difficulty mode (Easy→Medium→Hard→God)")]
    public GameMode currentMode = GameMode.Easy;

    [Tooltip("Tracks completed minigames in current mode. Resets to 0 when advancing modes.")]
    public int minigamesCompletedInMode = 0;

    [Header("Tree Data Structure - Difficulty Progression")]
    private DifficultyNode difficultyTree;
    private DifficultyNode currentDifficultyNode;

    [Tooltip("Required games per mode for Easy/Medium/Hard (mechanic: 6)")]
    public int gamesPerMode = 6;

    [Tooltip("God mode is unlimited; this is ignored")]
    public int godModeGames = 0;

    [Header("Timer")]
    public float timer;
    private bool timerRunning;
    private bool isGameOver = false;
    private bool isResolvingMinigame = false;

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
            InitializeDifficultyTree();
            Debug.Log("[GameModeManager] Instance created and set to DontDestroyOnLoad");
        }
        else
        {
            // Keep old instance alive until NameInput finishes saving the score
            Debug.LogWarning("[GameModeManager] Duplicate found, destroying new instance to preserve score");
            Destroy(gameObject);
            return;
        }
    }

    private void InitializeDifficultyTree()
    {
        // Root Node
        difficultyTree = new DifficultyNode("Game Mode", 0, 0f);

        // Create difficulty nodes with their properties
        DifficultyNode easyNode = new DifficultyNode("Easy", 20, 25f);
        DifficultyNode mediumNode = new DifficultyNode("Medium", 50, 20f);
        DifficultyNode hardNode = new DifficultyNode("Hard", 100, 15f);
        DifficultyNode godNode = new DifficultyNode("God", 200, 10f);

        // Build tree: Root -> Easy -> Medium -> Hard -> God
        difficultyTree.AddChild(easyNode);
        easyNode.AddChild(mediumNode);
        mediumNode.AddChild(hardNode);
        hardNode.AddChild(godNode);

        // Start at Easy
        currentDifficultyNode = easyNode;

        Debug.Log("[GameModeManager] Difficulty Tree initialized: Root -> Easy -> Medium -> Hard -> God");
    }

    public void ResolveMinigame(bool success, int timeBonus = 0)
    {
        if (Instance == null)
        {
            Debug.LogError("[GameModeManager] Instance is null in ResolveMinigame!");
            return;
        }

        // Prevent duplicate resolution calls
        if (isResolvingMinigame)
        {
            Debug.LogWarning("[GameModeManager] ResolveMinigame already called, ignoring duplicate call");
            return;
        }

        isResolvingMinigame = true;
        timerRunning = false;

        // Base score per mode
        int baseScore = GetBaseScoreForExternalCall();

        // If caller didn't pass a bonus but time remains, grant it automatically
        if (success && timeBonus == 0)
        {
            // Use remaining whole seconds as bonus
            timeBonus = Mathf.Max(0, Mathf.FloorToInt(timer));
        }

        int bonus = timeBonus;
        int total = success ? (baseScore + bonus) : 0;

        // God mode: no extra multiplier (base score for God mode is already set higher)

        lastMinigameScore = total;
        lastMinigameBaseScore = success ? baseScore : 0;
        lastMinigameBonus = success ? bonus : 0;
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
        // Use Tree traversal to advance difficulty
        if (currentDifficultyNode.children.Count > 0)
        {
            currentDifficultyNode = currentDifficultyNode.children[0]; // Move to next child node
            Debug.Log($"[Tree] Advanced to: {currentDifficultyNode.name} (Score: {currentDifficultyNode.baseScore}, Time: {currentDifficultyNode.timeLimit}s)");
        }
        else
        {
            Debug.Log($"[Tree] Already at max difficulty: {currentDifficultyNode.name}");
        }

        // Update enum to match tree node
        switch (currentDifficultyNode.name)
        {
            case "Easy":
                currentMode = GameMode.Easy;
                break;
            case "Medium":
                currentMode = GameMode.Medium;
                break;
            case "Hard":
                currentMode = GameMode.Hard;
                break;
            case "God":
                currentMode = GameMode.God;
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

        // Reset resolution flag for next minigame
        isResolvingMinigame = false;
        
        timer = GetTimeLimitForExternalCall();
        MinigameRandomizer.Instance.LoadNextMinigame();
    }

    public void FinalizeSession(string playerName = "")
    {
        // If called with player name (from NameInput scene), always allow it to load landing page
        if (!string.IsNullOrEmpty(playerName))
        {
            Debug.Log($"[GameModeManager] Session finalized: {playerName} - {score} - {currentMode}");
            isGameOver = true;
            StopAllCoroutines();
            
            // Score already saved by NameInputController, just reset and load landing page
            ResetGame();
            HeartUIHandler.StaticResetLives();
            SceneManager.LoadScene(landingPage);
            return;
        }

        // For game-over-from-loss path, check if already called
        if (isGameOver)
        {
            Debug.Log("[GameModeManager] FinalizeSession already called, ignoring duplicate");
            return;
        }

        isGameOver = true;
        Debug.Log($"[GameModeManager] Game Over triggered! Score: {score}, Mode: {currentMode}");

        // Stop any running coroutines to prevent conflicts
        StopAllCoroutines();

        // Called from HeartUIHandler when lives reach 0 - start end sequence
        StartCoroutine(ShowEndSequence());
    }

    private void ResetGame()
    {
        Debug.Log("[GameModeManager] Resetting game state");
        score = 0;
        lastMinigameScore = 0;
        minigamesCompletedInMode = 0;
        currentMode = GameMode.Easy;
        isGameOver = false;

        // Reset tree to Easy node
        if (difficultyTree != null && difficultyTree.children.Count > 0)
        {
            currentDifficultyNode = difficultyTree.children[0]; // Back to Easy
            Debug.Log("[Tree] Reset to Easy node");
        }
    }

    public void DestroyOldInstance()
    {
        // Called after score is saved to clean up old instance
        if (Instance != null && Instance.gameObject != null)
        {
            Debug.Log("[GameModeManager] Destroying old instance after score saved");
            Destroy(Instance.gameObject);
        }
    }

    public void StartTimerExternally()
    {
        timerRunning = true;
        Debug.Log($"[GameModeManager] Timer started: {timer}s for {currentMode} mode");
    }

    public void StopTimerExternally()
    {
        timerRunning = false;
        Debug.Log($"[GameModeManager] Timer stopped externally");
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
        // Use Tree node data instead of switch statement
        if (currentDifficultyNode != null)
        {
            return currentDifficultyNode.baseScore;
        }

        // Fallback to switch if tree not initialized
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
        // Use Tree node data instead of switch statement
        if (currentDifficultyNode != null)
        {
            return currentDifficultyNode.timeLimit;
        }

        // Fallback to switch if tree not initialized
        switch (currentMode)
        {
            case GameMode.Easy: return 25f;
            case GameMode.Medium: return 20f;
            case GameMode.Hard: return 15f;
            case GameMode.God: return 10f;
            default: return 25f;
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

    public void ResetForNewRun()
    {
        Debug.Log("[GameModeManager] Initializing new run: clearing state and flags");
        // Ensure no stale coroutines interfere with next run
        StopAllCoroutines();

        // Clear gameplay state
        score = 0;
        lastMinigameScore = 0;
        lastMinigameBaseScore = 0;
        lastMinigameBonus = 0;
        minigamesCompletedInMode = 0;
        currentMode = GameMode.Easy;

        // Reset control flags and timers
        isGameOver = false;
        isResolvingMinigame = false;
        timerRunning = false;
        timer = 0f;

        // Reset tree to Easy node
        if (difficultyTree != null && difficultyTree.children.Count > 0)
        {
            currentDifficultyNode = difficultyTree.children[0]; // Back to Easy
            Debug.Log("[Tree] Reset to Easy node for new run");
        }
    }
}