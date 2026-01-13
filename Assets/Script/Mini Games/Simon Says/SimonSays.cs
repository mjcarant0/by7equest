using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimonSaysLinkedListUI : MonoBehaviour
{
    public SpriteRenderer background;
    public KeyCode playerKey = KeyCode.Space;
    public TextMeshProUGUI commandDisplay;
    public SpriteButton spriteButton;

    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    private string[] commandOptions = new string[]
    {
        "Simon says press me",
        "Simon says don't press me",
        "Press me",
        "Don't press me"
    };

    private LinkedList<string> commandSequence;

    private float commandTime = 2f;
    private float feedbackTime = 0.4f;

    private int mistakes = 0;
    private int correctCount = 0;
    private int requiredCorrect;

    public float timeLimit = 12f;
    private float timer;
    private bool gameEnded = false;
    private bool resultSent = false;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        if (GameModeManager.Instance != null)
        {
            timer = GameModeManager.Instance.GetTimeLimitForExternalCall();
            
            // Adjust speed based on difficulty
            float speedMultiplier = GetSpeedMultiplier();
            commandTime /= speedMultiplier;
            feedbackTime /= speedMultiplier;
        }
        else
        {
            timer = timeLimit;
        }

        background.color = Color.black;

        mistakes = 0;
        correctCount = 0;
        gameEnded = false;
        resultSent = false;

        // Player needs 5 correct commands to win
        requiredCorrect = 5;

        commandSequence = new LinkedList<string>();

        // Start with one command
        AddRandomCommand();

        StartCoroutine(RunGame());
    }

    void Update()
    {
        if (gameEnded || GameModeManager.Instance == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            EndGame(false);
        }
    }

    void AddRandomCommand()
    {
        int randIndex = Random.Range(0, commandOptions.Length);
        commandSequence.AddLast(commandOptions[randIndex]);
    }

    IEnumerator RunGame()
    {
        LinkedListNode<string> currentNode = commandSequence.First;

        while (true) // infinite loop
        {
            if (gameEnded) yield break;

            string cmd = currentNode.Value;
            commandDisplay.text = cmd;

            // Arm the button for this new command
            if (spriteButton != null)
                spriteButton.ArmForNewCommand();

            bool pressed = false;
            float commandTimer = 0f;

            while (commandTimer < commandTime)
            {
                if (Input.GetKeyDown(playerKey))
                {
                    pressed = true;
                    if (spriteButton != null)
                        spriteButton.TryPlay();
                }

                commandTimer += Time.deltaTime;
                yield return null;
            }

            bool correct = CheckCommand(cmd, pressed);

            if (SoundManager.Instance != null)
            {
                if (correct && correctSFX != null)
                    SoundManager.Instance.PlaySFX(correctSFX);
                else if (!correct && wrongSFX != null)
                    SoundManager.Instance.PlaySFX(wrongSFX);
            }

            background.color = correct ? Color.green : Color.red;

            if (correct)
                correctCount++;
            else
            {
                mistakes++;

                // Deduct time based on mistake count
                if (mistakes == 1)
                    timer -= 1f;
                else if (mistakes == 2)
                    timer -= 3f;
                else if (mistakes == 3)
                    timer -= 5f;
                else if (mistakes >= 4)
                {
                    // 4th mistake: deduct life
                    mistakes = 0;
                    EndGame(false); // This will deduct a life via GameModeManager
                    yield break;
                }

                // Clamp timer to non-negative before checking expiry
                if (timer < 0f) timer = 0f;

                // Check if time penalty caused time to run out
                if (timer <= 0f)
                {
                    timer = 0f;
                    EndGame(false);
                    yield break;
                }
            }

            yield return new WaitForSecondsRealtime(feedbackTime);
            background.color = Color.black;

            // WIN (5 correct commands)
            if (correctCount >= requiredCorrect)
            {
                EndGame(true);
                yield break;
            }

            // Move to next command
            if (currentNode.Next == null)
            {
                AddRandomCommand(); // infinite LinkedList growth
            }

            currentNode = currentNode.Next;
        }
    }

    bool CheckCommand(string cmd, bool pressed)
    {
        if (cmd == "Simon says press me") return pressed;
        if (cmd == "Simon says don't press me") return !pressed;
        if (cmd == "Press me") return !pressed;
        if (cmd == "Don't press me") return pressed;

        return false;
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

    void EndGame(bool success)
    {
        if (resultSent) return;

        resultSent = true;
        gameEnded = true;
        commandDisplay.text = "";

        StopAllCoroutines();
        GameModeManager.Instance.ResolveMinigame(success);
    }
}