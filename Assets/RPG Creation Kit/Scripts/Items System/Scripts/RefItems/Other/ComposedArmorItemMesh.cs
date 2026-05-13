using NUnit.Framework;
using UnityEngine;

public class ComposedArmorItemMesh : MonoBehaviour
{
    // Objects in here will be disabled/enabled when switching in first/third person (on the third person character)
    public GameObject[] toToggleThirdPerson;

    public void DisableObjects()
    {
        for(int i = 0; i < toToggleThirdPerson.Length; i++)
            toToggleThirdPerson[i].SetActive(false);
    }

    public void EnableObjects()
    {
        for (int i = 0; i < toToggleThirdPerson.Length; i++)
            toToggleThirdPerson[i].SetActive(true);
    }
}
