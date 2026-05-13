using RPGCreationKit.AI;
using RPGCreationKit.CellsSystem;
using RPGCreationKit.Player;
using System.Collections;
using UnityEngine;

namespace RPGCreationKit
{
    public class CityInterior_LockShopsAtNight : MonoBehaviour
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
            if (curHour >= 7 && curHour < 20 && QuestManager.instance.GetStage("RSQL_TheyShallFall") < 20)
            {
                RCKFunctions.UnlockDoor("CityInteriorToArmorShop");
                RCKFunctions.UnlockDoor("CityInteriorToGeneralGoodsStore");
                RCKFunctions.UnlockDoor("CityInteriorToBlacksmithShop");
            }
            else 
            {
                RCKFunctions.LockDoor("CityInteriorToArmorShop", DoorLockLevel.Impossible);
                RCKFunctions.LockDoor("CityInteriorToGeneralGoodsStore", DoorLockLevel.Impossible);
                RCKFunctions.LockDoor("CityInteriorToBlacksmithShop", DoorLockLevel.Impossible);
            }
        }
    }
}