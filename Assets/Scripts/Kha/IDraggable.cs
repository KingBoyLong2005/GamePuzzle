using System.Collections.Generic;
using UnityEngine;

public interface IDraggable
{
    void OnDragStart(Vector3 worldPos);
    void OnDragging(Vector3 worldPos);
    void OnDragEnd(Vector3 worldPos);
    Transform GetTransform();
    List<Vector2Int> GetOccupiedCells(Vector2Int centerGridPos);
}