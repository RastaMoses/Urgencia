using RPGCreationKit.Player;
using UnityEngine;

namespace RPGCreationKit
{
    public class ShieldOnHand : MonoBehaviour
    {
        bool isPlayers = false;
        bool reacted = false;

        private void Start()
        {
            if(GetComponentInParent<RckPlayer>() != null)
                isPlayers = true;
        }

        public void DisableCollisionWithOwner()
        {
            Entity thisEntity = GetComponentInParent<Entity>();

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

        private void OnCollisionEnter(Collision collision)
        {
            if(isPlayers && reacted == false && collision.transform.CompareTag("RPG Creation Kit/Projectile"))
            {
                if (PlayerCombat.instance.isBlocking)
                {
                    PlayerCombat.instance.fpcAnim.SetTrigger("hasBlockedAttack");
                    PlayerCombat.instance.tpsAnim.SetTrigger("hasBlockedAttack");

                    PlayerCombat.instance.fpcAnim.Update(0);
                    PlayerCombat.instance.tpsAnim.Update(0);
                }

                Equipment equipment = Equipment.PlayerEquipment;
                AudioClip blockingSound = (equipment.isUsingShield) ? equipment.currentShield.blockingSound : equipment.currentWeapon.blockSound;

                if (blockingSound != null)
                    PlayerCombat.instance.currentWeaponOnHand.PlayOneShot(blockingSound);

                reacted = true;
            }
        }
    }
}