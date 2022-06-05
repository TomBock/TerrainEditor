using UnityEngine;
using UnityEngine.EventSystems;

namespace RuntimeTerrainEditor
{    
    public class CameraController : MonoBehaviour
    {
        public GameObject               camHolder;
        public float                    zoomMin         = 5f;
        public float                    zoomMax         = 100f;
        public float                    yawSpeed        = 4f;
        public float                    normalMoveSpeed = 0.3f;
        public float                    fastMoveSpeed   = 2.0f;

        
        private Camera                  _cam;
        private Vector3                 _newPos;
        private Vector3                 _newZoom;
        private Quaternion              _newRot;
        private Transform               _rig;
        private Transform               _camTransform;

        private static Vector3          _zoomVector;
        private static float            _zoomMin;
        private static float            _zoomMax;
        private float                   _newZoomFactor;

        public void Init()
        {
            _cam            = Camera.main;
            _camTransform   = _cam.transform;

            _newZoom        = _camTransform.localPosition;
            _newPos         = camHolder.transform.position;
            _newRot         = camHolder.transform.rotation;
            _rig            = camHolder.transform;

            _zoomVector     = new Vector3(0,1,-1);
            _zoomMin        = zoomMin;
            _zoomMax        = zoomMax;

            _newZoomFactor  = _camTransform.localPosition.y/_zoomVector.y;
        }

        public void ListenInputs()
        {
            bool    leftShift   = Input.GetKey(KeyCode.LeftShift);
            float   moveValue   = leftShift ? fastMoveSpeed : normalMoveSpeed;
            
            //  WASD and arrow key movement
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                _newPos += _rig.right * -moveValue;
            }

            if (Input.GetKey(KeyCode.RightArrow)|| Input.GetKey(KeyCode.D))
            {
                _newPos += _rig.right * moveValue;
            }

            if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                _newPos += _rig.forward * -moveValue;
            }

            if (Input.GetKey(KeyCode.UpArrow)   || Input.GetKey(KeyCode.W))
            {
                _newPos += _rig.forward * moveValue;
            }

            //  right click yaw
            if (Input.GetMouseButton(1))
            {
                var yawX = Input.GetAxis("Mouse X");
                var yawY = Input.GetAxis("Mouse Y");

                _newPos += _rig.right   * -moveValue * yawX * yawSpeed;
                _newPos += _rig.forward * -moveValue * yawY * yawSpeed;
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(2))
            {
                var yaw = Input.GetAxis("Mouse Y");
                _newRot.eulerAngles += new Vector3(yaw * 5f, transform.eulerAngles.y, transform.eulerAngles.z);
            }
            //  check for horizontal rotate from mouse
            else if (Input.GetMouseButton(2))
            {
                var yaw = Input.GetAxis("Mouse X");
                _newRot.eulerAngles += new Vector3(transform.eulerAngles.x, yaw * 5f, transform.eulerAngles.z);
            }

            //  check for horizontal rotate from keyboard
            else if (Input.GetKey(KeyCode.Q))
            {
                _newRot.eulerAngles += new Vector3(transform.eulerAngles.x, -1f, transform.eulerAngles.z);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                _newRot.eulerAngles += new Vector3(transform.eulerAngles.x, 1f, transform.eulerAngles.z);
            }

            //  check for zoom from middle mouse
            if (Input.mouseScrollDelta.y != 0 || Input.mouseScrollDelta.x != 0)
            {
                float mouseScrollDelta = 0f;
                if (Input.mouseScrollDelta.y != 0)
                {
                    mouseScrollDelta = Input.mouseScrollDelta.y;
                }
                else if (Input.mouseScrollDelta.x != 0)
                {
                    mouseScrollDelta = Input.mouseScrollDelta.x;
                }

                _newZoomFactor  +=  mouseScrollDelta * -moveValue/1.4f;
                _newZoomFactor  = Mathf.Clamp(_newZoomFactor, _zoomMin, _zoomMax);
                
                _newZoom        = _zoomVector * _newZoomFactor;
            }
            
            //  finally apply changes with some lerping
            _rig.rotation = Quaternion.Lerp(_rig.rotation,
                                            _newRot,
                                            Time.deltaTime * 12f);

            _newPos.y = 0f; //  prevent y axis movement
            _rig.position = Vector3.Lerp(_rig.position,
                                        _newPos,
                                        Time.deltaTime * 5f);
            
            _camTransform.localPosition = Vector3.Lerp(_camTransform.localPosition,
                                        _newZoom,
                                        Time.deltaTime * 5f);
        }
    }
}