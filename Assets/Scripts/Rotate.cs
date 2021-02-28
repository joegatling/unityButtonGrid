using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector2 _rotationSpeed = Vector2.one;
    public Vector2 _targetRotationSpeed = Vector2.one;

    float xVelocity = 0;
    float yVelocity = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IERandomizeRotation());
        
    }

    // Update is called once per frame
    void Update()
    {
        float xRotation = _rotationSpeed.x;
        float yRotation = _rotationSpeed.y;

        xRotation = Mathf.SmoothDamp(xRotation, _targetRotationSpeed.x, ref xVelocity, 0.4f);
        yRotation = Mathf.SmoothDamp(yRotation, _targetRotationSpeed.y, ref yVelocity, 0.4f);

        _rotationSpeed.x = xRotation;
        _rotationSpeed.y = yRotation;

        transform.Rotate(Vector3.up, _rotationSpeed.x * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.left, _rotationSpeed.y * Time.deltaTime, Space.World);

        
    }

    IEnumerator IERandomizeRotation()
    {
        while(true)
        {
            _targetRotationSpeed = new Vector2(Random.Range(-400.0f, 400.0f), Random.Range(-400.0f, 400.0f));
            yield return new WaitForSeconds(5.0f + Random.Range(0, 5.0f));
        }

    }

}
