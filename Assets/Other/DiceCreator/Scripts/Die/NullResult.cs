namespace InnerDriveStudios.DiceCreator
{
	/**
	 * Represents a zero result, used for disabled dice, to avoid exceptions.
	 * 
	 * @author J.C. Wichman
	 * @copyright Inner Drive Studios
	 */
	public class NullResult : IRollResult
	{
		public static readonly NullResult DEFAULT = new NullResult();

		private NullResult () {}

		public bool isExact
		{
			get { return false;	}
		}

		public int valueCount { get { return 0; } }

		public string valuesAsString { get { return "0"; } }

		public int Value(int pIndex = 0)
		{
			return 0;
		}
	}
}
