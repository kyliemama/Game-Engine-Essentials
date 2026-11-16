using System.Collections;
using UnityEngine;

public class Staircase : MonoBehaviour
{
    [SerializeField] private GameObject floor1Group;
    [SerializeField] private GameObject floor2Group;
    [SerializeField] private GameObject floor3Group;
    [SerializeField] private GameObject level1Enemy;
    [SerializeField] private GameObject hatch1To2;
    [SerializeField] private Transform level1Staircase;
    [SerializeField] private float level1Position = 2.23f;
    [SerializeField] private float dropSpeed = 8f;
    [SerializeField] private GameObject level2Enemy;
    [SerializeField] private GameObject hatch2To3;
    [SerializeField] private Transform level2Staircase;
    [SerializeField] private float level2Position = 6.75f;
    [SerializeField] private float riseSpeed = 8f;

    private bool level1Triggered = false;
    private bool level2Triggered = false;
    private bool level1EnemyHasAppeared = false;
    private bool level2EnemyHasAppeared = false;

    void Start()
    {
        if (floor1Group != null) floor1Group.SetActive(true);
        if (floor2Group != null) floor2Group.SetActive(false);
        if (floor3Group != null) floor3Group.SetActive(false);

        if (level1Staircase != null) level1Staircase.gameObject.SetActive(false);
        if (level2Staircase != null) level2Staircase.gameObject.SetActive(false);

        if (hatch1To2 != null) hatch1To2.SetActive(true);
        if (hatch2To3 != null) hatch2To3.SetActive(true);
    }

    void Update()
    {
        if (level1Enemy != null && level1Enemy.activeInHierarchy)
        {
            level1EnemyHasAppeared = true;
        }

        if (level2Enemy != null && level2Enemy.activeInHierarchy)
        {
            level2EnemyHasAppeared = true;
        }

        if (!level1Triggered && level1EnemyHasAppeared && IsDefeated(level1Enemy))
        {
            level1Triggered = true;
            StartCoroutine(MoveStaircase(
                hatch1To2,
                level1Staircase,
                level1Position,
                dropSpeed
            ));
        }

        if (!level2Triggered && level2EnemyHasAppeared && IsDefeated(level2Enemy))
        {
            level2Triggered = true;
            StartCoroutine(MoveStaircase(
                hatch2To3,
                level2Staircase,
                level2Position,
                riseSpeed
            ));
        }
    }

    public void CloseHatch1To2()
    {
        if (hatch1To2 != null)
        {
            hatch1To2.SetActive(true);
        }
    }

    bool IsDefeated(GameObject target)
    {
        return target == null || !target.activeInHierarchy;
    }

    IEnumerator MoveStaircase(GameObject hatch, Transform stairs, float targetY, float speed)
    {
        if (hatch != null)
        {
            hatch.SetActive(false);
        }

        if (stairs == null) yield break;

        stairs.gameObject.SetActive(true);

        Vector3 targetPos = new Vector3(
            stairs.position.x,
            targetY,
            stairs.position.z
        );

        while (Vector3.Distance(stairs.position, targetPos) > 0.01f)
        {
            stairs.position = Vector3.MoveTowards(
                stairs.position,
                targetPos,
                speed * Time.deltaTime
            );
            yield return null;
        }

        stairs.position = targetPos;
    }
}
