using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * An example of how we could trigger the rolling of a die by using a mouse.
     * It depends on the Die having an actual collider to raycast against.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [RequireComponent(typeof(Die))]
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public class MouseClickDieRoller : MonoBehaviour
    {

        Die _die;

        [SerializeField]
        [Tooltip("Force the Die to roll even if it is currently already rolling.")]
        private bool _force = true;

        private void Awake()
        {
            _die = GetComponent<Die>();
        }

        private void OnMouseUp()
        {
            if (!_die.isRolling || _force) _die.Roll();
        }

    }
}