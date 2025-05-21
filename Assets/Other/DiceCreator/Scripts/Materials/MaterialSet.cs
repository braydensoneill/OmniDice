using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Describes a set of materials.
     * 
     * Can be used either at editor or at runtime to assign materials to gameobjects quickly by using 
     * specific naming conventions (see the MaterialSetUtility).
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [CreateAssetMenu(fileName = "DieMaterialSet", menuName = "IDS/DiceCreator/Create material set", order = 0)]
    public class MaterialSet : ScriptableObject
    {
        /** Description to show for this MaterialSet */
        public string description;
        /** All materials within this set */
        public Material[] materials;
    }
}