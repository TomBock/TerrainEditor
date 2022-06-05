using System.Linq;
 
using UnityEngine;
 
public sealed class TerrainTool : MonoBehaviour
{
    public enum TerrainModificationAction
    {
        Raise,
        Lower,
        Flatten,
        Sample,
        SampleAverage,
    }
 
    public int brushWidth;
    public int brushHeight;
 
    [Range(0.001f, 0.1f)]
    public float strength;
 
    public TerrainModificationAction modificationAction;
 
    private Terrain _targetTerrain;
 
    private float _sampledHeight;
 
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
            {
                if (hit.transform.TryGetComponent(out Terrain terrain)) _targetTerrain = terrain;
 
                switch (modificationAction)
                {
                    case TerrainModificationAction.Raise:
 
                        RaiseTerrain(hit.point, strength, brushWidth, brushHeight);
 
                        break;
 
                    case TerrainModificationAction.Lower:
 
                        LowerTerrain(hit.point, strength, brushWidth, brushHeight);
 
                        break;
 
                    case TerrainModificationAction.Flatten:
 
                        FlattenTerrain(hit.point, _sampledHeight, brushWidth, brushHeight);
 
                        break;
 
                    case TerrainModificationAction.Sample:
 
                        _sampledHeight = SampleHeight(hit.point);
 
                        break;
 
                    case TerrainModificationAction.SampleAverage:
 
                        _sampledHeight = SampleAverageHeight(hit.point, brushWidth, brushHeight);
 
                        break;
                }
            }
        }
    }
 
    private TerrainData GetTerrainData() => _targetTerrain.terrainData;
 
    private int GetHeightmapResolution() => GetTerrainData().heightmapResolution;
 
    private Vector3 GetTerrainSize() => GetTerrainData().size;
 
    public Vector3 WorldToTerrainPosition(Vector3 worldPosition)
    {
        var terrainPosition = worldPosition - _targetTerrain.GetPosition();
 
        var terrainSize = GetTerrainSize();
 
        var heightmapResolution = GetHeightmapResolution();
 
        terrainPosition = new Vector3(terrainPosition.x / terrainSize.x, terrainPosition.y / terrainSize.y, terrainPosition.z / terrainSize.z);
 
        return new Vector3(terrainPosition.x * heightmapResolution, 0, terrainPosition.z * heightmapResolution);
    }
 
    public Vector2Int GetBrushPosition(Vector3 worldPosition, int brushWidth, int brushHeight)
    {
        var terrainPosition = WorldToTerrainPosition(worldPosition);
 
        var heightmapResolution = GetHeightmapResolution();
 
        return new Vector2Int((int)Mathf.Clamp(terrainPosition.x - brushWidth / 2.0f, 0.0f, heightmapResolution), (int)Mathf.Clamp(terrainPosition.z - brushHeight / 2.0f, 0.0f, heightmapResolution));
    }
 
    public Vector2Int GetSafeBrushSize(int brushX, int brushY, int brushWidth, int brushHeight)
    {
        var heightmapResolution = GetHeightmapResolution();
 
        while (heightmapResolution - (brushX + brushWidth) < 0) brushWidth--;
 
        while (heightmapResolution - (brushY + brushHeight) < 0) brushHeight--;
 
        return new Vector2Int(brushWidth, brushHeight);
    }
 
    public void RaiseTerrain(Vector3 worldPosition, float strength, int brushWidth, int brushHeight)
    {
        var brushPosition = GetBrushPosition(worldPosition, brushWidth, brushHeight);
 
        var brushSize = GetSafeBrushSize(brushPosition.x, brushPosition.y, brushWidth, brushHeight);
 
        var terrainData = GetTerrainData();
 
        var heights = terrainData.GetHeights(brushPosition.x, brushPosition.y, brushSize.x, brushSize.y);
 
        for (var y = 0; y < brushSize.y; y++)
        {
            for (var x = 0; x < brushSize.x; x++)
            {
                heights[y, x] += strength * Time.deltaTime;
            }
        }
 
        terrainData.SetHeights(brushPosition.x, brushPosition.y, heights);
    }
 
    public void LowerTerrain(Vector3 worldPosition, float strength, int brushWidth, int brushHeight)
    {
        var brushPosition = GetBrushPosition(worldPosition, brushWidth, brushHeight);
 
        var brushSize = GetSafeBrushSize(brushPosition.x, brushPosition.y, brushWidth, brushHeight);
 
        var terrainData = GetTerrainData();
 
        var heights = terrainData.GetHeights(brushPosition.x, brushPosition.y, brushSize.x, brushSize.y);
 
        for (var y = 0; y < brushSize.y; y++)
        {
            for (var x = 0; x < brushSize.x; x++)
            {
                heights[y, x] -= strength * Time.deltaTime;
            }
        }
 
        terrainData.SetHeights(brushPosition.x, brushPosition.y, heights);
    }
 
    public void FlattenTerrain(Vector3 worldPosition, float height, int brushWidth, int brushHeight)
    {
        var brushPosition = GetBrushPosition(worldPosition, brushWidth, brushHeight);
 
        var brushSize = GetSafeBrushSize(brushPosition.x, brushPosition.y, brushWidth, brushHeight);
 
        var terrainData = GetTerrainData();
 
        var heights = terrainData.GetHeights(brushPosition.x, brushPosition.y, brushSize.x, brushSize.y);
 
        for (var y = 0; y < brushSize.y; y++)
        {
            for (var x = 0; x < brushSize.x; x++)
            {
                heights[y, x] = height;
            }
        }
 
        terrainData.SetHeights(brushPosition.x, brushPosition.y, heights);
    }
 
    public float SampleHeight(Vector3 worldPosition)
    {
        var terrainPosition = WorldToTerrainPosition(worldPosition);
 
        return GetTerrainData().GetInterpolatedHeight((int)terrainPosition.x, (int)terrainPosition.z);
    }
 
    public float SampleAverageHeight(Vector3 worldPosition, int brushWidth, int brushHeight)
    {
        var brushPosition = GetBrushPosition(worldPosition, brushWidth, brushHeight);
 
        var brushSize = GetSafeBrushSize(brushPosition.x, brushPosition.y, brushWidth, brushHeight);
 
        var heights2D = GetTerrainData().GetHeights(brushPosition.x, brushPosition.y, brushSize.x, brushSize.y);
 
        var heights = new float[heights2D.Length];
 
        var i = 0;
 
        for (int y = 0; y <= heights2D.GetUpperBound(0); y++)
        {
            for (int x = 0; x <= heights2D.GetUpperBound(1); x++)
            {
                heights[i++] = heights2D[y, x];
            }
        }
 
        return heights.Average();
    }
}