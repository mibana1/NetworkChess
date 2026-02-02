using System;

public class Pawn : FirstActionPieces
{
    protected new class MyActionData : FirstActionPieces.MyActionData
    {
        int enpassantTarget = 0;
        ActionData promote;

        public MyActionData(ActionData data, int enpassantTarget, ActionData promote) : base(data)
        {
            this.enpassantTarget = enpassantTarget;
            this.promote = promote;
        }

        public MyActionData(ActionData data) : base(data)
        {
            if (data is MyActionData my)
            {
                enpassantTarget = my.enpassantTarget;
            }
        }

        public override void Undo()
        {
            promote?.Undo();
            base.Undo();
            if (promote?.Owner != null)
            {
                UnityEngine.Object.Destroy(promote.Owner.gameObject);
            }

            if (Owner is Pawn p)
            {
                p.enpassantTarget = enpassantTarget;
            }
        }
        public override NetAction ToNetAction()
        {
            var net = base.ToNetAction();

            if (promote != null)
            {
                net.netMoveType = NetMoveType.Promotion;
            }

            return net;
        }
    }

    int enpassantTarget = 0;

    public override void Construct(params object[] args)
    {
        base.Construct(
            (ChessTeam)args[0],
            (GridIndex)args[1],
            (ChessBoard)args[2]
        );
    }

    public override ActionData MoveTo(GridIndex gridIndex)
    {
        int rest = enpassantTarget;

        if (Math.Abs(CellIndex.Y - gridIndex.Y) >= 2)
        {
            enpassantTarget = 2;
        }

        var from = CellIndex;
        var data = base.MoveTo(gridIndex);

        ActionData promote = null;

        // 프로모션
        if (gridIndex.Y == 0 || gridIndex.Y == 7)
        {
            var piece = board.SpawnMgr.SpawnActor<Queen>(from, Team);
            promote = piece.MoveTo(gridIndex);
        }

        return new MyActionData(data, rest, promote);
    }

    public override void Turnover()
    {
        if (enpassantTarget > 0)
        {
            enpassantTarget -= 1;
        }
    }

    public override MoveSequence QueryMovable(MoveType type)
    {
        switch (type)
        {
            case MoveType.StandardMove:
                return QueryMove();
            case MoveType.Attack:
                return QueryAttack();
            default:
                throw new ArgumentException();
        }
    }

    public MoveSequence QueryEnpassant(ChessBoard board)
    {
        var seq = new MoveSequence(this, new GridIndex(0, 0), 2);

        GridIndex dir;
        switch (Team)
        {
            case ChessTeam.Black:
                dir = ChessBoard.ToWhite;
                break;
            case ChessTeam.White:
                dir = ChessBoard.ToBlack;
                break;
            default:
                throw new InvalidOperationException();
        }

        dir += CellIndex;

        var left = new GridIndex(-1, 0);
        var right = new GridIndex(1, 0);

        left += CellIndex;
        right += CellIndex;

        if (left.IsValid && board[left] is Pawn leftPawn && leftPawn.Team != Team && leftPawn.enpassantTarget != 0)
        {
            seq.AddMove(1, new GridIndex(left.X, dir.Y));
        }

        if (right.IsValid && board[right] is Pawn rightPawn && rightPawn.Team != Team && rightPawn.enpassantTarget != 0)
        {
            seq.AddMove(1, new GridIndex(right.X, dir.Y));
        }

        return seq;
    }

    // ---------------- 내부 이동 생성 ----------------

    MoveSequence QueryMove()
    {
        var seq = new MoveSequence(this, CellIndex, 1);

        GridIndex dir;
        switch (Team)
        {
            case ChessTeam.Black:
                dir = ChessBoard.ToWhite;
                break;
            case ChessTeam.White:
                dir = ChessBoard.ToBlack;
                break;
            default:
                throw new InvalidOperationException();
        }

        seq.AddMove(0, dir);

        if (IsFirstAction)
        {
            seq.AddMove(0, dir + dir);
        }

        return seq;
    }

    MoveSequence QueryAttack()
    {
        var seq = new MoveSequence(this, CellIndex, 2);

        GridIndex dir;
        switch (Team)
        {
            case ChessTeam.Black:
                dir = ChessBoard.ToWhite;
                break;
            case ChessTeam.White:
                dir = ChessBoard.ToBlack;
                break;
            default:
                throw new InvalidOperationException();
        }

        var left = dir;
        left.X = -1;

        var right = dir;
        right.X = 1;

        seq.AddMove(0, left);
        seq.AddMove(1, right);

        return seq;
    }
}
