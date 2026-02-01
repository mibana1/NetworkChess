using UnityEngine;
using System;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [Header("White Pieces")]
    [SerializeField] GameObject whiteKingPrefab;
    [SerializeField] GameObject whiteQueenPrefab;
    [SerializeField] GameObject whiteBishopPrefab;
    [SerializeField] GameObject whiteKnightPrefab;
    [SerializeField] GameObject whiteRookPrefab;
    [SerializeField] GameObject whitePawnPrefab;

    [Header("Black Pieces")]
    [SerializeField] GameObject blackKingPrefab;
    [SerializeField] GameObject blackQueenPrefab;
    [SerializeField] GameObject blackBishopPrefab;
    [SerializeField] GameObject blackKnightPrefab;
    [SerializeField] GameObject blackRookPrefab;
    [SerializeField] GameObject blackPawnPrefab;

    ChessBoard board;

    Dictionary<(Type, ChessTeam), GameObject> prefabMap;

    public void Init(ChessBoard board)
    {
        this.board = board;
        BuildPrefabMap();
    }

    void BuildPrefabMap()
    {
        prefabMap = new Dictionary<(Type, ChessTeam), GameObject>();

        Map<King>(whiteKingPrefab, blackKingPrefab);
        Map<Queen>(whiteQueenPrefab, blackQueenPrefab);
        Map<Bishop>(whiteBishopPrefab, blackBishopPrefab);
        Map<Knight>(whiteKnightPrefab, blackKnightPrefab);
        Map<Rook>(whiteRookPrefab, blackRookPrefab);
        Map<Pawn>(whitePawnPrefab, blackPawnPrefab);
    }

    void Map<T>(GameObject white, GameObject black) where T : Pieces
    {
        if (white == null || black == null)
            Debug.LogError($"Prefab not assigned: {typeof(T).Name}");

        prefabMap[(typeof(T), ChessTeam.White)] = white;
        prefabMap[(typeof(T), ChessTeam.Black)] = black;
    }

    GameObject GetPrefab<T>(ChessTeam team) where T : Pieces
    {
        var key = (typeof(T), team);
        if (!prefabMap.TryGetValue(key, out var prefab) || prefab == null)
        {
            throw new Exception($"Prefab not found: {typeof(T).Name}, {team}");
        }
        return prefab;
    }

    public T SpawnActor<T>(GridIndex gridIndex, ChessTeam team) where T : Pieces
    {
        var prefab = GetPrefab<T>(team);
        var go = Instantiate(prefab);
        go.name = $"{team} {typeof(T).Name}";

        var component = go.AddActorComponent<T>(team, gridIndex, board);
        board[gridIndex] = component;

        go.transform.SetParent(board.transform, false);

        return component;
    }

    public T SpawnActor<T>(int x, int y, ChessTeam team) where T : Pieces
        => SpawnActor<T>(new GridIndex(x, y), team);

    // ---------------- 초기 배치 ----------------

    public void InitialSpawn()
    {
        for (int i = 0; i < 8; ++i)
        {
            SpawnActor<Pawn>(i, 1, ChessTeam.White);
            SpawnActor<Pawn>(i, 6, ChessTeam.Black);
        }

        SpawnActor<King>(4, 0, ChessTeam.White);
        SpawnActor<Queen>(3, 0, ChessTeam.White);
        SpawnActor<Bishop>(2, 0, ChessTeam.White);
        SpawnActor<Bishop>(5, 0, ChessTeam.White);
        SpawnActor<Knight>(1, 0, ChessTeam.White);
        SpawnActor<Knight>(6, 0, ChessTeam.White);
        SpawnActor<Rook>(0, 0, ChessTeam.White);
        SpawnActor<Rook>(7, 0, ChessTeam.White);

        SpawnActor<King>(4, 7, ChessTeam.Black);
        SpawnActor<Queen>(3, 7, ChessTeam.Black);
        SpawnActor<Bishop>(2, 7, ChessTeam.Black);
        SpawnActor<Bishop>(5, 7, ChessTeam.Black);
        SpawnActor<Knight>(1, 7, ChessTeam.Black);
        SpawnActor<Knight>(6, 7, ChessTeam.Black);
        SpawnActor<Rook>(0, 7, ChessTeam.Black);
        SpawnActor<Rook>(7, 7, ChessTeam.Black);
    }
}