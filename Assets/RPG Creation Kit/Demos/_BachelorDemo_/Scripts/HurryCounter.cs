using UnityEngine;

public class HurryCounter : MonoBehaviour
{
    public int hurryCount = 0;

    public void IncrementHurryCount()
    {
        hurryCount++;
        Debug.Log("Hurry count incremented to: " + hurryCount);
    }
}
