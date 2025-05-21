using System.Text;
using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{

    /**
     * A debug class that can be used to print info about what a DieCollection is doing.
     * You can also use the DieCollectionDebugUI instead.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [RequireComponent(typeof(DieCollection))]
    public class DieCollectionDebugEventListener : MonoBehaviour
    {
        public bool updateEveryFrame = false;
        public bool logEvents = true;

        [SerializeField]
        private DieCollection _dieCollection;

        private StringBuilder _stringBuilder = new StringBuilder(); 

        private void Awake()
        {
            if (_dieCollection == null) _dieCollection = GetComponent<DieCollection>();
            _dieCollection.OnRollBegin += (a) => { statusChanged("OnRollBegin", a); enabled = true; };
            _dieCollection.OnRollEnd += (a) => { statusChanged("OnRollEnd", a); enabled = false; };
            _dieCollection.OnEndResultCleared += (a) => { statusChanged("OnEndResultCleared", a); };
            _dieCollection.OnChildRollBegin += (a, b) => { statusChanged("OnChildRollBegin", a, b); };
            _dieCollection.OnChildRollEnd += (a, b) => { statusChanged("OnChildRollEnd", a, b); };
            _dieCollection.OnChildEndResultCleared += (a, b) => { statusChanged("OnChildEndResultCleared", a, b); };
        }

        private void Start()
        {
            statusChanged("-");
            enabled = false;
        }

        private void Update()
        {
            if (updateEveryFrame) statusChanged(null);
        }

        private void statusChanged(string pEvent, ARollable pSourceA = null, ARollable pSourceB = null)
        {
            //collect info about all the dice info (one level deep) and print it
            _stringBuilder.Length = 0;

            if (logEvents) _stringBuilder.AppendLine(pEvent + (pSourceA == null ? "" : " / " + pSourceA.name) + (pSourceB == null ? "" : " / " + pSourceB.name));

            for (int i = 0; i < _dieCollection.Count; i++)
            {
                ARollable die = _dieCollection.Get(i);
                IRollResult result = die.GetRollResult();

                string resultColor = (die.HasEndResult() && result.isExact) ? "green" : "blue";
                //only show values if we are NOT rolling OR updating every frame
                string resultValues = (!die.isRolling || updateEveryFrame) ? result.valuesAsString : "*";
                _stringBuilder.AppendFormat("<color={0}>{1}={2}</color> ", resultColor, die.name, resultValues);
            }

            _stringBuilder.AppendLine ("\nAny dice still rolling? "+(_dieCollection.isRolling ? "Yes (" + _dieCollection.RollingCount + ")" : "No"));

            IRollResult collectionResult = _dieCollection.GetRollResult();
            string collectionResultValues = (!_dieCollection.isRolling || updateEveryFrame) ? collectionResult.valuesAsString : "*";
            _stringBuilder.AppendLine ("Dice collection value totals:"+collectionResultValues);

            Debug.Log(_stringBuilder.ToString());
        }


    }
}