using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    public enum ThrowableType
    {
        Explosive = 0,
        Blade = 1
    }

    public class Throwable : MonoBehaviour
    {
        // if true, it wont explode (used for TPS player)
        public bool dummy = false;

        public WeaponItem thisWeaponItem;
        public ThrowableType throwableType;

        [SerializeField] Rigidbody rb;
        [SerializeField] TrailRenderer trail;

        public Entity shooterEntity;
        public bool thrownByPlayer = false;
        public bool thrown = false;
        public bool exploded = false;

        public void Throw(Entity _shooterEntity, Vector3 _direction, bool _thrownByPlayer = false)
        {
            shooterEntity = _shooterEntity;
            thrownByPlayer = _thrownByPlayer;

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                GetComponent<Collider>().enabled = true;
                rb.isKinematic = true;
            }

            gameObject.transform.SetParent(null);
            
            // Move a bit forward
            transform.position = transform.position + shooterEntity.transform.forward;
            
            rb.isKinematic = false;

            thrown = true;

            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            rb.linearVelocity = _direction * (thisWeaponItem.Reach);

            gameObject.SetLayer(0, true);

            if (trail != null)
                trail.enabled = true;

            if (throwableType == ThrowableType.Explosive)
            {
                if(thrownByPlayer && !RCKSettings.TRIGGER_EXPLOSIVE_AS_SOON_AS_HOLD || !thrownByPlayer)
                    Invoke("Explode", thisWeaponItem.explodesAfter);

                InvokeRepeating("SpawnGrenadeAlertEvent", 0.0f, 0.5f);
            }
        }

        public void Explode()
        {
            if (dummy)
                return;

            exploded = true;

            // Check if the player was holding the explosive when it exploded (if TRIGGER_EXPLOSIVE is true in RCKSettings)
            if (RCKSettings.TRIGGER_EXPLOSIVE_AS_SOON_AS_HOLD && thrownByPlayer && PlayerCombat.instance.isThrowingGrenade && PlayerCombat.instance.currentThrowable == this && !PlayerCombat.instance.grenadeThrown)
                PlayerCombat.instance.ThrowThrowable();

            Instantiate(thisWeaponItem.explosionObject, transform.position, Quaternion.identity);

            // Do damage
            Collider[] colliders = Physics.OverlapSphere(transform.position, thisWeaponItem.explosionRadius);

            List<Entity> entityHitted = new List<Entity>();
            foreach (Collider nearbyObject in colliders)
            {
                if (nearbyObject.CompareTag("RPG Creation Kit/BodyPart"))
                {
                    Entity hit = nearbyObject.GetComponentInParent<Entity>();
                    if (!entityHitted.Contains(hit))
                    {
                        // Check if there is an obstacle between the explosion and damageable
                        if (!IsExplosionBlocked(nearbyObject.gameObject))
                        {
                            IDamageable damageable = hit.gameObject.GetComponent<IDamageable>();

                            // Calculate damage in base of distance
                            float distanceToTarget = Vector3.Distance(transform.position, damageable.GetEntity().gameObject.transform.position);
                            float reductionFactor = Mathf.Clamp01(1f - (distanceToTarget / thisWeaponItem.explosionRadius));
                            float reducedDamage = thisWeaponItem.Damage * reductionFactor;

                            // Spawn an AlertEvent if an NPC was killd, could be a one-shot
                            if (damageable.IsAlive() && damageable.GetCurrentHP() - reducedDamage <= 0)
                            {
                                if (damageable.IsRckAI())
                                {
                                    AlertEventData alertEventData = new AlertEventData();
                                    alertEventData.overrideRadius = true;
                                    alertEventData.radius = 30;
                                    alertEventData.killedNPC = damageable.GetEntity().gameObject.GetComponent<RPGCreationKit.AI.RckAI>();

                                    if (!thrownByPlayer)
                                        alertEventData.killerNPC = shooterEntity.GetComponent<RPGCreationKit.AI.RckAI>();

                                    AlertEvent.NewAlertEvent(thrownByPlayer ? AlertEvents.PlayerKillsNPC : AlertEvents.NPCKillsNPC, transform, alertEventData);
                                }
                            }

                            // Deal the reduced damage to the target
                            if (damageable != null)
                                damageable.Damage(new DamageContext(reducedDamage, shooterEntity, null, null, false, null));


                            entityHitted.Add(hit);
                        }

                        Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.AddExplosionForce(thisWeaponItem.explosionForce * RCKSettings.EXPLOSION_FORCE_FOR_AI_MULTIPLIER, transform.position, thisWeaponItem.explosionRadius);
                        }
                    }
                }
                // If an explosive was hit by the explosion, make it explode too
                else if (nearbyObject.CompareTag("RPG Creation Kit/Throwable"))
                {
                    Throwable t = nearbyObject.GetComponent<Throwable>();

                    // Check if there is an obstacle between the explosion and damageable
                    if (!IsExplosionBlocked(nearbyObject.gameObject))
                    {
                        if (t != this && t.thrown && t.throwableType == ThrowableType.Explosive && !t.exploded)
                        {
                            t.Explode();
                        }
                    }
                }
                else
                {
                    // Check if there is an obstacle between the explosion and damageable
                    if (!IsExplosionBlocked(nearbyObject.gameObject))
                    {
                        Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.AddExplosionForce(thisWeaponItem.explosionForce, transform.position, thisWeaponItem.explosionRadius);
                        }
                    }
                }
            }

            Destroy(this.gameObject);
        }

        public bool IsExplosionBlocked(GameObject entity)
        {
            RaycastHit hit;
            Debug.DrawRay(transform.position + (Vector3.up * 1.5f), (entity.transform.position - this.transform.position), Color.cyan, 5.0f);
            if (Physics.Raycast((transform.position + (Vector3.up * 1.5f)), // starting pos is the explosion point a bit higher 
            (entity.transform.position - this.transform.position), out hit, 10000, (1 << 19)))
            {
                return true;
            }
            else
                return false;
        }


        public void SpawnGrenadeAlertEvent()
        {
            AlertEventData alertEventData = new AlertEventData();
            alertEventData.overrideRadius = true;
            alertEventData.radius = 15;

            if (!thrownByPlayer)
                alertEventData.killerNPC = shooterEntity.GetComponent<RPGCreationKit.AI.RckAI>();

            AlertEvent.NewAlertEvent(AlertEvents.GrenadeIncoming, transform, alertEventData);
        }

        public void TriggerExplosive()
        {
            Invoke("Explode", thisWeaponItem.explodesAfter);
        }

        public void OnCollisionEnter(Collision coll)
        {
            if(coll.gameObject.CompareTag("RPG Creation Kit/AI") && thrown)
            {
                RckAI ai = coll.gameObject.GetComponent<RckAI>();

                if (ai != null)
                {
                    ai.EnterInCombatAgainst(shooterEntity);

                    if (thrownByPlayer)
                        RPGCreationKit.Player.RckPlayer.instance.OnPlayerHits();
                }
            }

            if(throwableType == ThrowableType.Blade)
            {
                // Deal throwing knife dmg
            }
        }

#if UNITY_EDITOR
        public virtual void OnDrawGizmos()
        {
            if(throwableType == ThrowableType.Explosive && thrown) 
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, thisWeaponItem.explosionRadius);
            }
        }
#endif
    }
}