using System;
using System.Collections.Generic;

public class MoveSequence
{
    // 이 이동 시퀀스를 생성한 말
    readonly Pieces owner;

    // 시퀀스 생성 시점의 말 위치
    readonly GridIndex originIndex;

    // 방향별 이동 시퀀스
    List<GridIndex>[] sequences;

    public MoveSequence(Pieces owner, GridIndex ownerIndex, int numSequences)
    {
        this.owner = owner;
        this.originIndex = ownerIndex;

        sequences = new List<GridIndex>[numSequences];
        for (int i = 0; i < numSequences; ++i)
        {
            sequences[i] = new List<GridIndex>();
        }
    }

    /// <summary>
    /// 기준 위치를 실제 좌표로 변환해 시퀀스에 추가
    /// </summary>
    public void AddMove(int sequenceIndex, GridIndex move)
    {
        GridIndex final = originIndex + move;
        if (final.IsValid)
        {
            sequences[sequenceIndex].Add(final);
        }
    }

    public int MoveCount(int sequenceIndex)
    {
        return sequences[sequenceIndex].Count;
    }

    /// <summary>
    /// 이동 시퀀스를 보드 상태에 맞게 필터링한다.
    /// </summary>
    public void Build(ChessBoard board, MoveType moveType, bool buildCriticalMove = true)
    {
        if (moveType == MoveType.StandardMove)
        {
            FilterStandardMove(board);
        }
        else
        {
            FilterAttack(board);
        }

        if (buildCriticalMove)
        {
            FilterCriticalMove(board);
        }
    }

    /// <summary>
    /// 특정 좌표가 포함되어 있는지 검사
    /// </summary>
    public bool ContainsMove(GridIndex index)
    {
        for (int i = 0; i < SequenceCount; ++i)
        {
            var single = this[i];
            for (int j = 0; j < single.Count; ++j)
            {
                if (index.Equals(single[j])) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 중간에 말이 있으면 그 뒤 경로 전부 제거
    /// </summary>
    void FilterStandardMove(ChessBoard board)
    {
        for (int i = 0; i < SequenceCount; ++i)
        {
            var single = this[i];
            for (int j = 0; j < single.Count; ++j)
            {
                if (board[single[j]] != null)
                {
                    // 막힌 지점부터 끝까지 제거
                    single.RemoveRange(j, single.Count - j);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 각 방향에서 처음 만나는 적만 공격 가능
    /// </summary>
    void FilterAttack(ChessBoard board)
    {
        var result = new List<GridIndex>();

        for (int i = 0; i < SequenceCount; ++i)
        {
            var single = this[i];
            for (int j = 0; j < single.Count; ++j)
            {
                var target = board[single[j]];

                if (target != null)
                {
                    if (target.Team != owner.Team)
                    {
                        result.Add(single[j]);
                    }
                    break;
                }
            }
        }

        sequences = new List<GridIndex>[1];
        sequences[0] = result;
    }

    /// <summary>
    /// 체크 상태 검사
    /// </summary>
    void FilterCriticalMove(ChessBoard board)
    {
        var tempBuild = new BuildedData();

        for (int i = 0; i < SequenceCount; ++i)
        {
            var single = this[i];
            for (int j = 0; j < single.Count; ++j)
            {
                var action = owner.MoveTo(single[j]);

                tempBuild.Rebuild(board);

                if (tempBuild.IsChecked(owner.Team))
                {
                    single.RemoveAt(j);
                    j -= 1;
                }

                action.Undo();
            }
        }
    }

    public List<GridIndex> this[int index] => sequences[index];
    public int SequenceCount => sequences.Length;
    public Pieces Owner => owner;
}