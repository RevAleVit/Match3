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
                /*tilesOnField[i][j] = TilesStash.GetTile();                      //Try apply tile from stash

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
                
                tilesOnField[i][j].Reset(tileColor, new Vector2(i, j));*/
                tilesOnField[i][j] = GenerateNewTile(new Vector2(i, j));
            }
        }
    }

    private Tile GenerateNewTile(Vector2 adress)
    {
        Tile tile;
        tile = TilesStash.GetTile();                      //Try apply tile from stash

        if (tile == null)                                  //Check for existing tile
            tile = Instantiate(prefabTile, transform);    //Instantiate new tile

        tile.transform.localPosition = new Vector2(adress.x, adress.y);
        TileColor tileColor;
        do
        {
            tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
        } while ( //Generate new color while there is a ready match. - Protect from pregenerated matches
        ((int)adress.x > 1 && tileColor == tilesOnField[(int)adress.x - 1][(int)adress.y].tileColor && tileColor == tilesOnField[(int)adress.x - 2][(int)adress.y].tileColor) ||  //Check for ready match by x
        ((int)adress.y > 1 && tileColor == tilesOnField[(int)adress.x][(int)adress.y - 1].tileColor && tileColor == tilesOnField[(int)adress.x][(int)adress.y - 2].tileColor)     //Check for ready match by y
        );

        tile.Reset(tileColor, new Vector2(adress.x, adress.y));
        return tile;
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

                List<Tile> tilesForBurst = CheckAdressesForMatch(new Vector2[] { selectedTile.adress, tile.adress });
                if (tilesForBurst.Count <= 0)
                {
                    SwapTiles(selectedTile, tile);  //Swap tiles back if no matches ready
                    Debug.Log("Swapback");
                }
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

    private List<Tile> CheckMatchesOnAllField()
    {
        List<Tile> tilesForBurst = new List<Tile>();

        Vector2[] adressesForCheck = new Vector2[tilesOnField[0].Length];
        for (int i = 0; i < tilesOnField[0].Length; i++)
        {
            adressesForCheck[i] = new Vector2(i, i);
        }

        return CheckAdressesForMatch(adressesForCheck);
    }

    private List<Tile> CheckAdressesForMatch(Vector2[] adresses)
    {
        List<Tile> tilesForBurst = new List<Tile>();

        foreach(Vector2 adress in adresses)
        {
            tilesForBurst.AddRange(CheckColumnForMatch((int)adress.x));
            tilesForBurst.AddRange(CheckRowForMatch((int)adress.y));
        }

        return tilesForBurst;
    }

    private List<Tile> CheckRowForMatch(int index)
    {
        List<Tile> tilesForBurst = new List<Tile>();
        List<Tile> tilesForMatch = new List<Tile>();       //List of tiles for match

        tilesForMatch.Add(tilesOnField[0][index]);
        for (int i = 1; i < tilesOnField[0].Length; i++)
        {
            if (tilesOnField[i][index].tileColor != tilesOnField[i - 1][index].tileColor)
            {
                if (tilesForMatch.Count >= 3)
                    tilesForBurst.AddRange(tilesForMatch);
                tilesForMatch.Clear();
            }
            tilesForMatch.Add(tilesOnField[i][index]);            
        }
        if (tilesForMatch.Count >= 3) //Check match with last tile
            tilesForBurst.AddRange(tilesForMatch);

        return tilesForBurst;
    }

    private List<Tile> CheckColumnForMatch(int index)
    {
        List<Tile> tilesForBurst = new List<Tile>();
        List<Tile> tilesForMatch = new List<Tile>();       //List of tiles for match

        tilesForMatch.Add(tilesOnField[index][0]);
        for (int j = 1; j < tilesOnField[0].Length; j++)
        {
            if (tilesOnField[index][j].tileColor != tilesOnField[index][j - 1].tileColor)
            {
                if (tilesForMatch.Count >= 3)
                    tilesForBurst.AddRange(tilesForMatch);
                tilesForMatch.Clear();
            }
            tilesForMatch.Add(tilesOnField[index][j]);
        }
        if (tilesForMatch.Count >= 3) //Check match with last tile
            tilesForBurst.AddRange(tilesForMatch);

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
        Invoke("Tilefall", 1f);
    }

    private void Tilefall()
    {
        for(int i = 0; i < tilesOnField[0].Length; i ++)
        {
            for (int j = 0; j < tilesOnField[0].Length; j++)
            {
                if (tilesOnField[i][j] == null)
                    ColumnFall(new Vector2(i, j));
            }
        }

        BurstTiles(CheckMatchesOnAllField()); //Check matches after fall
    }

    private void ColumnFall(Vector2 adress) //Send adress of tile to falling on it
    {
        int i = (int)adress.y; //index of upper empty tile in column
        for (int j = i + 1; j < tilesOnField[0].Length; j ++) //Fall exist tiles
        {
            if(tilesOnField[(int)adress.x][j] != null) //Check tile for non empty
            {
                tilesOnField[(int)adress.x][i] = tilesOnField[(int)adress.x][j]; //Fall tile on upper empty
                tilesOnField[(int)adress.x][i].adress = new Vector2(adress.x, i); //Set tile new adress
                tilesOnField[(int)adress.x][i].transform.localPosition = tilesOnField[(int)adress.x][i].adress; //Change tile position
                tilesOnField[(int)adress.x][j] = null;
                i++; //Increase upper empty
                j = i + 1;
            }
        }

        for(; i < tilesOnField[0].Length; i++) //Create new tiles on top
        {
            tilesOnField[(int)adress.x][i] = GenerateNewTile(new Vector2(adress.x,i));
        }
    }
}