using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * A class that allows you to trigger the main methods on a DieCollection through key presses.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public class KeyPressCollectionRoller : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If not set, this component will look for a DiceCup on the same GameObject")]
        DieCollection _dieCollection = null;

        [SerializeField]
        [Tooltip("Will reroll all dice")]
        private KeyCode _rollAllKey = KeyCode.A;

        [SerializeField]
        [Tooltip("Will reroll only the dice that didn't have exact matches")]
        private KeyCode _rollFaulties = KeyCode.F;

        [SerializeField]
        [Tooltip("Will clear all currently stored results")]
        private KeyCode _clearResultsKey = KeyCode.C;

        private void Awake()
        {
            if (_dieCollection == null) _dieCollection = GetComponent<DieCollection>();
        }

        void Update()
        {
            if (Input.GetKeyDown(_rollAllKey)) _dieCollection.Roll();
            if (Input.GetKeyDown(_rollFaulties)) _dieCollection.RollNonExact();
            if (Input.GetKeyDown(_clearResultsKey)) _dieCollection.ClearEndResult();
        }
    }
}