using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimonSaysLinkedListUI : MonoBehaviour
{
    public SpriteRenderer background;
    public KeyCode playerKey = KeyCode.Space;
    public TextMeshProUGUI commandDisplay;

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

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        if (EasyModeManager.Instance != null)
            timer = timeLimit;

        background.color = Color.black;

        mistakes = 0;
        correctCount = 0;
        gameEnded = false;

        // Player needs 2–5 correct commands to win
        requiredCorrect = Random.Range(2, 6);

        commandSequence = new LinkedList<string>();

        // Start with one command
        AddRandomCommand();

        StartCoroutine(RunGame());
    }

    void Update()
    {
        if (gameEnded) return;
        if (EasyModeManager.Instance == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            gameEnded = true;
            commandDisplay.text = "";
            EasyModeManager.Instance.MinigameFailed();
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
            string cmd = currentNode.Value;
            commandDisplay.text = cmd;

            bool pressed = false;
            float timer = 0f;

            while (timer < commandTime)
            {
                if (Input.GetKeyDown(playerKey))
                    pressed = true;

                timer += Time.deltaTime;
                yield return null;
            }

            bool correct = CheckCommand(cmd, pressed);

            background.color = correct ? Color.green : Color.red;

            if (correct)
                correctCount++;
            else
                mistakes++;

            yield return new WaitForSeconds(feedbackTime);
            background.color = Color.black;

            // ❌ GAME OVER (3 mistakes)
            if (mistakes >= 3)
            {
                gameEnded = true;
                commandDisplay.text = "";
                EasyModeManager.Instance.MinigameFailed();
                yield break;
            }

            // ✅ WIN (2–5 correct commands)
            if (correctCount >= requiredCorrect)
            {
                gameEnded = true;
                commandDisplay.text = "";
                EasyModeManager.Instance.MinigameCompleted();
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
}
