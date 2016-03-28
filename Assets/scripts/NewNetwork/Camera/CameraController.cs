using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public float _defaultDistance = 8;
    private GameObject _target;

    public GameObject Target
    {
        get
        {
            return _target;
        }
        set
        {
            _target = value;
        }
    }
    
    void AddTarget(string name)
    {
        Target = GameObject.Find(name);
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (_target != null)
            this.transform.position = _target.transform.position + new Vector3(0, _defaultDistance, 0);
    }
}
