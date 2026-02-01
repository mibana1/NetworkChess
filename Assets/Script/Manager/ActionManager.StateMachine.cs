public partial class ActionManager
{
    public abstract class StateMachine
    {
        ActionManager actionMgr;

        public StateMachine(ActionManager actionMgr)
        {
            this.actionMgr = actionMgr;
        }

        public abstract StateMachine Update(GridIndex? click);

        public ActionManager ActionMgr => actionMgr;
    }
}