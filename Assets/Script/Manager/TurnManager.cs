using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TurnManager : MonoBehaviourPun
{
    public static TurnManager Instance;

    public ChessTeam CurrentTurn { get; private set; }

    // Photon RaiseEvent 코드 (겹치지 않게 관리)
    private const byte TurnChangeEventCode = 1;

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

    /// <summary>
    /// 내 턴인지 확인 (모든 입력, 선택 전에 반드시 사용)
    /// </summary>
    public bool IsMyTurn()
    {
        return ChessGameManager.Instance.MyColor == CurrentTurn;
    }

    /// <summary>
    /// 말 이동이 끝났을 때 호출
    /// MasterClient만 실제로 턴을 변경함
    /// </summary>
    public void EndTurn()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        SwitchTurn();
        BroadcastTurn();
    }

    /// <summary>
    /// 턴 변경 로직 (서버 권한)
    /// </summary>
    private void SwitchTurn()
    {
        CurrentTurn = (CurrentTurn == ChessTeam.White)
            ? ChessTeam.Black
            : ChessTeam.White;
    }

    /// <summary>
    /// 턴 변경을 모든 클라이언트에 알림
    /// </summary>
    private void BroadcastTurn()
    {
        object[] data = new object[]
        {
            (int)CurrentTurn
        };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };

        PhotonNetwork.RaiseEvent(
            TurnChangeEventCode,
            data,
            options,
            SendOptions.SendReliable
        );
    }

    /// <summary>
    /// 턴 변경 이벤트 수신
    /// </summary>
    private void OnEventReceived(EventData photonEvent)
    {
        if (photonEvent.Code != TurnChangeEventCode)
            return;

        object[] data = (object[])photonEvent.CustomData;
        CurrentTurn = (ChessTeam)(int)data[0];

        Debug.Log($"[TurnManager] 현재 턴: {CurrentTurn}");
    }
}