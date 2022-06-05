using UnityEngine;

namespace RuntimeTerrainEditor
{    
    public class ModifyHeightsCommand : ICommand
    {
        private float[,] _startHeights;
        private float[,] _endHeights;
        
        private TerrainData _terrainData;

        public ModifyHeightsCommand(TerrainData terrainData)
        {
            _terrainData = terrainData;
            _startHeights = _terrainData.GetHeights(0, 0, _terrainData.heightmapResolution, _terrainData.heightmapResolution);
        }

        public void Complete()
        {
            _endHeights = _terrainData.GetHeights(0, 0, _terrainData.heightmapResolution, _terrainData.heightmapResolution);  
        }

        public void Execute()
        {
            _terrainData.SetHeights(0,0,_endHeights);
        }

        public void Undo()
        {
            _terrainData.SetHeights(0,0,_startHeights);
        }
    }
}