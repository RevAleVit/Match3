using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldController : MonoBehaviour
{
    public static FieldController instance;

    public Tile[][] tilesOnField;

    private Tile selectedTile;
    private GameObject highlightFxObject;

    [SerializeField] private Tile prefabTile;
    [SerializeField] private GameObject prefabHighlight;

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;

        if (highlightFxObject == null)
        {
            highlightFxObject = Instantiate(prefabHighlight);
            highlightFxObject.SetActive(false);
        }
    }

    private void Start()
    {
        GenerateNewField(9);
    }

    public void GenerateNewField(int size)
    {
        transform.position = new Vector2(-size/2f + 0.5f, -size/2f + 0.5f);
        tilesOnField = new Tile[size][];
        for(int i = 0; i < size; i++)       //Create columns
        {
            tilesOnField[i] = new Tile[size];
            for(int j = 0; j < size; j++)   //Generate tiles in current column
            {
                tilesOnField[i][j] = Instantiate(prefabTile, transform);
                tilesOnField[i][j].transform.localPosition = new Vector2(i, j);
                tilesOnField[i][j].Reset((TileColor)Random.Range(0, 5), new Vector2(i,j));
            }
        }
    }

    public void ClickOn(Tile tile)
    {
        if (selectedTile == tile)
        {
            selectedTile = null;    //Deselect earlier selected tile
            highlightFxObject.SetActive(false);
        }
        else
        {
            if (selectedTile == null || 
                ((Mathf.Abs(selectedTile.adress.x - tile.adress.x) > 1 || selectedTile.adress.y != tile.adress.y) &&
                (Mathf.Abs(selectedTile.adress.y - tile.adress.y) > 1 || selectedTile.adress.x != tile.adress.x)))
            {
                selectedTile = tile;
                highlightFxObject.transform.position = tile.transform.position;
                highlightFxObject.SetActive(true);                
            }
            else
            {
                SwapTiles(selectedTile, tile);
                selectedTile = null;
                highlightFxObject.SetActive(false);
            }
        }
    }

    private void SwapTiles(Tile first, Tile second)
    {
        tilesOnField[(int)first.adress.x][(int)first.adress.y] = tilesOnField[(int)second.adress.x][(int)second.adress.y];
        tilesOnField[(int)second.adress.x][(int)second.adress.y] = tilesOnField[(int)first.adress.x][(int)first.adress.y];

        Vector2 tmp = first.adress;
        first.adress = second.adress;
        second.adress = tmp;

        first.transform.localPosition = first.adress;
        second.transform.localPosition = second.adress;
    }
}
