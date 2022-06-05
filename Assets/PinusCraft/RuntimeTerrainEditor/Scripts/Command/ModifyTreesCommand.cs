using System.Collections.Generic;
using UnityEngine;

namespace RuntimeTerrainEditor
{    
    public class ModifyTreesCommand : ICommand
    {
        private List<TreeInstance> _startTrees;
        private List<TreeInstance> _endTrees;
        
        private TerrainData _terrainData;

        public ModifyTreesCommand(TerrainData terrainData)
        {
            _terrainData = terrainData;
            _startTrees = new List<TreeInstance>(_terrainData.treeInstances);
        }

        public void Complete()
        {
            _endTrees = new List<TreeInstance>(_terrainData.treeInstances);
        }

        public void Execute()
        {
            _terrainData.treeInstances = _endTrees.ToArray();
        }

        public void Undo()
        {
            _terrainData.treeInstances = _startTrees.ToArray();
        }
    }
}