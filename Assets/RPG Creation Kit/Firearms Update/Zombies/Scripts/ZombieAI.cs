using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.AI;
using RPGCreationKit.Player;
using UnityEditor;

namespace RPGCreationKit.AI
{
    public class ZombieAI : AIBase, IHittable, IDamageable
    {
        public NavMeshAgent agent;              // The agent component of this AI
        public NavMeshPath navmeshPath;         // The NavMeshPath the AI will generate and calculate at runtime while being in UnityNavmesh mode

        public List<Transform> targets;         // Zombies take their mainTarget from this List
        public Transform mainTarget;

        public EntityAttributes attributes;
        public Ragdoll ragdoll;
        public Collider coll;
        public bool isAlive;

        public bool isWalking = true;           // Defines wheter the AI is Walking (true) or running (false)
        public float rotSpeed = 10.0f;
        public float currentSpeed = 0.0f;
        public float stoppingDistance = 1.4f;
        public float stoppingHalt = 0.1f;

        private Vector3 velocity = Vector3.zero;    // Just a storage for the velocity

        public GameObject bloodParticle;

        public WeaponOnHand currentWeaponOnHand;
        public int attackType = 0;
        bool isAttacking = false;

        public AudioClip idleSound;
        public AudioClip attackSound;

        private void Start()
        {
            navmeshPath = new NavMeshPath();

            if (agent.isOnNavMesh)
                agent.CalculatePath(agent.transform.position + (transform.forward * 0.1f), navmeshPath);

            isAlive = true;

            // Add player as target
            targets.Add(RckPlayer.instance.transform);
        }

        private void Update()
        {
            UpdateTargets();
            UpdateAnimator();
            ZombieMovement();
            ZombieSounds();
        }


        // Selects the mainTarget from the targets List in base of distance and status
        void UpdateTargets()
        {
            if (targets.Count > 0)
            {
                float minDist = Mathf.Infinity;

                float curDist = Mathf.Infinity;
                int curIndex = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    curDist = RCKTransform.HorizontalDistance(this.transform.position, targets[i].position);

                    if (curDist < minDist)
                    {
                        minDist = curDist;
                        curIndex = i;
                    }
                }

                mainTarget = targets[curIndex];
            }
            else
                mainTarget = null;
        }

