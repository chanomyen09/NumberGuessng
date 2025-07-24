using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

public class Gameplay : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI attemptLeft;
    [SerializeField] private TextMeshProUGUI currentPlayer;
    [SerializeField] private TextMeshProUGUI gameState;
    [SerializeField] private TextMeshProUGUI gameLog;

    [Header("Input")]
    [SerializeField] private TMP_InputField guessInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button newgameButton;

    [Header("Game Settings")]
    [SerializeField] private int minNumber = 1;
    [SerializeField] private int maxNumber = 100;
    [SerializeField] private int maxAttempts = 12;

    private int targetNumber;
    private int currentAttempts;
    private bool isPlayerTurn;
    private bool gameActive;

    private int computerMinGuess;
    private int computerMaxGuess;
    private List<int> computerGuesses; // Renamed for clarity

    private void Awake()
    {
        submitButton.onClick.AddListener(OnSubmitGuess);
        newgameButton.onClick.AddListener(StartNewGame);
        guessInputField.onSubmit.AddListener(delegate { OnSubmitGuess(); });
    }

    private void Start()
    {
        StartNewGame();
    }

    private void StartNewGame()
    {
        targetNumber = Random.Range(minNumber, maxNumber + 1);
        currentAttempts = 0;
        isPlayerTurn = true;
        gameActive = true;

        attemptLeft.text = $"Attempts Left: {maxAttempts}";
        currentPlayer.text = "Player Turn";
        gameLog.text = "=== Game Log ===\nNew game started; Player goes first.\n";
        gameState.text = "";

        guessInputField.text = "";
        guessInputField.interactable = true;
        submitButton.interactable = true;
        guessInputField.Select();
        guessInputField.ActivateInputField();

        computerMinGuess = minNumber;
        computerMaxGuess = maxNumber;
        computerGuesses = new List<int>(); // Initialize the list
    }

    private void OnSubmitGuess()
    {
        if (!gameActive || !isPlayerTurn) return;

        string input = guessInputField.text.Trim();
        if (string.IsNullOrEmpty(input))
        {
            gameState.text = "Input cannot be empty.";
            return;
        }

        if (!int.TryParse(input, out int guess))
        {
            gameState.text = "Please enter a valid number.";
            return;
        }

        if (guess < minNumber || guess > maxNumber)
        {
            gameState.text = $"Please enter a number between {minNumber} and {maxNumber}.";
            return;
        }

        gameState.text = "";
        guessInputField.text = "";
        HandleGuess(guess, true);
    }

    private void HandleGuess(int guess, bool playerGuess)
    {
        currentAttempts++;
        string playerName = playerGuess ? "Player" : "Computer";
        gameLog.text += $"{playerName} guessed: {guess}\n";

        if (guess == targetNumber)
        {
            gameLog.text += $"{playerName} got it right!\n";
            EndGame($"{playerName} wins!");
            return;
        }

        if (currentAttempts >= maxAttempts)
        {
            gameLog.text += $"Game Over! The correct number was {targetNumber}\n";
            EndGame("No more attempts!");
            return;
        }

        string hint = guess < targetNumber ? "<sprite=\"cross\" index=116> Too Low" : "<sprite=\"cross\" index=115> Too High";
        gameLog.text += $"{hint}!\n";

        isPlayerTurn = !playerGuess;
        currentPlayer.text = isPlayerTurn ? "Player Turn" : "Computer Turn";
        attemptLeft.text = $"Attempts Left: {maxAttempts - currentAttempts}";

        if (!isPlayerTurn)
        {
            guessInputField.interactable = false;
            submitButton.interactable = false;
            StartCoroutine(ComputerTurn(guess < targetNumber));
        }
        else
        {
            guessInputField.interactable = true;
            submitButton.interactable = true;
            guessInputField.Select();
            guessInputField.ActivateInputField();
        }
    }

    private IEnumerator ComputerTurn(bool targetIsHigher)
    {
        yield return new WaitForSeconds(1.5f);
        if (!gameActive) yield break;

        int lastGuess = computerGuesses.Count > 0 ? computerGuesses[computerGuesses.Count - 1] : (computerMinGuess + computerMaxGuess) / 2;

        if (computerGuesses.Count > 0)
        {
            if (targetIsHigher)
            {
                computerMinGuess = lastGuess + 1;
            }
            else
            {
                computerMaxGuess = lastGuess - 1;
            }
        }

        int compGuess = (computerMinGuess + computerMaxGuess) / 2;
        compGuess = Mathf.Clamp(compGuess, computerMinGuess, computerMaxGuess);

        computerGuesses.Add(compGuess);

        HandleGuess(compGuess, false);
    }

    private void EndGame(string message)
    {
        gameActive = false;
        guessInputField.interactable = false;
        submitButton.interactable = false;
        currentPlayer.text = "Game Over";
        gameState.text = message;
        Canvas.ForceUpdateCanvases();
    }
}