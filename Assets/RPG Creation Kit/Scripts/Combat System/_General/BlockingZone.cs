using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.Player;

namespace RPGCreationKit
{
    /// <summary>
    /// Defines a BlockingZone. Each of instance of this script is directly connected to a 'CombatSystem' component.
    /// </summary>
    public class BlockingZone : MonoBehaviour, IHittable
    {
        [SerializeField] bool isPlayer = false;
        public AI.AICombatSystem aiCombat;

        void Start()
        {
            RCKLayers.SetLayer(gameObject, RCKLayers.IgnorePlayer, true);
        }

        public void GenerateDecal()
        {
            
        }

        public void Hit(Vector3 hitDir, float forceAmount = 25, DamageContext damageContext = null)
        {
            if(isPlayer)
            {
                RckPlayer.instance.GetComponent<IDamageable>().DamageBlocked(damageContext);
            }
            else
            {
                aiCombat.DamageBlocked(damageContext);
            }
        }
    }
}