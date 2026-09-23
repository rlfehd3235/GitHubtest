using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public Transform grenadeModel;

    public GameObject meshObj;
    public GameObject effectObj;
    public Rigidbody rigid;

    private void Update()
    {
        grenadeModel.Rotate(Vector3.right * 500f * Time.deltaTime);
    }

    private void Start()
    {
        StartCoroutine(Explosion());
    }


    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(2f);
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObj.SetActive(false);
        effectObj.SetActive(true);

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position,
                                                     15, Vector3.up, 0f,
                                                     LayerMask.GetMask("Enemy"));
        foreach (RaycastHit hitObj in rayHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        Destroy(gameObject, 4f);
    }
}
