using UnityEngine;

//카메라가 캐릭터를 따라다니는 클래스
public class Follow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
 

    
    void Update()
    {
        transform.position = target.position + offset;
    }
}
