using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
    * The DieSide class represents all there is to know about a single DieSide:
    * - the normal of the side
    * - the centerpoint of the side
    * - an array of ints describing the data values stored for this side
    * 
    * We use an array of ints as values since it is the most general approach:
    * - you can use an int to represent a side ID or a single die value
    * - you can use multiple ints to represent multiple values for example for Descent dice
    * - you can match ints to their string counterpart using lookup dictionaries
    *	 
    * @author J.C. Wichman
    * @copyright Inner Drive Studios
    */
    [System.Serializable]
    public class DieSide
    {
        [SerializeField]
        private Vector3 _normal;         //once set, this cannot be overwritten from the outside
        [SerializeField]
        private Vector3 _centerPoint;    //once set, this cannot be overwritten from the outside

        /** 
         * This array contains all the values that have been stored for this DieSide 
         * (through the DieSidesEditor class).
         * 
         * The values have been made public so that you can overwrite them at runtime, 
         * for example in case you have a game where you dynamically want to change or upgrade your dice. 
         * If you don't like public, you can also just wrap
         * this value same as has been done for the normal and the centerPoint.
         */
        [SerializeField]
        public int[] values;             

        //we cache the string rep because we use it a lot while the values at runtime will be static
        //if you do change the values at runtime be sure to call DirtyStringCache
        private string _cachedStringRepresentation = null;


        public DieSide(Vector3 pNormal, Vector3 pCenterPoint)
        {
            _normal = pNormal;
            _centerPoint = pCenterPoint;
        }

        /**
         * The normal of this DieSide
         */
        public Vector3 normal { get { return _normal; } }

        /**
         * The centerPoint of this DieSide
         */
        public Vector3 centerPoint { get { return _centerPoint; } }

        /**
         * @return a cached version of the int values stored in this DieSide in string format, 
         * separated with ,
         */
        public string ValuesAsString()
        {
            if (_cachedStringRepresentation == null || _cachedStringRepresentation.Length == 0)
            {     
                _cachedStringRepresentation = StringUtility.ToString(values);
            }
            return _cachedStringRepresentation;
        }

        /**
         * In case you decide to change your DieSide datavalues at runtime, you need to dirty
         * the string cache to make sure the string representation stays up to date.
         */
        public void DirtyStringCache()
        {
            _cachedStringRepresentation = null;
        }

        /**
         * @return "DieSide:"+ValuesAsString()
         */
        public override string ToString()
        {
            return "DieSide:" + ValuesAsString();
        }
    }

}