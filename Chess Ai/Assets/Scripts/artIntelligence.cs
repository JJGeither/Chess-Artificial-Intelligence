using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class artIntelligence : MonoBehaviour
{

    public int color = 0;
    private List<int> teamPieces;
    private createPieces createPieces;
    private movePieces movePieces;

    // Start is called before the first frame update
    void Start()
    {
        createPieces = GameObject.Find("PieceHandler").GetComponent<createPieces>();
        movePieces = GameObject.Find("ChessPiece").GetComponent<movePieces>();
    }

    // Update is called once per frame
    void Update()
    {
        if (createPieces.getTurn() == color)
        {
            teamPieces = createPieces.getPieces(color);
            moveRandom();
        }
            
    }

    public void moveRandom()    //moves a random piece to a random location
    {
        bool canMove = false;   //used to determine if a piece can find a movement
        while(!canMove) //repeats until it finds a piece that can move
        {
            System.Random random = new System.Random();
            int randomPiece = random.Next(0, teamPieces.Count);   //chooses a random number from 0 to the amount of pieces
            createPieces.evaluateCheckMoves(createPieces.getCheckStatus(), teamPieces[randomPiece]);  //evaluates all the pieces that a defending piece can make to prevent checkmate
            List<int> movements = createPieces.chessCoordinates[teamPieces[randomPiece]].getValidMovementList();
            int randomMove = random.Next(0, movements.Count);   //selects a random movement from the piece

            if (movements.Count != 0)   //if the random piece can actually move
            {
                bool temp = createPieces.mouseIsPlaced; //stores statae of mouse is placed temporarily
                movePieces.move(teamPieces[randomPiece], movements[randomMove]);    //moves a random piece to one of it's movements
                createPieces.mouseIsPlaced = temp; //prevents piece from getting deleted randomly, dont ask me why
                createPieces.nullifyValidity();
                canMove = true;
            }
        }
    }
    
}
