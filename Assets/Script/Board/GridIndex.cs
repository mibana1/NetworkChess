using System;

public struct GridIndex : IEquatable<GridIndex>
{
    public int X
    {
        get;
        set;
    }

    public int Y
    {
        get;
        set;
    }

    public GridIndex(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() =>
        $"X: {X}, Y: {Y}";

    public override int GetHashCode()
    {
        return X.GetHashCode() ^ Y.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if (obj is GridIndex idx) {
            return Equals(idx);
        }
        else {
            return false;
        }
    }

    public bool Equals(GridIndex idx)
    {
        return this == idx;
    }

    public bool IsValid
    {
        get
        {
            return X >= 0 && X < 8
                && Y >= 0 && Y < 8;
        }
    }

    public static bool operator ==(GridIndex left, GridIndex right)
    {
        return left.X == right.X
            && left.Y == right.Y;
    }

    public static bool operator !=(GridIndex left, GridIndex right)
    {
        return left.X != right.X
            || left.Y != right.Y;
    }

    public static GridIndex operator +(GridIndex left, GridIndex right)
    {
        return new GridIndex(left.X + right.X, left.Y + right.Y);
    }

    public static GridIndex operator -(GridIndex left, GridIndex right)
    {
        return new GridIndex(left.X - right.X, left.Y - right.Y);
    }

    public static GridIndex operator *(GridIndex left, int right)
    {
        return new GridIndex(left.X * right, left.Y * right);
    }
}