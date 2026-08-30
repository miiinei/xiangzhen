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
        var q = col - ((row - (row & 1)) / 2);
        return new Vector2I(q, row);
    }

    public static Vector2I AxialToOffset(int q, int r)
    {
        var col = q + ((r - (r & 1)) / 2);
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

    public static List<List<Vector2>> ConnectSegmentsIntoLoops(List<(Vector2 Start, Vector2 End)> segments)
    {
        var loops = new List<List<Vector2>>();
        var visited = new bool[segments.Count];

        for (int i = 0; i < segments.Count; i++)
        {
            if (visited[i]) continue;

            visited[i] = true;
            var currentLoop = new List<Vector2> { segments[i].Start, segments[i].End };
            var currentEnd = segments[i].End;

            while (true)
            {
                int nextIdx = -1;
                float bestDist = 1.0f;

                for (int j = 0; j < segments.Count; j++)
                {
                    if (visited[j]) continue;
                    float dist = currentEnd.DistanceTo(segments[j].Start);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        nextIdx = j;
                    }
                }

                if (nextIdx != -1)
                {
                    visited[nextIdx] = true;
                    currentLoop.Add(segments[nextIdx].End);
                    currentEnd = segments[nextIdx].End;

                    if (currentEnd.DistanceTo(currentLoop[0]) < 1.0f)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            loops.Add(currentLoop);
        }

        return loops;
    }

    public static List<Line2D> DrawRangeBoundary(
        Node2D parent,
        HashSet<Vector2I> inRangePositions,
        IReadOnlyDictionary<Vector2I, Tile> tiles,
        List<Line2D> existingLines,
        Color color,
        string prefix,
        int zIndex)
    {
        var outerSegments = new List<(Vector2 Start, Vector2 End)>();

        foreach (var gridPos in inRangePositions)
        {
            if (!tiles.TryGetValue(gridPos, out var tile) || tile == null)
            {
                continue;
            }

            var vertices = tile.GetHexagonVertices();
            var axial = OffsetToAxial(gridPos.X, gridPos.Y);

            for (int i = 0; i < 6; i++)
            {
                var neighborAxial = axial + HexEdgeDirections[i];
                var neighborOffset = AxialToOffset(neighborAxial.X, neighborAxial.Y);

                if (inRangePositions.Contains(neighborOffset))
                {
                    continue;
                }

                Vector2 start = tile.GlobalPosition + vertices[i];
                Vector2 end = tile.GlobalPosition + vertices[(i + 1) % 6];
                outerSegments.Add((start, end));
            }
        }

        if (outerSegments.Count == 0)
        {
            return existingLines;
        }

        var loops = ConnectSegmentsIntoLoops(outerSegments);

        for (int i = 0; i < loops.Count; i++)
        {
            var loop = loops[i];
            if (loop.Count < 2) continue;

            Line2D line;
            if (i < existingLines.Count)
            {
                line = existingLines[i];
            }
            else
            {
                line = new Line2D
                {
                    Name = $"{prefix}_{i}",
                    Width = 3f,
                    ZIndex = zIndex,
                    JointMode = Line2D.LineJointMode.Round,
                    BeginCapMode = Line2D.LineCapMode.Round,
                    EndCapMode = Line2D.LineCapMode.Round,
                    Antialiased = true
                };
                parent.AddChild(line);
                existingLines.Add(line);
            }

            bool isClosed = loop[0].DistanceTo(loop[^1]) < 1.0f;
            line.Points = loop.ToArray();
            line.Closed = isClosed;
            line.DefaultColor = color;
            line.Visible = true;
        }

        return existingLines;
    }

    public static void ClearLines(List<Line2D> lines)
    {
        if (lines == null) return;
        foreach (var line in lines)
        {
            if (line != null)
            {
                line.Visible = false;
                line.Points = Array.Empty<Vector2>();
            }
        }
    }
}
