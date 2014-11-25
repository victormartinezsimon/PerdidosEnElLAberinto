using UnityEngine;
using System.Collections;

public class MovementController : MonoBehaviour 
{
    public  float velocidadMovimientoDelante = 10;
    public float velocidadMovimientoAtras = 0.5f;
    public float velocidadMovimientoGiro = 1.0f;
    public float minimoVelocidad = 2.0f;

    private float velocidad;

    public enum EstadoActual { Ground,Air }

    public EstadoActual estadoActual=EstadoActual.Air;
    public Vector3 moveDirection = Vector3.zero;

    public string groundTag = "Ground";

    public bool Delante { get; set; }
    private Vector3 rotacionAntigua;

    private NetworkManager m_networkManager;
    private Info m_info;

    void Start()
    {
        rotacionAntigua = transform.rotation.eulerAngles;
        m_networkManager = GameObject.FindGameObjectWithTag("Network").GetComponent<NetworkManager>();
        m_info = GetComponent<Info>();
    }

    void FixedUpdate()
    {
        if (m_info.MINE)
        {
            Movement();
        }
        else
        {
            this.enabled = false;
        }
        
    }

    private void Movement()
    {
        if (m_networkManager.SplitScreen)
        {
            if (m_info.ID % 2 == 0)
            {
                if (Input.GetAxis("Vertical1") != 0)
                {
                    moveDirection = new Vector3(moveDirection.x, moveDirection.y, Input.GetAxisRaw("Vertical1")).normalized;
                }

                //giro de la figura porque han pulsado las teclas A/D
                if (Input.GetAxis("Horizontal1") != 0)
                {
                    float rotacion = Input.GetAxisRaw("Horizontal1") * getVelocidadGiro();

                    Vector3 rotacionNueva = transform.rotation.eulerAngles;
                    rotacionNueva.y += rotacion;

                    transform.rotation = Quaternion.Euler(rotacionNueva);

                    rotacionAntigua = rotacionNueva;
                    //transform.Rotate(Vector3.up * rotacion);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(rotacionAntigua);
                }
            }
            else
            {
                if (Input.GetAxis("Vertical2") != 0)
                {
                    moveDirection = new Vector3(moveDirection.x, moveDirection.y, Input.GetAxisRaw("Vertical2")).normalized;
                }

                //giro de la figura porque han pulsado las teclas A/D
                if (Input.GetAxis("Horizontal2") != 0)
                {
                    float rotacion = Input.GetAxisRaw("Horizontal2") * getVelocidadGiro();

                    Vector3 rotacionNueva = transform.rotation.eulerAngles;
                    rotacionNueva.y += rotacion;

                    transform.rotation = Quaternion.Euler(rotacionNueva);

                    rotacionAntigua = rotacionNueva;
                    //transform.Rotate(Vector3.up * rotacion);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(rotacionAntigua);
                }
            }
        }
        else
        {
            if (Input.GetAxis("Vertical1") != 0)
            {
                moveDirection = new Vector3(moveDirection.x, moveDirection.y, Input.GetAxisRaw("Vertical1")).normalized;
            }

            //giro de la figura porque han pulsado las teclas A/D
            if (Input.GetAxis("Horizontal1") != 0)
            {
                float rotacion = Input.GetAxisRaw("Horizontal1") * getVelocidadGiro();

                Vector3 rotacionNueva = transform.rotation.eulerAngles;
                rotacionNueva.y += rotacion;

                transform.rotation = Quaternion.Euler(rotacionNueva);

                rotacionAntigua = rotacionNueva;
                //transform.Rotate(Vector3.up * rotacion);
            }
            else
            {
                transform.rotation = Quaternion.Euler(rotacionAntigua);
            }
        }        

        if (moveDirection.z > 0)
        {
            //hacia delante
            velocidad = getVelocidadDelante();
            Delante = true;
        }
        if (moveDirection.z < 0)
        {
            //hacia atras
            velocidad = getVelocidadAtras();
            Delante = false;
        }
        if (moveDirection.z == 0)
        {
            velocidad = 0;
        }


        if (estadoActual != EstadoActual.Air)
        {
            this.transform.Translate((moveDirection.normalized * velocidad) * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == groundTag)
        {
            estadoActual = EstadoActual.Ground;
        }
       
    }

    public float getVelocidadActual()
    {
        return velocidad;
    }
    public float getVelocidadAtras()
    {
        return velocidadMovimientoDelante * velocidadMovimientoAtras;
    }
    public float getVelocidadDelante()
    {
        return velocidadMovimientoDelante;
    }
    public float getVelocidadGiro()
    {
        return velocidadMovimientoDelante * velocidadMovimientoGiro;
    }

    public void quitarVida(float vida = 1.0f)
    {
        velocidadMovimientoDelante -= vida;
        velocidadMovimientoDelante = Mathf.Max(velocidadMovimientoDelante, minimoVelocidad);
    }

}
