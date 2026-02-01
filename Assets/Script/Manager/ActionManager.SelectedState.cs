public partial class ActionManager
{
    public class SelectedState : StateMachine
    {
        public SelectedState(ActionManager actionMgr, GridIndex selected) : base(actionMgr)
        {
            actionMgr.selection = selected;
        }

        public override StateMachine Update(GridIndex? click)
        {
            var selectionPiece = ActionMgr.SelectionPiece;
            ActionMgr.selection = null;

            if (click.HasValue) {
                var target = ActionMgr.board[click.Value];
                if (target != null) {
                    ActionMgr.AddAttack(selectionPiece, target);
                }
                else {
                    ActionMgr.AddMove(selectionPiece, click.Value);
                }
            }

            return new UnselectedState(ActionMgr);
        }
    }
}