using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using EnumManager;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;
    private string playerName;

    private int life;
    public int maxLife = 3;
    private int stage = 1;
    private Character character;
    private int score = 0;
    public Color[,] originColorSave = null;
    public int bossStageMaxLife = 5;

    private void Awake()
    {
        if (GameManager.inst == null)
        {
            GameManager.inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        life = maxLife;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void LoadStageSelection()
    {
        SceneManager.LoadScene("StageSelectionScene");
    }

    public void LoadCharacterSelection()
    {
        SceneManager.LoadScene("CharacterSelectionScene");
    }

    public void LoadMainStage()
    {
        SceneManager.LoadScene("MainStage" + stage + "Scene");
    }

    public void LoadBossStage()
    {
        SceneManager.LoadScene("BossStage" + stage + "Scene");
    }

    public void LoadLeaderboard()
    {
        SceneManager.LoadScene("LeaderBoardScene");
    }

    public void ResetStats()
    {
        life = maxLife;
        score = 0;
    }

    public string GetPlayerName()
    {
        return playerName;
    }
    public void SetPlayerName()
    {
        playerName = GameObject.FindWithTag("PlayerName").GetComponent<TextMeshProUGUI>().text;
    }

    public int GetStage()
    {
        return stage;
    }
    public void SetStage(int _stage)
    {
        stage = _stage;
    }

    public Character GetCharacter()
    {
        return character;
    }
    public void SetCharacter(Character _character)
    {
        character = _character;
        originColorSave = null;
    }

    public int GetLife()
    {
        return life;
    }
    public void RemoveLife()
    {
        if (life > 0)
            life--;
    }
    public void AddLife(int _maxLife)
    {
        if (life < _maxLife)
            life++;
    }

    public void AddScore(int _score)
    {
        score += _score;
        if (score < 0) score = 0;
    }
    public int GetScore()
    {
        return score;
    }
    public IEnumerator DeactivateLivesAndAddScore(GameObject[] hearts, AudioClip heartDeactivateSound, float soundVolume )
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i].activeSelf) // 활성화된 Life만 처리
            {
                // Life 비활성화
                hearts[i].SetActive(false);
                // 점수 추가
                GameManager.inst.AddScore(1000);
                // 효과음 재생
                if (heartDeactivateSound != null)
                {
                    AudioSource.PlayClipAtPoint(heartDeactivateSound, Camera.main.transform.position, soundVolume);
                }
                // 0.5초 대기 후 다음 Life 처리
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
