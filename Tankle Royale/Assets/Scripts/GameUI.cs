using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI playerInfoText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI winText;
    public Image winBackground;
    public GameObject chatLog;
    public GameObject textPrefab;

    private PlayerController player;

    //instance
    public static GameUI instance;

    void Awake()
    {
        instance = this;
    }

    public void Initialize (PlayerController localPlayer)
    {
        player = localPlayer;
        healthBar.maxValue = player.maxHp;
        healthBar.value = player.curHp;

        UpdatePlayerInfoText();
        UpdateHealthBar();
        UpdateAmmoText();
    }

    public void UpdateHealthBar ()
    {
        healthBar.value = player.curHp;
    }

    public void UpdatePlayerInfoText ()
    {
        playerInfoText.text = "<b>Alive:</b> " + GameManager.instance.alivePlayers + "\n<b>Kills:</b> " + player.kills;
    }

    public void UpdateAmmoText ()
    {
        ammoText.text = player.weapon.curAmmo + " / " + player.weapon.maxAmmo;
    }

    public void SetWinText (string playerName)
    {
        winBackground.gameObject.SetActive(true);
        winText.text = playerName + " wins";
    }

    public void PlayerDied (string deadPlayer)
    {
        GameObject deadText = Instantiate(textPrefab, chatLog.transform.position, chatLog.transform.rotation, chatLog.transform);
        deadText.GetComponent<TextMeshProUGUI>().text = deadPlayer + " died.";
    }
}
