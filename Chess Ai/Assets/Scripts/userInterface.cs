using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class userInterface : MonoBehaviour
{
    public GameObject UIPrefab;
    private createPieces createPieces;

    [Header("Chess Sprites")]
    public Sprite[] pieceSheet = new Sprite[8];

    static int knight = 2;
    static int bishop = 3;
    static int rook = 4;
    static int queen = 6;
    static int[] pieceType = { knight, bishop, rook, queen };

    private int playerHasWon = -1;
    public int pos;
    public int type;

    GameObject[] promotionUI = new GameObject[5];

    // Start is called before the first frame update
    void Start()
    {
        createPieces = GameObject.Find("PieceHandler").GetComponent<createPieces>();
    }

    public void setWin(int player)  //sets what player wins
    {
        playerHasWon = player;
    }

    public int getWin()  //gets what player wins
    {
        return playerHasWon;
    }

    string convertTurn(int turnNum)
    {
        string turn;
        if (turnNum == 1)
            turn = "Black";
        else if (turnNum == 0)
            turn = "White";
        else
            turn = "N/A";
        return turn;
    }
    void OnGUI()
    {

        string turn = convertTurn(createPieces.getTurn());

        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 100;



        GUI.Label(new Rect(3100, 100, 100, 100), turn + "'s Turn", guiStyle);

        if (getWin() != -1)  //if checkmate has happened
        {
            
            GUIStyle guiStyleCheckmate = new GUIStyle();
            guiStyleCheckmate.fontSize = 1000;
            guiStyleCheckmate.normal.textColor = Color.white;
            Rect position = new Rect((Screen.width) / 2 - (Screen.width) / 8, (Screen.height) / 2 - (Screen.height) / 8, (Screen.width) / 4, (Screen.height) / 4);
            GUI.Label(position, turn + " has won!", guiStyleCheckmate);
        }
    }

    // Update is called once per frame

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

    public void playerWins(int color)
    {
        setWin(color);
    }

    public void destroyPromotionUI()
    {
        for (int i = 0; i < 5; i++)
            Destroy(promotionUI[i].gameObject);
    }
}
