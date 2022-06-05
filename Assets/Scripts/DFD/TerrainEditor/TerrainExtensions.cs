using UnityEngine;

namespace DFD.TerrainEditor
{

    public static class TerrainExtensions
    {
        public static (int x, int z) GetTerrainCoordinates(this UnityEngine.Terrain terrain, RaycastHit hit, int brushSize)
        {
            //World Position Offset Coords, these can differ from the terrain coords if the terrain object is not at (0,0,0)
            int offset = brushSize / 2; 
            //This offsets the hit position to account for the size of the brush which gets drawn from the corner out
            Vector3 tempTerrainCoodinates = hit.point - hit.transform.position;
            
            //This takes the world coords and makes them relative to the terrain
            var terrainData = terrain.terrainData;
            Vector3 terrainCoordinates = new Vector3(tempTerrainCoodinates.x / terrainData.size.x,
                                                     tempTerrainCoodinates.y / terrainData.size.y,
                                                     tempTerrainCoodinates.z / terrainData.size.z);
            // This will take the coords relative to the terrain and make them relative to the height map(which often has different dimensions)
            Vector3 locationInTerrain = new Vector3(terrainCoordinates.x * terrainData.heightmapResolution,
                                                    0,
                                                    terrainCoordinates.z * terrainData.heightmapResolution);
            //Finally, this will spit out the X Y values for use in other parts of the code
            var x = (int)locationInTerrain.x - offset;
            var z = (int)locationInTerrain.z - offset;
            return (x, z);
        }
    }

}
