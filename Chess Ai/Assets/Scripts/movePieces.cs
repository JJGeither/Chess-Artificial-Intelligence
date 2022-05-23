using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePieces : MonoBehaviour
{
    public int position;
    private SpriteRenderer spriteRenderer;
    Vector3 originalPos;
    bool isFollowMouse = false;

    bool wait = false;
    public bool isCheckStatus;

    private createPieces createPieces;
    private userInterface userInterface;

    //follower object
    public GameObject piecePrefab;
    public GameObject newPiece;
   

    // Start is called before the first frame update
    void Start()
    {
        createPieces = GameObject.Find("PieceHandler").GetComponent<createPieces>();
        userInterface = GameObject.Find("PieceHandler").GetComponent<userInterface>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        waitForPromotionSelection();

        if (isFollowMouse)
        {
            followMouse();
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("Check Status: " + createPieces.getCheckStatus());
        Debug.Log("Turn: " + createPieces.getTurn());
        int selectedTurn = createPieces.chessCoordinates[position].getColor();
        if (selectedTurn == createPieces.getTurn() || createPieces.getTurn() == -1)    //if turn is -1, able to move any piece however
        {
            //if the mouse is not holding anything and not selecting an empty space
            if (!createPieces.mouseIsHolding && !createPieces.getCoordinateEmpty(position))
            {
                //-1 = no check
                //0 = check
                //1 = checkmate

                createPieces.evaluateCheckMoves(createPieces.getCheckStatus(), position);  //evaluates all the pieces that a defending piece can make to prevent checkmate

                //sets the original position to reference later
                originalPos = this.transform.position;

                //creates follower object
                newPiece = Instantiate(piecePrefab, originalPos, Quaternion.Euler(0f, 0f, 0f));

                //makes it so that this object will follow the mouse position until it is moved
                isFollowMouse = true;

                //sets sprite to empty
                this.GetComponent<SpriteRenderer>().sprite = null;

                return;

            }
        }

            //replaces the pieces only when the mouse is holding unto something
            if (createPieces.mouseIsHolding)
            {
                if (createPieces.mouseHolding == position)  //cancels if click on original tile
                {
                    cancelMove();
                    return;
                }
                else if (createPieces.chessCoordinates[position].isValidMovement) //moves piece
                {
                    removeEnPassantPiece();
                    createPieces.replacePiece(position, createPieces.mouseHolding);
                    reducePawnRange();
                    if (createPieces.chessCoordinates[position].isPawnAtEnd())
                    {
                        //knight = 2
                        //bishop = 3
                        //rook = 4
                        //queen = 6
                        createPieces.chessCoordinates[position].isValidMovement = false;
                        userInterface.drawPawnPromotion();
                        wait = true;    //used to wait to select promotion for pawn 
                    }
                    swapTurns();    //switches whos turn it is
                    updateKingPos();
                    createPieces.chessCoordinates[position].setMoved(true);     //sets the piece to of moved
                    createPieces.setCheckStatus(createPieces.evaluateCheckmate());
                    createPieces.nullifyValidity();
                    Debug.Log("Check Status: " + createPieces.getCheckStatus());
                    return;
                }
            }  
    }

    public void move(int FromPos, int ToPos)    //moves FromPos to ToPos
    {
        removeEnPassantPiece(); //used for special movements
        createPieces.replacePiece(position, createPieces.mouseHolding); //moves first element to the second element
        reducePawnRange();
        if (createPieces.chessCoordinates[position].isPawnAtEnd())  //determines if a pawn is at end and allows to promote it
        {
            //knight = 2
            //bishop = 3
            //rook = 4
            //queen = 6
            createPieces.chessCoordinates[position].isValidMovement = false;
            userInterface.drawPawnPromotion();
            wait = true;    //used to wait to select promotion for pawn 
        }
        swapTurns();    //switches whos turn it is
        updateKingPos();    //self explanitory
        createPieces.chessCoordinates[position].setMoved(true);     //sets the piece to of moved
        createPieces.setCheckStatus(createPieces.evaluateCheckmate());     //sets a value depending on whether or not pieces or in check
        createPieces.nullifyValidity(); //sets all spaces to invalid
        Debug.Log("Check Status: " + createPieces.getCheckStatus());
        return;
    }

    

    void waitForPromotionSelection()
    {
        if (wait)
        {
            userInterface.GetComponent<userInterface>().pos = position;
            int type = userInterface.GetComponent<userInterface>().type;
            if (type != 0)
            {
                setPieceTo(type);
                createPieces.setCheckStatus(createPieces.evaluateCheckmate());
                createPieces.nullifyValidity();
                createPieces.evaluateCheckMoves(createPieces.getCheckStatus(), position);  //evaluates all the pieces that a defending piece can make to prevent checkmate
            }
        }
    }

    void setPieceTo(int pieceType)
    {
        int color = createPieces.chessCoordinates[position].getColor();
        bool isChangePiece = false;
        switch (pieceType)
        {
            case 2: //knight
                createPieces.chessCoordinates[position] = new createPieces.Knight(position, color);
                isChangePiece = true;
                break;
            case 3: //bishop
                createPieces.chessCoordinates[position] = new createPieces.Bishop(position, color);
                isChangePiece = true;
                break;
            case 4: //rook
                createPieces.chessCoordinates[position] = new createPieces.Rook(position, color);
                isChangePiece = true;
                break;
            case 6: //queen
                createPieces.chessCoordinates[position] = new createPieces.Queen(position, color);
                isChangePiece = true;
                break;
            default:
                break;
        }

        if (isChangePiece)
        {
            this.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[createPieces.chessCoordinates[position].getSprite()];
            wait = false;
            userInterface.GetComponent<userInterface>().type = 0;
        }
    }

    void removeEnPassantPiece() //used for special movements 
    {
        int type = createPieces.chessCoordinates[position].getType();
        createPieces.chessPieceClass piece = createPieces.chessCoordinates[position];
        if (piece.isSpecialMovement() && piece.getSpecialMovementType() == 1)   //used for en passant
        {
            emptyPiece(createPieces.chessCoordinates[position].getSpecialMovementPos());    //will not have a destination
            createPieces.chessCoordinates[position].setSpecialMovement(false);
           // createPieces.chessCoordinates[position].setMoveTwoLastTurn(true);
        } else if (createPieces.chessCoordinates[position].isSpecialMovement() && piece.getSpecialMovementType() == 5) //used for castling
        {
            createPieces.replacePiece(createPieces.chessCoordinates[position].getSpecialMovementDestinationPos(), createPieces.chessCoordinates[position].getSpecialMovementPos());  //will have a destination
            createPieces.chessCoordinates[position].setSpecialMovement(false);
        }
    }


    void followMouse()
    {

        //removes collider to click on other pieces

        //follows mouse pos by creating clone
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = Camera.main.transform.position.z + Camera.main.nearClipPlane;
        newPiece.GetComponent<BoxCollider2D>().enabled = false;
        newPiece.transform.position = mousePosition;

        //sets mouse object to holding an opbject
        createPieces.mouseHolding = position;
        createPieces.mouseIsHolding = true;

        if (createPieces.mouseIsPlaced)
        {
            emptyPiece(position);
            endMovement();
        }
    }

    

    void emptyPiece(int pos)    //sets piece at position to new empty
    {
        createPieces.chessCoordinates[pos] = new createPieces.Empty(pos);
        GameObject obj = createPieces.pieceObjects[pos];
        obj.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[0];
        
    }

    void cancelMove()   //cancels the move when selecting original square
    {
        int holdingSprite = createPieces.getCoordinateSprite(createPieces.mouseHolding);
        this.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[holdingSprite];
        endMovement();
    }

    void reducePawnRange()  //reduces the pawn range from 2 to 1 once moved from it's original spot
    {
        endPassant();
        if (createPieces.chessCoordinates[position].getType() == 1)
        {
            createPieces.chessCoordinates[position].setRange(1);
            //WORK ON THISSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS
            if (createPieces.rowDifference(position, createPieces.mouseHolding) == 2)
                createPieces.chessCoordinates[position].setMoveTwoLastTurn(true);
        }
    }

    void endPassant()   //sets all en passant capability to false after turn ends
    {
        //makes sure that en passant can only be used right after pawn moves
        for (int i = 0; i <= 63; i++)
        {
            createPieces.chessCoordinates[i].setMoveTwoLastTurn(false);
        }
    }

    void endMovement()  //the process that is needed to end a turn
    {
        //turns validity off
        createPieces.nullifyValidity();
        Destroy(newPiece.gameObject);
        this.transform.position = originalPos;          //transforms the piece back
        this.GetComponent<BoxCollider2D>().enabled = true;
        isFollowMouse = false;
        createPieces.mouseIsHolding = false;
        createPieces.mouseIsPlaced = false;
    }

    void updateKingPos()
    {
        if (createPieces.chessCoordinates[position].getType() == 5) //updates kings position
        {
            createPieces.kingPos[createPieces.chessCoordinates[position].getColor()] = position;
        }
    }

    void swapTurns()
    {
        if (createPieces.getTurn() == 0)
        {
            createPieces.setTurn(1);
        }   
        else if (createPieces.getTurn() == 1)
        {
            createPieces.setTurn(0);    
        }     
    }
}