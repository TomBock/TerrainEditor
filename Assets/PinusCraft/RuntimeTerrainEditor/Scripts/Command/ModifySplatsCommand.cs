using UnityEngine;

namespace RuntimeTerrainEditor
{    
    public class ModifySplatsCommand : ICommand
    {
        private float[,,] _startSplats;
        private float[,,] _endSplats;
        
        private TerrainData _terrainData;

        public ModifySplatsCommand(TerrainData terrainData)
        {
            _terrainData = terrainData;
            _startSplats = _terrainData.GetAlphamaps  (0, 0, _terrainData.alphamapWidth, _terrainData.alphamapHeight);
        }

        public void Complete()
        {
            _endSplats = _terrainData.GetAlphamaps  (0, 0, _terrainData.alphamapWidth, _terrainData.alphamapHeight);
        }

        public void Execute()
        {
            _terrainData.SetAlphamaps(0,0,_endSplats);
        }

        public void Undo()
        {
            _terrainData.SetAlphamaps(0,0,_startSplats);
        }
    }
}