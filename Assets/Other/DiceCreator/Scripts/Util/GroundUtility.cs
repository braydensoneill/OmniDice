using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{

	/**
	 * Utility class to floor an item.
	 * 
	 * @author J.C. Wichman
	 * @copyright Inner Drive Studios
	 */
	public class GroundUtility
	{

		/**
		 * Try to put the given object on the first object below it.
		 * @param pGameObject the object to align
		 */
		public static void AlignWithGround(GameObject pGameObject)
		{
			RaycastHit info;
			Ray ray = new Ray(pGameObject.transform.position, Vector3.down);
			if (Physics.Raycast(ray, out info, float.PositiveInfinity))
			{
				//the collision point
				Vector3 result = info.point;
				//the world AABB bounds taking rotation and scaling into account
				Bounds worldBounds = FindBounds(pGameObject);
				//basically we want to adjust our result by the extents of the worldbounds 
				//but we also need to offset for the difference between what unity thinks is our center
				//and what we think is our center
				result.y += worldBounds.extents.y + (pGameObject.transform.position.y - worldBounds.center.y);
				pGameObject.transform.position = result;
			}
			else
			{
				Debug.Log("No floor found.");
			}
		}

		/**
		 * @return the world AABB of the given object taking rotation and scale into account.
		 */
		private static Bounds FindBounds(GameObject pGameObject)
		{
			//check which mesh to use (try to use the simplest one
			Mesh mesh = null;
			MeshCollider collider = pGameObject.GetComponent<MeshCollider>();
			if (collider != null)
			{
				mesh = collider.sharedMesh;
			}
			else
			{
				MeshFilter filter = pGameObject.GetComponent<MeshFilter>();
				mesh = filter.sharedMesh;
			}

			//iterate over all vertices
			Vector3[] vertices = mesh.vertices;

			float minX = float.PositiveInfinity;
			float minY = float.PositiveInfinity;
			float minZ = float.PositiveInfinity;

			float maxX = float.NegativeInfinity;
			float maxY = float.NegativeInfinity;
			float maxZ = float.NegativeInfinity;

			for (int i = 0; i < vertices.Length; i++)
			{
				//taking the vertex in worldspace and updating the min max
				Vector3 point = vertices[i];
				point = pGameObject.transform.TransformPoint(point);

				minX = Mathf.Min(minX, point.x);
				minY = Mathf.Min(minY, point.y);
				minZ = Mathf.Min(minZ, point.z);

				maxX = Mathf.Max(maxX, point.x);
				maxY = Mathf.Max(maxY, point.y);
				maxZ = Mathf.Max(maxZ, point.z);
			}

			//then calculate world bounds based on these min and maxes
			Vector3 center = new Vector3(0.5f * (maxX + minX), 0.5f * (maxY + minY), 0.5f * (maxZ + minZ));
			Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);

			return new Bounds(center, size);
		}

	}

}