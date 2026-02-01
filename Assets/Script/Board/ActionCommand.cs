public struct ActionCommand
{
    public ActionData Action;
    public ActionData Special;

    public static ActionCommand SimpleMove(ActionData actionData, ActionData specialData = null)
    {
        ActionCommand command;
        command.Action = actionData;
        command.Special = specialData;
        return command;
    }
}