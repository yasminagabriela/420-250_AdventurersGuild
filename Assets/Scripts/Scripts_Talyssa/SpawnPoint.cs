using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Point de départ du joueur dans le niveau.
/// Au lancement : le joueur apparaît ici.
/// Au retour d'un combat : il revient là où il était.
/// </summary>
[DefaultExecutionOrder(-100)]
public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private Transform player;

    // Exécuté une seule fois au début de chaque Play :
    // efface les vieilles valeurs de combat restées en mémoire.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetBattleMemory()
    {
        BattleContext.HasReturnPosition = false;
        BattleContext.PlayerReturnPosition = Vector3.zero;
        BattleContext.ReturnSceneName = null;
        BattleContext.EnemyId = null;
        BattleContext.TriggerGraceSeconds = 0f;
        BattleContext.DefeatedEnemies.Clear();
    }

    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        string scene = SceneManager.GetActiveScene().name;
        bool retourCombat = BattleContext.HasReturnPosition && BattleContext.ReturnSceneName == scene;

        // Position de retour d'une autre scène -> on l'ignore
        if (BattleContext.HasReturnPosition && !retourCombat)
            BattleContext.HasReturnPosition = false;

        Vector3 pos = retourCombat ? BattleContext.PlayerReturnPosition : transform.position;
        pos.z = 0f;
        player.position = pos;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = pos;

        Debug.Log($"[SpawnPoint] Joueur placé en {pos} (retour de combat : {retourCombat})");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}