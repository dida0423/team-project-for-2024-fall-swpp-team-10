using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MainStageManagerTest
{
    private MainStageManager mainStageManager;
    private GameObject gameManagerObject;
    private GameManager gameManager;

    [SetUp]
    public void Setup()
    {
        // Create a GameManager instance
        gameManagerObject = new GameObject("GameManager");
        gameManager = gameManagerObject.AddComponent<GameManager>();
        GameManager.inst = gameManager;

        // Load the test scene
        gameManager.LoadMainStage();
        Debug.Log(SceneManager.GetActiveScene().name);
        mainStageManager = GameObject.FindObjectOfType<MainStageManager>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(gameManagerObject);
        Object.Destroy(mainStageManager);
    }

    [UnityTest]
    public IEnumerator TestStageCompletesAfterDuration()
    {
        // Arrange
        mainStageManager.stageDuration = 2.0f;

        // Act
        yield return new WaitForSeconds(2.1f);

        // Assert
        Assert.IsTrue(mainStageManager.IsStageComplete());
    }

    [UnityTest]
    public IEnumerator TestBossSpawnsAtStageCompletion()
    {
        // Arrange
        mainStageManager.stageDuration = 2.0f;

        // Act
        yield return new WaitForSeconds(2.1f);

        // Assert
        Assert.IsNotNull(mainStageManager.boss);
        Assert.AreEqual(new Vector3(0, 13, mainStageManager.ActiveCharacter().transform.position.z + 3),
                        mainStageManager.boss.transform.position);
    }

    [UnityTest]
    public IEnumerator TestScoreAddsEverySecond()
    {
        // Arrange
        int initialScore = GameManager.inst.GetScore();

        // Act
        yield return new WaitForSeconds(3.1f);

        // Assert
        Assert.Greater(GameManager.inst.GetScore(), initialScore);
        Assert.AreEqual(initialScore + 300, GameManager.inst.GetScore());
    }

    [UnityTest]
    public IEnumerator TestPauseGameDisablesCharacterControls()
    {
        // Act
        mainStageManager.PauseGame();
        yield return null;

        // Assert
        Assert.IsFalse(mainStageManager.ActiveCharacter().GetComponent<MainStagePlayer>().enabled);
    }

    [UnityTest]
    public IEnumerator TestResumeGameEnablesCharacterControls()
    {
        // Arrange
        mainStageManager.PauseGame();
        yield return null;

        // Act
        mainStageManager.ResumeGame();
        yield return null;

        // Assert
        Assert.IsTrue(mainStageManager.ActiveCharacter().GetComponent<MainStagePlayer>().enabled);
    }
}
