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
    public Vector2 adress;

    [SerializeField] public TileColor tileColor;
    [SerializeField] private Sprite spriteCyan;
    [SerializeField] private Sprite spriteMagenta;
    [SerializeField] private Sprite spriteYellow;
    [SerializeField] private Sprite spriteBlue;
    [SerializeField] private Sprite spriteOrange;

    [Space]
    [SerializeField] private ParticleSystem BurstParticle;

    public void Start()
    {
        SetColor((TileColor)Random.Range(0, 5));
    }

    public void Reset(TileColor color, Vector2 adress)
    {
        SetColor(color);
        this.adress = adress;
    }

    private void SetColor(TileColor color)
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
        Destroy(gameObject, 0.5f);
    }

    private void OnMouseDown()
    {
        FieldController.instance.ClickOn(this);
    }
}