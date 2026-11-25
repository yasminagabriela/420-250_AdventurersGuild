using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    [SerializeField] private BattleTheme theme;
    [SerializeField] private string enemyId = "gloob_1";
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyPatrol patrol;
    [SerializeField] private float attackDelay = 0.6f;
    [SerializeField] private EnemyData enemyData;
 
    private bool _triggered;

    void Start()
    {
        if (BattleContext.DefeatedEnemies.Contains(enemyId))
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < BattleContext.IgnoreBattlesUntil) return;
        if (_triggered || !other.CompareTag("Player")) return;
        if (Time.timeSinceLevelLoad < BattleContext.TriggerGraceSeconds) return; // ajout Talyssa

        _triggered = true;
        StartCoroutine(StartBattle(other));
    }

    private IEnumerator StartBattle(Collider2D player)
    {
        if (patrol != null) patrol.Stop();
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        BattleContext.Enemy = enemyData;
        BattleContext.EnemyId = enemyId;
        BattleContext.Theme = theme;
        BattleContext.PlayerReturnPosition = player.transform.position;
        BattleContext.HasReturnPosition = true;
        BattleContext.ReturnSceneName = SceneManager.GetActiveScene().name;

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackDelay);

        SceneManager.LoadScene(battleSceneName);
    }
}
