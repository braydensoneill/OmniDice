namespace InnerDriveStudios.DiceCreator
{
    /**
     * Represents the result for a single Die roll.
     * For more info see IRollResult.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public class DieResult : IRollResult
    {
        //stores reference to all the values for a given side
        private int[] _values;

        public DieResult(Die pParent)
        {
			//check the status of the given die
            DieSideMatchInfo matchInfo = pParent.dieSides.GetDieSideMatchInfo();
            //if we are not rolling and the match is exact, the result is also exact
            isExact = !pParent.isRolling && matchInfo.isExactMatch;
            _values = matchInfo.closestMatch.values;
            valueCount = _values.Length;
            valuesAsString = matchInfo.closestMatch.ValuesAsString();
        }

        /**
         * @return whether the result is exact: one die side matches exactly and the die is not rolling
         */
        public bool isExact { get; private set; }
        
        /**
         * The number of values stored in this result.
         */
        public int valueCount { get; private set; }

        /**
         * All the values stored in this result concatenated as a comma separated string
         */
        public string valuesAsString { get; private set; }

        /**
         * Get the value at the given index (default is the first element)
         * 
         * @param pIndex the index for the value to return, for simple dice, 
         * valueCount will always be 1 and no index should be passed.
         */
        public int Value(int pIndex = 0)
        {
            return _values[pIndex];
        }

    }
}