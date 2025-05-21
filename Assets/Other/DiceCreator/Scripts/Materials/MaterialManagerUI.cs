using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * UI that lets you change the material for the given DieCollection to the material from a selected materialset.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public class MaterialManagerUI : MonoBehaviour
    {
		//reference to the full collection of materials
        [SerializeField]
        private MaterialSetCollection _materialSetCollection = null;

        [SerializeField]
        private Dropdown _materialDropdown = null;

		//reference to full collection of dice
		[SerializeField]
		private DieCollection _dieCollection = null;

		//internal storage of all dice in another format for quicker processing
		private List<GameObject> _diceAsGameObjects = new List<GameObject>();
		private int _currentMaterialSetIndex = -1;
		private int _materialSetCount = 0;

        // Use this for initialization
        void Awake()
        {
            _materialDropdown.AddOptions(new List<string>() { "Select a material set..." });
            _materialDropdown.AddOptions(new List<string>(_materialSetCollection.GetAllDescriptions()));
            _materialDropdown.onValueChanged.AddListener(onDropdownValueChanged);

			//fill the gameobject list with all dice from the collection
			_diceAsGameObjects.AddRange (_dieCollection.GetEnumerable().Select(x => x.gameObject));
			_materialSetCount = _materialSetCollection.materialSets.Length;

		}

        private void onDropdownValueChanged(int pSelectedIndex)
        {
			//if the first element or the same as we already have is selected, ignore
			if (pSelectedIndex == 0 || pSelectedIndex-1 == _currentMaterialSetIndex) return;
			Debug.Log(_materialDropdown.options[pSelectedIndex].text + " selected.");

			//subtract one since the first element is our "Select a material set ..." header
			_currentMaterialSetIndex = pSelectedIndex - 1;
			updateMaterialSet();
        }

		private void updateMaterialSet()
		{

			_currentMaterialSetIndex = ((_currentMaterialSetIndex % _materialSetCount) + _materialSetCount) % _materialSetCount;

			_materialDropdown.value = (_currentMaterialSetIndex + 1);

			MaterialSet materialSet = _materialSetCollection.materialSets[_currentMaterialSetIndex];
			string logInfo = MaterialSetUtility.MapMaterialSetToGameObjects(materialSet, _diceAsGameObjects);

			if (logInfo != null)
			{
				Debug.Log(logInfo);
			}
			else
			{
				Debug.Log("Done processing, for more info, define the MSUD_ON scripting symbol in the player settings.");
			}
		}

		public void onNextSet()
		{
			_currentMaterialSetIndex++;
			updateMaterialSet();
		}

		public void onPreviousSet()
		{
			_currentMaterialSetIndex--;
			updateMaterialSet();
		}

        private void OnDestroy()
        {
            _materialDropdown.onValueChanged.RemoveAllListeners();
        }
    }

}