using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    /// <summary>
    /// Represents an Entity in the game. Used mainly to differentiate one entity with another.
    /// </summary>
    [DisallowMultipleComponent]
    public class Entity : MonoBehaviour, ITargetable
    {
        public string entityID = ""; // MUST BE THE SAME OF PREF IF USING PERSISTENT REFERENCE
        public string entityName = "";

        Guid entityGuid;
        [SerializeField] private byte[] serializedGuid;

        public CellsSystem.CellInformation myCellInfo;

        /// The part of the body that's more meaningful (usually the head). Used by other targets to look at this entity
        public Transform entityFocusPart;

        public bool isLoaded = false; /// If the Entity has been loaded from the SaveFile

        private void Reset()
        {
            RegenerateEntityGUID();
        }

        [ContextMenu("RegenerateEntityGUID")]
        public void RegenerateEntityGUID()
        {
            entityGuid = Guid.NewGuid();
            serializedGuid = entityGuid.ToByteArray();
        }

        public byte[] GetGuid()
        {
            return serializedGuid;
        }

        public static Entity GetPlayerEntity()
        {
            return RckPlayer.instance;
        }

        string ITargetable.GetID()
        {
            return entityID;
        }

        ITargetableType ITargetable.GetTargetableType()
        {
            return ITargetableType.Entity;
        }

        string ITargetable.GetExtraData()
        {
            return string.Empty;
        }
    }

    public class EntityFactionable : Entity
    {
        public List<Faction> belongsToFactions = new List<Faction>();

        /// <summary>
        ///  Returns true if the this NPC is a member of the specified faction. 
        /// </summary>
        public bool GetInFaction(string _factionID)
        {
            return belongsToFactions.Contains(FactionsDatabase.GetFaction(_factionID));
        }

        public void AddToFaction(string _factionID)
        {
            Faction faction = FactionsDatabase.GetFaction(_factionID);

            if (faction != null)
            {
                bool found = false;
                for (int i = 0; i < belongsToFactions.Count; i++)
                {
                    if (belongsToFactions[i].ID == faction.ID)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    belongsToFactions.Add(faction);
            }
            else
                Debug.LogWarning("Faction with ID: " + _factionID + " not found. Maybe you have to update the database?");

        }

        public void RemoveFromFaction(string _factionID)
        {
            Debug.Log("REMVOED FROM FACTION!!!");
            Faction faction = FactionsDatabase.GetFaction(_factionID);

            if (faction != null)
            {
                for (int i = 0; i < belongsToFactions.Count; i++)
                {
                    if (belongsToFactions[i].ID == faction.ID)
                    {
                        belongsToFactions.RemoveAt(i);
                        break;
                    }
                }
            }
            else
                Debug.LogWarning("Faction with ID: " + _factionID + " not found. Maybe you have to update the database?");

        }

        /// <summary>
        /// Returns the number of factions this entity belongs to.
        /// </summary>
        /// <returns></returns>
        public int GetNumFactions()
        {
            return belongsToFactions.Count;
        }

        /// <summary>
        /// Modifies the disposition towards a faction for this entity
        /// </summary>
        /// <param name="_factionID"></param>
        public void SetFactionDisposition(string _factionID, int _disposition)
        {

        }

        /// <summary>
        /// Returns 1 if the calling entity is in faction that the player also belongs to. 
        /// </summary>
        public void HasAFactionInCommonWithPC()
        {

        }

        public bool IsHostileTowards(EntityFactionable _otherEntity)
        {
            return false;
        }

    }
}