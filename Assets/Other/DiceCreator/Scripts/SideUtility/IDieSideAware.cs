namespace InnerDriveStudios.DiceCreator
{
	/**
	 * Describes an interface for instances of prefabs that have been attached to a Die through 
	 * the DieSideUtility (which is an addon utility instead of using materials to represent die faces).
	 * IF such an instance implements this interface, it receives information about its index and 
	 * the DieSide info it represents.
	 * 
	 * @author J.C. Wichman 
     * @copyright Inner Drive Studios 2019
	 */
	public interface IDieSideAware
	{
		void SetDieSide(int pSideIndex, DieSide pDieSide);
	}
}
 