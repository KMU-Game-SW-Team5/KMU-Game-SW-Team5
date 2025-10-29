using UnityEngine;

public class PlayerFire : MonoBehaviour
{
    public GameObject bulletFactory;
    public GameObject firePosition;
    public float fireRate = 0.1f;

    private float lastFireTime = 0f;
    
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= lastFireTime + fireRate)
        {
            Fire();
            lastFireTime = Time.time;
        }
    }

    void Fire()
    {
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;
        Vector3 targetPoint; 

        
        if (Physics.Raycast(ray, out hit))
        {
            
            targetPoint = hit.point;
        }
        else
        {
            
            targetPoint = ray.GetPoint(1000); 
        }

        Vector3 direction = targetPoint - firePosition.transform.position;
        GameObject bullet = Instantiate(bulletFactory, firePosition.transform.position, Quaternion.LookRotation(direction));
    }
}