using UnityEngine;
using System.Collections;

public class Giro : MonoBehaviour 
{
    public Vector3 rotacion = new Vector3(10, -2, 5);
	// Update is called once per frame
	void Update ()
    {
        transform.Rotate(rotacion * Time.deltaTime*30);
	}
}
