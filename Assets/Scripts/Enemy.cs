using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public enum Role { Chaser, Flanker }
    public Role role = Role.Chaser;
    Vector3 offsetTarget;

    public Transform target;
    NavMeshAgent agent;

    public float baseSpeed = 3f; // 基本速度

    public float predictDistance = 3f;   // 未来予測距離

    PlayerControll playerCotrl;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = GameObject.FindWithTag("Player").transform;

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
            playerCotrl = playerObj.GetComponent<PlayerControll>();
        }

        agent.stoppingDistance = 0f;
    }

    private void Update()
    {
        // 敵速度を随時更新
        agent.speed = baseSpeed * GameManager.instance.NowEnemySpeed;

        // プレイヤー停止チェック
        if(playerCotrl != null && playerCotrl.IsStopped)
        {
            MoveToSafe(target.position);
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        // 一定距離以内ならプレイヤーへ直進
        if (dist > 2)
        {
            MoveToSafe(target.position);
            return;
        }

        // プレイヤーの少し後ろを目的地にする
        if(role == Role.Chaser)
        {
            offsetTarget = transform.position - target.forward * 1.5f;
        }

        // 進める位置に補正
        NavMeshHit hit;
        if (NavMesh.SamplePosition(offsetTarget, out hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            agent.SetDestination(target.position);
        }
    }

    private void MoveToSafe(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 3f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        else agent.SetDestination(position);
    }
}
