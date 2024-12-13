using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossControl : MonoBehaviour
{

    [Header("Color related variables")]
    [SerializeField] Color bossStartColor;
    [SerializeField] Color myColorRed = new Color(203f / 255f, 83f / 255f, 83f / 255f, 1);
    Dictionary<Color, Color> myColorDict;

    [Header("Boss movement variables")]
    [SerializeField] Transform bossTransform;
    [SerializeField] float bossHorizontalRange; // x range of boss
    [SerializeField] float bossHorizontalSpeed;
    bool bossDead = false;
    float bossHorizontalPos;    // Position boss is to move to
    Transform playerTransform;

    [Header("Boss Death animation variables")]
    [SerializeField] ParticleSystem bossSmoke;
    [SerializeField] float bossReducedSize;
    // Initial positions and rotations for post death effect
    Dictionary<Transform, Vector3> bossComponentsInitialPositions = new Dictionary<Transform, Vector3>();
    Dictionary<Transform, Quaternion> bossComponentsInitialRotations = new Dictionary<Transform, Quaternion>();

    BossStageManager bossStageManager;

    // Start is called before the first frame update
    void Awake()
    {
        // Set up boss color
        myColorDict = new Dictionary<Color, Color>()
        {
            {Color.red, myColorRed},
            {Color.black, Color.gray }
        };
        ChangeColor(bossStartColor);

        // Set up for boss death animation
        foreach (Transform childTransform in bossTransform)
        {
            bossComponentsInitialPositions[childTransform] = childTransform.position;
            bossComponentsInitialRotations[childTransform] = childTransform.rotation;
        }

        bossStageManager = GameObject.Find("BossStageManager").GetComponent<BossStageManager>();
    }

    void Start()
    {
        playerTransform = bossStageManager.ActiveCharacter().transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!bossDead)
        {
            HandleBossMovement();
        }
    }

    public void HandleBossMovement()
    {
        // Rotate to look at player (+ height adjustment to rotate only y axis)
        bossTransform.LookAt(playerTransform.position + Vector3.up * (bossTransform.position.y - playerTransform.position.y));
        // Generate new boss target position once in close enough proximity
        if (Mathf.Abs(bossHorizontalPos - bossTransform.position.x) < 0.1)
        {
            bossHorizontalPos = Random.Range(-bossHorizontalRange, bossHorizontalRange);
        }

        // Boss side to side movement
        if (bossHorizontalPos < bossTransform.position.x)
        {
            bossTransform.position -= new Vector3(bossHorizontalSpeed * Time.deltaTime, 0, 0);
        }
        if (bossHorizontalPos > bossTransform.position.x)
            bossTransform.position += new Vector3(bossHorizontalSpeed * Time.deltaTime, 0, 0);
    }

    public void ChangeColor(Color bossColorKey)
    {
        Color bossColorValue = (myColorDict.GetValueOrDefault(bossColorKey, bossColorKey)); // If dict contains key, extract value. Otherwise, pass color as is.
        ChangeColorHelper(gameObject.transform, bossColorValue); // Actual color (value) passed as parameter
    }

    // Recursively change color of all children
    public void ChangeColorHelper(Transform transform, Color bossColor)
    {
        foreach (Transform childTransform in transform)
        {
            GameObject child = childTransform.gameObject;
            SkinnedMeshRenderer smr = child.GetComponent<SkinnedMeshRenderer>();
            if (smr != null) smr.material.color = bossColor;
            MeshRenderer mr = child.GetComponent<MeshRenderer>();
            if (mr != null) mr.material.color = bossColor;

            if (childTransform.childCount > 0) ChangeColorHelper(childTransform, bossColor);
        }
    }

    // Call for boss death
    public void BossDeath()
    {
        if (!bossDead)
        {
            bossDead = true;

            // Fall back effect
            foreach (Transform childTransform in bossTransform)
            {
                GameObject child = childTransform.gameObject;
                Rigidbody rb = child.GetComponent<Rigidbody>();

                if (rb)
                {
                    rb.isKinematic = false;
                    rb.AddForce(Vector3.forward * 500 + Vector3.up * 100f, ForceMode.Impulse);
                    if (child.transform.position.y < 1.0f) 
                    {
                        Debug.Log("child y position is under 1.0f");
                        Vector3 currentVelocity = rb.velocity;

                        rb.velocity = new Vector3(-currentVelocity.x, -currentVelocity.y, -currentVelocity.z);
                    }
                }
            }
            Invoke("BossDeathHelper", 2f);
        }
    }
    /*IEnumerator CheckAndRemoveFallenPartsCoroutine()
    {
        while (bossTransform.childCount > 0)
        {
            List<Transform> partsToRemove = new List<Transform>();

            foreach (Transform child in bossTransform)
            {
                // 파츠의 y 위치가 임계값 이하인지 확인
                
            }

            // 임계값 이하인 파츠 제거
            foreach (Transform part in partsToRemove)
            {
               
            }
            // 다음 체크까지 대기
            yield return new WaitForSeconds(0.2f);
        }
    }*/
    // Called by BossDeath()
    public void BossDeathHelper()
    {
        // Particle effect
        bossSmoke.Play();
        Invoke("BossDeathTransform", 0.5f);
    }

    // Called by BossDeathHelper()
    void BossDeathTransform()
    {
        // Transform boss into small white rabbit
        ChangeColor(Color.white); // Pass the color key: Color.red or Color.black. Colors that are not defined as keys will be passed as is to the helper function.
        foreach (Transform childTransform in bossTransform)
        {
            GameObject child = childTransform.gameObject;

            if (child.CompareTag("WeakSpot")) continue;

            Rigidbody rb = child.GetComponent<Rigidbody>();

            // Restore positions
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
            }
            childTransform.position = bossComponentsInitialPositions[childTransform];
            childTransform.rotation = bossComponentsInitialRotations[childTransform];
        }
        bossTransform.localScale *= bossReducedSize;
    }

    public bool IsDead()
    {
        return bossDead;
    }
}