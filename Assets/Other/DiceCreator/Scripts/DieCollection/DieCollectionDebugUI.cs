using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Implements a DieCollectionDebugUI for a maximum of 9 dice, to show some information
     * about the state of those dice (rollables to be more precise), their current value, 
     * etc. It also sums up the total over the whole collection.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [DisallowMultipleComponent]
    public class DieCollectionDebugUI : MonoBehaviour
    {

        public bool updateEveryFrame { set; get; }
        public bool logEvents { set; get; }

        [SerializeField] private DieCollection _dieCollection = null;
        [SerializeField] private Text _dieCountText = null;
        [SerializeField] private Text _lastEventText = null;
        [SerializeField] private Text _rollingInfoText = null;
        [SerializeField] private Text _dieTotalText = null;
		[SerializeField] private Transform _dieInfoParent = null;
		[SerializeField] private Text _dieInfoPrefab = null;

		private List<Text> _diceTexts = null;

        private void Awake()
        {
			if (_dieCollection == null)
			{
				Debug.Log("No DieCollection attached.");
				return;
			}

            _dieCollection.OnRollBegin += (a) => { statusChanged("OnRollBegin",a); enabled = true; };
            _dieCollection.OnRollEnd += (a) => { statusChanged("OnRollEnd",a); enabled = false; };
            _dieCollection.OnEndResultCleared += (a) => { statusChanged("OnEndResultCleared",a); };
            _dieCollection.OnChildRollBegin += (a,b) => { statusChanged("OnChildRollBegin",a,b); };
            _dieCollection.OnChildRollEnd += (a,b) => { statusChanged("OnChildRollEnd",a,b); };
            _dieCollection.OnChildEndResultCleared += (a,b) => { statusChanged("OnChildEndResultCleared",a,b); };

			_diceTexts = new List<Text>();
			for (int i = 0; i < _dieCollection.Count; i++)
			{
				_diceTexts.Add(Instantiate<Text>(_dieInfoPrefab, _dieInfoParent));
			}
        }

        private void Start()
        {
            foreach (Text text in _diceTexts) text.text = "";
            statusChanged("-");
            enabled = false;
        }

        private void Update()
        {
            if (updateEveryFrame) statusChanged(null);
        }

        private void statusChanged(string pEvent, ARollable pSourceA = null, ARollable pSourceB = null)
        {
			if (_dieCollection == null) return;

            if (logEvents && pEvent != null) Debug.Log(pEvent + (pSourceA==null?"": "/" + pSourceA.name) + (pSourceB == null ? "" : "/" + pSourceB.name));

            int diceTextsCount = Mathf.Min(_diceTexts.Count, _dieCollection.Count);

            for (int i = 0; i < diceTextsCount; i++)
            {
                ARollable die = _dieCollection.Get(i);
                IRollResult result = die.GetRollResult();

                string resultColor = (die.HasEndResult() && result.isExact)?"green":"blue";
                //only show values if we are NOT rolling OR updating every frame
                string resultValues = 
                    (!die.isRolling || updateEveryFrame) ? result.valuesAsString : "*";

                _diceTexts[i].text = string.Format ("<color={0}>{1}={2}</color>", resultColor, die.name, resultValues);
            }

            _dieCountText.text = ""+_dieCollection.Count;
            if (pEvent != null) _lastEventText.text = pEvent;
            _rollingInfoText.text = _dieCollection.isRolling ? ("Yes ("+_dieCollection.RollingCount+")") : "No";

            IRollResult collectionResult = _dieCollection.GetRollResult();
            string collectionResultValues = (!_dieCollection.isRolling || updateEveryFrame) ? collectionResult.valuesAsString : "*";
            _dieTotalText.text = collectionResultValues;
        }

		public void Roll()
		{
			_dieCollection.Roll();
		}

		public void RollNonExact()
		{
			_dieCollection.RollNonExact();
		}

		public void ClearEndResult()
		{
			_dieCollection.ClearEndResult();
		}

	}

}