using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    public static GUIManager instance;

    [Header("Screens")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject gameOver;

    [Header("Values")]
    [SerializeField] private Text bestPoints;
    [SerializeField] private Text currentPoints;
    [SerializeField] private Text movesLeftCount;

    private int fieldSize = 6;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }

    public void SetFieldSize(Slider fieldSizeSlider)
    {
        fieldSize = (int)fieldSizeSlider.value;
    }
    
    public void StartGame()
    {
        mainMenu.SetActive(false);
        GameManager.instance.StartGame(fieldSize);
    }

    public void EndGame()
    {
        gameOver.SetActive(true);
    }

    public void MainMenu()
    {
        gameOver.SetActive(false);
        mainMenu.SetActive(true);
        GameManager.instance.ResetGame();
    }

    public void SetBestPoints(int value) => bestPoints.text = value.ToString();

    public void SetCurrentPoints(int value) => currentPoints.text = value.ToString();

    public void SetMovesLeftCount(int value) => movesLeftCount.text = value.ToString();

}
