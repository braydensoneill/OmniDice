namespace InnerDriveStudios.DiceCreator
{
    /**
     * A generic interface for roll results, no matter whether you are rolling a single 
     * die or a collection of dice.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public interface IRollResult
    {
        /** Are these values exact ? */
        bool isExact { get; }
        
        /** How many values does the result have ? */
        int valueCount { get; }
        
        /**
         * The result as string, this can be as simple as returning the result of ToString() 
         * or something more appropriate.
         */
        string valuesAsString { get; }

        /** What is the value at the given index? */
        int Value(int pIndex = 0);
    }
}