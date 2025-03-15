using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace PuppetEnemy.AI;

public class EnemyPuppet : MonoBehaviour
{
    public enum State
    {
        Spawn,
        Idle,
        Roam,
        Curious,
        Investigate,
        Leave,
        Stun
    }

    private EnemyNavMeshAgent _navMeshAgent => EnemyUtil.GetEnemyNavMeshAgent(_enemy);
    private EnemyRigidbody _rigidbody => EnemyUtil.GetEnemyRigidbody(_enemy);
    private EnemyParent _enemyParent => EnemyUtil.GetEnemyParent(_enemy);
    private EnemyVision _vision => EnemyUtil.GetEnemyVision(_enemy);
    private EnemyStateInvestigate _investigate => EnemyUtil.GetEnemyStateInvestigate(_enemy);
    
    private Enemy _enemy;
    public Enemy Enemy => _enemy;
    
    private PhotonView _photonView;
    private PlayerAvatar _targetPlayer;
    
    private bool _stateImpulse;
    private bool _deathImpulse;

    public bool DeathImpulse
    {
        get => _deathImpulse;
        set => _deathImpulse = value;
    }
    
    private Quaternion _horizontalRotationTarget = Quaternion.identity;
    private Vector3 _agentDestination;
    private Vector3 _targetPosition;
    private bool _hurtImpulse;
    private float hurtLerp;
    private int _hurtAmount;
    private Material _hurtableMaterial;
    private float _pitCheckTimer;
    private bool _pitCheck;
    private float _talkTimer;
    private bool _talkImpulse;

    [Header("State")]
    [SerializeField] public State currentState;
    [SerializeField] public float stateTimer;
        
    [Header("Animation")]
    [SerializeField] private EnemyPuppetAnimationController animator;
    [SerializeField] private SkinnedMeshRenderer _renderer;
    [SerializeField] private AnimationCurve hurtCurve;

    [Header("Rotation and LookAt")]
    public SpringQuaternion horizontalRotationSpring;
    public SpringQuaternion headLookAtSpring;
    public Transform headLookAtTarget;
    public Transform headLookAtSource;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _photonView = GetComponent<PhotonView>();
        _hurtAmount = Shader.PropertyToID("_ColorOverlayAmount");

        if (_renderer != null)
        {
            _hurtableMaterial = _renderer.sharedMaterial;
        }

        hurtCurve = AssetManager.instance.animationCurveImpact;
        
