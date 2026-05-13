using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.SaveSystem;
using RPGCreationKit.Player;

namespace RPGCreationKit
{

    public enum EntityBaseAttributes
    {
        Strength = 0,
        Dexterity = 1,
        Agility = 2,
        Constitution = 3,
        Speed = 4,
        Endurance = 5,
        Charisma = 6,
        Intelligence = 7,
        Willpower = 8
    };

    public enum EntityBaseAttributesState
    {
        Normal = 0,
        Buffed = 1,
        Debuffed = 2
    }

    [System.Serializable]
    public class BaseAttributes
    {
        public int Strength = 25;
        public int Dexterity = 25;
        public int Agility = 25;
        public int Constitution = 25;
        public int Speed = 25;
        public int Endurance = 25;
        public int Charisma = 25;
        public int Intelligence = 25;
        public int Willpower = 25;

        public static BaseAttributes Zero()
        {
            BaseAttributes attr = new BaseAttributes();
            attr.Strength = 0;
            attr.Dexterity = 0;
            attr.Agility = 0;
            attr.Constitution = 0;
            attr.Speed = 0;
            attr.Endurance = 0;
            attr.Charisma = 0;
            attr.Intelligence = 0;
            attr.Willpower = 0;
            return attr;
        }

        // Define the defualt settings for attributes
        [ContextMenu("Set to Default")]
        public void SetToDefault()
        {
            Strength = 1;
            Dexterity = 1;
            Agility = 1;
            Constitution = 25;
            Speed = 1;
            Endurance = 25;
            Charisma = 1;
            Intelligence = 1;
            Willpower = 25;
        }

        public static BaseAttributes Subtraction(BaseAttributes attr, BaseAttributes min)
        {
            attr.Strength -= min.Strength;
            attr.Dexterity -= min.Dexterity;
            attr.Agility -= min.Agility;
            attr.Constitution -= min.Constitution;
            attr.Speed -= min.Speed;
            attr.Endurance -= min.Endurance;
            attr.Charisma -= min.Charisma;
            attr.Intelligence -= min.Intelligence;
            attr.Willpower -= min.Willpower;
            return attr;
        }

        public static BaseAttributes Addition(BaseAttributes attr, BaseAttributes min)
        {
            attr.Strength += min.Strength;
            attr.Dexterity += min.Dexterity;
            attr.Agility += min.Agility;
            attr.Constitution += min.Constitution;
            attr.Speed += min.Speed;
            attr.Endurance += min.Endurance;
            attr.Charisma += min.Charisma;
            attr.Intelligence += min.Intelligence;
            attr.Willpower += min.Willpower;
            return attr;
        }
    }

    [System.Serializable]
    public class EntityDerivedAttributes
    {
        public float maxHealth = RCKSettings.ATTRIBUTES_DEF_HEALTH;
        public float curHealth = RCKSettings.ATTRIBUTES_DEF_HEALTH;
        public float curMana = RCKSettings.ATTRIBUTES_DEF_MANA;
        public float maxMana = RCKSettings.ATTRIBUTES_DEF_MANA;

        public float maxStamina = RCKSettings.ATTRIBUTES_DEF_STAMINA;
        public float curStamina = RCKSettings.ATTRIBUTES_DEF_STAMINA;

        public int maxEncumbrance = 150;

        public float walkSpeed = 1.4f;
        public float jogSpeed = 1.4f;
        public float runSpeed = 2.5f;
        public float crouchSpeed = 2.5f;

        [Range(20, 200), Tooltip("Determines the accuracy of the shots of this AI, 100 means it will always aim correctly, 0 means it will likely never get the target")]
        public float rangedCombatAccuracy = 70;

        [Range(0, 100), Tooltip("Determines the ability of this AI to Predict the position of its target while using a ranged weapon. This includes aiming higher to get targets that are further away (or just too far away for the Weapon's Reach")]
        public float rangedCombatPrediction = 70;

        [Tooltip("1.0 means base weapon spread, < 1.0 means less spred than usual, while > 1.0 means more spread than usual")]
        public float firearmSpreadMultiplier = 1.0f;

