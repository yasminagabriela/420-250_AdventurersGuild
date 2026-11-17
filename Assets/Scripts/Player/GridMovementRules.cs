using System;
using UnityEngine;

public class GridMovementRules
{
    private readonly Func<Vector2Int, bool> _isWalkable;

    public GridMovementRules(Func<Vector2Int, bool> isWalkable)
    {
        _isWalkable = isWalkable;
    }

    public Vector2Int GetNextCell(Vector2Int current, Vector2Int direction)
    {
        Vector2Int target = current + direction;
        return _isWalkable(target) ? target : current;
    }
}