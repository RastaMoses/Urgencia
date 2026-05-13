using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    public class SpellProjectile : MonoBehaviour
    {
        [HideInInspector]
        public Entity shooterEntity;
        bool shotByPlayer = false;
        public bool shot = false;
        bool hasCreatedItem = false;
        bool projectileHit = false;

        public Spell thisSpell;

        // Used when this spell projectile collides with something/someone
        public GameObject explosionEffect;

        [SerializeField] Rigidbody rb;
        [SerializeField] Collider physicalCollider;

        float damageAmount = 0f;
        Vector3 direction;

        /// <summary>
        /// General init function, it is overriden by Shoot() but needed for spells like Restoration as they never get Shoot called
        /// </summary>
        /// <param name="_shooterEntity"></param>
        /// <param name="_shotByPlayer"></param>
        public void Init(Entity _shooterEntity, bool _shotByPlayer = false)
        {
            shooterEntity = _shooterEntity;
            shotByPlayer = _shotByPlayer;
            rb.isKinematic = true;
            physicalCollider.enabled = false;

            // Ignore collision with the shooter
            if (shotByPlayer)
            {
                Physics.IgnoreCollision(physicalCollider, RckPlayer.instance.charController, true);

                // Also ignore collision with mount if the player is mounted
                if (RckPlayer.instance.isMounted)
                {
                    Physics.IgnoreCollision(physicalCollider, RckPlayer.instance.aiMounting.physicalCollider, true);
                }
            }
            else if (shooterEntity.GetComponent<Collider>() != null)
                Physics.IgnoreCollision(physicalCollider, shooterEntity.GetComponent<Collider>(), true);
        }

        /// <summary>
        /// Used by Spell Types: Projectile to shoot and travel in a direction
        /// </summary>
        /// <param name="_shooterEntity"></param>
        /// <param name="_direction"></param>
        /// <param name="_shotByPlayer"></param>
        public void Shoot(Entity _shooterEntity, Vector3 _direction, bool _shotByPlayer = false)
        {
            shooterEntity = _shooterEntity;
            shotByPlayer = _shotByPlayer;
            direction = _direction;

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                GetComponent<Collider>().enabled = true;
                rb.isKinematic = true;
            }
            else
                physicalCollider.enabled = true;

            gameObject.SetLayer(RCKLayers.Default, true);

            rb.isKinematic = false;
            gameObject.transform.SetParent(null);

            shot = true;
        }

        private void Update()
        {
            if(shot && !projectileHit)
            {
                // Move the projectile
                rb.position += direction * thisSpell.speed * Time.deltaTime;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (projectileHit || !shot)
                return;

            IHittable hittable = collision.gameObject.GetComponent<IHittable>();
            IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();

            Entity entityHit = null;

            if(damageable != null)
                entityHit = damageable.GetEntity();

            if (entityHit != null && entityHit == shooterEntity)
            {
                // AI hit itself, return
                return;
            }

            if (collision.transform.CompareTag("RPG Creation Kit/EntityShield"))
            {
                StickProjectile(collision);
                return;
            }


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
                        damageAmount = thisSpell.magnitude;

                        LocationalDamage locDamage = bodyPart.GetComponentInParent<LocationalDamage>();
                        if (locDamage != null)
                            damageAmount = locDamage.CalculateLocationalDamage(bodyPart, damageAmount);

                        DamageContext context = new DamageContext(damageAmount, shooterEntity, null, null, false, thisSpell);

                        // Add intelligence addition
                        EntityAttributes thisEntityAttributes = shooterEntity.GetComponent<EntityAttributes>();
                        if (thisEntityAttributes)
                            context.AddToDamage(RCKSettings.GetIntelligenceDamageAddition(thisEntityAttributes.attributes.Intelligence, context.amount));

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

                        /*
                        if (damageable.Bleeds())
                            damageable.InstantiateBlood(transform.position);
                        */

                        EntityAttributes attributes = bodyPart.GetComponentInParent<EntityAttributes>();
                        if (attributes != null && thisSpell.onTargetEffects.Length > 0)
                            attributes.ExecuteEffects(thisSpell.onTargetEffects);

                    }

                    hittable.Hit(RckPlayer.instance.mainCamera.transform.forward, RCKSettings.MELEE_ONBODY_RIGIDBODY_FORCE);
                    hittable.GenerateDecal();
                    StickProjectile(collision);
                }
                else // simple hit against an hittable
                {
                    // Check for damageable
                    if (damageable != null)
                    {
                        // Check if player shot himself
                        if (shooterEntity == RckPlayer.GetPlayerEntity() && shotByPlayer && damageable == RckPlayer.instance.GetIDamageable())
                            return;

                        // Check if projectile is hitting NPC's physical coll
                        if (entityHit.CompareTag("RPG Creation Kit/AI"))
                            return;

                        damageAmount = thisSpell.magnitude;

                        DamageContext context = new DamageContext(damageAmount, shooterEntity, null, null, false, thisSpell);

                        // Add intelligence addition
                        EntityAttributes thisEntityAttributes = shooterEntity.GetComponent<EntityAttributes>();
                        if (thisEntityAttributes)
                            context.AddToDamage(RCKSettings.GetIntelligenceDamageAddition(thisEntityAttributes.attributes.Intelligence, context.amount));

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

                        /*
                        if (damageable.Bleeds())
                            damageable.InstantiateBlood(transform.position);
                        */

                        StickProjectile(collision);
                    }
                    else
                    {
                        StickProjectile(collision);
                    }
                }

                hittable.Hit(transform.forward, RCKSettings.ARROW_ONBODY_RIGIDBODY_FORCE);
                hittable.GenerateDecal();
            }

            if (hittable == null && damageable == null && !collision.gameObject.CompareTag("Player"))   // If it hit a random object
                StickProjectile(collision);
        }

        public void StickProjectile(Collision coll)
        {
            projectileHit = true;

            // Stop the projectile
            gameObject.transform.SetParent(coll.transform);

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = true;
            physicalCollider.enabled = false;

            //transform.rotation = Quaternion.FromToRotation(Vector3.up, coll.contacts[0].point);

            // Push it trough objects

            Explode();
        }

        public void Explode()
        {
            if (explosionEffect != null)
            {
                // Deatach from this object
                explosionEffect.transform.parent = transform.parent;

                explosionEffect.SetActive(true);

                // Destroy the projectile
                Destroy(gameObject);
            }
        }

        public void ExecuteOnCasterEffects()
        {
            EntityAttributes attr = null;

            if (shotByPlayer)
                attr = EntityAttributes.PlayerAttributes;
            else if(shooterEntity != null)
                attr = shooterEntity.GetComponent<EntityAttributes>();

            if (attr != null)
            {
                attr.ExecuteEffects(thisSpell.onCasterEffects);
            }
        }


        public void CastToTouch()
        {
            shot = true;
        }
    }
}