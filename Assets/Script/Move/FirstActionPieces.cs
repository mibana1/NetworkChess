public abstract class FirstActionPieces : Pieces
{
    protected new class MyActionData : Pieces.MyActionData
    {
        bool bFirstAction;

        public MyActionData(ActionData data, bool bFirstAction) : base(data)
        {
            this.bFirstAction = bFirstAction;
        }

        public MyActionData(ActionData data) : base(data)
        {
            if (data is MyActionData my)
            {
                bFirstAction = my.bFirstAction;
            }
        }

        public override void Undo()
        {
            base.Undo();
            if (Owner is FirstActionPieces actor)
            {
                actor.bFirstAction = bFirstAction;
            }
        }
    }

    bool bFirstAction = false;

    void Awake()
    {
        bFirstAction = true;
    }

    public override ActionData MoveTo(GridIndex gridIndex)
    {
        bool bRest = bFirstAction;
        var data = base.MoveTo(gridIndex);
        bFirstAction = false;

        return new MyActionData(data, bRest);
    }

    public bool IsFirstAction => bFirstAction;
}
