using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using InnerDriveStudios.DiceCreator;

/**
	* SideUtility is a simple editor window that allows you to attach a specific prefab to each and every side of a die.
	* If the prefab has a component that implements IDieSideAware it will also pass the DieSide info to that component on the
	* instantiated prefab.
	* 
	* @author J.C. Wichman / Inner Drive Studios
	*/
public class SideUtility : EditorWindow
{
	[SerializeField]
	private DieSides[] _dice = null;
	private GameObject _prefab;
	private bool _flip = true;
	private float _offset = 0.04f;
	private float _scale = 1;

	SerializedObject _dieSidesSO;
	SerializedProperty _dieSidesSP;

	[MenuItem("IDS/DiceCreator/Die side utility")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow<SideUtility>();
	}

	private void OnEnable()
	{
		//this is required to reuse the default unity array editor
		_dieSidesSO = new SerializedObject(this);
		_dieSidesSP = _dieSidesSO.FindProperty("_dice");
		_dieSidesSP.isExpanded = true;
		_dieSidesSP.arraySize = Mathf.Max(_dieSidesSP.arraySize, 1);

		titleContent = new GUIContent("Side Utility");
	}

	private void OnGUI()
	{
		if (_dieSidesSP == null) return;

		GUI.enabled = true;
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Which dice would you like to process?", EditorStyles.boldLabel);

		EditorGUILayout.PropertyField(_dieSidesSP, true); // True means show children
		_dieSidesSO.ApplyModifiedProperties();            // Remember to apply modified properties

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Which prefab would you like to attach to each die side?", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Prefab to attach:", "This prefab will be attached to each side of the Die specified above."));
		_prefab = (GameObject)EditorGUILayout.ObjectField(_prefab, typeof(GameObject), false);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Setting to apply to each created instance:", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Flip normals:", "Toggle if your prefab instance is facing the wrong way."));
		_flip = EditorGUILayout.Toggle(_flip);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Offset in face direction:", "Adjust if your prefab instance has the wrong distance from the Die face."));
		_offset = EditorGUILayout.FloatField(_offset);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(new GUIContent("Scale:", "Adjust if your prefab instance has the scale on the die face."));
		_scale = EditorGUILayout.Slider(_scale, 0.01f, 5);
		EditorGUILayout.EndHorizontal();

		GUI.enabled = _dice != null && _dice.Length > 0 && _prefab != null;

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if (GUILayout.Button ("Remove children"))
		{
			removeChildrenForAllDice();
		}

		if (GUILayout.Button("Go !"))
		{
			if (EditorUtility.DisplayDialog("Warning", "If a die already has children, pressing OK will remove these children...", "OK", "Cancel"))
			{
				removeChildrenForAllDice();
				processChildrenForAll();
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
		}

		GUI.enabled = true;
	}

	private void removeChildrenForAllDice()
	{
		try
		{
			Array.ForEach(_dice, removeChildrenForDie);
		}
		catch (Exception e)
		{
			EditorUtility.DisplayDialog(
				"Error",
				"An error occurred while removing the children:" +
				e.StackTrace,
				"OK"
			);
		}
	}

	private void removeChildrenForDie(DieSides pDie)
	{
		if (pDie == null) return;

		while (pDie.transform.childCount > 0) DestroyImmediate(pDie.transform.GetChild(0).gameObject);
	}

	private void processChildrenForAll()
	{
		try
		{
			Array.ForEach(_dice, processDie);
		}
		catch (Exception e)
		{
			EditorUtility.DisplayDialog(
				"Error",
				"An error occurred while removing the children:" +
				e.StackTrace,
				"OK"
			);
		}
	}

	private void processDie(DieSides pDie)
	{
		if (pDie == null) return;

		for (int i = 0; i < pDie.dieSideCount; i++)
		{
			//get the die side itself to calculate the position and orientation of the prefab to attach
			DieSide dieSide = pDie.GetDieSide(i);
			Vector3 position = dieSide.centerPoint + _offset * dieSide.normal;
			Quaternion orientation = Quaternion.LookRotation(_flip ? -dieSide.normal : dieSide.normal);

			//attach prefab first
			GameObject attachment = Instantiate(_prefab, pDie.transform);
			//then make sure all LOCAL settings are set correctly (so that it also performs correctly in worldspace)
			attachment.transform.localScale = Vector3.one * _scale;
			attachment.transform.localPosition = position;
			attachment.transform.localRotation = orientation;

			attachment.name = "" + i;

			//if the prefab is diesideaware, pass the index and dieside to the instance created from the prefab
			IDieSideAware dieSideAware = attachment.GetComponent<IDieSideAware>();
			if (dieSideAware != null) dieSideAware.SetDieSide(i, dieSide);
		}
	}
	
}
