using Godot;
using System;
using System.Collections.Generic;

public static class Function
{
    public static readonly Vector2I[] HexEdgeDirections =
    {
        new(0, 1),   // Edge 0: 右下 (points[0] -> points[1])
        new(-1, 1),  // Edge 1: 左下 (points[1] -> points[2])
        new(-1, 0),  // Edge 2: 正左 (points[2] -> points[3])
        new(0, -1),  // Edge 3: 左上 (points[3] -> points[4])
        new(1, -1),  // Edge 4: 右上 (points[4] -> points[5])
        new(1, 0)    // Edge 5: 正右 (points[5] -> points[0])
    };

    public static Vector2I OffsetToAxial(int col, int row)
    {
        var q = col - ((row - (row & HexGeometry.OddRowMask)) / HexGeometry.OffsetParityDivisor);
        return new Vector2I(q, row);
    }

    public static Vector2I AxialToOffset(int q, int r)
    {
        var col = q + ((r - (r & HexGeometry.OddRowMask)) / HexGeometry.OffsetParityDivisor);
        return new Vector2I(col, r);
    }

    public static int HexDistance(Vector2I a, Vector2I b)
    {
        var axialA = OffsetToAxial(a.X, a.Y);
        var axialB = OffsetToAxial(b.X, b.Y);
        var dq = axialA.X - axialB.X;
        var dr = axialA.Y - axialB.Y;
        var ds = -dq - dr;
        return Mathf.Max(Mathf.Abs(dq), Mathf.Max(Mathf.Abs(dr), Mathf.Abs(ds)));
    }

    public static bool TryGetHexStraightDirection(Vector2I startOffset, Vector2I targetOffset, out Vector2I directionAxial)
    {
        var axialA = OffsetToAxial(startOffset.X, startOffset.Y);
        var axialB = OffsetToAxial(targetOffset.X, targetOffset.Y);
        int dq = axialB.X - axialA.X;
        int dr = axialB.Y - axialA.Y;

        if (dq == 0 && dr == 0)
        {
            directionAxial = Vector2I.Zero;
            return false;
        }

        if (dq == 0)
        {
            directionAxial = new Vector2I(0, Math.Sign(dr));
            return true;
        }
        if (dr == 0)
        {
            directionAxial = new Vector2I(Math.Sign(dq), 0);
            return true;
        }
        if (dq + dr == 0)
        {
            directionAxial = new Vector2I(Math.Sign(dq), Math.Sign(dr));
            return true;
        }

        directionAxial = Vector2I.Zero;
        return false;
    }

    public static bool TryGetHexDirectionToward(Vector2I startOffset, Vector2I targetOffset, out Vector2I directionAxial)
    {
        if (startOffset == targetOffset)
        {
            directionAxial = Vector2I.Zero;
            return false;
        }

        var startAxial = OffsetToAxial(startOffset.X, startOffset.Y);
        int bestDistance = int.MaxValue;
        directionAxial = Vector2I.Zero;

        foreach (var candidateDirection in HexEdgeDirections)
        {
            var candidateOffset = AxialToOffset(
                startAxial.X + candidateDirection.X,
                startAxial.Y + candidateDirection.Y
            );
            int distance = HexDistance(candidateOffset, targetOffset);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                directionAxial = candidateDirection;
            }
        }

        return true;
    }

    public static List<Vector2I> GetHexStraightLine(Vector2I startOffset, Vector2I directionAxial, int range)
    {
        var line = new List<Vector2I>();
        var startAxial = OffsetToAxial(startOffset.X, startOffset.Y);
        for (int step = 1; step <= range; step++)
        {
            var nextAxial = startAxial + directionAxial * step;
            line.Add(AxialToOffset(nextAxial.X, nextAxial.Y));
        }
        return line;
    }

    public static IEnumerable<Tile> GetNeighborTiles(Tile tile, IReadOnlyDictionary<Vector2I, Tile> tiles)
    {
        if (tile == null || tiles == null)
        {
            yield break;
        }

        var axial = OffsetToAxial(tile.GridPosition.X, tile.GridPosition.Y);
        foreach (var direction in HexEdgeDirections)
        {
            var neighborAxial = axial + direction;
            var neighborPosition = AxialToOffset(neighborAxial.X, neighborAxial.Y);
            if (tiles.TryGetValue(neighborPosition, out var neighbor))
            {
                yield return neighbor;
            }
        }
    }

}
