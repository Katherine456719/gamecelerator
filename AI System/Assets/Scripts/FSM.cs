﻿using UnityEngine;
using UnityEngine.AI;

public enum State
{
    patrol,
    change_color,
    attack,
}

public class FSM : MonoBehaviour
{

    private EnemyController enemyController;
    [SerializeField] public InputCommandObject inputCommandObject; 
    [SerializeField] private State initialState;
    private State curentState;

    [SerializeField] private Transform [] positions;
    [SerializeField] private GameObject objectWithChildren;
    [SerializeField] bool generateRandom = true;
    [SerializeField] bool smoothRandom = false;
    [SerializeField] private int lowerRange = 4, upperRange = 10;// pentru a spune cate pozitii random generez
    [Range(1, 100)]
    [SerializeField] int spawningRandomness = 20;
    [SerializeField] float unitRandom = 5f;

    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float distanceAttack = 35;
    [SerializeField] public GameObject targetPlayer;

    private int index = 0;
    private int numberPositions;
    private Vector3 agentPosition,destinationPosition;
    private GameObject bot;


    private void Start()
    {

        // iau parintele -> Enemy -> Enemy Controller
        GameObject parinte = transform.parent.gameObject;
        enemyController = parinte.GetComponent<EnemyController>();

        // iau playerul din scena,pozitia obiectului care se misca efectiv(Bot) si scriptableObject
        targetPlayer = enemyController.player;
        inputCommandObject = enemyController.inputCommandObject;
        bot = enemyController.bot;

        //initalizez curentstate cu initial State
        curentState = initialState;


        if (!generateRandom)
        {
            // daca vectorul de pozitii nu este modificat din editor si este pus un GameObject cu copii
            // atunci consider pozitiile de patrol ca fiind pozitiile copiilor
            if (objectWithChildren != null && positions.Length == 0)
            {
                int i = 0;
                positions = new Transform[objectWithChildren.transform.childCount];
                foreach (Transform transform in objectWithChildren.transform)
                {
                    positions[i++] = transform;
                }
            }
            // cate pozitii am = lungime vector
            numberPositions = positions.Length;
        }
        else
        {

            
            // generez aleator un numar de n pozitii si le pun in vectorul randomPositions
            numberPositions = Random.Range(lowerRange, upperRange);
            positions = new Transform[numberPositions];
            Vector3 randomPosition = Vector3.zero;
            for (int i = 0; i < numberPositions; i++)
            {
                float randomX, randomZ;
                if (smoothRandom)
                {
                    randomX = Mathf.PerlinNoise(0, 1) * unitRandom * spawningRandomness;
                    randomZ = Mathf.PerlinNoise(0, 1) * unitRandom * spawningRandomness;
                }
                else
                {
                    randomX = Random.Range(1, spawningRandomness) * unitRandom;
                    randomZ = Random.Range(1, spawningRandomness) * unitRandom;
                }
                randomPosition.x += randomX;
                randomPosition.z += randomZ;

                Debug.Log(randomPosition);
                GameObject newGameObject = new GameObject();
                newGameObject.transform.SetParent(transform);
                newGameObject.transform.position = randomPosition;
                positions[i] = newGameObject.transform;
                print(randomPosition);
            }
        }



    }
    void CheckDistanceToPlayer()
    {
        float curentdist = Vector3.Distance(targetPlayer.transform.position, bot.transform.position);
        if (curentdist < distanceAttack)
        {
            curentState = State.attack;
        }
    }
    private void Update()
    {
        Debug.Log(numberPositions);

        agentPosition = navMeshAgent.gameObject.transform.position;
        destinationPosition = positions[index].position;
        CheckDistanceToPlayer();

        switch (curentState)
        {
            case State.patrol:
                if (!navMeshAgent.pathPending)
                {
                    inputCommandObject.destination = destinationPosition;
                }
                if (Vector3.Distance(agentPosition, destinationPosition) < 2f)
                {
                    curentState = State.change_color;
                }
                break;
            case State.change_color:
                Color color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                inputCommandObject.desiredColor = color;
                curentState = State.patrol;
                index++;
                index = index % numberPositions;
                break;
            case State.attack:
                inputCommandObject.destination = targetPlayer.transform.position;
                color = new Color(255, 0, 0);
                break;

        }
    }
}