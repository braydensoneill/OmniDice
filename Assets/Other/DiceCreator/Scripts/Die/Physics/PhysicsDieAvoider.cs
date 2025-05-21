using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * This is a utility script to use in combination with a PhysicsDieController,
     * to make sure dice avoid each other and any other GameObject that has this component,
     * it has no use on its own.
     * 
     * Dice leaning into eachother or Dice leaning into a wall may cause a Die value
     * to not be exact, it may be like almost in between two values for example.
     * To avoid this you can attach a PhysicsDieAvoider to any objects you want the dice
     * to avoid, this includes both dice and environment walls for example
     * (but not the floor or the dice will be "bouncin'-all-day" !).
     * 
     * Upon collision an explosion force will be added to the objects that actually have a 
     * non kinematic rigidbody (so although you can attach this to a wall, the wall itself
     * will not be blown away for example). This causes dice to roll away from eachother or
     * from the wall, and reduces the chance of getting these aforementioned non exact values.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [DisallowMultipleComponent]
    public class PhysicsDieAvoider : MonoBehaviour
    {

        Rigidbody _rigidBody;

        [SerializeField]
        private float _avoidanceForce = 4;
		[SerializeField]
		private bool _triggerOnStay = false;

        private void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            PhysicsDieAvoider other = collision.transform.GetComponent<PhysicsDieAvoider>();
            if (other != null && _rigidBody != null && !_rigidBody.isKinematic)
            {
				//Debug.Log("Avoiding " + other.name);
                 _rigidBody.AddExplosionForce(
                    _avoidanceForce,
                    collision.contacts[0].point,
                    1,
                    0,
                    ForceMode.VelocityChange
                );
            }
        }

		/**
		 * Optionally continously trigger this collision while OnCollisionStay
		 */
		private void OnCollisionStay(Collision collision)
		{
			if (_triggerOnStay) OnCollisionEnter(collision);
		}


	}
}