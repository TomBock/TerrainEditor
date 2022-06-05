using System;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeTerrainEditor
{
    public class ToolView : MonoBehaviour
    {
        public Transform brushSelectionParent;
        public Transform paintLayerSelectionParent;
        public Transform objectSelectionParent;
        
        public GameObject selectionItemPrefab;
        
        public Dropdown modeDropdown;
        public Dropdown terrainSizeDropdown;
        public Slider sizeSlider;
        public Slider strengthSlider;
        public Button resetButton;
        public Slider flattenSlider;

        public Button undoButton;
        public Button redoButton;


        public GameObject paintGroup;
        public GameObject objectGroup;
        public GameObject flattenGroup;

        public SelectionItem CreateBrushSelectionItem()
        {
            return Instantiate(selectionItemPrefab, brushSelectionParent).GetComponent<SelectionItem>();
        }

        public SelectionItem CreatePaintLayerSelectionItem()
        {
            return Instantiate(selectionItemPrefab, paintLayerSelectionParent).GetComponent<SelectionItem>();
        }

        public SelectionItem CreateObjectSelectionItem()
        {
            return Instantiate(selectionItemPrefab, objectSelectionParent).GetComponent<SelectionItem>();
        }
    }
}