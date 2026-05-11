using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Board : MonoBehaviour
{

    [SerializeField] private GameObject Cell;
    [SerializeField] private GameObject ClueBadge;
    [SerializeField] LevelBoardData levelBoardData;
    private Cell[,] cellList;
    public int width = 5;
    public int height = 5;

    private void Start()
    {
        cellList = new Cell[levelBoardData.width, levelBoardData.height];        
        CreateBoard(levelBoardData.width, levelBoardData.height);
        CreateClueBadge();


    }
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {   
        //     for (int y = 0; y < levelBoardData.height; y++)
        //     {
        //         for (int x = 0; x < levelBoardData.width; x++)
        //         {
        //             Debug.Log($" Cell at ({x}, {y}): " + cellList[x, y].GetColor());
        //         }
        //     }
        // }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(CheckFinish());
            if(CheckFinish() == levelBoardData.GetTotalActiveCount())
            {
                Debug.Log("Finish");
            }
        }
    }

    public void CreateBoard(int width, int height)
    {
        Vector3 startPos = new Vector3(-(width-1)/2f, (height-1)/2f, 0);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = Instantiate(Cell, startPos + new Vector3(x, -y, 0), Quaternion.identity);
                cell.transform.SetParent(this.transform);
                cellList[x, y] = cell.GetComponent<Cell>();
            }
        }
    }

    public void CreateClueBadge()
    {
        Vector3 startPosUp = new Vector3(-(width-1)/2f, (height-1)/2f+1, 0);
        Vector3 startPosLeft = new Vector3(-(width-1)/2f-1, (height-1)/2f, 0);
        for (int x = 0; x < width; x++)
        {
            // Lấy dữ liệu của một cột x
            // TileType[] columnTiles = new TileType[height];
            // for (int y = 0; y < height; y++) {
            //     columnTiles[y] = levelBoardData.rows[y].columns[x];
            // }

            // int clues = GetClues(columnTiles);
            // string clueString = string.Join("\n", clues);
            int totalActive = levelBoardData.GetActiveCountInColumn(x);
            var clueBadgeUp = Instantiate(ClueBadge, startPosUp + new Vector3(x, 0, 0), Quaternion.identity);
            clueBadgeUp.transform.SetParent(this.transform);
            clueBadgeUp.GetComponentInChildren<TextMeshPro>().text = totalActive.ToString();
        }
        for (int y = 0; y < height; y++)
        {
            // Lấy dữ liệu của hàng y
            // TileType[] rowTiles = levelBoardData.rows[y].columns;

            // int clues = GetClues(rowTiles);
            // string clueString = clues.ToString();
            int clues = levelBoardData.GetActiveCountInRow(y);
            var clueBadgeLeft = Instantiate(ClueBadge, startPosLeft + new Vector3(0, -y, 0), Quaternion.identity);
            clueBadgeLeft.transform.SetParent(this.transform);
            clueBadgeLeft.GetComponentInChildren<TextMeshPro>().text = clues.ToString();
        }

    }
    public int CheckFinish()
    {
        int countCheck = 0;
        for (int y = 0; y < levelBoardData.height; y++)
        {
            for (int x = 0; x < levelBoardData.width; x++)
            {
                if(cellList[x, y].GetColor() == Color.red && levelBoardData.GetTile(x, y) == TileType.Active)
                {
                    countCheck++;
                }

            }
        }
        return countCheck;
    }
    // public int GetClues(TileType[] line)
    // {
    //     List<int> clues = new List<int>();
    //     int count = 0;

    //     foreach (var tile in line)
    //     {
    //         if (tile == TileType.Active) // Giả sử 1 (Active) là ô đen
    //         {
    //             count++;
    //         }
    //         else
    //         {
    //             if (count > 0) clues.Add(count);
    //             count = 0;
    //         }
    //     }
    //     if (count > 0) clues.Add(count);

    //     // Nếu hàng/cột trống, thường Nonogram hiện số 0
    //     if (clues.Count == 0) clues.Add(0); 

    //     return count;
    // }
    public Cell GetCellFromWorldPos(Vector3 worldPos)
    {
        // Tính toán lại vị trí bắt đầu giống hệt trong CreateBoard
        //float startX = -(levelBoardData.width - 1) / 2f;
        //float startY = (levelBoardData.height - 1) / 2f;
        float startX = -(width - 1) / 2f;
        float startY = (height - 1) / 2f;

        // Tính toán chỉ số x, y dựa trên khoảng cách từ điểm chạm đến startPos
        int x = Mathf.RoundToInt(worldPos.x - startX);
        int y = Mathf.RoundToInt(startY - worldPos.y);

        // Kiểm tra xem có nằm trong phạm vi mảng không
        if (x >= 0 && x < levelBoardData.width && y >= 0 && y < levelBoardData.height)
        {
            return cellList[x, y];
        }
        return null;
    }
}
