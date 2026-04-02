using System.Collections.Generic;
using UnityEngine;

public class PieceInstance
    {
        public GameObject  root;
        public PieceShapeData shape;
        public Vector2Int[] currentCells;
        public int         rotState;
        public Color       color;
        public bool        usePrefab;          // true = sprite từ prefab, false = tạo bằng code
        public List<SpriteRenderer> visuals   = new();
        public List<BoxCollider2D>  colliders = new();
        public bool  isPlaced;
        public int   placedRow, placedCol;
        public Vector3 homePos;
        public int   pressedCellIdx = 0;      // ô đang bấm vào → pivot khi xoay
    }