using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    public Transform aimPivot;
    public Transform firePoint;
    public GameObject bubblePrefab;
    public float shootSpeed = 10f;

    private GameObject currentBubble;
    private bool canShoot = true;

    void Start()
    {
        SpawnBubble();
    }

    void Update()
    {
        Aim();

        if (Mouse.current.leftButton.wasPressedThisFrame && canShoot)
        {
            Shoot();
        }
    }

    void Aim()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector2 dir = mousePos - transform.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Shoot()
    {
        if (currentBubble == null) return;

        canShoot = false;

        Vector2 dir = firePoint.right;

        currentBubble.GetComponent<Bubble>()
            .Shoot(dir, shootSpeed);

        currentBubble = null;

        Invoke(nameof(SpawnBubble), 0.5f);
    }

    void SpawnBubble()
    {
        currentBubble = Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);
        canShoot = true;
    }
}