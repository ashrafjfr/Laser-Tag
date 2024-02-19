using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Vuforia;

/**
 * Refactored! no longer handles game logic
 * Process inputs within Unity
 * Coordinate animations and visual events
 * Communicates with MQTT Client 
 */


public class ActionHandler : MonoBehaviour
{   
    public GameObject playerUI;
    public GameObject enemyUI;
    public GameObject enemyTarget;
    public ARTrackingHandler arTrackingHandler;

    [System.Serializable]
    public class PlayerState
    {
        public int HP;
        public int Bullets;
        public int Grenades;
        public int ShieldPoints;
        public int Deaths;
        //public int Score;
        public int Shields;
        
        
        //public bool ShieldPoints > 0;
        public bool IsSpecialActive;
        public bool IsReloading;
    }

    [System.Serializable]
    public class GameState
    {
        public PlayerState Player = new PlayerState();
        public PlayerState Enemy = new PlayerState();
    }

    public GameState CurrentGameState = new GameState();


    // Connection Status
    //public bool gunConnected = true;
    //public bool vestConnected = true;
    //public bool gloveConnected = true;


    // Events

    public event Action PlayerStateUpdated;
    public event Action EnemyStateUpdated;
    

    // AR events
    public event Action onPlayerShoot;
    public event Action onPlayerReload;
    public event Action onPlayerGrenadeThrow;
    public event Action onPlayerHammerThrow;
    public event Action onPlayerSpearThrow;
    public event Action onPlayerPortal;
    public event Action onPlayerWeb;
    public event Action onPlayerFist;
    public event Action onPlayerShield;
    public event Action onEnemyShield;

    public event Action onPlayerLogout;

    public event Action onGunDisconnected;
    public event Action onGunConnected;

    void Start()
    {
        RespawnPlayer();
        RespawnEnemy();
        CurrentGameState.Player.Deaths = 0;
        CurrentGameState.Enemy.Deaths = 0;
        //CurrentGameState.Player.Score = 0;
        //CurrentGameState.Enemy.Score = 0;
        //playerScore = 0;
        //enemyScore = 0;
    }

    // Game logic
    void Update()
    {
 
    }

    public void PlayerShield()
    {   
        if (CurrentGameState.Player.Shields > 0 && (CurrentGameState.Player.ShieldPoints == 0))
        {
            //CurrentGameState.Player.ShieldPoints > 0 = true;
            //CurrentGameState.Player.ShieldPoints = 30;
            //CurrentGameState.Player.Shields -= 1;
            PlayerStateUpdated?.Invoke();
        }
              
    }

    public void EnemyShield()
    {
        if (CurrentGameState.Enemy.Shields > 0 && (CurrentGameState.Enemy.ShieldPoints == 0))
        {
            //CurrentGameState.Enemy.ShieldPoints > 0 = true;        
            EnemyStateUpdated?.Invoke();
        }
        
    }
    

    // Applies damage according to the following rules:
    // If isPlayerShieldUp, damage the shield first. 
    // If damage is greater than the shield points, shields--, and the overflow damage is applied to the player's HP.
    // If the player has no more HP, call RespawnPlayer() to reset the player's stats immediately
    // Apply the same rules if it's the enemy that's being damaged.
    public void DamagePlayer(string action)
    {
        //int damage = 0;
        //if (action == "grenade") damage = 30;
        //if (action == "bullet") damage = 10;
        //if (action == "special") damage = 10;
        //if (isPlayerShieldUp && playerShields > 0)
        //{
        //    playerShieldPoints -= damage;

        //    if (playerShieldPoints <= 0) // shield breaks
        //    {
        //        int overflow = -playerShieldPoints; // Same as Mathf.Abs(playerShieldPoints)

        //        playerShields -= 1;
        //        playerHP -= overflow;
        //        isPlayerShieldUp = false;
        //        onPlayerShieldOverlayToggled?.Invoke(false);
        //        if (playerShields > 0) // shield bar will show next shield health; i.e 30
        //        {
        //            playerShieldPoints = 30;
        //        }
        //        else // last shield or no shields
        //        {
        //            playerShieldPoints = 0;
        //        }
        //    }
        //}
        //else
        //{
        //    playerHP -= damage;
        //}

        //if (playerHP <= 0) // player dies, enemy scores
        //{
        //    RespawnPlayer();
        //    enemyScore += 1;
        //    onEnemyScoreChanged?.Invoke(enemyScore);
        //}
        //onPlayerHPChanged?.Invoke(playerHP);
        //onPlayerShieldChanged?.Invoke(playerShieldPoints);
        //onPlayerShieldCountChanged?.Invoke(playerShields);
    }

