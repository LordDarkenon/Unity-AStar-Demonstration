using UnityEngine;

[ExecuteAlways]
public class ScaleCamera : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        float orthographicsize = (30.22f / (float)Screen.width) * (float)Screen.height / 2.0f;

        if (orthographicsize < 8.5f)
        {
            orthographicsize = 8.5f;
        }

        Camera.main.orthographicSize = orthographicsize;
    }

#if UNITY_EDITOR

    private void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            float orthographicsize = (30.22f / (float)Screen.width) * (float)Screen.height / 2.0f;

            if (orthographicsize < 8.5f)
            {
                orthographicsize = 8.5f;
            }

            Camera.main.orthographicSize = orthographicsize;
        }
    }

#endif
}