using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePieces : MonoBehaviour
{
    public int position;
    private SpriteRenderer spriteRenderer;
    Vector3 originalPos;
    bool isFollowMouse = false;

    private createPieces createPieces;

    //follower object
    public GameObject piecePrefab;
    public GameObject newPiece;

    // Start is called before the first frame update
    void Start()
    {
        createPieces = GameObject.Find("PieceHandler").GetComponent<createPieces>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isFollowMouse)
        {
            followMouse();
        }
    }

    private void OnMouseDown()
    {
        //if the mouse is not holding anything and not selecting an empty space
        if (!createPieces.mouseIsHolding && !createPieces.getCoordinateEmpty(position))
        {
            //sets validity
            createPieces.checkValidity(position);

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
                replacePiece(createPieces.mouseHolding);
                reducePawnRange();
                return;
            }
                
        }
    }

    void removeEnPassantPiece()
    {
        if (createPieces.chessCoordinates[position].isEnPassant())
        {
            emptyPiece(createPieces.chessCoordinates[position].getEnPassantPos());
            createPieces.chessCoordinates[position].setEnPassant(false);
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

    void replacePiece(int mouseHolding)
    {
        int holdingSprite = createPieces.getCoordinateSprite(mouseHolding);
        this.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[holdingSprite];
        createPieces.swapPieces(position, mouseHolding);
        createPieces.mouseIsPlaced = true;
    }

    void emptyPiece(int pos)
    {
        Debug.Log(pos);
        createPieces.chessCoordinates[pos] = new createPieces.Empty(pos);
        GameObject obj = createPieces.pieceObjects[pos];
        obj.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[0];
        
    }

    void cancelMove()
    {
        int holdingSprite = createPieces.getCoordinateSprite(createPieces.mouseHolding);
        this.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[holdingSprite];
        endMovement();
    }

    void reducePawnRange()
    {
        endPassant();
        if (createPieces.chessCoordinates[position].getType() == 1)
        {
            createPieces.chessCoordinates[position].setRange(1);
            createPieces.chessCoordinates[position].setMoveTwoLastTurn(true);
        }
    }

    void endPassant()
    {
        //sets all en passant capability to false after turn ends
        //makes sure that en passant can only be used right after pawn moves
        for (int i = 0; i < 63; i++)
        {
            createPieces.chessCoordinates[i].setMoveTwoLastTurn(false);
        }
    }

    void endMovement()
    {
        //turns all the valid tiles into invalid ones
        for (int i = 0; i < 63; i++)
        {
            createPieces.chessCoordinates[i].isValidMovement = false;
        }

        Destroy(newPiece.gameObject);
        this.transform.position = originalPos;          //transforms the piece back
        this.GetComponent<BoxCollider2D>().enabled = true;
        isFollowMouse = false;
        createPieces.mouseIsHolding = false;
        createPieces.mouseIsPlaced = false;

    }


}