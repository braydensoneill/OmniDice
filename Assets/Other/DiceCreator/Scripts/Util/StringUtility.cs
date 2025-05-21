using System.Collections.Generic;
using System.Linq;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Simple string utility to concatenate IEnumerable<int>'s (eg List<int>, int []) 
     * to a comma separated string.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public static class StringUtility
    {
        /**
         * Converts the given enumerable int collection to a comma separated string.
         * @param pInts the collection of ints
         * @return the comma separated string
         */
        public static string ToString(IEnumerable<int> pInts)
        {
            return string.Join(",", pInts.Select(x => x.ToString()).ToArray<string>());
        }
    }
}
