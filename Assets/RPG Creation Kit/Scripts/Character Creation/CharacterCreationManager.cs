using RPGCreationKit;
using RPGCreationKit.PersistentReferences;
using RPGCreationKit.Player;
using RPGCreationKit.SaveSystem;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterCreationManager : MonoBehaviour
{
    [Header("SETTINGS")]
    public Race[] playableRaces;
    public Race defaultRace;

    public Race selectedRace;

    public RuntimeAnimatorController animControllerMale;
    public RuntimeAnimatorController animControllerFemale;


    [SerializeField] private PlayerInput input;
    [SerializeField] private Animator camAnimator;

    [SerializeField] private bool isCreatingMale = true;
    
    public BodyData currentCharacter;

    [SerializeField] private GameObject LeftPanelMenu;
    [SerializeField] private GameObject LeftPanelFaceMenu;
    [SerializeField] private TMP_InputField characterNameField;

    [SerializeField] private GameObject messagePanel;
    [SerializeField] private GameObject messageBoxErrorName;
    [SerializeField] private GameObject messageBoxConfirmation;

    [SerializeField] private Transform characterT;

    [SerializeField] private Toggle maleToggle;
    [SerializeField] private Button editFaceButton;

    [SerializeField] private GameObject headBlendshapePrefab;
    [SerializeField] private Transform headBlendshapeContent;

    [SerializeField] private TextMeshProUGUI raceNameText;
    [SerializeField] private TextMeshProUGUI hairTypeText;
    [SerializeField] private TextMeshProUGUI eyesTypeText;
    [SerializeField] private int selectedHair = -1;
    [SerializeField] private int selectedEyes = 0;
    [SerializeField] private int selectedRaceIndex = 0;

    [SerializeField] private GameObject ModeSelectionUI;


    // Class selection
    Inventory inventory;
    Equipment equipment;

    private WeaponItem currentWeapon;
    private GameObject currentWeaponObject;

    private AmmoItem currentAmmo;
    private GameObject currentAmmoObject;
    public Animator m_Animator;

    [SerializeField] private GameObject defaultPanelUI;
    [SerializeField] private GameObject classSelectionUI;
    [SerializeField] private GameObject statsPanelUI;

    [SerializeField] private TextMeshProUGUI classNameText;
    public EntityAttributes creationAttributes;

    [SerializeField] private TextMeshProUGUI strVal;
    [SerializeField] private TextMeshProUGUI agiVal;
    [SerializeField] private TextMeshProUGUI conVal;
    [SerializeField] private TextMeshProUGUI speVal;
    [SerializeField] private TextMeshProUGUI endVal;
    [SerializeField] private TextMeshProUGUI dexVal;
    [SerializeField] private TextMeshProUGUI chaVal;
    [SerializeField] private TextMeshProUGUI intVal;
    [SerializeField] private TextMeshProUGUI willVal;

    [SerializeField] private TextMeshProUGUI healthVal;
    [SerializeField] private TextMeshProUGUI manaVal;
    [SerializeField] private TextMeshProUGUI staminaVal;

    [SerializeField] private List<Item> _warItems;
    [SerializeField] private List<Item> _ranItems;
    [SerializeField] private List<Item> _magItems;

    public bool startWithDemoItem = false;
    public Item demoItem;
    public enum StartClassess
    {
        Warrior = 0,
        Ranger = 1,
        Mage = 2
    };

    public StartClassess curClass = StartClassess.Warrior;

    // Start is called before the first frame update
    void Start()
    {
        RckInput.LoadInputSave();
        RckInput.SetPlayerInputInstance(input);

        selectedRace = defaultRace;

        // Load default race
        LoadRaceCharacter(selectedRace);
    }


    public void LoadRaceCharacter(Race race)
    {
        selectedHair = -1;
        selectedEyes = 0;

        // Destroy all in character T
        foreach (Transform T in characterT)
            Destroy(T.gameObject);

        // Spawn the character
        GameObject toSpawn = (isCreatingMale) ? race.maleModel.gameObject : race.femaleModel.gameObject;

        GameObject character = Instantiate(toSpawn, characterT);
        currentCharacter = character.GetComponent<BodyData>();

        m_Animator = character.GetComponent<Animator>();

        if(m_Animator != null)
        {
            var animController = (isCreatingMale) ? animControllerMale : animControllerFemale;
            m_Animator.runtimeAnimatorController = animController;
            m_Animator.enabled = true;
        }

        raceNameText.text = race.raceName;
    }

    public void OnMaleToggleChanges()
    {
        isCreatingMale = maleToggle.isOn;
        LoadRaceCharacter(selectedRace);
    }


    public void EditFaceButton()
    {
        LeftPanelMenu.SetActive(false);
        LeftPanelFaceMenu.SetActive(true);

        Cam_BodyToFace();

        SkinnedMeshRenderer face = currentCharacter.head;

        faceBlendshapes.Clear();

        // Display all blendshapes
        for (int i = 0; i < face.sharedMesh.blendShapeCount; i++)
        {
            string blendshapeName = face.sharedMesh.GetBlendShapeName(i).ToString();

            // Skip facial animation blendshapes
            if (blendshapeName.Contains("FANIM_"))
                continue;

            GameObject newBlendshape = Instantiate(headBlendshapePrefab, headBlendshapeContent);
            BlendshapeInCreationList newBlendshapeScript = newBlendshape.GetComponent<BlendshapeInCreationList>();
            newBlendshapeScript.usesHeadBlendshapes = true;

            faceBlendshapes.Add(newBlendshapeScript.slider);

            newBlendshapeScript.AssignBlendshape(face, i, blendshapeName);
        }

        if(faceBlendshapes.Count > 0 && RckInput.isUsingGamepad)
            faceBlendshapes[0].Select();

    }

    public void BackFromFaceButton()
    {
        foreach (Transform T in headBlendshapeContent)
            Destroy(T.gameObject);

        LeftPanelMenu.SetActive(true);
        LeftPanelFaceMenu.SetActive(false);

        Cam_FaceToBody();

        if (RckInput.isUsingGamepad)
            editFaceButton.Select();
    }

    [ContextMenu("Face Button")]
    public void Cam_BodyToFace()
    {
        camAnimator.SetBool("isMale", isCreatingMale);
        camAnimator.SetTrigger("bodyToFace");
    }

    [ContextMenu("Back Button")]
    public void Cam_FaceToBody()
    {
        camAnimator.SetBool("isMale", isCreatingMale);
        camAnimator.SetTrigger("faceToBody");
    }

    public FaceBlendshapesSaveData GenerateFaceBlendshapeSaveData()
    {
        FaceBlendshapesSaveData data = new RPGCreationKit.SaveSystem.FaceBlendshapesSaveData();

        for (int i = 0; i < currentCharacter.head.sharedMesh.blendShapeCount; i++) 
            data.allShapes.Add(new FaceBlendshapeSaveData(i, currentCharacter.head.GetBlendShapeWeight(i)));

        return data;
    }

    public void ContinueButton()
    {
        messagePanel.SetActive(true);

        if (characterNameField.text.Length <= 0)
            messageBoxErrorName.SetActive(true);
        else
            messageBoxConfirmation.SetActive(true);
    }

    /*
    public void ConfirmCreation()
    {
        // Create savegame
        Savegame newSave = SaveSystemManager.instance.CreateNewCharacter(characterNameField.text, !isCreatingMale, selectedRace.ID, GenerateFaceBlendshapeSaveData(), selectedHair, selectedEyes);
        messageBoxConfirmation.SetActive(false);
        messagePanel.SetActive(false);

        // Load new character
        SaveSystemManager.instance.LoadSaveGame(newSave.fileInfo, newSave.saveFile);
    }
    */

    public void ConfirmCreation()
    {
        if(RCKSettings.ENABLE_DEMO_SELECTION)
        {
            ModeSelectionUI.gameObject.SetActive(true);
        }
        else
        {
            if (!RCKSettings.ENABLE_CLASS_SELECTION)
                StartDemoOne();
            else
                GoToClassSelection();
        }
    }

    public void GoToClassSelection()
    {
        if (RCKSettings.ENABLE_CLASS_SELECTION)
        {
            defaultPanelUI.SetActive(false);
            classSelectionUI.SetActive(true);
            statsPanelUI.SetActive(true);

            messagePanel.SetActive(false);
            ModeSelectionUI.gameObject.SetActive(false);

            if (RCKSettings.ENABLE_CLASS_SELECTION)
                SelectWarriorClass();
        }
        else
        {
            StartDemoOne();
        }
    }

    public void StartDemoOne()
    {
        // Create savegame
        Savegame newSave = SaveSystemManager.instance.CreateNewCharacter(characterNameField.text, !isCreatingMale, selectedRace.ID, GenerateFaceBlendshapeSaveData(), selectedHair, selectedEyes, (int)curClass, creationAttributes);
        messageBoxConfirmation.SetActive(false);
        messagePanel.SetActive(false);
        // Load new character
        SaveSystemManager.instance.LoadSaveGame(newSave.fileInfo, newSave.saveFile);

        
    }

    public void StartDemoTwo()
    {
        // Create savegame
        Savegame newSave = SaveSystemManager.instance.DEMO_CreateNewCharacterShootingRange(characterNameField.text, !isCreatingMale, selectedRace.ID, GenerateFaceBlendshapeSaveData(), selectedHair, selectedEyes);
        messageBoxConfirmation.SetActive(false);
        messagePanel.SetActive(false);

        // Load new character
        SaveSystemManager.instance.LoadSaveGame(newSave.fileInfo, newSave.saveFile);
    }

    public void StartDemoThree()
    {
        // Create savegame
        Savegame newSave = SaveSystemManager.instance.DEMO_CreateNewCharacterZombies(characterNameField.text, !isCreatingMale, selectedRace.ID, GenerateFaceBlendshapeSaveData(), selectedHair, selectedEyes);
        messageBoxConfirmation.SetActive(false);
        messagePanel.SetActive(false);

        // Load new character
        SaveSystemManager.instance.LoadSaveGame(newSave.fileInfo, newSave.saveFile);
    }

    public void HairRightButton()
    {
        List<Hair> hair = (isCreatingMale) ? selectedRace.maleHairTypes : selectedRace.femaleHairTypes;

        if (selectedHair + 1 < hair.Count)
            selectedHair++;
        else if (selectedHair == hair.Count - 1)
            selectedHair = -1;
        else if (selectedHair == -1)
            selectedHair = 0;

        OnHairChanges();
    }

    public void HairLeftButton()
    {
        List<Hair> hair = (isCreatingMale) ? selectedRace.maleHairTypes : selectedRace.femaleHairTypes;

        if (selectedHair - 1 >= 0)
            selectedHair--;
        else if (selectedHair == 0)
            selectedHair = -1;
        else if (selectedHair == -1)
            selectedHair = hair.Count - 1;

        OnHairChanges();
    }

    public void RaceRightButton()
    {
        List<Hair> hair = (isCreatingMale) ? selectedRace.maleHairTypes : selectedRace.femaleHairTypes;

        if (selectedRaceIndex + 1 < playableRaces.Length)
            selectedRaceIndex++;
        else if (selectedRaceIndex == hair.Count - 1)
            selectedRaceIndex = -1;
        else if (selectedRaceIndex == -1)
            selectedRaceIndex = 0;

        selectedRace = playableRaces[selectedRaceIndex];
        LoadRaceCharacter(selectedRace);
    }

    public void RaceLeftButton()
    {
        if (selectedRaceIndex - 1 >= 0)
            selectedRaceIndex--;
        else if (selectedRaceIndex == 0)
            selectedRaceIndex = -1;
        else if (selectedRaceIndex == -1)
            selectedRaceIndex = playableRaces.Length - 1;

        selectedRace = playableRaces[selectedRaceIndex];
        LoadRaceCharacter(selectedRace);
    }

    public void EyesRightButton()
    {
        List<Eye> eyes = selectedRace.eyeTypes;

        if (selectedEyes + 1 < eyes.Count)
            selectedEyes++;
        else if (selectedEyes == eyes.Count - 1)
            selectedEyes = 0;

        OnEyesChanges();
    }

    public void EyesLeftButton()
    {
        List<Eye> eyes = selectedRace.eyeTypes;

        if (selectedEyes - 1 >= 0)
            selectedEyes--;
        else if (selectedEyes == 0)
            selectedEyes = eyes.Count - 1;

        OnEyesChanges();
    }

    public void OnEyesChanges()
    {
        Eye eyesType = selectedRace.eyeTypes[selectedEyes];

        currentCharacter.eyes.sharedMaterial = eyesType.eyes.sharedMaterial;
        eyesTypeText.text = eyesType.eyesName;
    }

    public void OnHairChanges()
    {
        if (currentCharacter.hair != null)
            Destroy(currentCharacter.hair.gameObject);

        if (selectedHair != -1)
        {
            // Spawn new hair
            Hair hair = (isCreatingMale) ? selectedRace.maleHairTypes[selectedHair] : selectedRace.femaleHairTypes[selectedHair];

            currentCharacter.hair = Instantiate(hair.mesh.gameObject, currentCharacter.transform).GetComponent<SkinnedMeshRenderer>();

            // Attach
            currentCharacter.hair.transform.parent = currentCharacter.head.transform;
            currentCharacter.hair.rootBone = currentCharacter.head.rootBone;
            currentCharacter.hair.bones = currentCharacter.head.bones;

            hairTypeText.text = hair.hairName;
        }
        else
            hairTypeText.text = "NONE";

    }

    public List<Slider> faceBlendshapes;
    public void RandomizeFace()
    {
        for (int i = 0; i < faceBlendshapes.Count; i++)
            faceBlendshapes[i].value = Random.Range(-100, 100);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("_MainMenu_");
    }

    public void SelectWarriorClass()
    {
        curClass = StartClassess.Warrior;
        classNameText.text = "Warrior";

        creationAttributes.attributes.Strength = 25;
        creationAttributes.attributes.Dexterity = 10;
        creationAttributes.attributes.Agility = 15;
        creationAttributes.attributes.Constitution = 35;
        creationAttributes.attributes.Speed = 10;
        creationAttributes.attributes.Endurance = 25;
        creationAttributes.attributes.Charisma = 10;
        creationAttributes.attributes.Intelligence = 10;
        creationAttributes.attributes.Willpower = 10;

        creationAttributes.derivedAttributes.CalculateFromAttributes(creationAttributes.attributes);
        creationAttributes.derivedAttributes.SetCurValuesToMax();

        UpdateDisplayedClassValues();

        EquipCharacterWithClass();
    }

    public void SelectRangerClass()
    {
        curClass = StartClassess.Ranger;
        classNameText.text = "Ranger";

        creationAttributes.attributes.Strength = 15;
        creationAttributes.attributes.Dexterity = 25;
        creationAttributes.attributes.Agility = 20;
        creationAttributes.attributes.Constitution = 20;
        creationAttributes.attributes.Speed = 25;
        creationAttributes.attributes.Endurance = 20;
        creationAttributes.attributes.Charisma = 10;
        creationAttributes.attributes.Intelligence = 5;
        creationAttributes.attributes.Willpower = 10;

        creationAttributes.derivedAttributes.CalculateFromAttributes(creationAttributes.attributes);
        creationAttributes.derivedAttributes.SetCurValuesToMax();

        UpdateDisplayedClassValues();

        EquipCharacterWithClass();
    }

    public void SelectMageClass()
    {
        curClass = StartClassess.Mage;
        classNameText.text = "Mage";

        creationAttributes.attributes.Strength = 10;
        creationAttributes.attributes.Dexterity = 10;
        creationAttributes.attributes.Agility = 15;
        creationAttributes.attributes.Constitution = 15;
        creationAttributes.attributes.Speed = 15;
        creationAttributes.attributes.Endurance = 20;
        creationAttributes.attributes.Charisma = 10;
        creationAttributes.attributes.Intelligence = 25;
        creationAttributes.attributes.Willpower = 30;

        creationAttributes.derivedAttributes.CalculateFromAttributes(creationAttributes.attributes);
        creationAttributes.derivedAttributes.SetCurValuesToMax();

        UpdateDisplayedClassValues();

        EquipCharacterWithClass();
    }

    public void UpdateDisplayedClassValues()
    {
        strVal.text = creationAttributes.attributes.Strength.ToString();
        agiVal.text = creationAttributes.attributes.Agility.ToString();
        conVal.text = creationAttributes.attributes.Constitution.ToString();
        speVal.text = creationAttributes.attributes.Speed.ToString();
        endVal.text = creationAttributes.attributes.Endurance.ToString();
        dexVal.text = creationAttributes.attributes.Dexterity.ToString();
        chaVal.text = creationAttributes.attributes.Charisma.ToString();
        intVal.text = creationAttributes.attributes.Intelligence.ToString();
        willVal.text = creationAttributes.attributes.Willpower.ToString();

        healthVal.text = (int)creationAttributes.derivedAttributes.curHealth + "/" + (int)creationAttributes.derivedAttributes.maxHealth;
        manaVal.text = (int)creationAttributes.derivedAttributes.curMana + "/" + (int)creationAttributes.derivedAttributes.maxMana;
        staminaVal.text = (int)creationAttributes.derivedAttributes.curStamina + "/" + (int)creationAttributes.derivedAttributes.maxStamina;
    }

    public void EquipCharacterWithClass()
    {
        if(inventory == null || equipment == null)
        {
            inventory = currentCharacter.AddComponent<Inventory>();
            equipment = currentCharacter.AddComponent<Equipment>();
        }

        for(int i = 0; i < inventory.Items.Count; i++)
        {
            if (inventory.Items[i] != null)
                equipment.Unequip(inventory.Items[i]);
        }

        inventory.ClearInventory();
        equipment.currentWeapon = null;

        switch (curClass)
        {
            case StartClassess.Warrior:
                equipment.characterModel = currentCharacter;
                equipment.inv = inventory;

                for(int i = 0; i < _warItems.Count; i++)
                {
                    inventory.AddItem(_warItems[i], new ItemMetadata(), 1);
                    equipment.Equip(_warItems[i]);

                    if (_warItems[i].itemType == ItemTypes.WeaponItem)
                        equipment.currentWeapon = (WeaponItem)_warItems[i];

                    equipment.OnEquipmentChanges();
                    OnEquipmentChangesHands();
                    OnEquipmentChangesAmmo();
                }
                break;

            case StartClassess.Ranger:
                equipment.characterModel = currentCharacter;
                equipment.inv = inventory;

                for (int i = 0; i < _ranItems.Count; i++)
                {
                    inventory.AddItem(_ranItems[i], new ItemMetadata(), 1);
                    equipment.Equip(_ranItems[i]);

                    if (_ranItems[i].itemType == ItemTypes.WeaponItem)
                        equipment.currentWeapon = (WeaponItem)_ranItems[i];

                    equipment.OnEquipmentChanges();
                    OnEquipmentChangesHands();
                    OnEquipmentChangesAmmo();
                }
                break;

            case StartClassess.Mage:
                equipment.characterModel = currentCharacter;
                equipment.inv = inventory;

                for (int i = 0; i < _magItems.Count; i++)
                {
                    inventory.AddItem(_magItems[i], new ItemMetadata(), 1);
                    equipment.Equip(_magItems[i]);

                    if (_magItems[i].itemType == ItemTypes.WeaponItem)
                        equipment.currentWeapon = (WeaponItem)_magItems[i];

                    equipment.OnEquipmentChanges();
                    OnEquipmentChangesHands();
                    OnEquipmentChangesAmmo();
                }
                break;
        }

        
    }

    public void OnEquipmentChangesHands()
    {
        // If there is a weapon equipped
        if (equipment.itemsEquipped[(int)EquipmentSlots.RHand] != null && equipment.itemsEquipped[(int)EquipmentSlots.RHand].item != null)
        {
            // Destroy previous weapon equipped if there was a swap
            if (currentWeapon != null)
            {
                Destroy(currentWeaponObject);
                currentWeapon = null;
                currentWeapon = equipment.currentWeapon;
            }

            // Instantiate Weapon on hand, assign references
            currentWeapon = (WeaponItem)equipment.itemsEquipped[(int)EquipmentSlots.RHand].item;
            currentWeaponObject = Instantiate(currentWeapon.WeaponOnHand, currentCharacter.rHand);

        }
        else if (currentWeapon != null)
        {
            // Unequip = destroy previous equipped weapon
            Destroy(currentWeaponObject);
            currentWeapon = null;
            currentWeapon = equipment.currentWeapon;
        }


        UpdateAnimator();
    }

    public void OnEquipmentChangesAmmo()
    {
        if (equipment.itemsEquipped[(int)EquipmentSlots.Ammo] != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != null)
        {
            if (currentAmmoObject != null && equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item != currentAmmoObject)
            {
                Destroy(currentAmmoObject);
                currentAmmo = null;
            }

            // Instantiate Weapon on hand, assign references
            currentAmmo = (AmmoItem)equipment.itemsEquipped[(int)EquipmentSlots.Ammo].item;
            currentAmmoObject = Instantiate(currentAmmo.bagOnBody, currentCharacter.upperChest);
        }
        else if (currentAmmo != null)
        {
            // Unequip = destroy previous equipped weapon
            Destroy(currentAmmoObject);
            currentAmmo = null;
        }

        UpdateAnimator();
    }

    public void UpdateAnimator()
    {
        // Decide what animator to use
        if (currentWeapon != null && m_Animator != null)
        {
            if (currentWeapon.weaponType == WeaponType.BladeOneHand || currentWeapon.weaponType == WeaponType.DaggerOneHand || currentWeapon.weaponType == WeaponType.BluntOneHand)
                m_Animator.SetInteger("WeaponEquipped", 1);
            if (currentWeapon.weaponType == WeaponType.BladeTwoHands || currentWeapon.weaponType == WeaponType.BluntTwoHands)
                m_Animator.SetInteger("WeaponEquipped", 2);
            if (currentWeapon.weaponType == WeaponType.Bow)
                m_Animator.SetInteger("WeaponEquipped", 3);
            if (currentWeapon.weaponType == WeaponType.FIREARM_OneHanded_1)
                m_Animator.SetInteger("WeaponEquipped", 5);
            if (currentWeapon.weaponType == WeaponType.FIREARM_TwoHanded_1)
                m_Animator.SetInteger("WeaponEquipped", 6);

        }
        else
        {
            m_Animator.SetInteger("WeaponEquipped", 0);
        }

        m_Animator.SetBool("isUsingTorch", equipment.isUsingTorch);

    }


    public void SetDemoItem()
    {
        startWithDemoItem = true;
        FindAnyObjectByType<DemoItemAdder>().addDemoItem = true;
        GoToClassSelection();
    }
}
