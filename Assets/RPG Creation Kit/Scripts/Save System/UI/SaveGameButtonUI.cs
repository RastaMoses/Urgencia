using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPGCreationKit.SaveSystem;
using UnityEngine.EventSystems;
using System.IO;
using UnityEngine.UI;

namespace RPGCreationKit.SaveSystem
{
    /// <summary>
    /// Represent a savegame, in the form of a button - inside the Load Game panel in the Main Menu
    /// </summary>
    public class SaveGameButtonUI : Savegame, ISelectHandler, IDeselectHandler
    {
        public int indexInList = -1;
        [HideInInspector] public RckSaveSystemPanel panel;
        public TextMeshProUGUI text;

        Button btn;
        public bool isLoading;

        public void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnGamepadClick_A);
        }


        public void Update()
        {
            /*
            if (RckInput.isUsingGamepad && RckInput.input.currentActionMap.name == "Player")
            {
                if (EventSystem.current.currentSelectedGameObject == this.gameObject)
                {
                    if (RckInput.input.currentActionMap.FindAction("DeleteSavegame").triggered)
                    {
                        LoadPanel panel = GetComponentInParent<LoadPanel>();
                        panel.Delete();
                    }
                }
            }
            */
        }

        public void OnSelect(BaseEventData eventData)
        {
            panel.SelectedSaveFile = this;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            
        }

        void OnGamepadClick_A()
        {
            if (RckInput.isUsingGamepad)
            {
                if (isLoading)
                {
                    LoadPanel panel = GetComponentInParent<LoadPanel>();
                    panel.ConfirmLoadSave();
                }
                else
                {
                    SavePanel panel = GetComponentInParent<SavePanel>();
                    panel.OverwriteButton();
                }
            }
        }
    }
}