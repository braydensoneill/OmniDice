using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Sample script to demonstrate how we can zoom in on the result of a single Die.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [DisallowMultipleComponent]
    public class RightClickCameraDieZoomer : MonoBehaviour
    {
        //which camera has to do the zooming?
        private Camera _camera;

        //what is the original start position and orientation of the camera?
        private Vector3 _startPosition;
        private Quaternion _startRotation;

        //what is the target position/orientation for the zoom?
        private Vector3 _endPosition;
        private Quaternion _endRotation;

        //how far along are we with zooming?
        private float _interpolationAlpha = 0;

        //which die are we zoomed in on at the moment? (default:none)
        private Die _lastZoomedDie = null;

        [SerializeField]
        private float _zoomSpeed = 1;
        [SerializeField]
        private float _zoomDistance = 3;

        [SerializeField]
        private bool _zoomInOnARollingDieAllowed = false;

        private enum ZoomState { ZOOMING_IN, ZOOMING_OUT, ZOOMED_IN, ZOOMED_OUT };
        private ZoomState _zoomState = ZoomState.ZOOMED_OUT;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null) _camera = Camera.main;

            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (_zoomState == ZoomState.ZOOMED_IN || _zoomState == ZoomState.ZOOMING_IN)
                {
                    startZoomOut();
                }
                else if (_zoomState == ZoomState.ZOOMED_OUT || _zoomState == ZoomState.ZOOMING_OUT)
                {
                    tryToFindTargetForZoomingIn();
                }
            }

            //if we are zoomed in on a die (_lastZoomedDie != null) and the die starting rolling, zoom out
            if (_lastZoomedDie != null && _lastZoomedDie.isRolling)
            {
                startZoomOut();
            }

            //do we have to do something at the moment?
            switch (_zoomState)
            {
                case ZoomState.ZOOMING_IN: zoomIn(); break;
                case ZoomState.ZOOMING_OUT: zoomOut(); break;
            }
        }

        private void startZoomOut()
        {
            _lastZoomedDie = null;
            _interpolationAlpha = 0;
            _zoomState = ZoomState.ZOOMING_OUT;
        }

        private void tryToFindTargetForZoomingIn()
        {
            //check if we can find a die under the mouse pointer
            RaycastHit info;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            bool infoFound = Physics.Raycast(ray, out info);
            Die dieFound = infoFound ? info.transform.GetComponent<Die>() : null;

            if (infoFound && dieFound && (_zoomInOnARollingDieAllowed || !dieFound.isRolling))
            {
                //get the position
                _endPosition = info.transform.position + Vector3.up * _zoomDistance;
                //get the rotation by first getting an orientation vector from camera to die
                Vector3 forward = info.transform.position - _camera.transform.position;
                Quaternion forwardRotation = Quaternion.LookRotation(forward);
                //then rotate THAT vector downward to Vector3.down, gives a nicer camera orientation
                Quaternion downwardRotation = Quaternion.FromToRotation(forward, Vector3.down) * forwardRotation;
                _endRotation = downwardRotation;

                //start over with the interpolation
                _interpolationAlpha = 0;
                _zoomState = ZoomState.ZOOMING_IN;

                //set the current zoom die
                _lastZoomedDie = dieFound;
            }
            /*
            else
            {
                Debug.Log("Could not find target");
            }
            */
        }

        private void zoomIn()
        {
            transform.position = Vector3.Slerp(transform.position, _endPosition, _interpolationAlpha);
            transform.rotation = Quaternion.Slerp(transform.rotation, _endRotation, _interpolationAlpha);

            _interpolationAlpha += Time.deltaTime * _zoomSpeed;

            if (_interpolationAlpha > 1) _zoomState = ZoomState.ZOOMED_IN;
        }

        private void zoomOut()
        {
            transform.position = Vector3.Slerp(transform.position, _startPosition, _interpolationAlpha);
            transform.rotation = Quaternion.Slerp(transform.rotation, _startRotation, _interpolationAlpha);

            _interpolationAlpha += Time.deltaTime * _zoomSpeed;

            if (_interpolationAlpha > 1) _zoomState = ZoomState.ZOOMED_OUT;
        }
    }
}