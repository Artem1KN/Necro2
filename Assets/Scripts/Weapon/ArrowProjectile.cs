using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 20f;
    public float lifeSpan = 5f; // Врьемя жизни стрелы, чтобы не засорять памят
    public float speed = 30f;
    
    private Rigidbody rb;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Запускаем полет вперед сразу при спавне
        rb.linearVelocity = transform.forward * speed;
        
        // Уничтожаем стрелу через N секунд, если она никуда не попала
        Destroy(gameObject, lifeSpan);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return; // Чтобы не срабатывало дважды

        // Проверяем, есть ли у цели компонент врага
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Debug.Log($"[Arrow] Hit {enemy.name} for {damage} damage!");
        }

        // Эффект попадания (можно добавить спавн частиц здесь)
        hasHit = true;
        
        // Стрела останавливается или втыкается в объект
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true; 

        // Удаляем стрелу через короткое время после попадания
        Destroy(gameObject, 0.5f);
    }
}
