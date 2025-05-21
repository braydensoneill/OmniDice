using System.Collections.Generic;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * DieCollectionResult represents the totals and status for a whole collection of dice 
     * (rollables to be more exact), adding the results of all items in the collection together.
     * Of course you can still inspect the individual results by inspecting the individual items
     * in the collection.
     * 
     * The DieCollectionResult for a die with 1 value per side and a die with 3 values per side,
     * will also contains 3 values per side. On other words, a DieCollectionResult will make most
     * sense when applied to similar dice.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public class DieCollectionResult : IRollResult
    {
        private List<int> _valueTotals = new List<int>();
        private string _valuesAsString = null;

        public DieCollectionResult(DieCollection pDieCollection)
        {
            //to be exact we cannot be rolling
            isExact = !pDieCollection.isRolling;

            //go through all items in the collection, get the results and add them together
            int rollableCount = pDieCollection.Count;
            for (int i = 0; i < rollableCount;i++)
            {
                IRollResult result = pDieCollection.Get(i).GetRollResult();

                //the collection is only exact if every item is exact
                isExact &= result.isExact;

                //make sure the valueTotals list contains enough items and add the totals
                for (int j = 0; j < result.valueCount;j++)
                {
                    if (_valueTotals.Count < j + 1) _valueTotals.Add(0);
                    _valueTotals[j] += result.Value(j);
                }
            }

            _valuesAsString = StringUtility.ToString(_valueTotals);
        }

        /**
         * Are all dice in the collection exact and none of them rolling?
         */
        public bool isExact     { get; private set; }
        
        /**
         * How many different values does our result have?
         * For all regular dice this will be 1, for dice such as the descent dice, this might be more.
         */
        public int valueCount   { get { return _valueTotals.Count; } }

        /**
         * The totals returned as a comma separated string
         */
        public string valuesAsString { get { return _valuesAsString; } }

        /**
         * The value at the given index. For all regular dice, there will only
         * be one value.
         */
        public int Value(int pIndex = 0)  { return _valueTotals[pIndex]; }
    }
}