        public void CalculateFromAttributes(BaseAttributes baseAttributes, bool clamp = false)
        {
            maxHealth = RCKSettings.GetMaxHealthCalculation(baseAttributes.Constitution, baseAttributes.Strength);
            maxMana = RCKSettings.GetMaxManaCalculation(baseAttributes.Willpower, baseAttributes.Intelligence);
            maxStamina = RCKSettings.GetMaxStaminaCalculation(baseAttributes.Endurance);

            maxEncumbrance = RCKSettings.GetMaxEncumbranceCalculation(baseAttributes.Strength);

            // Apply to player specific scripts
            if (EntityAttributes.PlayerAttributes != null && EntityAttributes.PlayerAttributes.derivedAttributes == this)
            {
                jogSpeed = RCKSettings.GetJogSpeedCalculation(baseAttributes.Speed);
                runSpeed = RCKSettings.GetRunSpeedCalculation(baseAttributes.Speed);
                crouchSpeed = RCKSettings.GetCrouchSpeedCalculation(baseAttributes.Speed);

                RckPlayer.instance.jogSpeed = jogSpeed;
                RckPlayer.instance.runSpeed = runSpeed;
                RckPlayer.instance.crouchSpeed = crouchSpeed;

                RckPlayer.instance.recoverHealthAmount = RCKSettings.GetHealthRegenRate(baseAttributes.Constitution);
                RckPlayer.instance.recoverAfterHitDelay = RCKSettings.GetHealthRecoverAfterHitDelay(baseAttributes.Constitution);

                RckPlayer.instance.recoverStaminaAmount = RCKSettings.GetStaminaRegenRate(baseAttributes.Endurance);
                RckPlayer.instance.recoverAfterActionDelay = RCKSettings.GetStaminaRecoverAfterHitDelay(baseAttributes.Endurance);

                RckPlayer.instance.recoverManaAmount = RCKSettings.GetManaRegenRate(baseAttributes.Willpower);
                RckPlayer.instance.recoverAfterManaUseDelay = RCKSettings.GetStaminaRecoverAfterHitDelay(baseAttributes.Willpower);
            }

            if (clamp)
            {
                if(curHealth >= maxHealth)
                    curHealth = maxHealth;

                if(curMana >= maxMana)
                    curMana = maxMana;

                if(curStamina  >= maxStamina)
                    curStamina = maxStamina;
            }
        }

        public void SetCurValuesToMax()
        {
            curHealth = maxHealth;
            curMana = maxMana;
            curStamina = maxStamina;
        }
    }

    public class EntityAttributes : MonoBehaviour
    {
        #region PlayerAttributes
        public static EntityAttributes PlayerAttributes;
        private void Awake()
        {
            if (!PlayerAttributes)
                if (gameObject.CompareTag("Player"))
                    PlayerAttributes = this;
        }

        [SerializeField] Transform activeEffectsUIContainer;
        #endregion

        [Header("Attributes")]

        [SerializeField]
        public BaseAttributes attributes;

        // Adds attributes are the offset that creates when an attribute is fortified/Damaged, it's used to restore the correct base value when saving attributes
        [HideInInspector]
        public BaseAttributes addsAttributes = BaseAttributes.Zero();

        [Space(5)]

        [SerializeField]
        public EntityDerivedAttributes derivedAttributes;


        public bool bleeds = true;
        public GameObject bloodPrefab;

        // Levelling
        public uint curLevel = 1;
        public ulong curXP = 0;
        public ulong nextLvlXP = RCKSettings.GetNextLevelXPValue(1);
        public uint curAttributePoints = 0;
        public float xpMultiplierOnDeath = 1.0f; // when an entity is killed, gained xp is multiplied by this value

        public bool skipAttributesCalculationOnStart = false;

        public float CurHealth
        {
            get { return derivedAttributes.curHealth; }
            set { derivedAttributes.curHealth = value; }
        }

        public float CurStamina
        {
            get { return derivedAttributes.curStamina; }
            set { derivedAttributes.curStamina = value; }
        }

        public float CurMana
        {
            get { return derivedAttributes.curMana; }
            set { derivedAttributes.curMana = value; }
        }

        public float MaxHealth
        {
            get { return derivedAttributes.maxHealth; }
            set
            {
                derivedAttributes.maxHealth = value;

                if (CurHealth >= derivedAttributes.maxHealth)
                    CurHealth = derivedAttributes.maxHealth;

                if (PlayerAttributes == this)
                    Player.RckPlayer.instance.UpdateHealthStaminaGUI();
            }
        }

