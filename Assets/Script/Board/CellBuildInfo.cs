public struct CellBuildInfo
{
    uint underAttack;

    public ChessTeam? Owner;

    public void Clear()
    {
        Owner = null;
        underAttack = 0;
    }

    public override string ToString() =>
        $"Owner: {Owner}, IsUnderAttack: {UnderAttackMsg}";

    public bool IsUnderAttack(ChessTeam team)
    {
        return (underAttack & GetMask(team)) != 0;
    }

    public void CanAttack(ChessTeam team, bool bCan = true)
    {
        if (bCan) {
            underAttack |= GetMask(team);
        }
        else {
            underAttack &= ~GetMask(team);
        }
    }

    uint GetMask(ChessTeam team)
    {
        switch (team) {
            case ChessTeam.Black:
                return 0x1;
            case ChessTeam.White:
                return 0x2;
            default:
                return 0;
        }
    }

    string UnderAttackMsg
    {
        get
        {
            bool bWhite = IsUnderAttack(ChessTeam.White);
            bool bBlack = IsUnderAttack(ChessTeam.Black);

            if (bWhite && bBlack) {
                return "White:Black";
            }
            else if (bWhite) {
                return "White";
            }
            else if (bBlack) {
                return "Black";
            }
            else {
                return "None";
            }
        }
    }
}