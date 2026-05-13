using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RPGCreationKit;
using UnityEditor.SceneManagement;

namespace RPGCreationKit
{
    public class WeaponItem_Window : ItemWindow
    {
        public bool isReady = false;

        public SerializedObject itemObj = null;

        SerializedProperty itemID;
        SerializedProperty itemName;
        SerializedProperty itemIcon;
        SerializedProperty itemWeight;
        SerializedProperty itemValue;

        SerializedProperty itemQuestItem;
        SerializedProperty isCumulable;

        SerializedProperty WorldModel;
        SerializedProperty WeaponOnHand;
        SerializedProperty WeaponSheathed;

        SerializedProperty weaponType;
        SerializedProperty weightType;
        
        SerializedProperty itemBlockingMultiplier;
        SerializedProperty blockSound;

        SerializedProperty itemReach;
        SerializedProperty itemDamage;
        SerializedProperty itemSpeed;
        SerializedProperty attackTypes;
        SerializedProperty weaponAttacks;
        SerializedProperty chargedAttackTypes;
        SerializedProperty weaponChargedAttacks;

        SerializedProperty attackChainTime;
        SerializedProperty chargedAttackChainTime;

        SerializedProperty aggroModifier;

        SerializedProperty fpsAnimatorController;
        SerializedProperty tpsAnimatorController;

        // Firearms
        SerializedProperty fireType;
        SerializedProperty reloadType;
        SerializedProperty weaponClass;
        SerializedProperty ammo;
        SerializedProperty clipRounds;
        SerializedProperty ammoPerShot;
        SerializedProperty projectilesPerShot;
        SerializedProperty fireRate;
        SerializedProperty aimSpread;
        SerializedProperty hipSpread;
        SerializedProperty sightFov;
        SerializedProperty criticalMultiplier;
        SerializedProperty staggeringChancePerShot;
        SerializedProperty shellCase;
        SerializedProperty recoilHit;

        SerializedProperty swayWhileAiming;


        SerializedProperty fireSound;
        SerializedProperty emptyFireSound;
        SerializedProperty defaultHole;

        SerializedProperty explodesAfter;
        SerializedProperty explosionObject;

        SerializedProperty explosionRadius;
        SerializedProperty explosionForce;

        GameObject gameObject;
        Editor gameObjectEditor;
        bool gameObjectChanged = false;

        Vector2 weaponAttacksScrollView;

        SerializedProperty sneakAttackMultiplier;
        SerializedProperty staminaDrainMultiplier;


        public override void Init(SerializedObject _item)
        {
            // Windows is created from 'Configure' button of the Inspector of the Item

            // Set Title
            Texture icon = AssetDatabase.LoadAssetAtPath<Texture>(EditorIconsPath.WeaponItemWindowIcon);
            GUIContent titleContent = new GUIContent("WeaponItem", icon);
            this.titleContent = titleContent;

            // We copy the Item SerializedObject to not lose reference.
            SerializedObject itemcopy = new SerializedObject(_item.targetObject);
            itemObj = itemcopy;

            itemID = itemObj.FindProperty("ItemID");
            itemName = itemObj.FindProperty("ItemName");
            itemIcon = itemObj.FindProperty("ItemIcon");
            itemWeight = itemObj.FindProperty("Weight");
            itemValue = itemObj.FindProperty("Value");

            itemQuestItem = itemObj.FindProperty("QuestItem");
            isCumulable = itemObj.FindProperty("isCumulable");

            WorldModel = itemObj.FindProperty("itemInWorld");
            WeaponOnHand = itemObj.FindProperty("WeaponOnHand");
            WeaponSheathed = itemObj.FindProperty("WeaponSheathed");

            weightType = itemObj.FindProperty("weightType");
            weaponType = itemObj.FindProperty("weaponType");

            blockSound = itemObj.FindProperty("blockSound");

            itemBlockingMultiplier = itemObj.FindProperty("BlockingMultiplier");
            staminaDrainMultiplier = itemObj.FindProperty("StaminaDrainMultiplierOnBlock");
            itemReach = itemObj.FindProperty("Reach");
            itemSpeed = itemObj.FindProperty("Speed");
            itemDamage = itemObj.FindProperty("Damage");

            fpsAnimatorController = itemObj.FindProperty("fpsAnimatorController");
            tpsAnimatorController = itemObj.FindProperty("tpsAnimatorController");
            attackTypes = itemObj.FindProperty("AttackTypes");
            weaponAttacks = itemObj.FindProperty("weaponAttacks");
            chargedAttackTypes = itemObj.FindProperty("chargedAttackTypes");
            weaponChargedAttacks = itemObj.FindProperty("weaponChargedAttacks");

            attackChainTime = itemObj.FindProperty("attackChainTime");
            chargedAttackChainTime = itemObj.FindProperty("chargedAttackChainTime");

            aggroModifier = itemObj.FindProperty("aggroModifier");

            fireType = itemObj.FindProperty("fireType");
            reloadType = itemObj.FindProperty("reloadType");
            weaponClass = itemObj.FindProperty("weaponClass");
            ammo = itemObj.FindProperty("ammo");
            clipRounds = itemObj.FindProperty("clipRounds");
            ammoPerShot = itemObj.FindProperty("ammoPerShot");
            projectilesPerShot = itemObj.FindProperty("projectilesPerShot");
            fireRate = itemObj.FindProperty("fireRate");
            aimSpread = itemObj.FindProperty("aimSpread");
            hipSpread = itemObj.FindProperty("hipSpread");
            sightFov = itemObj.FindProperty("sightFov");
            criticalMultiplier = itemObj.FindProperty("criticalMultiplier");
            staggeringChancePerShot = itemObj.FindProperty("staggeringChancePerShot");
            shellCase = itemObj.FindProperty("shellCase");
            recoilHit = itemObj.FindProperty("recoilHit");
            swayWhileAiming = itemObj.FindProperty("recoilHit");

            fireSound = itemObj.FindProperty("fireSound");
            emptyFireSound = itemObj.FindProperty("emptyFireSound");
            defaultHole = itemObj.FindProperty("defaultHole");

            explodesAfter = itemObj.FindProperty("explodesAfter");
            explosionObject = itemObj.FindProperty("explosionObject");

            explosionRadius = itemObj.FindProperty("explosionRadius");
            explosionForce = itemObj.FindProperty("explosionForce");

            sneakAttackMultiplier = itemObj.FindProperty("sneakAttackMultiplier");
            
            isReady = true;
            this.Show();
        }

