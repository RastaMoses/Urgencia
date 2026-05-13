using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using System.Linq;

namespace RPGCreationKit
{
    [CreateAssetMenu(fileName = "New Faction", menuName = "RPG Creation Kit/New Faction", order = 1)]
    [System.Serializable]
    public class Faction : ScriptableObject
    {
        public bool _IS_DEMO_FILE = true;

        public string ID = "FACTIONID";
        public string factionName = "New Faction";
        public bool friendlyFire = false;

        public List<FactionRelationship> alliedFactions = new List<FactionRelationship>();
        public List<FactionRelationship> hostileFactions = new List<FactionRelationship>();

        public static bool AreHostile(Faction f1, Faction f2)
        {
            if (f1.hostileFactions.Any(x => x.faction == f2) ||
                f2.hostileFactions.Any(x => x.faction == f1))
            {
                return true;
            }

            return false;
        }

        public static bool AreHostile(List<Faction> f1, List<Faction> f2)
        {
            bool factionsHostile = false;
            for (int x = 0; x < f1.Count; x++)
            {
                for (int z = 0; z < f2.Count; z++)
                {
                    if (Faction.AreHostile(f1[x], f2[z]))
                    {
                        factionsHostile = true;
                        break;
                    }
                }

                if (factionsHostile)
                    break;
            }

            return factionsHostile;
        }

        public static bool AreInSameFactions(List<Faction> f1, List<Faction> f2)
        {
            return f1.Any(t => f2.Contains(FactionsDatabase.GetFaction(t.ID)));
        }
    }

    [System.Serializable]
    public class FactionRelationship
    {
        public Faction faction;
        public int disposition;
    }
}