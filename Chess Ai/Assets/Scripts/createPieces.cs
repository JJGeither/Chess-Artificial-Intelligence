using System.Collections;
using System.Collections.Generic;
using static Unity.Mathematics.math;
using UnityEngine;





public class createPieces : MonoBehaviour
{
    private userInterface userInterface;
    private int playerTurn = 1;
    public string FENString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public GameObject piecePrefab;
    public chessPieceClass[] chessCoordinates = new chessPieceClass[64];
    public GameObject[] pieceObjects = new GameObject[64];  //used for directly referencing object
    public int Scale;
    private int checkStatus = -1; //check status, starts at no check

    //returns the player turn
    public int getTurn()
    {
        return playerTurn;
    }

    public void setCheckStatus(int newCheckStatus)
    {
        checkStatus = newCheckStatus;
    }

    public int getCheckStatus()
    {
        return checkStatus;
    }

    //sets the player turn
    public void setTurn(int num)
    {
        playerTurn = num;
    }

    public int[] kingPos = new int[2];  //stores the positions of both kings on the board 

    public int[] isCheckStatus = new int[2];  //stores the check and checkmate status, true if in check/mate and false if not
    //1 = check
    //2 = checkmate

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
        userInterface = GameObject.Find("PieceHandler").GetComponent<userInterface>();
        setUpPieces(FENString);
        numToEdge();
        drawPieces();

        setCheckStatus(evaluateCheckmate());    //determines if starting setup results in check or not
        nullifyValidity();
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
                        kingPos[PieceColor] = position;
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

    public void evaluateCheckMoves(int checkStatus, int position)   //checkstatus is the state of check, position is the piece you want to check 
    {
        
        if (checkStatus == 0)   //if in check
        {
            //-1 = no check
            //0 = check
            //1 = checkmate
            List<int> checkmateFreeMoves = new List<int>(63); //list of all moves that will not lead to checkmate
            chessCoordinates[position].clearValidMovementList();
            nullifyValidity();
            checkValidity(position);
            List<int> valid = chessCoordinates[position].getValidMovementList();
            int movementCheckValue;
            for (int i = 0; i < valid.Count; i++)
            {
                var validMovement = valid[i];
                movementCheckValue = canEscapeCheckmate(position, validMovement); //moves a piece to one of it's movement options 
                nullifyValidity();
                checkValidity(position);
                if (movementCheckValue == -1)    //if the defending team can prevent checkmate returns -1 else returns the type of piece
                {
                    checkmateFreeMoves.Add(validMovement);
                }
            }
            nullifyValidity();
            setValidMovementWithList(position, checkmateFreeMoves);
            displayList(chessCoordinates[position].getValidMovementList());
            return;
        }
        //else if not in check
        checkValidity(position);
        canMoveWithoutCheck(position);
        return;

    }

    public void checkValidity(int pos)  //determines all the movements that any piece can make
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

