using UnityEngine;

/// <summary>
/// 체스 말의 공통 베이스 클래스.
/// - 보드 상의 위치 관리
/// - 이동 처리
/// - 캡처 처리
/// - Undo / Redo 를 위한 ActionData 생성
/// </summary>
public abstract class Pieces : ActorComponent
{
    /// <summary>
    /// 말 이동에 대한 기본 ActionData.
    /// Undo / Redo 를 위해 이동 전/후 위치와 캡처된 말을 저장한다.
    /// </summary>
    protected class MyActionData : ActionData
    {
        Pieces actor;
        GridIndex from;
        GridIndex to;
        Pieces killActor;

        public MyActionData(Pieces actor, GridIndex from, GridIndex to, Pieces killActor)
        {
            this.actor = actor;
            this.from = from;
            this.to = to;
            this.killActor = killActor;
        }

        /// <summary>
        /// ActionData 복제용 생성자.
        /// FirstActionPieces / Pawn 등에서 확장 ActionData를 만들 때 사용됨.
        /// </summary>
        public MyActionData(ActionData data)
        {
            if (data is MyActionData my)
            {
                actor = my.actor;
                from = my.from;
                to = my.to;
                killActor = my.killActor;
            }
        }

        public override void Undo()
        {
            actor.MoveTo(from);
            actor.board[to] = killActor;
            killActor?.gameObject.SetActive(true);
        }

        public override void Redo()
        {
            actor.MoveTo(to);
        }

        public override Pieces Owner => actor;
        public override GridIndex From => from;
        public override GridIndex To => to;
        public override Pieces KillActor => killActor;
    }

    ChessTeam team;
    GridIndex cellIndex;
    protected ChessBoard board;

    protected void Construct(ChessTeam team, GridIndex startIndex, ChessBoard board)
    {
        base.Construct();

        this.team = team;
        this.board = board;
        this.cellIndex = startIndex;

        transform.localPosition = board.QueryLocation(startIndex);
    }

    /// <summary>
    /// 말을 특정 좌표로 이동시킨다.
    /// </summary>
    public virtual ActionData MoveTo(GridIndex targetIndex)
    {
        var from = cellIndex;
        cellIndex = targetIndex;

        transform.localPosition = board.QueryLocation(targetIndex);

        var captured = board[targetIndex];
        board[from] = null;
        board[targetIndex] = this;

        captured?.gameObject.SetActive(false);

        return new MyActionData(this, from, targetIndex, captured);
    }

    /// <summary>
    /// 턴 종료 시 호출 (Pawn enpassant 등)
    /// </summary>
    public virtual void Turnover() { }

    public abstract MoveSequence QueryMovable(MoveType type);

    public ChessTeam Team => team;
    public GridIndex CellIndex => cellIndex;
}
