public class King : FirstActionPieces
{
    public override void Construct(params object[] args)
    {
        Construct(
            (ChessTeam)args[0],
            (GridIndex)args[1],
            (ChessBoard)args[2]
        );
    }

    public override MoveSequence QueryMovable(MoveType type)
    {
        const int MoveCount = 1;

        GridIndex[] sepIndex = new GridIndex[8] {
            new GridIndex(-1,  1),
            new GridIndex( 1,  1),
            new GridIndex( 1, -1),
            new GridIndex(-1, -1),
            new GridIndex( 0,  1),
            new GridIndex( 1,  0),
            new GridIndex( 0, -1),
            new GridIndex(-1,  0)
        };

        var seq = new MoveSequence(this, CellIndex, sepIndex.Length);

        for (int i = 1; i <= MoveCount; ++i)
        {
            for (int j = 0; j < sepIndex.Length; ++j)
            {
                seq.AddMove(j, sepIndex[j] * i);
            }
        }

        return seq;
    }

    // ---------------- 캐슬링 ----------------

    public MoveSequence QueryCastling(ChessBoard board, out bool bQueenSide, out bool bKingSide)
    {
        bQueenSide = false;
        bKingSide = false;

        if (!IsFirstAction)
        {
            return new MoveSequence(this, CellIndex, 0);
        }

        var queenSide = new GridIndex(CellIndex.X - 4, CellIndex.Y);
        var kingSide = new GridIndex(CellIndex.X + 3, CellIndex.Y);
        int available = 0;

        if (board[queenSide] is Rook rook1 && rook1.IsFirstAction)
        {
            if (CheckQueenSideLine(board))
            {
                bQueenSide = true;
                available += 1;
            }
        }

        if (board[kingSide] is Rook rook2 && rook2.IsFirstAction)
        {
            if (CheckKingSideLine(board))
            {
                bKingSide = true;
                available += 1;
            }
        }

        if (!bQueenSide && !bKingSide)
        {
            return new MoveSequence(this, CellIndex, 0);
        }

        int seqIdx = 0;
        var seq = new MoveSequence(this, CellIndex, available);

        if (bQueenSide)
        {
            seq.AddMove(seqIdx++, new GridIndex(-2, 0));
        }
        if (bKingSide)
        {
            seq.AddMove(seqIdx++, new GridIndex(2, 0));
        }

        return seq;
    }

    public MoveSequence QueryCastling(ChessBoard board)
    {
        return QueryCastling(board, out var _, out var _);
    }

    bool CheckQueenSideLine(ChessBoard board)
    {
        var index = CellIndex;
        for (int i = 0; i < 3; ++i)
        {
            index.X -= 1;
            if (board[index] != null)
            {
                return false;
            }
        }

        return !board.BuildedData.IsUnderAttack(CellIndex + new GridIndex(-1, 0), Team.Invert());
    }

    bool CheckKingSideLine(ChessBoard board)
    {
        var index = CellIndex;
        for (int i = 0; i < 2; ++i)
        {
            index.X += 1;
            if (board[index] != null)
            {
                return false;
            }
        }

        return !board.BuildedData.IsUnderAttack(CellIndex + new GridIndex(1, 0), Team.Invert());
    }
}
