using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;

public class artIntelligence : MonoBehaviour
{

    public int color = 0;
    public int depth = 4;
    List<int> teamPieces;
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
            Stopwatch timer = Stopwatch.StartNew();
            movePieces.move(negaMaxRoot(depth, color, double.NegativeInfinity, double.PositiveInfinity));
            timer.Stop();
            TimeSpan timespan = timer.Elapsed;

            UnityEngine.Debug.Log(timespan);
            movePieces.swapTurns();
        }

     }

    public int getDepth()
    {
        return depth;
    }

    public int getColor()
    {
        return color;
    }

    public void moveRandom()    //moves a random piece to a random location
    {
        bool canMove = false;   //used to determine if a piece can find a movement
        while (!canMove) //repeats until it finds a piece that can move
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
                movePieces.swapTurns();
                createPieces.mouseIsPlaced = temp; //prevents piece from getting deleted randomly, dont ask me why
                createPieces.nullifyValidity();
                canMove = true;
            }
        }
    }

    public double evaluate()
    {
        int[] oppColor = { 1, 0 };  //color of the attacking team
        return createPieces.getPieces(color).Count - createPieces.getPieces(oppColor[color]).Count;
    }
    movePieces.Move negaMaxRoot(int depth, int color, double alpha, double beta)
    {

        int[] oppColor = { 1, 0 };  //color of the attacking team
        if (depth == 0)
            return null;

        List<movePieces.Move> moves = movePieces.generateMoves(color);

        if (moves.Count == 0)
        {
            if (createPieces.getCheckStatus() == 1) //if in checkmate
            {
                return null;
            }
            return null;    //if in JUST check
        }

        double bestEvaluation = double.NegativeInfinity;

        //returns the best move
        orderMoves(ref moves);  //orders moves to prioritize better moves first
        movePieces.Move bestMove = moves[1];    //stores temp move
        foreach (movePieces.Move move in moves)
        {

            //trstrdt
            int kingInitPos = createPieces.kingPos[color];  //sets the initial king position to store when moving the king
            createPieces.chessPieceClass[] temp2 = { createPieces.chessCoordinates[move.getFromPos()], createPieces.chessCoordinates[move.getToPos()] }; //stores the initla positions of the pieces

            movePieces.moveTest(move);  //moves piece
            double evaluation = -negaMax(depth - 1, oppColor[color], -beta, -alpha);
            bestEvaluation = Math.Max(evaluation, bestEvaluation);

            //undo moves
            //movePieces.revertMove(move);
            createPieces.chessCoordinates[move.getFromPos()] = temp2[0];
            createPieces.chessCoordinates[move.getToPos()] = temp2[1];
            createPieces.pieceObjects[move.getFromPos()].GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[temp2[0].getSprite()];
            createPieces.pieceObjects[move.getToPos()].GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[temp2[1].getSprite()];
            createPieces.kingPos[color] = kingInitPos;   //moves the king back to it's initial position

            if (evaluation > alpha)
            {
                bestMove = move;
            }

            alpha = Math.Max(alpha, evaluation);


        }

        return bestMove;

    }

    double negaMax(int depth, int color, double alpha, double beta)
    {
        int[] oppColor = { 1, 0 };  //color of the attacking team
        if (depth == 0)
            return evaluate();

        List<movePieces.Move> moves = movePieces.generateMoves(color);
        if (moves.Count == 0)
        {
            if (createPieces.getCheckStatus() == 1) //if puts you checkmate
            {
                return double.NegativeInfinity;
            }
            return 0;   //if in stalemate
        }

        double bestEvaluation = double.NegativeInfinity;

        foreach (movePieces.Move move in moves)
        {
            //trstrdt
            int kingInitPos = createPieces.kingPos[color];  //sets the initial king position to store when moving the king
            createPieces.chessPieceClass[] temp2 = { createPieces.chessCoordinates[move.getFromPos()], createPieces.chessCoordinates[move.getToPos()] }; //stores the initla positions of the pieces

            movePieces.moveTest(move);  //moves piece
            double evaluation = -negaMax(depth - 1, oppColor[color], -beta, -alpha);
            bestEvaluation = Math.Max(evaluation, bestEvaluation);

            //undo moves
            //movePieces.revertMove(move);
            createPieces.chessCoordinates[move.getFromPos()] = temp2[0];
            createPieces.chessCoordinates[move.getToPos()] = temp2[1];
            createPieces.pieceObjects[move.getFromPos()].GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[temp2[0].getSprite()];
            createPieces.pieceObjects[move.getToPos()].GetComponent<SpriteRenderer>().sprite = createPieces.pieceSheet[temp2[1].getSprite()];
            createPieces.kingPos[color] = kingInitPos;   //moves the king back to it's initial position

            if (evaluation >= beta)
            {
                //move was too good, opponent wins
                return beta;    //snip
            }
            alpha = Math.Max(alpha, evaluation);
        }

        return alpha;

    }

    void orderMoves(ref List<movePieces.Move> moveList) //orders moves with the highest score moves going first
    {

        foreach (var move in moveList)
        {


            int score = 0;
            int moveColor = createPieces.chessCoordinates[move.getFromPos()].getColor();
            int captureColor = createPieces.chessCoordinates[move.getToPos()].getColor();

            //prioritizes if a piece of lower value captures a piece of higher value
            if (createPieces.chessCoordinates[move.getToPos()].getValue() != 0)  //if not moving to empty space
                score = 10 * createPieces.chessCoordinates[move.getToPos()].getValue() - createPieces.chessCoordinates[move.getFromPos()].getValue();

            //create one for pawn promotion

            //create one for losing a piece


            move.setScore(score);
        }

        List<movePieces.Move> test = moveList.OrderByDescending(o => o.getScore()).ToList();
        moveList = test;


    }
}