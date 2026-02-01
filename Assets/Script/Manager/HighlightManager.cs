using System;
using System.Collections.Generic;
using UnityEngine;

public class HighlightManager
{
    ChessBoard board;

    Material hoverMaterial;
    Material moveMaterial;
    Material attackMaterial;
    Material specialMaterial;

    GridIndex? hoverIndex;
    GameObject hoverGlow;

    readonly List<GameObject> movableGlows = new();
    readonly List<GameObject> attackGlows = new();
    readonly List<GameObject> specialGlows = new();

    public HighlightManager(
        ChessBoard board,
        Material hover,
        Material move,
        Material attack,
        Material special)
    {
        this.board = board;

        hoverMaterial = hover;
        moveMaterial = move;
        attackMaterial = attack;
        specialMaterial = special;

        hoverGlow = CreateGlow(GlowType.Hover);
    }
    public void ClearState()
    {
        HoverIndex = null;
        ClearMoveHighlight();
        ClearAttackHighlight();
        ClearSpecialHighlight();
    }

    public void BuildMoveHighlight(Pieces piece)
    {
        ClearMoveHighlight();

        var moveSeq = piece.QueryMovable(MoveType.StandardMove);
        moveSeq.Build(board, MoveType.StandardMove);

        for (int i = 0; i < moveSeq.SequenceCount; ++i)
        {
            var single = moveSeq[i];
            for (int j = 0; j < single.Count; ++j)
            {
                movableGlows.Add(CreateGlow(GlowType.Movable, single[j]));
            }
        }

        movableGlows.Add(CreateGlow(GlowType.Movable, piece.CellIndex));
    }

    public void BuildAttackHighlight(Pieces piece)
    {
        ClearAttackHighlight();

        var attackSeq = piece.QueryMovable(MoveType.Attack);
        attackSeq.Build(board, MoveType.Attack);

        for (int i = 0; i < attackSeq.SequenceCount; ++i)
        {
            var single = attackSeq[i];
            for (int j = 0; j < single.Count; ++j)
            {
                attackGlows.Add(CreateGlow(GlowType.UnderAttack, single[j]));
            }
        }
    }

    public void BuildSpecialHighligh(Pieces piece)
    {
        ClearSpecialHighlight();

        if (piece is King king)
        {
            var seq = king.QueryCastling(board);
            seq.Build(board, MoveType.StandardMove);

            for (int i = 0; i < seq.SequenceCount; ++i)
            {
                var single = seq[i];
                for (int j = 0; j < single.Count; ++j)
                {
                    specialGlows.Add(CreateGlow(GlowType.Special, single[j]));
                }
            }
        }
        else if (piece is Pawn pawn)
        {
            var seq = pawn.QueryEnpassant(board);

            for (int i = 0; i < seq.SequenceCount; ++i)
            {
                var single = seq[i];
                for (int j = 0; j < single.Count; ++j)
                {
                    specialGlows.Add(CreateGlow(GlowType.Special, single[j]));
                }
            }
        }
    }

    public void ClearMoveHighlight() => ClearHighlights(movableGlows);
    public void ClearAttackHighlight() => ClearHighlights(attackGlows);
    public void ClearSpecialHighlight() => ClearHighlights(specialGlows);

    public GridIndex? HoverIndex
    {
        get => hoverIndex;
        set
        {
            hoverIndex = value;

            if (hoverIndex.HasValue)
            {
                if (!hoverGlow.activeSelf)
                    hoverGlow.SetActive(true);

                hoverGlow.transform.localPosition =
                    board.QueryLocation(hoverIndex.Value);
            }
            else
            {
                if (hoverGlow.activeSelf)
                    hoverGlow.SetActive(false);
            }
        }
    }
    void ClearHighlights(List<GameObject> glows)
    {
        foreach (var glow in glows)
        {
            UnityEngine.Object.Destroy(glow);
        }
        glows.Clear();
    }

    GameObject CreateGlow(GlowType type, GridIndex? initialIndex = null)
    {
        Material material = type switch
        {
            GlowType.Hover => hoverMaterial,
            GlowType.Movable => moveMaterial,
            GlowType.UnderAttack => attackMaterial,
            GlowType.Special => specialMaterial,
            _ => throw new ArgumentOutOfRangeException()
        };

        var glow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glow.transform.SetParent(board.transform, false);

        glow.transform.localScale = new Vector3(1f, 0.01f, 1f);
        glow.GetComponent<MeshRenderer>().material = material;

        if (initialIndex.HasValue)
        {
            glow.transform.localPosition =
                board.QueryLocation(initialIndex.Value);
        }
        else
        {
            glow.SetActive(false);
        }

        return glow;
    }
}