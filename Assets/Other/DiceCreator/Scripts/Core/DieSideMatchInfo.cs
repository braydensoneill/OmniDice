namespace InnerDriveStudios.DiceCreator
{
    /**
     * DieSideMatchInfo is returned by the DieSides component, describing which DieSide most
     * closely matches the current state of the transform the DieSides component is attached to.
     * For example if you have a D6 and the side with a 3 on it is pointing upwards, then
     * getting the DieSideMatchInfo for the DieSides component attached to that D6 will return
     * the DieSide representing the values for the side with the 3 on it (assuming you entered
     * all DieSide data correctly ofcourse).
     * "Exact" means that the side is pointing exactly upward so there can be no discussion 
     * about the outcome of the roll.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public class DieSideMatchInfo {

        /** Which DieSide matches the current rotation of the DieSides' transform the closest */
        public readonly DieSide closestMatch;
        /** Is the closestMatch also an exact match? */
        public readonly bool isExactMatch;

        public DieSideMatchInfo (DieSide pClosestMatch, bool pIsExactMatch)
        {
            closestMatch = pClosestMatch;
            isExactMatch = pIsExactMatch;
        }
    }
}