    public void DamageEnemy(string action)
    {
        //int damage = 0;
        //if (action == "grenade") damage = 30;
        //if (action == "bullet") damage = 10;
        //if (action == "special") damage = 10;
        //if (isEnemyShieldUp && enemyShields > 0)
        //{
        //    enemyShieldPoints -= damage;

        //    if (enemyShieldPoints <= 0)
        //    {
        //        int overflow = -enemyShieldPoints; // Same as Mathf.Abs(enemyShieldPoints)

        //        enemyShields -= 1;
        //        enemyHP -= overflow;
        //        isEnemyShieldUp = false;
        //        onEnemyShieldOverlayToggled?.Invoke(false);
        //        if (enemyShields > 0) // shield bar will show next shield health; i.e 30
        //        {
        //            enemyShieldPoints = 30;
        //        }
        //        else // last shield or no shields
        //        {
        //            enemyShieldPoints = 0;
        //        }
        //    }
        //}
        //else
        //{
        //    enemyHP -= damage;
        //}

        //if (enemyHP <= 0) // enemy dies, player scores
        //{
        //    RespawnEnemy();
        //    playerScore += 1;
        //    onPlayerScoreChanged?.Invoke(playerScore);
        //}
        //onEnemyHPChanged?.Invoke(enemyHP);
        //onEnemyShieldChanged?.Invoke(enemyShieldPoints);
        //onEnemyShieldCountChanged?.Invoke(enemyShields);

    }


    // Resets player stats
    public void RespawnPlayer()
    {
        CurrentGameState.Player.HP = 100;
        CurrentGameState.Player.ShieldPoints = 0;
        CurrentGameState.Player.Shields = 3;
        CurrentGameState.Player.Grenades = 2;
        CurrentGameState.Player.Bullets = 6;
        PlayerStateUpdated?.Invoke();
       
    }

    public void RespawnEnemy()
    {   
        CurrentGameState.Enemy.HP = 100;
        CurrentGameState.Enemy.ShieldPoints = 0;
        CurrentGameState.Enemy.Shields = 3;
        CurrentGameState.Enemy.Grenades = 2;
        CurrentGameState.Enemy.Bullets = 6;
        EnemyStateUpdated?.Invoke();
        
    }

    public void UpdateGameState()     
    {   
        PlayerStateUpdated?.Invoke();
        EnemyStateUpdated?.Invoke();

    }

    public void PlayerShoot()
    {   
        if (CurrentGameState.Player.Bullets > 0)
        {
            //CurrentGameState.Player.Bullets -= 1;
            //CurrentGameState.Player.IsReloading = false;
            PlayerStateUpdated?.Invoke();
            onPlayerShoot?.Invoke();
            DamageEnemy("bullet"); // special logic;                
        }
       
    }

    public void EnemyShoot()
    {
        if (CurrentGameState.Enemy.Bullets > 0)
        {
            //CurrentGameState.Enemy.Bullets -= 1;
            EnemyStateUpdated?.Invoke();
            DamagePlayer("bullet"); // special logic;                
        }


        //if (enemyBullets > 0)
        //{
        //    DamagePlayer("bullet");
        //    enemyBullets -= 1;

        //    onEnemyBulletCountChanged?.Invoke(enemyBullets);
        //}
    }

