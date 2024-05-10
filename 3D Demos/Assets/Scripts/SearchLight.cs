using UnityEngine;

public class SearchLight : MonoBehaviour
{
    public float rotationSpeed = 30f;

    [HideInInspector]
    public float rotationAngle = 45f;

    [HideInInspector]
    public Light searchLight;
    private float currentAngle;
    private int direction = 1;

    void Start()
    {
        searchLight = GetComponent<Light>();

        if (searchLight == null)
        {
            enabled = false;
        }
    }

    void Update()
    {
        currentAngle += rotationSpeed * Time.deltaTime * direction;

        if (currentAngle >= rotationAngle || currentAngle <= -rotationAngle)
        {
            direction *= -1;
        }

        transform.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }
}
