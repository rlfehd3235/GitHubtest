using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 15;

    Vector3 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;

        Debug.Log(
            "총알 생성 : " +
            gameObject.name +
            " / Tag=" + gameObject.tag
        );

        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        CheckHit();

        previousPosition = transform.position;
    }

    void CheckHit()
    {
        Vector3 move = transform.position - previousPosition;

        float distance = move.magnitude;

        if (distance <= 0.001f)
            return;

        Vector3 direction = move.normalized;

        RaycastHit[] hits = Physics.SphereCastAll(
            previousPosition,
            0.2f,
            direction,
            distance,
            ~0,
            QueryTriggerInteraction.Collide
        );

        foreach (RaycastHit hit in hits)
        {
            // 자기 자신 무시
            if (hit.collider.transform.IsChildOf(transform))
                continue;

            // ==============================
            // 적이 쏜 총알
            // ==============================
            if (CompareTag("EnemyBullet"))
            {
                Player player =
                    hit.collider.GetComponentInParent<Player>();

                if (player != null)
                {
                    Debug.Log(
                        "★★★★★ EnemyBullet 플레이어 적중! ★★★★★"
                    );

                    player.TakeDamage(
                        damage,
                        transform.position
                    );

                    Destroy(gameObject);
                    return;
                }

                // 적 자신의 몸은 무시
                if (hit.collider.GetComponentInParent<Enemy>() != null)
                    continue;

                if (hit.collider.CompareTag("Wall"))
                {
                    Destroy(gameObject);
                    return;
                }
            }

            // ==============================
            // 플레이어가 쏜 총알
            // ==============================
            else if (CompareTag("Bullet"))
            {
                Enemy enemy =
                    hit.collider.GetComponentInParent<Enemy>();

                if (enemy != null)
                {
                    Debug.Log("플레이어 총알 → Enemy 적중");

                    // 기존 Enemy 데미지 방식을 그대로 쓸 거면
                    // 여기에서 Enemy의 데미지 함수를 호출하면 됨.

                    Destroy(gameObject);
                    return;
                }

                if (hit.collider.CompareTag("Wall"))
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}