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
        for(int i = 0; i < size; i++)                                           //Create columns
        {
            tilesOnField[i] = new Tile[size];
            for(int j = 0; j < size; j++)                                       //Generate tiles in current column
            {
                tilesOnField[i][j] = TilesStash.GetTile();                      //Try apply tile from stash

                if(tilesOnField[i][j] == null)                                  //Check for existing tile
                    tilesOnField[i][j] = Instantiate(prefabTile, transform);    //Instantiate new tile

                tilesOnField[i][j].transform.localPosition = new Vector2(i, j);
                TileColor tileColor;
                do
                {
                    tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
                } while ( //Generate new color while there is a ready match
                (i > 1 && tileColor == tilesOnField[i - 1][j].tileColor && tileColor == tilesOnField[i - 2][j].tileColor) ||  //Check for ready match by x
                (j > 1 && tileColor == tilesOnField[i][j - 1].tileColor && tileColor == tilesOnField[i][j - 2].tileColor)     //Check for ready match by y
                );
                
                tilesOnField[i][j].Reset(tileColor, new Vector2(i, j));
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
            if (selectedTile == null ||                                                                                 //Check for not already selected tile
                ((Mathf.Abs(selectedTile.adress.x - tile.adress.x) > 1 || selectedTile.adress.y != tile.adress.y) &&    //Check for not proximity by x
                (Mathf.Abs(selectedTile.adress.y - tile.adress.y) > 1 || selectedTile.adress.x != tile.adress.x)))      //Check for not proximity by y
            {
                selectedTile = tile;            //Select new tile
                highlightFxObject.transform.position = tile.transform.position;
                highlightFxObject.SetActive(true);                
            }
            else
            {                                       //Tiles are proximity
                SwapTiles(selectedTile, tile);      //Swap tiles

                List<Tile> tilesForBurst = CheckMatch(selectedTile.adress, tile.adress);
                if (tilesForBurst.Count <= 0)
                    SwapTiles(selectedTile, tile);  //Swap tiles back if no matches ready
                else
                {
                    BurstTiles(tilesForBurst);      //Burst matched tiles
                    selectedTile = null;
                    highlightFxObject.SetActive(false);
                }
            }
        }
    }

    private void SwapTiles(Tile first, Tile second)
    {
        /*tilesOnField[(int)first.adress.x][(int)first.adress.y] = tilesOnField[(int)second.adress.x][(int)second.adress.y];
        tilesOnField[(int)second.adress.x][(int)second.adress.y] = tilesOnField[(int)first.adress.x][(int)first.adress.y];
        Vector2 tmp = first.adress;
        first.adress = second.adress;
        second.adress = tmp;
        first.transform.localPosition = first.adress;
        second.transform.localPosition = second.adress;*/


        //Much better, but if there is no need animation, just swap colors value and sprites
        TileColor tmpColor = first.tileColor;
        first.SetColor(second.tileColor);
        second.SetColor(tmpColor);
    }

    private List<Tile> CheckMatchesOnAllField(Vector2 adress1, Vector2 adress2)
    {
        List<Tile> tilesForBurst = new List<Tile>();

        for (int i = 1; i < tilesOnField[0].Length; i++)
        {
            //Check every lines and columns
            //Add founded matches to list for burst tiles
            //Send adresses of current(i) column&line and previous column&line
            tilesForBurst.AddRange(CheckMatch(new Vector2(i - 1, i - 1), new Vector2(i, i)));
        }

        return tilesForBurst;
    }

    private List<Tile> CheckMatch(Vector2 adress1, Vector2 adress2)
    {
        List<Tile> tilesForBurst = new List<Tile>();

        List<Tile> tilesForMatch1 = new List<Tile>();       //List of tiles for  match by first adress
        List<Tile> tilesForMatch2 = new List<Tile>();       //List of tiles for  match by second adress
        tilesForMatch1.Add(tilesOnField[0][(int)adress1.y]);
        tilesForMatch2.Add(tilesOnField[0][(int)adress2.y]);
        for (int i = 1; i < tilesOnField[0].Length; i++)    //Check lines  
        {
            if (tilesOnField[i][(int)adress1.y].tileColor != tilesOnField[i - 1][(int)adress1.y].tileColor)     //Check line by adress 1
            {
                if (tilesForMatch1.Count >= 3)
                    tilesForBurst.AddRange(tilesForMatch1);
                tilesForMatch1.Clear();
            }
            tilesForMatch1.Add(tilesOnField[i][(int)adress1.y]);

            if (adress1.y != adress2.y)                                                                         //Not check already checked tiles
            {
                if (tilesOnField[i][(int)adress2.y].tileColor != tilesOnField[i - 1][(int)adress2.y].tileColor) //Check line by adress 2
                {
                    if (tilesForMatch2.Count >= 3)
                        tilesForBurst.AddRange(tilesForMatch2);
                    tilesForMatch2.Clear();
                }
                tilesForMatch2.Add(tilesOnField[i][(int)adress2.y]);
            }
        }

        tilesForMatch1.Clear();
        tilesForMatch2.Clear();
        tilesForMatch1.Add(tilesOnField[(int)adress1.x][0]);
        tilesForMatch2.Add(tilesOnField[(int)adress2.x][0]);
        for (int j = 1; j < tilesOnField[0].Length; j++)    //Check columns
        {
            if (tilesOnField[(int)adress1.x][j].tileColor != tilesOnField[(int)adress1.x][j - 1].tileColor)     //Check column by  adress 1
            {
                if (tilesForMatch1.Count >= 3)
                    tilesForBurst.AddRange(tilesForMatch1);
                tilesForMatch1.Clear();
            }
            tilesForMatch1.Add(tilesOnField[(int)adress1.x][j]);

            if (adress1.x != adress2.x)                                                                         //Not check already checked tiles
            {
                if (tilesOnField[(int)adress2.x][j].tileColor != tilesOnField[(int)adress2.x][j - 1].tileColor) //Check column by  adress 2
                {
                    if (tilesForMatch2.Count >= 3)
                        tilesForBurst.AddRange(tilesForMatch2);
                    tilesForMatch2.Clear();
                }
                tilesForMatch2.Add(tilesOnField[(int)adress2.x][j]);
            }
        }

        return tilesForBurst;
    }

    private void BurstTiles(List<Tile> tilesForBurst)
    {
        foreach(Tile tile in tilesForBurst)
        {
            if (tilesOnField[(int)tile.adress.x][(int)tile.adress.y] != null) //Check for already bursted tile
            {
                tile.Burst();
                tilesOnField[(int)tile.adress.x][(int)tile.adress.y] = null;
            }
        }
    }   
}
