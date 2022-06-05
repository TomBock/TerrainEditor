using UnityEngine;
using UnityEngine.UI;

namespace RuntimeTerrainEditor
{
    public class SelectionItem : MonoBehaviour
    {
        public int index;
        public RawImage image;
        public Button selectButton;

        public void ClearSelection()
        {
            selectButton.image.color = Color.white;
        }

        public void Select()
        {
            selectButton.image.color = Color.green;
        }
    }
}