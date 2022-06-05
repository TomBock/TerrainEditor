using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeTerrainEditor
{
    public class ViewController : MonoBehaviour
    {
        public ToolView toolView;

        private RuntimeTerrain          _runtimeTerrain;

        private int[]                   _terrainSizes;
        private BrushEffectMode[]       _modes;
        private SelectionItem[]         _brushSelections;
        private SelectionItem[]         _paintLayerSelections;
        private SelectionItem[]         _objectSelections;

        public void Init(RuntimeTerrain editor)
        {
            _runtimeTerrain = editor;
            _modes = Enum.GetValues(typeof(BrushEffectMode)) as BrushEffectMode[];
            _terrainSizes = Enum.GetValues(typeof(TerrainSize)) as int[];

            SetupTool();
        }

        private void SetupTool()
        {
            //  brush
            _brushSelections = new SelectionItem[_runtimeTerrain.settings.brushTextures.Length];
            for (int i = 0; i < _brushSelections.Length; i++)
            {
                var item                = toolView.CreateBrushSelectionItem();
                item.index              = i;
                item.image.texture      = _runtimeTerrain.settings.brushTextures[i];
                item.selectButton.onClick.AddListener(()=>SetBrushSelected(item));

                _brushSelections[i] = item;
            }

            //  paint
            _paintLayerSelections = new SelectionItem[_runtimeTerrain.settings.paintLayers.Length];
            for (int i = 0; i < _paintLayerSelections.Length; i++)
            {
                var item                = toolView.CreatePaintLayerSelectionItem();
                item.index              = i;
                item.image.texture      = _runtimeTerrain.settings.paintLayers[i].diffuseTexture;
                item.selectButton.onClick.AddListener(()=>SetPaintLayerSelected(item));

                _paintLayerSelections[i] = item;
            }

            //  object
            _objectSelections = new SelectionItem[_runtimeTerrain.settings.objectPrefabs.Length];
            for (int i = 0; i < _objectSelections.Length; i++)
            {
                var item                = toolView.CreateObjectSelectionItem();
                item.index              = i;
                item.image.texture      = _runtimeTerrain.settings.GetThumbnailAtIndex(i);
                item.selectButton.onClick.AddListener(()=>SetObjectSelected(item));

                _objectSelections[i] = item;
            }

            //  mode
            toolView.modeDropdown.ClearOptions();
            for (int i = 0; i < _modes.Length; i++)
            {
                var od = new Dropdown.OptionData(_modes[i].ToString());
                toolView.modeDropdown.options.Add(od);
            }
            toolView.modeDropdown.onValueChanged.AddListener(OnModeSelected);
            toolView.modeDropdown.RefreshShownValue();

            //  terrain size
            toolView.terrainSizeDropdown.ClearOptions();
            for (int i = 0; i < _terrainSizes.Length; i++)
            {
                var od = new Dropdown.OptionData(_terrainSizes[i].ToString());
                toolView.terrainSizeDropdown.options.Add(od);
            }
            toolView.terrainSizeDropdown.onValueChanged.AddListener(OnSizeSelected);
            toolView.terrainSizeDropdown.RefreshShownValue();

            //  brush size
            toolView.sizeSlider.minValue        = _runtimeTerrain.settings.brushSizeMin;
            toolView.sizeSlider.maxValue        = _runtimeTerrain.settings.brushSizeMax;
            toolView.sizeSlider.value           = _runtimeTerrain.BrushSize;
            toolView.sizeSlider.onValueChanged.AddListener(OnBrushSizeChanged);

            //  strength
            toolView.strengthSlider.minValue    = 0;
            toolView.strengthSlider.maxValue    = Constants.MAX_BRUSH_STRENGTH;
            toolView.strengthSlider.value       = _runtimeTerrain.settings.brushStrengthDefault;
            toolView.strengthSlider.onValueChanged.AddListener(OnBrushStrengthChanged);

            //  flatten
            toolView.flattenSlider.minValue     = _runtimeTerrain.settings.flattenHeightMin;
            toolView.flattenSlider.maxValue     = _runtimeTerrain.settings.flattenHeightMax;
            toolView.flattenSlider.value        = _runtimeTerrain.FlattenHeight;
            toolView.flattenSlider.onValueChanged.AddListener(OnFlattenValueChanged);

            //  reset
            toolView.resetButton.onClick.AddListener(OnReset);
            toolView.undoButton.onClick.AddListener(OnUndo);
            toolView.redoButton.onClick.AddListener(OnRedo);

            //  set initial values to view
            SetBrushSelected(_brushSelections[_runtimeTerrain.BrushIndex]);
            SetPaintLayerSelected(_paintLayerSelections[_runtimeTerrain.PaintLayerIndex]);
            SetObjectSelected(_objectSelections[_runtimeTerrain.ObjectIndex]);
            OnModeSelected((int)_runtimeTerrain.BrushEffectMode);
        }

        private void OnBrushStrengthChanged(float value)
        {
            _runtimeTerrain.SetBrushStrength(value);
        }

        private void OnFlattenValueChanged(float value)
        {
            _runtimeTerrain.SetFlattenHeight(value);
        }

        private void OnBrushSizeChanged(float value)
        {
            _runtimeTerrain.SetBrushSize((int)value);
        }

        private void OnModeSelected(int index)
        {
            _runtimeTerrain.SetMode(_modes[index]);

            toolView.flattenGroup.SetActive(false);
            toolView.paintGroup.SetActive(false);
            toolView.objectGroup.SetActive(false);
        
            switch (_runtimeTerrain.BrushEffectMode)
            {
                case BrushEffectMode.FLATTEN:
                {
                    toolView.flattenGroup.SetActive(true);
                }
                break;
                case BrushEffectMode.PAINT:
                {
                    toolView.paintGroup.SetActive(true);
                }
                break;
                case BrushEffectMode.OBJECT_ADD:
                {
                    toolView.objectGroup.SetActive(true);
                }
                break;
            }
        }

        private void OnSizeSelected(int index)
        {
            _runtimeTerrain.SetTerrainSize(_terrainSizes[index]);
        }

        private void OnReset()
        {
            _runtimeTerrain.Reset();
        }

        private void OnRedo()
        {
            CommandHistory.Redo();
        }

        private void OnUndo()
        {
            CommandHistory.Undo();
        }

        private void SetBrushSelected(SelectionItem selection)
        {
            foreach (var item in _brushSelections)
            {
                item.ClearSelection();
            }

            _brushSelections[selection.index].Select();
            _runtimeTerrain.SetBrushIndex(selection.index);
        }

        private void SetPaintLayerSelected(SelectionItem selection)
        {
            foreach (var item in _paintLayerSelections)
            {
                item.ClearSelection();
            }

            _paintLayerSelections[selection.index].Select();
            _runtimeTerrain.SetPaintLayerIndex(selection.index);
        }

        private void SetObjectSelected(SelectionItem selection)
        {
            foreach (var item in _objectSelections)
            {
                item.ClearSelection();
            }

            _objectSelections[selection.index].Select();
            _runtimeTerrain.SetObjectIndex(selection.index);
        }

    }
}
