using System;

public static class ActionFactory
{
    public static ActionData Apply(NetAction net, ChessBoard board)
    {
        Pieces piece = board.GetPieceById(net.pieceId);
        if (piece == null)
            throw new Exception($"pieceId not found: {net.pieceId}");

        if (piece.CellIndex != net.from)
            throw new Exception($"piece position mismatch. expected:{net.from} actual:{piece.CellIndex}");

        return piece.MoveTo(net.to);
    }
}
