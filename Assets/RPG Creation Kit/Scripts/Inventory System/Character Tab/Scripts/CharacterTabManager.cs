using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGCreationKit;
using RPGCreationKit.Player;

public class CharacterTabManager : MonoBehaviour
{
    [Header("Player Info Content")]
    [SerializeField] TextMeshProUGUI levelValue;
    [SerializeField] TextMeshProUGUI healthValue;
    [SerializeField] TextMeshProUGUI staminaValue;
    [SerializeField] TextMeshProUGUI manaValue;
    [SerializeField] TextMeshProUGUI xpValue;

    [SerializeField] Slider healthSlider;
    [SerializeField] Slider staminaSlider;
    [SerializeField] Slider manaSlider;
    [SerializeField] Slider xpSlider;

    [Space(5)]

    [Header("Stats Content")]
    [SerializeField] TextMeshProUGUI strengthValue;
    [SerializeField] TextMeshProUGUI dexterityValue;
    [SerializeField] TextMeshProUGUI agilityValue;
    [SerializeField] TextMeshProUGUI constitutionValue;
    [SerializeField] TextMeshProUGUI speedValue;
    [SerializeField] TextMeshProUGUI enduranceValue;
    [SerializeField] TextMeshProUGUI charismaValue;
    [SerializeField] TextMeshProUGUI intelligenceValue;
    [SerializeField] TextMeshProUGUI willpwowerValue;

    // Levelling attributes
    public GameObject alertInCharacterUI;
    public GameObject activePointsUI;
    public TextMeshProUGUI activePointsUIText;
    public TextMeshProUGUI activePointsInTabText;

    public List<GameObject> lvlUpButtons;

    private void OnEnable()
    {
        SetAttributesColors(EntityBaseAttributes.Strength, strengthValue);
        SetAttributesColors(EntityBaseAttributes.Agility, agilityValue);
        SetAttributesColors(EntityBaseAttributes.Constitution, constitutionValue);
        SetAttributesColors(EntityBaseAttributes.Speed, speedValue);
        SetAttributesColors(EntityBaseAttributes.Endurance, enduranceValue);
        SetAttributesColors(EntityBaseAttributes.Charisma, charismaValue);
        SetAttributesColors(EntityBaseAttributes.Intelligence, intelligenceValue);
        SetAttributesColors(EntityBaseAttributes.Willpower, willpwowerValue);
    }

    private void Update()
    {
        // Update PlayerInfo

        levelValue.text = EntityAttributes.PlayerAttributes.curLevel.ToString();

        healthValue.text = ((int)EntityAttributes.PlayerAttributes.CurHealth + "/" + (int)EntityAttributes.PlayerAttributes.derivedAttributes.maxHealth);
        staminaValue.text = ((int)EntityAttributes.PlayerAttributes.CurStamina + "/" + (int)EntityAttributes.PlayerAttributes.derivedAttributes.maxStamina);
        manaValue.text = ((int)EntityAttributes.PlayerAttributes.CurMana + "/" + (int)EntityAttributes.PlayerAttributes.derivedAttributes.maxMana);
        xpValue.text = ((int)EntityAttributes.PlayerAttributes.curXP + "/" + (int)EntityAttributes.PlayerAttributes.nextLvlXP);

        healthSlider.maxValue = EntityAttributes.PlayerAttributes.derivedAttributes.maxHealth;
        healthSlider.value = EntityAttributes.PlayerAttributes.CurHealth;

        staminaSlider.maxValue = EntityAttributes.PlayerAttributes.derivedAttributes.maxStamina;
        staminaSlider.value = EntityAttributes.PlayerAttributes.CurStamina;

        manaSlider.maxValue = EntityAttributes.PlayerAttributes.derivedAttributes.maxMana;
        manaSlider.value = EntityAttributes.PlayerAttributes.CurMana;

        xpSlider.maxValue = EntityAttributes.PlayerAttributes.nextLvlXP;
        xpSlider.value = EntityAttributes.PlayerAttributes.curXP;

        strengthValue.text = EntityAttributes.PlayerAttributes.attributes.Strength.ToString();
        dexterityValue.text = EntityAttributes.PlayerAttributes.attributes.Dexterity.ToString();
        agilityValue.text = EntityAttributes.PlayerAttributes.attributes.Agility.ToString();
        constitutionValue.text = EntityAttributes.PlayerAttributes.attributes.Constitution.ToString();
        speedValue.text = EntityAttributes.PlayerAttributes.attributes.Speed.ToString();
        enduranceValue.text = EntityAttributes.PlayerAttributes.attributes.Endurance.ToString();
        charismaValue.text = EntityAttributes.PlayerAttributes.attributes.Charisma.ToString();
        intelligenceValue.text = EntityAttributes.PlayerAttributes.attributes.Intelligence.ToString();
        willpwowerValue.text = EntityAttributes.PlayerAttributes.attributes.Willpower.ToString();


        activePointsInTabText.text = EntityAttributes.PlayerAttributes.curAttributePoints.ToString();
        if (EntityAttributes.PlayerAttributes.curAttributePoints > 0)
        {
            activePointsUI.SetActive(true);
            alertInCharacterUI.SetActive(true);

            activePointsUIText.text = EntityAttributes.PlayerAttributes.curAttributePoints.ToString();

            for (int i = 0; i < lvlUpButtons.Count; i++)
                if (!lvlUpButtons[i].activeSelf)
                    lvlUpButtons[i].SetActive(true);
        }
        else
        {
            activePointsUI.SetActive(false);
            alertInCharacterUI.SetActive(false);

            for (int i = 0; i < lvlUpButtons.Count; i++)
                if (lvlUpButtons[i].activeSelf)
                {
                    lvlUpButtons[i].SetActive(false);

                    // Fixes an UI bug
                    AttributeInInventoryUI aUI = lvlUpButtons[i].GetComponent<AttributeInInventoryUI>();
                    if (aUI)
                        aUI.tooltipGameObject.SetActive(false);
                }
        }
    }

    private void SetAttributesColors(EntityBaseAttributes _attribute, TextMeshProUGUI _text)
    {
        switch(EntityAttributes.PlayerAttributes.GetAttributeState(_attribute))
        {
            case EntityBaseAttributesState.Buffed:
                _text.color = Color.green;
                break;

            case EntityBaseAttributesState.Debuffed:
                _text.color = Color.red;
                break;

            case EntityBaseAttributesState.Normal:
                _text.color = Color.white;
                break;
        }
    }


    public void IncreaseAttribute(int attr)
    {
        EntityAttributes playerAttr = EntityAttributes.PlayerAttributes;

        switch(attr)
        {
            case 0:
                playerAttr.attributes.Strength += 1;
                break;
            case 1:
                playerAttr.attributes.Agility += 1;
                break;
            case 2:
                playerAttr.attributes.Constitution += 1;
                break;
            case 3:
                playerAttr.attributes.Speed += 1;
                break;
            case 4:
                playerAttr.attributes.Endurance += 1;
                break;
            case 5:
                playerAttr.attributes.Dexterity += 1;
                break;
            case 6:
                playerAttr.attributes.Charisma += 1;
                break;
            case 7:
                playerAttr.attributes.Intelligence += 1;
                break;
            case 8:
                playerAttr.attributes.Willpower += 1;
                break;
        }

        playerAttr.curAttributePoints -= 1;
        playerAttr.derivedAttributes.CalculateFromAttributes(playerAttr.attributes);

        RckPlayer.instance.UpdateHealthStaminaGUI();
    }
}
