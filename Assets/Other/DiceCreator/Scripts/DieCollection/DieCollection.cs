using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * DieCollection is a component to which you can add other ARollable instances,
     * allowing you to track events and results for a group. Examples of ARollable instances
     * are of course Die, PhysicsDie but also DieCollection itself, in other words you can also
     * track multiple collections using a collection.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public class DieCollection : ARollable
    {
        /**
         * Triggered when a child within the collection starts rolling.
         * The child is passed as second parameter to the event handler.
         */
        public event Action<DieCollection, ARollable> OnChildRollBegin = delegate { };

        /**
         * Triggered when a child within within the collection stops rolling.
         * The child is passed as second parameter to the event handler.
         */
        public event Action<DieCollection, ARollable> OnChildRollEnd = delegate { };

        /**
         * Triggered when the result for a child within the collection is cleared.
         * The child is passed as second parameter to the event handler.
         */
        public event Action<DieCollection, ARollable> OnChildEndResultCleared = delegate { };

        [SerializeField]
        //all the rollables in the collection
        private List<ARollable> _rollables = new List<ARollable>();
        //only the rollables that are currently rolling
        private HashSet<ARollable> _rollingRollables = new HashSet<ARollable>();

        //tracks whether a child clear is due to us clearing the whole collection or the child itself
        private bool _clearingWholeCollection = false;

        //if this is true, the diecollection will only respond to child events, updating its internal
        //administration when it is rolling itself. If false, triggering the Roll on a child will also
        //set the rolling state of the parent collection to true.
		[Tooltip(
			"If checked, the collection will only respond to child events when it started the roll itself. "+
			"If unchecked, triggering a roll on a child die will also set the rolling state of the parent collection to true."
		)]

		public bool ignoreIndependentChildEvents = false;

        private void Awake()
        {
            //make sure we register for the events of all our dice that were already in our serialized collection.
            foreach (ARollable rollable in _rollables) registerForChildEvents(rollable);
        }

        /**
         * Adds the given ARollable instances to the collection so their events can be tracked.
         */
        public void Add (IEnumerable<ARollable> pRollables)
        {
            foreach (ARollable rollable in pRollables) Add(rollable);
        }

        /**
         * Adds the given ARollable instance to the collection so its events can be tracked.
         */
        public void Add(ARollable pRollable)
        {
            registerForChildEvents(pRollable);
            _rollables.Add(pRollable);
        }

        /**
         * Removes the given ARollable instances from the collection so their events will no longer be tracked.
         */
        public void Remove(IEnumerable<ARollable> pRollables)
        {
            foreach (ARollable rollable in pRollables) Remove(rollable);
        }

        /**
         * Removes the given ARollable instance from the collection so its events will no longer be tracked.
         */
        public void Remove(ARollable pRollable)
        {
            unregisterForChildEvents(pRollable);
            _rollables.Remove(pRollable);
        }

        public void RemoveAll()
        {
            foreach (ARollable rollable in _rollables)
            {
                unregisterForChildEvents(rollable);
            }

            _rollables.Clear();
            _rollingRollables.Clear();
        }

        public ARollable Get(int pIndex)
        {
            Assert.IsTrue(pIndex >= 0 && pIndex < _rollables.Count, "Invalid index");
            return _rollables[pIndex];
        }

        public int Count {  get { return _rollables.Count; } }
        public int RollingCount { get { return _rollingRollables.Count; } }

		public ARollable Current
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/**
        * Roll all the items in this collection.
        */
		public override void Roll()
        {
            //assuming we will be rolling
            isRolling = true;
			//then roll every item
			for (int i = 0; i < _rollables.Count; i++)
			{
				rollSingleDie(_rollables[i], i, i);
			}
            //then check whether we actually have items rolling
            isRolling = _rollingRollables.Count > 0;
            //this flow makes sure we trigger OnBeginRoll, OnEndRoll no matter what
        }

        /**
         * Only reroll those items that do not have an endresult or where the endresult is not exact.
         */
        public void RollNonExact()
        {
            isRolling = true;

			int activeCount = 0;
			ARollable rollable = null;
			for (int i = 0; i < _rollables.Count; i++)
			{
				rollable = _rollables[i];
				if (!rollable.HasEndResult() || !rollable.GetRollResult().isExact)
				{
					rollSingleDie(rollable, i, activeCount ++);
				}
			}

            isRolling = _rollingRollables.Count > 0;
        }

		/**
		 * This can be overridden in subclass if you want to roll a die in another specific way.
		 * @param pRollable the rollable to roll
		 * @param pGroupIndex the index of the rollable within the complete collection
		 * @param pActiveIndex the index of the rollable within the active group to roll
		 *		  For the Roll() call these will be the same, for the RollNonExact there will be a difference.
		 */
		protected virtual void rollSingleDie(ARollable pRollable, int pGroupIndex, int pActiveIndex)
		{
			if (pRollable.gameObject.activeSelf) pRollable.Roll();
		}


		public override void ClearEndResult()
        {
            _clearingWholeCollection = true;

            base.ClearEndResult();
            foreach (ARollable rollable in _rollables) rollable.ClearEndResult();

            _clearingWholeCollection = false;
        }

        private void registerForChildEvents(ARollable pRollable)
        {
            pRollable.OnRollBegin += onChildRollBegin;
            pRollable.OnRollEnd += onChildRollEnd;
            pRollable.OnEndResultCleared += onChildEndResultCleared;
        }

        private void unregisterForChildEvents(ARollable pRollable)
        {
            pRollable.OnRollBegin -= onChildRollBegin;
            pRollable.OnRollEnd -= onChildRollEnd;
            pRollable.OnEndResultCleared -= onChildEndResultCleared;
        }

        private void onChildRollBegin(ARollable pRollable)
        {
            if (ignoreIndependentChildEvents && !isRolling) return;

            if (_rollingRollables.Add(pRollable)) {
                //if we actually added the item, make sure isRolling is true, this wont trigger an event if
                //we were already rolling
                isRolling = true;
                OnChildRollBegin(this, pRollable);
            }
        }

        private void onChildRollEnd(ARollable pRollable)
        {
            if (ignoreIndependentChildEvents && !isRolling) return;
            
            if (_rollingRollables.Remove(pRollable))
            {
                //if we actually removed the item from the rolling administration, trigger child event
                OnChildRollEnd(this, pRollable);
                //and possibly our own end event, if the result didn't change, no event will be triggered
                isRolling = _rollingRollables.Count > 0;
            }
        }

        private void onChildEndResultCleared(ARollable pRollable)
        {
            //if we are clearing the whole collection, just pass on the child event
            //the call to Clear will already have reset the result cache and triggered our own main event
            if (_clearingWholeCollection)
            {
                OnChildEndResultCleared(this, pRollable);
                return;
            }

            //if this event is triggered because Clear has been called on an item in our collection
            //check if we want to ignore it or not. If we are not ignoring it, and we have an endresult
            //clear our end result
            if (!ignoreIndependentChildEvents) { 
                if (HasEndResult()) base.ClearEndResult();
                OnChildEndResultCleared(this, pRollable);
            }
        }

        /**
         * @return a DieCollectionResult 
         */
        protected override IRollResult createRollResult()
        {
            return new DieCollectionResult(this);
        }

		public IEnumerable<ARollable> GetEnumerable()
		{
			return ((IEnumerable<ARollable>)_rollables);
		}

	
	}
}

