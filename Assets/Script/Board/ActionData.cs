public abstract class ActionData
{
    // 이전 상태로 되돌린다
    public abstract void Undo();
    // 상태를 다시 적용한다
    public abstract void Redo();

    // 적용 시킬 기물
    public abstract Pieces Owner { get; }
    // 이동 전 위치
    public abstract GridIndex From { get; }
    // 이동 후 위치
    public abstract GridIndex To { get; }
    // 캡처된 기물
    public abstract Pieces KillActor { get; }
}