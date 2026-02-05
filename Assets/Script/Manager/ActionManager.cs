using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class ActionManager
{
    ChessBoard board;
    BuildedData buildedData = new BuildedData();

    GridIndex? selection;
    ChessTeam actionTeam;

    StateMachine actionState;

    public ActionManager(ChessBoard board)
    {
        this.board = board;

        buildedData.Rebuild(board);
        actionTeam = ChessTeam.White;
        actionState = new UnselectedState(this);
    }

    public void ClearState() => selection = null;

    public void AddClick(GridIndex? click)
    {
        actionState = actionState.Update(click);
    }

    public void Undo() => Debug.LogWarning("[ActionManager] Undo disabled in multiplayer authoritative mode.");
    public void Redo() => Debug.LogWarning("[ActionManager] Redo disabled in multiplayer authoritative mode.");

    public void OnMoveApplied()
    {
        GenericAction();
        board.Turnover();
    }

    void GenericAction()
    {
        Func<MoveSequence, int> getMovableCount = (seq) =>
        {
            int count = 0;
            for (int i = 0; i < seq.SequenceCount; ++i)
                count += seq[i].Count;
            return count;
        };

        actionTeam = (actionTeam == ChessTeam.White) ? ChessTeam.Black : ChessTeam.White;
        buildedData.Rebuild(board);

        int movableCount = 0;

        foreach (var p in board.GetPieces())
        {
            if (p?.Team != actionTeam) continue;

            var seq = p.QueryMovable(MoveType.StandardMove);
            seq.Build(board, MoveType.StandardMove);
            movableCount += getMovableCount(seq);

            seq = p.QueryMovable(MoveType.Attack);
            seq.Build(board, MoveType.Attack);
            movableCount += getMovableCount(seq);

            if (p is Pawn pawn)
            {
                seq = pawn.QueryEnpassant(board);
                movableCount += getMovableCount(seq);
            }
        }

        if (movableCount == 0)
        {
            if (buildedData.IsChecked(actionTeam))
                GameoverEvent?.Invoke(this, GameoverType.Checkmate);
            else
                GameoverEvent?.Invoke(this, GameoverType.Stalemate);
        }
    }

    void AddAttack(Pieces from, Pieces to)
    {
        if (from.Team == to.Team)
        {
            selection = (from == to) ? (GridIndex?)null : to.CellIndex;
            return;
        }

        var seq = from.QueryMovable(MoveType.Attack);
        seq.Build(board, MoveType.Attack);

        if (!seq.ContainsMove(to.CellIndex))
            return;

        SendMoveToServer(from, from.CellIndex, to.CellIndex, NetMoveType.Attack);
    }

    void AddMove(Pieces from, GridIndex to)
    {
        var seq = from.QueryMovable(MoveType.StandardMove);
        seq.Build(board, MoveType.StandardMove);

        if (seq.ContainsMove(to))
        {
            SendMoveToServer(from, from.CellIndex, to, NetMoveType.Move);
            return;
        }

        if (from is King king)
        {
            var castlingSeq = king.QueryCastling(board, out _, out _);
            castlingSeq.Build(board, MoveType.StandardMove);
            if (castlingSeq.ContainsMove(to))
                SendMoveToServer(from, from.CellIndex, to, NetMoveType.Castling);
            return;
        }

        if (from is Pawn pawn)
        {
            var enpassantSeq = pawn.QueryEnpassant(board);
            if (enpassantSeq.ContainsMove(to))
                SendMoveToServer(from, from.CellIndex, to, NetMoveType.EnPassant);
            return;
        }
    }

    void SendMoveToServer(Pieces piece, GridIndex from, GridIndex to, NetMoveType type)
    {
        if (TurnManager.Instance == null)
        {
            Debug.LogError("[SendMoveToServer] TurnManager.Instance NULL");
            return;
        }

        if (!TurnManager.Instance.IsMyTurn())
            return;

        if (piece == null)
        {
            Debug.LogError("[SendMoveToServer] piece is NULL");
            return;
        }

        NetAction net = NetAction.Create(piece.PieceId, from, to, type);
        object[] payload = NetActionPhotonCodec.Encode(net);

        if (PhotonNetwork.IsMasterClient)
        {
            TurnManager.Instance.MasterApplyFromLocal(net, payload);
            return;
        }

        PhotonNetwork.RaiseEvent(
            2,
            payload,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            SendOptions.SendReliable
        );
    }

    public bool HasSelection => selection.HasValue;
    public Pieces SelectionPiece => board[selection.Value];
    public BuildedData BuildedData => buildedData;

    public event EventHandler<GameoverType> GameoverEvent;
}
