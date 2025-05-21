using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Utility class to match GameObjects with materials from a given MaterialSet.
     * Matching is done based on part of the name of a given GameObject. 
     * 
     * The principle is simple:
     * - giving a material, we map its name to a key
     * - giving a gameobject, we map the name to a key as well
     * - if the keys of material and gameobject match we assign the material to the gameobject
     * 
     * To avoid this becoming a O(mn) operation, we first build up a quick dictionary of
     * the material name keys mapped to their respective materials. The reason we do that here
     * instead of in the MaterialSet is that we have no control over when to dirty that dictionary cache.
     * 
     * Current key generation: 
     *  take everything in the given name up to the last _ in the name, 
     *  if no _ is found the whole string is used.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    public static class MaterialSetUtility
    {
        private static Dictionary<string, Material> _key2MaterialMap = new Dictionary<string, Material>();
        private static StringBuilder _stringBuilder = new StringBuilder();

        /**
         * Tries to map each material in the given materialset to the given gameobjects.
         * 
         * @param pMaterialSet the materialset containing all materials to map
         * @param pGameObjects a collection of GameObject to change materials of
         */
        public static string MapMaterialSetToGameObjects(MaterialSet pMaterialSet, IEnumerable<GameObject> pGameObjects)
        {
            clearLog();

            if (pMaterialSet == null)
            {
                log("No valid material set provided.");
            } else
            {
                createMaterialMap(pMaterialSet);
                processAllGameObjects(pGameObjects);
            }

            return logContents();
        }

        private static void processAllGameObjects(IEnumerable<GameObject> pGameObjects)
        {
            log("");
            log("Mapping game objects:");
            log("");
            foreach (GameObject gameObject in pGameObjects) processGameObject(gameObject);
            log("");
            log("Done.");
        }

        /**
         * Creates a simple map of processed material name to material, so that we 
         * can retrieve a material quickly based on a key name instead of a linear search.
         * 
         * @param pMaterialSet the material to create a map for
         */
        private static void createMaterialMap(MaterialSet pMaterialSet)
        {
            log("Processing " + pMaterialSet.description);
            log("Mapping materials:");
            log("");

            _key2MaterialMap.Clear();
            string key = "";
            foreach (Material material in pMaterialSet.materials)
            {
                key = convertToKey(material.name);
                log("[" + material.name + "] => [" + key+"]");
                _key2MaterialMap[key] = material;
            }
        }

        /**
         * Converts the given input to a key for map storage and lookup.
         * In this current implementation we simply take everything in the given name up to the
         * last _ in the name, if no _ is found the whole string is used.
         */
        private static string convertToKey(string pInput)
        {
            int index = pInput.LastIndexOf("_");
            if (index == -1)
            {
                return pInput;
            }
            else
            {
                return pInput.Substring(0, index);
            }
        }

        /**
         * This method processes the name of the GameObject and then tries to find a material with 
         * the same key.
         */
        private static void processGameObject(GameObject pGameObject)
        {
            Assert.IsNotNull(pGameObject, "GameObject is null");
            
            string name = pGameObject.name;
            if (name.Length == 0)
            {
                log("GameObject doesn't have a name, skipping.");
                return;
            }

            MeshRenderer renderer = pGameObject.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                log("GameObject doesn't have a mesh renderer, skipping.");
                return;
            }

            string key = convertToKey(pGameObject.name);
            log("[" + name + "] => ["+key+"] =>", false);
            Material material;
            _key2MaterialMap.TryGetValue(key, out material);

            if (material == null)
            {
                log("No match.");
                return;
            }

            log("[" + material.name+"]");
            renderer.material = material;
        }

        private static void clearLog ()
        {
            _stringBuilder.Length = 0;
        }

        //MaterialSetUtilityDebug_ON
        [Conditional ("MSUD_ON")]
        private static void log(string pInfo, bool pAppendNewLine = true)
        {
            if (pAppendNewLine)
            {
                _stringBuilder.AppendLine(pInfo);
            }else
            {
                _stringBuilder.Append(pInfo);
            }
        }

        private static string logContents()
        {
            if (_stringBuilder.Length > 0)
            {
                return _stringBuilder.ToString();
            } else
            {
                return null;
            }
        }
    }


}