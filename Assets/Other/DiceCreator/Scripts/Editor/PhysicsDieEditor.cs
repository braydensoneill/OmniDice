using UnityEditor;
using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Simple editor that checks whether the component that the PhysicsDie is attached to
     * has a physics material set, and if not, sets the default DiePhysicsMaterial.physicMaterial
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [CustomEditor(typeof(PhysicsDie))]
    [CanEditMultipleObjects]
    public class PhysicsDieEditor : Editor
    {

        private void OnEnable()
        {
            PhysicsDie go = target as PhysicsDie;
            MeshCollider collider = go.GetComponent<MeshCollider>();
            if (collider == null) return;
            if (collider.sharedMaterial != null) return;

            collider.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(PathConstants.DIE_PHYSICS_MATERIAL);

            if (collider.sharedMaterial == null)
            {
                Debug.LogWarning(
                    "Could not load the DiePhysicsMaterial at " +
                    PathConstants.DIE_PHYSICS_MATERIAL + ", did you move it?" +
                    "\nPlease readd it, or update the path accordingly in the Editor/PathConstants file"
                );
            }
        }

    }
}