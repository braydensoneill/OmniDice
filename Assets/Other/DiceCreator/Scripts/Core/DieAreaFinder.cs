using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
    * The DieAreaFinder class is a helper script used internally by the DieSidesEditor class to process 
    * all triangles in a model to deduct how many sides a model has and what the normals of those sides are.
    * 
    * The basic principle of the script is this:
    * - first create DieArea instances for each triangle of the first submesh of a given Mesh 
    *   in order to register some data about those triangles (area, normal, centerpoint)
    * - then combine all DieAreas that share the same normal, adding their areas, averaging the new center points.
    *	 (note that this assumes that areas with similar normals are also adjacent, 
    *	 which will be true for dice models, but not for other models eg)
    * - lastly filter out all areas whose area size is less than some cutoff value * the max area size encountered,
    *   so that we are left with only the sides that actually represent sides of a die.
    * 
    * @author J.C. Wichman
    * @copyright Inner Drive Studios
	* @changes 1.1 - added an area count to the DieArea class to make sure we can calculate the correctly weighted average of the different die areas
    */
    public static class DieAreaFinder
    {
        /**
         * Inner class to wrap all the details we want to keep track of for each area 
         * that we found while processing submesh 0 of the given mesh. 
         */
        public class DieArea
        {
            public float area;          
            public Vector3 normal;      
            public Vector3 centerPoint;
			public int areaCount;
        }

        //store some debug info about the generation process to request and display at a later time
        private static StringBuilder _stringBuilder = new StringBuilder();

        /**
         * Find all the DieArea's according to the algorithm described in the class header.
         * 
         * @param pDieMesh  the mesh to generate DieAreas from. Note that areas will only be generated 
         * for the first submesh of this given mesh.
         * @param pAreaCutOffMultiplier after finding all areas and combining those with similar normals,
         * a filtering step will take place in which all areas below a certain size will be removed.
         * This size is calculated by multiplying this parameter (in the range 0..1) with the maximum area
         * size found during the combination phase. If you are getting false positives (too many DieAreas),
         * reduce this value. If you are getting false negatives (missing DieAreas) increase this value.
         * @return the unique DieArea's for the given mesh according to the algorithm as described in the class header.
         */
        public static List<DieArea> FindDieAreas(Mesh pDieMesh, float pAreaCutOffMultiplier)
        {
            clearLog();
            log("\nGathering areas...");
            List<DieArea> dieAreas = convertMeshTrianglesToDieAreaInstances(pDieMesh);
            log("Areas found:" + dieAreas.Count);

            log("Combining areas and calculating max area size...");
            float maxAreaSize;
            List<DieArea> combinedAreas = combineDieAreasWithSimilarNormals(dieAreas, out maxAreaSize);
            log(combinedAreas.Count + " combined areas.");
            int areasRemoved = dieAreas.Count - combinedAreas.Count;
            log(areasRemoved + " areas removed.");
            log("Max area size:" + maxAreaSize);

            log("Filtering all areas below " + pAreaCutOffMultiplier + " threshold...");

            List<DieArea> resultAreas = filterDieAreas(combinedAreas, maxAreaSize * pAreaCutOffMultiplier);
 
            log(resultAreas.Count + " areas left.");
            log("Done.");
            return resultAreas;
        }

        /**
         * @param pDieMesh a mesh of a Die to process
         * @return a list of DieAreas based on all the triangles of the FIRST submesh.
         */
        private static List<DieArea> convertMeshTrianglesToDieAreaInstances(Mesh pDieMesh)
        {
            List<DieArea> result = new List<DieArea>();

            //get all mesh data
            int[] triangleIndices = pDieMesh.GetTriangles(0);
            int triangleIndicesLength = triangleIndices.Length;
            Vector3[] vertices = pDieMesh.vertices;
            Vector3[] normals = pDieMesh.normals;

            //convert all mesh data into a list of areas
            for (int i = 0; i < triangleIndicesLength; i += 3)
            {
                DieArea area = new DieArea();

                int index1 = triangleIndices[i];
                int index2 = triangleIndices[i + 1];
                int index3 = triangleIndices[i + 2];

                Vector3 vertex1 = vertices[index1];
                Vector3 vertex2 = vertices[index2];
                Vector3 vertex3 = vertices[index3];

                //first calculate normal using cross product, this is not necessarily of length one....
                area.normal = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1);
                //now use that normal to calculate area of the triangle spanned by v1,v2,v3:
                //the magnitude (or length) of the vector a×b, written as ∥a×b∥, 
                //is the area of the parallelogram spanned by a and b, half of it gives us the triangle
                //a = v2-v1 here and b = v3-v1 (see Wikipedia)
                area.area = area.normal.magnitude / 2;
                //now normalize the normal since we like'm at length 1, size matters! ;).
                area.normal.Normalize();

                //average the centerpoint
                area.centerPoint = (vertex1 + vertex2 + vertex3) / 3.0f;

                //lastly add the result to our area list
                result.Add(area);
            }

            return result;
        }

        /**
         * @param pAreasToCombine a list of all areas to search through for areas with similar normals
         * @param pMaxAreaSize (out parameter) the size of the biggest area found
         * @return a list of combined areas that all have unique normals
         */
        private static List<DieArea> combineDieAreasWithSimilarNormals(List<DieArea> pAreasToCombine, out float pMaxAreaSize)
        {
            //process all areas so that we are left with 
            //combined areas that all have a unique normal
            List<DieArea> uniqueAreas = new List<DieArea>();
            float maxAreaSize = 0;

            for (int i = 0; i < pAreasToCombine.Count; i++)
            {
                DieArea currentArea = pAreasToCombine[i];
                DieArea similarArea = findAreaWithSimilarNormal(uniqueAreas, currentArea.normal);

                if (similarArea == null)
                {
                    //if there was no area that had a similar normal, create one:
                    similarArea = new DieArea();
                    similarArea.area = currentArea.area;
                    similarArea.centerPoint = currentArea.centerPoint;
                    similarArea.normal = currentArea.normal;
					similarArea.areaCount = 1;
                    uniqueAreas.Add(similarArea);
                }
                else
                {
					//if there already was an area in the uniqueArea list with a similar normal
					//combine it with the data for the current area
					similarArea.area += currentArea.area;
					//calculcate the new centerpoint using the weighted average of the previous centerpoint with the new one
					similarArea.centerPoint = (similarArea.centerPoint * similarArea.areaCount + currentArea.centerPoint) / (similarArea.areaCount+1);
					similarArea.areaCount++;
                }

                //to be able to do the cutoff calculation in a later step we need to 
                //keep track of the maximum area size thus far
                maxAreaSize = Mathf.Max(maxAreaSize, similarArea.area);
            }

            pMaxAreaSize = maxAreaSize;
            return uniqueAreas;
        }

        /**
         * @param pDieAreas all areas to look through
         * @param pNormal the normal to match
         * @return the first DieArea that matches the given normal or null otherwise
         */
        private static DieArea findAreaWithSimilarNormal(List<DieArea> pDieAreas, Vector3 pNormal)
        {
            return pDieAreas.Find(dieArea => Vector3.Dot(dieArea.normal, pNormal) > 0.999f);
        }

        /**
         * @param pDieAreas all areas to look through
         * @param pMaxAreaSize anything below this size is discarded
         * @return all areas that match the size criterium
         */
        private static List<DieArea> filterDieAreas(List<DieArea> pDieAreas, float pMaxAreaSize)
        {
            return pDieAreas.Where(x => x.area >= pMaxAreaSize).ToList<DieArea>();
        }

        private static void clearLog()
        {
            _stringBuilder.Length = 0;
        }

        private static void log(string pInfo, bool pAppendNewLine = true)
        {
            if (pAppendNewLine)
            {
                _stringBuilder.AppendLine(pInfo);
            }
            else
            {
                _stringBuilder.Append(pInfo);
            }
        }

        public static string LogContents()
        {
            if (_stringBuilder.Length > 0)
            {
                return _stringBuilder.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}