using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createTile : MonoBehaviour
{
    public GameObject tilePrefab;
    public Color colorLight;
    public Color colorDark;
    public Color colorSelected;
    public int Scale;
    int boardPosition;
    

    // Start is called before the first frame update
    void Start()
    {
        createBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void createBoard()
    {
        bool isLightTile;
        boardPosition = 0;
        for (int column = 0; column < 8; column++)   
        {
            for (int row = 7; row >= 0; row--)
            {
                isLightTile = (column + row) % 2 != 0;
                var tileColor = (isLightTile) ? colorLight : colorDark;

                Vector2 position = new Vector2(row * Scale, column * Scale);
                drawTile(position,tileColor);
                boardPosition++;        //used to convert to a 1D position instead of a 2D position
            }
        }
    }
    public void drawTile(Vector2 position, Color tileColor)
    {
        GameObject newTile = Instantiate(tilePrefab, position , Quaternion.Euler(0f, 0f, 0f));
        //sets the position of the tiles (64)
        newTile.GetComponent<colorTiles>().position = boardPosition;
        newTile.transform.parent = GameObject.Find("Square").transform;
        newTile.transform.localScale = new Vector3(Scale, Scale, 0);
        Renderer rend = newTile.GetComponent<Renderer>();
        rend.material.color = tileColor;
    }


}
