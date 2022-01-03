using System.Collections;
using System.Collections.Generic;
using static Unity.Mathematics.math;
using UnityEngine;

public class createPieces : MonoBehaviour
{
    public string FENString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public GameObject piecePrefab;
    public chessPieceClass[] chessCoordinates = new chessPieceClass[64];
    public GameObject[] pieceObjects = new GameObject[64];  //used for directly referencing object
    public int Scale;

    public static readonly int[] incrementAmnts = {
    8,
    -8,
    1,
    -1,
    9,
    -9,
    7,
    -7
  };
    public static readonly int[][] numSquareToEdge = new int[64][];

    static void numToEdge()
    {
        for (int column = 0; column < 8; column++)
        {
            for (int row = 7; row >= 0; row--)
            {
                int edgeUp = 7 - column;
                int edgeDown = column;
                int edgeRight = 7 - row;
                int edgeLeft = row;

                int squareIndex = column * 8 + (7 - row);

                numSquareToEdge[squareIndex] = new int[] {
          edgeUp,
          edgeDown,
          edgeLeft,
          edgeRight,
          min(edgeUp, edgeLeft),
          min(edgeDown, edgeRight),
          min(edgeUp, edgeRight),
          min(edgeDown, edgeLeft)
        };
            }
        }
    }

    private int black = 0;
    private int white = 1;

    //values for when holding a piece
    public int mouseHolding = 0; //the position of the piece
    public bool mouseIsHolding = false; //is holding a piece
    public bool mouseIsPlaced = false; //is the piece being placed

    [Header("Chess Sprites")]
    public Sprite[] pieceSheet = new Sprite[13];

    // Start is called before the first frame update
    void Start()
    {
        setUpPieces(FENString);
        numToEdge();
        drawPieces();
    }

