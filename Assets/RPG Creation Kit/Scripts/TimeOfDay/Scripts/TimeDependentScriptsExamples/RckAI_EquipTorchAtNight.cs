using RPGCreationKit.AI;
using System.Collections;
using UnityEngine;

namespace RPGCreationKit
{
    public class RckAI_EquipTorchAtNight : MonoBehaviour
    {
        private RckAI ai;
        [SerializeField] private int dayStartHour = 7;
        [SerializeField] private int nightStartHour = 20;

        void Awake()
        {
            ai = GetComponent<RckAI>();
        }

        private void OnEnable()
        {
            StartCoroutine(nameof(SubscribeOnHourChanges));
        }

        private IEnumerator SubscribeOnHourChanges()
        {
            // Wait for world to finish loading
            while(TimeOfDayManager.instance == null)
                yield return null;

            TimeOfDayManager.instance.onHourChanges -= HandleOnHourChange;
            TimeOfDayManager.instance.onHourChanges += HandleOnHourChange;

            while (ai == null || ai.equipment == null || ai.isInOfflineMode || !ai.isLoaded)
                yield return null;

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
            if (ai == null || ai.equipment == null || ai.isInOfflineMode || !ai.isAlive) 
                return;

            bool isTorchEquipped = ai.equipment.currentTorchInHand != null;

            // Equip torch logic
            if (curHour >= dayStartHour && curHour < nightStartHour)
            {
                if (isTorchEquipped)
                    ai.equipment.Unequip(EquipmentSlots.LHand);
            }
            else if (!isTorchEquipped)
            {
                var torch = ai.inventory.GetItem("ITorch001");
                if (torch != null)
                    ai.equipment.Equip(torch);
            }
        }
    }
}
