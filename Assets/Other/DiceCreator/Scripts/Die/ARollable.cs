using System;
using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * ARollable is the abstract base class for anything rollable, eg Die or DieCollection.
     * 
     * It provides:
     * - an event mechanism allowing you to listen for RollBegin and RollEnd events and code to trigger these events
     * - an interface to determine whether a Rollable is still rolling
     * - an interface to start a roll
     * - an interface to retrieve the result of a roll. 
     * 
     * Note that although it is possible to retrieve the result of a die at any time, retrieving it only 
     * AFTER the OnRollEnd event will be much cheaper, because the result is static from that point onward 
     * (until the next OnRollBegin) and therefore also cached.
     * 
     * It has two methods you need to override while subclassing: Roll and createRollResult.
     * See the Die and DieCollection classes for examples.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public abstract class ARollable : MonoBehaviour 
    {
        /**
         * Triggered when isRolling toggles from false to true.
         */
        public virtual event Action<ARollable> OnRollBegin = delegate { };

        /**
         * Triggered when isRolling toggles from true to false.
         */
        public event Action<ARollable> OnRollEnd = delegate { };

        /**
         * Triggered when ClearEndResult is called.
         */
        public event Action<ARollable> OnEndResultCleared = delegate { };

        //keep tracking of whether we are rolling so that we can trigger an event when this state changes
        private bool _isRolling = false;

        //the moment _isRolling becomes false, the result will be cached for later retrieval
        //it is of type IRollResult so that we can treat Die's (Dice) and DieCollections similarly
        private IRollResult _cachedResult = null;

        /**
         * Fulfills two rolls: publicly tells you whether this rollable is currently considered
         * to be rolling and protectedly allows you to change this flag and dispatch events
         * accordingly.
         */
        public bool isRolling
        {
            get
            {
                return _isRolling;
            }

            protected set
            {
                //if the flag wasnt changed don't do anything
                if (_isRolling == value) return;

                //otherwise store it, dispatch the correct event and update the cache accordingly
                _isRolling = value;
                if (_isRolling)
                {
                    _cachedResult = null;
                    OnRollBegin(this);
                }
                else
                {
                    _cachedResult = createRollResult();
                    OnRollEnd(this);
                }
            }
        }

        /**
         * @return the current IRollResult. If isRolling==false this will be the
         * cached result created when the roll ended, if isRolling==true it will be 
         * created on the fly.
         */
        public IRollResult GetRollResult()
        {
            if (_cachedResult != null)
            {
                return _cachedResult;
            }
            else
            {
                return createRollResult();
            }
        }

        /**
         * Has a result been cached at the end of a roll?
         */
        public bool HasEndResult ()
        {
            return _cachedResult != null;
        }

        /**
         * Clear the cache and trigger an event. This can be used to sort of keep track whether
         * results on a rollable are still valid on not.
         */
        public virtual void ClearEndResult()
        {
            _cachedResult = null;
            OnEndResultCleared(this);
        }

        /**
         * Create an instance of IRollResult which represents the result for this roll.
         * Needs to be implemented in a subclass, see Die and DieCollection for an example.
         */
        protected abstract IRollResult createRollResult();

        /**
         * Needs to be implemented in a subclass to do the actual rolling.
         * Make sure you update the isRolling flag correctly to stay in sync with the
         * actual state of the Die.
         */
        public abstract void Roll();

    }
}