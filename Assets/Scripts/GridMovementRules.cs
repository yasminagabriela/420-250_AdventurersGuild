using UnityEngine;

public class GridMovementRules
{
    private readonly Vector2Int _min;
    private readonly Vector2Int _max;

    public GridMovementRules(Vector2Int min, Vector2Int max)
    {
        _min = min;
        _max = max;
    }

    public Vector2Int GetNextCell(Vector2Int current, Vector2Int direction)
    {
        Vector2Int target = current + direction;
        bool inside = target.x >= _min.x && target.x <= _max.x
                   && target.y >= _min.y && target.y <= _max.y;
        return inside ? target : current;
    }
}