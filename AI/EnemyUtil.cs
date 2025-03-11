using UnityEngine;
using System;
using System.Reflection;

namespace PuppetEnemy.AI;

/*
 * This class is a utility class for the Enemy class.
 *
 * Since a lot of the variables are marked as internal, we have to use reflection to access them.
 * This class provides methods to access these internal variables through static methods.
 * This is an alternative to using BepInEx Publicizer.
 *
 * - Vyrus
 */

public class EnemyUtil
{
    public static EnemyNavMeshAgent GetEnemyNavMeshAgent(Enemy enemy)
    {
        Type enemyType = enemy.GetType();
        FieldInfo agentField = enemyType.GetField("NavMeshAgent", BindingFlags.NonPublic | BindingFlags.Instance);

        if (agentField != null)
        {
            return (EnemyNavMeshAgent) agentField.GetValue(enemy);
        }
        else
        {
            Debug.LogError("NavMeshAgent field not found!");
            return null;
        }
    }

    public static EnemyRigidbody GetEnemyRigidbody(Enemy enemy)
    {
        Type enemyType = enemy.GetType();
        FieldInfo rigidbodyField = enemyType.GetField("Rigidbody", BindingFlags.NonPublic | BindingFlags.Instance);

        if (rigidbodyField != null)
        {
            return (EnemyRigidbody) rigidbodyField.GetValue(enemy);
        }
        else
        {
            Debug.LogError("Rigidbody field not found!");
            return null;
        }
    }
    
    public static EnemyParent GetEnemyParent(Enemy enemy)
    {
        Type enemyType = enemy.GetType();
        FieldInfo parentField = enemyType.GetField("EnemyParent", BindingFlags.NonPublic | BindingFlags.Instance);

        if (parentField != null)
        {
            return (EnemyParent) parentField.GetValue(enemy);
        }
        else
        {
            Debug.LogError("EnemyParent field not found!");
            return null;
        }
    }
    
    public static EnemyVision GetEnemyVision(Enemy enemy)
    {
        Type enemyType = enemy.GetType();
        FieldInfo visionField = enemyType.GetField("Vision", BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (visionField != null)
        {
            return (EnemyVision) visionField.GetValue(enemy);
        }
        else
        {
            Debug.LogError("Vision field not found!");
            return null;
        }
    }
    
    public static EnemyStateInvestigate GetEnemyStateInvestigate(Enemy enemy)
    {
        Type enemyType = enemy.GetType();
        FieldInfo investigateField = enemyType.GetField("StateInvestigate", BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (investigateField != null)
        {
            return (EnemyStateInvestigate) investigateField.GetValue(enemy);
        }
        else
        {
            Debug.LogError("StateInvestigate field not found!");
            return null;
        }
    }
    
    public static bool IsEnemyJumping(Enemy enemy)
    {
        Type enemyType = enemy.GetType();
        FieldInfo jumpField = enemyType.GetField("Jump", BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (jumpField != null)
        {
            Type jumpType = jumpField.FieldType;
            FieldInfo jumpingField = jumpType.GetField("jumping", BindingFlags.NonPublic | BindingFlags.Instance);
                
            if (jumpingField != null)
            {
                return (bool) jumpingField.GetValue(jumpField.GetValue(enemy));
            }
            else
            {
                Debug.LogError("Jumping field not found!");
                return false;
            }
        }
        else
        {
            Debug.LogError("Jump field not found!");
            return false;
        }
    }

    public static bool IsPlayerDisabled(PlayerAvatar playerTarget)
    {
        Type playerType = playerTarget.GetType();
        FieldInfo disabledField = playerType.GetField("isDisabled", BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (disabledField != null)
        {
            return (bool) disabledField.GetValue(playerTarget);
        }
        else
        {
            Debug.LogError("isDisabled field not found!");
            return false;
        }
    }
    
    public static Vector3 GetAgentVelocity(EnemyNavMeshAgent agent)
    {
        Type agentType = agent.GetType();
        FieldInfo velocityField = agentType.GetField("AgentVelocity", BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (velocityField != null)
        {
            return (Vector3) velocityField.GetValue(agent);
        }
        else
        {
            Debug.LogError("AgentVelocity field not found!");
            return Vector3.zero;
        }
    }

    public static Vector3 GetOnInvestigateTriggeredPosition(EnemyStateInvestigate investigate)
    {
        Type visionType = investigate.GetType();
        FieldInfo triggeredPositionField = visionType.GetField("onInvestigateTriggeredPosition", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (triggeredPositionField != null)
        {
            return (Vector3) triggeredPositionField.GetValue(investigate);
        }
        else
        {
            Debug.LogError("onInvestigateTriggeredPosition field not found!");
            return Vector3.zero;
        }
    }
    
    public static PlayerAvatar GetVisionTriggeredPlayer(EnemyVision vision)
    {
        Type visionType = vision.GetType();
        FieldInfo triggeredPlayerField = visionType.GetField("onVisionTriggeredPlayer", BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (triggeredPlayerField != null)
        {
            return (PlayerAvatar) triggeredPlayerField.GetValue(vision);
        }
        else
        {
            Debug.LogError("onVisionTriggeredPlayer field not found!");
            return null;
        }
    }

    public static float GetNotMovingTimer(EnemyRigidbody rb)
    {
            Type rbType = rb.GetType();
        FieldInfo timerField = rbType.GetField("notMovingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (timerField != null)
        {
            return (float) timerField.GetValue(rb);
        }
        else
        {
            Debug.LogError("NotMovingTimer field not found!");
            return 0f;
        }
    }
    
    public static void SetNotMovingTimer(EnemyRigidbody rb, float value)
    {
        Type rbType = rb.GetType();
        FieldInfo timerField = rbType.GetField("notMovingTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            
        if (timerField != null)
        {
            timerField.SetValue(rb, value);
        }
        else
        {
            Debug.LogError("NotMovingTimer field not found!");
        }
    }
    
}