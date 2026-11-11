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

    private void Awake()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        bool retourCombat = BattleContext.HasReturnPosition
                            && BattleContext.ReturnSceneName == SceneManager.GetActiveScene().name;

        Vector3 pos = retourCombat ? BattleContext.PlayerReturnPosition : transform.position;
        pos.z = 0f;
        player.position = pos;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = pos;
    }

    // Petit cercle vert visible dans la fenêtre Scene
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}