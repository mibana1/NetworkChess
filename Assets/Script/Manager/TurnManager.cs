using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TurnManager : MonoBehaviourPun
{
    public static TurnManager Instance;

    private const byte TurnChangeEventCode = 1;

    // 요청: Client -> Master
    private const byte MoveEventCode = 2;

    // 결과: Master -> All
    private const byte MoveBroadcastCode = 3;

    public ChessTeam CurrentTurn { get; private set; }

    [SerializeField] private ChessBoard board;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentTurn = ChessTeam.White;
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;
    }

    public void SetBoard(ChessBoard chessBoard) => board = chessBoard;

    public bool IsMyTurn()
    {
        return ChessGameManager.Instance.MyColor == CurrentTurn;
    }

    public void EndTurn()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        CurrentTurn = (CurrentTurn == ChessTeam.White) ? ChessTeam.Black : ChessTeam.White;
        BroadcastTurn();
    }

    private void BroadcastTurn()
    {
        object[] data = new object[] { (int)CurrentTurn };

        PhotonNetwork.RaiseEvent(
            TurnChangeEventCode,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }
    public void MasterApplyFromLocal(NetAction net, object[] payload)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        ApplyMove(net);
        EndTurn();

        PhotonNetwork.RaiseEvent(
            MoveBroadcastCode,
            payload,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    private void OnEventReceived(EventData photonEvent)
    {
        if (photonEvent.Code == MoveEventCode)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            int myActor = PhotonNetwork.LocalPlayer.ActorNumber;
            if (photonEvent.Sender == myActor)
                return;

            object[] payload = (object[])photonEvent.CustomData;
            NetAction net = NetActionPhotonCodec.Decode(payload);

            ApplyMove(net);
            EndTurn();

            PhotonNetwork.RaiseEvent(
                MoveBroadcastCode,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable
            );
            return;
        }


        if (photonEvent.Code == MoveBroadcastCode)
        {
            if (PhotonNetwork.IsMasterClient)
                return;

            object[] payload = (object[])photonEvent.CustomData;
            NetAction net = NetActionPhotonCodec.Decode(payload);

            ApplyMove(net);
            return;
        }

        if (photonEvent.Code == TurnChangeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            CurrentTurn = (ChessTeam)(int)data[0];
            Debug.Log($"[TurnManager] 현재 턴: {CurrentTurn}");
            return;
        }
    }

    private void ApplyMove(NetAction net)
    {
        Debug.Log($"[ApplyMove] pieceId={net.pieceId} {net.From}->{net.To} actor={PhotonNetwork.LocalPlayer.ActorNumber} master={PhotonNetwork.IsMasterClient}");

        if (board == null)
        {
            Debug.LogError("[TurnManager] ChessBoard reference is null.");
            return;
        }

        var action = ActionFactory.Apply(net, board);
        action.Redo();

        ApplySpecialMove(net);

        board.ActionMgr.OnMoveApplied();
    }

    private void ApplySpecialMove(NetAction net)
    {
        switch (net.netMoveType)
        {
            case NetMoveType.Castling:
                ApplyCastling(net);
                break;
            case NetMoveType.EnPassant:
                ApplyEnPassant(net);
                break;
            case NetMoveType.Promotion:
                break;
        }
    }

    private void ApplyCastling(NetAction net)
    {
        Pieces king = board.GetPieceById(net.pieceId);
        if (king is not King) return;

        int y = net.To.Y;

        if (net.To.X == 6)
        {
            var rookFrom = new GridIndex(7, y);
            var rookTo = new GridIndex(5, y);

            if (board[rookFrom] is Rook rook)
            {
                rook.MoveTo(rookTo).Redo();
            }
        }
        else if (net.To.X == 2)
        {
            var rookFrom = new GridIndex(0, y);
            var rookTo = new GridIndex(3, y);

            if (board[rookFrom] is Rook rook)
            {
                rook.MoveTo(rookTo).Redo();
            }
        }
    }

    private void ApplyEnPassant(NetAction net)
    {
        Pieces pawn = board.GetPieceById(net.pieceId);
        if (pawn is not Pawn p) return;

        int killedY = (p.Team == ChessTeam.White) ? net.To.Y + 1 : net.To.Y - 1;
        var killedIndex = new GridIndex(net.To.X, killedY);

        Pieces killedPawn = board[killedIndex];
        if (killedPawn == null) return;
        if (killedPawn.Team == p.Team) return;

        Destroy(killedPawn.gameObject);
        board[killedIndex] = null;
    }
}
