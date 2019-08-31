﻿using System.Collections;
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
                tilesOnField[i][j] = GenerateNewTile(new Vector2(i, j));
        }
    }

    private Tile GenerateNewTile(Vector2 adress)
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
        ((int)adress.x > 1 && tileColor == tilesOnField[(int)adress.x - 1][(int)adress.y].tileColor && tileColor == tilesOnField[(int)adress.x - 2][(int)adress.y].tileColor) ||  //Check for ready match by x
        ((int)adress.y > 1 && tileColor == tilesOnField[(int)adress.x][(int)adress.y - 1].tileColor && tileColor == tilesOnField[(int)adress.x][(int)adress.y - 2].tileColor)     //Check for ready match by y
        );

        tile.ResetColor(tileColor);
        tile.transform.localPosition = new Vector3(adress.x, tilesOnField[0].Length - 1); //Move tile on top of column
        tile.ResetAdress(new Vector2(adress.x, adress.y));
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
        //Much better than phisical swap tiles, but if there is no need animation, just swap color values and sprites
        TileColor tmpColor = first.tileColor;
        first.ResetColor(second.tileColor);
        second.ResetColor(tmpColor);
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
            if (tilesOnField[index][i] == null || tilesOnField[index][i - 1] == null)
            {
                if (tilesForMatch.Count >= 3)
                    tilesForBurst.AddRange(tilesForMatch);
                tilesForMatch.Clear();
            }
            else
            if (tilesOnField[i][index]==null || tilesOnField[i - 1][index] == null || tilesOnField[i][index].tileColor != tilesOnField[i - 1][index].tileColor)
            {
                if (tilesForMatch.Count >= 3)
                    tilesForBurst.AddRange(tilesForMatch);
                tilesForMatch.Clear();
            }
            if (tilesOnField[index][i] != null)
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
            if(tilesOnField[index][j] == null || tilesOnField[index][j - 1] == null)
            {
                if (tilesForMatch.Count >= 3)
                    tilesForBurst.AddRange(tilesForMatch);
                tilesForMatch.Clear();
            }
            else
            if (tilesOnField[index][j].tileColor != tilesOnField[index][j - 1].tileColor)
            {
                if (tilesForMatch.Count >= 3)
                    tilesForBurst.AddRange(tilesForMatch);
                tilesForMatch.Clear();
            }

            if (tilesOnField[index][j] != null)
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
        Invoke("Tilefall", 0.1f);
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
                tilesOnField[(int)adress.x][i].ResetAdress(new Vector2(adress.x, i)); //Set tile new adress
                tilesOnField[(int)adress.x][j] = null;
                i++; //Increase upper empty
                j = i;
            }
        }
        
        for(; i < tilesOnField[0].Length; i++) //Create new tiles on top
        {
                tilesOnField[(int)adress.x][i] = GenerateNewTile(new Vector2(adress.x,i));
        }
    }
}