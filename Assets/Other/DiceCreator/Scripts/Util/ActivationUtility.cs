using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * A simple utility class to toggle gameobjects on/off based on key presses for the demo scene.
     *
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public class ActivationUtility : MonoBehaviour
    {

        /**
         * ActivationItem describes a single set of GameObjects that can be activated or deactivated
         * including the (de)activation key and a human readable description for this set.
         */
        [Serializable]
        public class ActivationItem
        {
            /** Description for this set of objects */
            public string description;
            /** Key that toggles the objects active/inactive state on or off */
            public KeyCode key;
            /** The actual gameObjects to toggle the active state for */
            public GameObject[] gameObjects;        
        }

        [SerializeField]
        //a list with all the different activation sets
        private ActivationItem[] _activationItems = null;
		//if you want to show some help text you can either hook up this field, 
		//or use GetActivationInfo()
		[SerializeField]
		private Text _helpText = null;
        private string _activationInfo = "";

        private void Awake()
        {
            //Display some help generated based on the descriptions set in the different activation items
            _activationInfo =
                String.Join(
                    "\n",
                    _activationItems.Select(x => "Press " + x.key + " to toggle " + x.description).ToArray<string>()
                );

            if (_helpText != null)
            {
                _helpText.text = _activationInfo;
            }
        }

        void Update()
        {
            if (!Input.anyKey) return;

            foreach (ActivationItem ai in _activationItems)
            {
                if (Input.GetKeyDown(ai.key))
                {
                    foreach (GameObject go in ai.gameObjects) go.SetActive(!go.activeSelf);
                }
            }
        }
    }
}