    public void PlayerGrenade()
    {
        if (CurrentGameState.Player.Grenades > 0)
        {
            CurrentGameState.Player.Grenades -= 1;
            CurrentGameState.Player.IsReloading = false;
            PlayerStateUpdated?.Invoke();
            onPlayerGrenadeThrow?.Invoke();
            DamageEnemy("grenade"); // special logic;                
        }     
    }

    // helper function for damage
    private void GrenadeDamageEnemy()
    {
        if (arTrackingHandler.canSeeEnemy)
        {
            DamageEnemy("grenade");
        }          
    }

    public void PlayerHammer()
    {
        if (CurrentGameState.Player.IsSpecialActive) return;

        CurrentGameState.Player.IsSpecialActive = true;

        onPlayerHammerThrow?.Invoke();
        
        if (arTrackingHandler.canSeeEnemy)
        {
            Invoke("PlayerSpecialDamage", 1.5f); 
        }

        CurrentGameState.Player.IsSpecialActive = false;
    }

    public void PlayerSpear()
    {
        if (CurrentGameState.Player.IsSpecialActive) return;

        CurrentGameState.Player.IsSpecialActive = true;

        onPlayerSpearThrow?.Invoke();

        if (arTrackingHandler.canSeeEnemy)
        {
            Invoke("PlayerSpecialDamage", 1.5f);
        }

        CurrentGameState.Player.IsSpecialActive = false;
    }

    public void PlayerPortal()
    {
        if (CurrentGameState.Player.IsSpecialActive) return;

        CurrentGameState.Player.IsSpecialActive = true;

        onPlayerPortal?.Invoke();

        if (arTrackingHandler.canSeeEnemy)
        {
            Invoke("PlayerSpecialDamage", 1.5f);
        }

        CurrentGameState.Player.IsSpecialActive = false;
    }

    public void PlayerWeb()
    {
        if (CurrentGameState.Player.IsSpecialActive) return;

        CurrentGameState.Player.IsSpecialActive = true;

        onPlayerWeb?.Invoke();

        if (arTrackingHandler.canSeeEnemy)
        {
            Invoke("PlayerSpecialDamage", 1.5f);
        }

        CurrentGameState.Player.IsSpecialActive = false;
    }

    public void PlayerFist()
    {
        if (CurrentGameState.Player.IsSpecialActive) return;

        CurrentGameState.Player.IsSpecialActive = true;

        onPlayerFist?.Invoke();

        if (arTrackingHandler.canSeeEnemy)
        {
            Invoke("PlayerSpecialDamage", 1.5f);
        }

        CurrentGameState.Player.IsSpecialActive = false;
    }
    
    private void PlayerSpecialDamage()
    {
        if (arTrackingHandler.canSeeEnemy)
        {
            DamageEnemy("special");
        }       
    }

    public void TriggerPlayerSpecialAttack()
    {
        if (!CurrentGameState.Player.IsSpecialActive)
        {
            StartCoroutine(PlayerSpecialAttack());
        }
    }

    

    IEnumerator PlayerSpecialAttack()
    {   
        CurrentGameState.Player.IsSpecialActive = true;

        yield return new WaitForSeconds(1);
        DamageEnemy("special");

        CurrentGameState.Player.IsSpecialActive = false;
    }


    public void TriggerPlayerReload()
    {
        //if (CurrentGameState.Player.Bullets == 0 && !CurrentGameState.Player.IsReloading)
        //{
        //    StartCoroutine(PlayerReload());
        //}
        StartCoroutine(PlayerReload());
    }
    IEnumerator PlayerReload()
    {   
        CurrentGameState.Player.IsReloading = true;

        onPlayerReload?.Invoke();
        yield return new WaitForSeconds(2);
        CurrentGameState.Player.Bullets = 6;
        PlayerStateUpdated?.Invoke();

        CurrentGameState.Player.IsReloading = false;
    }

    public void PlayerLogout()
    {
        onPlayerLogout?.Invoke();
    }

}
