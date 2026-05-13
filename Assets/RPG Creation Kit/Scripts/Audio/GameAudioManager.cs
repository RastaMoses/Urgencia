using RPGCreationKit;
using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace RPGCreationKit
{
    public enum AudioSources
    {
        GeneralSounds = 0, // This is very near to the camera, used for general sounds (Item pick up, trade sounds, etc.)
        UISounds = 1,
        Player = 2,
        PlayerFPS = 3,
        PlayerFPSExtra = 4
    }

    public class GameAudioManager : MonoBehaviour
    {
        public static GameAudioManager instance;
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Debug.Log("Anomaly detected with the singleton pattern of 'GameAudioManager', do you have multiple instances?");
                Destroy(this);
            }
        }

        public AudioSource player;
        public AudioSource playerFPS;
        public AudioSource playerFPSExtra;

        public AudioSource uiSounds;
        public AudioSource generalSounds;

        // sounds to play when the player is hit
        public AudioClip[] playerHitSoundsMale;
        public AudioClip[] playerHitSoundsFemale;

        public void PlayOneShot(AudioSources source, AudioClip clip)
        {
            switch (source)
            {
                case AudioSources.GeneralSounds:
                    //generalSounds.PlayOneShot(clip);
                    generalSounds.clip = clip;
                    generalSounds.Play();
                    break;

                case AudioSources.UISounds:
                    uiSounds.clip = clip;
                    uiSounds.Play(); break;

                case AudioSources.Player:
                    player.clip = clip;
                    player.Play();
                    break;

                case AudioSources.PlayerFPS:
                    playerFPS.clip = clip;
                    playerFPS.Play(); 
                    break;

                case AudioSources.PlayerFPSExtra:
                    playerFPSExtra.clip = clip;
                    playerFPSExtra.Play();
                    break;
            }
        }

        public void PlayRandomPlayerDamageSound()
        {
            if (Equipment.PlayerEquipment.characterModel.isMale)
            {
                if (playerHitSoundsMale.Length > 0 && !playerFPSExtra.isPlaying)
                {
                    int i = Random.Range(0, playerHitSoundsMale.Length - 1);
                    PlayOneShot(AudioSources.PlayerFPSExtra, playerHitSoundsMale[i]);
                }
            }
            else
            {
                if (playerHitSoundsFemale.Length > 0 && !playerFPSExtra.isPlaying)
                {
                    int i = Random.Range(0, playerHitSoundsFemale.Length - 1);
                    PlayOneShot(AudioSources.PlayerFPSExtra, playerHitSoundsFemale[i]);
                }
            }
        }

    }
}