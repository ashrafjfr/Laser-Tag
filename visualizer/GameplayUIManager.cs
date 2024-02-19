using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameplayUIManager : MonoBehaviour
{
    public ActionHandler actionHandler;
    [Header("LEFT CANVAS")]
    public Text leftDetectedActionText;
    [Header("Connection Statuses")]
    public Text leftGunConnectionStatusText;
    public Text leftVestConnectionStatusText;
    public Text leftGloveConnectionStatusText;

    [Header("Scores")]
    public Text leftPlayerScoreText;
    public Text leftEnemyScoreText;


    [Header("Player Health Bar Components")]
    public Image leftPlayerHPBar;
    public Text leftPlayerHPText;

    [Header("Player Shield Bar Components")]
    public Image leftPlayerShieldBar;
    public Text leftPlayerShieldText;
    
    [Header("Player Equipment")]
    //public Text leftPlayerShieldCount;
    public Image[] leftPlayerShieldIcons;
    //public Text leftPlayerGrenadeCountText;
    public Image[] leftPlayerGrenadeIcons;
    public Text leftPlayerBulletCountText;

    public Text leftReloadingText;
    public Image leftPlayerShieldOverlay;

    public Text leftLogoutText;


    [Header("Enemy Health Bar Components")]
    public Image leftEnemyHPBar;
    public Text leftEnemyHPText;

    [Header("Enemy Shield Bar Components")]
    public Image leftEnemyShieldBar;
    public Text leftEnemyShieldText;
    
    [Header("Enemy Equipment")]
    //public Text leftEnemyShieldCount;
    public Image[] leftEnemyShieldIcons;
    //public Text leftEnemyGrenadeCountText;
    public Image[] leftEnemyGrenadeIcons;
    public Text leftEnemyBulletCountText;

    [Header("RIGHT CANVAS")]
    public Text rightDetectedActionText;

    [Header("Connection Statuses")]
    public Text rightGunConnectionStatusText;
    public Text rightVestConnectionStatusText;
    public Text rightGloveConnectionStatusText;


    [Header("Scores")]
    public Text rightPlayerScoreText;
    public Text rightEnemyScoreText;


    [Header("Player Health Bar Components")]
    public Image rightPlayerHPBar;
    public Text rightPlayerHPText;

    [Header("Player Shield Bar Components")]
    public Image rightPlayerShieldBar;
    public Text rightPlayerShieldText;

    [Header("Player Equipment")]
    //public Text rightPlayerShieldCount;
    public Image[] rightPlayerShieldIcons;
    //public Text rightPlayerGrenadeCountText;
    public Image[] rightPlayerGrenadeIcons;
    public Text rightPlayerBulletCountText;

    public Text rightReloadingText;
    public Image rightPlayerShieldOverlay;

    public Text rightLogoutText;


    [Header("Enemy Health Bar Components")]
    public Image rightEnemyHPBar;
    public Text rightEnemyHPText;

    [Header("Enemy Shield Bar Components")]
    public Image rightEnemyShieldBar;
    public Text rightEnemyShieldText;

    [Header("Enemy Equipment")]
    //public Text rightEnemyShieldCount;
    public Image[] rightEnemyShieldIcons;
    //public Text rightEnemyGrenadeCountText;
    public Image[] rightEnemyGrenadeIcons;
    public Text rightEnemyBulletCountText;

    public GameObject enemyShieldOverlay;



    [Header("AR Effects")]
    public Camera ARCamera_left;
    public Camera ARCamera_right;
    public GameObject enemyTarget;
    public ARTrackingHandler arTrackingHandler;

    public GameObject muzzleFlashPrefab;
    public GameObject bulletImpactPrefab;

    public GameObject grenadePrefab;
    public GameObject grenadeExplosionPrefab;

    public GameObject hammerPrefab;
    public GameObject hammerHitPrefab;

    public GameObject spearPrefab;
    public GameObject spearHitPrefab;

    public GameObject portalPrefab;
    public GameObject portalHitPrefab;

    public GameObject fistPrefab; //it's really more of an energy ball
    public GameObject fistHitPrefab;

    public GameObject webPrefab;
    public GameObject webHitPrefab;

    // Define colors for active and faded icons
    public Color activeColor = Color.white; // Full color for active icons
    public Color fadedColor = new Color(1, 1, 1, 0.00f); //  for faded icons

    public Color connectedColor = new Color(0.1f, 0.9f, 0.1f, 1); // RGBA for green is (0, 1, 0, 1)
    public Color disconnectedColor = new Color(0.9f, 0.1f, 0.1f, 1); // RGBA for red is (1, 0, 0, 1)

    public void OnEnable()
    {
        actionHandler.PlayerStateUpdated += UpdatePlayerUI;
        actionHandler.EnemyStateUpdated += UpdateEnemyUI;

        actionHandler.onPlayerShoot += TriggerPlayerShoot;
        actionHandler.onPlayerReload += TriggerPlayerReload;
        actionHandler.onPlayerGrenadeThrow += TriggerPlayerGrenadeAnimation;
        actionHandler.onPlayerHammerThrow += TriggerPlayerHammerAnimation;
        actionHandler.onPlayerSpearThrow += TriggerPlayerSpearAnimation;
        actionHandler.onPlayerPortal += TriggerPlayerPortalAnimation;
        actionHandler.onPlayerFist += TriggerPlayerFistAnimation;
        actionHandler.onPlayerWeb += TriggerPlayerWebAnimation;

        actionHandler.onPlayerLogout += ShowLogoutText;
      
    }

    public void OnDisable()
    {
        actionHandler.PlayerStateUpdated -= UpdatePlayerUI;
        actionHandler.EnemyStateUpdated -= UpdateEnemyUI;

        actionHandler.onPlayerShoot -= TriggerPlayerShoot;
        actionHandler.onPlayerReload -= TriggerPlayerReload;
        actionHandler.onPlayerGrenadeThrow -= TriggerPlayerGrenadeAnimation;
        actionHandler.onPlayerHammerThrow -= TriggerPlayerHammerAnimation;
        actionHandler.onPlayerSpearThrow -= TriggerPlayerSpearAnimation;
        actionHandler.onPlayerPortal -= TriggerPlayerPortalAnimation;
        actionHandler.onPlayerFist -= TriggerPlayerFistAnimation;
        actionHandler.onPlayerWeb -= TriggerPlayerWebAnimation;

        actionHandler.onPlayerLogout -= ShowLogoutText;
     
    }

    public void ShowDetectedAction(string action)
    {
        // Code to show the last detected action for 5s
        string text_to_show = "DETECTED: " + action.ToUpper();
        leftDetectedActionText.text = text_to_show;
        rightDetectedActionText.text =  text_to_show;

        StartCoroutine(ShowDetectedActionAnimation());
    }

    public IEnumerator ShowDetectedActionAnimation()
    {
        leftDetectedActionText.color = new Color(0.7f, 0.7f, 0.9f, 1);  // Set to visible
        rightDetectedActionText.color = new Color(0.7f, 0.7f, 0.9f, 1);  // Set to visible

        yield return new WaitForSeconds(5f);

        leftDetectedActionText.color = new Color(0.7f, 0.7f, 0.9f, 0);  // Set to invisible
        rightDetectedActionText.color = new Color(0.7f, 0.7f, 0.9f, 0);  // Set to invisible
    }

    public void UpdateGunConnectionStatus(bool is_connected)
    {
        
        //leftGunConnectionStatusText.text = is_connected ? "GUN IS CONNECTED" : "GUN IS DISCONNECTED";
        //rightGunConnectionStatusText.text = is_connected ? "GUN IS CONNECTED" : "GUN IS DISCONNECTED";

        // Set color to green if connected, red if disconnected
        leftGunConnectionStatusText.color = is_connected ? connectedColor : disconnectedColor;
        rightGunConnectionStatusText.color = is_connected ? connectedColor : disconnectedColor;
    }

    public void UpdateVestConnectionStatus(bool is_connected)
    {
        //leftVestConnectionStatusText.text = is_connected ? "VEST IS CONNECTED" : "VEST IS DISCONNECTED";
        //rightVestConnectionStatusText.text = is_connected ? "VEST IS CONNECTED" : "VEST IS DISCONNECTED";

        leftVestConnectionStatusText.color = is_connected ? connectedColor : disconnectedColor;  // Set to green if connected, red if disconnected
        rightVestConnectionStatusText.color = is_connected ? connectedColor : disconnectedColor;  // Set to green if connected, red if disconnected
    }

    public void UpdateGloveConnectionStatus(bool is_connected)
    {
        //leftGloveConnectionStatusText.text = is_connected ? "GLOVE IS CONNECTED" : "GLOVE IS DISCONNECTED";
        //rightGloveConnectionStatusText.text = is_connected ? "GLOVE IS CONNECTED" : "GLOVE IS DISCONNECTED";

        leftGloveConnectionStatusText.color = is_connected ? connectedColor : disconnectedColor;  // Set to green if connected, red if disconnected
        rightGloveConnectionStatusText.color = is_connected ? connectedColor : disconnectedColor;  // Set to green if connected, red if disconnected
    }

    public void UpdatePlayerUI()
    {
        // Code to update the visual representation of the player's HP
        int playerHP = actionHandler.CurrentGameState.Player.HP;
        leftPlayerHPText.text = playerHP.ToString(); 
        leftPlayerHPBar.fillAmount = (float)playerHP / 100.0f;
        rightPlayerHPText.text = playerHP.ToString();
        rightPlayerHPBar.fillAmount = (float)playerHP / 100.0f;

        // Code to update the visual representation of the player's shield
        int playerShield = actionHandler.CurrentGameState.Player.ShieldPoints;
        leftPlayerShieldText.text = playerShield.ToString();
        leftPlayerShieldBar.fillAmount = (float)playerShield / 30.0f;
        leftPlayerShieldOverlay.gameObject.SetActive(playerShield > 0);
        rightPlayerShieldText.text = playerShield.ToString();
        rightPlayerShieldBar.fillAmount = (float)playerShield / 30.0f;
        rightPlayerShieldOverlay.gameObject.SetActive(playerShield > 0);

        // Code to update the visual representation of the player's shield count
        int playerShields = actionHandler.CurrentGameState.Player.Shields;
        //leftPlayerShieldCount.text = "Shields: " + playerShields.ToString();
        //rightPlayerShieldCount.text = "Shields: " + playerShields.ToString();

        // Update left player shield icons
        for (int i = 0; i < leftPlayerShieldIcons.Length; i++)
        {
            //leftPlayerShieldIcons[i].color = i < playerShields ? activeColor : fadedColor;
            leftPlayerShieldIcons[i].enabled = i < playerShields;
        }

        // Update right player shield icons
        for (int i = 0; i < rightPlayerShieldIcons.Length; i++)
        {
            //rightPlayerShieldIcons[i].color = i < playerShields ? activeColor : fadedColor;
            rightPlayerShieldIcons[i].enabled = i < playerShields;
        }


        // Code to update the visual representation of the player's grenade count
        int playerGrenades = actionHandler.CurrentGameState.Player.Grenades;
        //leftPlayerGrenadeCountText.text = "Grenades: " + playerGrenades.ToString();
        //rightPlayerGrenadeCountText.text = "Grenades: " + playerGrenades.ToString();

        // Update left player grenade icons
        for (int i = 0; i < leftPlayerGrenadeIcons.Length; i++)
        {
            //leftPlayerGrenadeIcons[i].color = i < playerGrenades ? activeColor : fadedColor;
            leftPlayerGrenadeIcons[i].enabled = i < playerGrenades;
        }

        // Update right player grenade icons
        for (int i = 0; i < rightPlayerGrenadeIcons.Length; i++)
        {
            //rightPlayerGrenadeIcons[i].color = i < playerGrenades ? activeColor : fadedColor;
            rightPlayerGrenadeIcons[i].enabled = i < playerGrenades;
        }



        // Code to update the visual representation of the player's bullet count
        int playerBullets = actionHandler.CurrentGameState.Player.Bullets;
        leftPlayerBulletCountText.text = "Bullets: " + playerBullets.ToString() + " / 6";
        rightPlayerBulletCountText.text = "Bullets: " + playerBullets.ToString() + " / 6";

        // Code to update the visual representation of the player's score
        //int playerScore = actionHandler.CurrentGameState.Player.Score;
        int playerScore = actionHandler.CurrentGameState.Enemy.Deaths;
        leftPlayerScoreText.text = playerScore.ToString();
        rightPlayerScoreText.text = playerScore.ToString();
   

    }

    public void UpdateEnemyUI()
    {
        // Code to update the visual representation of the enemy's HP
        int enemyHP = actionHandler.CurrentGameState.Enemy.HP;
        leftEnemyHPText.text = enemyHP.ToString();
        leftEnemyHPBar.fillAmount = (float)enemyHP / 100.0f;
        rightEnemyHPText.text = enemyHP.ToString();
        rightEnemyHPBar.fillAmount = (float)enemyHP / 100.0f;

        // Code to update the visual representation of the enemy's shield
        int enemyShield = actionHandler.CurrentGameState.Enemy.ShieldPoints;
        leftEnemyShieldText.text = enemyShield.ToString();
        leftEnemyShieldBar.fillAmount = (float)enemyShield / 30.0f;
        enemyShieldOverlay.gameObject.SetActive(enemyShield > 0);
        rightEnemyShieldText.text = enemyShield.ToString();
        rightEnemyShieldBar.fillAmount = (float)enemyShield / 30.0f;

        // Code to update the visual representation of the enemy's shield count
        int enemyShields = actionHandler.CurrentGameState.Enemy.Shields;
        //leftEnemyShieldCount.text = "Shields: " + enemyShields.ToString();
        //rightEnemyShieldCount.text = "Shields: " + enemyShields.ToString();

        // Update left enemy shield icons
        for (int i = 0; i < leftEnemyShieldIcons.Length; i++)
        {
            //leftEnemyShieldIcons[i].color = i < enemyShields ? activeColor : fadedColor;
            leftEnemyShieldIcons[i].enabled = i < enemyShields;
        }

        // Update right enemy shield icons
        for (int i = 0; i < rightEnemyShieldIcons.Length; i++)
        {
            //rightEnemyShieldIcons[i].color = i < enemyShields ? activeColor : fadedColor;
            rightEnemyShieldIcons[i].enabled = i < enemyShields;
        }

        // Code to update the visual representation of the enemy's grenade count
        int enemyGrenades = actionHandler.CurrentGameState.Enemy.Grenades;
        //leftEnemyGrenadeCountText.text = "Grenades: " + enemyGrenades.ToString();
        //rightEnemyGrenadeCountText.text = "Grenades: " + enemyGrenades.ToString();

        // Update left enemy grenade icons
        for (int i = 0; i < leftEnemyGrenadeIcons.Length; i++)
        {
            //leftEnemyGrenadeIcons[i].color = i < enemyGrenades ? activeColor : fadedColor;
            leftEnemyGrenadeIcons[i].enabled = i < enemyGrenades;
        }

        // Update right enemy grenade icons
        for (int i = 0; i < rightEnemyGrenadeIcons.Length; i++)
        {
            //rightEnemyGrenadeIcons[i].color = i < enemyGrenades ? activeColor : fadedColor;
            rightEnemyGrenadeIcons[i].enabled = i < enemyGrenades;
        }

        // Code to update the visual representation of the enemy's bullet count
        int enemyBullets = actionHandler.CurrentGameState.Enemy.Bullets;
        leftEnemyBulletCountText.text = "Bullets: " + enemyBullets.ToString() + " / 6";
        rightEnemyBulletCountText.text = "Bullets: " + enemyBullets.ToString() + " / 6";

        // Code to update the visual representation of the enemy's score
        //int enemyScore = actionHandler.CurrentGameState.Enemy.Score;
        int enemyScore = actionHandler.CurrentGameState.Player.Deaths;
        leftEnemyScoreText.text = enemyScore.ToString();
        rightEnemyScoreText.text = enemyScore.ToString();


    }


    // AR EFFECTS SECTION! 

    // Function for muzzle flash, and bullet impact
    public void TriggerPlayerShoot()
    {
        StopCoroutine(PlayerShootAnimation());
        StartCoroutine(PlayerShootAnimation());
    }

    // Coroutine for muzzle flash, and bullet impact
    // CURRENTLY WRONG! Damage does not depend on visibility
    public IEnumerator PlayerShootAnimation()
    {
        // Muzzle Flash
        Vector3 flashPos = ARCamera_left.ScreenToWorldPoint(new Vector3(Screen.width * 0.7f, Screen.height * 0.1f, ARCamera_left.nearClipPlane));
        GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, flashPos, Quaternion.identity);
        muzzleFlash.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        Destroy(muzzleFlash, 0.2f);

        // Bullet impact
        if (arTrackingHandler.canSeeEnemy)
        {
            Vector3 impactPos = enemyTarget.transform.position;
            GameObject bulletImpact = Instantiate(bulletImpactPrefab, impactPos, Quaternion.identity);
            bulletImpact.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            Destroy(bulletImpact, 0.4f);
        }
        yield return null;
    }

    // Function for Grenade Throw Animation
    public void TriggerPlayerGrenadeAnimation()
    {     
        StopCoroutine(PlayerGrenadeAnimation());
        StartCoroutine(PlayerGrenadeAnimation());
    }

    // Coroutine for Grenade Throw Animation
    public IEnumerator PlayerGrenadeAnimation()
    {
        Vector3 startPos = ARCamera_left.ScreenToWorldPoint(new Vector3(Screen.width * 0.9f, Screen.height * 0.1f, ARCamera_left.nearClipPlane));
        GameObject grenade = Instantiate(grenadePrefab, startPos, Quaternion.identity);
        grenade.transform.rotation = Quaternion.Euler(0, 0, 0); // or whatever orientation you want
        //grenade.transform.LookAt(enemyTarget.transform.position);

        grenade.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);      

        if (arTrackingHandler.canSeeEnemy)
        {
            Debug.Log("Can see enemy now!");
            Debug.Log("Grenade rotation after seeing enemy " + grenade.transform.rotation);

            //Vector3 endPos = enemyTarget.transform.position;

            float t = 0;
            float duration = 1.5f;  // Time in seconds to reach the target

            while (t < 1)
            {
                t += Time.deltaTime / duration;
                //grenade.transform.position = Vector3.Lerp(startPos, endPos, t);
                grenade.transform.position = Vector3.Lerp(startPos, enemyTarget.transform.position, t);
                yield return null;
            }
        }
        else
        {
            Debug.Log("Cannot see enemy now!");
            Debug.Log("Grenade rotation after not seeing enemy " + grenade.transform.rotation);

            Vector3 flyDirection = ARCamera_left.transform.forward;
            float speed = 1.5f;
            float step = speed * Time.deltaTime;
            //float distanceCovered = 0f;
            float timeElapsed = 0f;
            float maxTime = 1.5f;

            while (timeElapsed < maxTime)
            {
                Vector3 nextPos = grenade.transform.position + (flyDirection * step);
                grenade.transform.position = nextPos;
                //distanceCovered += step;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Add code for explosion effect here      
        GameObject explosion = Instantiate(grenadeExplosionPrefab, grenade.transform.position, Quaternion.identity);
        Destroy(grenade);
        Destroy(explosion, 2f);
    }

    // Function for Hammer Throw Animation
    public void TriggerPlayerHammerAnimation()
    {
        StopCoroutine(PlayerHammerAnimation());
        StartCoroutine(PlayerHammerAnimation());
    }

    // Coroutine for Hammer Throw Animation
    public IEnumerator PlayerHammerAnimation()
    {
        Vector3 startPos = ARCamera_left.ScreenToWorldPoint(new Vector3(Screen.width * 0.9f, Screen.height * 0.1f, ARCamera_left.nearClipPlane));
        GameObject hammer = Instantiate(hammerPrefab, startPos, Quaternion.identity);
        
        //hammer.transform.rotation = Quaternion.Euler(90, 0, 0); // phone rotation
        hammer.transform.rotation = Quaternion.Euler(180, 0, 0); // PC rotation
        Quaternion initialRotation = hammer.transform.rotation;
        hammer.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        

        if (arTrackingHandler.canSeeEnemy)
        {
                     
            //Vector3 endPos = enemyTarget.transform.position;           
            float t = 0;
            float duration = 1.5f;  // Time in seconds to reach the target

            while (t < 1)
            {
                t += Time.deltaTime / duration;
                hammer.transform.position = Vector3.Lerp(startPos, enemyTarget.transform.position, t);
                hammer.transform.rotation = initialRotation;
                yield return null;
            }

        }
        else
        {         
            Vector3 flyDirection = ARCamera_left.transform.forward;
            float speed = 1.5f;
            float step = speed * Time.deltaTime;
            //float distanceCovered = 0f;
            float timeElapsed = 0f;
            float maxTime = 1.5f;

            while (timeElapsed < maxTime)
            {
                Vector3 nextPos = hammer.transform.position + (flyDirection * step);
                hammer.transform.position = nextPos;
                hammer.transform.rotation = initialRotation;
                //distanceCovered += step;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Add code for explosion effect here
        GameObject hammerHit = Instantiate(hammerHitPrefab, hammer.transform.position, Quaternion.identity);
        Destroy(hammer);
        Destroy(hammerHit, 1f);
    }

    // Function for Spear Throw Animation
    public void TriggerPlayerSpearAnimation()
    {
        StopCoroutine(PlayerSpearAnimation());
        StartCoroutine(PlayerSpearAnimation());
    }

    // Coroutine for Spear Throw Animation
    public IEnumerator PlayerSpearAnimation()
    {
        Vector3 startPos = ARCamera_left.ScreenToWorldPoint(new Vector3(Screen.width * 0.9f, Screen.height * 0.1f, ARCamera_left.nearClipPlane));
        GameObject spear = Instantiate(spearPrefab, startPos, Quaternion.identity);
        //spear.transform.rotation = Quaternion.Euler(90, 0, 0); // phone rotation
        spear.transform.rotation = Quaternion.Euler(180, 0, 0); // PC rotation
        Quaternion initialRotation = spear.transform.rotation;
        spear.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        if (arTrackingHandler.canSeeEnemy)
        {
            //Vector3 endPos = enemyTarget.transform.position;          

            float t = 0;
            float duration = 1.5f;  // Time in seconds to reach the target

            while (t < 1)
            {
                t += Time.deltaTime / duration;
                spear.transform.position = Vector3.Lerp(startPos, enemyTarget.transform.position, t);
                spear.transform.rotation = initialRotation;
                yield return null;
            }
        }
        else
        {
            Debug.Log("Cannot see enemy now!");

            Vector3 flyDirection = ARCamera_left.transform.forward;
            float speed = 1.5f;
            float step = speed * Time.deltaTime;
            //float distanceCovered = 0f;
            float timeElapsed = 0f;
            float maxTime = 1.5f;

            while (timeElapsed < maxTime)
            {
                Vector3 nextPos = spear.transform.position + (flyDirection * step);
                spear.transform.position = nextPos;
                spear.transform.rotation = initialRotation;
                //distanceCovered += step;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Add code for explosion effect here
        GameObject spearHit = Instantiate(spearHitPrefab, spear.transform.position, Quaternion.identity);
        Destroy(spear);
        Destroy(spearHit, 1f);
    }

    // Function for Portal Animation
    public void TriggerPlayerPortalAnimation()
    {
        StopCoroutine(PlayerPortalAnimation());
        StartCoroutine(PlayerPortalAnimation());
    }

    // Coroutine for Portal Animation
    public IEnumerator PlayerPortalAnimation()
    {
        Vector3 startPos = ARCamera_left.ScreenToWorldPoint(new Vector3(Screen.width * 0.9f, Screen.height * 0.1f, ARCamera_left.nearClipPlane));
        GameObject portal = Instantiate(portalPrefab, startPos, Quaternion.identity);
        //portal.transform.rotation = Quaternion.Euler(0, 0, 0); // phone rotation
        portal.transform.rotation = Quaternion.Euler(90, 0, 0); // PC rotation
        Quaternion initialRotation = portal.transform.rotation;

        portal.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        if (arTrackingHandler.canSeeEnemy)
        {
            
            //Vector3 endPos = enemyTarget.transform.position;

            float t = 0;
            float duration = 1.5f;  // Time in seconds to reach the target

            while (t < 1)
            {
                t += Time.deltaTime / duration;
                portal.transform.position = Vector3.Lerp(startPos, enemyTarget.transform.position, t);
                portal.transform.rotation = initialRotation;
                yield return null;
            }
        }
        else
        {
            Debug.Log("Cannot see enemy now!");

            Vector3 flyDirection = ARCamera_left.transform.forward;
            float speed = 1.5f;
            float step = speed * Time.deltaTime;
            //float distanceCovered = 0f;
            float timeElapsed = 0f;
            float maxTime = 1.5f;

            while (timeElapsed < maxTime)
            {
                Vector3 nextPos = portal.transform.position + (flyDirection * step);
                portal.transform.position = nextPos;
                portal.transform.rotation = initialRotation;
                //distanceCovered += step;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Add code for explosion effect here
        GameObject portalHit = Instantiate(portalHitPrefab, portal.transform.position, Quaternion.identity);
        Destroy(portal);
        Destroy(portalHit, 1f);
    }

    // Function for Fist Animation
    public void TriggerPlayerFistAnimation()
    {
        StopCoroutine(PlayerFistAnimation());
        StartCoroutine(PlayerFistAnimation());
    }

    // Coroutine for Fist Animation
    public IEnumerator PlayerFistAnimation()
    {
        Vector3 startPos = ARCamera_left.ScreenToWorldPoint(new Vector3(Screen.width * 0.9f, Screen.height * 0.1f, ARCamera_left.nearClipPlane));
        GameObject fist = Instantiate(fistPrefab, startPos, Quaternion.identity);
        fist.transform.rotation = Quaternion.Euler(90, 0, 0); // or whatever orientation you want

        fist.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        if (arTrackingHandler.canSeeEnemy)
        {
            Debug.Log("Can see enemy now!");

            //Vector3 endPos = enemyTarget.transform.position;

            float t = 0;
            float duration = 1.5f;  // Time in seconds to reach the target

            while (t < 1)
            {
                t += Time.deltaTime / duration;
                fist.transform.position = Vector3.Lerp(startPos, enemyTarget.transform.position, t);
                yield return null;
            }
        }
        else
        {
            Debug.Log("Cannot see enemy now!");

            Vector3 flyDirection = ARCamera_left.transform.forward;
            float speed = 1.5f;
            float step = speed * Time.deltaTime;
            //float distanceCovered = 0f;
            float timeElapsed = 0f;
            float maxTime = 1.5f;

            while (timeElapsed < maxTime)
            {
                Vector3 nextPos = fist.transform.position + (flyDirection * step);
                fist.transform.position = nextPos;
                //distanceCovered += step;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Add code for explosion effect here
        GameObject fistHit = Instantiate(fistHitPrefab, fist.transform.position, Quaternion.identity);
        Destroy(fist);
        Destroy(fistHit, 1f);
    }
    
    // Function for Web Animation
    public void TriggerPlayerWebAnimation()
    {
        StopCoroutine(PlayerWebAnimation());
        StartCoroutine(PlayerWebAnimation());
    }

    // Coroutine for Web Animation
    public IEnumerator PlayerWebAnimation()
    {
        Vector3 startPos = ARCamera_left.ScreenToWorldPoint(new Vector3(Screen.width * 0.9f, Screen.height * 0.1f, ARCamera_left.nearClipPlane));
        GameObject web = Instantiate(webPrefab, startPos, Quaternion.identity);
        //web.transform.rotation = Quaternion.Euler(0, 0, 0); // phone rotation
        web.transform.rotation = Quaternion.Euler(90, 0, 0); // PC rotation
        Quaternion initialRotation = web.transform.rotation;

        web.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        if (arTrackingHandler.canSeeEnemy)
        {
            Debug.Log("Can see enemy now!");

            //Vector3 endPos = enemyTarget.transform.position;

            float t = 0;
            float duration = 1.5f;  // Time in seconds to reach the target

            while (t < 1)
            {
                t += Time.deltaTime / duration;
                web.transform.position = Vector3.Lerp(startPos, enemyTarget.transform.position, t);
                web.transform.rotation = initialRotation;
                yield return null;
            }
        }
        else
        {

            Vector3 flyDirection = ARCamera_left.transform.forward;
            float speed = 1.5f;
            float step = speed * Time.deltaTime;
            float timeElapsed = 0f;
            float maxTime = 1.5f;

            while (timeElapsed < maxTime)
            {
                Vector3 nextPos = web.transform.position + (flyDirection * step);
                web.transform.position = nextPos;
                web.transform.rotation = initialRotation;
                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Add code for explosion effect here
        GameObject webHit = Instantiate(webHitPrefab, web.transform.position, Quaternion.identity);
        Destroy(web);
        Destroy(webHit, 1f);
    }

    public void TriggerPlayerReload()
    {
        StartCoroutine(PlayerReloadAnimation());
    }

    public IEnumerator PlayerReloadAnimation()
    {
        // Code to play reload animation
        float duration = 2f;  // Total duration in seconds
        float singleFlashDuration = duration / 4f;  // We will flash twice, so four phases: on-off-on-off

        // First flash
        leftReloadingText.color = new Color(1, 1, 1, 1);  // Set to visible
        rightReloadingText.color = new Color(1, 1, 1, 1);  // Set to visible

        yield return new WaitForSeconds(singleFlashDuration);

        leftReloadingText.color = new Color(1, 1, 1, 0);  // Set to invisible
        rightReloadingText.color = new Color(1, 1, 1, 0);  // Set to invisible
        yield return new WaitForSeconds(singleFlashDuration);

        // Second flash
        leftReloadingText.color = new Color(1, 1, 1, 1);  // Set to visible
        rightReloadingText.color = new Color(1, 1, 1, 1);  // Set to visible
        yield return new WaitForSeconds(singleFlashDuration);
        leftReloadingText.color = new Color(1, 1, 1, 0);  // Set to invisible
        rightReloadingText.color = new Color(1, 1, 1, 0);  // Set to invisible
    }

    public void ShowLogoutText()
    {
        StartCoroutine(LogoutTextAnimation());
    }

    // FILLER ANIMATION
    public IEnumerator LogoutTextAnimation()
    {
        float duration = 2f;  // Total duration in seconds
        float singleFlashDuration = duration / 4f;  // We will flash twice, so four phases: on-off-on-off

        // First flash
        leftLogoutText.color = new Color(1, 1, 1, 1);  // Set to visible
        rightLogoutText.color = new Color(1, 1, 1, 1);  // Set to visible

        yield return new WaitForSeconds(singleFlashDuration);

        leftLogoutText.color = new Color(1, 1, 1, 0);  // Set to invisible
        rightLogoutText.color = new Color(1, 1, 1, 0);  // Set to invisible
        yield return new WaitForSeconds(singleFlashDuration);

        // Second flash
        leftLogoutText.color = new Color(1, 1, 1, 1);  // Set to visible
        rightLogoutText.color = new Color(1, 1, 1, 1);  // Set to visible

        yield return new WaitForSeconds(singleFlashDuration);

        leftLogoutText.color = new Color(1, 1, 1, 0);  // Set to invisible
        rightLogoutText.color = new Color(1, 1, 1, 0);  // Set to invisible
    }


    void Start()
    {
        leftPlayerShieldOverlay.gameObject.SetActive(false);
        rightPlayerShieldOverlay.gameObject.SetActive(false);
        enemyShieldOverlay.gameObject.SetActive(false);

        // Set connection statuses to discconnected
        leftGunConnectionStatusText.color = disconnectedColor;  // Set to visible
        rightGunConnectionStatusText.color = disconnectedColor;    // Set to visible

        leftVestConnectionStatusText.color = disconnectedColor;  // Set to visible
        rightVestConnectionStatusText.color = disconnectedColor;  // Set to visible

        leftGloveConnectionStatusText.color = disconnectedColor;  // Set to visible
        rightGloveConnectionStatusText.color = disconnectedColor;  // Set to visible

    }

    
    void Update()
    {
        
    }
}
