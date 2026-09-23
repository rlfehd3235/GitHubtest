using System.Collections;
using UnityEngine;//유니티 엔진 기본 기능 을 사용하기 휘한 네임 스페이스
using UnityEngine.UIElements;//유니티 유아이 툴킷의 네임스페이스

public class Player : MonoBehaviour
{
    float hAxis; //수평 입력값을 저장할 변수
    float vAxis; //수직 입력값을 저장할 변수

    public int ammo;
    public int coin;
    public int hp;
    
    public int maxAmmo;
    public int maxCoin;
    public int maxHp;
    public int maxHasGrenade;

    public float speed;//플레이어의 이동속도를 조절할수있는 공개 변수  인스펙터에서 설정 가능

    [Header("피격")]
    public float hitStunTime = 0.3f;
    public float KnockBackForce = 4f; //밀리는힘
    public Camera followCamera;
    

    bool wDown; //걷기 입력 상태를 저장할 불리언 변수
    bool jDown; //점프 입력 상태를 순간적으로 저장할 불리언 변수
    bool dDown; //구르기
    bool fDown; //공격
    bool gDown; //수류탄
    bool rDown; //장전
    bool iDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;    
    
    
    bool isDodge; //회피 입력 상태는 순간적으로 저장할 불리언 변수
    bool isSwap; //무기교체 입력 상태를 순간적으로 저장할 불리언 변수
    bool isFireReady = true;
    bool isReload;
    bool isJump;//현재 플레이어가 점프중인지 여부를 체크하는 변수
    bool isAction;
    bool isShoting;
    bool isDamage;
    bool isDead;
    float fireDelay;

    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenade;
    public GameObject grenadeOBJ;
    public Transform grenadePos;

    Vector3 moveVec; //플레이어가 이동할 방향과 크기를 담는 3차원 벡터 변수
    Vector3 dodgeVec;

    Vector3 jumpVec;
    Rigidbody rigid;//플레이어의 물리연산을 담당하는 리지드 바디 컴포넌트를 참조할 변수
    Animator anim; //플레이어의 애니메이션을 제어하는 애니메이터컴포넌트를 참조할 변수

    MeshRenderer[] meshs;
    GameObject nearObject;
    Weapon equipWeapon;
    int equipWeaponIndex = -1;
    
  

