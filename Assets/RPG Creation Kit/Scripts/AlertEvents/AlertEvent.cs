using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit.AI;

namespace RPGCreationKit
{
    public enum AlertEvents
    {
        PlayerKillsNPC,
        NPCKillsNPC,
        GrenadeIncoming,
        NPCEntersInCombat // called when an NPC enters in combat and says aggro line, so it's supposed to make noise and alert NPCs near him (like in the tutorial cave)
    }

    // Some data tha can be used by an AlertEvent
    public struct AlertEventData
    {
        public bool overrideRadius;
        public float radius;

        public RckAI killerNPC; // "default" NPC reference
        public RckAI killedNPC;
    }

    /// <summary>
    /// AlertEvents allows to propagate events to nearby entities, such as one-shot kills or incoming grenades.
    /// </summary>
    public class AlertEvent : MonoBehaviour
    {
        public AlertEvents thisEvent;
        public float propagationRadius = 10.0f;

        AlertEventData data;

        public static GameObject NewAlertEvent(AlertEvents eventType, Transform t, AlertEventData data)
        {
            GameObject go = new GameObject("AlertEvent");
            go.transform.position = t.position;

            go.AddComponent<AlertEvent>();
            AlertEvent goEvent = go.GetComponent<AlertEvent>();

            goEvent.thisEvent = eventType;
            goEvent.DoAlert(data);
            return go;
        }

        public void DoAlert(AlertEventData _data)
        {
            data = _data;

            if (data.overrideRadius)
                propagationRadius = data.radius;

            Collider[] colliders = Physics.OverlapSphere(transform.position, propagationRadius);
            foreach (Collider nearbyObject in colliders)
            {
                switch(thisEvent)
                {
                    case AlertEvents.PlayerKillsNPC:
                        PlayerKillsNPCEvent(nearbyObject);
                        break;

                    case AlertEvents.NPCKillsNPC:
                        NPCKillsNPCEvent(nearbyObject);
                        break;

                    case AlertEvents.GrenadeIncoming:
                        GrenadeIncoming(nearbyObject);
                        break;

                    case AlertEvents.NPCEntersInCombat:
                        NPCEntersCombatEvent(nearbyObject);
                        break;

                    default:
                        break;
                }
            }

            Destroy(this.gameObject, 1.0f);
        }

        public void PlayerKillsNPCEvent(Collider nearbyObject)
        {
            // Check if there is another NPC nearby
            if(nearbyObject.gameObject.CompareTag("RPG Creation Kit/AI"))
            {
                RckAI nearbyAI = nearbyObject.gameObject.GetComponent<RckAI>();

                // Check if they were in the same faction
                if(Faction.AreInSameFactions(data.killedNPC.belongsToFactions, nearbyAI.belongsToFactions))
                {
                    // Check if nearbyAI can see the player
                    if(nearbyAI.ignoreVisionWhenAlertedKillNPC || RCKFunctions.CheckAICanSeePlayer(nearbyAI))
                    {
                        // Attack player
                        nearbyAI.EnterInCombatAgainst(Player.RckPlayer.GetPlayerEntity());
                    }
                }
            }
        }

        public void NPCEntersCombatEvent(Collider nearbyObject)
        {
            // Check if there is another NPC nearby
            if (nearbyObject.gameObject.CompareTag("RPG Creation Kit/AI"))
            {
                RckAI nearbyAI = nearbyObject.gameObject.GetComponent<RckAI>();

                // Check if they were in the same faction
                if (Faction.AreInSameFactions(data.killerNPC.belongsToFactions, nearbyAI.belongsToFactions))
                {
                    // Check if nearbyAI can see the player
                    if (nearbyAI.ignoreVisionWhenAlertedAggroNPC)
                    {
                        for (int i = 0; i < data.killerNPC.enemyTargets.Count; i++)
                            nearbyAI.EnterInCombatAgainst(data.killerNPC.enemyTargets[i].m_entity);
                    }
                }
            }
        }

        public void NPCKillsNPCEvent(Collider nearbyObject)
        {
            // Check if there is another NPC nearby
            if (nearbyObject.gameObject.CompareTag("RPG Creation Kit/AI"))
            {
                RckAI nearbyAI = nearbyObject.gameObject.GetComponent<RckAI>();

                // Check if they were in the same faction
                if (Faction.AreInSameFactions(data.killedNPC.belongsToFactions, nearbyAI.belongsToFactions))
                {
                    // Check if nearbyAI can see the other NPC
                    if (nearbyAI.visibleAI.Contains(data.killerNPC))
                    {
                        // Attack player
                        nearbyAI.EnterInCombatAgainst(data.killerNPC);
                    }
                }
            }
        }

        public void GrenadeIncoming(Collider nearbyObject)
        {
            // Check if there is another NPC nearby
            if (nearbyObject.gameObject.CompareTag("RPG Creation Kit/AI"))
            {
                RckAI nearbyAI = nearbyObject.gameObject.GetComponent<RckAI>();

                if (nearbyAI != null)
                {
                    if (nearbyAI.isInCover)
                        nearbyAI.LeaveCurrentCover();

                    nearbyAI.SetNearbyExplosive(this.gameObject);
                }
            }
        }

#if UNITY_EDITOR
        public virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, propagationRadius);
        }
#endif
    }
}