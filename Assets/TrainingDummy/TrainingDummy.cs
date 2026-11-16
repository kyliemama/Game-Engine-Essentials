using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A target dummy for testing attacks. Needs a Health component (added automatically)
/// and a Collider (the "Build Default Dummy" context menu adds one).
///
/// Quick start: create an empty GameObject, add this component, then click the three dots
/// (or right-click the component header) and choose "Build Default Dummy".
///
/// On every hit it flashes, wobbles away from the camera, floats the damage number and
/// updates a small stats display (HP, hits, total damage, last hit, DPS).
/// </summary>
[RequireComponent(typeof(Health))]
public class TrainingDummy : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("The part that wobbles. Its pivot should sit at ground level.")]
    public Transform visual;
    [Tooltip("On: the dummy refills its health instead of dying, so you can keep testing.")]
    public bool refillInsteadOfDying = true;
    [Tooltip("Only used when 'Refill Instead Of Dying' is off.")]
    public float respawnDelay = 2f;
    [Tooltip("Seconds without a hit before health and stats reset.")]
    public float resetAfterIdle = 5f;

    [Header("Hit Feedback")]
    public Color flashColor = new Color(1f, 0.25f, 0.25f);
    public float flashTime = 0.15f;
    public float wobbleKick = 140f;        // degrees per second added by a hit
    public float wobbleStiffness = 120f;
    public float wobbleDamping = 8f;
    public float maxTilt = 25f;

    [Header("Floating Text")]
    public bool showDamageNumbers = true;
    public bool showStats = true;
    public float statsHeight = 2.8f;
    [Tooltip("DPS is the damage dealt in this many seconds, divided by this many seconds.")]
    public float dpsWindow = 5f;
    public bool logHits = false;

    Health health;
    Renderer[] renderers;
    Color[] baseColors;
    MaterialPropertyBlock block;
    TextMeshPro statsText;
    Camera cam;

    float flashTimer;
    float lastHitTime = -999f;
    int hitCount;
    int totalDamage;
    int lastDamage;
    bool dead;

    float tilt, tiltVel;
    Vector3 tiltAxis = Vector3.right;

    readonly List<(float time, int dmg)> recent = new List<(float time, int dmg)>();

    void Awake()
    {
        health = GetComponent<Health>();
        block = new MaterialPropertyBlock();
        CacheRenderers();
    }

    void Start()
    {
        health.onDamaged.AddListener(OnDamaged);
        health.onDied.AddListener(OnDied);

        if (showStats)
            statsText = CreateWorldText("StatsText", transform.position + Vector3.up * statsHeight, 3f, Color.white);
    }

    void OnDestroy()
    {
        if (health)
        {
            health.onDamaged.RemoveListener(OnDamaged);
            health.onDied.RemoveListener(OnDied);
        }
    }

    void CacheRenderers()
    {
        Transform root = visual ? visual : transform;
        renderers = root.GetComponentsInChildren<Renderer>();
        baseColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material m = renderers[i].sharedMaterial;
            Color c = Color.white;
            if (m)
            {
                if (m.HasProperty("_BaseColor")) c = m.GetColor("_BaseColor");   // URP / HDRP
                else if (m.HasProperty("_Color")) c = m.GetColor("_Color");      // Built-in
            }
            baseColors[i] = c;
        }
    }

    // ---------------------------------------------------------------- hits

    void OnDamaged(int amount)
    {
        // Health calls this before its death check, so refilling here stops it dying.
        if (refillInsteadOfDying && health.current <= 0) health.current = health.maxHealth;

        if (!cam) cam = Camera.main;

        lastHitTime = Time.time;
        hitCount++;
        totalDamage += amount;
        lastDamage = amount;
        recent.Add((Time.time, amount));

        flashTimer = flashTime;
        KickWobble(amount);

        if (showDamageNumbers) StartCoroutine(FloatText(amount.ToString()));
        if (logHits) Debug.Log($"{name} took {amount} damage ({health.current}/{health.maxHealth})");
    }

    void OnDied()
    {
        if (!dead) StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        dead = true;
        yield return new WaitForSeconds(respawnDelay);
        ResetDummy();
        dead = false;
    }

    void KickWobble(int amount)
    {
        // Tip away from the camera (the player's point of view)
        Vector3 away = cam ? transform.position - cam.transform.position : transform.forward;
        away.y = 0f;
        if (away.sqrMagnitude < 0.001f) away = transform.forward;
        away.Normalize();

        tiltAxis = Vector3.Cross(Vector3.up, away);
        tiltVel += wobbleKick * Mathf.Clamp(amount / 20f, 0.5f, 2f);
    }

    /// <summary>Refills health and clears the stats.</summary>
    [ContextMenu("Reset Dummy")]
    public void ResetDummy()
    {
        health.current = health.maxHealth;
        hitCount = 0;
        totalDamage = 0;
        lastDamage = 0;
        recent.Clear();
    }

    // ---------------------------------------------------------------- per frame

    void Update()
    {
        UpdateWobble();
        UpdateFlash();

        if (!dead && hitCount > 0 && Time.time - lastHitTime > resetAfterIdle)
            ResetDummy();
    }

    void UpdateWobble()
    {
        if (!visual) return;

        float target = dead ? 90f : 0f;
        float limit = dead ? 90f : maxTilt;

        float acc = -wobbleStiffness * (tilt - target) - wobbleDamping * tiltVel;
        tiltVel += acc * Time.deltaTime;
        tilt = Mathf.Clamp(tilt + tiltVel * Time.deltaTime, -limit, limit);

        Vector3 localAxis = visual.parent ? visual.parent.InverseTransformDirection(tiltAxis) : tiltAxis;
        visual.localRotation = Quaternion.AngleAxis(tilt, localAxis);
    }

    void UpdateFlash()
    {
        if (flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;
        ApplyColor(Mathf.Clamp01(flashTimer / flashTime));
    }

    void ApplyColor(float flashAmount)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i]) continue;
            Color c = Color.Lerp(baseColors[i], flashColor, flashAmount);
            renderers[i].GetPropertyBlock(block);
            block.SetColor("_Color", c);
            block.SetColor("_BaseColor", c);
            renderers[i].SetPropertyBlock(block);
        }
    }

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!statsText) return;

        recent.RemoveAll(e => Time.time - e.time > dpsWindow);
        float dps = 0f;
        foreach (var e in recent) dps += e.dmg;
        dps /= Mathf.Max(0.01f, dpsWindow);

        statsText.text =
            $"HP {health.current}/{health.maxHealth}\n" +
            $"Hits {hitCount}   Total {totalDamage}\n" +
            $"Last {lastDamage}   DPS {dps:0.0}";

        Face(statsText.transform);
    }

    // ---------------------------------------------------------------- floating text

    IEnumerator FloatText(string message)
    {
        Vector3 start = transform.position + Vector3.up * (statsHeight - 0.6f)
                        + new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f));
        TextMeshPro t = CreateWorldText("DamageNumber", start, 5f, Color.yellow);
        t.text = message;

        const float life = 0.8f;
        float time = 0f;
        while (time < life && t)
        {
            time += Time.deltaTime;
            t.transform.position = start + Vector3.up * (time * 1.5f);
            Color c = t.color;
            c.a = 1f - time / life;
            t.color = c;
            Face(t.transform);
            yield return null;
        }

        if (t) Destroy(t.gameObject);
    }

    TextMeshPro CreateWorldText(string objectName, Vector3 worldPosition, float size, Color color)
    {
        GameObject go = new GameObject(objectName);
        go.transform.position = worldPosition;

        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.text = "";
        return tmp;
    }

    void Face(Transform t)
    {
        if (cam) t.rotation = Quaternion.LookRotation(t.position - cam.transform.position);
    }

    // ---------------------------------------------------------------- one-click builder

    /// <summary>Builds a simple dummy from primitives. Use the component's context menu.</summary>
    [ContextMenu("Build Default Dummy")]
    void BuildDefaultDummy()
    {
        if (visual)
        {
            Debug.LogWarning("TrainingDummy: 'Visual' is already assigned, so nothing was built.");
            return;
        }

        GameObject v = new GameObject("Visual");
        v.transform.SetParent(transform, false);
        visual = v.transform;
#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(v, "Build Training Dummy");
#endif

        MakePart(PrimitiveType.Cylinder, "Base",  new Vector3(0f, 0.05f, 0f), new Vector3(1.0f, 0.05f, 1.0f));
        MakePart(PrimitiveType.Cylinder, "Body",  new Vector3(0f, 0.90f, 0f), new Vector3(0.6f, 0.90f, 0.6f));
        MakePart(PrimitiveType.Cube,     "Arms",  new Vector3(0f, 1.50f, 0f), new Vector3(1.4f, 0.15f, 0.15f));
        MakePart(PrimitiveType.Sphere,   "Head",  new Vector3(0f, 2.05f, 0f), Vector3.one * 0.5f);

        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (!col) col = gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 1.15f, 0f);
        col.height = 2.3f;
        col.radius = 0.5f;
    }

    void MakePart(PrimitiveType type, string partName, Vector3 localPosition, Vector3 localScale)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = partName;
        go.transform.SetParent(visual, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;

        // The root's capsule collider handles hits, so the parts don't need their own.
        Collider c = go.GetComponent<Collider>();
        if (c) DestroyImmediate(c);

#if UNITY_EDITOR
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Build Training Dummy");
#endif
    }
}