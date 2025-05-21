using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * This Die class subclasses ARollable to provide an abstract base for a single Die.
     * The only thing this adds to the ARollable class is that we wrap a DieSides component
     * and pass it to an instance of DieResult everytime the roll result is requested.
     * 
     * In itself the Die class still doesn't do anything, you have to subclass it to actually 
     * roll the die and determine when the roll is done, either using Physics or some sort of 
     * manual roll. Only a PhysicsExample has been provided at the moment, see PhysicsDie.
     *   
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [RequireComponent(typeof(DieSides))]
    [DisallowMultipleComponent]
    public abstract class Die : ARollable {

        //reference to the DieSides information required to let the Die do it's work
        public DieSides dieSides { get; private set; }

        protected virtual void Awake()
        {
            dieSides = GetComponent<DieSides>();
        }

        /**
         * Creates a new DieResult which implements IRollResult, 
         * to provide info about the current state of the Die.
         */
        override protected IRollResult createRollResult()
        {
			if (gameObject.activeSelf)
			{
				return new DieResult(this);
			} else
			{
				return NullResult.DEFAULT;
			}
        }
        
    }
}


