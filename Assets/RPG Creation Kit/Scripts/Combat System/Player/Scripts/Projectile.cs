using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    public class Projectile : MonoBehaviour
    {
        public Entity shooterEntity;
        bool shotByPlayer = false;
        bool shot;
        bool hasCreatedItem = false;


        AmmoItem thisAmmoItem;      // What we've shot
        WeaponItem thisWeaponItem;  // With what weapon?

        [SerializeField] Rigidbody rb;
        [SerializeField] Collider physicalCollider;
        [SerializeField] GameObject projectileInWorld;  // Used to recover a projectile if it didn't hit anything
        [SerializeField] TrailRenderer trail;

        float damageAmount = 0f;

        public void Shoot(WeaponItem _weaponItem, AmmoItem _ammoItem, Entity _shooterEntity, Vector3 _direction, bool _shotByPlayer = false)
        {
            shooterEntity = _shooterEntity;
            shotByPlayer = _shotByPlayer;

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                GetComponent<Collider>().enabled = true;
                rb.isKinematic = true;
            }
            // Offset the arrow forward a little bit
            transform.position = transform.position + transform.forward;

            thisAmmoItem = _ammoItem;
            thisWeaponItem = _weaponItem;

            gameObject.transform.SetParent(null);
            rb.isKinematic = false;

            shot = true;

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            rb.linearVelocity = _direction * (thisWeaponItem.Reach * thisAmmoItem.Speed);

            gameObject.layer = 10;

            if (trail != null)
                trail.enabled = true;

            GetComponent<DestroyAfter>().enabled = true;

            Physics.IgnoreCollision(this.GetComponent<Collider>(), RckPlayer.instance.charController);
        }

        private void Update()
        {
            // Rotates the arrow towards its direction 
            if(shot && rb.linearVelocity.magnitude > RCKSettings.MIN_VELOCITY_TO_CONTINUE_ROTATING_ARROW)
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }

        bool projectileHit = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (projectileHit || !shot)
                return;

            if (trail != null)
                trail.enabled = false;

            IHittable hittable = collision.gameObject.GetComponent<IHittable>();
            IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

            Entity entityHit = shooterEntity;

            if (collision.transform.CompareTag("RPG Creation Kit/EntityShield"))
            {
                StickProjectile(false, collision);
                return;
            }

            /*
            if (entityHit != null && entityHit == shooterEntity)
            {
                Debug.Log("Stopped!!!");
                return;
            }
            */

            projectileHit = true;

            if (hittable != null)
            {
                // Check for body
                // Check for BodyParts, with a single swing we can hit multiple bodyparts but we don't want to be able to hit the same enemy more than once.
                if (collision.transform.CompareTag("RPG Creation Kit/BodyPart"))
                {
                    BodyPart bodyPart = collision.gameObject.GetComponent<BodyPart>();

                    // Check if player shot himself
                    if (shooterEntity == RckPlayer.GetPlayerEntity() && shotByPlayer && damageable == RckPlayer.instance.GetIDamageable())
                        return;

                    // Check for damageable, body part should always be damageable

                    if (damageable != null)
                    {
                        damageAmount = thisWeaponItem.Damage + thisAmmoItem.Damage;

                        LocationalDamage locDamage = bodyPart.GetComponentInParent<LocationalDamage>();
                        if(locDamage != null)
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

                                if (!shotByPlayer)
                                    alertEventData.killerNPC = shooterEntity.GetComponent<RPGCreationKit.AI.RckAI>();

                                AlertEvent.NewAlertEvent(shotByPlayer ? AlertEvents.PlayerKillsNPC : AlertEvents.NPCKillsNPC, transform, alertEventData);
                            }
                        }

                        DamageContext context = new DamageContext(damageAmount, shooterEntity, thisWeaponItem, thisAmmoItem, false);

                        // Add dexterity addition
                        EntityAttributes thisEntityAttributes = shooterEntity.GetComponent<EntityAttributes>();
                        if (thisEntityAttributes)
                            context.AddToDamage(RCKSettings.GetDexterityDamageAddition(thisEntityAttributes.attributes.Dexterity, context.amount));

                        // Sneak attack multiplier
                        if (RCKSettings.ENABLE_SNEAK_ATTACKS && shooterEntity == Player.RckPlayer.instance.GetEntity() &&
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
                                    damageMultiplier *= thisWeaponItem.sneakAttackMultiplier;

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

                                bool playerShooting = shooterEntity == Player.RckPlayer.instance.GetEntity();

                                if (!playerShooting)
                                    alertEventData.killerNPC = shooterEntity.GetComponent<RPGCreationKit.AI.RckAI>();

                                AlertEvent.NewAlertEvent(playerShooting ? AlertEvents.PlayerKillsNPC : AlertEvents.NPCKillsNPC, transform, alertEventData);
                            }
                        }

                        damageable.Damage(context);

                        if (damageable.Bleeds())
                            damageable.InstantiateBlood(transform.position);
                    }

                    hittable.Hit(RckPlayer.instance.mainCamera.transform.forward, RCKSettings.MELEE_ONBODY_RIGIDBODY_FORCE);
                    hittable.GenerateDecal();
                    StickProjectile(false, collision);
                }
                else // simple hit against an hittable
                {
                    // Check for damageable
                    if (damageable != null)
                    {
                        // Check if player shot himself
                        if (shooterEntity == RckPlayer.GetPlayerEntity() && shotByPlayer && damageable == RckPlayer.instance.GetIDamageable())
                            return;

                        damageAmount = thisWeaponItem.Damage + thisAmmoItem.Damage;

                        DamageContext context = new DamageContext(damageAmount, shooterEntity, thisWeaponItem, thisAmmoItem, false);

                        // Add dexterity addition
                        EntityAttributes thisEntityAttributes = shooterEntity.GetComponent<EntityAttributes>();
                        if (thisEntityAttributes)
                            context.AddToDamage(RCKSettings.GetDexterityDamageAddition(thisEntityAttributes.attributes.Dexterity, context.amount));

                        // Sneak attack multiplier
                        if (RCKSettings.ENABLE_SNEAK_ATTACKS && shooterEntity == Player.RckPlayer.instance.GetEntity() &&
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
                                    damageMultiplier *= thisWeaponItem.sneakAttackMultiplier;

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

                                bool playerShooting = shooterEntity == Player.RckPlayer.instance.GetEntity();

                                if (!playerShooting)
                                    alertEventData.killerNPC = shooterEntity.GetComponent<RPGCreationKit.AI.RckAI>();

                                AlertEvent.NewAlertEvent(playerShooting ? AlertEvents.PlayerKillsNPC : AlertEvents.NPCKillsNPC, transform, alertEventData);
                            }
                        }

                        damageable.Damage(context);

                        if (damageable.Bleeds())
                            damageable.InstantiateBlood(transform.position);

                        StickProjectile(false, collision);
                    }
                    else
                    {
                        StickProjectile(true, collision);
                    }
                }

                //hittable.Hit(transform.forward, RCKSettings.ARROW_ONBODY_RIGIDBODY_FORCE);
                hittable.GenerateDecal();
            }

            if (hittable == null && damageable == null && !collision.gameObject.CompareTag("Player"))   // If it hit a random object
                StickProjectile(true, collision);
        }

        public void StickProjectile(bool spawnInWorld, Collision coll)
        {
            if(coll.gameObject.layer == 2)
                Destroy(gameObject);

            // Stop the arrow
            if(!spawnInWorld)
                gameObject.transform.SetParent(coll.transform);

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = true;
            physicalCollider.enabled = false;

            transform.position = coll.contacts[0].point;

            //transform.rotation = Quaternion.FromToRotation(Vector3.up, coll.contacts[0].point);

            // Push it trough objects

            if (spawnInWorld)
            {
                // The arrow hit something and got sticked in it
                Rigidbody pInW = projectileInWorld.GetComponent<Rigidbody>();
                pInW.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                pInW.isKinematic = true;

                Collider[] colliders = projectileInWorld.GetComponents<Collider>();
                foreach (Collider col in colliders)
                    if (!col.isTrigger)
                        col.enabled = false;

                projectileInWorld.transform.SetParent(null);
                projectileInWorld.layer = RCKLayers.Projectile;

                pInW.transform.position = transform.position;
                pInW.transform.rotation = transform.rotation;
                pInW.transform.localScale = transform.localScale;

                ItemInWorld projectileItem = pInW.GetComponent<ItemInWorld>();
                projectileItem.isCreatedItem = true;
                projectileItem.SaveOnFile(false);
                hasCreatedItem = true;

                projectileInWorld.SetActive(true);
                Destroy(gameObject);
            }
        }
    }
}