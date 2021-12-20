using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createPieces : MonoBehaviour
{
    public string FENString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public GameObject piecePrefab;
    chessPieceClass[] chessCoordinates = new chessPieceClass[64];
    private SpriteRenderer spriteR;
    private Sprite sprites;
    public int Scale;

    private int black = 0;
    private int white = 1;

    [Header("Chess Sprites")]
    public Sprite[] pieceSheet = new Sprite[13];


    // Start is called before the first frame update
    void Start()
    {
        setUpPieces(FENString);
        drawPieces(chessCoordinates);
    }

    void drawPieces(chessPieceClass[] chessCoordinates)
    {
        int tileArrayPos = 63;
        for (int column = 7; column >= 0; column--)
        {
            for (int row = 0; row < 8; row++)
            {
                //multiply by the scale of tile, in this case 4
                Vector2 position = new Vector2(row * Scale * 4, column * Scale * 4);
                GameObject newTile = Instantiate(piecePrefab, position, Quaternion.Euler(0f, 0f, 0f));
                newTile.transform.localScale = new Vector3(Scale, Scale, 0);
                newTile.transform.parent = GameObject.Find("ChessPiece").transform;
                newTile.GetComponent<SpriteRenderer>().sprite = pieceSheet[chessCoordinates[tileArrayPos].getSprite()];
                tileArrayPos--;
            }
        }
        
    }

    void setUpPieces(string FENString)
    {
        string[] FENSubs = FENString.Split('/');
        int length;
        int position = 63;

        foreach (var sub in FENSubs)
        {
            length = sub.Length;
            for (int tilePosition = 0; tilePosition < length; tilePosition++)
            {
                char subSection = sub[tilePosition];

                //determines color
                int PieceColor = (char.IsUpper(subSection)) ? white : black;

                switch (char.ToLower(subSection))
                {
                    //rook
                    case 'r':
                        chessCoordinates[position] = new Rook(position, PieceColor);
                        position--;
                        break;
                    //knight
                    case 'n':
                        chessCoordinates[position] = new Knight(position, PieceColor);
                        position--;
                        break;
                    //pawn
                    case 'p':
                        chessCoordinates[position] = new Pawn(position, PieceColor);
                        position--;
                        break;
                    //bishop
                    case 'b':
                        chessCoordinates[position] = new Bishop(position, PieceColor);
                        position--;
                        break;
                    //queen
                    case 'q':
                        chessCoordinates[position] = new Queen(position, PieceColor);
                        position--;
                        break;
                    //King
                    case 'k':
                        chessCoordinates[position] = new King(position, PieceColor);
                        position--;
                        break;
                    //if it's a number
                    default:
                        //defaults to empty space
                        int posNum = subSection - '0';
                        for (int i = 0; i < posNum; i++)
                        {
                            chessCoordinates[position] = new Empty(position);
                            position--;
                        }
                        break;
                }
            }
        }
    }

    class chessPieceClass
    {
        public int black = 0;
        public int white = 1;

        public int pawn = 1;
        public int knight = 2;
        public int bishop = 3;
        public int rook = 4;
        public int king = 5;
        public int queen = 6;

        //deconstructor
        ~chessPieceClass()
        {

        }

        //setters
        public void setColor(int newColor)
        {
            pieceColor = newColor;
        }

        public void setPosition(int tilePosition)
        {
            position = tilePosition;
        }

        public void setEmpty(bool state)
        {
            isEmpty = state;
        }

        public virtual void setSprite(int piece, int color)
        {
            pieceSprite = piece + (color * 6);
        }

        //getters
        public virtual int getSprite()
        {
            return pieceSprite;
        }

        public int getColor()
        {
            return pieceColor;
        }

        int pieceSprite;
        bool isEmpty = false;
        int pieceColor;
        int position;
        int pieceType;

    }

    class Empty : chessPieceClass
    {
        public Empty(int tilePosition)
        {
            setEmpty(true);
            setPosition(tilePosition);
            //sets it to empty sprite
            setSprite(0,0);
        }
    }

    class Pawn : chessPieceClass
    {
        //constructor
        public Pawn(int tilePosition, int newPieceColor)
        {
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(pawn,newPieceColor);
        }
        
        
    }

    class Knight : chessPieceClass
    {
        //constructor
        public Knight(int tilePosition, int newPieceColor)
        {
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(knight, newPieceColor);
        }
    }

    class Bishop : chessPieceClass
    {
        //constructor
        public Bishop(int tilePosition, int newPieceColor)
        {
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(bishop, newPieceColor);
        }
    }

    class Rook : chessPieceClass
    {
        //constructor
        public Rook(int tilePosition, int newPieceColor)
        {
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(rook, newPieceColor);
        }
    }

    class King : chessPieceClass
    {
        //constructor
        public King(int tilePosition, int newPieceColor)
        {
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(king, newPieceColor);
        }
    }

    class Queen : chessPieceClass
    {
        //constructor
        public Queen(int tilePosition, int newPieceColor)
        {
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(queen, newPieceColor);
        }
    }
}

