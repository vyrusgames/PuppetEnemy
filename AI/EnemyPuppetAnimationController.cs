using System;
using UnityEngine;

namespace PuppetEnemy.AI;

public class EnemyPuppetAnimationController : MonoBehaviour
{
    [Header("References")]
    public EnemyPuppet controller;
    public Animator animator;
    
    [Header("Particles")]
    public ParticleSystem[] deathParticles;
    
    [Header("Sounds")]
    [SerializeField] private Sound roamSounds;
    [SerializeField] private Sound visionSounds;
    [SerializeField] private Sound curiousSounds;
    [SerializeField] private Sound hurtSounds;
    [SerializeField] private Sound deathSound;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (controller.DeathImpulse)
        {
            controller.DeathImpulse = false;
            animator.SetTrigger("Dies");
        }

        if (!controller.Enemy.IsStunned())
        {
            animator.SetFloat("Walking", EnemyUtil.GetAgentVelocity(EnemyUtil.GetEnemyNavMeshAgent(controller.Enemy)).magnitude);
        }
    }

    public void SetDespawn()
    {
        EnemyUtil.GetEnemyParent(controller.Enemy).Despawn();
    }

    public void DeathParticlesImpulse()
    {
        ParticleSystem[] array = deathParticles;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].Play();
        }
    }
    
    public void PlayRoamSound()
    {
        roamSounds.Play(controller.Enemy.CenterTransform.position);
    }
    
    public void PlayVisionSound()
    {
        visionSounds.Play(controller.Enemy.CenterTransform.position);
    }
    
    public void PlayCuriousSound()
    {
        curiousSounds.Play(controller.Enemy.CenterTransform.position);
    }
    
    public void PlayHurtSound()
    {
        hurtSounds.Play(controller.Enemy.CenterTransform.position);
    }
    
    public void PlayDeathSound()
    {
        deathSound.Play(controller.Enemy.CenterTransform.position);
    }
}