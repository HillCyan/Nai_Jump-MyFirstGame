using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorMoveUp : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;
    bool vanishing = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        
        transform.Translate(0,moveSpeed * Time.deltaTime,0);
        if (transform.position.y > 5.5f)
        {   
            transform.parent.GetComponent<FloorManager>().SpawnFloor();

            // 如果这还是一块亮着 naiKobe 的 Floor2，也让它跟着淡掉
            if (gameObject.CompareTag("Floor2"))
                FadeNaiKobe();

            Destroy(gameObject);
        }
    }  

    // 被玩家踩到后调用：0.25 秒后生成一块新地板并销毁自己
    public void StartVanish()
    {
        if (vanishing) return;
        vanishing = true;
        StartCoroutine(VanishAfterDelay());
    }

    IEnumerator VanishAfterDelay()
    {
        yield return new WaitForSeconds(0.25f);

        if (transform.parent != null)
        {
            FloorManager fm = transform.parent.GetComponent<FloorManager>();
            if (fm != null)
                fm.SpawnFloor();
        }

        FadeNaiKobe();
        Destroy(gameObject);
    }

    // 让 naiKobe 背景跟着这块地板淡出
    void FadeNaiKobe()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
            player.FadeOutNaiKobe(0.3f);
    }
}
