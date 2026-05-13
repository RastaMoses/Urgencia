using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public static class UINavigationHelper
    {
        /// <summary>
        /// Attempts to navigate up in the UI, if it can, returns true.
        /// </summary>
        /// <returns></returns>
        public static bool CanNavigateUp()
        {
            GameObject curSelected = EventSystem.current.currentSelectedGameObject;

            AxisEventData data = new AxisEventData(EventSystem.current)
            {
                moveDir = MoveDirection.Up,
                selectedObject = EventSystem.current.currentSelectedGameObject
            };
            ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);

            GameObject res = EventSystem.current.currentSelectedGameObject;

            bool ret = false;
            if (res != curSelected)
                ret = true;

            if (ret)
            {
                // Revert
                data = new AxisEventData(EventSystem.current)
                {
                    moveDir = MoveDirection.Down,
                    selectedObject = EventSystem.current.currentSelectedGameObject
                };
                ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);
            }

            return ret;
        }


        /// <summary>
        /// Attempts to navigate down in the UI, if it can, returns true.
        /// </summary>
        /// <returns></returns>
        public static bool CanNavigateDown()
        {
            GameObject curSelected = EventSystem.current.currentSelectedGameObject;

            AxisEventData data = new AxisEventData(EventSystem.current)
            {
                moveDir = MoveDirection.Down,
                selectedObject = EventSystem.current.currentSelectedGameObject
            };
            ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);

            GameObject res = EventSystem.current.currentSelectedGameObject;

            bool ret = false;
            if (res != curSelected)
                ret = true;

            if (ret)
            {
                // Revert
                data = new AxisEventData(EventSystem.current)
                {
                    moveDir = MoveDirection.Up,
                    selectedObject = EventSystem.current.currentSelectedGameObject
                };
                ExecuteEvents.Execute(data.selectedObject, data, ExecuteEvents.moveHandler);
            }

            return ret;
        }
    }
}
