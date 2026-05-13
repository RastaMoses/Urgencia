using RPGCreationKit;
using RPGCreationKit.AI;
using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGCreationKit
{
    public class WeaponOnHand : MonoBehaviour
    {
        // The entity who's holding the weapon
        public AudioSource audioSource;
        public Entity thisEntity;
        public IMeleeAttacker thisAttacker;

        public LayerMask targetLayerMask;

        // For debug
        [SerializeField] bool debugWeapon = true;
        List<Vector3> spheres = new List<Vector3>();

        // Settings
        public int MAX_COLLISIONS_DETECTABLE = 10;
        int numColliders = 0;
        public Collider[] results;

        [Space(10)]

        public WeaponItem weaponItem;

        bool casting = false;
        bool hasHit = false;
        public int numOfHits = 0;

        public Transform[] hitMarks;

        Vector3 lastPos;

        public float tickness = .1f;

        List<IHittable> hitInThisSwing = new List<IHittable>();
        List<IDamageable> damageableHitInThisSwing = new List<IDamageable>();
        List<EntityAttributes> entitiesHitInThisSwing = new List<EntityAttributes>();

        // Firearms
        public ParticleSystem muzzleflash;
        public Transform muzzlePos;

        public Transform raycastStart;



        private void Start()
        {
            results = new Collider[MAX_COLLISIONS_DETECTABLE];

            // Attempt to take an entity
            if(thisEntity == null)
                thisEntity = GetComponentInParent<Entity>();

            // Take the IMeleeAttacker
            if (thisAttacker == null)
                thisAttacker = GetComponentInParent<IMeleeAttacker>();
        }

        public void StartCasting(bool _charged, int _index)
        {
            charged = _charged;
            index = _index;

            spheres.Clear();
            hitInThisSwing.Clear();
            damageableHitInThisSwing.Clear();
            entitiesHitInThisSwing.Clear();
            numOfHits = 0;
            hasHit = false;
            casting = true;
        }

        public void StopCasting()
        {
            casting = false;
            hasHit = true;
        }

        public void PlayOneShot(AudioClip _clip)
        {
            if(audioSource.enabled && _clip != null)
                audioSource.PlayOneShot(_clip);
        }

        float damageAmount;
        bool charged;
        int index;
        void FixedUpdate()
        {
            if (casting)
            {
                damageAmount = (charged) ? weaponItem.weaponChargedAttacks[index].BaseDamage : weaponItem.weaponAttacks[index].BaseDamage;

                // For every hitmark
                for (int i = 0; i < hitMarks.Length; i++)
                {
                    if (debugWeapon)
                        spheres.Add(hitMarks[i].position); // FOR DEBUG

                    numColliders = Physics.OverlapSphereNonAlloc(hitMarks[i].position, tickness, results, targetLayerMask);

                    // Used to have priority on Blocks on the same entity
                    List<string> entitiesThatBlocked = new List<string>();

                    // Collect all the blocking zone
                    for(int j = 0; j < numColliders; j++)
                    {
                        if (results[j].transform.CompareTag("RPG Creation Kit/BlockingZone"))
                        {
                            IDamageable damageable = results[j].transform.GetComponentInParent<IDamageable>();
                            entitiesThatBlocked.Add(damageable.GetEntityID());
                        }
                    }

                    // For every collision detected on that hitmark
                    for (int j = 0; j < numColliders; j++)
                    {
                        Entity entity = results[j].transform.GetComponent<Entity>();

                        if (entity == null)
                            entity = results[j].transform.GetComponentInParent<Entity>();

                        IHittable hittable = results[j].transform.GetComponent<IHittable>();
                        IDamageable damageable = results[j].transform.GetComponentInParent<IDamageable>();

                        // Check if we hit ourself, if we did, ignore (for AI)
                        if (RCKTransform.IsChildOfRecursive(this.transform.parent, results[j].transform))
                            continue;

                        // Check if we hit ourself, if we did, ignore (for Player)
                        if (this.transform.root.CompareTag("Player") && results[j].transform.parent != null && results[j].transform.parent.CompareTag("Player"))
                            continue;

                        if (results[j].transform.CompareTag("RPG Creation Kit/IgnoreWeapons"))
                            continue;

                        if (results[j].transform.CompareTag("RPG Creation Kit/AI"))
                            continue;

                        if (results[j].transform.CompareTag("RPG Creation Kit/Terrain"))
                            continue;

                        if(entity != null && thisEntity != null)
                            if (entity.GetGuid() == thisEntity.GetGuid())
                                continue;

                        if (hittable != null && !hitInThisSwing.Contains(hittable))
                        {
                            // Check if we hit a blocking area
                            if(results[j].transform.CompareTag("RPG Creation Kit/BlockingZone") && hittable != null)
                            {
                                DamageContext context = new DamageContext(damageAmount, thisEntity, weaponItem, null, true);
                                hittable.Hit(thisEntity.gameObject.transform.forward, RCKSettings.MELEE_ONBODY_RIGIDBODY_FORCE, context);

                                // If an attack is blocked we immediatily stop everything
                                i = hitMarks.Length+1;
                                j = numColliders+1;
                                StopCasting();

                                if (thisAttacker != null)
                                    thisAttacker.OnAttackIsBlocked();
                                else
                                    Debug.Log("ATTACKER NULL!!!");
                                break;
                            }

                            // Check for BodyParts, with a single swing we can hit multiple bodyparts but we don't want to be able to hit the same enemy more than once.
                            if (results[j].transform.CompareTag("RPG Creation Kit/BodyPart"))
                            {
                                BodyPart bodyPart = results[j].GetComponent<BodyPart>();
                                if (!entitiesHitInThisSwing.Contains(bodyPart.entity))
                                {
                                    // Check if this collision was from an entity that blocked the attack, but for some reasons the block was detected after
                                    if (damageable != null && entitiesThatBlocked.Contains(damageable.GetEntityID()))
                                        continue;

                                    entitiesHitInThisSwing.Add(bodyPart.entity);

                                    // Check for damageable, body part should always be damageable
                                    if (damageable != null && !damageableHitInThisSwing.Contains(damageable))
                                    {
                                        damageableHitInThisSwing.Add(damageable);

                                        LocationalDamage locDamage = bodyPart.GetComponentInParent<LocationalDamage>();
                                        if (locDamage != null)
                                            damageAmount = locDamage.CalculateLocationalDamage(bodyPart, damageAmount);

                                        DamageContext context = new DamageContext(damageAmount, thisEntity, weaponItem, null, false);

                                        // Add strength addition
                                        EntityAttributes thisEntityAttributes = thisEntity.GetComponent<EntityAttributes>();
                                        if(thisEntityAttributes)
                                            context.AddToDamage(RCKSettings.GetStrengthDamageAddition(thisEntityAttributes.attributes.Strength, context.amount));

                                        // Sneak attack multiplier
                                        if (RCKSettings.ENABLE_SNEAK_ATTACKS && thisEntity == Player.RckPlayer.instance.GetEntity() &&
                                            RckPlayer.instance.IsCrouching)// todo AND PLAYER IS NOT IN COMBAT
                                        {
                                            RckAI aiHit = damageable.GetEntity().gameObject.GetComponent<RckAI>();

                                            if (aiHit && !aiHit.isInCombat)
                                            {
                                                bool seesPlayer = false;

                                                foreach (VisibleTarget t in aiHit.visibleTargets)
                                                    if (t.tr.gameObject.CompareTag("Player"))
                                                        seesPlayer = true;

                                                if (!seesPlayer)
                                                {
                                                    float damageMultiplier = RCKSettings.GetSneakDamageMultiplier(thisEntityAttributes.attributes.Agility);

                                                    // Give the weapon sneak attack multiplier
                                                    damageMultiplier *= weaponItem.sneakAttackMultiplier;

                                                    context.MultiplyDamage(damageMultiplier);

                                                    if (damageable.IsAlive())
                                                    {
                                                        float truncatedDisplayVal = Mathf.Floor(damageMultiplier * 10f) / 10f;
                                                        AlertMessage.instance.InitAlertMessage("Sneak damage! x" + truncatedDisplayVal, AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                                                    }
                                                }
                                            }
                                        }

                                        // Spawn an AlertEvent if an NPC was killd, could be a one-shot
                                        if (damageable.IsAlive() && damageable.GetCurrentHP() - context.amount <= 0)
                                        {
                                            if (damageable.IsRckAI())
                                            {
                                                AlertEventData alertEventData = new AlertEventData();
                                                alertEventData.overrideRadius = true;
                                                alertEventData.radius = 30;
                                                alertEventData.killedNPC = damageable.GetEntity().gameObject.GetComponent<RPGCreationKit.AI.RckAI>();

                                                bool playerShooting = thisEntity == Player.RckPlayer.instance.GetEntity();

                                                if (!playerShooting)
                                                    alertEventData.killerNPC = thisEntity.GetComponent<RPGCreationKit.AI.RckAI>();

                                                AlertEvent.NewAlertEvent(playerShooting ? AlertEvents.PlayerKillsNPC : AlertEvents.NPCKillsNPC, transform, alertEventData);
                                            }
                                        }

                                        damageable.Damage(context);

                                        if(damageable.Bleeds())
                                            damageable.InstantiateBlood(hitMarks[i].position);
                                    }

                                    hittable.Hit(thisEntity.gameObject.transform.forward, RCKSettings.MELEE_ONBODY_RIGIDBODY_FORCE);
                                    hittable.GenerateDecal();
                                }
                                else continue; // We've hit the same enemy two times, skip this
                            }
                            else // simple hit against an hittable
                            {
                                hitInThisSwing.Add(hittable);

                                // Check for damageable
                                if (damageable != null && !damageableHitInThisSwing.Contains(damageable))
                                {
                                    damageableHitInThisSwing.Add(damageable);


                                    DamageContext context = new DamageContext(damageAmount, thisEntity, weaponItem, null, false);

                                    // Add strength addition
                                    EntityAttributes thisEntityAttributes = thisEntity.GetComponent<EntityAttributes>();
                                    if (thisEntityAttributes)
                                        context.AddToDamage(RCKSettings.GetStrengthDamageAddition(thisEntityAttributes.attributes.Strength, context.amount));

                                    // Sneak attack multiplier
                                    if (RCKSettings.ENABLE_SNEAK_ATTACKS && thisEntity == Player.RckPlayer.instance.GetEntity() &&
                                        RckPlayer.instance.IsCrouching)// todo AND PLAYER IS NOT IN COMBAT
                                    {
                                        RckAI aiHit = damageable.GetEntity().gameObject.GetComponent<RckAI>();

                                        if (aiHit && !aiHit.isInCombat)
                                        {
                                            bool seesPlayer = false;

                                            foreach (VisibleTarget t in aiHit.visibleTargets)
                                                if (t.tr.gameObject.CompareTag("Player"))
                                                    seesPlayer = true;

                                            if (!seesPlayer)
                                            {
                                                float damageMultiplier = RCKSettings.GetSneakDamageMultiplier(thisEntityAttributes.attributes.Agility);

                                                // Give the weapon sneak attack multiplier
                                                damageMultiplier *= weaponItem.sneakAttackMultiplier;

                                                context.MultiplyDamage(damageMultiplier);

                                                float truncatedDisplayVal = Mathf.Floor(damageMultiplier * 10f) / 10f;
                                                AlertMessage.instance.InitAlertMessage("Sneak damage! x" + truncatedDisplayVal, AlertMessage.DEFAULT_MESSAGE_DURATION_MEDIUM);
                                            }
                                        }
                                    }

                                    // Spawn an AlertEvent if an NPC was killd, could be a one-shot
                                    if (damageable.IsAlive() && damageable.GetCurrentHP() - context.amount <= 0)
                                    {
                                        if (damageable.IsRckAI())
                                        {
                                            AlertEventData alertEventData = new AlertEventData();
                                            alertEventData.overrideRadius = true;
                                            alertEventData.radius = 30;
                                            alertEventData.killedNPC = damageable.GetEntity().gameObject.GetComponent<RPGCreationKit.AI.RckAI>();

                                            bool playerShooting = thisEntity == Player.RckPlayer.instance.GetEntity();

                                            if (!playerShooting)
                                                alertEventData.killerNPC = thisEntity.GetComponent<RPGCreationKit.AI.RckAI>();

                                            AlertEvent.NewAlertEvent(playerShooting ? AlertEvents.PlayerKillsNPC : AlertEvents.NPCKillsNPC, transform, alertEventData);
                                        }
                                    }

                                    damageable.Damage(context);

                                    if (damageable.Bleeds())
                                        damageable.InstantiateBlood(hitMarks[i].position);
                                }

                                hittable.Hit(thisEntity.gameObject.transform.forward, RCKSettings.MELEE_GENERAL_RIGIDBODY_FORCE);
                                hittable.GenerateDecal();
                            }

                            numOfHits++; // increments hits

                            // Check if the swing is over because it hit all it could
                            if (numOfHits > (charged ? weaponItem.weaponChargedAttacks[index].howManyHitsCanSustain : weaponItem.weaponAttacks[index].howManyHitsCanSustain))
                            {
                                hasHit = true;
                                StopCasting();
                                break;
                            }
                        }
                    }

                    // Stop the for every hit mark if we're not casting anymore
                    if (!casting)
                        break;
                }
            }
        }

        public void FireRaycast(Entity _shooter, Ray ray)
        {
            bool playerShooting = _shooter == Player.RckPlayer.instance.GetEntity();

            Vector3 from = muzzlePos.transform.position;
            

            RaycastHit firstValidHit = new RaycastHit();
            firstValidHit.distance = Mathf.Infinity;

            RaycastHit[] hits = Physics.RaycastAll(ray, weaponItem.Reach, targetLayerMask);

            // Filter hits
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                // Check if we hit ourself, if we did, ignore (for Player)
                if ((playerShooting && RCKTransform.IsChildOfPlayer(hit.transform)) ||
                    !playerShooting && hit.transform.CompareTag("RPG Creation Kit/BodyPart") && hit.transform.GetComponentInParent<Entity>() == thisEntity)
                {
                    //Debug.Log("SKIPPED SELF SHOT");
                    continue;
                }
                else if(hit.transform.CompareTag("RPG Creation Kit/IgnoreWeapons") ||
                        hit.transform.CompareTag("RPG Creation Kit/AI") ||
                        hit.transform.CompareTag("RPG Creation Kit/BlockingZone") ||
                        hit.transform.CompareTag("RPG Creation Kit/Looting Point"))
                {
                    continue;
                }
                else
                {
                    // Get first valid 
                    if (hit.distance < firstValidHit.distance)
                        firstValidHit = hit;
                }
            }

            // Get first valid hit
            if(firstValidHit.distance != Mathf.Infinity)
            {
                // Now shoot from the muzzle
                Ray beam = new Ray(muzzlePos.position, firstValidHit.point - muzzlePos.position);

                firstValidHit = new RaycastHit();
                firstValidHit.distance = Mathf.Infinity;

                hits = Physics.RaycastAll(beam, weaponItem.Reach, targetLayerMask);

                // Filter hits
                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit hit = hits[i];

                    // Check if we hit ourself, if we did, ignore (for Player)
                    if ((playerShooting && RCKTransform.IsChildOfPlayer(hit.transform)) ||
                    !playerShooting && hit.transform.CompareTag("RPG Creation Kit/BodyPart") && hit.transform.GetComponentInParent<Entity>() == thisEntity)
                    {
                        //Debug.Log("SKIPPED SELF SHOT");
                        continue;
                    }
                    else if (hit.transform.CompareTag("RPG Creation Kit/IgnoreWeapons") ||
                            hit.transform.CompareTag("RPG Creation Kit/AI") ||
                            hit.transform.CompareTag("RPG Creation Kit/BlockingZone") ||
                            hit.transform.CompareTag("RPG Creation Kit/Looting Point"))
                    {
                        continue;
                    }
                    else
                    {
                        // Get first valid 
                        if (hit.distance < firstValidHit.distance)
                            firstValidHit = hit;
                    }
                }

                if (firstValidHit.distance != Mathf.Infinity)
                {
                    //print("Hit " + firstValidHit.transform.name);

                    Entity entity = firstValidHit.transform.GetComponent<Entity>();

                    if (entity == null)
                        entity = firstValidHit.transform.GetComponentInParent<Entity>();

                    IHittable hittable = firstValidHit.transform.GetComponent<IHittable>();
                    IDamageable damageable = firstValidHit.transform.GetComponentInParent<IDamageable>();

                    if (firstValidHit.transform.CompareTag("RPG Creation Kit/BodyPart"))
                    {
                        BodyPart bodyPart = firstValidHit.transform.GetComponent<BodyPart>();

                        damageAmount = weaponItem.Damage;

                        // Check for damageable, body part should always be damageable
                        if (damageable != null)
                        {
                            LocationalDamage locDamage = bodyPart.GetComponentInParent<LocationalDamage>();
                            if (locDamage != null)
                                damageAmount = locDamage.CalculateLocationalDamage(bodyPart, damageAmount);

                            // Spawn an AlertEvent if an NPC was killd, could be a one-shot
                            if (damageable.IsAlive() && damageable.GetCurrentHP() - damageAmount <= 0)
                            {
                                if (damageable.IsRckAI())
                                {
                                    AlertEventData alertEventData = new AlertEventData();
                                    alertEventData.overrideRadius = true;
                                    alertEventData.radius = 30;
                                    alertEventData.killedNPC = damageable.GetEntity().gameObject.GetComponent<RPGCreationKit.AI.RckAI>();

                                    if (!playerShooting)
                                        alertEventData.killerNPC = thisEntity.GetComponent<RPGCreationKit.AI.RckAI>();

                                    AlertEvent.NewAlertEvent(playerShooting ? AlertEvents.PlayerKillsNPC : AlertEvents.NPCKillsNPC, transform, alertEventData);
                                }
                            }

                            DamageContext context = new DamageContext(damageAmount, thisEntity, weaponItem, null, false);
                            damageable.Damage(context);

                            if (damageable.Bleeds())
                                damageable.InstantiateBlood(firstValidHit.point);
                        }

                        hittable.Hit(ray.direction, RCKSettings.FIREARM_ONBODY_RIGIDBODY_FORCE);
                        hittable.GenerateDecal();
                    }
                    // If the player shoots a live explosive, make it explode 
                    else if(firstValidHit.transform.CompareTag("RPG Creation Kit/Throwable"))
                    {
                        Throwable t = firstValidHit.transform.gameObject.GetComponent<Throwable>();

                        // Check if there is an obstacle between the explosion and damageable
                        if (t != this && t.thrown && t.throwableType == ThrowableType.Explosive && !t.exploded)
                            t.Explode();
                    }
                    else // simply hit an object
                    {
                        if (weaponItem.defaultHole != null)
                        {
                            GameObject bulletHole = Instantiate(weaponItem.defaultHole, firstValidHit.point + firstValidHit.normal * 0.0001f, Quaternion.LookRotation(firstValidHit.normal));
                            bulletHole.transform.up = firstValidHit.normal;
                            bulletHole.transform.SetParent(firstValidHit.transform);

                            if(hittable != null)
                            {
                                hittable.Hit(ray.direction, RCKSettings.FIREARM_ONBODY_RIGIDBODY_FORCE);
                            }
                        }
                    }
                }
            }

            // Spawn muzzleflash if set
            if(muzzleflash != null)
            {
                muzzleflash.Play();
                if (playerShooting && RckPlayer.instance.isInThirdPerson)
                    ThirdPersonPlayer.instance.currentWeaponOnHand.muzzleflash.Play();
            }
        }

        /// <summary>
        /// For debug
        /// </summary>
        private void OnDrawGizmos()
        {
            if (hitMarks == null || !debugWeapon)
                return;

            Gizmos.color = Color.cyan;

            for (int i = 0; i < hitMarks.Length; i++)
                if (hitMarks[i] != null)
                    Gizmos.DrawSphere(hitMarks[i].position, tickness);

            for (int i = 0; i < spheres.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(spheres[i], tickness);
            }
        }

        public void DisableCollisionWithOwner()
        {
            if (thisEntity)
            {
                Ragdoll rgdl = thisEntity.GetComponent<Ragdoll>();

                if (!rgdl)
                    rgdl = thisEntity.GetComponentInChildren<Ragdoll>();

                if (rgdl)
                {
                    Collider thisC = GetComponent<Collider>();
                    if (rgdl)
                    {
                        foreach (Collider c in rgdl.colliders)
                        {
                            Physics.IgnoreCollision(c, thisC);
                        }
                    }
                }
            }
        }

    }
}