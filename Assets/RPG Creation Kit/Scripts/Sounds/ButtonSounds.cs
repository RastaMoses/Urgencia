using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPGCreationKit;
using UnityEngine.EventSystems;

namespace RPGCreationKit
{
    public class ButtonSounds : MonoBehaviour, IPointerEnterHandler, ISelectHandler, ISubmitHandler
    {
        [SerializeField] private bool inGameButton = false;

        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip onEnter;
        [SerializeField] private AudioClip onClick;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (source != null && onEnter != null)
                source.PlayOneShot(onEnter);
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if(source != null && onClick != null)
                source.PlayOneShot(onClick);
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            if (source != null && onClick != null)
                source.PlayOneShot(onClick);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (source != null && inGameButton)
                source = GameAudioManager.instance.uiSounds;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}