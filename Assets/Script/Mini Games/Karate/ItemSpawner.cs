using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour
{
    public AudioClip woodChopSFX;

    [Header("Prefabs")]
    public GameObject woodPrefab;
    public GameObject stonePrefab;
    public GameObject karateTable; 
    
    [Header("Settings")]
    public float spawnDelay = 0.15f; 
    public int itemsPerRound = 40; 
    public float itemSpacing = 0.33f;
    public float itemFallTime = 1f;

    [Header("Game Time & Lives")]
    public int minBoardsToWin = 6;
    private int rocksHitCount = 0;
    private int totalBoards = 0;
    private int boardsBroken = 0;

    private Stack<GameObject> woodStack = new Stack<GameObject>();
    private bool isSpawning = false;
    private bool timerStarted = false;
    private float topItemTimer = 0f; 
    private Vector3 initialTablePos;

    private bool gameEnded = false;

    void Start()
    {
        initialTablePos = karateTable.transform.position;
        
        // Stop the GameModeManager's timer during spawning
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.StopTimerExternally();
            
            float speedMultiplier = GetSpeedMultiplier();
            spawnDelay /= speedMultiplier;
            itemFallTime /= speedMultiplier;
        }
        
        StartCoroutine(SpawnInitialStack());
    }

    float GetSpeedMultiplier()
    {
        switch (GameModeManager.Instance.currentMode)
        {
            case GameModeManager.GameMode.Easy: return 1.0f;
            case GameModeManager.GameMode.Medium: return 1.3f;
            case GameModeManager.GameMode.Hard: return 1.6f;
            case GameModeManager.GameMode.God: return 2.0f;
            default: return 1.0f;
        }
    }

    System.Collections.IEnumerator SpawnInitialStack()
    {
        isSpawning = true;
        
        for (int i = 0; i < itemsPerRound; i++)
        {
            SpawnItem(i);
            yield return new WaitForSecondsRealtime(spawnDelay);
        }

        yield return new WaitForSecondsRealtime(0.5f);
        
        isSpawning = false;
        
        // Wait a brief moment before starting the timer so player can see all items
        yield return new WaitForSecondsRealtime(0.3f);
        
        // Start the GameModeManager timer
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.StartTimerExternally();
        }
        
        timerStarted = true;
    }

    void SpawnItem(int index)
    {
        bool isWood = Random.value < 0.7f; 
        GameObject prefab = isWood ? woodPrefab : stonePrefab;
        Vector3 spawnPos = transform.position + new Vector3(0, index * itemSpacing, 0);
        GameObject newItem = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Track total boards spawned
        if (isWood)
        {
            totalBoards++;
        }

        // STACK (LIFO)
        woodStack.Push(newItem);
    }

    void Update()
    {
        // karate table
        if (gameEnded) return;

        // Check if GameModeManager timer has expired (only after spawning is complete)
        if (timerStarted && GameModeManager.Instance != null && GameModeManager.Instance.timer <= 0)
        {
            // Time's up - check if player broke at least 6 boards
            gameEnded = true;
            GameModeManager.Instance.StopTimerExternally();
            
            bool success = boardsBroken >= minBoardsToWin;
            GameModeManager.Instance.ResolveMinigame(success);
            enabled = false;
            return;
        }

        // WIN CONDITION (all items gone - broken or vanished)
        if (!isSpawning && woodStack.Count == 0 && !gameEnded)
        {
            gameEnded = true;
            
            // Stop GameModeManager timer to prevent double resolution
            if (GameModeManager.Instance != null)
            {
                GameModeManager.Instance.StopTimerExternally();
            }
            
            bool success = boardsBroken >= minBoardsToWin;
            GameModeManager.Instance.ResolveMinigame(success);
            enabled = false;
            return;
        }

        // karate table movement
        if (karateTable != null)
        {
            float targetY = initialTablePos.y - (woodStack.Count * itemSpacing);
            Vector3 targetTablePos = new Vector3(karateTable.transform.position.x, targetY, karateTable.transform.position.z);
            karateTable.transform.position = Vector3.Lerp(karateTable.transform.position, targetTablePos, Time.deltaTime * 10f);
        }

        if (woodStack.Count == 0 || isSpawning) 
        {
            topItemTimer = 0; 
            return;
        }

        // PEEK AND POP on the items
        GameObject topItem = woodStack.Peek();
        if (topItem == null) { woodStack.Pop(); return; }

        KarateItem script = topItem.GetComponent<KarateItem>();
        
        topItemTimer += Time.deltaTime;

        if (topItemTimer >= itemFallTime)
        {
            // POP
            woodStack.Pop(); 
            Destroy(topItem); 
            topItemTimer = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (script.isWood)
            {
                // Hit a board - SUCCESS
                woodStack.Pop();
                boardsBroken++;
                script.ChopWood();
                topItemTimer = 0;

                if (woodChopSFX != null && SoundManager.Instance != null)
                    SoundManager.Instance.PlaySFX(woodChopSFX);
            }
            else
            {
                // Hit a rock - PENALTY
                rocksHitCount++;
                
                if (rocksHitCount == 1)
                {
                    // 1st rock: -2 seconds
                    if (GameModeManager.Instance != null)
                    {
                        float timeBefore = GameModeManager.Instance.timer;
                        GameModeManager.Instance.timer = Mathf.Max(0, GameModeManager.Instance.timer - 2f);
                        float timeDeducted = timeBefore - GameModeManager.Instance.timer;
                        Debug.Log($"[Karate] Stone hit #{rocksHitCount}! Time deduction: -{timeDeducted:F1} seconds (Timer: {GameModeManager.Instance.timer:F1}s remaining)");
                    }
                }
                else if (rocksHitCount == 2)
                {
                    // 2nd rock: -5 seconds
                    if (GameModeManager.Instance != null)
                    {
                        float timeBefore = GameModeManager.Instance.timer;
                        GameModeManager.Instance.timer = Mathf.Max(0, GameModeManager.Instance.timer - 5f);
                        float timeDeducted = timeBefore - GameModeManager.Instance.timer;
                        Debug.Log($"[Karate] Stone hit #{rocksHitCount}! Time deduction: -{timeDeducted:F1} seconds (Timer: {GameModeManager.Instance.timer:F1}s remaining)");
                    }
                }
                else if (rocksHitCount == 3)
                {
                    // 3rd rock: -10 seconds
                    if (GameModeManager.Instance != null)
                    {
                        float timeBefore = GameModeManager.Instance.timer;
                        GameModeManager.Instance.timer = Mathf.Max(0, GameModeManager.Instance.timer - 10f);
                        float timeDeducted = timeBefore - GameModeManager.Instance.timer;
                        Debug.Log($"[Karate] Stone hit #{rocksHitCount}! Time deduction: -{timeDeducted:F1} seconds (Timer: {GameModeManager.Instance.timer:F1}s remaining)");
                    }
                }
                else if (rocksHitCount >= 4)
                {
                    // 4th rock: Game Over (lose a life)
                    Debug.Log($"[Karate] Stone hit #{rocksHitCount}! CRITICAL HIT - GAME OVER!");
                    gameEnded = true;
                    
                    // Stop GameModeManager timer to prevent double resolution
                    if (GameModeManager.Instance != null)
                    {
                        GameModeManager.Instance.StopTimerExternally();
                    }
                    
                    GameModeManager.Instance.ResolveMinigame(false);
                    enabled = false;
                    return;
                }

                // Remove the rock from stack
                woodStack.Pop();
                script.ChopWood(); // Visual feedback
                topItemTimer = 0;
            }
        }
    }
}
