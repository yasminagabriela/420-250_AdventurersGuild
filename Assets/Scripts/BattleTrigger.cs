using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    [SerializeField] private string enemyId = "gloob_1";
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyPatrol patrol;
    [SerializeField] private float attackDelay = 0.6f;

    private bool _triggered;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (BattleContext.DefeatedEnemies.Contains(enemyId))
            gameObject.SetActive(false);

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || !other.CompareTag("Player")) return;
        _triggered = true;
        StartCoroutine(StartBattle(other));
    }


    private IEnumerator StartBattle(Collider2D player)
    {
        if (patrol != null) patrol.Stop();
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        BattleContext.EnemyId = enemyId;
        BattleContext.PlayerReturnPosition = player.transform.position;
        BattleContext.HasReturnPosition = true;

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackDelay);

        SceneManager.LoadScene(battleSceneName);
    }
}
