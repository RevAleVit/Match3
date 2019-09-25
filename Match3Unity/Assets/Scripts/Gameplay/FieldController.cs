using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    //private void start()
    //{
    //    generatenewfield(9);
    //}

    public void GenerateNewField(int size)
    {
        transform.position = new Vector2(-size/2f + 0.5f, -size/2f + 0.5f);
        tilesOnField = new Tile[size][];
        for(int i = 0; i < size; i++)                                           //Create columns
        {
            tilesOnField[i] = new Tile[size];
            for(int j = 0; j < size; j++)                                       //Generate tiles in current column
                tilesOnField[i][j] = GenerateNewTile(new Vector2Int(i, j));
        }
    }

    public void ClearField()
    {
        if (tilesOnField == null) return;
        foreach (Tile[] rows in tilesOnField)
        {
            foreach (Tile tile in rows)
            {
                if (tile != null)
                    tile.Burst();
            }
        }
        highlightFxObject.SetActive(false);
    }

    private Tile GenerateNewTile(Vector2Int adress)
    {
        Tile tile;
        tile = TilesStash.GetTile();                      //Try apply tile from stash

        if (tile == null)                                  //Check for existing tile
            tile = Instantiate(prefabTile, transform);    //Instantiate new tile

        TileColor tileColor;
        do
        {
            tileColor = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
        } while ( //Generate new color while there is a ready match. - Protect from pregenerated matches
        (adress.x > 1 && tileColor == tilesOnField[adress.x - 1][adress.y].tileColor && tileColor == tilesOnField[adress.x - 2][adress.y].tileColor) ||  //Check for ready match by x
        (adress.y > 1 && tileColor == tilesOnField[adress.x][adress.y - 1].tileColor && tileColor == tilesOnField[adress.x][adress.y - 2].tileColor)     //Check for ready match by y
        );

        tile.ResetColor(tileColor);
        tile.transform.localPosition = new Vector3(adress.x, tilesOnField[0].Length - 1); //Move tile on top of column
        tile.ResetAdress(new Vector2Int(adress.x, adress.y));
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

                List<Tile> tilesForBurst = CheckAdressesForMatch(new Vector2Int[] { selectedTile.adress, tile.adress });
                if (tilesForBurst.Count <= 0)
                {
                    SwapTiles(selectedTile, tile);  //Swap tiles back if no matches ready
                    selectedTile = tile;
                    highlightFxObject.transform.position = tile.transform.position;
                    highlightFxObject.SetActive(true);
                }
                else
                {
                    GameManager.instance.DecreaseMoves();
                    BurstTiles(tilesForBurst);      //Burst matched tiles
                    selectedTile = null;
                    highlightFxObject.SetActive(false);
                }
            }
        }
    }

    private void SwapTiles(Tile first, Tile second)
    {
        //Much better than phisical swap tiles, but if there is no need animation, just swap color values and sprites
        TileColor tmpColor = first.tileColor;
        first.ResetColor(second.tileColor);
        second.ResetColor(tmpColor);
    }

    private List<Tile> CheckMatchesOnAllField()
    {
        List<Tile> tilesForBurst = new List<Tile>();

        Vector2Int[] adressesForCheck = new Vector2Int[tilesOnField[0].Length];
        for (int i = 0; i < tilesOnField[0].Length; i++)
        {
            adressesForCheck[i] = new Vector2Int(i, i);
        }

        return CheckAdressesForMatch(adressesForCheck);
    }

    private List<Tile> CheckAdressesForMatch(Vector2Int[] adresses)
    {
        List<Tile> tilesForBurst = new List<Tile>();

        foreach(Vector2Int adress in adresses)
        {
            tilesForBurst.AddRange(CheckColumnForMatch(adress.x));
            tilesForBurst.AddRange(CheckRowForMatch(adress.y));
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
            if (tilesOnField[i][index]==null || tilesOnField[i - 1][index] == null || tilesOnField[i][index].tileColor != tilesOnField[i - 1][index].tileColor)
            {
                tilesForBurst.AddRange(GetMatch(tilesForMatch));
                tilesForMatch.Clear();
            }
            if (tilesOnField[index][i] != null)
                tilesForMatch.Add(tilesOnField[i][index]);            
        }
        tilesForBurst.AddRange(GetMatch(tilesForMatch)); //Check match with last tile

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
                tilesForBurst.AddRange(GetMatch(tilesForMatch));
                tilesForMatch.Clear();
            }

            if (tilesOnField[index][j] != null)
                tilesForMatch.Add(tilesOnField[index][j]);

        }
        tilesForBurst.AddRange(GetMatch(tilesForMatch)); //Check match with last tile

        return tilesForBurst;
    }

    private List<Tile> GetMatch(List<Tile> list)
    {
        List<Tile> tilesForBurst = new List<Tile>();
        if (list.Count >= 5)
        {
            foreach (Tile[] rows in tilesOnField)
            {
                foreach (Tile tile in rows)
                {
                    if (tile.tileColor == list[0].tileColor)
                        tilesForBurst.Add(tile);
                }
            }
        }
        else
        if (list.Count >= 4)
        {
            if (list[0].adress.x == list[1].adress.x)
                tilesForBurst.AddRange(tilesOnField[list[0].adress.x].ToList());
            else
                for (int i = 0; i < tilesOnField.Length; i++)
                    tilesForBurst.Add(tilesOnField[i][list[0].adress.y]);
        }
        else
        if (list.Count >= 3)
            tilesForBurst.AddRange(list);


            return tilesForBurst;
    }

    private void BurstTiles(List<Tile> tilesForBurst)
    {
        foreach(Tile tile in tilesForBurst)
        {
            if (tilesOnField[tile.adress.x][tile.adress.y] != null) //Check for already bursted tile
            {
                GameManager.instance.IncreasePoints(1);
                tile.Burst();
                tilesOnField[tile.adress.x][tile.adress.y] = null;
            }
        }
        Invoke("Tilefall", 0.1f);
    }

    private void Tilefall()
    {
        for(int i = 0; i < tilesOnField[0].Length; i ++)
        {
            for (int j = 0; j < tilesOnField[0].Length; j++)
            {
                if (tilesOnField[i][j] == null)
                    ColumnFall(new Vector2Int(i, j));
            }
        }

        //StartCoroutine(CheckMatchesAfterFallDelay(1f));
        BurstTiles(CheckMatchesOnAllField()); //Check matches after fall
    }

    IEnumerator CheckMatchesAfterFallDelay(float value)
    {
        yield return new WaitForSeconds(value);
        BurstTiles(CheckMatchesOnAllField()); //Check matches after fall
    }

    private void ColumnFall(Vector2Int adress) //Send adress of tile to falling on it
    {
        int i = adress.y; //index of upper empty tile in column
        for (int j = i + 1; j < tilesOnField[0].Length; j ++) //Fall exist tiles
        {
            if(tilesOnField[adress.x][j] != null) //Check tile for non empty
            {
                tilesOnField[adress.x][i] = tilesOnField[adress.x][j]; //Fall tile on upper empty
                tilesOnField[adress.x][i].ResetAdress(new Vector2Int(adress.x, i)); //Set tile new adress
                tilesOnField[adress.x][j] = null;
                i++; //Increase upper empty
                j = i;
            }
        }
        
        for(; i < tilesOnField[0].Length; i++) //Create new tiles on top
        {
                tilesOnField[adress.x][i] = GenerateNewTile(new Vector2Int(adress.x,i));
        }
    }
}