        void OnGUI()
        {
            if (!itemObj.targetObject)
            {
                Debug.LogWarning("ItemObj: NullReferenceException");
                return;
            }

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField("Configuring: " + itemObj.targetObject.name, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            // Draw Sprite
            GUILayout.Space(20);

            // Icon Field
            EditorGUILayout.BeginVertical();

            EditorGUIUtility.labelWidth = 30f;

            EditorGUILayout.LabelField("Type: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(weaponClass, GUIContent.none, GUILayout.ExpandWidth(false));

            EditorGUIUtility.labelWidth = 25;

            EditorGUILayout.LabelField("Icon: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemIcon, GUIContent.none, GUILayout.ExpandWidth(false));

            EditorGUILayout.EndVertical();
            // End Icon field


            Sprite s;

            s = (itemIcon.objectReferenceValue) ? itemIcon.objectReferenceValue as Sprite :
                                                  AssetDatabase.LoadAssetAtPath<Sprite>(EditorIconsPath.NoIcon);

            EditorGUI.DrawTextureTransparent(new Rect(110, 90, 100, 100), s.texture);
            // End Draw Sprite

            GUILayout.Space(85);

            EditorGUILayout.BeginVertical();

            // Model Field
            EditorGUIUtility.labelWidth = 50f;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("World Model:", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("WeaponOnHand:", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("WeaponSheathed:", GUILayout.ExpandWidth(false));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

           

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(WorldModel, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(WeaponOnHand, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(WeaponSheathed, GUIContent.none, GUILayout.ExpandWidth(false));

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                // Update 
                WorldModel = itemObj.FindProperty("itemInWorld");
                gameObjectChanged = true;
            }
            // End Model field


            // Draw Model Preview
            gameObject = (GameObject)WorldModel.objectReferenceValue;

            GUIStyle bgColor = new GUIStyle();
            bgColor.normal.background = Texture2D.blackTexture;

            if (gameObject != null)
            {
                if (gameObjectEditor == null || gameObjectChanged)
                {
                    gameObjectEditor = Editor.CreateEditor(gameObject);
                    gameObjectChanged = false;
                }

                gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(64, 128), bgColor);
            }
            else
            {
                GUILayout.Space(128);
            }

            // End model Preview

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(35);

            if (weaponClass.enumValueIndex == ((int)WeaponClass.Melee))
                DrawMeleeWeaponContent();
            else if (weaponClass.enumValueIndex == ((int)WeaponClass.Firearm))
                DrawFirearmWeaponContent();
            else if (weaponClass.enumValueIndex == ((int)WeaponClass.Throwable))
                DrawThrowableWeaponContent();

            EditorGUILayout.Space();

            EditorGUIUtility.labelWidth = 80f;

            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(itemQuestItem);
            EditorGUILayout.PropertyField(isCumulable);
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Configure Sounds"))
            {
                if (soundWinOpened)
                {
                    for (int i = 0; i < childWindows.Count; i++)
                        if (childWindows[i].GetType() == typeof(ItemSoundsWindow))
                        {
                            childWindows[i].Focus();
                            childWindows[i].position = new Rect(this.position.center.x,
                                                                this.position.center.y, this.position.xMax, this.position.yMax);
                        }

                    return;
                }

                //Check if the window wasn't already opened
                ItemSoundsWindow myWindow = CreateInstance<ItemSoundsWindow>();
                myWindow.minSize = new Vector2(400, 260);
                myWindow.maxSize = new Vector2(400, 260);

                myWindow.Init(itemObj, this);

                childWindows.Add(myWindow);

                soundWinOpened = true;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("OK", "Save changes and close the Window")))
            {
                itemObj.ApplyModifiedProperties();
                this.Close();

                Selection.objects = new Object[0];
            }

            if (GUILayout.Button(new GUIContent("Cancel", "Cancel changes and close the Window")))
            {
                this.Close();

                Selection.objects = new Object[0];
            }

            GUILayout.EndHorizontal();

        }

        public void DrawMeleeWeaponContent()
        {
            EditorGUIUtility.labelWidth = 55f;

            // Vertical of properties
            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Weapon:", EditorStyles.boldLabel);

            // ID
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemID, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Name", "In-Game Name of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemName, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Weight
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weight", "The weight value of the Item in the Inventory"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemWeight, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Value
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Value", "Value (in Golds) of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemValue, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Weight Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weight Type", "The Weight Type of the Weapon"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(weightType, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Aggro modifier
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Aggro Modifier", "The amount of aggro this weapon has"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(aggroModifier, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Sneak Attack
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Sneak Multiplier", "The multiplier that will be applied to the Sneak Attack multiplier"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(sneakAttackMultiplier, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();


            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Settings:", EditorStyles.boldLabel);

            // Weapon Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weapon Type", "The type of the Weapon"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(weaponType, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Blocking Multiplier
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Blocking Multiplier", "How much the block of an incoming attack will absorb the damage."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemBlockingMultiplier, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Stamina Drain Multiplier", "Stamina Drain Multiplier, how much blocking the damage with this shield will reduce the stamina drain. (Value should be less for shields, higher for weapons)."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(staminaDrainMultiplier, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Blocking Sound", "The sound that will be played when an attack will be blocked."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(blockSound, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Damage
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Damage", "The damage value of the Weapon."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemDamage, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("FPS Animator", "Define the Camera Animator for this weapon."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(fpsAnimatorController, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("TPS Animator", "Define the Camera Animator for this weapon."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(tpsAnimatorController, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Speed
            //EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.LabelField(new GUIContent("Speed", "The speed of the attacks (Animator)"), GUILayout.ExpandWidth(false));
            //EditorGUILayout.PropertyField(itemSpeed, GUIContent.none, GUILayout.ExpandWidth(false));
            //EditorGUILayout.EndHorizontal();

            // Reach
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Reach", "The distance to hit the target"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemReach, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Attacks:", EditorStyles.boldLabel);

            EditorGUIUtility.labelWidth = 180f;


            EditorGUILayout.BeginHorizontal();

            // Attacks Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Attack Types", "How many attacks this weapon has in the Animator."), GUILayout.ExpandWidth(false));
            EditorGUILayout.DelayedIntField(attackTypes, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Attacks Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Charged Attack Types", "How many charged attacks this weapon has in the Animator."), GUILayout.ExpandWidth(false));
            EditorGUILayout.DelayedIntField(chargedAttackTypes, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            // Attacks Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Attack Chain Time", "Max amount of time that has to pass before not being able to chain an attack."), GUILayout.ExpandWidth(false));
            EditorGUILayout.DelayedFloatField(attackChainTime, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Attacks Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Charged Attack Chain Time", "Max amount of time that has to pass before not being able to chain a charged attack."), GUILayout.ExpandWidth(false));
            EditorGUILayout.DelayedFloatField(attackChainTime, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();




            EditorGUIUtility.labelWidth = 180f;

            EditorGUILayout.BeginHorizontal();




            if (weaponAttacks.arraySize != attackTypes.intValue)
                weaponAttacks.arraySize = attackTypes.intValue;

            if (weaponChargedAttacks.arraySize != chargedAttackTypes.intValue)
                weaponChargedAttacks.arraySize = chargedAttackTypes.intValue;

            //weaponAttacks.isExpanded = true;

            weaponAttacksScrollView =
                EditorGUILayout.BeginScrollView(weaponAttacksScrollView, GUILayout.Width(position.width - 15), GUILayout.Height(100));


            EditorGUILayout.PropertyField(weaponAttacks, true);
            EditorGUILayout.PropertyField(weaponChargedAttacks, true);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();
        }

        public void DrawFirearmWeaponContent()
        {
            EditorGUIUtility.labelWidth = 55f;

            // Vertical of properties
            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Weapon:", EditorStyles.boldLabel);

            // ID
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemID, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Name", "In-Game Name of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemName, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Weight
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weight", "The weight value of the Item in the Inventory"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemWeight, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Value
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Value", "Value (in Golds) of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemValue, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Weight Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weight Type", "The Weight Type of the Weapon"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(weightType, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Aggro modifier
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Aggro Modifier", "The amount of aggro this weapon has"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(aggroModifier, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Sight FOV", "FOV when looking in the iron sight"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(sightFov, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Critical Multiplier", "Multiplier when hitting critical shots"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(criticalMultiplier, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Aim Spread", "Base spread of the weapon (when iron sight"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(aimSpread, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Hip Spread", "Spread of the weapon when firing on the hip"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(hipSpread, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Recoil Hit", "How much the weapon raises per shot"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(recoilHit, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // End vertical of properties
            EditorGUILayout.EndVertical();


            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Settings:", EditorStyles.boldLabel);

            // Weapon Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weapon Type", "The type of the Weapon"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(weaponType, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Fire Type", "The fire type of the Weapon"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(fireType, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Reload Type", "The reload type of the Weapon"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(reloadType, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("FPS Animator", "Define the Camera Animator for this weapon."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(fpsAnimatorController, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("TPS Animator", "Define the Camera Animator for this weapon."), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(tpsAnimatorController, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Reach
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Reach", "The distance to hit the target"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemReach, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Ammo", "The ammo this weapon uses"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(ammo, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Base Damage", "Damage"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemDamage, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Clip Rounds", "Magazine capacity"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(clipRounds, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Ammo per shot", "How much ammo is consumed per shot"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(ammoPerShot, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Fire rate", "Fire rate in ms"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(fireRate, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Projectiles Per Shot", "How many raycasts (weapon) or projectiles (launcher) are fired per each shot"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(projectilesPerShot, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Staggering Chance", "Staggering Chance (per shot)"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(staggeringChancePerShot, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();

            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Other:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Sway while aiming", "Sway while aiming (when moving the mouse around)"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(swayWhileAiming, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Fire Sound", ")"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(fireSound, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Empty Fire Sound", ""), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(emptyFireSound, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Default Bullet Hole", ""), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(defaultHole, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // End vertical of properties
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();


            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));

            EditorGUIUtility.labelWidth = 180f;


            // End vertical of properties
            EditorGUILayout.EndVertical();
        }

        public void DrawThrowableWeaponContent()
        {
            EditorGUIUtility.labelWidth = 55f;

            // Vertical of properties
            EditorGUILayout.BeginHorizontal();


            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Weapon:", EditorStyles.boldLabel);

            // ID
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID: ", GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemID, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Name", "In-Game Name of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemName, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Weight
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weight", "The weight value of the Item in the Inventory"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemWeight, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();


            // Value
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Value", "Value (in Golds) of the Item"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemValue, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Weight Type
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Weight Type", "The Weight Type of the Weapon"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(weightType, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // Aggro modifier
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Aggro Modifier", "The amount of aggro this weapon has"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(aggroModifier, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Critical Multiplier", "Multiplier when hitting critical shots"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(criticalMultiplier, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();


            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Settings:", EditorStyles.boldLabel);

            // Reach
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Reach", "The distance to hit the target"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemReach, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Explodes After", "The time in seconds before this throwable explodes"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(explodesAfter, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Explosion Object", "The explosion object"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(explosionObject, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Explosion Radius", "Radius of the collision"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(explosionRadius, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Explosion Force", "Force of explosion (knockoff)"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(explosionForce, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Base Damage", "Damage"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(itemDamage, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Staggering Chance", "Staggering Chance (per shot)"), GUILayout.ExpandWidth(false));
            EditorGUILayout.PropertyField(staggeringChancePerShot, GUIContent.none, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            // End vertical of properties
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();


            // RIGHT VALUES
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(false));

            EditorGUIUtility.labelWidth = 180f;


            // End vertical of properties
            EditorGUILayout.EndVertical();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < childWindows.Count; i++)
                childWindows[i].Close();
        }

    }
}