using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnumManager;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    private float gridSpacing = 1.0f; // Distance between grid positions
    private Vector2Int gridSize = new Vector2Int(3, 3); // 3x3 grid
    public float moveSpeed = 10f; // Speed of movement between grid positions

    private Vector2Int currentGridPosition; // Current grid position (logical, not world space)
    private Vector3 initialPosition; // Initial world position of the player
    private bool isMoving = false; // Flag to prevent movement while transitioning

    public float projectileSpeed = 10.0f; // Speed of laser
    public GameObject projectilePrefab; // Laser prefab
    public Transform projectileSpawnPoint; // Laser is instantiated at this point

    private Renderer[] childRenderers;
    private Color[,] originColors;
    private int blinkCount = 3;
    private float invincibleLength;
    private bool isInvincible = false;
    private float magnetDuration = 10.0f;

    void Awake()
    {
        // Set the initial position as the down of the grid (1,0)
        initialPosition = transform.position - Vector3.down;
        currentGridPosition = new Vector2Int(1, 0); // Start at the down logically

        childRenderers = GetComponentsInChildren<Renderer>();
        originColors = new Color[childRenderers.Length, 2];

        for (int i = 0; i < childRenderers.Length; i++)
        {
            for (int j = 0; j < childRenderers[i].sharedMaterials.Length; j++)
            {
                originColors[i, j] = childRenderers[i].sharedMaterials[j].color;
            }
        }
    }

    void Update()
    {
        // Only allow new movement input if we're not currently moving
        if (!isMoving)
        {
            // Handle left movement
            if (Input.GetKeyDown(KeyCode.LeftArrow) && currentGridPosition.x > 0)
            {
                currentGridPosition.x--;
                StartCoroutine(SmoothMove());
            }
            // Handle right movement
            else if (Input.GetKeyDown(KeyCode.RightArrow) && currentGridPosition.x < gridSize.x - 1)
            {
                currentGridPosition.x++;
                StartCoroutine(SmoothMove());
            }
            // Handle up movement
            else if (Input.GetKeyDown(KeyCode.UpArrow) && currentGridPosition.y < gridSize.y - 1)
            {
                currentGridPosition.y++;
                StartCoroutine(SmoothMove());
            }
            // Handle down movement
            else if (Input.GetKeyDown(KeyCode.DownArrow) && currentGridPosition.y > 0)
            {
                currentGridPosition.y--;
                StartCoroutine(SmoothMove());
            }
        }

        // Fire Laser
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireLaser();
        }

        if (GameManager.inst.GetLife() <= 0)
        {
            isMoving = true;
            transform.Translate(Vector3.down * moveSpeed * 0.005f, Space.World);
        }
        if (transform.position.y < 0)
        {
            gameObject.SetActive(false);
        }
    }

    void FireLaser()
    {
        // Create the projectile at the preassigned point
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position + new Vector3(0, 0, 1f), Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();


        if (projectileRb != null)
        {
            projectileRb.velocity = Vector3.forward * projectileSpeed;
        }
    }

    IEnumerator SmoothMove()
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = CalculateTargetPosition();
        float journeyLength = Vector3.Distance(startPosition, endPosition);
        float startTime = Time.time;

        // Move until we reach the target position
        while (transform.position != endPosition)
        {
            // Calculate journey progress
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            // Move the player using lerp
            transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);

            // If we're very close to the target, snap to it
            if (Vector3.Distance(transform.position, endPosition) < 0.01f)
            {
                transform.position = endPosition;
            }

            yield return null;
        }

        isMoving = false;
    }

    Vector3 CalculateTargetPosition()
    {
        // Calculate target position relative to the initial position
        return initialPosition + new Vector3(
            (currentGridPosition.x - 1) * gridSpacing,
            (currentGridPosition.y - 1) * gridSpacing,
            0
        );
    }

    IEnumerator Blink()
    {
        isInvincible = true;
        for (int i = 0; i < blinkCount; i++)
        {
            ChangeColor(Color.red);
            yield return new WaitForSeconds(0.2f);

            ChangeColorOriginal();
            yield return new WaitForSeconds(0.2f);
        }
        isInvincible = false;
    }

    IEnumerator Invincible()
    {
        invincibleLength = 10f;
        if (isInvincible)
            yield break;
        isInvincible = true;
        while (invincibleLength > 0)
        {
            invincibleLength -= 0.6f;
            ChangeColor(Color.red);
            yield return new WaitForSeconds(0.1f);
            ChangeColor(Color.yellow);
            yield return new WaitForSeconds(0.1f);
            ChangeColor(Color.green);
            yield return new WaitForSeconds(0.1f);
            ChangeColor(Color.blue);
            yield return new WaitForSeconds(0.1f);
            ChangeColor(Color.magenta);
            yield return new WaitForSeconds(0.1f);
        }
        ChangeColorOriginal();

        isInvincible = false;
    }

    private void ChangeColor(Color _color)
    {
        foreach (Renderer renderer in childRenderers)
        {
            foreach (Material material in renderer.sharedMaterials)
                material.color = _color;
        }
    }

    private void ChangeColorOriginal()
    {
        for (int i = 0; i < childRenderers.Length; i++)
        {
            for (int j = 0; j < childRenderers[i].sharedMaterials.Length; j++)
            {
                childRenderers[i].sharedMaterials[j].color = originColors[i, j];
            }
        }
    }

    IEnumerator Magnet()
    {
        float coinSpeed = 50f;
        float distance = 2f;

        while (magnetDuration > 0)
        {
            magnetDuration -= Time.deltaTime;
            GameObject[] coins = GameObject.FindGameObjectsWithTag("Coin");

            foreach (GameObject coin in coins)
            {
                // Debug.Log(coins);
                if (coin != null)
                {
                    if (coin.transform.position.z - transform.position.z < distance)
                        coin.transform.position = Vector3.MoveTowards(coin.transform.position, transform.position, coinSpeed * Time.deltaTime);
                }
            }

            yield return null;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Obstacle"))
        {
            if (isInvincible)
            {
                Destroy(other.gameObject);
                return;
            }
            GameManager.inst.RemoveLife();
            StartCoroutine(Blink());
        }

        if (other.gameObject.CompareTag("Heart"))
        {
            GameManager.inst.AddLife();
        }

        if (other.gameObject.CompareTag("Invincible"))
        {
            StartCoroutine(Invincible());
        }

        if (other.gameObject.CompareTag("Magnet"))
        {
            StartCoroutine(Magnet());
        }

        if (!other.gameObject.CompareTag("Enemy") && !other.gameObject.CompareTag("Obstacle")) //if it is item or coin
        {
            Destroy(other.gameObject);
        }
    }
}
