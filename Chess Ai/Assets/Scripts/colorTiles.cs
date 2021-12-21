using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colorTiles : MonoBehaviour
{
    public Color colorSelected; //the color that is displayed when selected
    public Color colorValid;    //color that is displayed that is valid to place
    
    [HideInInspector]
    //original color of tile
    private Color originalColor;

    
    public int position;    //position in a 64 member array
    private SpriteRenderer spriteRenderer;

    private createPieces createPieces;

    // Start is called before the first frame update
    void Start()
    {
        createPieces = GameObject.Find("PieceHandler").GetComponent<createPieces>();
        originalColor = this.GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        updateSelectColor();
    }

    void updateSelectColor()
    {
        if (createPieces.mouseHolding == position && createPieces.mouseIsHolding)
        {
            this.GetComponent<SpriteRenderer>().color = colorSelected;
        }
        else
        {
            this.GetComponent<SpriteRenderer>().color = originalColor;
        }
    }
}
