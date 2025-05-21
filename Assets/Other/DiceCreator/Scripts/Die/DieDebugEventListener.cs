using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Simple debug class that prints information about status and the events thrown by a Die.
     * Only usuable during development.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [RequireComponent(typeof(ARollable))]
    [DisallowMultipleComponent]
    public class DieDebugEventListener : MonoBehaviour
    {
        void Awake()
        {
            ARollable die = GetComponent<ARollable>();
            if (die != null)
            {
                die.OnRollBegin += onRollBegin;
                die.OnRollEnd += onRollEnd;
            }
        }

        private void onRollBegin(ARollable die)
        {
            Debug.Log(name + " roll began...");
        }

        private void onRollEnd(ARollable die)
        {
            IRollResult result = die.GetRollResult();

            Debug.Log(
                name +
                " roll ended with value: " +
                result.valuesAsString + " " +
                (result.isExact ? "(Exact)" : "(Closest)")
            );
        }

    }

}