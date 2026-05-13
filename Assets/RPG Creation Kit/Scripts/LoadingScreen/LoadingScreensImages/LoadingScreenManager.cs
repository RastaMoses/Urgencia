using RPGCreationKit.CellsSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGCreationKit
{
    public static class LoadingScreenInfo
    {
        public static List<string> whatToLoadPriority = new List<string>();
        public static List<string> whatToLoad = new List<string>();
    }

    public class LoadingScreenManager : MonoBehaviour
    {
        public bool isFinished = false;
        public static LoadingScreenManager instance;
        private void Awake()
        {
            instance = this;
        }

        public GameObject loadingText;
        public Animation fadeInOut;

        public List<GameObject> loadingScreens;
        private Stack<GameObject> usedLoadingScreens = new Stack<GameObject>();

        private void Start()
        {
            StartLoading();
        }

        /// <summary>
        /// Used for very small loading < 1 sec
        /// </summary>
        /// <param name="_time"></param>
        public void StartLoadingAfterTime(float _time)
        {
            Invoke("StartLoading", _time);
        }

        List<AsyncOperation> priorityOperations = new List<AsyncOperation>();
        List<AsyncOperation> operations = new List<AsyncOperation>();
        public void StartLoading()
        {
            for (int i = 0; i < loadingScreens.Count; i++)
                usedLoadingScreens.Push(loadingScreens[i]);

            loadingText.SetActive(true);

            ChangeLoadingScreen();

            // Load 
            isFinished = false;

            StartCoroutine(IsLoadingFinished());
        }


        public void ChangeLoadingScreen()
        {
            // Pick a random loading screen that wasn't used before
            if (usedLoadingScreens.Count > 0)
                StartCoroutine(UseLoadingScreen(usedLoadingScreens.Pop()));
            else
            {
                for (int i = 0; i < loadingScreens.Count; i++)
                    usedLoadingScreens.Push(loadingScreens[i]);

                if(loadingScreens.Count != 0)
                    ChangeLoadingScreen();
            }
        }

        private GameObject previousObj;
        public IEnumerator UseLoadingScreen(GameObject _obj)
        {
            // Fadeout
            fadeInOut.Play("FadeIn");

            while (fadeInOut.isPlaying)
                yield return null;

            if (previousObj != null)
                previousObj.SetActive(false);

            fadeInOut.Play("FadeOut");

            // Enable screen
            _obj.SetActive(true);
            var objAnim = _obj.GetComponent<Animation>();

            while (objAnim.isPlaying)
                yield return null;

            previousObj = _obj;

            ChangeLoadingScreen();
        }

        public IEnumerator IsLoadingFinished()
        {
            Stack<string> pOp = new Stack<string>();
            for (int i = LoadingScreenInfo.whatToLoadPriority.Count - 1; i >= 0; i--)
                pOp.Push(LoadingScreenInfo.whatToLoadPriority[i]);

            while (pOp.Count > 0)
            {
                var op = SceneManager.LoadSceneAsync(pOp.Pop(), LoadSceneMode.Additive);
                priorityOperations.Add(op);
                yield return null;
            }

            while (priorityOperations.Count > 0)
            {
                for (int i = 0; i < priorityOperations.Count; i++)
                {
                    if (priorityOperations[i].isDone)
                        priorityOperations.Remove(priorityOperations[i]);

                    yield return null;
                }

                yield return null;
            }

            Stack<string> sOp = new Stack<string>();
            for (int i = LoadingScreenInfo.whatToLoad.Count-1; i >= 0; i--)
                sOp.Push(LoadingScreenInfo.whatToLoad[i]);

            while(sOp.Count > 0)
            {
                var op = SceneManager.LoadSceneAsync(sOp.Pop(), LoadSceneMode.Additive);
                operations.Add(op);
                yield return null;
            }

            while (operations.Count > 0)
            {
                for (int i = 0; i < operations.Count; i++)
                {
                    if (operations[i].isDone)
                        operations.Remove(operations[i]);

                    yield return null;
                }

                yield return null;
            }

            if (WorldManager.instance != null) 
            {
                while (WorldManager.instance.isLoading)
                    yield return null;
            }

            OnLoadEnds();
        }

        public void OnLoadEnds()
        {
            LoadingScreenInfo.whatToLoad.Clear();
            LoadingScreenInfo.whatToLoadPriority.Clear();

            isFinished = true;

            SceneManager.UnloadSceneAsync("_LoadingScreen_");
        }
    }
}