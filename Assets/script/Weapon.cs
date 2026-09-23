using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee,Range};
    public Type type;
    public int damage;
    public float AtkSpeed;
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;
    public int maxAmmo;
    public int curAmmo;

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }

        else if (type == Type.Range && curAmmo > 0)
        {
            curAmmo--;            
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        //1
        yield return new WaitForSeconds(0.3f);//3프레임 대기
        meleeArea.isTrigger = true;
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        //2
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;
        meleeArea.isTrigger = false;
        //3
        yield return new WaitForSeconds(0.5f);
        trailEffect.enabled = false;
    }

    //Use() 메인루틴 -> Swing() 서브루틴 -> use() 메인루틴
    //코루틴은 메인루틴 -> Swing() 코루틴(함께실행)
    IEnumerator Shot()
    {
        //총알발사
        GameObject intanBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = intanBullet.GetComponent<Rigidbody>();
        bulletRigid.linearVelocity = bulletPos.forward * 50;
        Destroy(intanBullet,4f);
        yield return null;
        //탄피배출
        GameObject intanCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody CaseRigid = intanCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
        CaseRigid.AddForce(caseVec, ForceMode.Impulse);
        CaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
        Destroy(intanCase, 3f);
    }


  

    
    void Update()
    {
        
    }
}
