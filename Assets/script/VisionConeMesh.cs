using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using static UnityEngine.Rendering.HableCurve;



[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]

public class VisionConeMesh : MonoBehaviour
{
    public Enemy enemy;
    [Header("시야 Mesh")]
    public int segments = 40;
    public float yOffset = 0.05f;

    [Header("시야 색상")]
    public Color normalColor = new Color(0f, 1f, 0f, 0.1f);
    public Color detectedColor = new Color(1f, 0f, 0f, 0.1f);

    public float colorChangeSpeed = 8f;

    Mesh mesh;
    MeshRenderer meshRenderer;
    Material visionMaterial;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Vision Cone Mesh";

        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();

        // 이 Enemy 전용 Material 인스턴스
        visionMaterial = meshRenderer.material;
    }

    private void LateUpdate()
    {
        if (enemy == null)
            return;


        // Enemy 위치/회전 따라가기
        transform.position =
            enemy.transform.position + Vector3.up * yOffset;

        transform.rotation =
            Quaternion.Euler(
                0,
                enemy.transform.eulerAngles.y,
                0
            );


        // 시야 Mesh 생성
        DrawCone(
            enemy.sightRange,
            enemy.sightAngle
        );


        // ========================
        // 색상 변경
        // ========================

        Color targetColor;

        // 발견했거나 공격 중
        if (enemy.isChase || enemy.isAttack)
        {
            targetColor = detectedColor;
        }
        else
        {
            targetColor = normalColor;
        }


        visionMaterial.color =
            Color.Lerp(
                visionMaterial.color,
                targetColor,
                Time.deltaTime * colorChangeSpeed
            );
    }


    void DrawCone(float radius, float angle)
    {
        int vertexCount = segments + 2;

        Vector3[] vertices =
            new Vector3[vertexCount];

        int[] triangles =
            new int[segments * 3];


        // 부채꼴 중심
        vertices[0] = Vector3.zero;


        float startAngle =
            -angle * 0.5f;

        float angleStep =
            angle / segments;


        for (int i = 0; i <= segments; i++)
        {
            float currentAngle =
                startAngle + angleStep * i;

            float rad =
                currentAngle * Mathf.Deg2Rad;


            float x =
                Mathf.Sin(rad) * radius;

            float z =
                Mathf.Cos(rad) * radius;


            vertices[i + 1] =
                new Vector3(x, 0, z);
        }


        int triIndex = 0;


        for (int i = 0; i < segments; i++)
        {
            triangles[triIndex++] = 0;

            triangles[triIndex++] =
                i + 1;

            triangles[triIndex++] =
                i + 2;
        }



        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}





  

