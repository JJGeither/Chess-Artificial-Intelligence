using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userInterface : MonoBehaviour
{
    public GameObject UIPrefab;

    [Header("Chess Sprites")]
    public Sprite[] pieceSheet = new Sprite[8];

    static int knight = 2;
    static int bishop = 3;
    static int rook = 4;
    static int queen = 6;
    static int[] pieceType = { knight, bishop, rook, queen };

    public int pos;
    public int type;

    GameObject[] promotionUI = new GameObject[5];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void drawPawnPromotion()
    {
        drawPromotionPieces();

        Vector2 position = new Vector2(2,0);
        GameObject newUI = Instantiate(UIPrefab, position, Quaternion.Euler(0f, 0f, 0f));
        newUI.transform.localScale = new Vector3(8, 20, 0);
        newUI.transform.localPosition = new Vector3(42, 14.5f, 0);
        newUI.GetComponent<BoxCollider2D>().enabled = false;
        promotionUI[0] = newUI;
    }

    void drawPromotionPieces()
    {
        Vector2 position;
        for (int i = 0; i < 4; i++)
        {
            position = new Vector2(41, 7 + i * 5);
            GameObject newUI = Instantiate(UIPrefab, position, Quaternion.Euler(0f, 0f, 0f));
            newUI.GetComponent<SpriteRenderer>().sprite = pieceSheet[i];
            newUI.transform.localScale = new Vector3(1, 1, 0);
            newUI.GetComponent<BoxCollider2D>().size = new Vector3(4, 4, 0);
            newUI.GetComponent<userInterfacePrefab>().prefabPieceType = pieceType[i];
            promotionUI[i + 1] = newUI;
        }
        
    }

    public void destroyPromotionUI()
    {
        for (int i = 0; i < 5; i++)
            Destroy(promotionUI[i].gameObject);
    }
}
