using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DFD.TerrainEditor
{

    public class TerrainEditorTool : MonoBehaviour
    {
        public InputActionReference ExecuteAction;
        public InputActionReference UndoAction;
        
        public TerrainModificationCommand CurrentCommand;

        public Texture2D BrushTexture;
        public int BrushSize = 5;
        public float BrushStrength = 1f;

        public void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Start new command
                if (TryRaycastTerrain(out var terrain, out RaycastHit _))
                {
                    Debug.LogError($"[TET] Command started on {terrain.name}");
                    var brush = new TerrainBrush(BrushTexture, this);
                    CurrentCommand = new RaiseCommand(terrain.terrainData, brush);
                }
            } else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // End command
                Debug.LogError($"[TET] Command ended");
                CurrentCommand = null;
            }

            // Undo
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Debug.LogError($"[TET] Undo command");
                CurrentCommand?.Undo();
            }

            // Continue current command
            if (Mouse.current.leftButton.isPressed)
            {
                if (TryRaycastTerrain(out var terrain, out var hit))
                {
                    var (terrainX, terrainZ) = terrain.GetTerrainCoordinates(hit, CurrentCommand.Brush.Size);
                    CurrentCommand.Execute(terrainX, terrainZ);
                }
            }
        }

        private bool TryRaycastTerrain(out UnityEngine.Terrain terrain, out RaycastHit hit)
        {
            terrain = null;
            return Physics.Raycast(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), out hit) &&
                   hit.transform.TryGetComponent <UnityEngine.Terrain>(out terrain);
        }
    }

    public class TerrainBrush
    {
        private TerrainEditorTool _terrainEditorTool;
        private Texture2D _brushTexture;
        private float[,] _heightMap;
        public float this[int x, int y] => _heightMap[x, y];

        public int Size => _terrainEditorTool.BrushSize;
        public float Strength => _terrainEditorTool.BrushStrength;
        
        public TerrainBrush(Texture2D texture, TerrainEditorTool terrainEditorTool)
        {
            _terrainEditorTool = terrainEditorTool;

            _brushTexture = texture.Rescale(Size, Size);
            GenerateHeightMap();
        }
        
        /// <summary>
        /// Creates a 2d array which will store the brush data
        /// </summary>
        private void GenerateHeightMap()
        {
            _heightMap = new float[Size, Size];
            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    var pixelValue = _brushTexture.GetPixel(x, y);
                    _heightMap[x, y] = pixelValue.grayscale / 255f;
                }
            }
        }

    }


    public abstract class TerrainModificationCommand
    {
        protected float[,] _heights;
        protected float[,] _startHeights;
        protected TerrainData _terrainData;
        public TerrainBrush Brush;
        
        public TerrainModificationCommand(TerrainData terrainData, TerrainBrush brush)
        {
            Brush = brush;
            _terrainData = terrainData;

            _startHeights = _terrainData.GetHeights(
                0,
                0,
                _terrainData.heightmapResolution,
                _terrainData.heightmapResolution);
        }

        public abstract void Execute(int x, int z);

        public void Undo()
        {
            _terrainData.SetHeights(0, 0, _startHeights);
        }
    }

    public class RaiseCommand : TerrainModificationCommand
    {
        public RaiseCommand(TerrainData terrainData, TerrainBrush brush) : base(terrainData, brush) {}
        
        public override void Execute(int x, int z)
        {
            //int xMin = 0;
            //int xMax = 0;
            //int zMin = 0;
            //int zMax = 0;

            //if (x < 0)
            //    xMin = x;
            //else if (x + Brush.size > _terrainData.heightmapResolution)
            //    xMax = x + Brush.size - _terrainData.heightmapResolution;

            //if (z < 0)
            //    zMin = z;
            //else if (z + Brush.size > _terrainData.heightmapResolution)
            //    zMax = z + Brush.size - _terrainData.heightmapResolution;
            
            //_heights = _terrainData.GetHeights(
            //    x - xMin,
            //    z - zMin,
            //    Brush.size + xMin - xMax,
            //    Brush.size + zMin - zMax);
            
            if (x < 0)
                x = 0;
            else if (x + Brush.Size > _terrainData.heightmapResolution)
                x = _terrainData.heightmapResolution - Brush.Size;

            if (z < 0)
                z = 0;
            else if (z + Brush.Size > _terrainData.heightmapResolution)
                z = _terrainData.heightmapResolution - Brush.Size;

            Debug.LogError($"[TET:Raise] Execute ({x}, {z}) ");

            try
            {

                _heights = _terrainData.GetHeights(x, z, Brush.Size, Brush.Size);
            }
            catch (Exception e)
            {
                Debug.LogError($"ERROR {e}");
            }
            
            // Apply raise
            for (int xx = 0; xx < Brush.Size; xx++)
            {
                for (int yy = 0; yy < Brush.Size; yy++)
                {
                    _heights[xx, yy] += Brush[xx, yy] * Brush.Strength;
                } 
            }
            _terrainData.SetHeights(x, z, _heights);
        }
    }

    public class LowerCommand : TerrainModificationCommand
    {
        public LowerCommand(TerrainData terrainData, TerrainBrush brush) : base(terrainData, brush) {}

        public override void Execute(int x, int z)
        {
            // Apply raise
            for (int xx = 0; xx < Brush.Size; xx++)
            {
                for (int yy = 0; yy < Brush.Size; yy++)
                {
                    _heights[xx, yy] -= Brush[xx, yy] * Brush.Strength;
                } 
            }
            _terrainData.SetHeights(x, z, _heights);
        }
    }
}