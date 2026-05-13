using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using TMPro;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace RPGCreationKit
{
    public class SavePanel : RckSaveSystemPanel
    {
        // Load Game
        [SerializeField] private GameObject loadSavePanel;
        [SerializeField] private GameObject descriptionContent;

        [SerializeField] private Transform saveFilesT;
        [SerializeField] private GameObject savefileUIPrefab;

        [SerializeField] private List<FileInfo> allSaveFiles = new List<FileInfo>();
        [SerializeField] private Button saveButton;
        [SerializeField] private Button deleteButton;

        [SerializeField] private GameObject alertPanel;
        [SerializeField] private OverwriteAlertPanel overwriteAlertPanel;

        //Textes
        [SerializeField] private Image screenshot;
        [SerializeField] private TextMeshProUGUI loadCharName;
        [SerializeField] private TextMeshProUGUI loadLevel;
        [SerializeField] private TextMeshProUGUI loadLocation;
        [SerializeField] private TextMeshProUGUI loadDate;
        [SerializeField] private TextMeshProUGUI loadSaveNumber;
        [SerializeField] private TextMeshProUGUI loadSaveVersion;
        [SerializeField] private GameObject overwriteAlertPanelDefaultPanel;
        private bool ready;
        GameObject firstElementInList;

        public GameObject createNewSavegameButton;
        public UnityEvent OnPanelCloses;

        private void OnEnable()
        {
            ready = false;
            allSaveFiles.Clear();
            SelectedSaveFile = null;
            loadSavePanel.SetActive(true);
            DiscoverSaveFiles();
            SpawnSaveFilesUI();
            ready = true;
        }

        public void Update()
        {
            if (RckInput.isUsingGamepad)
            {
                if (RckInput.input.currentActionMap.FindAction("Back").triggered)
                    ClosePanel();
            }
        }

        /// <summary>
        /// Scans Application.persistentDataPath to get all the savegames 
        /// </summary>
        [ContextMenu("Discover")]
        private void DiscoverSaveFiles()
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

            // Get all files
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/Saves");
            FileInfo[] info = dir.GetFiles("*.json*");

            foreach (FileInfo f in info)
                allSaveFiles.Add(f);

            allSaveFiles.Sort(SortByCreationDate);
        }


        /// <summary>
        /// Spawns the save games as buttons
        /// </summary>
        private void SpawnSaveFilesUI()
        {
            for (int i = 0; i < allSaveFiles.Count; i++)
            {
                SaveGameButtonUI go = Instantiate(savefileUIPrefab, saveFilesT).GetComponent<SaveGameButtonUI>();
                go.text.text = allSaveFiles[i].Name.Substring(0, allSaveFiles[i].Name.IndexOf('.'));
                go.panel = this;
                go.isLoading = false;

                go.LoadSaveFile(allSaveFiles[i]);

                if (firstElementInList == null)
                    firstElementInList = go.gameObject;
            }
        }

        public override void OnSelectedSavegameChanges()
        {
            if (selectedSaveFile == null)
            {
                saveButton.interactable = false;
                deleteButton.interactable = false;
                descriptionContent.SetActive(false);
            }
            else
            {
                saveButton.interactable = true;
                deleteButton.interactable = true;
                descriptionContent.SetActive(true);

                string screenPath = selectedSaveFile.fileInfo.FullName.Substring(0, selectedSaveFile.fileInfo.FullName.IndexOf('.'));
                screenPath += ".png";
                screenshot.sprite = LoadSprite(screenPath);

                loadCharName.text = selectedSaveFile.saveFile.PlayerData.playerName;
                loadLevel.text = selectedSaveFile.saveFile.PlayerData.playerLevel.ToString();
                loadLocation.text = selectedSaveFile.saveFile.PlayerData.playerLocation;
                loadDate.text = selectedSaveFile.saveFile.SaveFileData.saveDate;
                loadSaveNumber.text = selectedSaveFile.saveFile.SaveFileData.saveNumber.ToString();
                loadSaveVersion.text = selectedSaveFile.saveFile.SaveFileData.fileVersion.ToString();
            }
        }

        private Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (System.IO.File.Exists(path))
            {
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }
            return null;
        }


        public void ConfirmLoadSave()
        {
            Time.timeScale = 1.0f;

            if (selectedSaveFile != null)
                SaveSystemManager.instance.LoadSaveGame(selectedSaveFile.fileInfo, selectedSaveFile.saveFile);
        }

        public void CreateNewSavegame()
        {
            StartCoroutine(CreateNewSavegameTask());
        }

        IEnumerator CreateNewSavegameTask()
        {
            alertPanel.SetActive(true);
            SaveSystemManager.instance.SaveOnNewFile();

            // Clear savegame
            foreach (Transform t in saveFilesT.transform)
                if (t.gameObject.name != "_NewSaveButton_")
                    Destroy(t.gameObject);

            OnEnable();
            alertPanel.SetActive(false);

            yield return new WaitForEndOfFrame();
            ClosePanel();
        }

        public void ClosePanel()
        {
            loadSavePanel.SetActive(false);
            SelectedSaveFile = null;

            // Clear savegame
            foreach (Transform t in saveFilesT.transform)
                if (t.gameObject.name != "_NewSaveButton_")
                    Destroy(t.gameObject);

            OnPanelCloses.Invoke();
        }

        public void OverwriteButton()
        {
            if (selectedSaveFile != null)
            {
                overwriteAlertPanel.previouslySelectedObject = EventSystem.current.currentSelectedGameObject;

                overwriteAlertPanel.description.text = "Are you sure you want to overwrite the savegame: " + selectedSaveFile.fileInfo.Name.Substring(0, selectedSaveFile.fileInfo.Name.IndexOf('.')) + "?";
                overwriteAlertPanel.gameObject.SetActive(true);

                if (RckInput.isUsingGamepad)
                {
                    if(overwriteAlertPanelDefaultPanel != null)
                        EventSystem.current.SetSelectedGameObject(overwriteAlertPanelDefaultPanel);
                }
            }
        }

        public void ConfirmOverwriteCurrentSave()
        {
            StartCoroutine(ConfirmOverwriteCurrentSaveTask());
        }

        IEnumerator ConfirmOverwriteCurrentSaveTask()
        {
            if (selectedSaveFile != null)
            {
                overwriteAlertPanel.gameObject.SetActive(false);

                alertPanel.SetActive(true);
                SaveSystemManager.instance.SaveAndOverwrite(selectedSaveFile.fileInfo, selectedSaveFile.saveFile);
                alertPanel.SetActive(false);

                SelectedSaveFile = selectedSaveFile;

                allSaveFiles.Sort(SortByCreationDate);
                // Clear savegame
                foreach (Transform t in saveFilesT.transform)
                    if (t.gameObject.name != "_NewSaveButton_")
                        Destroy(t.gameObject);

                OnEnable();

                yield return new WaitForEndOfFrame();

                ClosePanel();
            }
        }

        public static int SortByCreationDate(FileInfo f1, FileInfo f2)
        {
            var d1 = File.GetLastWriteTime(f1.FullName);
            var d2 = File.GetLastWriteTime(f2.FullName);
            return System.DateTime.Compare(d2, d1);
        }

        /// <summary>
        /// Selects the first button that represents a savegame
        /// </summary>
        public void SelectFirstElementInList()
        {
            EventSystem.current.SetSelectedGameObject(createNewSavegameButton);
            //StartCoroutine(SelectFirstElementTask());
        }

        IEnumerator SelectFirstElementTask()
        {
            while (!ready)
                yield return null;

            if (firstElementInList != null)
                EventSystem.current.SetSelectedGameObject(firstElementInList);
        }
    }
}