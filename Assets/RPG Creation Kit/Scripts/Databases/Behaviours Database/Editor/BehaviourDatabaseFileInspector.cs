using UnityEngine;
using UnityEditor;

namespace RPGCreationKit
{
    [CustomEditor(typeof(BehaviourDatabaseFile))]
    public class BehaviourDatabaseFileInspector : Editor
    {
        BehaviourDatabaseFile m_target;

        private void OnEnable()
        {
            m_target = (BehaviourDatabaseFile)target;
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Update Behaviors"))
            {
                if (EditorUtility.DisplayDialog("Update Behaviors", "Are you sure you want to update " + m_target.allBehaviours.Count + " behaviors? Updating Behaviors can take several minutes, depending on the number of behaviors and their complexity.", "Do it", "Cancel"))
                {
                    for (int i = 0; i < m_target.allBehaviours.Count; i++)
                        m_target.allBehaviours[i].UpdateChildNodesRef();
                }
            }
        }
    }
}