using System;

public static class ActionFactory
{
    public static ActionData Apply(NetAction net, ChessBoard board)
    {
        Pieces piece = board.GetPieceById(net.pieceId);
        if (piece == null)
            throw new Exception($"pieceId not found: {net.pieceId}");

        return piece.MoveTo(net.To);
    }
}
