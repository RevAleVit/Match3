using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileColor
{
    Cyan,
    Magenta,
    Yellow,
    Blue,
    Orange
}

public class Tile : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public Vector2 adress { get; private set; }
    public TileColor tileColor { get; private set; }

    [SerializeField] private Sprite spriteCyan;
    [SerializeField] private Sprite spriteMagenta;
    [SerializeField] private Sprite spriteYellow;
    [SerializeField] private Sprite spriteBlue;
    [SerializeField] private Sprite spriteOrange;

    Coroutine translateToNewPosition;

    public void ResetAdress(Vector2 adress)
    {
        this.adress = adress;

        if (translateToNewPosition != null)         //Check for tile alredy in move
            StopCoroutine(translateToNewPosition);  //Stop tile mooving

        translateToNewPosition = StartCoroutine(TranslateToNewPosition(new Vector3(adress.x, adress.y))); //Start move tile to new position
    }

    IEnumerator TranslateToNewPosition(Vector3 newPosition)
    {
        while (transform.localPosition != newPosition)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, newPosition, 0.5f);
            yield return new WaitForSeconds(0.03f);
        }
    }

    public void ResetColor(TileColor color)
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        tileColor = color;
        switch (color)
        {
            case TileColor.Cyan:
                spriteRenderer.sprite = spriteCyan;
                break;
            case TileColor.Magenta:
                spriteRenderer.sprite = spriteMagenta;
                break;
            case TileColor.Yellow:
                spriteRenderer.sprite = spriteYellow;
                break;
            case TileColor.Blue:
                spriteRenderer.sprite = spriteBlue;
                break;            
            case TileColor.Orange:            
                spriteRenderer.sprite = spriteOrange;
                break;

            default:
                Debug.LogWarning(gameObject.name + " - invalid tile type! Set to default");
                tileColor = TileColor.Cyan;
                spriteRenderer.sprite = spriteCyan;
                break;
        }        
    }

    public void Burst()
    {
        TilesStash.AddTile(this);
    }

    private void OnMouseDown() => FieldController.instance.ClickOn(this);
}