    //[Awake]스크립트가 실행 될때 가장먼저 호출되며, 주로 컴포넌트 초기화에 사용
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();//게임 오브젝트에 부착된 리지드바디 컴포넌트를 찾아 rigid 변수 에 할당
        anim = GetComponentInChildren<Animator>();// 자식 오브젝트에 있는 Animator 컴포넌트를 찾아 anim 변수에 할당
        meshs = GetComponentsInChildren<MeshRenderer>(); 
    }

    //[Update] 매프레임마다 호출되며,주로 사용자 입력과 프레임 단위의 로직처리에 사용
    private void Update()
    {
        if (isDead)
            return;

        if (isDamage)
            return;

        GetInput(); //사용자의 키보드 마우스 입력을 받아오는 함수를 호출 (함수를 만들어서 업데이트에서 호출)
        Move();//입력값을 바탕으로 플레이어를 이동시키는 함수를 호출 (함수를 만들어서 업데이트에서 호출)
        Turn(); //이동방향을 바라보고록 회전시키는 함수를 호출 
        Jump(); //점프 입력을 확인하고 점프를 실행하는 합수를 호출
        Attack();
        Grenade();
        Reload();
        Swap();
        Interation(); //상호작용 입력을 확인하고 상호작용을 실행하는 함수를 호출       
    }

    void GetInput()
    {
        if (!isDodge)
        {
            hAxis = Input.GetAxisRaw("Horizontal");
            vAxis = Input.GetAxisRaw("Vertical");
            wDown = Input.GetButton("Walk");//지정한 버튼이 눌려있는 동안 ture를 반환합니다
        }
        jDown = Input.GetButtonDown("Jump");//지정한 버튼을 누른 그순간에만 단발적으로 ture를 반환
        fDown = Input.GetButtonDown("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        dDown = Input.GetKeyDown(KeyCode.LeftShift);
        iDown = Input.GetKeyDown(KeyCode.E);
        sDown1 = Input.GetKeyDown(KeyCode.Alpha1);
        sDown2 = Input.GetKeyDown(KeyCode.Alpha2);
        sDown3 = Input.GetKeyDown(KeyCode.Alpha3);

        //겟인풋 함수 안에서 v 키가 눌린 순간 바로 회피방향 고정
        //이렇게 하면 업데이트 함수가 돌때 무브함수가 실행되기 전부터 이미 회피방향이 고정
        if (dDown && !isJump && !isDodge && !isAction)
        {
            if (gDown)
                Debug.Log("Fire2 입력 들어옴!");
            Vector3 inputDir = new Vector3(hAxis, 0, vAxis).normalized;
            if (inputDir == Vector3.zero)  //a만약 가만히 서서 회피를 누르면 바라보는 방향으로 구름
            {
                dodgeVec = transform.forward;
            }
            else
            {
                dodgeVec = inputDir;
            }
            transform.LookAt(transform.position + dodgeVec);

            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }

    }

    void Move()
    {
        if (isDodge)
        {
            float dodgeSpeed = speed * 2f;
            rigid.linearVelocity = new Vector3(dodgeVec.x * dodgeSpeed, rigid.linearVelocity.y,dodgeVec.z * dodgeSpeed);
        }

        // 무기를 교체 중이거나 총을 쏘는 중이면
        // 이동을 하지 않음
        if (isSwap || isShoting)
        {
            rigid.linearVelocity = new Vector3(0, rigid.linearVelocity.y, 0);
            anim.SetBool("isRun", false);
            anim.SetBool("isWalk", false);
            return;
        }
        // 점프 중이거나
        if (isJump)
        {
            rigid.linearVelocity = new Vector3(jumpVec.x * speed, rigid.linearVelocity.y, jumpVec.y * speed);
            return;
        }
        // 재장전 중이거나
        // 다른 행동 중이라면 이동을 멈춥니다
        if (isReload || isAction)
        {
            rigid.linearVelocity = new Vector3(0,rigid.linearVelocity.y,0);//멈춰 있을때 미끄러짐 방지
            anim.SetBool("isRun",false);
            anim.SetBool("isWalk",false);
            return;
        }
         //수평 수직 입력을 이용해 3차원 이동 벡터를 만들고,
         //노말라이즈드를 이용해 대각선 이동시 속도가 빨라지는 현상을 방지하고자 방향 벡터의 크기를 1로 정규화
        moveVec = new Vector3(hAxis,0,vAxis).normalized; //평소 이동로직
       

        float applySpeed = speed;
        if (wDown) applySpeed *= 0.3f;        

        //Vector3 nextVec = moveVec * applySpeed * Time.deltaTime;
        Vector3 nextVelocity = new Vector3(moveVec.x * applySpeed, rigid.linearVelocity.y, moveVec.z * applySpeed);
        rigid.linearVelocity = nextVelocity;
        
       
        anim.SetBool("isRun", moveVec != Vector3.zero); //애니메이터의 "isRun"불리언 파라미터를 값을 설정
                                                        //moveVec이 0이 아니면(움직이면) ture,아니면false
        anim.SetBool("isWalk",wDown);//애니메이터의"isWalk"파라미터에 wDown 걷기 여부를 전달
    }

    void Turn()
    {
        // 회피 중이거나
        // 무기 교체 중이거나
        // 총을 쏘는 중이라면 턴을 하지않음.
        if (isDodge || isSwap || isShoting)
            return;
        if (moveVec != Vector3.zero)
        //이동방향이 존재할 때, 함수 transform.LookAt 을 통해
        //현재 위치에서 이동할 방향(현재위치+moveVec)을 즉시 바라보게 회전
        {
            //키보드에 의한 회전
            transform.LookAt(transform.position + moveVec);
        }
    }

    void Jump()
    {

        if (jDown && !isJump && !isDodge && !isSwap)//점프 버튼이 눌렸고 ,현재 점프중이 아닌 상태일때만 실행
        {
            jumpVec = moveVec;
            rigid.AddForce(Vector3.up * 14, ForceMode.Impulse);//rigidbody.AddForce: 윗방향(vector3.up*14)으로
                                                               //순간적인힘(ForceMode.Impulse)을가해 위로 뛰게함.           
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");//애니메이터의"doJump"트리거(일회성 이벤트)를 작동시켜 점프 애니메이션을 즉시 재생
            isJump = true;//점프중 상태를 트루로 바꿔 또점프하는것을 방지           
        }
    }

    void Attack()
    {
        if (equipWeapon == null) //현재 장착중인 무기가 없다면  공격할수 없음으로 종료
            return;
        fireDelay += Time.deltaTime; //매 프레임 마다 경과시간(델타타임:이전프레임에서 현재 프레임까지 걸린시간)
                                     //을 파이어 딜에이에 더합.
                                     // 예:
                                     // 0 → 0.016 → 0.032 → 0.048 ...
       
       // 현재 무기의 공격속도보다
       // 기다린 시간이 길어졌는지 검사합니다.
       // 예:
       // AtkSpeed = 0.5
       // fireDelay = 0.3 → 공격 불가능
       // fireDelay = 0.6 → 공격 가능
        isFireReady = equipWeapon.AtkSpeed < fireDelay;

        if (fDown && isFireReady && !isDodge && !isSwap)
        {
            // 총일 경우
            if (equipWeapon.type == Weapon.Type.Range)
            {
                // 이동 즉시 정지
                rigid.linearVelocity =
                    new Vector3(0, rigid.linearVelocity.y, 0);

                isShoting = true;

                // 마우스 방향 찾기
                Ray ray =
                    followCamera.ScreenPointToRay(Input.mousePosition);

                Plane groundPlane =
                    new Plane(Vector3.up, transform.position);

                if (groundPlane.Raycast(ray, out float distance))
                {
                    Vector3 mousePoint = ray.GetPoint(distance);

                    Vector3 shootDir =
                        mousePoint - transform.position;

                    shootDir.y = 0;

                    if (shootDir != Vector3.zero)
                    {
                        transform.LookAt(
                            transform.position + shootDir
                        );
                    }
                }

                // 사격 후 0.25초 동안 정지
                CancelInvoke(nameof(ShotOut));
                Invoke(nameof(ShotOut), 0.25f);
            }

            //===회전후 발사.
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }        
    }

    void Grenade()
    {
        if (hasGrenade == 0) return;
        if (gDown && !isReload && !isSwap)
        {
            //카메라에서 마우스 방향으로 ray생성
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            //플레이어 높이의 가상의 바닥
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float distance))
            {
                //마우스가 월드에서 가리키는 위치
                Vector3 mousePoint = ray.GetPoint(distance);
                //수류탄 생성 위치에서 마우스 방향 계상
                Vector3 throwDir = mousePoint - grenadePos.position;
                //수평방향만사용
                throwDir.y = 0;
                throwDir.Normalize();

                //플레이어앞grenadePos에서 수류탄 샌성.                
                GameObject instantGrenade = Instantiate(grenadeOBJ,grenadePos.position, grenadePos.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                //앞으로 가는힘
                Vector3 forwardForce = throwDir * 12f;
                //위로 뜨는힘
                Vector3 upForce = Vector3.up * 6f;

                //포물선으로 던지기
                rigidGrenade.AddForce(forwardForce + upForce, ForceMode.Impulse);
                //rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenade--;
                grenades[hasGrenade].SetActive(false);
            }
        }


    }
    void ShotOut()
    {
        isShoting = false;
    }

    void Reload()
    {
        print("reload");
        if (equipWeapon == null)
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (ammo == 0)
            return;
        if (rDown && !isJump && !isDodge && !isSwap && isFireReady)
        {
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut",2f);
        }
    }

    void ReloadOut()
    {
        int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;
    }


    void Dodge()
    {        
    }

    void DodgeOut()
    {        
        isDodge = false;
    }

    void Swap()
    {
        int weaponIndex = -1;

        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        // 숫자키를 안 눌렀으면 종료
        if (weaponIndex == -1)
            return;

        Debug.Log("선택한 무기 번호 : " + weaponIndex);
        Debug.Log("보유 여부 : " + hasWeapons[weaponIndex]);

        // 안 먹은 무기면 종료
        if (!hasWeapons[weaponIndex])
        {
            Debug.Log("아직 없는 무기");
            return;
        }

        // 배열에 실제 오브젝트가 있는지 확인
        if (weapons[weaponIndex] == null)
        {
            Debug.LogError("Weapons[" + weaponIndex + "]이 비어있음");
            return;
        }

        Debug.Log("장착 대상 : " + weapons[weaponIndex].name);

        // 기존 무기 끄기
        if (equipWeapon != null)
        {
            equipWeapon.gameObject.SetActive(false);
        }

        // 새 무기 가져오기
        equipWeapon = weapons[weaponIndex].GetComponentInChildren<Weapon>(true);

        if (equipWeapon == null)
        {
            Debug.LogError("Weapon 컴포넌트를 못 찾음");
            return;
        }

        // 실제 활성화
        equipWeapon.gameObject.SetActive(true);

        Debug.Log("ActiveSelf : " + equipWeapon.gameObject.activeSelf);
        Debug.Log("ActiveInHierarchy : " + equipWeapon.gameObject.activeInHierarchy);

        equipWeaponIndex = weaponIndex;

        anim.SetTrigger("doSwap");
        isSwap = true;

        Invoke(nameof(SwapOut), 0.4f);
    }
    void SwapOut()
    {
        isSwap = false;
    }



    void Interation()
    {
        if (iDown && nearObject != null && !isJump && !isDodge)
        {
            if (nearObject.tag == "Weapon" || nearObject.tag == "Melee")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
        }
    }
    public void TakeDamage(int damage , Vector3 hitPosition)
    {
        if (isDamage || isDead)
            return;

        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            Die();
            return;

        }
        StartCoroutine(HitRoutine(hitPosition));
        Debug.Log("플레이어 피격! HP : " + hp);

       
    }
    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        rigid.linearVelocity = Vector3.zero;

        anim.SetTrigger("doDie");

        Debug.Log("Player Dead");
    }

    private void OnCollisionEnter(Collision collision) //유니티 물리엔진에 의해 다른 오브젝트와 충돌이
                                                       //시작되는 "그 순간" 자동으로 호출되는 이벤트 함수
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);
            isJump = false; //바닦에 떨어졌음으로 false로 바꿔 다시 점프가 가능한 상태로 돌림

            rigid.linearVelocity = new Vector3(0, rigid.linearVelocity.y, 0);

            //착지후 잠시 멈추게하는 코루틴 실행
            StartCoroutine(LandingRoutine());
        }
    }

    private void OnTriggerEnter(Collider other)//트리거가 발동하면 시작돼는 함수
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    hp += item.value;
                    if (hp > maxHp)
                        hp = maxHp;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenade].SetActive(true);
                    hasGrenade += item.value;
                    if (hasGrenade > maxHasGrenade)
                        hasGrenade = maxHasGrenade;
                    break;
            }
            Destroy(other.gameObject);
        }
        //적 총알에 맞았을때.
        //else if (other.CompareTag("EnemyBullet"))
        //{
        //    Bullet enemyBullet = other.GetComponent<Bullet>();

        //    if (enemyBullet != null)
        //    {
        //        Debug.Log("적 총알 맞음!");

        //        TakeDamage(
        //            enemyBullet.damage,
        //            other.transform.position
        //        );
        //    }
        //}

    }

    IEnumerator HitRoutine(Vector3 hitPosition)
    {
        isDamage = true;
        //현재 이동 멈춤
        rigid.linearVelocity = Vector3.zero;
        //맞은 반대방향 계산
        Vector3 knockBackDir = transform.position - hitPosition;
        knockBackDir.y = 0;
        knockBackDir.Normalize();
        //살짝뒤로 밀기
        rigid.AddForce(knockBackDir * KnockBackForce,ForceMode.Impulse);
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }
        //잠깐 경직
        yield return new WaitForSeconds(hitStunTime);
        //밀림을 멈춤
        rigid.linearVelocity = new Vector3(0, rigid.linearVelocity.y, 0);

        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
        isDamage = false;

    }
    //IEnumerator OnDamage()
    //{
    //    isDamage = true;
    //    foreach(MeshRenderer mesh in meshs)
    //    {
    //        mesh.material.color = Color.yellow;
    //    }
    //    yield return new WaitForSeconds(1f);
    //    isDamage = false;
    //    foreach (MeshRenderer mesh in meshs)
    //    {
    //        mesh.material.color = Color.white;
    //    }
    //}

    void OnTriggerStay(Collider other)
    {
        if(other.tag == "Weapon" || other.tag == "Melee")
        {
            Debug.Log("트리거 작동 중: " + other.name);
            nearObject = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Weapon" || other.tag == "Melee")
        {
            nearObject = null;
        }    
    }


    //착지후 잠시 멈추는 시간을 조절하는 코루틴
    IEnumerator LandingRoutine()
    {
        isAction = true;
        yield return new WaitForSeconds(0.3f); 
        isAction = false;
    }





}