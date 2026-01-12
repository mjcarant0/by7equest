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
    public float spawnDelay = 0.2f; 
    public int itemsPerRound = 10; 
    public float itemSpacing = 0.45f;
    public float itemFallTime = 1.0f;

    [Header("Game Time & Lives")]
    public float gameTime = 30f; 
    private float currentTime;
    private int rocksHitCount = 0;
    private int totalBoards = 0;
    private int boardsBroken = 0;

    private Stack<GameObject> woodStack = new Stack<GameObject>();
    private bool isSpawning = false;
    private float topItemTimer = 0f; 
    private Vector3 initialTablePos;

    private bool gameEnded = false;

    void Start()
    {
        initialTablePos = karateTable.transform.position;
        currentTime = gameTime;
        
        // Adjust speed based on difficulty
        if (GameModeManager.Instance != null)
        {
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

        // Update game timer
        if (!isSpawning)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                // Time's up - Game Over
                currentTime = 0;
                gameEnded = true;
                GameModeManager.Instance.ResolveMinigame(false);
                enabled = false;
                return;
            }
        }

        // WIN CONDITION (all boards broken)
        if (!isSpawning && boardsBroken >= totalBoards && totalBoards > 0 && !gameEnded)
        {
            gameEnded = true;
            GameModeManager.Instance.ResolveMinigame(true);
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
                Debug.Log($"[Karate] Rock hit! Count: {rocksHitCount}");

                if (rocksHitCount == 1)
                {
                    // 1st rock: -2 seconds
                    currentTime = Mathf.Max(0, currentTime - 2f);
                    Debug.Log("[Karate] 1st rock penalty: -2 seconds");
                }
                else if (rocksHitCount == 2)
                {
                    // 2nd rock: -5 seconds
                    currentTime = Mathf.Max(0, currentTime - 5f);
                    Debug.Log("[Karate] 2nd rock penalty: -5 seconds");
                }
                else if (rocksHitCount == 3)
                {
                    // 3rd rock: -10 seconds
                    currentTime = Mathf.Max(0, currentTime - 10f);
                    Debug.Log("[Karate] 3rd rock penalty: -10 seconds");
                }
                else if (rocksHitCount >= 4)
                {
                    // 4th rock: Game Over (lose a life)
                    Debug.Log("[Karate] 4th rock penalty: GAME OVER");
                    gameEnded = true;
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
