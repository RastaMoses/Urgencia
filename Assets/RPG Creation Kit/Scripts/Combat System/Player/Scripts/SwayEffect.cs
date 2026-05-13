using UnityEngine;
using System.Collections;
using RPGCreationKit;
using RPGCreationKit.Player;
using UnityEngine.InputSystem;

namespace RPGCreationKit
{
    /// <summary>
    /// Sway effect for FPS hands
    /// </summary>
    public class SwayEffect : MonoBehaviour
    {
        public float amount = 0.055f;
        public float maxAmount = 0.09f;
        public float smooth = 3;
        public float damping = 0.1f;
        Vector3 def;
        Vector2 defAth;

        Vector3 velocity = Vector3.zero;

        float x, y;

        public void INPUT_OnLookChanges(Vector2 value)
        {
            Vector2 dValue = value;
            x = dValue.x;
            y = dValue.y;

            x = Mathf.Clamp(x, -1, 1);
            y = Mathf.Clamp(y, -1, 1);
        }

        void Start()
        {
            RckInput.input.actions["Look"].performed += context =>
            {
                INPUT_OnLookChanges(context.ReadValue<Vector2>());
            };

            def = transform.localPosition;
        }

        void FixedUpdate()
        {
            if (!RckPlayer.instance.isInteracting && !RckPlayer.instance.isInConversation &&
                !QuestManagerUI.instance.isUIopened && !InventoryUI.instance.isOpen && !RckPlayer.instance.isLooting && !RckPlayer.instance.isInDeathSequence &&
                !BookReaderManager.instance.backgroundUI.activeInHierarchy)
            {
                float factorX = -x * amount;
                float factorY = -y * amount;

                factorX = Mathf.Clamp(factorX, -maxAmount, maxAmount);
                factorY = Mathf.Clamp(factorY, -maxAmount, maxAmount);

                Vector3 targetPosition = new Vector3(def.x + factorX, def.y + factorY, def.z);

                // Use SmoothDamp for smoother motion
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref velocity, damping);

                // Gradually reset x and y to 0
                x = Mathf.Lerp(x, 0, Time.deltaTime * smooth);
                y = Mathf.Lerp(y, 0, Time.deltaTime * smooth);
            }
        }
    }
}
