using Photon.Pun;
using UnityEngine;

public class ChessGameManager : MonoBehaviourPun
{
    public static ChessGameManager Instance;

    public ChessTeam MyColor { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Debug.Log(
            $"[Photon][InGame] IsMaster={PhotonNetwork.IsMasterClient}, " +
            $"Actor={PhotonNetwork.LocalPlayer.ActorNumber}, " +
            $"PlayerCount={PhotonNetwork.CurrentRoom?.PlayerCount}"
        );

        MyColor = PhotonNetwork.IsMasterClient
            ? ChessTeam.White
            : ChessTeam.Black;

        Debug.Log($"[MyColor] {MyColor}");
    }
}
