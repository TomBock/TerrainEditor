using UnityEngine;

namespace RuntimeTerrainEditor
{
    public class ApplicationStart : MonoBehaviour
    {
        public RuntimeTerrain runtimeTerrain;
        public CameraController cameraController;
        public ViewController viewController;

        private InputController _inputController;

        private void Start()
        {
            Application.targetFrameRate = 60;

            runtimeTerrain.Init();
            cameraController.Init();
            viewController.Init(runtimeTerrain);
            
            _inputController = new InputController(runtimeTerrain);

            runtimeTerrain.SetTerrainSize((int)TerrainSize.Size128);
        }

        private void Update()
        {
            cameraController.ListenInputs();
            _inputController.ListenInputs();
        }
    }
}