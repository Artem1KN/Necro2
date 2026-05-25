using UnityEngine;

/// Spawns a fading LineRenderer between two points and self-destroys.
/// Add to a GameObject with a LineRenderer to use as a hitscan tracer.
[RequireComponent(typeof(LineRenderer))]
public class BulletTracer : MonoBehaviour
{
    public float lifeSeconds = 0.08f;
    public AnimationCurve widthCurve = AnimationCurve.EaseInOut(0f, 0.05f, 1f, 0f);

    private LineRenderer line;
    private float age;
    private float startWidth;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        startWidth = line.startWidth;
    }

    public void Setup(Vector3 from, Vector3 to)
    {
        if (line == null) line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, from);
        line.SetPosition(1, to);
    }

    private void Update()
    {
        age += Time.deltaTime;
        if (age >= lifeSeconds)
        {
            Destroy(gameObject);
            return;
        }

        float t = age / lifeSeconds;
        float w = startWidth * widthCurve.Evaluate(t);
        line.startWidth = w;
        line.endWidth = w;
    }
}
