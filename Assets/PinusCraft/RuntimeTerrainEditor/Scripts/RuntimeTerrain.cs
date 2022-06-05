using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuntimeTerrainEditor
{
    public class RuntimeTerrain : MonoBehaviour
    {
        public BrushEffectMode      BrushEffectMode         { get { return _brushEffect; } }
        public int                  BrushSize               { get { return _brushSize; } }
        public int                  BrushIndex              { get { return _brushIndex; } }
        public int                  PaintLayerIndex         { get { return _paintLayerIndex; } }
        public int                  ObjectIndex             { get { return _objectIndex; } }
        public float                FlattenHeight           { get { return _flattenHeight; } }


        public Terrain              targetTerrain;
        public GlobalSettings       settings;
        
        //  brush
        private Texture2D[]         _brushTextures;         //  This will allow you to switch brushes
        private int                 _brushSize;             
        private int                 _brushIndex;            
        private float               _brushStrength;         
        private BrushEffectMode     _brushEffect;

        //  flatten
        private float               _flattenHeight;         //  the height to which the flatten mode will go
        
        //  paint
        private TerrainLayer[]      _paintLayers;           //  a list containing all of the paints
        private int                 _paintLayerIndex;       

        //  object                 
        private TreePrototype[]     _objectLayers;          //  object prefabs will be registered as TreePrototypes in target terrain
        private int                 _objectIndex;      
        private float               _objectDensity;         
        private float               _objectHeightMin;
        private float               _objectHeightMax;
        private float               _objectWidthMin;
        private float               _objectWidthMax;

        //  terrain
        private static TerrainData  _terrainData;
        private static float[,]     _heights;               //  a variable to store the new terrain heights
        private static float[,]     _brush;                 //  this stores the brush textures pixel data
        private static float[,,]    _splat;                 //  A splat map is what unity uses to overlay all of your paints on to the terrain
        
        //  raycasting
        private static Camera       _cam;
        private static Ray          _ray;
        private static RaycastHit   _hit;


        public void Init()
        {
            _terrainData             = targetTerrain.terrainData;

            _paintLayers             = settings.paintLayers;
            _paintLayerIndex         = 0;

            _flattenHeight           = settings.flattenHeightDefault;

            _brushStrength           = settings.brushStrengthDefault;
            _brushSize               = settings.brushSizeDefault;
            _brushIndex              = 0;
            _brushTextures           = settings.brushTextures;
            _brush                   = GenerateBrush(_brushTextures[_brushIndex], _brushSize); // This will take the brush image from our array and will resize it to the area of effect
        
            _objectHeightMin         = settings.randomObjectHeightMin; 
            _objectHeightMax         = settings.randomObjectHeightMax; 
            _objectWidthMin          = settings.randomObjectWidthMin; 
            _objectWidthMax          = settings.randomObjectWidthtMax; 
            _objectLayers            = new TreePrototype[settings.objectPrefabs.Length];
            
            //  create tree prototypes
            for (int i = 0; i < _objectLayers.Length; i++)
            {
                var prototype = new TreePrototype();
                prototype.prefab = settings.objectPrefabs[i];
                _objectLayers[i] = prototype;
            }

            //  assign to terrain
            _terrainData.treePrototypes = _objectLayers;
            _terrainData.terrainLayers  = _paintLayers;

            _cam = Camera.main;
        }

        public void UseBrush()
        {
            _ray = _cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast (_ray, out _hit))
            {
                if (_hit.transform != null)
                {
                    GetTerrainCoordinates(_hit, out int terX, out int terZ);
                    ModifyTerrain(terX, terZ);
                    ModifyTree(_hit);
                }
            }
        }  

        public void SetBrushSize(int value)
        {
            _brushSize = value;
            _brush = GenerateBrush(_brushTextures[_brushIndex], _brushSize); 
        }

        public void SetBrushStrength(float value)
        {
            _brushStrength = value;
        }

        public void SetFlattenHeight(float value)
        {
            _flattenHeight = value;
        }

        public void SetBrushIndex(int index)
        {
            _brushIndex = index;

            //  refresh brush data with new index
            _brush = GenerateBrush(_brushTextures[_brushIndex], _brushSize);
        }

        public void SetPaintLayerIndex(int index)
        {
            _paintLayerIndex = index;
        }

        public void SetObjectIndex(int index)
        {
            _objectIndex = index;
        }

        public void SetObjectDensity(float density)
        {
            _objectDensity = density;
        }

        public void SetMode(BrushEffectMode mode)
        {
            _brushEffect = mode;
        }

        public void SetTerrainSize(int size)
        {
            //  update the size of necessary terrain fields
            //  this way, brush effect will be the same for 
            //  all map sizes

            _terrainData.heightmapResolution    = size;
            _terrainData.alphamapResolution     = size;
            _terrainData.baseMapResolution      = size;
            _terrainData.size                   = new Vector3(size, _terrainData.size.y, size);
             
            Reset();
            
            //  since map size changed, command history values are invalid now
            CommandHistory.Clear();
            
        }

        public void Reset()
        {
            _terrainData.SetHeights(0,0,new float[_terrainData.heightmapResolution, _terrainData.heightmapResolution]);
            _terrainData.terrainLayers = new TerrainLayer[]{};
            _terrainData.terrainLayers = _paintLayers;
            _terrainData.SetTreeInstances(new TreeInstance[]{}, true);
            targetTerrain.Flush();
        }

        private void GetTerrainCoordinates(RaycastHit hit, out int x,out int z)
        {
            //World Position Offset Coords, these can differ from the terrain coords if the terrain object is not at (0,0,0)
            int offset = _brushSize / 2; 
            //This offsets the hit position to account for the size of the brush which gets drawn from the corner out
            Vector3 tempTerrainCoodinates = hit.point - hit.transform.position;
            //This takes the world coords and makes them relative to the terrain
            Vector3 terrainCoordinates = new Vector3(tempTerrainCoodinates.x / _terrainData.size.x,
                                                    tempTerrainCoodinates.y / _terrainData.size.y,
                                                    tempTerrainCoodinates.z / _terrainData.size.z);
            // This will take the coords relative to the terrain and make them relative to the height map(which often has different dimensions)
            Vector3 locationInTerrain = new Vector3(terrainCoordinates.x * _terrainData.heightmapResolution,
                                                    0,
                                                    terrainCoordinates.z * _terrainData.heightmapResolution);
            //Finally, this will spit out the X Y values for use in other parts of the code
            x = (int)locationInTerrain.x - offset;
            z = (int)locationInTerrain.z - offset;
        }
        
        private float GetSurroundingHeights(float[,] height, int x, int z)
        {
            float value; 
            // we will add all the heights to this and divide by int num bellow to get the average height
            float avg = height[x, z]; 
            int num = 1;
            //this will loop us through the possible surrounding spots
            for (int i = 0; i < 4; i++) 
            {
                //  This will try to run the code bellow, 
                //  and if one of the coords is not on the terrain(ie we are at an edge) it will pass the exception to the Catch{} below
                try 
                {
                    // These give us the values surrounding the point
                    if (i == 0)
                    {value = height[x + 1, z];}
                    else if (i == 1)
                    {value = height[x - 1, z];}
                    else if (i == 2)
                    {value = height[x, z + 1];}
                    else
                    {value = height[x, z - 1];}
                    
                    // keeps track of how many iterations were successful  
                    num++; 
                    avg += value;
                }
                catch (System.Exception)
                {
                }
            }
            avg = avg / num;
            return avg;
        }

        private float[,] GenerateBrush(Texture2D texture, int size)
        {
            //  Creates a 2d array which will store our brush
            float[,] heightMap = new float[size,size];
            Texture2D scaledBrush = ResizeBrush(texture,size,size); // this calls a function which we will write next, and resizes the brush image
            //  This will iterate over the entire re-scaled image and convert the pixel color into a value between 0 and 1
            for (int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    Color pixelValue = scaledBrush.GetPixel(x, y);
                    heightMap[x, y] = pixelValue.grayscale / 255F;
                }
            }
            
            return heightMap;
        }
        
        private static Texture2D ResizeBrush(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            ScaleTexture(src, width, height, mode);
            //  Get rendered data back to a new texture
            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }
        
        private static void ScaleTexture(Texture2D src, int width, int height, FilterMode fmode)
        {
            //  We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);
            //  Using RTT for best quality and performance.
            RenderTexture rtt = new RenderTexture(width, height, 32);
            //  Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);
            //  Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);
            //  Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }

        private void ModifyTerrain(int x, int z)
        {
            //  AreaOfEffectModifier variables below will help us if we are modifying 
            //  terrain that goes over the edge
            int AOExModMin = 0;
            int AOEzModMin = 0;
            int AOExModMax = 0;
            int AOEzModMax = 0;
            
            // if the brush goes off the negative end of the x axis we set the mod == to it to offset the edited area
            if (x < 0) 
            {
                AOExModMin = x;
            }
            // if the brush goes off the positive end of the x axis we set the mod == to this
            else if (x + _brushSize > _terrainData.heightmapResolution)
            {
                AOExModMax = x + _brushSize - _terrainData.heightmapResolution;
            }
            
            if (z < 0)
            {
                AOEzModMin = z;
            }
            else if (z + _brushSize > _terrainData.heightmapResolution)
            {
                AOEzModMax = z + _brushSize - _terrainData.heightmapResolution;
            }
            
            // the following code will apply the terrain height modifications
            if (_brushEffect != BrushEffectMode.PAINT) 
            {
                // this grabs the heightmap values within the brushes area of effect
                _heights = _terrainData.GetHeights(x - AOExModMin,
                                                z - AOEzModMin,
                                                _brushSize + AOExModMin - AOExModMax,
                                                _brushSize + AOEzModMin - AOEzModMax);
            }

            switch (_brushEffect)
            {
                case BrushEffectMode.RAISE: 
                {
                    for (int xx = 0; xx < _brushSize + AOEzModMin - AOEzModMax; xx++)
                    {
                        for (int yy = 0; yy < _brushSize + AOExModMin - AOExModMax; yy++)
                        {
                            //for each point we raise the value  by the value of brush at the coords * the strength modifier
                            _heights[xx, yy] += _brush[xx-AOEzModMin, yy-AOExModMin] * _brushStrength; 
                        }
                    }

                    // This bit of code will save the change to the Terrain data file, 
                    // this means that the changes will persist out of play mode into the edit mode
                    _terrainData.SetHeights(x - AOExModMin, z - AOEzModMin, _heights);
                }
                break;
                case BrushEffectMode.LOWER: 
                {
                    for (int xx = 0; xx < _brushSize + AOEzModMin - AOEzModMax; xx++)
                    {
                        for (int yy = 0; yy < _brushSize + AOExModMin - AOExModMax; yy++)
                        {
                            _heights[xx, yy] -= _brush[xx - AOEzModMin, yy - AOExModMin] * _brushStrength;
                        }
                    }
                    _terrainData.SetHeights(x - AOExModMin, z - AOEzModMin, _heights);
                }
                break;
                case BrushEffectMode.FLATTEN: 
                {
                    for (int xx = 0; xx < _brushSize + AOEzModMin - AOEzModMax; xx++)
                    {
                        for (int yy = 0; yy < _brushSize + AOExModMin - AOExModMax; yy++)
                        {
                            // moves the points towards their targets
                            _heights[xx, yy] = Mathf.MoveTowards(_heights[xx, yy],  _flattenHeight * Constants.FLATTEN_STROKE_MULTIPLIER, _brush[xx - AOEzModMin, yy - AOExModMin] * _brushStrength);
                        }
                    }
                    _terrainData.SetHeights(x - AOExModMin, z - AOEzModMin, _heights);
                }
                break;
                case BrushEffectMode.SMOOTH: 
                {
                    float[,] heightAvg = new float[_heights.GetLength(0), _heights.GetLength(1)];
                    for (int xx = 0; xx < _brushSize + AOEzModMin - AOEzModMax; xx++)
                    {
                        for (int yy = 0; yy < _brushSize + AOExModMin - AOExModMax; yy++)
                        {
                            // calculates the value we want each point to move towards
                            heightAvg[xx, yy] = GetSurroundingHeights(_heights, xx, yy);
                        }
                    }
                    for (int xx1 = 0; xx1 < _brushSize + AOEzModMin - AOEzModMax; xx1++)
                    {
                        for (int yy1 = 0; yy1 < _brushSize + AOExModMin - AOExModMax; yy1++)
                        {
                            // moves the points towards their targets
                            _heights[xx1, yy1] = Mathf.MoveTowards(_heights[xx1, yy1], heightAvg[xx1, yy1], _brush[xx1 - AOEzModMin, yy1 - AOExModMin] * _brushStrength); 
                        }
                    }
                    _terrainData.SetHeights(x - AOExModMin, z - AOEzModMin, _heights);
                }
                break;
                case BrushEffectMode.PAINT: 
                {
                    int splatX = x - AOExModMin;
                    int splatY = z - AOEzModMin;
                    
                    int splatWidth    = Mathf.Clamp(_brushSize + AOEzModMin - AOEzModMax, 0, _terrainData.alphamapResolution);
                    int splatHeight   = Mathf.Clamp(_brushSize + AOExModMin - AOExModMax, 0, _terrainData.alphamapResolution);

                    //  consider brush size before fetching splat
                    if (splatX + splatWidth > _terrainData.alphamapResolution)
                    {
                        splatWidth -= ((splatX + splatWidth) - _terrainData.alphamapResolution);
                    }

                    if (splatY + splatHeight > _terrainData.alphamapResolution)
                    {
                        splatHeight -= ((splatY + splatHeight) - _terrainData.alphamapResolution);
                    }

                    //grabs the splat map data for our brush area
                    _splat = _terrainData.GetAlphamaps(splatX,
                                                       splatY,
                                                       splatWidth,
                                                       splatHeight); 

                    for (int xx = 0; xx < _brushSize + AOEzModMin - AOEzModMax; xx++)
                    {
                        for (int yy = 0; yy < _brushSize + AOExModMin - AOExModMax; yy++)
                        {
                            //creates a float array and sets the size to be the number of paints your terrain has
                            float[] weights = new float[_terrainData.alphamapLayers]; 
                            for (int zz = 0; zz < _splat.GetLength(2); zz++)
                            {
                                //grabs the weights from the terrains splat map
                                int k = Mathf.Clamp(xx, 0, _splat.GetLength(0)-1);
                                int l = Mathf.Clamp(yy, 0, _splat.GetLength(1)-1);
                                int m = Mathf.Clamp(zz, 0, _splat.GetLength(2)-1);

                                weights[zz] = _splat[k, l, m];
                            }
                            // adds weight to the paint currently selected with the int paint variable
                            weights[_paintLayerIndex] += _brush[xx - AOEzModMin, yy - AOExModMin] * _brushStrength * Constants.PAINT_STROKE_MULTIPLIER; 
                            //this next bit normalizes all the weights so that they will add up to 1
                            float sum = weights.Sum();
                            for (int ww = 0; ww < weights.Length; ww++)
                            {
                                if (xx < _splat.GetLength(0) && yy < _splat.GetLength(1))
                                {
                                    weights[ww] /= sum;
                                    _splat[xx, yy, ww] = weights[ww];
                                }
                            }
                        }
                    }
                    //applies the changes to the terrain, they will also persist
                    _terrainData.SetAlphamaps(x - AOExModMin, z - AOEzModMin, _splat);
                    targetTerrain.Flush();
                }
                break;

            }
        }

        private void ModifyTree(RaycastHit hit)
        {
            switch (_brushEffect)
            {
                case BrushEffectMode.OBJECT_ADD:
                {
                    Vector2 randomOffset = 0.5f * UnityEngine.Random.insideUnitCircle;
                    randomOffset.x *= _brushSize / _terrainData.size.x;
                    randomOffset.y *= _brushSize / _terrainData.size.z;

                    var pos = Vector3.zero;
                    pos.x = _hit.point.x/_terrainData.size.x + randomOffset.x;
                    pos.z = _hit.point.z/_terrainData.size.z + randomOffset.y;

                    float spacing = 1/(_brushStrength*500);
                    if (pos.x >= 0 && pos.x <= 1 && pos.z >= 0 && pos.z <= 1 && CheckTreeDistance(pos, spacing))
                    {
                        var instance = new TreeInstance()
                        {
                            position        = pos,
                            heightScale     = UnityEngine.Random.Range(_objectHeightMin, _objectHeightMax),
                            widthScale      = UnityEngine.Random.Range(_objectWidthMin, _objectWidthMax),
                            rotation        = UnityEngine.Random.Range(0, 2 * Mathf.PI),
                            color           = Color.white,
                            lightmapColor   = Color.white,
                            prototypeIndex  = _objectIndex
                        };
                        
                        targetTerrain.AddTreeInstance(instance);
                    }

                }
                break;

                case BrushEffectMode.OBJECT_REMOVE: 
                {
                    var pos = Vector3.zero;
                    pos.x = _hit.point.x/_terrainData.heightmapResolution;
                    pos.z = _hit.point.z/_terrainData.heightmapResolution;

                    var range = (float)_brushSize / _terrainData.heightmapResolution;

                    var instances = new List<TreeInstance>(_terrainData.treeInstances);
                    var total = _terrainData.treeInstanceCount;
                    for (int i = 0; i < total; i++)
                    {
                        var instance = _terrainData.GetTreeInstance(i);
                        if (Vector3.Distance(instance.position, pos) < range)
                        {
                            instances.RemoveAt(i);
                            _terrainData.SetTreeInstances(instances.ToArray(), true);
                            break;
                        }
                    }

                }
                break;
            }
        }

        private static bool CheckTreeDistance(Vector3 pos, float spacing)
        {
            var total = _terrainData.treeInstanceCount;
            for (int i = 0; i < total; i++)
            {
                var instance = _terrainData.GetTreeInstance(i);
                var dist = Vector3.Distance(instance.position, pos);
                if (dist < spacing)
                {
                    return false;
                }
            }

            return true;
        }
    }
}