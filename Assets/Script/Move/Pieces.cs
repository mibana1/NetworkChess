using UnityEngine;

public abstract class Pieces : ActorComponent
{
    /// <summary>
    /// 말 이동에 대한 기본 액션데이터
    /// Undo, Redo 를 위해 이동 전, 후 위치와 캡처된 말을 저장한다.
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
    public int PieceId { get; private set; }

    public void SetPieceId(int id)
    {
        PieceId = id;
    }

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
    /// 턴 종료 시 호출
    /// </summary>
    public virtual void Turnover() { }

    public abstract MoveSequence QueryMovable(MoveType type);

    public ChessTeam Team => team;
    public GridIndex CellIndex => cellIndex;
}
