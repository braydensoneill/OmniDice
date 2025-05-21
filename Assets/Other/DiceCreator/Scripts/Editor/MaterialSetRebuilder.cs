using InnerDriveStudios.DiceCreator;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/**
 * MaterialSetRebuilder is a utility script to rebuild all MaterialSets including the final MaterialSetCollection
 * from all material folders in the Dice Creator Package. 
 * 
 * Base functionality:
 * - delete all existing materialsets from PathConstants.MATERIALS_FOLDER (by default Assets/InnerDriveStudios/DiceCreator/Materials)
 * - go through all folders in PathConstants.MATERIALS_FOLDER WITHOUT a _ in front of their name,
 *		- go through all materials found in each folder and:
 *			- rename the material to its basename + its parent material folder name
 *			  eg BlackGloss/???_Whatever.mat becomes BlackGloss/???_BlackGlossMaterial.mat
 *		- create a MaterialSet instance corresponding to the name of found folder and 
 *		  add all materials within that folder to the MaterialSet
 * - Lookup or create (if non existent) _MaterialsetCollection and fill it all created MaterialSet's
 * 
 * After rebuilding all material set they can be tested by running the default 001_gettingstarted scene
 * (all material sets should show up in the dropdownbox of the material manager).
 * 
 * Usage scenario (also see manual):
 *	- Select the PlasticPurple material folder, duplicate it and rename it to PlasticGreen
 *	- Run the MaterialSetRebuilder through the IDS menu option to:
 *		- rename all materials in the PlasticGreen to D2_PlasticGreenMaterial etc
 *		- create a MaterialSet called PlasticGreen
 *	- Now you can adjust all values for the PlasticGreenMaterials and quickly assign them to the dice
 *	
 *	Known issues:
 *	- for some reason the process triggers a 'was already deleted' exception for no apparent reason.
 *	Workaround:
 *	- just run the script again
 */
public class MaterialSetRebuilder : Editor {

	[MenuItem("IDS/DiceCreator/Rebuild material sets")]
	private static void rebuildMaterialSets()
	{
		deleteExistingMaterialSets();

		//create an empty list to pass in the different methods to operate upon
		List<MaterialSet> materialSetList = new List<MaterialSet>();
		//fill thee list with the created material sets
		createMaterialSetsForAllMaterialFolders(materialSetList);
		//and now pass it in again to combine all sets into a collection
		createOrUpdateMaterialSetCollection(materialSetList);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	private static void deleteExistingMaterialSets()
	{
		Debug.Log("Deleting existing material sets...");
		//find all GUIDs that reference an asset which is an instance of the MaterialSet scriptable object in the materials folder
		string[] materialSetGUIDList = AssetDatabase.FindAssets("t:MaterialSet", new[] { PathConstants.MATERIALS_FOLDER });

		//delete all found materialsets so we can rebuild them
		foreach (string materialSetGUID in materialSetGUIDList)
		{
			string materialSetPath = AssetDatabase.GUIDToAssetPath(materialSetGUID);
			AssetDatabase.DeleteAsset(materialSetPath);
			Debug.Log("Deleted " + materialSetPath);
		}
	}

	private static void createMaterialSetsForAllMaterialFolders(List<MaterialSet> pMaterialSetList)
	{
		string[] subfolders = AssetDatabase.GetSubFolders(PathConstants.MATERIALS_FOLDER);
		foreach (string subfolder in subfolders)
		{
			string subfolderName = Path.GetFileName(subfolder);
			if (subfolderName.StartsWith("_"))
			{
				Debug.Log("Skipping " + subfolderName);
			} else
			{
				pMaterialSetList.Add (createMaterialSetAssetForSubFolder(subfolder));
			}
		}
	}

	private static MaterialSet createMaterialSetAssetForSubFolder(string pSubfolder)
	{
		Debug.Log("Processing " + pSubfolder);
		//first create an instance of a MaterialSet scriptable object
		MaterialSet materialSet = createMaterialSetForSubFolder(pSubfolder);
		//and then store it in an asset
		AssetDatabase.CreateAsset(materialSet, pSubfolder+".asset");
		return materialSet;
	}

	private static MaterialSet createMaterialSetForSubFolder (string pSubfolder)
	{
		//get the last path from the subfolder, that will be the name of our material set
		string materialSetName = Path.GetFileName(pSubfolder);
		Debug.Log("Creating material set for:"+materialSetName);

		MaterialSet materialSet = ScriptableObject.CreateInstance<MaterialSet>();
		//turn MyMaterialSet into "My Material Set"
		string[] nameParts = Regex.Split(materialSetName, @"(?<!^)(?=[A-Z])");
		materialSet.description = (nameParts.Length == 0)?materialSetName:string.Join (" ", nameParts);
		materialSet.materials = getAllMaterialsInFolder(pSubfolder);
		return materialSet;
	}

	private static Material[] getAllMaterialsInFolder(string pSubfolder)
	{
		string materialSetName = Path.GetFileName(pSubfolder);
		//get all material GUIDs in the given material set sub folder
		string[] materialGUIDList= AssetDatabase.FindAssets("t:Material", new[] { pSubfolder });

		List<Material> materials = new List<Material>();
		foreach (string materialGUID in materialGUIDList)
		{
			string materialPath = AssetDatabase.GUIDToAssetPath(materialGUID);
			string materialName = Path.GetFileName(materialPath);

			Material material = (Material)AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material));
			materials.Add(material);
			Debug.Log(material.name + " found.");

			//rename the material file in the folder
			if (materialName.Contains("_"))
			{
				string newMaterialName = materialName.Substring(0, materialName.IndexOf("_") + 1) + materialSetName + "Material";
				AssetDatabase.RenameAsset(materialPath, newMaterialName);
			}
		}

		return materials.ToArray();
	}

	private static void createOrUpdateMaterialSetCollection(List<MaterialSet> pMaterialSetList)
	{
		string[] materialSetCollectionGUIDList = AssetDatabase.FindAssets("t:MaterialSetCollection", new[] { PathConstants.MATERIALS_FOLDER});

		MaterialSetCollection msc = null;

		if (materialSetCollectionGUIDList.Length == 0)
		{
			Debug.Log("Creating new material set collection...");
			msc = ScriptableObject.CreateInstance<MaterialSetCollection>();
			AssetDatabase.CreateAsset(msc, PathConstants.MATERIALS_FOLDER + "/_MaterialSetCollection.asset");
		} else {
			Debug.Log("Updating existing material set collection...");
			msc = AssetDatabase.LoadAssetAtPath<MaterialSetCollection>(AssetDatabase.GUIDToAssetPath(materialSetCollectionGUIDList[0]));
		}

		msc.materialSets = pMaterialSetList.ToArray();
	}
}
