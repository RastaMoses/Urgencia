using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    public class CreatureAI : RckAI
    {
        public GameObject assignedModel;
        public bool validAssignedModel = false;
        public GameObject[] toDisableOnDeath;

        public override void Start()
        {
            base.Start();

            // Creatures cannot be pick pocketed
            canBePickPocketed = false;
        }

        public override void OnEnterInCombat()
        {
            base.OnEnterInCombat();
            DrawWeaponEvent();
            canAttack = true;
        }

        public override void Die()
        {
            base.Die();
            for (int i = 0; i < toDisableOnDeath.Length; i++)
                toDisableOnDeath[i].SetActive(false);
        }
    }
}