        public float MaxStamina
        {
            get { return derivedAttributes.maxStamina; }
            set
            {
                derivedAttributes.maxStamina = value;

                if (CurStamina >= derivedAttributes.maxStamina)
                    CurStamina = derivedAttributes.maxStamina;

                if (PlayerAttributes == this)
                    Player.RckPlayer.instance.UpdateHealthStaminaGUI();
            }
        }

        public float MaxMana
        {
            get { return derivedAttributes.maxMana; }
            set
            {
                derivedAttributes.maxMana = value;

                if (CurMana >= derivedAttributes.maxMana)
                    CurMana = derivedAttributes.maxStamina;

                if (PlayerAttributes == this)
                    Player.RckPlayer.instance.UpdateHealthStaminaGUI();
            }
        }

        [Space(5)]

        public List<EffectOnEntity> activeEffects;
        //public Dictionary<int, EffectOnEntity> activeEffects;

        public void ExecuteEffects(EffectOnEntity[] effects)
        {
            bool overtime = false;

            for (int i = 0; i < effects.Length; i++)
            {
                EffectOnEntity effectCopy = new EffectOnEntity(effects[i]);

                switch (effects[i].effectType)
                {
                    case ConsumableEffectType.DamageAttribute:
                        effectCopy.isOnDuration = true;
                        overtime = (effectCopy.duration != 0);
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        DamageAttribute(effectCopy, overtime);

                        activeEffects.Add(effectCopy); break;

                    case ConsumableEffectType.DamageHealth:
                        effectCopy.isOnDuration = false;
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        overtime = (effectCopy.duration != 0);
                        DamageHealth(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;

                    case ConsumableEffectType.DamageStamina:
                        effectCopy.isOnDuration = false;
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        overtime = (effectCopy.duration != 0);
                        DamageStamina(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;

                    case ConsumableEffectType.DamageMana:
                        effectCopy.isOnDuration = true;
                        overtime = (effectCopy.duration != 0);
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        DamageMana(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;

                    case ConsumableEffectType.FortifyAttribute:
                        effectCopy.isOnDuration = true;
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        overtime = (effectCopy.duration != 0);

                        FortifyAttribute(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;

                    case ConsumableEffectType.FortifyHealth:
                        effectCopy.isOnDuration = true;
                        overtime = (effectCopy.duration != 0);
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        FortifyHealth(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;

                    case ConsumableEffectType.FortifyStamina:
                        effectCopy.isOnDuration = true;
                        overtime = (effectCopy.duration != 0);
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        FortifyStamina(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;

                    case ConsumableEffectType.FortifyMana:
                        effectCopy.isOnDuration = true;
                        overtime = (effectCopy.duration != 0);
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        FortifyMana(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;


                    case ConsumableEffectType.RestoreAttribute:
                        break;

                    case ConsumableEffectType.RestoreHealth:

                        overtime = (effectCopy.duration != 0);
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;
                        
                        RestoreHealth(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;

                    case ConsumableEffectType.RestoreStamina:
                        overtime = (effectCopy.duration != 0);
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;
                        
                        RestoreStamina(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;

                    case ConsumableEffectType.RestoreMana:
                        overtime = (effectCopy.duration != 0);
                        effectCopy.magnitudeAlreadyApplied = effects[i].magnitudeAlreadyApplied;

                        RestoreMana(effectCopy, overtime);

                        activeEffects.Add(effectCopy);
                        break;
                }


            }

            if(PlayerAttributes == this)
                RckPlayer.instance.UpdateHealthStaminaGUI();
        }

        public void DamageAttribute(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(DamageAttributeUpdate(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, true);
                }
            }
            else
                AlterAttribute(effect, false);
        }

        public void FortifyAttribute(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(FortifyAttributeUpdate(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                        EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                        effectUI.gameObject.SetActive(true);
                        effectUI.Init(effect, true);
                }
            }
            else
                AlterAttribute(effect, true);
        }

        public void RestoreAttribute(EntityBaseAttributes _attribute, float _duration, float _magnitude)
        {

        }

        public void DamageHealth(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(DecreaseHealth(effect));

                // Show on UI
                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, false);
                }
            }
            else
                CurHealth -= effect.magnitude;
        }

        public void DamageHealth(float _amount, bool _overtime, float rate)
        {
            if (_overtime)
                StartCoroutine(DecreaseHealth(_amount, rate));
            else
                CurHealth -= _amount;
        }

        public void DamageStamina(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(DecreaseStamina(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, false);
                }
            }
            else
                CurStamina -= effect.magnitude;
        }

        public void DamageMana(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(DecreaseMana(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, false);
                }
            }
            else
                CurMana -= effect.magnitude;
        }

        public void DamageStamina(float _amount, bool _overtime, float rate)
        {
            if (_overtime)
                StartCoroutine(DecreaseStamina(_amount, rate));
            else
                CurStamina -= _amount;
        }

        public void DamageMana(float _amount, bool _overtime, float rate)
        {
            if (_overtime)
                StartCoroutine(DecreaseMana(_amount, rate));
            else
                CurMana -= _amount;
        }

        public void RestoreHealth(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(IncreaseHealth(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, false);
                }
            }
            else
            {
                CurHealth += effect.magnitude;
                if (CurHealth >= MaxHealth)
                    CurHealth = MaxHealth;
            }
        }

        public void RestoreStamina(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(IncreaseStamina(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, false);
                }
            }
            else
            {
                CurStamina += effect.magnitude;
                if (CurStamina >= MaxStamina)
                    CurStamina = MaxStamina;
            }
        }

        public void RestoreMana(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(IncreaseMana(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, false);
                }
            }
            else
            {
                CurMana += effect.magnitude;
                if (CurMana >= MaxMana)
                    CurMana = MaxMana;
            }
        }

