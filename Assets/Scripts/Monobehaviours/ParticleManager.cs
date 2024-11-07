using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [System.Serializable]
    public class ParticleInfo
    {
        public string key;
        public ParticleSystem particlePrefab;
        public Transform spawnOrigin;
        public bool instantiateInAwake; 
    }
    
    public List<ParticleInfo> particleList = new List<ParticleInfo>();  
    private readonly Dictionary<string, ParticleSystem> _spawnedParticles = new Dictionary<string, ParticleSystem>();
    
    private void Awake()
    {
        foreach (var particle in particleList)
        {
            if (!particle.instantiateInAwake) continue;
            var instantiatedParticle = Instantiate(
                particle.particlePrefab,
                particle.spawnOrigin.position,
                particle.spawnOrigin.rotation,
                particle.spawnOrigin
            );

            instantiatedParticle.Stop();
            _spawnedParticles[particle.key] = instantiatedParticle;
        }
    }
    
    public void ToggleParticleSystem(string key, bool play)
    {
        if (_spawnedParticles.TryGetValue(key, out var particles))
        {
            switch (play)
            {
                case true when !particles.isPlaying:
                    particles.Play();
                    break;
                case false when particles.isPlaying:
                    particles.Stop();
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"Particle with key '{key}' not found.");
        }
    }

    public void SpawnTemporaryParticle(string key, Vector3 position, Quaternion rotation)
    {
        if (!_spawnedParticles.TryGetValue(key, out var particlePrefab))
        {
            particlePrefab = particleList.Find(p => p.key == key)?.particlePrefab;

            if (!particlePrefab)
            {
                Debug.LogWarning($"Particle with key '{key}' not found in particle list.");
                return;
            }
        }

        var tempParticle = Instantiate(particlePrefab, position, rotation);
        tempParticle.Play();

        Destroy(tempParticle.gameObject, tempParticle.main.duration + tempParticle.main.startLifetime.constantMax);
    }
}
