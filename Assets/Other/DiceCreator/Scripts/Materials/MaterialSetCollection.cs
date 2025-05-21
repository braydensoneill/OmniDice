using UnityEngine;
using System.Linq;

namespace InnerDriveStudios.DiceCreator
{

    /**
     * Combines a bunch of MaterialSets into a collection that we can use to switch materials
     * at editor or runtime.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [CreateAssetMenu(fileName = "DieMaterialSetCollection", menuName = "IDS/DiceCreator/Create material set collection", order = 0)]
    public class MaterialSetCollection : ScriptableObject
    {
        /** 
         * What the editor/user sees 
         */
        public MaterialSet[] materialSets;

        /**
         * Each MaterialSet has a description, this methods combines them all into an array of strings.
         */
        public string[] GetAllDescriptions()
        {
            return materialSets.Where(x => x != null).Select(x => x.description).ToArray<string>();
        }

        /**
         * Get a specific set based on a given description.
         */
        public MaterialSet GetMaterialSetByDescription(string pDescription)
        {
            return materialSets.FirstOrDefault<MaterialSet>(t => t.description == pDescription);
        }

    }
}