    public void drawPieces()
    {
        int tileArrayPos = 63;
        for (int column = 7; column >= 0; column--)
        {
            for (int row = 0; row < 8; row++)
            {
                //multiply by the scale of tile, in this case 4
                Vector2 position = new Vector2(row * Scale * 4, column * Scale * 4);
                GameObject newPiece = Instantiate(piecePrefab, position, Quaternion.Euler(0f, 0f, 0f));
                newPiece.transform.localScale = new Vector3(Scale, Scale, 0);
                newPiece.transform.parent = GameObject.Find("ChessPiece").transform;
                newPiece.GetComponent<SpriteRenderer>().sprite = pieceSheet[chessCoordinates[tileArrayPos].getSprite()];
                newPiece.GetComponent<movePieces>().position = tileArrayPos;
                pieceObjects[tileArrayPos] = newPiece;
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

    //moves pieceEmpty position into pieceReplace and empties pieceEmpty afterwards
    public void swapPieces(int pieceReplace, int pieceEmpty)
    {
        chessPieceClass temp = chessCoordinates[pieceEmpty];
        chessCoordinates[pieceEmpty] = new Empty(pieceEmpty);
        chessCoordinates[pieceReplace] = temp;
        chessCoordinates[pieceReplace].setPosition(pieceReplace);
    }

    //allows to get value of sprite without having direct class
    public int getCoordinateSprite(int coord)
    {
        return chessCoordinates[coord].getSprite();
    }

    //allows to determine if empty without having direct class
    public bool getCoordinateEmpty(int coord)
    {
        return chessCoordinates[coord].isEmpty();
    }

    public void checkValidity(int pos)
    {
        //0 = up
        //1 = down
        //2 = left
        //3 = right
        //4 = up/left
        //5 = down/right
        //6 = up/right
        //7 = down/left

        //gets array of movement directions and copies to new array
        int[] directionsAllowed = new int[chessCoordinates[pos].getDirections().Length];
        chessCoordinates[pos].getDirections().CopyTo(directionsAllowed, 0);
        int pieceColor = chessCoordinates[pos].getColor(); //gets the piece color
        int numDirections = directionsAllowed.Length; //the amount of directions it can move in (bishop can move in four directions)

        pawnMovement(pos); //handles the special movement of pawns

        if (chessCoordinates[pos].getType() != 2) //will have special movement if it is a knight piece
        {
            for (int direction = 0; direction < numDirections; direction++) //checks each eight directions
            {
                int checkPos = pos;
                int length = min(numSquareToEdge[pos][directionsAllowed[direction]], chessCoordinates[pos].getRange());

                for (int numToEdge = 0; numToEdge < length; numToEdge++) //checks each square advancing towards edge
                {
                    checkPos += incrementAmnts[directionsAllowed[direction]];
                    if (chessCoordinates[checkPos].isEmpty())
                    {
                        //makes tile valid
                        chessCoordinates[checkPos].isValidMovement = true;
                    }
                    else if (chessCoordinates[checkPos].getColor() == pieceColor || chessCoordinates[pos].getType() == 1) //will stop prematurly if encounters piece of same color or if its a pawn piece
                    {
                        //makes tile invalid and stops while method if meets same color piece
                        chessCoordinates[checkPos].isValidMovement = false;
                        break;
                    }
                    else
                    {
                        //makes tile valid and stops while method if meets a different color piece
                        chessCoordinates[checkPos].isValidMovement = true;
                        break;
                    }
                }
            }
        }
        else
        {
            knightMovement(pos);
        }
    }

    public void pawnMovement(int pos)
    {

        chessPieceClass piece = chessCoordinates[pos];
        
        
        //consideres movement for pawns and castling
        if (piece.getType() == 1)
        {
            //checks up if white, down if black
            int upLeft = incrementAmnts[5 - chessCoordinates[pos].getColor()];
            int upRight = incrementAmnts[7 - chessCoordinates[pos].getColor()];
            int[] diagonalMovement = { pos + upLeft, pos + upRight };

            for (int dir = 0; dir < 2; dir++)
            {
                //checks the left and right
                if (0 <= diagonalMovement[dir] && diagonalMovement[dir] <= 63)
                {
                    if (chessCoordinates[diagonalMovement[dir]].getColor() != piece.getColor() && chessCoordinates[diagonalMovement[dir]].getColor() >= 0 && rowDifference(diagonalMovement[dir], pos) == 1)
                    {
                        chessCoordinates[diagonalMovement[dir]].isValidMovement = true;
                    }
                }
                    
            }

            //en passant
            int[] adjMovement = {  pos + 1 , pos - 1 };
            if (chessCoordinates[pos].getColor() == 0)  //swaps adj movement depending on color
            {
                (adjMovement[0], adjMovement[1]) = (adjMovement[1], adjMovement[0]);
            }

            for (int i = 0; i < 2; i++)
            {
                if (0 <= adjMovement[i] && adjMovement[i] <= 63)
                {
                    if (chessCoordinates[adjMovement[i]].getMoveTwoLastTurn() && rowDifference(adjMovement[i], pos) == 0)
                    {
                        chessCoordinates[diagonalMovement[i]].isValidMovement = true;
                        chessCoordinates[diagonalMovement[i]].setEnPassantPos(adjMovement[i]);
                    }
                }
            }

            
        }
    }

    public int rowDifference(int pos1, int pos2)
    {
        return abs((pos1 / 8) - (pos2 / 8));
    }

    public int rowPosition(int pos)
    {
        return (pos / 8);
    }

    public void knightMovement(int pos)
    {
        int[] knightIncrements = { 17, 15, 10, 6, -6, -10, -15, -17 }; //the spaces from position that knight can move to
        int[] knightRowDiff = { 2, 2, 1, 1, -1, -1, -2, -2 }; //makes sure that the valid moves done overlap into other layers
        int incrementLength = knightIncrements.Length;

        for (int i = 0; i < incrementLength; i++)
        {
            int checkPos = pos + knightIncrements[i];
            if (checkPos <= 63 && checkPos >= 0 && (checkPos / 8) - (pos / 8) == knightRowDiff[i])
            {
                if (chessCoordinates[checkPos].isEmpty())
                {
                    //makes tile valid
                    chessCoordinates[checkPos].isValidMovement = true;
                }
                else if (chessCoordinates[pos].getColor() == chessCoordinates[checkPos].getColor()) //will stop prematurly if encounters piece of same color or if its a pawn piece
                {
                    //makes tile invalid and stops while method if meets same color piece
                    chessCoordinates[checkPos].isValidMovement = false;
                }
                else
                {
                    //makes tile valid and stops while method if meets a different color piece
                    chessCoordinates[checkPos].isValidMovement = true;

                }
            }
        }
    }

    public class chessPieceClass
    {

        public int black = 0;
        public int white = 1;
        public int emptyColor = -1;

        public int pawn = 1;
        public int knight = 2;
        public int bishop = 3;
        public int rook = 4;
        public int king = 5;
        public int queen = 6;

        public int up = 0;
        public int down = 1;
        public int left = 2;
        public int right = 3;
        public int upLeft = 4;
        public int downRight = 5;
        public int upRight = 6;
        public int downLeft = 7;

        public bool isValidMovement = false;

        //used for en passant
        bool moveTwoLastTurn = false;

        //used for pawn and king in order to limit their range
        public int range = int.MaxValue;

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
            empty = state;
        }

        public virtual void setSprite(int piece, int color)
        {
            pieceSprite = piece + (color * 6);
        }

        public void setInitialPos(int pos)
        {
            initialPos = pos;
        }

        public void setMoveTwoLastTurn(bool state)
        {
            moveTwoLastTurn = state;
        }

        public bool getMoveTwoLastTurn()
        {
            return moveTwoLastTurn;
        }

        public int getInitialPos()
        {
            return initialPos;
        }

        public void setRange(int newRange)
        {
            range = newRange;
        }

        public int getRange()
        {
            return range;
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

        public bool isEmpty()
        {
            return empty;
        }

        public int[] getDirections()
        {
            return directionsAllowed;
        }

        public void setDirection(int[] directions)
        {
            directionsAllowed = new int[directions.Length];
            directions.CopyTo(directionsAllowed, 0);
        }

        public void setType(int piece)
        {
            pieceType = piece;
        }

        public int getType()
        {
            return pieceType;
        }


        public void setEnPassantPos(int pos)
        {
            enPassantPos = pos;
            enPassantValid = true;
        }

        public int getEnPassantPos()
        {
            return enPassantPos;
        }

        public bool isEnPassant()
        {
            return enPassantValid;
        }

        public void setEnPassant(bool state)
        {
            enPassantValid = state;
        }

        public bool isPawnAtEnd()
        {
            if (getType() == 1)
            {
                int row = position / 8;
                if (row == 7 && getColor() == white)
                {
                    Debug.Log("White Reached End");
                    return true;
                }
                if (row == 0 && getColor() == black)
                {
                    Debug.Log("Black Reached End");
                    return true;
                }
            }
            return false;
        }

        

        int pieceSprite;
        bool empty = false;
        int pieceColor;
        int position;
        int pieceType;
        int[] directionsAllowed;
        int initialPos;
        int enPassantPos;
        bool enPassantValid;

    }

    public class Empty : chessPieceClass
    {
        public Empty(int tilePosition)
        {
            setEmpty(true);
            setPosition(tilePosition);
            //sets it to empty sprite
            setSprite(0, 0);
            setColor(emptyColor);
        }
    }

    public class Pawn : chessPieceClass
    {
        //constructor
        public Pawn(int tilePosition, int newPieceColor)
        {
            setType(pawn);
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(pawn, newPieceColor);
            
            //changes direction based on side of board (black == down, white == up)
            int relativeDir = up;
            if (newPieceColor == 0)
            {
                relativeDir = down;
            }
            int[] directions = 
            {
                relativeDir
            };
            setDirection(directions);
            setRange(2);
            setInitialPos(tilePosition);
        }
    }

    public class Knight : chessPieceClass
    {
        //constructor
        public Knight(int tilePosition, int newPieceColor)
        {
            setType(knight);
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(knight, newPieceColor);
            setInitialPos(tilePosition);
            int[] directions = {
        0
      };
            setRange(1);
            setDirection(directions);
        }
    }

    public class Bishop : chessPieceClass
    {
        //constructor
        public Bishop(int tilePosition, int newPieceColor)
        {
            setType(bishop);
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(bishop, newPieceColor);
            int[] directions = {
        upLeft,
        downRight,
        upRight,
        downLeft
      };
            setDirection(directions);
            setInitialPos(tilePosition);
        }
    }

    public class Rook : chessPieceClass
    {
        //constructor
        public Rook(int tilePosition, int newPieceColor)
        {
            setType(rook);
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(rook, newPieceColor);
            int[] directions = {
        up,
        down,
        left,
        right
      };
            setDirection(directions);
            setInitialPos(tilePosition);

        }
    }

    public class King : chessPieceClass
    {
        //constructor
        public King(int tilePosition, int newPieceColor)
        {
            setType(king);
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(king, newPieceColor);
            int[] directions = {
        upLeft,
        downRight,
        upRight,
        downLeft,
        up,
        down,
        left,
        right
      };
            setDirection(directions);
            setInitialPos(tilePosition);
            setRange(1);
        }

    }

    public class Queen : chessPieceClass
    {
        //constructor
        public Queen(int tilePosition, int newPieceColor)
        {
            setType(queen);
            setColor(newPieceColor);
            setPosition(tilePosition);
            setSprite(queen, newPieceColor);
            int[] directions = {
        upLeft,
        downRight,
        upRight,
        downLeft,
        up,
        down,
        left,
        right
      };
            setDirection(directions);
            setInitialPos(tilePosition);
        }
    }
}