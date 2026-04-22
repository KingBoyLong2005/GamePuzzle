using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Level/Board Data")]

public class LevelBoardData : ScriptableObject {
    public int width = 4;
    public int height = 4;

    // Unity sẽ hiển thị cái này vì nó là mảng của một Class [Serializable]
    public Row[] rows;

    public PieceInstanceTest[] pieces; // Danh sách các Piece có trong Level này, sẽ được khởi tạo ở Runtime

    // Hàm này tự chạy mỗi khi bạn thay đổi giá trị trên Inspector
    private void OnValidate() {
        if (rows == null || rows.Length != height) {
            System.Array.Resize(ref rows, height);
        }

        for (int i = 0; i < height; i++) {
            if (rows[i] == null || rows[i].columns.Length != width) {
                rows[i] = new Row { columns = new TileType[width] };
            }
        }
        for(int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null)
            {
                pieces[i] = new PieceInstanceTest { positions = new List<Vector2>() };
            }
        }
    }

    // Hàm tiện ích để lấy giá trị nhanh trong Code game
    public TileType GetTile(int x, int y) {
        return rows[y].columns[x];
    }
    public int GetActiveCountInRow(int y)
    {
        int count = 0;
        foreach (var tile in rows[y].columns)
        {
            if (tile == TileType.Active) count++;
        }
        return count;
    }

    public int GetActiveCountInColumn(int x)
    {
        int count = 0;
        for (int y = 0; y < height; y++)
        {
            if (rows[y].columns[x] == TileType.Active) count++;
        }
        return count;
    }

    // Hàm đếm tổng toàn bộ Board để biết điều kiện thắng tổng thể
    public int GetTotalActiveCount()
    {
        int total = 0;
        for (int y = 0; y < height; y++)
        {
            total += GetActiveCountInRow(y);
        }
        return total;
    }
}
// Định nghĩa các loại ô
public enum TileType {
    Default = 0,    // Mặc định
    Active = 1,     // Được kích hoạt
    Inactive = 2    // Không được quyền
}

[System.Serializable]
public class Row {
    public TileType[] columns; // Mảng các cột trong một hàng
}
[System.Serializable]
public class PieceInstanceTest
{
    public List<Vector2> positions; // Danh sách vị trí của các ô được kích hoạt trong Piece này
}