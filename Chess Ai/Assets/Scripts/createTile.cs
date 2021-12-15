using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createTile : MonoBehaviour
{
    public GameObject tilePrefab;
    public Color colorLight;
    public Color colorDark;

    //variables
    bool boardDrawn = false;

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
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                isLightTile = (column + row) % 2 != 0;
                var tileColor = (isLightTile) ? colorLight : colorDark;
                
                Vector2 position = new Vector2(row, column);
                drawTile(position,tileColor);
            }
        }
    }
    void drawTile(Vector2 position, Color tileColor)
    {
        GameObject newTile = Instantiate(tilePrefab, position , Quaternion.Euler(0f, 0f, 0f));
        newTile.transform.parent = GameObject.Find("Square").transform;
        Renderer rend = newTile.GetComponent<Renderer>();
        rend.material.color = tileColor;
    }


}
