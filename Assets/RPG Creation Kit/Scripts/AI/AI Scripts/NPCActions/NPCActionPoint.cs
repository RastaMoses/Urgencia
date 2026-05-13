using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;
using UnityEditor;

namespace RPGCreationKit.AI
{

    public enum TypeActionPoint
    {
        Rest = 0,
        Eat = 1,
        Sleep = 2,
        Mount = 3
    };

    public enum DynamicObjectBone
    {
        Rhand = 0,
        Lhand = 1
    };

    public class NPCActionPoint : MonoBehaviour
    {
        public string actionPointID;
        public TypeActionPoint actionType;

        // Editor - Debug // TOCHANGE - SHALL WE USE A CUSTOM INSPECTOR? IT MAY ERASE THE DRAWGIZMOS THING WHICH IS BAD!
        [SerializeField] private bool showDummies = true;

        [SerializeField] private GameObject dummyFirstKeyframe;
        [SerializeField] private GameObject dummyLastKeyframe;

        // Runtime
        public bool isOccupied = false;
        public Transform pivotPoint;
        public AnimationClip npcAction;
        public AnimationClip npcActionReturn;

        public Entity entityUsingThisPoint;

        public bool spawnDynamicObject;
        public GameObject dynamicObject;
        public DynamicObjectBone dynamicObjectBone;

        [ContextMenu("Regenerate ID")]
        public void RegenerateGUIDStr()
        {
            actionPointID = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public virtual void Start()
        {
            if (dummyFirstKeyframe != null)
                dummyFirstKeyframe.SetActive(false);

            if (dummyLastKeyframe != null)
                dummyLastKeyframe.SetActive(false);
        }

        /// <summary>
        /// This is called when isOccupied is set to true, so when an NPC wants to use this point (it doesn't mean he is currently using it)
        /// </summary>
        public virtual void OnUserAssigned(Entity _entity = null)
        {
            isOccupied = true;
            entityUsingThisPoint = _entity;
        }

        /// <summary>
        /// This is called when the NPC is playing the 'enter' animation to the point. (When the NPC is sitting for example)
        /// </summary>
        public virtual void OnUserUses(Entity _entity = null)
        {
            
        }

        /// <summary>
        /// This is called when the npc is playing the 'return' animation of the point. (When the NPC is getting up from the chair)
        /// </summary>
        public virtual void OnUserIsLeaving(Entity _entity = null)
        {

        }

        /// <summary>
        /// This is called when the NPC that was using this point has completly been released.
        /// </summary>
        public virtual void OnUserHasLeft(Entity _entity = null)
        {
            entityUsingThisPoint = null;
            isOccupied = false;
        }

        /// <summary>
        /// This is called when the NPC that was reaching this point gets reset and it should no more reach it (when it enters in combat)
        /// </summary>
        public virtual void OnUserForceLeaves()
        {
            entityUsingThisPoint = null;
            isOccupied = false;
        }


#if UNITY_EDITOR
        private void OnEnable()
        {
            if(Application.isPlaying)
            {
                if(dummyFirstKeyframe != null)
                    dummyFirstKeyframe.SetActive(false);

                if(dummyLastKeyframe != null)
                    dummyLastKeyframe.SetActive(false);
            }
        }

        [ContextMenu("Sample")]
        void SampleDummies()
        {
            if (npcAction != null && showDummies)
            {
                if(dummyFirstKeyframe)
                    npcAction.SampleAnimation(dummyFirstKeyframe, 0.0f);

                if(dummyLastKeyframe)
                    npcAction.SampleAnimation(dummyLastKeyframe, npcAction.length);

                SceneView.RepaintAll();
            }
        }

        void ShowHideDummies(bool show)
        {
            if (dummyFirstKeyframe)
                dummyFirstKeyframe.SetActive(show);

            if (dummyLastKeyframe)
                dummyLastKeyframe.SetActive(show);

            SceneView.RepaintAll();
        }

        private void OnDrawGizmosSelected()
        {
            ShowHideDummies(showDummies);
            SampleDummies();
        }
#endif

    }
}