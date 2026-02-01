public partial class ActionManager
{
    public class UnselectedState : StateMachine
    {
        public UnselectedState(ActionManager actionMgr) : base(actionMgr)
        {

        }

        public override StateMachine Update(GridIndex? click)
        {
            var piece = ActionMgr.board[click];
            if (piece?.Team == ActionMgr.actionTeam) {
                return new SelectedState(ActionMgr, click.Value);
            }
            else {
                return this;
            }
        }
    }
}