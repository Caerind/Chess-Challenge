using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    bool started = false;
    float totalTime;
    int phase = 0;
    int movesMade = 0;
    int[] openingMovesHash = { 14027, 13962 };
    System.Random rng = new Random();

    public Move Think(Board board, Timer timer)
    {
        if (!started)
        {
            started = true;
            totalTime = timer.MillisecondsRemaining;
        }

        int openingMove = (phase == 0 && board.IsWhiteToMove && movesMade < openingMovesHash.Length ? openingMovesHash[movesMade] : 0);

        Move[] moves = board.GetLegalMoves();
        int numMoves = moves.Length;
        if (numMoves == 1) return moves[0];

        int bestMoveScore = int.MinValue;
        List<Move> bestMoves = new List<Move>();

        movesMade++;

        foreach (Move move in moves)
        {
            if (MoveIsCheckmate(board, move) || move.GetHashCode() == openingMove)
            {
                return move;
            }

            int moveScore = EvaluateMove(board, move);
            if (moveScore == bestMoveScore)
            {
                bestMoves.Add(move);
            }
            else if (moveScore > bestMoveScore)
            {
                bestMoves.Clear();
                bestMoves.Add(move);
                bestMoveScore = moveScore;
            }
        }

        if (bestMoves.Count > 0)
        {
            return bestMoves[rng.Next(bestMoves.Count)];
        }
        return moves[rng.Next(numMoves)];
    }

    int EvaluateMove(Board board, Move move)
    {
        float moveScore = 0;

        int[] pieceCaptureValues = { 0, 10, 30, 30, 50, 90, 10000 };

        Piece piece = board.GetPiece(move.StartSquare);
        Piece capturedPiece = board.GetPiece(move.TargetSquare);
        PieceType pieceType = piece.PieceType;

        // Provoke check
        if (MoveIsCheck(board, move)) moveScore += 15;

        // Capturing opponent's pieces
        moveScore += pieceCaptureValues[(int)capturedPiece.PieceType];

        // Promoting pawns
        moveScore += pieceCaptureValues[(int)move.PromotionPieceType];

        // Anti suicide
        if (board.SquareIsAttackedByOpponent(move.TargetSquare)) moveScore -= 5 * pieceCaptureValues[(int)piece.PieceType]; // avoid going where you can be attacked

        return (int)moveScore;
    }
    
    bool MoveIsCheck(Board board, Move move)
    {
        board.MakeMove(move);
        bool isCheck = board.IsInCheck();
        board.UndoMove(move);
        return isCheck;
    }
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }
    bool MoveIsDraw(Board board, Move move)
    {
        board.MakeMove(move);
        bool isDraw = board.IsDraw();
        board.UndoMove(move);
        return isDraw;
    }

    /*
    private void PrecomputeOpeningMovesHash(Board board, string[] moves)
    {
        Console.WriteLine("Opening moves:");
        foreach (string m in moves)
        {
            Console.WriteLine(new Move(m, board).RawValue);
        }
    }
    */
}