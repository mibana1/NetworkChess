// Copyright 2020 Aumoa.lib. All right reserved.

public static class ChessTeamExtensions
{
    public static ChessTeam Invert(this ChessTeam @this)
    {
        if (@this == ChessTeam.Black) {
            return ChessTeam.White;
        }
        else {
            return ChessTeam.Black;
        }
    }
}