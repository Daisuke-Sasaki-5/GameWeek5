using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFlanker : MonoBehaviour
{
    public enum Role { Chaser,Flanker}
    public Role role = Role.Flanker;
    
    public Transform target;
    private NavMeshAgent agent;

    public float baseSpeed = 2f; // 基本速度

    // プレイヤーの横方向オフセット
    public float flankDistance = 3f;
    public float behindOffset = 1.5f;
    public float offsetRange = 1.2f;

    // 未来予測距離
    public float predictDistance = 3f;

    // プレイヤーの外周グルグル対策
    private Vector3 lastPlayerDir;
    private float sameDirectionTImer = 0f;
    private float angleTimer = 0f;

    // 追跡モード追加
    private float flankModeTimer = 0f;
    private bool isFlanking = false;

    // 敵(Chaser)が離れ始めた瞬間をカバー
    private bool chaserIsFar = false;
    private float checkTimer = 0f;

    PlayerControll playerCotrl;

    void Start()
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

    void Update()
    {
        if (target == null) return;

        // 敵速度を随時更新
        agent.speed = baseSpeed * GameManager.instance.NowEnemySpeed;

        // プレイヤー停止チェック
        if (playerCotrl != null && playerCotrl.IsStopped)
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

        DetectPlayerCircling();
        UpdateFlankMode();

        if(isFlanking)
        {
            FlankMove();
        }
        else
        {
            NormalChase();
        }

        // Chaserがプレイヤーから離れ始めたときカバーする
        checkTimer += Time.deltaTime;
        if(checkTimer > 0.2f)
        {
            checkTimer = 0f;
            float distNow = Vector3.Distance(target.position, transform.position);
            float distFuture = Vector3.Distance(target.position, transform.position + agent.velocity * 0.2f);

            chaserIsFar = (distFuture > distNow + 0.05f);
        }
    }

    private void MoveToSafe(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 3f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
        else agent.SetDestination(position);
    }

    /// <summary>
    /// 通常追跡
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void NormalChase()
    {
        Vector3 futurePos = target.position + target.forward * predictDistance;

        Vector3 randomOffset = target.right * UnityEngine.Random.Range(-offsetRange,offsetRange);

        futurePos += randomOffset;

        agent.SetDestination(futurePos);
    }

    /// <summary>
    /// プレイヤーの左右に回り込む
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void FlankMove()
    {
        Vector3 sideDir = (role == Role.Flanker)?target.right: -target.right;

        // 回り込み位置
        Vector3 flankPos = target.position + sideDir * flankDistance - target.forward * behindOffset;

        agent.SetDestination(flankPos);
    }

    /// <summary>
    /// プレイヤーの外周ぐるぐる判定
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void DetectPlayerCircling()
    {
        Vector3 playerDir = target.forward;

        // 向きがほぼ同じなら維持時間を加算
        if (Vector3.Dot(lastPlayerDir, playerDir) > 0.9f)
            sameDirectionTImer += Time.deltaTime;
        else sameDirectionTImer = 0f;

        // 角度変化も加味
        float angle = Vector3.SignedAngle(lastPlayerDir,playerDir,Vector3.up);
        if(Mathf.Abs(angle) > 1f)angleTimer += Time.deltaTime;
        else angleTimer = 0f;

        lastPlayerDir = playerDir;
    }

    /// <summary>
    /// フランクモード ON/OFF Chaserの弱点を補う
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void UpdateFlankMode()
    {
        // 発動条件
        if(!isFlanking && (sameDirectionTImer > 2f && angleTimer > 2f || chaserIsFar))
        {
            isFlanking = true;
            flankModeTimer = 3f;
        }

        if (isFlanking)
        {
            flankModeTimer -= Time.deltaTime;
            if(flankModeTimer <= 0f)
            {
                isFlanking= false;
            }
        }
    }
}