        if (isCheckStatus[pieceColor] == 0) //can only do if not in check
            kingCastling(pos);

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
                        addValidMovement(pos, checkPos);
                    }
                    else if (chessCoordinates[checkPos].getColor() == pieceColor || chessCoordinates[pos].getType() == 1) //will stop prematurly if encounters piece of same color or if its a pawn piece
                    {
                        //makes tile invalid and stops while method if meets same color piece
                        chessCoordinates[checkPos].isValidMovement = false;
                        chessCoordinates[pos].removeValidMovementList(checkPos);
                        break;
                    }
                    else
                    {
                        //makes tile valid and stops while method if meets a different color piece
                        addValidMovement(pos, checkPos);
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

    public void canMoveWithoutCheck(int pos)    //prevents a piece from moving out of the way causing a check to happen
    {
         
        List<int> movements = chessCoordinates[pos].getValidMovementList(); //copy of all moveable moves for a piece
        List<int> list = movements; //copy of the movements, removes as neccesary

        int kingInitPos = kingPos[chessCoordinates[pos].getColor()];  //sets the initial king position to store when moving the king
        int color = chessCoordinates[pos].getColor(); //gets the color of the defending team
        int[] oppColor = { 1, 0 };  //color of the attacking team
        int checkValue = -1;    //used to determine check
        

        for (int i = 0; i < movements.Count; i++)  //sees if can move piece without causing check
        {
            chessPieceClass[] temp2 = { chessCoordinates[pos], chessCoordinates[movements[i]] }; //stores the initla positions of the pieces
            replacePieceTemp(movements[i], pos);    //moves pos to movements
            
            for (int atkPos = 0; atkPos <= 63; atkPos++)   //checks all the responses of the attacking team
            {
                if (chessCoordinates[atkPos].getColor() == oppColor[color])
                {
                    checkValue = 123;//evaluateCheckAtPos(atkPos);
                    if (checkValue == color)  //if the team can checkmate, can't do this move and returns piece value
                    {
                        list.Remove(movements[i]);  //removes from valid list
                        break;
                    }
                        
                }
            }

            //revert change that was made by replace piece
            chessCoordinates[pos] = temp2[0];
            chessCoordinates[movements[i]] = temp2[1];
            pieceObjects[pos].GetComponent<SpriteRenderer>().sprite = pieceSheet[temp2[0].getSprite()];
            pieceObjects[movements[i]].GetComponent<SpriteRenderer>().sprite = pieceSheet[temp2[1].getSprite()];
            kingPos[color] = kingInitPos;   //moves the king back to it's initial position
           
        }

        return;

    }

    public void nullifyValidity()   //turns all valid tiles into invalid, typically at end of turn
    {
        for (int i = 0; i <= 63; i++)
        {
            chessCoordinates[i].isValidMovement = false;
            chessCoordinates[i].clearValidMovementList();
        }
    }

   

    public int evaluateCheckmate() //checks all possible movement options that a defending team has to prevent checkmate
    {

        //first determines CHECK
        int checkValue;
        int checkColor = -1;

       // int noCheck = 0;
        int check = 1;
        int checkmate = 2;

        int[] oppColor = { 1, 0 };

        int noCheckPiecePos = 0;   //used for setting check value
        nullifyValidity(); 

        for (int i = 0; i <= 63; i++)   //cycles through each piece and evaluates check
        {
            checkValue = evaluateCheckAtPos(i);
            if (checkValue != -1)   //if there are any pieces that can check the king
            {
                noCheckPiecePos = i;
                checkColor = checkValue;
                break;
            } 
        }



        if (checkColor == -1)   //if no check was found ends the check
        {
            Debug.Log("No Check");
            //isCheckStatus[chessCoordinates[noCheckPiecePos].getColor()] = noCheck;
            //isCheckStatus[checkColor] = noCheck;
            nullifyValidity();
            return -1;
        } else
        {
            isCheckStatus[checkColor] = check;    //sets colors CHECK to true
            //Debug.Log("Stat: " + checkColor + " " + isCheckStatus[checkColor]);
        }

        //now determines if there is a CHECKMATE
        nullifyValidity();
        int pieceCheckValue;
        int movementCheckValue;      //value for INDIVIDUAL movements whether they can check 
        int completeCheckValue = 0;    //check status is used in determining if Checkmate is happening or not.
        int test1 = 0, test2 = 0;
        for (int defPos = 0; defPos <= 63; defPos++)   //if a check is found it then cycles through all pieces of the defending color
        {
            pieceCheckValue = 0;
            if (!chessCoordinates[defPos].isEmpty() && chessCoordinates[defPos].getColor() == checkColor) //checks all move the defending color can do to respond
            {
                chessCoordinates[defPos].clearValidMovementList(); ; //clears all previous valid movements
                checkValidity(defPos);   //re-evaluates what movements the piece can make
                List<int> list = new List<int>(chessCoordinates[defPos].getValidMovementList()); //creates a list of all the possible movmenet options a piece can make
                List<int> checkMovements = new List<int>(); //has a list of all the movements that the defending team can make in order to get out of check
                
                foreach (int validMovement in list) //cycles through all movements each defending piece can make
                {
                    movementCheckValue = canEscapeCheckmate(defPos, validMovement); //moves a piece to one of it's movement options 
                    if (movementCheckValue == -1)    //if the defending team can prevent checkmate returns -1 else returns the type of piece
                    {
                        test1++;
                        checkMovements.Add(validMovement);
                        //Debug.Log("checkMove: " + defPos + " " + checkMovements[0]);
                        pieceCheckValue = movementCheckValue; //only swaps check status if at least one movement can check
                    }
                }

                if (pieceCheckValue == -1) //check status is used in determining if Checkmate is happening or not.
                    completeCheckValue = -1;
                else
                    test2++;

                checkMovements.Clear();
                list.Clear();
            }
        }
        Debug.Log("number check:" + test1);
        Debug.Log("number cleared:" + test2);
        if (completeCheckValue != -1) //if a checkmate is found
        {
            Debug.Log("Checkmate");
            isCheckStatus[checkColor] = checkmate;
            //Debug.Log("Stat: " + checkColor + " " + isCheckStatus[checkColor]);
            //nullifyValidity();
            userInterface.playerWins(oppColor[checkColor]);
            return 1;
        }
        else //if a checkmate is NOT found (checkreturnvalue == -1)
        {
            Debug.Log("No Checkmate");  //if a check is found with no checkmate
            //Debug.Log("Stat: " + checkColor + " " + isCheckStatus[checkColor]);
            //nullifyValidity();
            return 0;
        }

    }

     public int evaluateCheckAtPos(int pos) //checks if piece at pos can check the king
    {
        int[] oppColor = { 1, 0 };
        int color;

        if (!chessCoordinates[pos].isEmpty())   //checks all valid movements that a piece can make
        {
            checkValidity(pos);
            color = chessCoordinates[pos].getColor();
        }
        else
            return -1;  //if empty, returns false
        if (chessCoordinates[kingPos[oppColor[color]]].isValidMovement)     //checks if the king is one of the valid movements a piece can make
        {
            return oppColor[color]; //returns the color of the king that is in check
        }
        return -1;
    }

    public int canEscapeCheckmate(int i, int k) //moves piece i to valid movement k
    {
        int kingInitPos = kingPos[chessCoordinates[i].getColor()];  //sets the initial king position to store when moving the king
        int color = chessCoordinates[i].getColor(); //gets the color of the defending team
        int[] oppColor = { 1, 0 };  //color of the attacking team
        int checkValue = -1;
        chessPieceClass[] temp2 = { chessCoordinates[i], chessCoordinates[k] }; //stores the initla positions of the pieces
        replacePieceTemp(k, i); //moves a piece to one of it's valid movements
        nullifyValidity();  //resets valid movements

        for (int atkPos = 0; atkPos <= 63; atkPos++)   //checsk all the responses of the attacking team
        {
            if (chessCoordinates[atkPos].getColor() == oppColor[color])
            {
                checkValue = evaluateCheckAtPos(atkPos);
                if (checkValue == color)  //if the team can still checkmate, can't do this move and returns piece value
                    break;   
            }
        }
        //revert change that was made by replace piece
        chessCoordinates[i] = temp2[0];
        chessCoordinates[k] = temp2[1];
        pieceObjects[i].GetComponent<SpriteRenderer>().sprite = pieceSheet[temp2[0].getSprite()];
        pieceObjects[k].GetComponent<SpriteRenderer>().sprite = pieceSheet[temp2[1].getSprite()];
        kingPos[color] = kingInitPos;   //moves the king back to it's initial position
        return checkValue;    //returns value of checkmate
    }

    public void replacePiece(int posA, int posB)     //Sets the piece at posB to the piece at posA and empties posA, B ---> A
    {
        if (chessCoordinates[posB].getType() == 5)
            kingPos[chessCoordinates[posB].getColor()] = posA;

        int holdingSprite = getCoordinateSprite(posB);
        pieceObjects[posA].GetComponent<SpriteRenderer>().sprite = pieceSheet[holdingSprite];
        pieceObjects[posB].GetComponent<SpriteRenderer>().sprite = pieceSheet[0];
        movePiece(posA, posB);
        mouseIsPlaced = true;
    }

    public void replacePieceTemp(int posA, int posB)     //Sets the piece at posB to the piece at posA and empties posA, B ---> A, but doesn't place piece down
    {
        if (chessCoordinates[posB].getType() == 5)
            kingPos[chessCoordinates[posB].getColor()] = posA;

        int holdingSprite = getCoordinateSprite(posB);
        pieceObjects[posA].GetComponent<SpriteRenderer>().sprite = pieceSheet[holdingSprite];
        pieceObjects[posB].GetComponent<SpriteRenderer>().sprite = pieceSheet[0];
        movePiece(posA, posB);
        
    }

    //moves pieceEmpty position into pieceReplace and empties pieceEmpty afterwards
    public void movePiece(int pieceReplace, int pieceEmpty)
    {
        chessPieceClass temp = chessCoordinates[pieceEmpty];
        chessCoordinates[pieceEmpty] = new Empty(pieceEmpty);
        chessCoordinates[pieceReplace] = temp;
        chessCoordinates[pieceReplace].setPosition(pieceReplace);
    }

    public void kingCastling(int pos)   //movement for king to castle
    {

        chessPieceClass piece = chessCoordinates[pos];

        if (piece.getType() == 5 && !piece.hasMoved())
        {
            int[] adjMovement = { 2, 3 };
            if (chessCoordinates[pos].getColor() == 0)  //swaps adj movement depending on color
            {
                (adjMovement[0], adjMovement[1]) = (adjMovement[1], adjMovement[0]);
            }

            for (int j = 0; j < 2; j++)
            {
                for (int i = 1; i <= min(numSquareToEdge[pos][adjMovement[j]], 4); i++)
                {
                    int checkPos = pos + incrementAmnts[adjMovement[j]] * i;
                    int castlePos = pos + incrementAmnts[adjMovement[j]] * 2;

                    

                    if (chessCoordinates[checkPos].getType() == 4 && !chessCoordinates[checkPos].hasMoved())
                    {
                        //able to castle
                        addValidMovement(pos, castlePos);
                        chessCoordinates[castlePos].setSpeicalMovementPos(checkPos, pos + incrementAmnts[adjMovement[j]]);
                        break;
                    } else if (!chessCoordinates[checkPos].isEmpty())
                    {
                        //can't castle
                        break;
                    } 
                }

            }
            
        }
    }
    public void pawnMovement(int pos)   //the movement of the pawn
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
                        addValidMovement(pos, diagonalMovement[dir]);
                    }
                }
                    
            }

            //en passant
            int[] adjMovement = {  pos + 1 , pos - 1 }; //the pieces to the left and right
            if (chessCoordinates[pos].getColor() == 0)  //swaps adj movement depending on color
            {
                (adjMovement[0], adjMovement[1]) = (adjMovement[1], adjMovement[0]);
            }

            for (int i = 0; i < 2; i++)
            {
                if (0 <= adjMovement[i] && adjMovement[i] <= 63)
                {
                    //determines if en passsant can happen 
                    if (chessCoordinates[adjMovement[i]].getMoveTwoLastTurn() && rowDifference(adjMovement[i], pos) == 0 && chessCoordinates[pos].getColor() != chessCoordinates[adjMovement[i]].getColor())
                    {
                        addValidMovement(pos, diagonalMovement[i]);
                        chessCoordinates[diagonalMovement[i]].setSpeicalMovementPos(adjMovement[i]);
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
                    addValidMovement(pos, checkPos);
                }
                else if (chessCoordinates[pos].getColor() == chessCoordinates[checkPos].getColor()) //will stop prematurly if encounters piece of same color or if its a pawn piece
                {
                    //makes tile invalid and stops while method if meets same color piece
                    chessCoordinates[checkPos].isValidMovement = false;
                }
                else
                {
                    //makes tile valid and stops while method if meets a different color piece
                    addValidMovement(pos, checkPos);
                }
            }
        }
    }

 

    public void addValidMovement(int pos, int checkPos)
    {
        chessCoordinates[checkPos].isValidMovement = true;
        chessCoordinates[pos].addValidMovementList(checkPos);
    }

    public void setValidMovementWithList(int pos, List<int> checkMovement)
    {
        chessCoordinates[pos].clearValidMovementList();
        for (int i = 0; i < checkMovement.Count; i++)
        {
            chessCoordinates[pos].addValidMovementList(checkMovement[i]);
            chessCoordinates[checkMovement[i]].isValidMovement = true;
        }

    }

    public void displayList(List<int> checkMovement)
    {
        for (int i = 0; i < checkMovement.Count; i++)
        {
            Debug.Log("List: " + checkMovement[i]);
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

        int pieceSprite;
        bool empty = false;
        int pieceColor;
        int position;
        int pieceType;
        int[] directionsAllowed;
        int initialPos;
        int[] specialMovementPos;   //first # is used for what piece to move, second is where to move it to if not deleted instead
        bool specialMovementValid;
        int specialMovementType;
        bool moved = false;
        private List<int> validMovementsList = new List<int>();

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

        public void addValidMovementList(int pos)   //makes a piece able to move to pos tile
        {
            validMovementsList.Add(pos);
        }

        public void clearValidMovementList()
        {
            validMovementsList.Clear();
        }

        public List<int> getValidMovementList()
        {
            return validMovementsList;
        }

        public void removeValidMovementList(int pos)
        {
            validMovementsList.Remove(pos);
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

        public void setSpeicalMovementPos(int pos)
        {
            specialMovementType = 1;    //used only for pawns
            specialMovementPos = new int[1];
            specialMovementPos[0] = pos;
            specialMovementValid = true;
        }

        public void setSpeicalMovementPos(int piecePosFrom, int piecePosTo)
        {
            specialMovementType = 5;    //used only for rooks (and kings)
            specialMovementPos = new int[2];
            specialMovementPos[0] = piecePosFrom;
            specialMovementPos[1] = piecePosTo;
            specialMovementValid = true;
        }

        public int getSpecialMovementType()
        {
            return specialMovementType;
        }

        public void setSpecialMovement(bool state)
        {
            specialMovementValid = state;
        }


        public int getSpecialMovementPos()
        {
            if (specialMovementPos.Length != 0)
                return specialMovementPos[0];
            return position;
        }

        public int getSpecialMovementDestinationPos()
        {
            if (specialMovementPos.Length != 0)
                return specialMovementPos[1];
            return position;
        }

        public bool isSpecialMovement()
        {
            return specialMovementValid;
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

        public bool hasMoved()
        {
            return moved;
        }

        public void setMoved(bool state)
        {
            moved = state;
        }
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