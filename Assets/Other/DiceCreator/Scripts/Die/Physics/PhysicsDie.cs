using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * This Die subclass allows you to physically throw a Die.
     * 
     * Throwing is implemented by simply setting a velocity and an angularVelocity based on 
     * the parameters you specify.
     * Once the rigidbody attached to the same GameObject falls asleep, the roll is consider to
     * be done (in the general case, see other options below).
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(MeshCollider))]
    [DisallowMultipleComponent]
    public class PhysicsDie : Die
    {

        private Rigidbody _rigidbody;
        private MeshCollider _meshCollider;

        //all values below are default values which can be overwritten by calling Roll (...) with parameters.
        [SerializeField]
        [Tooltip("Direction in which force will be applied to throw the Die.")]
        private Vector3 _throwVector = Vector3.up;

        [SerializeField]
        [Tooltip("Maximum deviation in degrees from the throw vector specified above.")]
        private float _maxAngleDeviation = 45;
        [SerializeField]
        [Tooltip("The die will get a velocity based on the normalized deviated throw vector multiplied with a random number between min and max velocity")]
        private float _minVelocity = 10;
        [SerializeField]
        [Tooltip("The die will get a velocity based on the normalized deviated throw vector multiplied with a random number between min and max velocity")]
        private float _maxVelocity = 15;
        [SerializeField]
        [Tooltip("The die will get a random angular velocity between the min and max values")]
        private float _minAngularVelocity = 10;
        [SerializeField]
        [Tooltip("The die will get a random angular velocity between the min and max values")]
        private float _maxAngularVelocity = 15;

        [SerializeField]
        [Tooltip(
            "Sometimes a Die stops while it is balancing like a coin on its edge." +
            "To prevent that you can enter a non zero nudge along force here." +
            "A value of 0 disables this feature."
        )]
        private float _nudgeAlongForce = 10;

        protected override void Awake()
        {
            base.Awake();

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.mass = 1f;

            _meshCollider = GetComponent<MeshCollider>();
            _meshCollider.convex = true;

            enabled = false;
        }

        private void Start()
        {
            //if at the start we do not have an exact match, we give ourselves a little nudge
            //hoping that will fix it, if not the FixedUpdate gets another chance
            if (!dieSides.GetDieSideMatchInfo().isExactMatch)
            {
                enabled = true;
                _rigidbody.AddTorque(Vector3.left);
            }
        }

        public override void Roll()
        {
            Roll(
                _throwVector,
                _maxAngleDeviation,
                Random.Range(_minVelocity, _maxVelocity),
                Random.Range(_minAngularVelocity, _maxAngularVelocity)
            );
        }

        /**
         * Rolls a Die using the given parameters.
         * 
         * @param pThrowVector the general direction of the Die
         * @param pMaxAngleDeviation how far we can deviate from the pThrowVector
         * @param pVelocityMultiplier how many times the resulting deviation vector has to be multiplied to get the velocity vector
         * @param pAngularVelocityMultiplier the multiplication factor for the random rotation vector
         */
        public void Roll(Vector3 pThrowVector, float pMaxAngleDeviation = 0, float pVelocityMultiplier = 1, float pAngularVelocityMultiplier = 1)
        {
            //update our internal flag which will also trigger the OnRollBegin event
            isRolling = true;

            enabled = true;
            //make sure we now the velocity we are starting with is 1 by normaling the throw vector
            pThrowVector.Normalize();

            //random throw angle based of of pThrowVector:
            //first get normal for pThrowVector, taking into account that pThrowVector can be forward in
            //which case the magnitude will be 0
            Vector3 normal = Vector3.Cross(Vector3.forward, pThrowVector);
            if (normal.magnitude == 0) normal = Vector3.Cross(Vector3.left, pThrowVector);

            //first we rotate our throw vector randomly over the normal of the throw vector, 
            //then we randomly around the original throw vector
            Vector3 newVector = Quaternion.AngleAxis(Random.Range(0, pMaxAngleDeviation), normal) * pThrowVector;
            Quaternion randomRotationAroundThrowVector = Quaternion.AngleAxis(Random.Range(0, 360), pThrowVector);
            newVector = randomRotationAroundThrowVector * newVector;
            _rigidbody.linearVelocity = newVector * pVelocityMultiplier;

            //make sure we rotate in the direction of the throw
            Vector3 rotatedNormal = randomRotationAroundThrowVector * normal;
            rotatedNormal.Normalize();
            _rigidbody.angularVelocity = rotatedNormal * pAngularVelocityMultiplier;
        }

        public void FixedUpdate()
        {
            //the moment our rigidbody falls asleep, check if we are actually done rolling...
            if (_rigidbody.IsSleeping())
            {
                //...which depends on whether we have an exact match or not but then nudging should be turned off
                if (dieSides.GetDieSideMatchInfo().isExactMatch || _nudgeAlongForce == 0)
                {
                    enabled = false;
                    //dispatch OnRollEnd event
                    isRolling = false;
                }
                //if we don't have an exact match and nudging is on, nudge the die a little
                else if (_nudgeAlongForce > 0)
                {
                    nudgeAlong();
                }
            }

            //if not sleeping we are still rolling
        }

        private void nudgeAlong()
        {
            //just add a little torque to keep the die rolling...
            _rigidbody.AddTorque(Random.onUnitSphere * _nudgeAlongForce, ForceMode.Impulse);
        }

        private void OnCollisionExit(Collision collision)
        {
            //if we were disabled but are no longer sleeping after the collision, enable us again
            if (!enabled && !_rigidbody.IsSleeping())
            {
                enabled = true;
                //and make sure we inform any listeners a roll has begun again
                isRolling = true;
            }
        }

        private void OnValidate()
        {
            //some minor sanity checking
            if (_minVelocity < 0) _minVelocity = 0;
            if (_maxVelocity < 0) _maxVelocity = 0;
            if (_minAngularVelocity < 0) _minAngularVelocity = 0;
            if (_maxAngularVelocity < 0) _maxAngularVelocity = 0;
            if (_nudgeAlongForce < 0) _nudgeAlongForce = 0;
        }

    }

}