        public void FortifyHealth(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(FortifyHealthUpdate(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, true);
                }
            }
            else
                MaxHealth += effect.magnitude;
        }

        public void FortifyStamina(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(FortifyStaminaUpdate(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, true);
                }
            }
            else
                MaxStamina += effect.magnitude;
        }

        public void FortifyMana(EffectOnEntity effect, bool _overtime = false)
        {
            if (_overtime)
            {
                StartCoroutine(FortifyManaUpdate(effect));

                if (PlayerAttributes == this && effect.showInEffectsUI)
                {
                    EffectOnEntityUI effectUI = EffectsOnPlayerUIPoolManager.pool.GetPooledObject();
                    effectUI.gameObject.SetActive(true);
                    effectUI.Init(effect, true);
                }
            }
            else
                MaxMana += effect.magnitude;
        }

        void AlterAttribute(EffectOnEntity _effect, bool _add)
        {
            switch (_effect.onAttribute)
            {
                case EntityBaseAttributes.Strength:
                    if (_add)
                    {
                        attributes.Strength += (int)_effect.magnitude;
                        addsAttributes.Strength += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Strength -= (int)_effect.magnitude;
                        addsAttributes.Strength -= (int)_effect.magnitude;
                    }
                    break;

                case EntityBaseAttributes.Dexterity:
                    if (_add)
                    {
                        attributes.Dexterity += (int)_effect.magnitude;
                        addsAttributes.Dexterity += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Dexterity -= (int)_effect.magnitude;
                        addsAttributes.Dexterity -= (int)_effect.magnitude;
                    }
                    break;

                case EntityBaseAttributes.Agility:
                    if (_add)
                    {
                        attributes.Agility += (int)_effect.magnitude;
                        addsAttributes.Agility += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Agility -= (int)_effect.magnitude;
                        addsAttributes.Agility -= (int)_effect.magnitude;
                    }
                    break;

                case EntityBaseAttributes.Constitution:
                    if (_add)
                    {
                        attributes.Constitution += (int)_effect.magnitude;
                        addsAttributes.Constitution += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Constitution -= (int)_effect.magnitude;
                        addsAttributes.Constitution -= (int)_effect.magnitude;
                    }
                    break;

                case EntityBaseAttributes.Speed:
                    if (_add)
                    {
                        attributes.Speed += (int)_effect.magnitude;
                        addsAttributes.Speed += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Speed -= (int)_effect.magnitude;
                        addsAttributes.Speed -= (int)_effect.magnitude;
                    }
                    break;

                case EntityBaseAttributes.Endurance:
                    if (_add)
                    {
                        attributes.Endurance += (int)_effect.magnitude;
                        addsAttributes.Endurance += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Endurance -= (int)_effect.magnitude;
                        addsAttributes.Endurance -= (int)_effect.magnitude;
                    }
                    break;

                case EntityBaseAttributes.Charisma:
                    if (_add)
                    {
                        attributes.Charisma += (int)_effect.magnitude;
                        addsAttributes.Charisma += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Charisma -= (int)_effect.magnitude;
                        addsAttributes.Charisma -= (int)_effect.magnitude;
                    }
                    break;

                case EntityBaseAttributes.Intelligence:
                    if (_add)
                    {
                        attributes.Intelligence += (int)_effect.magnitude;
                        addsAttributes.Intelligence += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Intelligence -= (int)_effect.magnitude;
                        addsAttributes.Intelligence -= (int)_effect.magnitude;
                    }
                    break;

                case EntityBaseAttributes.Willpower:
                    if (_add)
                    {
                        attributes.Willpower += (int)_effect.magnitude;
                        addsAttributes.Willpower += (int)_effect.magnitude;
                    }
                    else
                    {
                        attributes.Willpower -= (int)_effect.magnitude;
                        addsAttributes.Willpower -= (int)_effect.magnitude;
                    }
                    break;
            }

            derivedAttributes.CalculateFromAttributes(attributes, true);

            if (PlayerAttributes == this)
                RckPlayer.instance.UpdateHealthStaminaGUI();
        }

        IEnumerator FortifyHealthUpdate(EffectOnEntity effect)
        {
            MaxHealth += effect.magnitude;

            while (effect.duration >= effect.magnitudeAlreadyApplied)
            {
                while (GameStatus.instance.IsPaused)
                    yield return null;

                effect.magnitudeAlreadyApplied += 1 * Time.deltaTime;


                yield return null;
            }

            MaxHealth -= effect.magnitude;

            yield return null;
        }

        IEnumerator FortifyAttributeUpdate(EffectOnEntity effect)
        {
            AlterAttribute(effect, true);

            while (effect.duration >= effect.magnitudeAlreadyApplied)
            {
                while (GameStatus.instance.IsPaused)
                    yield return null;

                effect.magnitudeAlreadyApplied += 1 * Time.deltaTime;


                yield return null;
            }

            AlterAttribute(effect, false);
            yield return null;
        }

        IEnumerator DamageAttributeUpdate(EffectOnEntity effect)
        {
            AlterAttribute(effect, false);

            while (effect.duration >= effect.magnitudeAlreadyApplied)
            {
                while (GameStatus.instance.IsPaused)
                    yield return null;

                effect.magnitudeAlreadyApplied += 1 * Time.deltaTime;

                yield return null;
            }

            AlterAttribute(effect, true);
            yield return null;
        }

        IEnumerator FortifyStaminaUpdate(EffectOnEntity effect)
        {
            MaxStamina += effect.magnitude;

            while (effect.duration >= effect.magnitudeAlreadyApplied)
            {
                while (GameStatus.instance.IsPaused)
                    yield return null;

                effect.magnitudeAlreadyApplied += 1 * Time.deltaTime;

                //Debug.Log(effect.duration + " | " + effect.magnitudeAlreadyApplied);

                yield return null;
            }

            MaxStamina -= effect.magnitude;

            yield return null;
        }

        IEnumerator FortifyManaUpdate(EffectOnEntity effect)
        {
            MaxMana += effect.magnitude;

            while (effect.duration >= effect.magnitudeAlreadyApplied)
            {
                while (GameStatus.instance.IsPaused)
                    yield return null;

                effect.magnitudeAlreadyApplied += 1 * Time.deltaTime;

                //Debug.Log(effect.duration + " | " + effect.magnitudeAlreadyApplied);

                yield return null;
            }

            MaxMana -= effect.magnitude;

            yield return null;
        }


        // -----------------------------------------------------------------------------
        // To gradually decrease a value - Health
        // -----------------------------------------------------------------------------
        IEnumerator DecreaseHealth(EffectOnEntity effect)
        {
            float decreased = effect.magnitudeAlreadyApplied;
            float delta = effect.magnitude / effect.duration;

            while (true)
            {
                float maxDecrease = effect.magnitude - decreased;
                float decreaseRate = Time.deltaTime * delta;

                if (decreaseRate > maxDecrease) // Reached max damage
                {
                    CurHealth -= maxDecrease;
                    effect.magnitudeAlreadyApplied += maxDecrease;
                    yield break;
                }
                else
                {
                    CurHealth -= decreaseRate;
                    decreased += decreaseRate;
                    effect.magnitudeAlreadyApplied += decreaseRate;
                    yield return null;
                }

                CurHealth = Mathf.Clamp(CurHealth, 0, MaxHealth);
            }
        }

        IEnumerator DecreaseHealth(float _amount, float rate = 1)
        {
            float decreased = 0f;

            while (true)
            {
                float maxDecrease = _amount - decreased;
                float decreaseRate = Time.deltaTime * rate;

                if (decreaseRate > maxDecrease) // Reached max damage
                {
                    CurHealth -= maxDecrease;
                    yield break;
                }
                else
                {
                    CurHealth -= decreaseRate;
                    decreased += decreaseRate;
                    yield return null;
                }

                CurHealth = Mathf.Clamp(CurHealth, 0, MaxHealth);
            }
        }

        // -----------------------------------------------------------------------------
        // To gradually increase a value - Health
        // -----------------------------------------------------------------------------
        IEnumerator IncreaseHealth(EffectOnEntity effect)
        {
            float increased = effect.magnitudeAlreadyApplied;
            float delta = effect.magnitude / effect.duration;

            while (true)
            {
                float maxIncrease = effect.magnitude - increased;
                float increaseRate = Time.deltaTime * delta;

                if (increaseRate > maxIncrease) // Reached max damage
                {
                    CurHealth += maxIncrease;
                    effect.magnitudeAlreadyApplied += maxIncrease;
                    yield break;
                }
                else
                {
                    CurHealth += increaseRate;
                    increased += increaseRate;
                    effect.magnitudeAlreadyApplied += increaseRate;
                    yield return null;
                }

                CurHealth = Mathf.Clamp(CurHealth, 0, MaxHealth);
            }
        }

        // -----------------------------------------------------------------------------
        // To gradually decrease a value - Stamina
        // -----------------------------------------------------------------------------
        IEnumerator DecreaseStamina(EffectOnEntity effect)
        {
            float decreased = effect.magnitudeAlreadyApplied;
            float delta = effect.magnitude / effect.duration;

            while (true)
            {
                float maxDecrease = effect.magnitude - decreased;
                float decreaseRate = Time.deltaTime * delta;

                if (decreaseRate > maxDecrease) // Reached max damage
                {
                    CurStamina -= maxDecrease;
                    effect.magnitudeAlreadyApplied += maxDecrease;
                    yield break;
                }
                else
                {
                    CurStamina -= decreaseRate;
                    decreased += decreaseRate;
                    effect.magnitudeAlreadyApplied += decreaseRate;
                    yield return null;
                }

                CurStamina = Mathf.Clamp(CurStamina, 0, MaxStamina);
            }
        }

        // -----------------------------------------------------------------------------
        // To gradually decrease a value - Stamina
        // -----------------------------------------------------------------------------
        IEnumerator DecreaseMana(EffectOnEntity effect)
        {
            float decreased = effect.magnitudeAlreadyApplied;
            float delta = effect.magnitude / effect.duration;

            while (true)
            {
                float maxDecrease = effect.magnitude - decreased;
                float decreaseRate = Time.deltaTime * delta;

                if (decreaseRate > maxDecrease) // Reached max damage
                {
                    CurMana -= maxDecrease;
                    effect.magnitudeAlreadyApplied += maxDecrease;
                    yield break;
                }
                else
                {
                    CurMana -= decreaseRate;
                    decreased += decreaseRate;
                    effect.magnitudeAlreadyApplied += decreaseRate;
                    yield return null;
                }

                CurMana = Mathf.Clamp(CurMana, 0, MaxMana);
            }
        }

        IEnumerator DecreaseStamina(float _amount, float rate = 1)
        {
            float decreased = 0f;

            while (true)
            {
                float maxDecrease = _amount - decreased;
                float decreaseRate = Time.deltaTime * rate;

                if (decreaseRate > maxDecrease) // Reached max damage
                {
                    CurStamina -= maxDecrease;
                    yield break;
                }
                else
                {
                    CurStamina -= decreaseRate;
                    decreased += decreaseRate;
                    yield return null;
                }

                CurStamina = Mathf.Clamp(CurStamina, 0, MaxStamina);
            }
        }

        IEnumerator DecreaseMana(float _amount, float rate = 1)
        {
            float decreased = 0f;

            while (true)
            {
                float maxDecrease = _amount - decreased;
                float decreaseRate = Time.deltaTime * rate;

                if (decreaseRate > maxDecrease) // Reached max damage
                {
                    CurMana -= maxDecrease;
                    yield break;
                }
                else
                {
                    CurMana -= decreaseRate;
                    decreased += decreaseRate;
                    yield return null;
                }

                CurMana = Mathf.Clamp(CurMana, 0, MaxMana);
            }
        }


        // -----------------------------------------------------------------------------
        // To gradually increase a value - Stamina
        // -----------------------------------------------------------------------------
        IEnumerator IncreaseStamina(EffectOnEntity effect)
        {
            float increased = effect.magnitudeAlreadyApplied;
            float delta = effect.magnitude / effect.duration;

            while (true)
            {
                float maxIncrease = effect.magnitude - increased;
                float increaseRate = Time.deltaTime * delta;

                if (increaseRate > maxIncrease) // Reached max damage
                {
                    CurStamina += maxIncrease;
                    effect.magnitudeAlreadyApplied += maxIncrease;
                    yield break;
                }
                else
                {
                    CurStamina += increaseRate;
                    increased += increaseRate;
                    effect.magnitudeAlreadyApplied += increaseRate;
                    yield return null;
                }

                CurStamina = Mathf.Clamp(CurStamina, 0, MaxStamina);
            }
        }

        // -----------------------------------------------------------------------------
        // To gradually increase a value - Mana
        // -----------------------------------------------------------------------------
        IEnumerator IncreaseMana(EffectOnEntity effect)
        {
            float increased = effect.magnitudeAlreadyApplied;
            float delta = effect.magnitude / effect.duration;

            while (true)
            {
                float maxIncrease = effect.magnitude - increased;
                float increaseRate = Time.deltaTime * delta;

                if (increaseRate > maxIncrease) // Reached max damage
                {
                    CurMana += maxIncrease;
                    effect.magnitudeAlreadyApplied += maxIncrease;
                    yield break;
                }
                else
                {
                    CurMana += increaseRate;
                    increased += increaseRate;
                    effect.magnitudeAlreadyApplied += increaseRate;
                    yield return null;
                }

                CurMana = Mathf.Clamp(CurMana, 0, MaxMana);
            }
        }


        private void Start()
        {
            nextLvlXP = RCKSettings.GetNextLevelXPValue(curLevel);

            if(!skipAttributesCalculationOnStart)
                derivedAttributes.CalculateFromAttributes(attributes);

            InvokeRepeating("CheckActiveEffects", 0, 0.5f);
        }

        // Sets the Level in base of the number of assigned attributes. Each 5 assigned attributes is a Level.
        // Useful when scaling XP gained by killing AI
        public void DeriveAndSetLevelFromAttributes()
        {
            curLevel = CalculateLevelFromAttributes(this);
        }

        public static uint CalculateLevelFromAttributes(EntityAttributes _entityAtt)
        {
            uint attributesPoints = 0;
            
            attributesPoints += (uint)(_entityAtt.attributes.Strength - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(_entityAtt.attributes.Dexterity - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(_entityAtt.attributes.Agility - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(_entityAtt.attributes.Constitution - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(_entityAtt.attributes.Speed - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(_entityAtt.attributes.Endurance - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(_entityAtt.attributes.Charisma - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(_entityAtt.attributes.Intelligence - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(_entityAtt.attributes.Willpower - RCKSettings.ATTRIBUTES_MIN_VALUE);

            uint calculatedLevel = 1 + (attributesPoints / RCKSettings.LEVELLING_AI_ATTRIBUTES_POINTS_PER_LEVEL);

            return calculatedLevel;
        }
        
        public static uint CalculateLevelFromAttributes(int str, int dex, int agi, int con, int spe, int end, int cha, int intell, int will)
        {
            uint attributesPoints = 0;

            attributesPoints += (uint)(str - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(dex - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(agi - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(con - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(spe - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(end - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(cha - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(intell - RCKSettings.ATTRIBUTES_MIN_VALUE);
            attributesPoints += (uint)(will - RCKSettings.ATTRIBUTES_MIN_VALUE);

            uint calculatedLevel = 1 + (attributesPoints / RCKSettings.LEVELLING_AI_ATTRIBUTES_POINTS_PER_LEVEL);

            return calculatedLevel;
        }

        /// <summary>
        /// Checks which effects are active on the character and removes them from the list if they're expired.
        /// </summary>
        public void CheckActiveEffects()
        {
            for(int i = activeEffects.Count-1; i >= 0; i--)
            {
                if (!activeEffects[i].isOnDuration)
                {
                    // Check if effect has expired/done what it had to
                    if (activeEffects[i].magnitudeAlreadyApplied >= activeEffects[i].magnitude)
                    {
                        activeEffects[i].isFinished = true;
                        activeEffects.RemoveAt(i);
                    }
                }
                else
                {
                    // Check if effect has expired/done what it had to
                    if (activeEffects[i].magnitudeAlreadyApplied >= activeEffects[i].duration)
                    {
                        activeEffects[i].isFinished = true;
                        activeEffects.RemoveAt(i);
                    }
                }
            }
        }


        /// <summary>
        /// Returns whenever an attribute is currently buffed, debuffed or not
        /// </summary>
        /// <param name="_attribute">The attribute to check</param>
        /// <returns></returns>
        public EntityBaseAttributesState GetAttributeState(EntityBaseAttributes _attribute)
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].effectType == ConsumableEffectType.DamageAttribute && activeEffects[i].onAttribute == _attribute)
                    return EntityBaseAttributesState.Debuffed;

                if (activeEffects[i].effectType == ConsumableEffectType.FortifyAttribute && activeEffects[i].onAttribute == _attribute)
                    return EntityBaseAttributesState.Buffed;
            }

            return EntityBaseAttributesState.Normal;
        }

        public void GainXP(ulong xpValue)
        {
            curXP += xpValue;

            if(curXP >= nextLvlXP)
            {
                ulong xpSurplus = curXP - nextLvlXP;

                LevelUp(xpSurplus);
            }

            if (this == EntityAttributes.PlayerAttributes)
                RckPlayer.instance.UpdateXpSlider();
        }

        public void LevelUp(ulong xpSurplus)
        {
            nextLvlXP = RCKSettings.GetNextLevelXPValue(curLevel);
            curLevel += 1;
            curXP = 0;
            curAttributePoints += RCKSettings.LEVELLING_POINTS_PER_LEVEL;

            // Tutorial alert
            if (PlayerAttributes == this && RCKSettings.LEVELLING_LEVEL_2_SKILL_TUTORIAL_ENABLED && curLevel == 2)
            {
                TutorialAlertMessage.instance.OpenMessageAfterDialogueEnds("You leveled up and gained " +RCKSettings.LEVELLING_AI_ATTRIBUTES_POINTS_PER_LEVEL+" Attribute Points!\n\nYou can assign your attribute points in the 'Character' tab accessible from the Inventory (I key).\n\nAttributes directly influence things like health, mana, damage and dialogue skill checks.");
            }

            // If the xp surplus doesn't trigger another lvl up
            if (xpSurplus < nextLvlXP)
            {
                GainXP(xpSurplus);

                if (PlayerAttributes == this)
                {
                    AlertMessage.instance.InitAlertMessage("You leveled up to level " + curLevel + "!", RCKSettings.LEVELLING_UP_ALERT_MESSAGE_DURATION);
                    GameAudioManager.instance.PlayOneShot(AudioSources.GeneralSounds, AudioClipsDatabase.GetItem("LEVEL_UP_SOUND"));
                }
            }
            else
                GainXP(xpSurplus); // this will trigger another lvl up
        }

        public EntityAttributesSaveData ToSaveData()
        {
            BaseAttributes.Subtraction(attributes, addsAttributes);
            EntityAttributesSaveData newAttData = new EntityAttributesSaveData(attributes, derivedAttributes, activeEffects, curLevel, curXP, nextLvlXP, curAttributePoints);
            return newAttData;
        }

        public void RestoreAddsAfterSave()
        {
            BaseAttributes.Addition(attributes, addsAttributes);
        }
    }
}