using UnityEngine;

public class SteamPlayerController : MonoBehaviour
{
    private SteamP2PManager p2pManager;
    private Vector3 lastSentPosition;
    private float sendInterval = 0.1f; // 초당 10번 전송
    private float lastSendTime;
    
    void Start()
    {
        p2pManager = FindObjectOfType<SteamP2PManager>();
    }
    
    void Update()
    {
        HandleMovement();
        SendPositionUpdate();
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0, vertical) * (5f * Time.deltaTime);
        transform.Translate(movement);
        
        // 액션 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            p2pManager.SendGameAction("jump");
        }
    }
    
    void SendPositionUpdate()
    {
        // 위치가 변경되었고 전송 간격이 지났으면 전송
        if (Time.time - lastSendTime > sendInterval)
        {
            if (Vector3.Distance(transform.position, lastSentPosition) > 0.01f)
            {
                p2pManager.SendPlayerPosition(transform.position);
                lastSentPosition = transform.position;
                lastSendTime = Time.time;
            }
        }
    }
}