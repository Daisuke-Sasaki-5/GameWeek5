using System;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerControll : MonoBehaviour
{
    [Header("移動設定")]
    private float moveSpeed = 5f;
    private float gravity = -9.81f;

    [Header("視点設定")]
    public float mouseSensitivityX = 3f;
    public float mouseSensitivityY = 2f;
    public Transform cam;
    [SerializeField] private float SmoothX = 0.1f;
    [SerializeField] private float SmoothY = 0.05f;

    private float targetXRot;
    private float smoothXRot;
    private float yRot;
    private float smoothYRot;

    private CharacterController characterController;
    private Vector3 velocity;


    // ---- プレイヤー停止判定用 ----
    public bool IsStopped { get; private set; }
    public Vector3 LastMoveVelocity { get; private set; }

    private Vector3 lastPos;
    private float checkTImer;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // カーソルを非表示
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();
    }

    // プレイヤー視点
    private void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivityX * Time.deltaTime * 100;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivityY * Time.deltaTime * 100;

        // 上下回転
        targetXRot -= mouseY;
        targetXRot = Mathf.Clamp(targetXRot, -50f, 50f);
        yRot += mouseX;

        // スムージング
        smoothXRot = Mathf.Lerp(smoothXRot,targetXRot,SmoothX);
        smoothYRot = Mathf.Lerp(smoothYRot,yRot,SmoothY);

        // 反映
        cam.localRotation = Quaternion.Euler(smoothXRot, 0f, 0f);
        transform.rotation = Quaternion.Euler(0, smoothYRot, 0f);
    }
    
    // 移動
    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // 重力を適用
        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    // 敵との当たり判定
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            // カメラ揺れ発生
            CameraShake.Instance.ShakeCamera();

            // ゲームオーバー
            GameManager.instance.GameOver();
        }
    }

    private void LateUpdate()
    {
        checkTImer += Time.deltaTime;

        if(checkTImer >= 0.1) // 0.1秒ごとに判定
        {
            Vector3 delta = transform.position - lastPos;
            float speed = delta.magnitude / checkTImer;

            LastMoveVelocity = delta / checkTImer;

            // ほぼ動いていなければ停止
            IsStopped = speed < 0.05f;

            lastPos = transform.position;
            checkTImer = 0;
        }
    }
}
