using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movePiece : MonoBehaviour
{
    public int position;
    public int pieceType;
    private SpriteRenderer spriteRenderer;
    Vector3 originalPos;

    private mouseBehavior mouseBehavior;
    private createPieces createPieces;

    

    private bool followingMouse = false;
    // Start is called before the first frame update
    void Start()
    {
        mouseBehavior = GameObject.Find("MouseObject").GetComponent<mouseBehavior>();
        createPieces = GameObject.Find("PieceHandler").GetComponent<createPieces>();
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<SpriteRenderer>().sprite = createPieces.GetComponent<createPieces>().pieceSheet[pieceType];
        if (followingMouse)
        {
            //removes collider to click on other pieces
            this.GetComponent<BoxCollider2D>().enabled = false;

            //follows mouse pos
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = Camera.main.transform.position.z + Camera.main.nearClipPlane;
            transform.position = mousePosition;
            
            mouseBehavior.GetComponent<mouseBehavior>().holding = pieceType;

            if (mouseBehavior.GetComponent<mouseBehavior>().isTarget == true)
            {
                pieceType = 0;
                followingMouse = false;
                this.transform.position = originalPos;
                this.GetComponent<BoxCollider2D>().enabled = true;
                mouseBehavior.GetComponent<mouseBehavior>().isHolding = false;
                mouseBehavior.GetComponent<mouseBehavior>().isTarget = false;
                

            }
        }
        
        
    }

    // Check for mouse input
    void OnMouseDown()
    {
        //if not holding a piece
        if (mouseBehavior.GetComponent<mouseBehavior>().isHolding == false && pieceType != 0)
        {
            originalPos = this.transform.position;
            followingMouse = true;
            mouseBehavior.GetComponent<mouseBehavior>().isHolding = true;
        //if already holding a piece, it replaces it with the held piece
        } else
        {
            pieceType = mouseBehavior.GetComponent<mouseBehavior>().holding;
            mouseBehavior.GetComponent<mouseBehavior>().holding = 0;
            mouseBehavior.GetComponent<mouseBehavior>().isTarget = true;
            
        }
    }
}