        Debug.Log("THE CHUD HAS ARRIVED!!");
    }

    private void Update()
    {
        if ((!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient) && LevelGenerator.Instance.Generated)
        {
            if (!_enemy.IsStunned())
            {
                if (_enemy.IsStunned())
                {
                    UpdateState(State.Stun);
                }

                switch (currentState)
                {
                    case State.Spawn:
                        StateSpawn();
                        break;
                    case State.Idle:
                        StateIdle();
                        break;
                    case State.Roam:
                        StateRoam();
                        break;
                    case State.Curious:
                        StateCurious();
                        break;
                    case State.Investigate:
                        StateInvestigate();
                        break;
                    case State.Leave:
                        StateLeave();
                        break;
                    case State.Stun:
                        StateStun();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                RotationLogic();
                TargetingLogic();
            }
                HurtEffect();
                
                if (_talkTimer > 0f)
                {
                    _talkTimer -= Time.deltaTime;
                }
                else
                {
                    _talkImpulse = true;
                }
        }
    }

    private void LateUpdate()
    {
        HeadLookAtLogic();
    }

    private void UpdateState(State _newState)
    {
        if (currentState != _newState)
        {
            currentState = _newState;
            stateTimer = 0f;
            _stateImpulse = true;

            if (GameManager.Multiplayer())
            {
                _photonView.RPC(nameof(UpdateStateRPC), RpcTarget.All, currentState);
            }
            else
            {
                UpdateStateRPC(currentState);
            }
        }
    }
    
    [PunRPC]
    private void UpdateStateRPC(State _state)
    {
        currentState = _state;
    }
    
    [PunRPC]
    private void UpdatePlayerTargetRPC(int viewID)
    {
        foreach (PlayerAvatar item in SemiFunc.PlayerGetList())
        {
            if (item.photonView.ViewID == viewID)
            {
                _targetPlayer = item;
                break;
            }
        }
    }

    #region Other Logic

    private void TargetingLogic()
    {
        if ((currentState == State.Curious) && _targetPlayer)
        {
            Vector3 vector = _targetPlayer.transform.position + _targetPlayer.transform.forward * 1.5f;
            if (_pitCheckTimer <= 0f)
            {
                _pitCheckTimer = 0.1f;
                _pitCheck = !Physics.Raycast(vector + Vector3.up, Vector3.down, 4f, LayerMask.GetMask("Default"));
            }
            else
            {
                _pitCheckTimer -= Time.deltaTime;
            }
            if (_pitCheck)
            {
                vector = _targetPlayer.transform.position;
            }
                
                
            _targetPosition = Vector3.Lerp(_targetPosition, vector, 20f * Time.deltaTime);
        }
    }
    
    private void RotationLogic()
    {
        if (EnemyUtil.GetAgentVelocity().normalized.magnitude > 0.1f)
        {
            _horizontalRotationTarget = Quaternion.LookRotation(EnemyUtil.GetAgentVelocity().normalized);
            _horizontalRotationTarget.eulerAngles = new Vector3(0f, _horizontalRotationTarget.eulerAngles.y, 0f);
        }
        if (currentState == State.Spawn || currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate || currentState == State.Leave)
        {
            horizontalRotationSpring.speed = 5f;
            horizontalRotationSpring.damping = 0.7f;
        }
        else
        {
            horizontalRotationSpring.speed = 10f;
            horizontalRotationSpring.damping = 0.8f;
        }
        base.transform.rotation = SemiFunc.SpringQuaternionGet(horizontalRotationSpring, _horizontalRotationTarget);
    }
    
    private void HeadLookAtLogic()
    {
        if (currentState == State.Curious && !_enemy.IsStunned() && currentState != State.Stun && _targetPlayer && !EnemyUtil.IsPlayerDisabled(_targetPlayer))
        {
            Vector3 direction = _targetPlayer.PlayerVisionTarget.VisionTransform.position - headLookAtTarget.position;
            direction = SemiFunc.ClampDirection(direction, headLookAtTarget.forward, 60f);
            headLookAtSource.rotation = SemiFunc.SpringQuaternionGet(headLookAtSpring, Quaternion.LookRotation(direction));
        }
        else
        {
            headLookAtSource.rotation = SemiFunc.SpringQuaternionGet(headLookAtSpring, headLookAtTarget.rotation);
        }
    }

    private void HurtEffect()
    {
        if (_hurtImpulse)
        {
            hurtLerp += 2.5f * Time.deltaTime;
            hurtLerp = Mathf.Clamp01(hurtLerp);
            
            if (_hurtableMaterial != null)
            {
                _hurtableMaterial.SetFloat(_hurtAmount, hurtCurve.Evaluate(hurtLerp));
            }

            if (hurtLerp > 1f)
            {
                _hurtImpulse = false;
                if (_hurtableMaterial != null)
                {
                    _hurtableMaterial.SetFloat(_hurtAmount, 0f);
                }
            }
        }
    }

    #endregion

    #region State Logic

    private void StateSpawn()
    {
        if (_stateImpulse)
        {
            _navMeshAgent.Warp(_rigidbody.transform.position);
            _navMeshAgent.ResetPath();
            _stateImpulse = false;
            stateTimer = 2f;
        }
        
        if (stateTimer > 0f)
        {
            stateTimer -= Time.deltaTime;
        }
        else
        {
            UpdateState(State.Idle);
        }
    }
    
    private void StateIdle()
    {
        if (_stateImpulse)
        {
            _stateImpulse = false;
            stateTimer = Random.Range(4f, 8f);
            _navMeshAgent.Warp(_rigidbody.transform.position);
            _navMeshAgent.ResetPath();
        }
        if (!SemiFunc.EnemySpawnIdlePause())
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                UpdateState(State.Roam);
            }
            if (SemiFunc.EnemyForceLeave(_enemy))
            {
                UpdateState(State.Leave);
            }
        }
    }

    private void StateRoam()
    {
        if (_stateImpulse)
        {
            bool foundTarget = false; 
            
            _talkTimer = Random.Range(5f, 15f);
            stateTimer = Random.Range(4f, 8f);
            LevelPoint point = SemiFunc.LevelPointGet(base.transform.position, 10f, 25f);
            
            if (!point)
            {
                point = SemiFunc.LevelPointGet(base.transform.position, 0f, 999f);
            }

            if (point &&
                NavMesh.SamplePosition(point.transform.position + Random.insideUnitSphere * 3f, out var hit, 5f, -1) &&
                Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
            {
                _agentDestination = hit.position;
                foundTarget = true;
            }
            
            if (!foundTarget)
            {
                return;
            }

            EnemyUtil.SetNotMovingTimer(_rigidbody, 0f);
            _stateImpulse = false;
        }
        else
        {
            _navMeshAgent.SetDestination(_agentDestination);
            
            if (EnemyUtil.GetNotMovingTimer(_rigidbody) > 2f)
            {
                stateTimer -= Time.deltaTime;
            }

            if (stateTimer <= 0f)
            {
                UpdateState(State.Idle);
            }
        }

        if (SemiFunc.EnemyForceLeave(_enemy))
        {
            UpdateState(State.Leave);
        }

        if (_talkImpulse)
        {
            _talkImpulse = false;
            animator.PlayRoamSound();
            _talkTimer = Random.Range(15f, 20f);
        }
    }

    private void StateCurious()
    {
        if (_stateImpulse)
        {
            _stateImpulse = false;
            stateTimer = Random.Range(24f, 30f);
            _talkTimer = Random.Range(5f, 15f);
        }
        
        _navMeshAgent.SetDestination(_targetPosition);
        stateTimer -= Time.deltaTime;
        _vision.StandOverride(0.25f);
        
        if (stateTimer <= 0f || !_targetPlayer || EnemyUtil.IsPlayerDisabled(_targetPlayer))
        {
            UpdateState(State.Idle);
            return;
        }
        
        if (_talkImpulse)
        {
            _talkImpulse = false;
            animator.PlayCuriousSound();
            _talkTimer = Random.Range(15f, 20f);
        }

        // If you get up high, he will just kind of lose interest and leave
        if (_navMeshAgent.CanReach(_targetPosition, 1f) &&
            Vector3.Distance(_rigidbody.transform.position, _navMeshAgent.GetPoint()) < 2f &&
            !NavMesh.SamplePosition(_targetPosition, out var _, 0.5f, -1))
        {
            UpdateState(State.Roam);
            return;
        }
    }
    
    private void StateInvestigate()
    {
        if (_stateImpulse)
        {
            _stateImpulse = false;
            stateTimer = Random.Range(24f, 30f);
            _talkTimer = Random.Range(5f, 15f);
        }
        
        _navMeshAgent.SetDestination(_targetPosition);
        stateTimer -= Time.deltaTime;
        _vision.StandOverride(0.25f);
        
        if (stateTimer <= 0f)
        {
            UpdateState(State.Idle);
            return;
        }
        
        if (_talkImpulse)
        {
            _talkImpulse = false;
            animator.PlayVisionSound();
            _talkTimer = Random.Range(15f, 20f);
        }

        if (_navMeshAgent.CanReach(_targetPosition, 1f) &&
            Vector3.Distance(_rigidbody.transform.position, _navMeshAgent.GetPoint()) < 2f &&
            !NavMesh.SamplePosition(_targetPosition, out var _, 0.5f, -1))
        {
            UpdateState(State.Roam);
            return;
        }
    }
    
    private void StateLeave()
    {
        if (_stateImpulse)
        {
            _talkTimer = Random.Range(5f, 15f);
            stateTimer = 5f;
            bool flag = false;
            LevelPoint levelPoint = SemiFunc.LevelPointGetPlayerDistance(base.transform.position, 30f, 50f);
            if (!levelPoint)
            {
                levelPoint = SemiFunc.LevelPointGetFurthestFromPlayer(base.transform.position, 5f);
            }
            if ((bool)levelPoint && NavMesh.SamplePosition(levelPoint.transform.position + Random.insideUnitSphere * 1f, out var hit, 5f, -1) && Physics.Raycast(hit.position, Vector3.down, 5f, LayerMask.GetMask("Default")))
            {
                _agentDestination = hit.position;
                flag = true;
            }
            if (flag)
            {
                _enemy.NavMeshAgent.SetDestination(_agentDestination);
                EnemyUtil.SetNotMovingTimer(_rigidbody, 0f);
                _stateImpulse = false;
            }
        }
        else
        {
            if (EnemyUtil.GetNotMovingTimer(_rigidbody) > 2f)
            {
                stateTimer -= Time.deltaTime;
            }
            SemiFunc.EnemyCartJump(_enemy);
            if (Vector3.Distance(base.transform.position, _agentDestination) < 1f || stateTimer <= 0f)
            {
                SemiFunc.EnemyCartJumpReset(_enemy);
                UpdateState(State.Idle);
            }
        }
        
        if (_talkImpulse)
        {
            _talkImpulse = false;
            animator.PlayRoamSound();
            _talkTimer = Random.Range(15f, 20f);
        }
    }

    private void StateStun()
    {
        if (_stateImpulse)
        {
            _stateImpulse = false;
        }
        if (!_enemy.IsStunned())
        {
            UpdateState(State.Idle);
        }
    }
    

    #endregion

    #region Callbacks

    public void OnSpawn()
    {
        if (SemiFunc.IsMasterClientOrSingleplayer() && SemiFunc.EnemySpawn(_enemy))
        {
            UpdateState(State.Spawn);
        }
    }
    
    public void OnInvestigate()
    {
        if (SemiFunc.IsMasterClientOrSingleplayer() && (currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate))
        {
            _targetPosition = EnemyUtil.GetOnInvestigateTriggeredPosition(_investigate);
            UpdateState(State.Investigate);
        }
    }
    
    public void OnVision()
    {
        if ((currentState == State.Idle || currentState == State.Roam || currentState == State.Investigate) && !_enemy.IsStunned())
        {
            if (SemiFunc.IsMasterClientOrSingleplayer())
            {
                _targetPlayer = EnemyUtil.GetVisionTriggeredPlayer(_vision);
                if (SemiFunc.IsMultiplayer())
                {
                    _photonView.RPC("UpdatePlayerTargetRPC", RpcTarget.All, _photonView.ViewID);
                }
                UpdateState(State.Curious);
                animator.PlayVisionSound();
            }
        }
    }

    public void OnHurt()
    {
        _hurtImpulse = true;
        animator.PlayHurtSound();
    }

    public void OnDeath()
    {
        _deathImpulse = true;
        animator.PlayDeathSound();
        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            _enemyParent.SpawnedTimerSet(0.0f);
        }
    }
    #endregion
}