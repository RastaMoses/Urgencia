using RPGCreationKit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RPGCreationKit
{
    /// <summary>
    /// Class that contains general game settings. Tweak it as you like.
    /// </summary>
    public static class RCKSettings
    {
        public static float NPC_TO_NPC_CONVERSATION_DISTANCE = 3.5f;
        public static float NPC_LOOKAT_TARGET_DISTANCE = 5f;
        public static float NPC_STOP_FOLLOW_AFTER_DISTANCE = 2.2f;
        public static float PLAYER_HEARS_NPC_DIALOGUES_DISTANCE = 15f;
        public static float PLAYER_DISTANCE_TO_DIALOGUE = 3f;
        public static float PLAYER_DISTANCE_TO_DISPLAY_ENEMY_BAR = 100f;

        public static float CHARGED_ATTACK_MOUSE_DOWN_TIME = 0.5f;

        public static float MIN_VELOCITY_TO_CONTINUE_ROTATING_ARROW = 5f;

        public static float DISTANCE_SHOW_NPC_IN_COMBAT_UI = 100f;

        public static float MELEE_GENERAL_RIGIDBODY_FORCE = 20f;
        public static float ARROW_GENERAL_RIGIDBODY_FORCE = 5f;
        public static float FIREARM_GENERAL_RIGIDBODY_FORCE = 35f;

        public static float MELEE_ONBODY_RIGIDBODY_FORCE = 35f;
        public static float ARROW_ONBODY_RIGIDBODY_FORCE = 10f;
        public static float FIREARM_ONBODY_RIGIDBODY_FORCE = 40f;

        public static float DRAIN_HEALTH_DEFAULT_RATE = 200f;
        public static float DRAIN_STAMINA_DEFAULT_RATE = 200f;

        public static float DISTANCE_WHEN_NPCS_START_LOOK_AT = 3.5f;

        public static float CROSSHAIR_HIT_TIME = .25f;

        public static float STOPPING_DETECTION_THRESHOLD = 0.15f;

        public static float HOSTILE_FACTION_COMBAT_THERSHOLD = 10;

        public static float MAX_PICKPOCKET_DISTANCE = 5f;

        public static bool AUTO_ROTATE_AI_IF_DIALOGUE_BEHIND = true;

        public static bool NPCS_FACIAL_ANIM_ENABLED = true;

        public static float NPCS_Z_CHECK_FOR_THREE_DIMENSIONAL_DIST = 3.5f;

        // EDITOR
        public static string EDITOR_AI_LOAD_LOCATION = "Assets/RPG Creation Kit/Prefab Library/AI/";
        public static string EDITOR_AI_SAVE_LOCATION = "Assets/RPG Creation Kit/Prefab Library/AI/";

        public static string EDITOR_CREATUREAI_LOAD_LOCATION = "Assets/RPG Creation Kit/Prefab Library/AI/Creatures/";
        public static string EDITOR_CREATUREAI_SAVE_LOCATION = "Assets/RPG Creation Kit/Prefab Library/AI/Creatures/";

        public static string EDITOR_MOUNTAI_LOAD_LOCATION = "Assets/RPG Creation Kit/Prefab Library/AI/Mounts/";
        public static string EDITOR_MOUNTAI_SAVE_LOCATION = "Assets/RPG Creation Kit/Prefab Library/AI/Mounts/";


        // Code
        public static float DEFAULT_WEAPON_REACH = 4.0f;

        public static float INTERACTOR_RAYCAST_MAXDISTANCE = float.MaxValue;

        public static float DRAIN_STAMINA_ON_ATTACK_SPEEDAMOUNT = 50f;
        public static float DRAIN_STAMINA_ON_ATTACKBLOCKED_SPEEDAMOUNT = 50f;
        public static float DRAIN_MANA_ON_CAST_SPEEDAMOUNT = 50f;
        public static float DRAIN_STAMINA_ON_DODGE_SPEEDAMOUNT = 50f;


        public static float DODGE_STAMINA_DRAIN_ON_DODGE = 10.0f;
        public static float DODGE_DISTANCE_BASE = 4.0f;


        public static string GAME_VERSION = "2.0";
        public static string SAVE_TYPE_VERSION = "rck_default";

        // Save Version should be x.y not x.y.z (1.1, 1.12, 1.123 NOT 1.1.2), when version checking, the "." is removed and the version is the two numbers combined (1.1 = 11, 1.2 = 12, 1.12 = 112)
        public static string FILE_SAVE_VERSION = "2.0";

        public static float PROJECTILE_DESPAWN_TIME = 30f;

        public static float DISTANCE_BEFORE_ROTATING_TO_TARGET = 4.5f;

        public static bool PICKPOCKET_DIALOGUE_CLIP_PLAYS = true;

        public static float DIALOGUE_ENDLINE_DELAY = 0.2f; // adds a small delay after each npc dialogue line lenght, mainly used to give just a little bit time to the NPC before speaking another line

        // NEW GAME START
        //public static string RCK_NEW_STARTING_LOCATION = "Virrihael";
        //public static string RCK_NEW_STARTING_WORLDSPACEID = "VirrihaelWorldspace";
        //public static string RCK_NEW_STARTING_CELLID = "Virrihael(0,2)";

        //public static int RCK_NEW_STARTING_LEVEL = 1;
        //public static Vector3 RCK_NEW_STARTING_POS = new Vector3(-2.543413f, 5.548239f, 297.4933f);
        //public static Vector3 RCK_NEW_STARTING_ROT = new Vector3(0, 169.825f, 0);
        public static string RCK_NEW_STARTING_LOCATION = "Cave";
        public static string RCK_NEW_STARTING_WORLDSPACEID = "Interiors";
        public static string RCK_NEW_STARTING_CELLID = "TutorialsCave001";

        public static uint RCK_NEW_STARTING_LEVEL = 1;
        public static Vector3 RCK_NEW_STARTING_POS = new Vector3(1.445318f, 1.470858f, 0.2606843f);
        public static Vector3 RCK_NEW_STARTING_ROT = new Vector3(0, 89.906f, 0);


        public static float ATTRIBUTES_DEF_HEALTH = 1.0f;
        public static float ATTRIBUTES_DEF_STAMINA = 1.0f;
        public static float ATTRIBUTES_DEF_MANA = 1.0f;

        // Save
        public static bool JSON_PRETTY_PRINT = false;
        public static float PLAYER_DAMAGE_SPEED = 100f;

        public static float HORCOMPASS_MAX_DISTANCE_LOC_VISIBLE = 300.0f;

        // SETTINGS MENU - SET BY PLAYER AT RUNTIME
        public static float MOUSE_HORIZONTAL_SPEED;
        public static float MOUSE_VERTICAL_SPEED;
        public static bool AUTOSAVE_ENABLED;


        // PERSISTENT REFERENCE
        public static float PREF_SHOW_DISTANCE = 140;
        public static float PREF_HIDE_DISTANCE = 150;

        // COMPASS
        public static bool HORIZONTAL_COMPASS_ENABLED = true;
        public static bool ROUND_COMPASS_ENABLED = false;

        // FIREARMS
        public static float NORMAL_FOV = 60.0f;
        public static float FOV_CHANGE_SPEED = 10.0f;
        public static float RECOIL_DECRASE_RATE = 300.0f;
        public static float RECOIL_CROUCHED_REDUCTION = 0.85f; // 15% recoil reduction when crouched
        public static float SPREAD_CROUCHED_REDUCTION_HIP = 0.85f; // 15% SPREAD reduction when crouched
        public static float SPREAD_CROUCHED_REDUCTION_AIM = 0.85f; // 15% SPREAD reduction when crouched and aiming

        public static float COVER_AI_STOPPING_DISTANCE = 1f;
        public static float COVER_AI_COOLDOWN_MIN = 6;
        public static float COVER_AI_COOLDOWN_MAX = 30;

        public static float RECOIL_AI_SPREAD_MULTIPLIER = 3f;

        // THROWABLES
        public static bool TRIGGER_EXPLOSIVE_AS_SOON_AS_HOLD = true;
        public static float EXPLOSION_FORCE_FOR_AI_MULTIPLIER = 2.0f;

        // Demo
        public static bool ENABLE_DEMO_SELECTION = true;

        // Mounts
        public static float PLAYER_MOUNT_DISTANCE = 3.0f; // Distance between the player and a mount to let the mount command run

        // LEVELLING
        public static float LEVELLING_UP_ALERT_MESSAGE_DURATION = 5;
        public static uint LEVELLING_POINTS_PER_LEVEL = 5;
        public static bool LEVELLING_LEVEL_2_SKILL_TUTORIAL_ENABLED = true;

        // In AI configuration, every LEVELLING_AI_ATTRIBUTES_POINTS_PER_LEVEL attribute points increases the AI level by 1
        // (see EntityAttributes.DeriveAndSetLevelFromAttributes).
        public static uint LEVELLING_AI_ATTRIBUTES_POINTS_PER_LEVEL = 5;

        // Rework Attributes Defaults
        public static int ATTRIBUTES_MIN_VALUE = 1; // sets the absolute min value for attributes.

        // Player default attributes when he gets created for the first time
        public static int PLAYER_ATTRIBUTES_DEFAULT_STR = 25;
        public static int PLAYER_ATTRIBUTES_DEFAULT_DEX = 25;
        public static int PLAYER_ATTRIBUTES_DEFAULT_AGI = 25;
        public static int PLAYER_ATTRIBUTES_DEFAULT_CON = 25;
        public static int PLAYER_ATTRIBUTES_DEFAULT_SPD = 25;
        public static int PLAYER_ATTRIBUTES_DEFAULT_END = 25;
        public static int PLAYER_ATTRIBUTES_DEFAULT_CHA = 25;
        public static int PLAYER_ATTRIBUTES_DEFAULT_INT = 25;
        public static int PLAYER_ATTRIBUTES_DEFAULT_WIL = 25;

        public static float ATTR_BASE_WALK_SPEED = 1.5f;
        public static float ATTR_BASE_JOG_SPEED = 5.0f;
        public static float ATTR_BASE_RUN_SPEED = 9.5f;
        public static float ATTR_BASE_CROUCH_SPEED = 2.0f;
        public static int   ATTR_BASE_ENCUMBRANCE = 149;

        public static float ATTR_BASE_STAMINA_REGEN_RATE = 25.0f;
        public static float ATTR_BASE_STAMINA_RECOVER_AFTER_HIT_DELAY = 2.0f;

        public static float ATTR_BASE_HEALTH_REGEN_RATE = 2.0f;
        public static float ATTR_BASE_HEALTH_RECOVER_AFTER_HIT_DELAY = 5.0f;

        public static float ATTR_BASE_MANA_REGEN_RATE = 1.0f;
        public static float ATTR_BASE_MANA_RECOVER_AFTER_USE_DELAY = 3.0f;

        // RootMotion controller
        public static bool ROOT_MOTION_CONTROLLER_ENABLED = true;

        // Fall damge
        public static bool FALLDAMAGE_ENABLED = true;
        public static float FALLDAMAGE_AIRBONE_THRESHOLD = 1.2f; // 1.2f airbone time results in damage
        public static float FALLDAMAGE_MULTIPLIER = 1.2f; 
        public static float FALLDAMAGE_GROWTH_RATE = 2.2f;

        public static bool INVENTORY_PAUSES_GAME = true;
        public static bool TRADE_PAUSES_GAME = true;
        public static bool LOOTING_PAUSES_GAME = true;
        public static bool READING_BOOK_PAUSES_GAME = true;

        // Sneak
        public static bool ENABLE_SNEAK_ATTACKS = true;
        public static float SNEAK_ATTACK_BASE_MULTIPLIER = 2.0f;

        public static int SNEAK_ATTACK_END_AGILITY_VALUE = 100;
        public static float SNEAK_ATTACK_END_MULTIPLIER = 5.0f;
        public static bool SNEAK_ATTACK_CLAMP_FINAL_MULTIPLIER = false;

        // XP related
        public static ulong XP_NEXT_LEVEL_SCALING = 60;
        public static ulong XP_GAINED_ON_AI_KILL_BASE = 22; // XP_GAINED_ON_AI_KILL_BASE * level

        public static bool HELPUI_SHOW_DISMOUNT_CMD = true;

        // Default Class selection
        public static bool ENABLE_CLASS_SELECTION = true;

        // Time Of Day
        public static float TOD_DEFAULT_TIMESCALE = 0.05f;
        public static bool TOD_ENABLE_SKYBOX_AND_LIGHTING_EDITS = true;

        // Stamina jump
        public static bool USE_STAMINA_WHILE_JUMPING = true;
        public static float STAMINA_DRAIN_PER_JUMP = 5.0f;

        // Interactive Objects
        public static bool INTERACTIVE_OBJECT_STOPS_TIME = true;
        public static int GetSaveVersion(string ver)
        {
            // Remove the "." from the version
            string str = ver.Replace(".","");

            int fileVersion = 0;
            if (int.TryParse(str, out fileVersion))
            {
                //Debug.Log("Loading file version: " + fileVersion);
                return fileVersion;
            }
            else
                return 1;
        }

        // Calculates the XP needed to reach the given level
        public static ulong GetNextLevelXPValue(uint level)
        {
            ulong xp = level * level * XP_NEXT_LEVEL_SCALING;
            return xp;
        }

        public static ulong CalculateXPGainedOnAIKill(uint aiLevel)
        {
            ulong xp = aiLevel * XP_GAINED_ON_AI_KILL_BASE;

            return xp;
        }

        public static float GetMaxHealthCalculation(int attConstitution, int attStrength)
        {
            float constitutionModifier = 5.25f;
            float strengthModifier = 0.5f;

            float maxHealth = ATTRIBUTES_DEF_HEALTH + (attConstitution * constitutionModifier) + (attStrength * strengthModifier);
            return maxHealth;
        }

        public static float GetMaxManaCalculation(int attWillpower, int attIntelligence)
        {
            float willpowerModifier = 5.25f;
            float intelligenceModifier = 0.25f;

            float maxMana = ATTRIBUTES_DEF_HEALTH + (attWillpower * willpowerModifier) + (attIntelligence * intelligenceModifier);
            return maxMana;
        }

        public static float GetMaxStaminaCalculation(int attEndurance)
        {
            float enduranceModifier = 5.25f;

            float maxMana = ATTRIBUTES_DEF_HEALTH + (attEndurance * enduranceModifier);
            return maxMana;
        }

        public static float GetJogSpeedCalculation(int attSpeed)
        {
            float speedModifier = 0.04f;
            float jogSpeed = ATTR_BASE_JOG_SPEED + (attSpeed * speedModifier);
            return jogSpeed;
        }

        public static float GetRunSpeedCalculation(int attSpeed)
        {
            float speedModifier = 0.05f;
            float runSpeed = ATTR_BASE_RUN_SPEED + (attSpeed * speedModifier);
            return runSpeed;
        }

        public static float GetCrouchSpeedCalculation(int attSpeed)
        {
            float speedModifier = 0.02f;
            float crouchSpeed = ATTR_BASE_CROUCH_SPEED + (attSpeed * speedModifier);
            return crouchSpeed;
        }


        public static int GetMaxEncumbranceCalculation(int attStrength)
        {
            float strengthModifier = 1.5f;
            int maxEnc = ATTR_BASE_ENCUMBRANCE + (int)(attStrength * strengthModifier);
            return maxEnc;
        }

        public static float GetStrengthDamageAddition(int attStrength, float baseDmg)
        {
            float addition = (baseDmg * ((attStrength/2))) / 100f;
            return addition;
        }

        public static float GetIntelligenceDamageAddition(int attIntelligence, float baseDmg)
        {
            float addition = (baseDmg * (attIntelligence/2)) / 100f;
            return addition;
        }

        public static float GetDexterityDamageAddition(int attDexterity, float baseDmg)
        {
            float addition = (baseDmg * ((attDexterity/2))) / 100f;
            return addition;
        }

        public static float GetFalldamageAgilityReduction(int attAgility, float baseDmg)
        {
            float reduction = (baseDmg * (attAgility / 2f)) / 100f;
            return MathF.Min(reduction, baseDmg); ;
        }

        public static float GetSneakDamageMultiplier(int attAgility)
        {
            float m = (SNEAK_ATTACK_END_MULTIPLIER - SNEAK_ATTACK_BASE_MULTIPLIER) / (SNEAK_ATTACK_END_AGILITY_VALUE - 1);
            float y = SNEAK_ATTACK_BASE_MULTIPLIER + m * (attAgility - 1);

            if(SNEAK_ATTACK_CLAMP_FINAL_MULTIPLIER)
                y = Mathf.Clamp(y, SNEAK_ATTACK_BASE_MULTIPLIER, SNEAK_ATTACK_END_MULTIPLIER);

            return y;
        }

        public static float GetDodgeDistance(int attAgility)
        {
            float dist = (DODGE_DISTANCE_BASE + ((attAgility)) / 25.0f);
            return dist;
        }

        public static float GetStaminaRegenRate(int attEndurance)
        {
            float enduranceModifier = 0.5f;

            float val = ATTR_BASE_STAMINA_REGEN_RATE + (attEndurance * enduranceModifier);
            return val;
        }


        public static float GetStaminaRecoverAfterHitDelay(int attEndurance)
        {
            //float enduranceModifier = 0.15f;
            float val = ATTR_BASE_STAMINA_RECOVER_AFTER_HIT_DELAY;
            return val;
        }

        public static float GetHealthRegenRate(int attConstitution)
        {
            float enduranceModifier = 0.25f;

            float val = ATTR_BASE_HEALTH_REGEN_RATE + (attConstitution * enduranceModifier);
            return val;
        }


        public static float GetHealthRecoverAfterHitDelay(int attConstitution)
        {
            //float enduranceModifier = 0.15f;
            float val = ATTR_BASE_HEALTH_RECOVER_AFTER_HIT_DELAY;
            return val;
        }

        public static float GetManaRegenRate(int attWillpower)
        {
            float willPowerModifier = 0.3f;

            float val = ATTR_BASE_MANA_REGEN_RATE + (attWillpower * willPowerModifier);
            return val;
        }


        public static float GetManaRecoverAfterUseDelay(int attEndurance)
        {
            //float enduranceModifier = 0.15f;
            float val = ATTR_BASE_MANA_RECOVER_AFTER_USE_DELAY;
            return val;
        }
    }
}