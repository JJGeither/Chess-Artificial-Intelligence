using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePieces : MonoBehaviour
{
    public int position;
    private SpriteRenderer spriteRenderer;
    Vector3 originalPos;
    bool isFollowMouse = false;

    private mouseBehavior mouseBehavior;
    private createPieces createPieces;

    //follower object
    public GameObject piecePrefab;
    public GameObject newPiece;

    // Start is called before the first frame update
    void Start()
    {
        mouseBehavior = GameObject.Find("MouseObject").GetComponent<mouseBehavior>();
        createPieces = GameObject.Find("PieceHandler").GetComponent<createPieces>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFollowMouse)
        {
            followMouse();
        }
    }

    private void OnMouseDown()
    {
        //if the mouse is not holding anything and not selecting an empty space
        if (!mouseBehavior.isHolding && !createPieces.getCoordinateEmpty(position))
        {
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
        if (mouseBehavior.isHolding)
        {
            if (mouseBehavior.holding == position)
            {
                cancelMove();
                return;
            }
            replacePiece();
            return;
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
        mouseBehavior.holding = position;
        mouseBehavior.isHolding = true;

        if (mouseBehavior.isTarget)
        {
            voidSpace();
        }
    }

    void replacePiece()
    {
        int holdingSprite = createPieces.getCoordinateSprite(mouseBehavior.holding);
        this.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[holdingSprite];
        createPieces.swapPieces(position, mouseBehavior.holding);
        mouseBehavior.isTarget = true;
    }

    void voidSpace()
    {
        Destroy(newPiece.gameObject);
        this.transform.position = originalPos;
        this.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[0];
        this.GetComponent<BoxCollider2D>().enabled = true;
        isFollowMouse = false;
        mouseBehavior.isHolding = false;
        mouseBehavior.isTarget = false;

    }

    void cancelMove()
    {
        int holdingSprite = createPieces.getCoordinateSprite(mouseBehavior.holding);
        this.GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[holdingSprite];
        Destroy(newPiece.gameObject);
        this.transform.position = originalPos;
        this.GetComponent<BoxCollider2D>().enabled = true;
        isFollowMouse = false;
        mouseBehavior.isHolding = false;
        mouseBehavior.isTarget = false;
    }
}