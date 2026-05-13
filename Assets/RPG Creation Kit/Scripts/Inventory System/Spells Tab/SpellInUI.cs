using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{

    public class SpellInUI : ItemInInventoryUI, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        public SpellsPoolManager poolManager;

        public Spell spell;
        bool isEquipped = false;

        public void Init(Spell _spell)
        {
            spell = _spell;
            UpdateItem();
        }

        public override void UpdateItem()
        {
            // Set the UI elements
            Icon.sprite = spell.spellIcon;
            Name.text = spell.spellName;
            DamageVal.text = spell.magnitude.ToString();
            ArmorVal.text = "-";
            WeightVal.text = "-";
            GoldsVal.text = "-";
            AmountVal.text = "";

            isEquipped = (SpellsKnowledge.Player.spellInUse != null) && spell.spellID == SpellsKnowledge.Player.spellInUse.spellID;
            equippedIcon.SetActive(isEquipped);
        }

        public override void OnClick(bool takeAll = false)
        {
            EquipUnequip();
        }

        public void EquipUnequip()
        {
            if(!isEquipped)
            {
                if (SpellsKnowledge.Player.EquipSpell(spell))
                {
                    if (spell.sOnEquip)
                        GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, spell.sOnEquip);

                    PlayerCombat.instance.SetWeaponUI();
                }
            }
            else
            {
                if (SpellsKnowledge.Player.UnequipSpell())
                {
                    if (spell.sOnEquip)
                        GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, spell.sOnEquip);

                    PlayerCombat.instance.SetWeaponUI();
                }
            }

            UpdateItem();
            poolManager.UpdateAllSpellsUI();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (spell.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = spell.tooltipValue;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (spell.usesTooltip)
                tooltipGameObject.SetActive(false);
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (spell != null && spell.usesTooltip)
            {
                tooltipGameObject.SetActive(true);
                tooltipText.text = spell.tooltipValue;
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (spell != null && spell.usesTooltip)
                tooltipGameObject.SetActive(false);
        }
    }
}