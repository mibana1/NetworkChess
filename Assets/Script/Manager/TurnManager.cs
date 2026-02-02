using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TurnManager : MonoBehaviourPun
{
    public static TurnManager Instance;

    private const byte TurnChangeEventCode = 1;
    private const byte MoveEventCode = 2;

    public ChessTeam CurrentTurn { get; private set; }

    [SerializeField] private ChessBoard board;

    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 체스는 항상 백 선
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

    public void SetBoard(ChessBoard chessBoard)
    {
        board = chessBoard;
    }

    /// <summary>
    /// 내 턴인지 확인
    /// </summary>
    public bool IsMyTurn()
    {
        return ChessGameManager.Instance.MyColor == CurrentTurn;
    }

    /// <summary>
    /// MasterClient만 턴 변경
    /// </summary>
    public void EndTurn()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        SwitchTurn();
        BroadcastTurn();
    }

    private void SwitchTurn()
    {
        CurrentTurn = (CurrentTurn == ChessTeam.White)
            ? ChessTeam.Black
            : ChessTeam.White;
    }

    private void BroadcastTurn()
    {
        object[] data = new object[]
        {
            (int)CurrentTurn
        };

        PhotonNetwork.RaiseEvent(
            TurnChangeEventCode,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    private void OnEventReceived(EventData photonEvent)
    {
        if (photonEvent.Code == MoveEventCode)
        {
            NetAction net = (NetAction)photonEvent.CustomData;

            if (PhotonNetwork.IsMasterClient)
            {
                // 서버 권한 이동 적용
                ApplyMove(net);

                // 턴 변경
                EndTurn();

                PhotonNetwork.RaiseEvent(
                    MoveEventCode,
                    net,
                    new RaiseEventOptions { Receivers = ReceiverGroup.All },
                    SendOptions.SendReliable
                );
            }
            else
            {
                ApplyMove(net);
            }
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
        if (board == null)
        {
            Debug.LogError("[TurnManager] ChessBoard reference is null.");
            return;
        }

        // 기본 이동
        var action = ActionFactory.Apply(net, board);
        action.Redo();

        // 특수 수 처리
        ApplySpecialMove(net);
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

    // 캐슬링
    private void ApplyCastling(NetAction net)
    {
        Pieces king = board.GetPieceById(net.pieceId);
        if (king is not King)
            return;

        int y = net.to.Y;

        if (net.to.X == 6)
        {
            var rookFrom = new GridIndex(7, y);
            var rookTo = new GridIndex(5, y);

            if (board[rookFrom] is Rook rook)
            {
                var rookAction = rook.MoveTo(rookTo);
                rookAction.Redo();
            }
        }
        else if (net.to.X == 2)
        {
            var rookFrom = new GridIndex(0, y);
            var rookTo = new GridIndex(3, y);

            if (board[rookFrom] is Rook rook)
            {
                var rookAction = rook.MoveTo(rookTo);
                rookAction.Redo();
            }
        }
    }

    // 앙파상
    private void ApplyEnPassant(NetAction net)
    {
        Pieces pawn = board.GetPieceById(net.pieceId);
        if (pawn is not Pawn p)
            return;

        int killedY = p.Team == ChessTeam.White
            ? net.to.Y + 1
            : net.to.Y - 1;

        var killedIndex = new GridIndex(net.to.X, killedY);
        Pieces killedPawn = board[killedIndex];

        if (killedPawn != null)
        {
            Destroy(killedPawn.gameObject);
            board[killedIndex] = null;
        }
    }
}
