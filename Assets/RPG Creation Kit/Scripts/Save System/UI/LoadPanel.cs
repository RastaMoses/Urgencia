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

/// <summary>
/// Contains base information for a save system panel such as the load/save panels
/// </summary>
public class RckSaveSystemPanel : MonoBehaviour
{
    protected SaveGameButtonUI selectedSaveFile;
    public SaveGameButtonUI SelectedSaveFile
    {
        get
        {
            return selectedSaveFile;
        }
        set
        {
            selectedSaveFile = value;

            OnSelectedSavegameChanges();
        }
    }

    public virtual void OnSelectedSavegameChanges()
    {

    }

}

namespace RPGCreationKit
{
    public class LoadPanel : RckSaveSystemPanel
    {
        // Load Game
        [SerializeField] private GameObject loadSavePanel;
        [SerializeField] private GameObject descriptionContent;

        [SerializeField] private Transform saveFilesT;
        [SerializeField] private GameObject savefileUIPrefab;

        [SerializeField] private List<Button> allButtons = new List<Button>();
        [SerializeField] private List<FileInfo> allSaveFiles = new List<FileInfo>();
        public Button selectedButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;

        //Textes
        [SerializeField] private Image screenshot;
        [SerializeField] private TextMeshProUGUI loadCharName;
        [SerializeField] private TextMeshProUGUI loadLevel;
        [SerializeField] private TextMeshProUGUI loadLocation;
        [SerializeField] private TextMeshProUGUI loadDate;
        [SerializeField] private TextMeshProUGUI loadSaveNumber;
        [SerializeField] private TextMeshProUGUI loadSaveVersion;

        [SerializeField] private GameObject deleteConfirmationGameobject;
        [SerializeField] private TextMeshProUGUI deleteConfirmationText;
        [SerializeField] private Button deleteDefaultButton;



        GameObject firstElementInList;
        bool ready;

        public UnityEvent OnPanelCloses;

        private void OnEnable()
        {
            ready = false;
            // Clear savegame
            foreach (Transform go in saveFilesT.transform)
                Destroy(go.gameObject);

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

                go.indexInList = i;
                go.LoadSaveFile(allSaveFiles[i]);
                go.isLoading = true;

                if (firstElementInList == null)
                    firstElementInList = go.gameObject;

                allButtons.Add(go.GetComponent<Button>());
            }
        }

        public override void OnSelectedSavegameChanges()
        {
            if (selectedSaveFile == null)
            {
                loadButton.interactable = false;
                deleteButton.interactable = false;
                descriptionContent.SetActive(false);
            }
            else
            {
                loadButton.interactable = true;
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

        public void ConfirmDeleteSaveGame()
        {
            if (selectedSaveFile != null)
            {
                DeleteSaveGame(selectedSaveFile);
                Destroy(selectedSaveFile.gameObject);

                if(RckInput.isUsingGamepad && allButtons.Count > 1 && allButtons[0] != null)
                    allButtons[0].Select();

                SelectedSaveFile = null;
            }
        }

        public void DeleteSaveGame(SaveGameButtonUI _save)
        {
            string filePath = _save.fileInfo.FullName;

            if (File.Exists(filePath))
                File.Delete(filePath);

            string imagePath = filePath.Replace(".json", ".png");

            if (File.Exists(imagePath))
                File.Delete(imagePath);

            deleteConfirmationGameobject.SetActive(false);
        }

        public void ClosePanel()
        {
            loadSavePanel.SetActive(false);
            SelectedSaveFile = null;

            // Clear savegame
            foreach (Transform t in saveFilesT.transform)
                Destroy(t.gameObject);

            OnPanelCloses.Invoke();
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
            StartCoroutine(SelectFirstElementTask());
        }

        IEnumerator SelectFirstElementTask()
        {
            while (!ready)
                yield return null;

            if (firstElementInList != null)
                EventSystem.current.SetSelectedGameObject(firstElementInList);
        }

        public void LoadLastSave()
        {
            string savesDir = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(savesDir))
            {
                Debug.LogWarning("No Saves directory found.");
                return;
            }

            // Find newest *.json
            DirectoryInfo dir = new DirectoryInfo(savesDir);
            FileInfo[] jsonFiles = dir.GetFiles("*.json", SearchOption.TopDirectoryOnly);

            if (jsonFiles == null || jsonFiles.Length == 0)
            {
                Debug.LogWarning("No save files found to load.");
                return;
            }

            // Sort newest first by last write time
            System.Array.Sort(jsonFiles, (a, b) =>
                System.DateTime.Compare(File.GetLastWriteTime(b.FullName), File.GetLastWriteTime(a.FullName)));

            FileInfo newest = jsonFiles[0];

            try
            {
                // Read + deserialize
                string json = File.ReadAllText(newest.FullName);
                SaveFile saveFile = JsonUtility.FromJson<SaveFile>(json);

                if (saveFile == null)
                {
                    Debug.LogError("Failed to deserialize save file:" + newest.FullName);
                    return;
                }

                // Load it
                SaveSystemManager.instance.LoadSaveGame(newest, saveFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error loading last save: " + newest.FullName);
            }
        }

        public void DeleteButton()
        {
            selectedButton = selectedSaveFile.GetComponent<Button>();

            deleteConfirmationText.text = "Are you sure you want to delete the savegame: \n\n" + selectedSaveFile.fileInfo.Name.Replace(".json", "") + "?";
            deleteConfirmationGameobject.SetActive(true);

            deleteDefaultButton.Select();
            StartCoroutine(ConfirmationButtonSelectionTask());
        }

        IEnumerator ConfirmationButtonSelectionTask()
        {
            yield return new WaitForSeconds(1);
            deleteDefaultButton.Select();
        }

        public void CloseDeleteConfirmation()
        {
            deleteConfirmationGameobject.SetActive(false);
            selectedButton.Select();
        }
    }
}