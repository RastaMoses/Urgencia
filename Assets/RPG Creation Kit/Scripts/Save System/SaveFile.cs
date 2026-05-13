using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.SaveSystem;

[System.Serializable]
public class SaveFile
{
    [SerializeField] public SaveFileData SaveFileData = new SaveFileData();
    [SerializeField] public PlayerData PlayerData = new PlayerData();
    [SerializeField] public QuestsData QuestData = new QuestsData();
    [SerializeField] public LootingPointsData LootingPointsData = new LootingPointsData();
    [SerializeField] public ItemsInWorldData ItemsInWorldData = new ItemsInWorldData();
    [SerializeField] public CreatedItemInWorldData CreatedItemsInWorldData = new CreatedItemInWorldData();
    [SerializeField] public DoorsData DoorData = new DoorsData();
    [SerializeField] public AIData AIData = new AIData();
    [SerializeField] public MutablesData MutablesData = new MutablesData();
    [SerializeField] public TimeOfDayData TimeOfDayData = new TimeOfDayData();
}
