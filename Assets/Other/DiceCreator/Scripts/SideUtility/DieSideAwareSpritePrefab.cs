using InnerDriveStudios.DiceCreator;
using UnityEngine;

/**
 * Example implementation of IDieSideAware to handle attached sprites to a prefab.
 * This assumes the prefab has an array of Sprite matching with DieSide values and a SpriteRenderer to assign them to.
 * 
 * @author J.C. Wichman 
 * @copyright Inner Drive Studios 2019
 */
public class DieSideAwareSpritePrefab : MonoBehaviour, IDieSideAware {

	public Sprite[] sprites;

	public void SetDieSide(int pIndex, DieSide pDieSide)
	{
		//if we have sprites and die side values and the value is within the range of the sprites that we have, assign it
		if (sprites != null && pDieSide.values != null && pDieSide.values[0] >= 0 && pDieSide.values[0] < sprites.Length)
		{
			GetComponent<SpriteRenderer>().sprite = sprites[pDieSide.values[0]];
		}
	}
}
