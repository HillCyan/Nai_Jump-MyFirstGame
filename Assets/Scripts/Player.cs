using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpSpeed = 6f;
    [SerializeField] int HP=5;
    [SerializeField] GameObject HPBar;

    [SerializeField] Text scoreText;
    private Rigidbody2D rb;

    private Animator anim;
    int score;
    float scoreTime;
    GameObject currentFloor;
    bool isGrounded;
    bool isDead;

    GameObject naiKobe;
    SpriteRenderer naiKobeSr;
    Coroutine naiKobeFadeCo;

    // 给 UI 读取用
    public bool IsDead { get { return isDead; } }
    public int Score { get { return score; } }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        score = 0;
        scoreTime = 0;

        // 找到 naiKobe 背景（包括未激活的），开局先藏起来
        naiKobe = FindNaiKobe();
        if (naiKobe != null)
        {
            naiKobeSr = naiKobe.GetComponent<SpriteRenderer>();
            naiKobe.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;   // 死亡后不再处理输入，保持下落

        if (Input.GetKey(KeyCode.RightArrow))
        {

        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {

        }


        bool moving = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A);

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(moveSpeed*Time.deltaTime,0,0);
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(-moveSpeed * Time.deltaTime, 0, 0);
            GetComponent<SpriteRenderer>().flipX = true;
        }

        // 只有“按住 A/D”且“在地面”时才触发跑步；空中 isGrounded=false，必定不会跑
        anim.SetBool("run", moving && isGrounded);


        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            anim.SetBool("jump", true);
            isGrounded = false;                 // 起跳瞬间就视为离地，避免空中还有几帧能跑
            //anim.SetBool("isGrounded", false);
        }

        // 按 S：把脚下的地板变成 Trigger，玩家就能直接穿过去
        if (Input.GetKeyDown(KeyCode.S) && isGrounded && currentFloor != null)
        {
            Collider2D floorCol = currentFloor.GetComponent<Collider2D>();
            if (floorCol != null)
                floorCol.isTrigger = true;

            isGrounded = false;   // 立刻视为离地，避免穿下去之后还能跳/跑
        }
        UpdateScore();
        updateHPBar();
    }


    void ModifyHP(int num)
    {
        if (isDead) return;   // 死亡后 HP 不再变化

        int oldHP = HP;
        HP += num;
        // 上限跟血条的实际格数一致，避免 HP 超过格数后血条顶格不动
        HP = Mathf.Clamp(HP, 0, HPBar.transform.childCount);
        if (num < 0)
        {
            anim.SetTrigger("hurt");
            
        }
        // 血量刚归零时：调整背景的 Order in Layer，并进入死亡状态
        if (oldHP > 0 && HP == 0)
        {
            OnHPZero();
            OnDeath();
        }
        updateHPBar();
    }

    // 血量归零时：background 的 Order in Layer 变成 -2，background2 变成 -1
    void OnHPZero()
    {
        SpriteRenderer[] renderers = FindObjectsOfType<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
        {
            string n = sr.gameObject.name.ToLower();
            if (n == "background")
                sr.sortingOrder = -2;
            else if (n == "background2")
                sr.sortingOrder = -1;
        }
    }

    // 血量归零：保持 hurt 姿态、无视碰撞、持续下落
    void OnDeath()
    {
        isDead = true;
        GetComponent<AudioSource>().Play();
        // 保持 hurt 姿态：强制切到 hurt 状态并冻结动画
        anim.Play("hurt", 0, 0f);
        anim.speed = 0f;

        // 关闭自己的碰撞体，这样会穿过所有地板继续下落
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    // 找到场景里（包括未激活）名为 naiKobe 的物体
    GameObject FindNaiKobe()
    {
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "naiKobe")
                    return t.gameObject;
            }
        }
        return null;
    }

    // 踩到 Floor2：naiKobe 背景出现
    public void ShowNaiKobe()
    {
        if (naiKobe == null) return;

        if (naiKobeFadeCo != null)
        {
            StopCoroutine(naiKobeFadeCo);
            naiKobeFadeCo = null;
        }

        naiKobe.SetActive(true);
        if (naiKobeSr != null)
        {
            Color c = naiKobeSr.color;
            c.a = 1f;
            naiKobeSr.color = c;
        }
    }

    // Floor2 消失：naiKobe 跟着淡出并隐藏
    public void FadeOutNaiKobe(float duration)
    {
        if (naiKobe == null || !naiKobe.activeSelf) return;

        if (naiKobeFadeCo != null)
            StopCoroutine(naiKobeFadeCo);
        naiKobeFadeCo = StartCoroutine(FadeNaiKobe(duration));
    }

    IEnumerator FadeNaiKobe(float duration)
    {
        float t = 0f;
        Color c = naiKobeSr.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            naiKobeSr.color = c;
            yield return null;
        }

        naiKobe.SetActive(false);
        naiKobeFadeCo = null;
    }

    void updateHPBar()
    {
        for (int i = 0; i < HPBar.transform.childCount;++i)
        {
            if (HP > i)
            {
                HPBar.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                HPBar.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;   // 死亡后不再响应碰撞

        if (collision.gameObject.tag == "Floor1" || collision.gameObject.tag == "Floor2")
        {
            if (collision.contacts[0].normal == new Vector2(0f, 1f))  // normal -> 碰撞处法线
            {
                currentFloor = collision.gameObject;
                anim.SetBool("jump", false);
                //anim.SetBool("isGrounded", true);
                isGrounded = true;
                if (collision.gameObject.tag == "Floor2")
                {
                    collision.gameObject.GetComponent<AudioSource>().Play();
                }

                // 只有真正踩在 Floor2 上才加血（每次落地加一次，不是每次碰撞）
                if (currentFloor.tag == "Floor2")
                {
                    ModifyHP(2);

                    // 踩到 Floor2：+11 分，并让这块地板 0.25 秒后触发 SpawnFloor 然后消失
                    score += 11;
                    scoreText.text = "分数: " + score.ToString();
                    FloorMoveUp floorMove = currentFloor.GetComponent<FloorMoveUp>();
                    if (floorMove != null)
                        floorMove.StartVanish();

                    // 踩到 Floor2：naiKobe 背景出现
                    ShowNaiKobe();
                }
            }
        }
        else if (collision.gameObject.tag == "TianHuaBan")
        {
            collision.gameObject.GetComponent<AudioSource>().Play();
            ModifyHP(-2);
            if (currentFloor != null)
                currentFloor.GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (collision.gameObject.tag == "JianCi")
        {
            collision.gameObject.GetComponent<AudioSource>().Play();
            anim.SetBool("jump", false);
            //anim.SetBool("isGrounded", true);
            ModifyHP(-1);
            if (collision.contacts[0].normal == new Vector2(0f, 1f))
                isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) //刚刚离开碰撞体时触发一次
    {
        if (collision.gameObject.tag == "Floor1" || collision.gameObject.tag == "Floor2" || collision.gameObject.tag == "JianCi")
        {
            isGrounded = false;
            //anim.SetBool("isGrounded", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;   // 死亡后不再响应触发器

        if (collision.gameObject.tag == "DeathField")
        { 
            Debug.Log("Wasted");
            ModifyHP(-HP);
        }
        
    }


    void UpdateScore()
    {
        if (isDead) return;   // 死亡后分数不再变化

        scoreTime += Time.deltaTime;
        if (scoreTime > 2f)
        {
            score++;
            scoreTime = 0;
            scoreText.text = "分数: "+score.ToString();
            // 每满 10 分加 1 血（只在加分的这一帧触发一次）
            if (score % 10 == 0)
                ModifyHP(1);
        }
    }

}
