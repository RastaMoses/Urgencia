using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoItemAdder : MonoBehaviour
{
    public bool addDemoItem = false;

    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("demoItem");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
