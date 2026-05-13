using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using RPGCreationKit.DialogueSystem;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.CellsSystem;
using UnityEditorInternal;

namespace RPGCreationKit
{
    public class ItemSoundsSetterEditor : EditorWindow
    {
        bool isReady = false;
        private List<Item> allItems;

        public AudioClip OnPickUp;

        public AudioClip OnAddedInInventory;
        public AudioClip OnRemovedFromInventory;


        public Material mat;
        public GameObject[] ppl;


        public static void ShowWindow()
        {
            EditorWindow thisWindow = GetWindow(typeof(ItemSoundsSetterEditor));

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.RPGCKEditorWindowIcon);
            GUIContent titleContent = new GUIContent("ItemSoundsSetterEditor ", icon);
            thisWindow.titleContent = titleContent;

        }

        private void Init()
        {
            FillAllLists();

        }

        private void OnGUI()
        {
            if (!isReady)
                Init();

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty pplprop = so.FindProperty("ppl");

            mat = (Material)EditorGUILayout.ObjectField(mat, typeof(Material), true);
            EditorGUILayout.PropertyField(pplprop, true); // True means show children
            /*
            OnPickUp = (AudioClip)EditorGUILayout.ObjectField(OnPickUp, typeof(AudioClip), true);
            OnAddedInInventory = (AudioClip)EditorGUILayout.ObjectField(OnAddedInInventory, typeof(AudioClip), true);
            OnRemovedFromInventory = (AudioClip)EditorGUILayout.ObjectField(OnRemovedFromInventory, typeof(AudioClip), true);
            */

            if (GUILayout.Button("GOO"))
            {
                for(int i = 0; i < ppl.Length; i++)
                {

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(ppl[i]));
                    
                    var a = prefab.transform.Find("GFX");

                    var head = a.Find("Head");
                    var upperbody = a.Find("Upperbody");
                    var arms = a.Find("Arms");
                    var hands = a.Find("Hands");
                    var lowerbody = a.Find("Lowerbody");
                    var feet = a.Find("Feet");

                    head.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
                    upperbody.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
                    arms.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
                    hands.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
                    lowerbody.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;
                    feet.gameObject.GetComponent<SkinnedMeshRenderer>().material = mat;

                    EditorUtility.SetDirty(prefab);
                    AssetDatabase.SaveAssets();
                }

                /*
                for(int i = 0; i < allItems.Count; i++)
                {
                    allItems[i].sOnPickUp = OnPickUp;
                    allItems[i].sOnAddedInInventory = OnAddedInInventory;
                    allItems[i].sOnRemovedFromInventory = OnRemovedFromInventory;
                }
                */
            }

        }

        private void OnFocus()
        {
        }

        private void FillAllLists()
        {
            allItems = GetAllInstances<Item>();
        }

        public static List<T> GetAllInstances<T>() where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            List<T> a = new List<T>(guids.Length);

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }

            return a;
        }
    }
}