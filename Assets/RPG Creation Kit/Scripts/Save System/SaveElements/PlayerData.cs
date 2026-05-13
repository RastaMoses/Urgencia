using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RPGCreationKit.SaveSystem
{
    [Serializable]
    public class PlayerData
    {
        public string playerName;
        public uint playerLevel;
        public bool playerSex;
        public string playerRace;
        public string playerLocation;
        public string playerWorldspaceID;
        public string playerCellID;
        public Vector3 playerPos;
        public Quaternion playerRot;
        public List<string> playerFactions = new List<string>();
        public float mouseRotX;
        public bool playerCrouched;
        public bool weaponDrawn;
        public EntityAttributesSaveData playerAttributes = new EntityAttributesSaveData(new BaseAttributes(), new EntityDerivedAttributes(), new List<EffectOnEntity>(), 1, 0, RCKSettings.GetNextLevelXPValue(1), 0);
        public InventorySaveData playerInventory = new InventorySaveData();
        public SpellsKnowledgeSaveData spellsKnowledge = new SpellsKnowledgeSaveData();
        public int hairType;
        public int eyesType;
        public FaceBlendshapesSaveData faceData = new FaceBlendshapesSaveData();
        public bool isInThirdPerson;

        // From 1.7
        public bool recoversSet = false; // it is true if the attributes below were initialized in the savegame. It is used to allow the default amounts in the RckPlayer to not be overwritten by a new save
        public float recoverStaminaAmount;
        public float recoverAfterActionDelay;
        public float recoverHealthAmount;
        public float recoverManaAmount;
        public float recoverManaAfterUseDelay;
        public float recoverAfterHitDelay;

        // From 1.8
        public bool isMounted;
        public string mountedEntityID;

        // From 2.0
        public int selectedClass;

        public PlayerData()
        {
            playerAttributes.attributes.Strength = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_STR;
            playerAttributes.attributes.Dexterity = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_DEX;
            playerAttributes.attributes.Agility = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_AGI;
            playerAttributes.attributes.Constitution = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_CON;
            playerAttributes.attributes.Speed = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_SPD;
            playerAttributes.attributes.Endurance = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_END;
            playerAttributes.attributes.Charisma = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_CHA;
            playerAttributes.attributes.Intelligence = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_INT;
            playerAttributes.attributes.Willpower = RCKSettings.PLAYER_ATTRIBUTES_DEFAULT_WIL;

            playerAttributes.derivedAttributes.CalculateFromAttributes(playerAttributes.attributes);
            playerAttributes.derivedAttributes.SetCurValuesToMax();
        }
    }
}