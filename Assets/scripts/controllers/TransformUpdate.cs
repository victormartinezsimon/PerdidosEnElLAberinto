using UnityEngine;
using System.Collections;

public class TransformUpdate : MonoBehaviour 
{

    private NetworkManager m_nm;
    private MovementController m_mc;
    private AnimationController m_ac;
    private Info m_info;

    public float enviosSegundo = 1;
    float contador = 0;

    Vector3 posicionAnterior;
    float m_velocidad;
    Vector3 m_vectorDirectorVelocidad;

    float m_giroAnterior;//solo la y
    float m_deltaGiro;
    float m_velocidadGiro;
    
    // Use this for initialization
	void Start () 
    {
        m_nm = GameObject.FindGameObjectWithTag("Network").GetComponent<NetworkManager>();
        m_mc = GetComponent<MovementController>();
        m_ac = GetComponent<AnimationController>();
        m_info = GetComponent<Info>();
	}
    void FixedUpdate()
    {
        if (m_info.MINE)
        {
            contador += Time.deltaTime;
            float ratio = 1.0f / enviosSegundo;
            if (contador >= ratio)
            {
                contador = 0;
                Vector3 pos = this.transform.position;
                Vector3 vectorVelocidad = (pos - posicionAnterior).normalized;

                float giroActual = transform.rotation.eulerAngles.y;
                float giro = giroActual - m_giroAnterior;


                m_nm.sendTransform(pos, vectorVelocidad, m_mc.getVelocidadActual(), transform.rotation.eulerAngles, giro, m_mc.getVelocidadGiro(),m_info.ID);
            }
            posicionAnterior = this.transform.position;
            m_giroAnterior = transform.rotation.eulerAngles.y;

        }
        else
        {
            interpolacion();
            setAnimacion();

        }
    }
   
    private void interpolacion()
    {
        Vector3 destino = transform.position + ((m_vectorDirectorVelocidad.normalized * m_velocidad) * Time.deltaTime);
        transform.position = destino;

        transform.Rotate(0, ((m_deltaGiro * m_velocidadGiro) * Time.deltaTime), 0);

        //Translate((m_vectorDirectorVelocidad.normalized * m_velocidad) * Time.deltaTime);       
    }

    private void setAnimacion()
    {
        if (m_velocidad == 0)
        {
            //girados o idle
            if (m_deltaGiro < 0.01f)
            {
                //idle
                m_ac.setAnimacionRed(AnimationController.tipoAnimacionRed.IDLE);
            }
            else
            {
                //giro
                m_ac.setAnimacionRed(AnimationController.tipoAnimacionRed.GIRO);
            }

        }
        else
        {
            //delante o detras
            if (m_velocidad == m_mc.getVelocidadDelante())
            {
                //delante
                m_ac.setAnimacionRed(AnimationController.tipoAnimacionRed.WALK);
            }

            if (m_velocidad == m_mc.getVelocidadAtras())
            {
                //atras
                m_ac.setAnimacionRed(AnimationController.tipoAnimacionRed.MOONWALK);
            }


        }
    }

    public void SetPosition(Vector3 pos)
    {
       transform.position = pos;
    }
    
    public void setVectorDirectorVelocidad(Vector3 v)
    {
        this.m_vectorDirectorVelocidad = v;
    }
    public void setVelocidad(float v)
    {
        this.m_velocidad = v;        
    }
    
    public void SetRotation(Vector3 q)
    {
        transform.rotation = Quaternion.Euler(q);
    }

    public void setDeltaGiro(float f)
    {
        m_deltaGiro = f;
    }

    public void setVelocidadGiro(float f)
    {
        m_velocidadGiro = f;
    }


}
