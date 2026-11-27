using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.5f; // around how long the animation is 

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}