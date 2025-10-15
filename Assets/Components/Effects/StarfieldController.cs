using UnityEngine;
[RequireComponent(typeof(ParticleSystem))]
public class StarfieldController : MonoBehaviour
{
    
    [Header("References")]
    public InertialBody shipBody;
    [Header("Settings")]
    public float speedMultiplier = 0.5f;
    public float smoothTime = 0.3f;
    public float minRandomFactor = 0.2f;
    public float maxRandomFactor = 2f;

    private ParticleSystem starfield;
    private ParticleSystem.Particle[] particles;
    private Vector3 targetVelocity;
    private Vector3 currentVelocity;
    private Vector3 velocitySmooth;

    void Awake()
    {
        starfield = GetComponent<ParticleSystem>();
        this.enabled = false;
    }
    public void Initialize(InertialBody body)
    {
        shipBody = body; 
        this.enabled = true;
    }
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (shipBody == null) return;
        
        targetVelocity = - (Vector3)shipBody.velocity * speedMultiplier;
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref velocitySmooth, smoothTime);

        int count = starfield.particleCount;
        if (particles == null || particles.Length < count)
            particles = new ParticleSystem.Particle[count];

        count = starfield.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {
            float randomFactor = Mathf.Lerp(minRandomFactor, maxRandomFactor, particles[i].randomSeed / (float)uint.MaxValue);
            Vector3 velocity = currentVelocity * randomFactor;
            particles[i].velocity = velocity;
        }

        starfield.SetParticles(particles, count);
    }
}