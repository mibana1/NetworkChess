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
    Stack<ActionCommand> undoStack = new Stack<ActionCommand>();
    Stack<ActionCommand> redoStack = new Stack<ActionCommand>();
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

    public void ClearState()
    {
        selection = null;
    }

    public void AddClick(GridIndex? click)
    {
        actionState = actionState.Update(click);
    }

    public void Undo()
    {
        // 멀티 authoritative 구조에서는 Undo/Redo는 일단 비활성 권장
        // (서버와 동기화까지 같이 설계해야 함)
        Debug.LogWarning("[ActionManager] Undo disabled in multiplayer authoritative mode.");
    }

    public void Redo()
    {
        Debug.LogWarning("[ActionManager] Redo disabled in multiplayer authoritative mode.");
    }

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
            {
                var single = seq[i];
                count += single.Count;
            }

            return count;
        };

        SwitchActionTeam();
        buildedData.Rebuild(board);

        int movableCount = 0;

        foreach (var p in board.GetPieces())
        {
            if (p?.Team == actionTeam)
            {
                var seq = p.QueryMovable(MoveType.StandardMove);
                seq.Build(board, MoveType.StandardMove);
                movableCount += getMovableCount(seq);

                seq = p.QueryMovable(MoveType.Attack);
                seq.Build(board, MoveType.Attack);
                movableCount += getMovableCount(seq);

                if (p is Pawn k)
                {
                    seq = k.QueryEnpassant(board);
                    movableCount += getMovableCount(seq);
                }
            }
        }

        if (movableCount == 0)
        {
            if (buildedData.IsChecked(actionTeam))
            {
                GameoverEvent?.Invoke(this, GameoverType.Checkmate);
            }
            else
            {
                GameoverEvent?.Invoke(this, GameoverType.Stalemate);
            }
        }
    }

    void SwitchActionTeam()
    {
        actionTeam = (actionTeam == ChessTeam.White) ? ChessTeam.Black : ChessTeam.White;
    }

    // ============================================================
    // ✅ 여기부터 핵심: MoveTo() 절대 호출 금지 -> 요청만 보냄
    // ============================================================

    void AddAttack(Pieces from, Pieces to)
    {
        if (from.Team == to.Team)
        {
            if (from == to) selection = null;
            else selection = to.CellIndex;
            return;
        }

        var seq = from.QueryMovable(MoveType.Attack);
        seq.Build(board, MoveType.Attack);

        if (!seq.ContainsMove(to.CellIndex))
            return;

        // 🔴 로컬 적용 금지: from.MoveTo(...) 하지 말 것
        SendMoveToServer(from, from.CellIndex, to.CellIndex, NetMoveType.Attack);
    }

    void AddMove(Pieces from, GridIndex to)
    {
        var seq = from.QueryMovable(MoveType.StandardMove);
        seq.Build(board, MoveType.StandardMove);

        if (seq.ContainsMove(to))
        {
            // 🔴 로컬 적용 금지
            SendMoveToServer(from, from.CellIndex, to, NetMoveType.Move);
            return;
        }

        // 캐슬링
        if (from is King king)
        {
            AddCastling(king, to);
            return;
        }

        // 앙파상
        if (from is Pawn pawn)
        {
            AddEnpassant(pawn, to);
            return;
        }
    }

    void AddCastling(King from, GridIndex to)
    {
        var castlingSeq = from.QueryCastling(board, out var bQueenSide, out var bKingSide);
        castlingSeq.Build(board, MoveType.StandardMove);

        if (!castlingSeq.ContainsMove(to))
            return;

        // 🔴 로컬에서 rook/king MoveTo 절대 하지 말 것
        // 캐슬링은 king 이동만 보내고, TurnManager에서 rook 이동 재현
        SendMoveToServer(from, from.CellIndex, to, NetMoveType.Castling);
    }

    void AddEnpassant(Pawn from, GridIndex to)
    {
        var enpassantSeq = from.QueryEnpassant(board);

        if (!enpassantSeq.ContainsMove(to))
            return;

        // 🔴 로컬에서 target/pawn MoveTo 금지
        // 앙파상은 pawn 이동만 보내고, TurnManager에서 잡힌 폰 제거 재현
        SendMoveToServer(from, from.CellIndex, to, NetMoveType.EnPassant);
    }

    // ============================================================
    // ✅ 서버로 "요청"만 보내는 함수 (NetAction 직접 생성)
    // ============================================================
    void SendMoveToServer(Pieces piece, GridIndex from, GridIndex to, NetMoveType type)
    {
        if (TurnManager.Instance == null)
        {
            Debug.LogError("[SendMoveToServer] TurnManager.Instance NULL");
            return;
        }

        // 🔴 내 턴 아니면 요청 금지 (입력 차단이 ChessBoard에도 있어야 더 안전)
        if (!TurnManager.Instance.IsMyTurn())
            return;

        if (piece == null)
        {
            Debug.LogError("[SendMoveToServer] piece is NULL");
            return;
        }

        NetAction net = NetAction.Create(piece.PieceId, from, to, type);
        object[] payload = NetActionPhotonCodec.Encode(net);

        PhotonNetwork.RaiseEvent(
            2, // MoveEventCode
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
