using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem<TGridObject> {

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;

    //private int[,] gridArray;
    private TextMesh[,] debugTextArray;
    private TGridObject[,] gridArray;

    // About  Func<T,TResult> Delegate
    // https://learn.microsoft.com/en-us/dotnet/api/system.func-2?view=net-8.0
    public GridSystem(int width, int height, float cellSize, Vector3 originPosition, Func<GridSystem<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        //gridArray = new int[width, height];
        gridArray = new TGridObject[width, height];

        // set up gridArray
        //                      for width
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            //                         for height 
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        debugTextArray = new TextMesh[width,height];

        //                      for width
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
        //                         for height 
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    debugTextArray[x,y] = CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize,cellSize) * .5f, 20, Color.white, TextAnchor.MiddleCenter);
                    // horizontal line
                    Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x, y + 1), Color.white,100f);
                    // vertical line
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
        }
        // horizontal line
        Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width,height), Color.white, 100f);
        // vertical line
        Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

        //SetValue(2,1,56); // 13:22
    }

    //Set value

    public void SetValue(int x, int y, TGridObject value)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        gridArray[x, y] = value;
        debugTextArray[x, y].text = gridArray[x, y].ToString();
    }

    public void SetValue(Vector3 worldPositon, TGridObject value)
    {
        int x, y;
        GetXY(worldPositon,out x, out y);
        SetValue(x, y, value);
    }

    // Get value
    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    // 14:20
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public TGridObject GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetValue(Vector3 worldPositon)
    {
        int x, y;
        GetXY(worldPositon, out x, out y);
        return GetValue(x, y);
    }




    //// create text in the world  
    //// https://www.youtube.com/watch?v=waEsGu--9P8   5:48

    // Create Text in the World
    public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 5000)
    {
        if (color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
    }

    // Create Text in the World
    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

}
