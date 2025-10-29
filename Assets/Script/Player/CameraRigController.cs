using UnityEngine;

public class CameraRigController : MonoBehaviour
{
    [SerializeField] private Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

     
        transform.position = target.position;
        
    
        transform.rotation = target.rotation;
    }
}