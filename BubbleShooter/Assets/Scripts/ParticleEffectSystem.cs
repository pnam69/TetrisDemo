using UnityEngine;
using System.Collections.Generic;

public class ParticleEffectSystem : MonoBehaviour
{
    public static ParticleEffectSystem Instance { get; private set; }

    public static ParticleEffectSystem GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        ParticleEffectSystem existing = Object.FindFirstObjectByType<ParticleEffectSystem>();
        if (existing != null)
            return existing;

        GameObject go = new GameObject("ParticleEffectSystem");
        return go.AddComponent<ParticleEffectSystem>();
    }

    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem bubblePopPrefab;
    [SerializeField] private ParticleSystem bubbleExplosionPrefab;
    [SerializeField] private int maxParticleSystems = 20;

    private Queue<ParticleSystem> availablePopParticles;
    private Queue<ParticleSystem> availableExplosionParticles;
    private HashSet<ParticleSystem> activeParticles;
    private bool initialized;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (!initialized)
        {
            InitializeParticlePool();
        }
    }

    void Start()
    {
        InitializeParticlePool();
    }

    private void InitializeParticlePool()
    {
        if (initialized) return;
        initialized = true;

        availablePopParticles = new Queue<ParticleSystem>();
        availableExplosionParticles = new Queue<ParticleSystem>();
        activeParticles = new HashSet<ParticleSystem>();

        // If no prefab assigned in inspector, create a simple default one so effects are visible
        if (bubblePopPrefab == null)
        {
            bubblePopPrefab = CreateDefaultPopPrefab();
        }

        if (bubbleExplosionPrefab == null)
        {
            bubbleExplosionPrefab = CreateDefaultExplosionPrefab();
        }

        if (bubblePopPrefab != null)
        {
            for (int i = 0; i < maxParticleSystems / 2; i++)
            {
                ParticleSystem ps = Instantiate(bubblePopPrefab, transform);
                ps.gameObject.SetActive(false);
                availablePopParticles.Enqueue(ps);
            }
        }

        if (bubbleExplosionPrefab != null)
        {
            for (int i = 0; i < maxParticleSystems / 2; i++)
            {
                ParticleSystem ps = Instantiate(bubbleExplosionPrefab, transform);
                ps.gameObject.SetActive(false);
                availableExplosionParticles.Enqueue(ps);
            }
        }
    }

    private ParticleSystem CreateDefaultPopPrefab()
    {
        GameObject go = new GameObject("Auto_BubblePopPrefab");
        go.transform.SetParent(transform, false);
        go.SetActive(false);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.45f;
        main.startSpeed = 2.2f;
        main.startSize = 0.35f;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 12, 18));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.08f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 200;

        return ps;
    }

    private ParticleSystem CreateDefaultExplosionPrefab()
    {
        GameObject go = new GameObject("Auto_BubbleExplosionPrefab");
        go.transform.SetParent(transform, false);
        go.SetActive(false);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.8f;
        main.startLifetime = 0.65f;
        main.startSpeed = 2.8f;
        main.startSize = 0.42f;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 18, 28));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingOrder = 200;

        return ps;
    }

    public void PlayBubblePopEffect(Vector3 position, Color bubbleColor)
    {
        if (!initialized) InitializeParticlePool();
        if (bubblePopPrefab == null) return;

        ParticleSystem ps = null;

        if (availablePopParticles.Count > 0)
        {
            ps = availablePopParticles.Dequeue();
        }
        else if (activeParticles.Count < maxParticleSystems)
        {
            ps = Instantiate(bubblePopPrefab, transform);
        }

        if (ps != null)
        {
            ps.transform.position = position;
            ps.gameObject.SetActive(true);

            // Set particle color
            var main = ps.main;
            main.startColor = bubbleColor;

            // ensure the particle system is reset and playing
            ps.Clear(true);
            ps.Play(true);

            activeParticles.Add(ps);
            StartCoroutine(ReturnParticleToPool(ps, ps.main.duration + 0.1f, true));
        }
    }

    public void PlayBubbleExplosionEffect(Vector3 position, Color bubbleColor, int bubblesDestroyed)
    {
        if (!initialized) InitializeParticlePool();
        if (bubbleExplosionPrefab == null) return;

        ParticleSystem ps = null;

        if (availableExplosionParticles.Count > 0)
        {
            ps = availableExplosionParticles.Dequeue();
        }
        else if (activeParticles.Count < maxParticleSystems)
        {
            ps = Instantiate(bubbleExplosionPrefab, transform);
        }

        if (ps != null)
        {
            ps.transform.position = position;
            ps.gameObject.SetActive(true);

            // Set particle properties based on number of bubbles
            var main = ps.main;
            main.startColor = bubbleColor;
            main.maxParticles = Mathf.Min(bubblesDestroyed * 2, 100);

            // ensure the particle system is reset and playing
            ps.Clear(true);
            ps.Play(true);

            activeParticles.Add(ps);
            StartCoroutine(ReturnParticleToPool(ps, ps.main.duration + 0.1f, false));
        }
    }

    private System.Collections.IEnumerator ReturnParticleToPool(ParticleSystem ps, float delay, bool isPop)
    {
        yield return new WaitForSeconds(delay);

        if (ps != null && ps.gameObject != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
            activeParticles.Remove(ps);

            if (isPop && availablePopParticles.Count < maxParticleSystems / 2)
            {
                availablePopParticles.Enqueue(ps);
            }
            else if (!isPop && availableExplosionParticles.Count < maxParticleSystems / 2)
            {
                availableExplosionParticles.Enqueue(ps);
            }
        }
    }
}
