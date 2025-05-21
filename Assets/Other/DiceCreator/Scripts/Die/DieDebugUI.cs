using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Shows debug information for a Die on a small worldcanvas.
     *
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class DieDebugUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        //minimum requirement is some DieSides information
        [SerializeField]
        private DieSides _dieSides = null;

        //how far above the die should we be?
        [SerializeField]
        private float _distanceFromTarget = 2;

        //canvas will fade based on camera distance
        [SerializeField]
        private float _fadeStartDistance = 4;
        [SerializeField]
        private float _fadeEndDistance = 2;
		[SerializeField]
		private float _offScale = 0.01f;
		[SerializeField]
		private float _overScale = 0.015f;

		//which text component should we put the info in
		private Text _text;
        //which camera should we rotate towards?
        private Camera _camera;
        //canvas group to fade out if camera gets too close to the canvas
        private CanvasGroup _canvasGroup;

        //these are optional, only required if you also want to show the events a given Die is throwing
        private Die _die;
        private string _lastDieEvent = "-";

        private void Awake()
        {
            _text = GetComponentInChildren<Text>();
            _camera = Camera.main;
            _canvasGroup = GetComponent<CanvasGroup>();
			_canvasGroup.alpha = 1;
			OnPointerExit(null);

            //DieSides component is required, but Die component is optional
            if (_dieSides != null)
            {
                _die = _dieSides.GetComponent<Die>();
                if (_die != null)
                {
                    _die.OnRollBegin += onDieRollBegin;
                    _die.OnRollEnd += onDieRollEnd;
                }
            }
            else
            {
                Debug.Log("Please connect this script to an instance of DieSides.");
            }
        }

        /**
         * Stores name of this event.
         */
        private void onDieRollBegin(ARollable pDie)
        {
            _lastDieEvent = "OnRollBegin";
        }

        /**
         * Stores name of this event.
         */
        private void onDieRollEnd(ARollable pDie)
        {
            _lastDieEvent = "OnRollEnd";
        }
        
        void Update()
        {
            //if the component is not active, hide us
            if (_dieSides == null || !_dieSides.gameObject.activeSelf) 
            {
                _canvasGroup.alpha = 0;
                return;
            } else
			{
				_canvasGroup.alpha = 1;
			}

            //update canvas position and rotation
            transform.position = _dieSides.transform.position + Vector3.up * _distanceFromTarget;
            if (Application.isPlaying) transform.rotation = Quaternion.LookRotation(_camera.transform.forward);

            //if no closestHit is available it means we have no die sides at all
            DieSideMatchInfo dieSideMatchInfo = _dieSides.GetDieSideMatchInfo();
            if (dieSideMatchInfo.closestMatch == null)
            {
                _text.text = "Please use the\nDieSidesEditor first\nto find all sides.";
            }
            else
            {
                //set basic info about the current die 
                _text.text = "Die type:" + _dieSides.dieSideCount +
                            "\nClosest match:" + dieSideMatchInfo.closestMatch.ValuesAsString() +
                            "\nExact match?:" + (dieSideMatchInfo.isExactMatch ? "<color=green>YES</color>" : "<color=blue>NO</color>");

                //add die specific info if available
                if (_die != null)
                {
                    _text.text += "\nIs rolling?:" + _die.isRolling +
                                    "\nLast event:" + _lastDieEvent;
                }
            }

            //fade out this world canvas as we get closer and closer to the camera
            float camDistance = (_camera.transform.position - transform.position).magnitude;
            if (camDistance > _fadeStartDistance) return;

            float fadeDistance = _fadeStartDistance - _fadeEndDistance;
            float currentDistance = camDistance - _fadeEndDistance;
            float alpha = Mathf.Clamp(currentDistance / fadeDistance, 0, 1);
            _canvasGroup.alpha = alpha;
        }

		public void OnPointerEnter(PointerEventData eventData)
		{
			transform.localScale = Vector3.one * _overScale;
			GetComponent<Canvas>().sortingOrder = 1;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			transform.localScale = Vector3.one * _offScale;
			GetComponent<Canvas>().sortingOrder = 0;
		}

		private void OnEnable()
		{
			Update();
		}

	}



}