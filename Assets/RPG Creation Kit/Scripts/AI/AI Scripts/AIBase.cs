using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using System;
using System.Linq;

namespace RPGCreationKit.AI
{
    /// <summary>
    /// Methods with this attrubutes are allowed to be called from a BehaviourTree (AI_Invoke node)
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public class AIInvokableAttribute : Attribute { }


    /// <summary>
    /// Base info for an AI Entity in the RPG Creation Kit
    /// </summary>
    public class AIBase : EntityFactionable
    {
        public AIGUIDReferences iGUIDReferences;
        public AudioSource audioSource;
        public AISounds aiSounds;
        public Animator m_Anim;

        public bool enableDebugInEditor = false; // if true, it will show all debug related info in the editor
    }
}