using RPGCreationKit.AI;
using RPGCreationKit.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RPGCreationKit
{
    public class ZombiesGameMode : MonoBehaviour
    {
        public const float START_TIME = 15.0f;

        public const int FIRST_ROUND_ZOMBIES_N = 7;
        public const float NEXT_ROUND_ZOMBIES_MULT = 1.18f;
        public const float NEXT_ROUND_ZOMBIES_HEALTH_BUFF = 2.2f;
        public const float NEXT_ROUND_ZOMBIES_SPEED_BUFF = 1.2f;
        public const float ZOMBIES_MAX_SPEED = 7f;
        public const int ZOMBIES_START_RUNNING_FROM_ROUND = 3;
        public const float SPAWN_CD_AT_START = 4;
        public const float NEXT_ROUND_WAIT_TIME = 5f;
        public const float NEXT_ROUND_SPAWN_CD_REDUCTION = 0.25f;
        public const float SPAWN_CD_MIN_VAL = 1f;
        public const int MAX_ZOMBIES_AT_TIME = 24;

        public GameObject ZombiePrefab;

        public List<Transform> spawnPoints;
        public List<ZombieAI> activeZombies;

        bool gamemodestarted = false;
        public float curTimer = 0;

        public float curSpawnTimer = 0;
        public float curSpawnCd = 0;

        public int curRound = 0;
        public int curRoundZombiesSpawned = 0;
        public int curRoundZombies = 0;

        public Canvas canvas;
        public TextMeshProUGUI roundCounterUI;
        public TextMeshProUGUI killsCounterUI;
        public TextMeshProUGUI pointsCounterUI;

        public long killCount = 0;
        public long curPoints = 0; 

        bool waitingRoundTime = false;

        private void Start()
        {
            curRoundZombiesSpawned = 0;
            curPoints = 0;
            killCount = 0;
            curRoundZombies = FIRST_ROUND_ZOMBIES_N;
            curSpawnCd = SPAWN_CD_AT_START;

            EntityAttributes.PlayerAttributes.attributes.Constitution = 10;
            EntityAttributes.PlayerAttributes.attributes.Endurance = 10;
            EntityAttributes.PlayerAttributes.derivedAttributes.CalculateFromAttributes(EntityAttributes.PlayerAttributes.attributes, true);
            RckPlayer.instance.UpdateHealthStaminaGUI();
        }

        private void Update()
        {
            curTimer += 1 * Time.deltaTime;
            curSpawnTimer += 1 * Time.deltaTime;

            if (!gamemodestarted)
            {
                if (curTimer >= START_TIME)
                    InitGamemode();
            }
            else
            {
                UpdateActiveZombies();

                if (curRoundZombiesSpawned < curRoundZombies)
                {
                    if (curSpawnTimer >= curSpawnCd && activeZombies.Count < MAX_ZOMBIES_AT_TIME)
                        ZombiesSpawn();
                }
                else
                {
                    //Debug.Log("Waiting for all to die");
                    if (activeZombies.Count == 0)
                    {
                        if (!waitingRoundTime)
                            Invoke("OnRoundBegins", NEXT_ROUND_WAIT_TIME);
                        
                        waitingRoundTime = true;
                    }
                }
            }

            // Check player death
            if(!Player.RckPlayer.instance.isAlive)
            {
                canvas.sortingOrder = 32767;
            }
        }

        void InitGamemode()
        {
            gamemodestarted = true;
            OnRoundBegins();
        }

        void OnRoundBegins()
        {
            waitingRoundTime = false;
            curRound++;
            curRoundZombiesSpawned = 0;

            if (curRound == 1)
                curRoundZombies = FIRST_ROUND_ZOMBIES_N;
            else
                curRoundZombies = Mathf.RoundToInt(curRoundZombies * NEXT_ROUND_ZOMBIES_MULT);


            curSpawnTimer = 0;
            curSpawnCd -= NEXT_ROUND_SPAWN_CD_REDUCTION;
            curSpawnCd = Mathf.Clamp(curSpawnCd, SPAWN_CD_MIN_VAL, SPAWN_CD_AT_START);

            activeZombies.Clear();

            roundCounterUI.text = "<color=\"red\">Round: " + curRound + "</color>";
            roundCounterUI.GetComponent<Animator>().CrossFadeInFixedTime("RoundCounterAnim", 0.0f, 0);
            roundCounterUI.gameObject.SetActive(true);

            killsCounterUI.text = "<color=\"red\">Kills: " + killCount + "</color>";
            killsCounterUI.gameObject.SetActive(true);

            pointsCounterUI.text = "<color=\"red\">Points: " + curPoints + "</color>";
            pointsCounterUI.gameObject.SetActive(true);
        }

        void ZombiesSpawn()
        {
            curSpawnTimer = 0.0f;

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count - 1)];
            ZombieAI zombie = Instantiate(ZombiePrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<ZombieAI>();

            // Set attributes
            zombie.currentSpeed = zombie.currentSpeed + (curRound * NEXT_ROUND_ZOMBIES_SPEED_BUFF);
            zombie.currentSpeed = Mathf.Clamp(zombie.currentSpeed, 0.0f, ZOMBIES_MAX_SPEED);

            zombie.attributes.MaxHealth = zombie.attributes.MaxHealth + (curRound * NEXT_ROUND_ZOMBIES_HEALTH_BUFF);
            zombie.attributes.CurHealth = zombie.attributes.CurHealth + (curRound * NEXT_ROUND_ZOMBIES_HEALTH_BUFF);

            if (curRound >= ZOMBIES_START_RUNNING_FROM_ROUND)
                zombie.isWalking = false;

            activeZombies.Add(zombie);
            curRoundZombiesSpawned++;
        }

        void UpdateActiveZombies()
        {
            for (int i = 0; i < activeZombies.Count; i++)
            {
                if (activeZombies[i] == null || !activeZombies[i].isAlive)
                {
                    activeZombies.RemoveAt(i);
                    killCount++;
                    curPoints += 100;

                    UpdateUI();
                    return;
                }
            }
        }

        public void RemovePoints(int _amount)
        {
            curPoints -= _amount;
            UpdateUI();
        }

        public void UpdateUI()
        {
            killsCounterUI.text = "<color=\"red\">Kills: " + killCount + "</color>";
            pointsCounterUI.text = "<color=\"red\">Points: " + curPoints + "</color>";
        }
    }
}