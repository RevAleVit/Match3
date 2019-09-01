using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private string bpSavesName = "best";

    private int currentPoints;
    private int bestPoints;

    [SerializeField] private int countMoves = 50;

    private int movesLeft;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(bpSavesName))
            bestPoints = PlayerPrefs.GetInt(bpSavesName);

        GUIManager.instance.SetBestPoints(bestPoints);
        ResetGame();
    }

    public void StartGame(int fieldSize)
    {        
        FieldController.instance.GenerateNewField(fieldSize);
    }
    
    public void DecreaseMoves()
    {
        movesLeft--;
        GUIManager.instance.SetMovesLeftCount(movesLeft);
        if (movesLeft <= 0)
        {
            Invoke("EndGame", 1f);
        }
    }

    public void IncreasePoints(int value)
    {
        currentPoints += value;
        GUIManager.instance.SetCurrentPoints(currentPoints);
    }

    private void EndGame()
    {
        GUIManager.instance.EndGame();
        if (currentPoints > bestPoints)
        {
            bestPoints = currentPoints;
            PlayerPrefs.SetInt(bpSavesName, bestPoints);
            GUIManager.instance.SetBestPoints(bestPoints);
        }
    }

    public void ResetGame()
    {
        FieldController.instance.ClearField();
        currentPoints = 0;
        movesLeft = countMoves;
        GUIManager.instance.SetCurrentPoints(currentPoints);
        GUIManager.instance.SetMovesLeftCount(movesLeft);
    }

}
