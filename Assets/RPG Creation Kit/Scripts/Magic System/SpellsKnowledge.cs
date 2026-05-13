using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;

namespace RPGCreationKit
{
    /// <summary>
    /// Represents the knowledge of an entity of spells
    /// </summary>
    public class SpellsKnowledge : MonoBehaviour
    {
        #region PlayerSingleton
        public static SpellsKnowledge Player;
        private void Awake()
        {
            // Find the player inventory and set reference
            if (!Player)
                if (gameObject.CompareTag("Player"))
                    Player = this;
        }
        #endregion

        public Spell spellInUse = null;

        [SerializeField]
        public List<Spell> Spells = new List<Spell>();



        public Spell LearnSpell(string _spellID)
        {
            Spell spell = SpellsDatabase.GetSpell(_spellID);

            if(spell == null)
            {
                Debug.LogWarning("Tried to learn the spell with ID: '" + _spellID + "' but it is not present in the SpellsDatabase, you probably have to update it.");
                return null;
            }

            if(!Spells.Contains(spell))
            {
                Spells.Add(spell);
                return spell;
            }
            else
                return null;
        }

        public Spell LearnSpell(Spell spellToLearn)
        {
            if (spellToLearn != null && !Spells.Contains(spellToLearn))
            {
                Spells.Add(spellToLearn);
                return spellToLearn;
            }
            else
                return null;
        }

        public bool UnlearnSpell(string _spellID)
        {
            Spell spell = SpellsDatabase.GetSpell(_spellID);

            if (spell == null)
            {
                Debug.LogWarning("Tried to unlearn the spell with ID: '" + _spellID + "' but it is not present in the SpellsDatabase, you probably have to update it.");
                return false;
            }

            if (Spells.Contains(spell))
            {
                Spells.Remove(spell);
                return true;
            }
            else
                return false;
        }

        public bool HasSpell(string _spellID)
        {
            Spell spell = SpellsDatabase.GetSpell(_spellID);

            if (spell == null)
            {
                Debug.LogWarning("Tried to unlearn the spell with ID: '" + _spellID + "' but it is not present in the SpellsDatabase, you probably have to update it.");
                return false;
            }

            if (Spells.Contains(spell))
                return true;
            else
                return false;
        }

        public void ClearSpells()
        {
            Spells.Clear();
        }

        public bool EquipSpell(Spell spellToEquip)
        {
            if (Player != null && Player == this)
            {
                if (PlayerCombat.instance.isAttacking || PlayerCombat.instance.isCastingSpell)
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change spells while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT);
                    return false;
                }

            }

            spellInUse = spellToEquip;
            return true;
        }

        public bool UnequipSpell()
        {
            if (Player != null && Player == this)
            {
                if (PlayerCombat.instance.isAttacking || PlayerCombat.instance.isCastingSpell)
                {
                    AlertMessage.instance.InitAlertMessage("You cannot change spells while attacking.", AlertMessage.DEFAULT_MESSAGE_DURATION_SHORT);
                    return false;
                }

            }

            spellInUse = null;
            return true;
        }

        public SpellsKnowledgeSaveData ToSaveData()
        {
            SpellsKnowledgeSaveData saveData = new SpellsKnowledgeSaveData();

            for (int i = 0; i < Spells.Count; i++)
                saveData.items.Add(Spells[i].spellID);

            if(SpellsKnowledge.Player.spellInUse != null)
                saveData.equippedSpell = SpellsKnowledge.Player.spellInUse.spellID;

            return saveData;
        }
    }
}