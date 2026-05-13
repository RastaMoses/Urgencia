using RPGCreationKit;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPGCreationKit.SaveSystem
{
    public class SaveSystemManager : MonoBehaviour
    {
        public static SaveSystemManager instance;
        public static bool CAN_SAVE = true;

        public Texture2D defaultSavegameImage;

        // Used only for RCK Demos
        public Texture2D demo2SavegameImage;
        public Texture2D demo3SavegameImage;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
                Destroy(this.gameObject);
        }

        [SerializeField] public SaveFile saveFile;
        public FileInfo saveInfo;

        public void LoadSaveGame(FileInfo _info, SaveFile _file)
        {
            saveFile.LootingPointsData.allLootingPoints.Clear();
            saveFile.ItemsInWorldData.allItemsInWorld.Clear();

            saveInfo = _info;
            saveFile = _file;

            LoadingScreenInfo.whatToLoadPriority.Add("_PersistentReferences_");
            LoadingScreenInfo.whatToLoad.Add("_WorldLoader_");

            SceneManager.LoadScene("_LoadingScreen_");
        }

        public void SaveAndOverwrite(FileInfo _info, SaveFile _saveFile)
        {
            saveInfo = _info;

            System.DateTime date = System.DateTime.Now;
            // Fill the relevant save info

            // SaveFileData
            saveFile.SaveFileData.saveType = RCKSettings.SAVE_TYPE_VERSION;
            saveFile.SaveFileData.fileVersion = RCKSettings.FILE_SAVE_VERSION;
            saveFile.SaveFileData.saveNumber++;
            saveFile.SaveFileData.saveDate = date.ToShortDateString() + " - " + date.Hour + ":" + date.Minute;


            // PlayerData
            //saveFile.PlayerData.playerName = name;
            saveFile.PlayerData.playerLevel = EntityAttributes.PlayerAttributes.curLevel;
            //saveFile.PlayerData.playerSex = sex;
            saveFile.PlayerData.playerLocation = WorldManager.instance.currentCenterCell.cellName;
            saveFile.PlayerData.playerWorldspaceID = WorldManager.instance.currentWorldspace.worldSpaceID;
            saveFile.PlayerData.playerCellID = WorldManager.instance.currentCenterCell.ID;
            saveFile.PlayerData.playerPos = RckPlayer.instance.transform.position;
            saveFile.PlayerData.playerRot = RckPlayer.instance.transform.rotation;
            saveFile.PlayerData.mouseRotX = RckPlayer.instance.mouseLook.xRotation;
            saveFile.PlayerData.playerCrouched = RckPlayer.instance.IsCrouching;
            saveFile.PlayerData.weaponDrawn = PlayerCombat.instance.weaponDrawn;
            saveFile.PlayerData.isInThirdPerson = RckPlayer.instance.isInThirdPerson;

            saveFile.PlayerData.recoversSet = true;
            saveFile.PlayerData.recoverStaminaAmount = RckPlayer.instance.recoverStaminaAmount;
            saveFile.PlayerData.recoverAfterActionDelay = RckPlayer.instance.recoverAfterActionDelay;
            saveFile.PlayerData.recoverHealthAmount = RckPlayer.instance.recoverHealthAmount;
            saveFile.PlayerData.recoverAfterHitDelay = RckPlayer.instance.recoverAfterHitDelay;

            saveFile.PlayerData.recoverManaAmount = RckPlayer.instance.recoverManaAmount;
            saveFile.PlayerData.recoverManaAfterUseDelay = RckPlayer.instance.recoverAfterManaUseDelay;

            saveFile.PlayerData.isMounted = RckPlayer.instance.isMounted;
            if (RckPlayer.instance.isMounted)
                saveFile.PlayerData.mountedEntityID = RckPlayer.instance.aiMounting.entityID;

            saveFile.PlayerData.playerAttributes = EntityAttributes.PlayerAttributes.ToSaveData();
            saveFile.PlayerData.playerInventory = Inventory.PlayerInventory.ToSaveData();
            saveFile.PlayerData.spellsKnowledge = SpellsKnowledge.Player.ToSaveData();

            saveFile.TimeOfDayData.curTime = TimeOfDayManager.instance.GetCurrentTime();

            // Save all factions
            saveFile.PlayerData.playerFactions.Clear();
            for (int i = 0; i < RckPlayer.instance.belongsToFactions.Count; i++)
                saveFile.PlayerData.playerFactions.Add(RckPlayer.instance.belongsToFactions[i].ID);

            // Save all quests
            saveFile.QuestData = QuestManager.instance.ToSaveData();

            // Save all touched items and ai
            foreach (KeyValuePair<string, CellInformation> cellInfo in CellInformation.activeCells)
            {
                Debug.Log(cellInfo.Value.cell.ID);
                cellInfo.Value.SaveTouchedItems();

                // Save AI
                cellInfo.Value.SaveRckAI();
            }

            PersistentReferences.PersistentReferenceManager.instance.SaveAllPersistentAI();

            string jsonFile = JsonUtility.ToJson(saveFile, RCKSettings.JSON_PRETTY_PRINT);
            string path = _info.FullName;
            File.WriteAllText(path, jsonFile);

            TakeScreenShot(path);

            RckPlayer.instance.playerAttributes.RestoreAddsAfterSave(); // fix save bug
        }

        public void SaveOnNewFile(bool autosave = false)
        {
            System.DateTime date = System.DateTime.Now;
            // Fill the relevant save info

            // SaveFileData
            saveFile.SaveFileData.saveType = RCKSettings.SAVE_TYPE_VERSION;
            saveFile.SaveFileData.fileVersion = RCKSettings.FILE_SAVE_VERSION;
            saveFile.SaveFileData.saveNumber++;
            saveFile.SaveFileData.saveDate = date.ToShortDateString() + " - " + date.Hour + ":" + date.Minute;


            // PlayerData
            //saveFile.PlayerData.playerName = name;
            saveFile.PlayerData.playerLevel = EntityAttributes.PlayerAttributes.curLevel;
            //saveFile.PlayerData.playerSex = sex;
            saveFile.PlayerData.playerLocation = WorldManager.instance.currentCenterCell.cellName;
            saveFile.PlayerData.playerWorldspaceID = WorldManager.instance.currentWorldspace.worldSpaceID;
            saveFile.PlayerData.playerCellID = WorldManager.instance.currentCenterCell.ID;
            saveFile.PlayerData.playerPos = RckPlayer.instance.transform.position;
            saveFile.PlayerData.playerRot = RckPlayer.instance.transform.rotation;
            saveFile.PlayerData.mouseRotX = RckPlayer.instance.mouseLook.xRotation;
            saveFile.PlayerData.playerCrouched = RckPlayer.instance.IsCrouching;
            saveFile.PlayerData.weaponDrawn = PlayerCombat.instance.weaponDrawn;
            saveFile.PlayerData.isInThirdPerson = RckPlayer.instance.isInThirdPerson;

            saveFile.PlayerData.recoversSet = true;
            saveFile.PlayerData.recoverStaminaAmount = RckPlayer.instance.recoverStaminaAmount;
            saveFile.PlayerData.recoverAfterActionDelay = RckPlayer.instance.recoverAfterActionDelay;
            saveFile.PlayerData.recoverHealthAmount = RckPlayer.instance.recoverHealthAmount;
            saveFile.PlayerData.recoverAfterHitDelay = RckPlayer.instance.recoverAfterHitDelay;
            saveFile.PlayerData.recoverManaAfterUseDelay = RckPlayer.instance.recoverAfterManaUseDelay;

            saveFile.PlayerData.isMounted = RckPlayer.instance.isMounted;
            if (RckPlayer.instance.isMounted)
                saveFile.PlayerData.mountedEntityID = RckPlayer.instance.aiMounting.entityID;

            saveFile.PlayerData.playerAttributes = EntityAttributes.PlayerAttributes.ToSaveData();
            saveFile.PlayerData.playerInventory = Inventory.PlayerInventory.ToSaveData();
            saveFile.PlayerData.spellsKnowledge = SpellsKnowledge.Player.ToSaveData();

            saveFile.TimeOfDayData.curTime = TimeOfDayManager.instance.GetCurrentTime();

            // Save all factions
            saveFile.PlayerData.playerFactions.Clear();
            for (int i = 0; i < RckPlayer.instance.belongsToFactions.Count; i++)
                saveFile.PlayerData.playerFactions.Add(RckPlayer.instance.belongsToFactions[i].ID);


            // Save all quests
            saveFile.QuestData = QuestManager.instance.ToSaveData();

            // Save all touched items and ai
            foreach(KeyValuePair<string, CellInformation> cellInfo in CellInformation.activeCells)
            {
                cellInfo.Value.SaveTouchedItems();

                // Save AI
                cellInfo.Value.SaveRckAI();
            }

            PersistentReferences.PersistentReferenceManager.instance.SaveAllPersistentAI();

            if(!autosave)
            {
                string jsonFile = JsonUtility.ToJson(saveFile, RCKSettings.JSON_PRETTY_PRINT);
                string saveInfoFullName = saveInfo.FullName;

                string path = saveInfoFullName;

                // If we've loaded an autosave and we're saving on a new file, prevent the new file to be called "Autosave_"
                if (path.Contains("Autosave_"))
                    saveInfoFullName = Application.persistentDataPath + "/Saves/" + saveFile.PlayerData.playerName + "_";

                uint add = saveFile.SaveFileData.saveNumber;
                while (File.Exists(path))
                {
                    add++;
                    path = saveInfoFullName;
                    path = saveInfoFullName.Substring(0, saveInfoFullName.IndexOf('_'));
                    path += "_ " + add.ToString();
                    path += ".json";
                }

               

                File.WriteAllText(path, jsonFile);

                // Save screenshot
                TakeScreenShot(path);

                RckPlayer.instance.playerAttributes.RestoreAddsAfterSave(); // fix save bug
            }
            else
            {
                string jsonFile = JsonUtility.ToJson(saveFile, RCKSettings.JSON_PRETTY_PRINT);
                string path = Application.persistentDataPath + "/Saves/" + "Autosave_" + saveFile.PlayerData.playerName + ".json";

                File.WriteAllText(path, jsonFile);

                // Save screenshot
                TakeScreenShot(path);

                RckPlayer.instance.playerAttributes.RestoreAddsAfterSave(); // fix save bug
            }
        }

        public Savegame CreateNewCharacter(string _name, bool _gender, string _raceID, FaceBlendshapesSaveData _faceData, int _hairType, int _eyesType, int _selectedClass, EntityAttributes _attr)
        {
            Savegame savegame = SaveSystemManager.instance.gameObject.AddComponent<Savegame>();

            if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

            // Get all files

            SaveFile saveFile = new SaveFile();

            string path = Application.persistentDataPath + "/Saves/" + _name + "_";
            int add = 0;

            if (File.Exists(path))
            {
                do
                {
                    add++;
                    path = path.Substring(0, path.IndexOf('_'));
                    path += "_ " + add.ToString();
                    path += ".json";
                } while (File.Exists(path));
            }
            else
                path += "0.json";

            FileInfo saveInfo = new FileInfo(path);

            System.DateTime date = System.DateTime.Now;
            // Fill the relevant save info

            // SaveFileData
            saveFile.SaveFileData.saveType = RCKSettings.SAVE_TYPE_VERSION;
            saveFile.SaveFileData.fileVersion = RCKSettings.FILE_SAVE_VERSION;
            saveFile.SaveFileData.saveNumber++;
            saveFile.SaveFileData.saveDate = date.ToShortDateString() + " - " + date.Hour + ":" + date.Minute;


            // PlayerData
            saveFile.PlayerData.playerName = _name;
            saveFile.PlayerData.playerRace = _raceID;
            saveFile.PlayerData.playerLevel = RCKSettings.RCK_NEW_STARTING_LEVEL;
            saveFile.PlayerData.playerSex = _gender;
            saveFile.PlayerData.hairType = _hairType;
            saveFile.PlayerData.eyesType = _eyesType;
            saveFile.PlayerData.selectedClass = _selectedClass;
            saveFile.PlayerData.playerLocation = RCKSettings.RCK_NEW_STARTING_LOCATION;
            saveFile.PlayerData.playerWorldspaceID = RCKSettings.RCK_NEW_STARTING_WORLDSPACEID;
            saveFile.PlayerData.playerCellID = RCKSettings.RCK_NEW_STARTING_CELLID;
            saveFile.PlayerData.playerPos = RCKSettings.RCK_NEW_STARTING_POS;
            saveFile.PlayerData.playerRot = Quaternion.Euler(RCKSettings.RCK_NEW_STARTING_ROT);
            saveFile.PlayerData.playerAttributes = _attr.ToSaveData();
            

            /*
            saveFile.PlayerData.mouseRotX = RckPlayer.instance.mouseLook.xRotation;
            saveFile.PlayerData.playerCrouched = RckPlayer.instance.IsCrouching;
            saveFile.PlayerData.weaponDrawn = PlayerCombat.instance.weaponDrawn;

            saveFile.PlayerData.playerAttributes = EntityAttributes.PlayerAttributes.ToSaveData();
            saveFile.PlayerData.playerInventory = Inventory.PlayerInventory.ToSaveData();


            // Save all quests
            saveFile.QuestData = QuestManager.instance.ToSaveData();

            // Save all touched items and ai
            foreach (KeyValuePair<string, CellInformation> cellInfo in CellInformation.activeCells)
            {
                cellInfo.Value.SaveTouchedItems();

                // Save AI
                cellInfo.Value.SaveRckAI();
            }

            */

            saveFile.PlayerData.faceData = _faceData;

            string jsonFile = JsonUtility.ToJson(saveFile, RCKSettings.JSON_PRETTY_PRINT);

            File.WriteAllText(path, jsonFile);

            savegame.saveFile = saveFile;
            savegame.fileInfo = saveInfo;

            // If a default image is provided, write it on disk
            if (SaveSystemManager.instance.defaultSavegameImage)
            {
                byte[] bytes = SaveSystemManager.instance.defaultSavegameImage.EncodeToPNG();
                string screenPath = path.Substring(0, path.IndexOf('.'));
                screenPath += ".png";
                System.IO.File.WriteAllBytes(screenPath, bytes);
            }

            return savegame;
        }

        public void TakeScreenShot(string _savePath)
        {
            Camera mainCamera = null;

            mainCamera = (RckPlayer.instance.isInThirdPerson) ? RckPlayer.instance.tpsCamera : RckPlayer.instance.mainCamera;

            // Let FPS camera render also the first person
            if(!RckPlayer.instance.isInThirdPerson)
                mainCamera.cullingMask |= 1 << RCKLayers.FirstPerson;

            RenderTexture rt = new RenderTexture(470, 250, 24);
            mainCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(470, 250, TextureFormat.RGB24, false);
            mainCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 470, 250), 0, 0);
            mainCamera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string screenPath = _savePath.Substring(0, _savePath.IndexOf('.'));
            screenPath += ".png";
            System.IO.File.WriteAllBytes(screenPath, bytes);

            // Restore culling mask
            if (!RckPlayer.instance.isInThirdPerson)
                mainCamera.cullingMask &= ~(1 << RCKLayers.FirstPerson);
        }

        /*
         * EXTRA, CREATE SAVEGAMES FOR RCK DEMOS
        */
        public Savegame DEMO_CreateNewCharacterShootingRange(string _name, bool _gender, string _raceID, FaceBlendshapesSaveData _faceData, int _hairType, int _eyesType)
        {
            Savegame savegame = SaveSystemManager.instance.gameObject.AddComponent<Savegame>();

            if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

            // Get all files

            SaveFile saveFile = new SaveFile();

            string path = Application.persistentDataPath + "/Saves/" + _name + "_";
            int add = 0;

            if (File.Exists(path))
            {
                do
                {
                    add++;
                    path = path.Substring(0, path.IndexOf('_'));
                    path += "_ " + add.ToString();
                    path += ".json";
                } while (File.Exists(path));
            }
            else
                path += "0.json";

            FileInfo saveInfo = new FileInfo(path);

            System.DateTime date = System.DateTime.Now;
            // Fill the relevant save info

            // SaveFileData
            saveFile.SaveFileData.saveType = RCKSettings.SAVE_TYPE_VERSION;
            saveFile.SaveFileData.fileVersion = RCKSettings.FILE_SAVE_VERSION;
            saveFile.SaveFileData.saveNumber++;
            saveFile.SaveFileData.saveDate = date.ToShortDateString() + " - " + date.Hour + ":" + date.Minute;


            // PlayerData
            saveFile.PlayerData.playerName = _name;
            saveFile.PlayerData.playerRace = _raceID;
            saveFile.PlayerData.playerLevel = RCKSettings.RCK_NEW_STARTING_LEVEL;
            saveFile.PlayerData.playerSex = _gender;
            saveFile.PlayerData.hairType = _hairType;
            saveFile.PlayerData.eyesType = _eyesType;
            saveFile.PlayerData.playerLocation = "Shooting Range";
            saveFile.PlayerData.playerWorldspaceID = "FirearmsTestSpace";
            saveFile.PlayerData.playerCellID = "FirearmsTestSpaceCell(0,0)";
            saveFile.PlayerData.playerPos = new Vector3(0, 1.47f, -19.05f);
            saveFile.PlayerData.playerRot = Quaternion.Euler(0, 0, 0);

            /*
            saveFile.PlayerData.mouseRotX = RckPlayer.instance.mouseLook.xRotation;
            saveFile.PlayerData.playerCrouched = RckPlayer.instance.IsCrouching;
            saveFile.PlayerData.weaponDrawn = PlayerCombat.instance.weaponDrawn;

            saveFile.PlayerData.playerAttributes = EntityAttributes.PlayerAttributes.ToSaveData();
            saveFile.PlayerData.playerInventory = Inventory.PlayerInventory.ToSaveData();


            // Save all quests
            saveFile.QuestData = QuestManager.instance.ToSaveData();

            // Save all touched items and ai
            foreach (KeyValuePair<string, CellInformation> cellInfo in CellInformation.activeCells)
            {
                cellInfo.Value.SaveTouchedItems();

                // Save AI
                cellInfo.Value.SaveRckAI();
            }

            */

            saveFile.PlayerData.faceData = _faceData;

            string jsonFile = JsonUtility.ToJson(saveFile, RCKSettings.JSON_PRETTY_PRINT);

            File.WriteAllText(path, jsonFile);

            savegame.saveFile = saveFile;
            savegame.fileInfo = saveInfo;

            // If a default image is provided, write it on disk
            if (SaveSystemManager.instance.demo2SavegameImage)
            {
                byte[] bytes = SaveSystemManager.instance.demo2SavegameImage.EncodeToPNG();
                string screenPath = path.Substring(0, path.IndexOf('.'));
                screenPath += ".png";
                System.IO.File.WriteAllBytes(screenPath, bytes);
            }

            return savegame;
        }

        public Savegame DEMO_CreateNewCharacterZombies(string _name, bool _gender, string _raceID, FaceBlendshapesSaveData _faceData, int _hairType, int _eyesType)
        {
            Savegame savegame = SaveSystemManager.instance.gameObject.AddComponent<Savegame>();

            if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

            // Get all files

            SaveFile saveFile = new SaveFile();

            string path = Application.persistentDataPath + "/Saves/" + _name + "_";
            int add = 0;

            if (File.Exists(path))
            {
                do
                {
                    add++;
                    path = path.Substring(0, path.IndexOf('_'));
                    path += "_ " + add.ToString();
                    path += ".json";
                } while (File.Exists(path));
            }
            else
                path += "0.json";

            FileInfo saveInfo = new FileInfo(path);

            System.DateTime date = System.DateTime.Now;
            // Fill the relevant save info

            // SaveFileData
            saveFile.SaveFileData.saveType = RCKSettings.SAVE_TYPE_VERSION;
            saveFile.SaveFileData.fileVersion = RCKSettings.FILE_SAVE_VERSION;
            saveFile.SaveFileData.saveNumber++;
            saveFile.SaveFileData.saveDate = date.ToShortDateString() + " - " + date.Hour + ":" + date.Minute;


            // PlayerData
            saveFile.PlayerData.playerName = _name;
            saveFile.PlayerData.playerRace = _raceID;
            saveFile.PlayerData.playerLevel = RCKSettings.RCK_NEW_STARTING_LEVEL;
            saveFile.PlayerData.playerSex = _gender;
            saveFile.PlayerData.hairType = _hairType;
            saveFile.PlayerData.eyesType = _eyesType;
            saveFile.PlayerData.playerLocation = "Zombies Map";
            saveFile.PlayerData.playerWorldspaceID = "ZombiesWorldspace";
            saveFile.PlayerData.playerCellID = "ZombiesMap001";
            saveFile.PlayerData.playerPos = new Vector3(0,1.47f,0);
            saveFile.PlayerData.playerRot = Quaternion.Euler(0, -90, 0);

            /*
            saveFile.PlayerData.mouseRotX = RckPlayer.instance.mouseLook.xRotation;
            saveFile.PlayerData.playerCrouched = RckPlayer.instance.IsCrouching;
            saveFile.PlayerData.weaponDrawn = PlayerCombat.instance.weaponDrawn;

            saveFile.PlayerData.playerAttributes = EntityAttributes.PlayerAttributes.ToSaveData();
            saveFile.PlayerData.playerInventory = Inventory.PlayerInventory.ToSaveData();


            // Save all quests
            saveFile.QuestData = QuestManager.instance.ToSaveData();

            // Save all touched items and ai
            foreach (KeyValuePair<string, CellInformation> cellInfo in CellInformation.activeCells)
            {
                cellInfo.Value.SaveTouchedItems();

                // Save AI
                cellInfo.Value.SaveRckAI();
            }

            */

            saveFile.PlayerData.faceData = _faceData;

            string jsonFile = JsonUtility.ToJson(saveFile, RCKSettings.JSON_PRETTY_PRINT);

            File.WriteAllText(path, jsonFile);

            savegame.saveFile = saveFile;
            savegame.fileInfo = saveInfo;

            // If a default image is provided, write it on disk
            if (SaveSystemManager.instance.demo3SavegameImage)
            {
                byte[] bytes = SaveSystemManager.instance.demo3SavegameImage.EncodeToPNG();
                string screenPath = path.Substring(0, path.IndexOf('.'));
                screenPath += ".png";
                System.IO.File.WriteAllBytes(screenPath, bytes);
            }

            return savegame;
        }
    }
}
