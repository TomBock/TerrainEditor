using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeTerrainEditor
{    
    public class InputController
    {
        private RuntimeTerrain _runtimeTerrain;
        private TerrainData _terrainData;
        private ICommand _modifyCommand;

        public InputController(RuntimeTerrain runtimeTerrain)
        {
            _runtimeTerrain = runtimeTerrain;
            _terrainData = _runtimeTerrain.targetTerrain.terrainData;
            _modifyCommand = null;
        }

        public void ListenInputs()
        {
            //  do not start to modify if its over a ui object
            if (EventSystem.current.IsPointerOverGameObject() == false)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    TerrainModifyStarted();
                }

                if (Input.GetMouseButton(0))
                {
                    _runtimeTerrain.UseBrush();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                TerrainModifyEnded();
            }

            //  Listen for undo shortcut
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
            {
                CommandHistory.Undo();
            }

            //  Listen for redo shortcut
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
            {
                CommandHistory.Redo();
            }
        }

        private void TerrainModifyEnded()
        {
            if (_modifyCommand != null)
            {
                _modifyCommand.Complete();
                CommandHistory.Register(_modifyCommand);
            }

            _modifyCommand = null;
        }

        private void TerrainModifyStarted()
        {
            switch (_runtimeTerrain.BrushEffectMode)
            {
                case BrushEffectMode.RAISE:
                case BrushEffectMode.LOWER:
                case BrushEffectMode.SMOOTH:
                case BrushEffectMode.FLATTEN:
                {
                    _modifyCommand = new ModifyHeightsCommand(_terrainData);
                }
                break;
                case BrushEffectMode.PAINT:
                {
                    _modifyCommand = new ModifySplatsCommand(_terrainData);
                }
                break;
                case BrushEffectMode.OBJECT_ADD:
                case BrushEffectMode.OBJECT_REMOVE:
                {
                    _modifyCommand = new ModifyTreesCommand(_terrainData);
                }
                break;
            }

        }

    }
}