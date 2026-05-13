using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;
using System.Collections;
using UnityEngine;

namespace RPGCreationKit
{
    public class Virrihael_LockUnlockVampiresDungeon : MonoBehaviour
    {
        private void OnEnable()
        {
            StartCoroutine(nameof(SubscribeOnHourChanges));
        }

        private IEnumerator SubscribeOnHourChanges()
        {
            // Wait for world to finish loading
            while (TimeOfDayManager.instance == null || WorldManager.instance.isLoading)
                yield return null;

            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
            TimeOfDayManager.instance.onHourChanges += HandleOnHourChange;

            HandleOnHourChange(TimeOfDayManager.instance.hours);
        }

        private void OnDisable()
        {
            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
        }

        private void OnDestroy()
        {
            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
        }

        public void HandleOnHourChange(int curHour)
        {
            // Lock logic
            if (QuestManager.instance.GetStage("SQ_KillingMonsters") >= 10)
            {
                if (curHour >= 5 && curHour < 22)
                {
                    RCKFunctions.LockDoor("Virrihael-10_To_VampiresCave", DoorLockLevel.Impossible);
                }
                else
                {
                    RCKFunctions.UnlockDoor("Virrihael-10_To_VampiresCave");
                }
            }
        }
    }
}