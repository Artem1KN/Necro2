using UnityEngine;
using System.Collections.Generic;

public class SwordWeapon : WeaponBase
{
    [Header("Attack Settings")]
    public LayerMask enemyLayers;
    public float attackRange = 2f;
    public float attackRadius = 1f;
    public GameObject energyOrbPrefab;

    protected void PerformAttack(float damage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayers);
        
        List<EnemyBase> enemiesHit = new List<EnemyBase>();
        
        foreach (var collider in hitColliders)
        {
            EnemyBase enemy = collider.GetComponent<EnemyBase>();
            if (enemy != null && !enemiesHit.Contains(enemy))
            {
                Vector3 hitPoint = collider.ClosestPointOnBounds(transform.position);
                OnHitEnemy(enemy, hitPoint);
                enemiesHit.Add(enemy);
            }
        }
        
        Debug.Log($"[Sword] Attack! Damage: {damage}, Enemies hit: {enemiesHit.Count}");
    }

    protected override void TryFire()
    {
        if (data.appliesToMeleeOnlyOnHit)
        {
            PerformAttack(data.baseDamage);
        }
        else
        {
            currentHeat += data.heatPerShot;
            currentHeat = Mathf.Clamp(currentHeat, 0, data.overheatThreshold);
            
            Debug.Log("[Sword] Swing animation/logic triggered");
            OnMiss();
        }
    }

    protected override void ExecuteSkill()
    {
        float damage = data.baseDamage * 2f;
        
        PerformAttack(damage);
        
        currentHeat += data.heatPerShot * 3f;
        currentHeat = Mathf.Clamp(currentHeat, 0, data.overheatThreshold);
        
        lastSkillTime = Time.time;
    }

    protected void OnHitEnemy(EnemyBase enemy, Vector3 hitPoint)
    {
        float damage = data.baseDamage;
        
        if (currentHeat >= data.optimalZoneStart && currentHeat <= data.optimalZoneEnd)
        {
            damage *= data.optimalHeatMultiplier;
        }
        
        enemy.TakeDamage(damage);
        
        currentHeat += data.heatPerShot;
        currentHeat = Mathf.Clamp(currentHeat, 0, data.overheatThreshold);
        
        /*if (enemy.CurrentHP == 0)
        {
            SpawnEnergyOrb(enemy);
        }*/
    }

    protected void OnMiss()
    {
    }

/*
    protected void SpawnEnergyOrb(EnemyBase enemy)
    {
        if (energyOrbPrefab != null && enemy != null)
        {
            OrbData orbData = energyOrbPrefab.GetComponent<EnergyOrb>()?.orbData;
            
            if (orbData == null)
            {
                GameObject orbObj = Instantiate(energyOrbPrefab, enemy.transform.position, Quaternion.identity);
                EnergyOrb orb = orbObj.GetComponent<EnergyOrb>();
                
                if (orb != null && playerMotor != null && playerMotor.playerHealth != null)
                {
                    PlayerHealth player = playerMotor.playerHealth;
                    orb.Setup(player.Heal, null);
                }
            }
            else
            {
                GameObject orbObj = Instantiate(energyOrbPrefab, enemy.transform.position, Quaternion.identity);
                EnergyOrb orb = orbObj.GetComponent<EnergyOrb>();
                
                if (orb != null && orb.orbData == null)
                {
                    orb.orbData = orbData;
                }
                
                if (orb != null && playerMotor != null && playerMotor.playerHealth != null)
                {
                    PlayerHealth player = playerMotor.playerHealth;
                    orb.Setup(player.Heal, null);
                }
            }
        }
    }*/
}
