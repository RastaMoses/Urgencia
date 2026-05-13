using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using RPGCreationKit.BehaviourTree;
using RPGCreationKit.BehaviourTree.Data;
using UnityEditor;

public class BTRunner : MonoBehaviour
{
    public static BTRunner instance;

    public int A = 0;
    public int b = 1;

    [ContextMenu("Debug")]
    void Debugzz()
    {
        UnityEngine.Assertions.Assert.AreEqual(A, b);

        Debug.Log("XD");
    }
}