        void ZombieMovement()
        {
            if (!isAlive)
                return;

            if (mainTarget != null)
            {
                agent.stoppingDistance = stoppingDistance;
                agent.speed = currentSpeed;

                bool closestPoint = false;
                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.CalculatePath(mainTarget.position, navmeshPath);

                    if (navmeshPath.status == NavMeshPathStatus.PathInvalid)
                    {
                        NavMeshHit myNavHit;
                        if (NavMesh.SamplePosition(mainTarget.transform.position, out myNavHit, Mathf.Infinity, -1))
                        {
                            //Handles.Label(myNavHit.position, new GUIContent("Closest"));
                            agent.CalculatePath(myNavHit.position, navmeshPath);
                            closestPoint = true;
                        }
                    }
                }

                float distanceAgentToTarget = RCKTransform.HorizontalDistance(agent.transform.position, mainTarget.position);

                // Stopping distance check
                if ((distanceAgentToTarget >= stoppingDistance && !closestPoint)
                    || (distanceAgentToTarget >= stoppingDistance && closestPoint && navmeshPath.corners.Length >= 2))
                {
                    // If there is room to walk
                    if (navmeshPath.corners.Length >= 2)
                    {
                        agent.velocity = Seek(navmeshPath.corners[1]);
                    }
                }
                else
                {
                    // Stop the agent
                    agent.velocity = Vector3.SmoothDamp(agent.velocity, Vector3.zero, ref velocity, stoppingHalt);

                    Vector3 newDirection = (mainTarget.position - transform.position).normalized;
                    newDirection.y = 0;
                    Quaternion lookRotation = Quaternion.LookRotation(newDirection);

                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotSpeed * Time.deltaTime);

                    // Attack
                    if (!isAttacking)
                        MeleeAttack();
                }
            }

        }

        void UpdateAnimator()
        {
            // Update Animator
            float walkModifier = (isWalking) ? .5f : 1f;
            m_Anim.SetFloat("Speed", (agent.velocity.magnitude / currentSpeed) * walkModifier, 1f, Time.deltaTime * 10);
        }

        protected Vector3 Seek(Vector3 targetPos)
        {
            Vector3 desiredVelocity = Vector3.Normalize(targetPos - transform.position) * currentSpeed;
            return (desiredVelocity - agent.desiredVelocity);
        }


        public void Damage(DamageContext damageContext)
        {
            if (!isAlive)
                return;

            attributes.CurHealth -= damageContext.amount;

            if (damageContext.sender != null && damageContext.sender.CompareTag("Player"))
            {
                RckPlayer.instance.OnPlayerHits();
            }

            // Check for essential
            if (attributes.CurHealth <= 0)
            {
                Die();
            }
        }

        public void DamageBlocked(DamageContext damageContext)
        {
            Damage(damageContext);
        }


        public void Die()
        {
            isAlive = false;

            if (ragdoll != null)
                ragdoll.ForceRagdoll();
            else
                m_Anim.SetTrigger("Die");

            agent.enabled = false;
            coll.enabled = false;

            Destroy(this.gameObject, 10f);
        }

        public bool IsHostile()
        {
            return true;
        }

        public bool IsHostileAgainstPC()
        {
            return true;
        }

        public bool ShouldDisplayHealthIfHostile()
        {
            return true;
        }

        public bool IsAlive()
        {
            return isAlive;
        }

        public bool IsUnconscious()
        {
            return false;
        }

        public float GetMaxHP()
        {
            return attributes.MaxHealth;
        }

        public float GetCurrentHP()
        {
            return attributes.CurHealth;
        }

        public string GetEntityName()
        {
            return entityName;
        }

        public string GetEntityID()
        {
            return entityID;
        }

        public Entity GetEntity()
        {
            return this;
        }

        public bool Bleeds()
        {
            return true;
        }

        public void InstantiateBlood(Vector3 pos)
        {
            var blood = Instantiate(bloodParticle, pos, Quaternion.identity, null);
            Destroy(blood, 2.5f);
        }

        public bool IsRckAI()
        {
            return false;
        }
        public void Hit(Vector3 hitDir, float forceAmount = 25, DamageContext damageContext = null)
        {

        }

        public void GenerateDecal()
        {

        }
        public void GenerateBlood()
        {

        }

        public void MeleeAttack()
        {
            isAttacking = true;

            m_Anim.ResetTrigger("Attack");

            attackType = Random.Range(0, currentWeaponOnHand.weaponItem.AttackTypes-1);

            // Set the animation from the AnimatorController, the AnimationsEvents on the 'Swing' animation will do the job
            m_Anim.SetTrigger("Attack");
            m_Anim.SetInteger("AttackType", attackType);

            audioSource.PlayOneShot(attackSound);
        }

        // Anim Event
        public void AttackAnimationEvent()
        {
            currentWeaponOnHand.StartCasting(false, attackType);
        }

        public void EndAttackAnimationEvent()
        {
            currentWeaponOnHand.StopCasting();
        }

        public void ResetAttackAnimationEvent()
        {
            isAttacking = false;
        }

        void ZombieSounds()
        {
            if (!isAlive)
            {
                audioSource.enabled = false;
                return;
            }

            if(!isAttacking && !audioSource.isPlaying)
            {
                audioSource.clip = idleSound;
                audioSource.loop = true;
                audioSource.Play();
            }

            if (!RckPlayer.instance.isAlive)
                audioSource.Stop();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (navmeshPath != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < navmeshPath.corners.Length - 1; i++)
                    Gizmos.DrawLine(navmeshPath.corners[i], navmeshPath.corners[i + 1]);

                Gizmos.color = Color.gray;
                for (int i = 0; i < navmeshPath.corners.Length; i++)
                    Handles.Label(navmeshPath.corners[i], new GUIContent(i.ToString()));
            }
        }
#endif
    }
}