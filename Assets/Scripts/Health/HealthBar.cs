using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private float barScale = 3f;
    private const string BattleSceneName = "BattleScene";

    // Position sur l'écran : (0, 0) = bas gauche, (1, 1) = haut droite
    private const float NormalX = 0.08f;
    private const float NormalY = 0.92f;
    private const float BattleX = 0.22f;
    private const float BattleY = 0.30f;
    private const float EnemyBattleX = 0.75f;
    private const float EnemyBattleY = 0.55f;

    [SerializeField] private HealthManager target;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private bool isEnemyBar = false;

    private Camera _camera;
    private float _screenX;
    private float _screenY;

    private void Awake()
    {
        _camera = Camera.main;
        spriteRenderer.sortingOrder = 100;
        transform.localScale = new Vector3(barScale, barScale, 1f);

        bool inBattle = SceneManager.GetActiveScene().name == BattleSceneName;

        if (inBattle && isEnemyBar)
        {
            _screenX = EnemyBattleX;
            _screenY = EnemyBattleY;
        }
        else if (inBattle)
        {
            _screenX = BattleX;
            _screenY = BattleY;
        }
        else
        {
            _screenX = NormalX;
            _screenY = NormalY;
        }
    }

    private void LateUpdate()
    {
        float distance = -_camera.transform.position.z;
        transform.position = _camera.ViewportToWorldPoint(
            new Vector3(_screenX, _screenY, distance));

        int index = Mathf.Clamp(target.currentHealth, 0, frames.Length - 1);
        spriteRenderer.sprite = frames[index];
    }
}
