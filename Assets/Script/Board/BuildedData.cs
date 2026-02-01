using System.Collections.Generic;

public class BuildedData
{
    CellBuildInfo[,] buildInfos;
    ChessTeam? checkedTeam = null;
    bool doubleCheck = false;

    public BuildedData()
    {

    }

    public void Rebuild(ChessBoard board)
    {
        buildInfos = new CellBuildInfo[8, 8];
        checkedTeam = null;
        doubleCheck = false;

        MaskOwning(board);
        MaskAttack(board);
    }

    public bool IsChecked(ChessTeam team)
    {
        if (checkedTeam.HasValue) {
            return checkedTeam.Value == team || doubleCheck;
        }
        else {
            return false;
        }
    }

    public bool IsUnderAttack(GridIndex index, ChessTeam team)
    {
        return this[index].IsUnderAttack(team);
    }

    void MaskOwning(ChessBoard board)
    {
        for (int x = 0; x < 8; ++x) {
            for (int y = 0; y < 8; ++y) {
                buildInfos[x, y].Owner = board[x, y]?.Team;
            }
        }
    }

    void MaskAttack(ChessBoard board)
    {
        for (int x = 0; x < 8; ++x) {
            for (int y = 0; y < 8; ++y) {
                var attackSeq = board[x, y]?.QueryMovable(MoveType.Attack);
                for (int k = 0; k < attackSeq?.SequenceCount; ++k) {
                    MaskAttackSingleLine(attackSeq.Owner, board, attackSeq[k]);
                }
            }
        }
    }

    void MaskAttackSingleLine(Pieces owner, ChessBoard board, IList<GridIndex> single)
    {
        foreach (var idx in single) {
            if (this[idx].Owner.HasValue) {
                if (this[idx].Owner.Value != owner.Team) {
                    AttackMask(idx, owner.Team, board);
                }
                break;
            }
            else {
                AttackMask(idx, owner.Team, board);
            }
        }
    }

    void AttackMask(GridIndex index, ChessTeam team, ChessBoard board)
    {
        this[index].CanAttack(team);

        if (board[index] is King) {
            if (checkedTeam.HasValue) {
                doubleCheck = true;
            }
            else {
                checkedTeam = board[index].Team;
            }
        }
    }

    ref CellBuildInfo this[GridIndex index] => ref buildInfos[index.X, index.Y];
}