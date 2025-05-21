using InnerDriveStudios.DiceCreator;
using UnityEngine;

/**
 * Example implementation of IDieSideAware to handle textfields attached to a prefab.
 * Basically just passes on the values of a DieSide as a string to the text of the TextMesh.
 * 
 * @author J.C. Wichman 
 * @copyright Inner Drive Studios 2019
 */
public class DieSideAwareTextDie : MonoBehaviour, IDieSideAware {

	public void SetDieSide(int pIndex, DieSide pDieSide)
	{
		TextMesh textMesh = GetComponent<TextMesh>();
		if (textMesh != null) textMesh.text = pDieSide.ValuesAsString();
	}

}
