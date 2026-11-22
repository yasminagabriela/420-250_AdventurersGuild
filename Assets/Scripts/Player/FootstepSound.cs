using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    private const float StepInterval = 0.35f;
    private float _timer;

    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (!animator.GetBool("IsMoving")) return;

        if (_timer <= 0f)
        {
            audioSource.Play();
            _timer = StepInterval;
        }
    }
}
