using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    public enum Type
    {
        Melee,
        Range
    }
    public Type enemyType;
    [Header("시야")]
    public float sightRange = 10f; //발견거리
    [Range(0, 360)]
    public float sightAngle = 90; //시야각
   


    public LayerMask targetMask;         // Player
    public LayerMask obstacleMask;       // Wall 등

    public int MaxHp;
    public int CurHp;
    public int attackDamage = 10;
    public float Speed;


    public float MeleeRange = 3.5f;
    public float rangeAtkRange = 10f;
    public Transform target;
    
    public bool isChase;
    public bool isAttack;

    [Header("원거리 공격")]
    public Transform bulletPos;
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;

        bool isDead;
    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    Color originColor;
    NavMeshAgent nav;
    Animator anim;

   
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anim =GetComponentInChildren<Animator>();

        originColor = mat.color;
    }

  
    private void Update()
    {
        Targeting();

        if (nav != null && nav.enabled && nav.isOnNavMesh)
        {
            if (isChase && !isAttack)
            {
                nav.isStopped = false;

                if (target != null)
                    nav.SetDestination(target.position);
            }
            else
            {
                nav.isStopped = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            anim.SetTrigger("doAttack");           
        }

    }



    void FreezeVelocity()
    {
        if (!isDead)
        {
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    void Targeting()
    {
        if (target == null)
            return;
        //적 눈위치
        Vector3 eyePos = transform.position + Vector3.up * 1.2f;
        //플레이어 몸통 위치
        Vector3 targetPos = target.position + Vector3.up * 1f;
        Vector3 dirToTarget = targetPos - eyePos;
        float distance = dirToTarget.magnitude;        

        //시야거리 체크
        if (distance > sightRange)
        {
            LostTarget();
            return;
        }

        //시야각도 체크(부채꼴)
        Vector3 flatDir = target.position - transform.position;
        flatDir.y = 0;

        float angle = Vector3.Angle(transform.forward, flatDir.normalized);
        if (angle > sightAngle * 0.5f)
        {
            LostTarget();
            return;
        }

        //벽에 가려졌는지 체크
        if (Physics.Raycast(eyePos, dirToTarget.normalized, distance, obstacleMask))
        {
            LostTarget();
            return;
        }
        //플레이어 발견
        isChase = true;
        if (!isAttack)
        {
            anim.SetBool("isWalk", true);
        }
        float attackRange = enemyType == Type.Melee ? MeleeRange : rangeAtkRange;
        if (distance <= attackRange && !isAttack)
        {
            StartCoroutine(Attack());
        }


    }
    
    void LostTarget()
    {
        if (isAttack) return;
        isChase = false;
        anim.SetBool("isWalk", false);
    }
   
    void Shot()
    {
        if (bulletPrefab == null || bulletPos == null || target == null)
            return;

        // Player의 Collider 중앙을 목표로 함
        Collider targetCollider =
            target.GetComponentInChildren<Collider>();

        Vector3 targetPos;

        if (targetCollider != null)
            targetPos = targetCollider.bounds.center;
        else
            targetPos = target.position + Vector3.up;

        Vector3 shootDir =
            (targetPos - bulletPos.position).normalized;

        GameObject bullet = Instantiate(
            bulletPrefab,
            bulletPos.position,
            Quaternion.LookRotation(shootDir)
        );

        Rigidbody bulletRigid =
            bullet.GetComponent<Rigidbody>();

        if (bulletRigid == null)
        {
            Debug.LogError("총알 Rigidbody 없음!");
            return;
        }

        bulletRigid.linearVelocity =
            shootDir * 20f;

        Debug.DrawRay(
            bulletPos.position,
            shootDir * 20f,
            Color.red,
            3f
        );

        Debug.Log(
            "발사 위치 : " + bulletPos.position +
            " / 목표 : " + targetPos +
            " / 방향 : " + shootDir
        );
    }
    

    IEnumerator Attack()
    {        
        isChase = false;
        isAttack = true;
        if (nav != null && nav.enabled && nav.isOnNavMesh)
        {
            nav.isStopped = true;
        }
        anim.SetBool("isWalk",false);
        anim.SetTrigger("doAttack");
        
        //플레이어 바라보기
        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.LookAt(transform.position + lookDir);
        }
     
        // 근거리 몬스터
        if (enemyType == Type.Melee)
        {
            //실제 공격 애니메이션에서 떄리는 타이밍.
            yield return new WaitForSeconds(1.2f);
            Vector3 dir = target.position - transform.position;
            dir.y = 0;
            float distance = dir.magnitude;
            if (distance <= MeleeRange)
            {
                Player player = target.GetComponentInParent<Player>();
                if (player != null)
                    player.TakeDamage(attackDamage,transform.position);               
            }
            
            yield return new WaitForSeconds(1f);
            
        }
        // 원거리 몬스터
        else if (enemyType == Type.Range)
        {
            Debug.Log("원거리 공격 시작");

            yield return new WaitForSeconds(0.5f);

            Debug.Log("이제 총알 발사!");
            Shot();

            yield return new WaitForSeconds(1f);
        }


        isAttack = false;      
        
    }

    private void FixedUpdate()
    {
        FreezeVelocity();
    }

    void Move()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            CurHp -= weapon.damage;
            if (CurHp < 0)
            {
                CurHp = 0;
            }
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec,false));
            Debug.Log("Melee : " + CurHp);
        }
        if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            CurHp -= bullet.damage;
            if (CurHp < 0)
            {
                CurHp = 0;
            }
            Vector3 reactVec = transform.position - other.transform.position;
            //Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVec, false));
            Debug.Log("Range : " + CurHp);
            
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        CurHp -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec,true));
    }
    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        mat.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (CurHp > 0)
        {
            mat.color = originColor;

            // NavMesh 이동 잠시 중지
            if (nav != null && nav.enabled && nav.isOnNavMesh)
            {
                nav.isStopped = true;
                nav.updatePosition = false;
            }

            reactVec.y = 0;
            reactVec.Normalize();

            float knockDistance = isGrenade ? 2.5f : 0.7f;
            float knockTime = 0.15f;

            Vector3 startPos = transform.position;
            Vector3 endPos =
                startPos + reactVec * knockDistance;

            float time = 0f;

            while (time < knockTime)
            {
                time += Time.deltaTime;

                transform.position = Vector3.Lerp(
                    startPos,
                    endPos,
                    time / knockTime
                );

                yield return null;
            }

            // NavMesh에게 현재 위치 다시 알려줌
            if (nav != null && nav.enabled)
            {
                nav.nextPosition = transform.position;
                nav.updatePosition = true;

                if (nav.isOnNavMesh)
                    nav.isStopped = false;
            }

            yield break;
        }

        // 사망
        isDead = true;

        mat.color = Color.gray;
        gameObject.layer = 12;

        isChase = false;
        isAttack = false;

        if (nav != null && nav.enabled)
            nav.enabled = false;

        anim.SetTrigger("doDie");

        reactVec = reactVec.normalized;

        if (isGrenade)
        {
            reactVec += Vector3.up * 3f;
            rigid.freezeRotation = false;

            rigid.AddForce(
                reactVec * 10f,
                ForceMode.Impulse
            );

            rigid.AddTorque(
                reactVec * 15f,
                ForceMode.Impulse
            );
        }
        else
        {
            reactVec += Vector3.up;

            rigid.AddForce(
                reactVec * 10f,
                ForceMode.Impulse
            );
        }

        Destroy(gameObject, 4f);
    }

}
