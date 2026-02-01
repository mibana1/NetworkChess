using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class RoomManager :MonoBehaviourPunCallbacks
{
    [SerializeField] Button roomBtn;
    [SerializeField] TMP_Text player1;
    [SerializeField] TMP_Text player2;
    Dictionary<int, TMP_Text> playerSlots = new Dictionary<int, TMP_Text>();
    Dictionary<int, ChessTeam> playerColors = new Dictionary<int, ChessTeam>();

    private void Start()
    {
        // 방속 사람 불러옴
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            AssignPlayer(players[i]);
        }

        // 방장이 아니라면 
        roomBtn.interactable = PhotonNetwork.IsMasterClient;
    }

    public void StartGame()
    {
        if(PhotonNetwork.IsMasterClient == true)
        {
            PhotonNetwork.LoadLevel("InGameScene");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + "님이 방에 입장하셨습니다");
        AssignPlayer(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + "님이 방에 나가셨습니다");
        if (playerSlots.TryGetValue(otherPlayer.ActorNumber, out TMP_Text slot))
        {
            slot.text = "";
            playerSlots.Remove(otherPlayer.ActorNumber);
        }
        ReorderSlots();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log(newMasterClient.NickName + "님이 방장이 되었습니다");
        roomBtn.interactable = PhotonNetwork.IsMasterClient;
        ReassignColors();
    }

    void AssignPlayer(Player player)
    {
        if (playerSlots.ContainsKey(player.ActorNumber))
            return;

        ChessTeam color =
            player.IsMasterClient ? ChessTeam.White : ChessTeam.Black;

        TMP_Text slot = color == ChessTeam.White ? player1 : player2;

        slot.text = $"{player.NickName} ({(color == ChessTeam.White ? "백" : "흑")})";

        playerSlots[player.ActorNumber] = slot;
        playerColors[player.ActorNumber] = color;
    }

    void ReassignColors()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            ChessTeam newColor =
                player.IsMasterClient ? ChessTeam.White : ChessTeam.Black;

            playerColors[player.ActorNumber] = newColor;

            TMP_Text slot = newColor == ChessTeam.White ? player1 : player2;
            slot.text = $"{player.NickName} ({(newColor == ChessTeam.White ? "백" : "흑")})";

            playerSlots[player.ActorNumber] = slot;
        }
    }

    // 1명만 남았을 때
    void ReorderSlots()
    {
        if (playerSlots.Count == 1)
        {
            foreach (var kv in playerSlots)
            {
                TMP_Text currentSlot = kv.Value;

                if (currentSlot == player2)
                {
                    player1.text = player2.text;
                    player2.text = "";

                    playerSlots[kv.Key] = player1;
                }
            }
        }
    }
}
