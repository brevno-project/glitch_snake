using UnityEngine;

public class FoodVisualAnimator : MonoBehaviour
{
    [SerializeField] private float pulseAmplitude = 0.08f;
    [SerializeField] private float pulseSpeed = 5.5f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobSpeed = 4.2f;
    [SerializeField] private float spinSpeed = 40f;

    private Vector3 baseLocalScale;
    private Vector3 baseLocalPosition;
    private float randomTimeOffset;

    private void Awake()
    {
        baseLocalScale = transform.localScale;
        baseLocalPosition = transform.localPosition;
        randomTimeOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float animatedTime = Time.time + randomTimeOffset;

        float pulse = 1f + Mathf.Sin(animatedTime * Mathf.Max(0.01f, pulseSpeed)) * Mathf.Max(0f, pulseAmplitude);
        transform.localScale = baseLocalScale * pulse;

        float bobOffset = Mathf.Sin(animatedTime * Mathf.Max(0.01f, bobSpeed)) * Mathf.Max(0f, bobAmplitude);
        transform.localPosition = baseLocalPosition + new Vector3(0f, bobOffset, 0f);

        float spin = Mathf.Max(0f, spinSpeed) * Time.deltaTime;
        transform.Rotate(0f, 0f, spin, Space.Self);
    }
}
