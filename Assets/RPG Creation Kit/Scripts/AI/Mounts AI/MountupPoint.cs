using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    public class MountupPoint : NPCActionPoint
    {
        // The mount that owns to this point
        public MountAI mount;

        private void Reset()
        {
            actionPointID = "_MOUT_ACTION_POINT_";
            actionType = TypeActionPoint.Mount;
        }

        public override void OnUserAssigned(Entity _entity = null)
        {
            if (!mount.isAlive)
                return;

            base.OnUserAssigned(_entity);

            mount.isOccupied = true;
            mount.playerMounting = false;
            mount.aiMounting = _entity.GetComponent<RckAI>();

            // Has to be unpaused after the NPC Anim is completed, in MountOnMountAITask iN RckAI
            mount.pauseBT3 = true;
        }

        /// <summary>
        /// This is called when the NPC is playing the 'enter' animation to the point. (When the NPC is sitting for example)
        /// </summary>
        public override void OnUserUses(Entity _entity = null)
        {
            base.OnUserUses(_entity);

            RckAI rckAI = _entity.GetComponent<RckAI>();

            if (rckAI != null)
            {
                rckAI.MountOnMountAI(mount, this);
            }
        }

        /// <summary>
        /// This is called when the npc is playing the 'return' animation of the point. (When the NPC is getting up from the chair)
        /// </summary>
        public override void OnUserIsLeaving(Entity _entity = null)
        {
            base.OnUserIsLeaving(_entity);

            // Has to be unpaused after the NPC Anim is completed, in MountOnMountAITask iN RckAI
            mount.pauseBT3 = true;

            RckAI rckAI = _entity.GetComponent<RckAI>();

            if (rckAI != null)
            {
                rckAI.DismountFromMountAI(mount, this);
            }
        }

        /// <summary>
        /// This is called when the NPC that was using this point has completly been released.
        /// </summary>
        public override void OnUserHasLeft(Entity _entity = null)
        {
            base.OnUserHasLeft(_entity);
        }

        /// <summary>
        /// This is called when the NPC that was reaching this point gets reset and it should no more reach it (when it enters in combat)
        /// </summary>
        public override void OnUserForceLeaves()
        {
            base.OnUserForceLeaves();
        }
    }
}
