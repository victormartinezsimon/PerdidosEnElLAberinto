using UnityEngine;
using System.Collections;

public class AnimationController : MonoBehaviour {

    private MovementController m_mc;
    private NetworkManager m_nm;
    private Info m_info;
    public float velocidadAnimacionGiro=0.5f;
    public float velocidadAnimacionDelante=1.0f;
    public float velocidadAnimacionAtras=0.5f;
    public float velocidadAnimacionIdle = 1.0f;

    public enum tipoAnimacionRed { IDLE, WALK, MOONWALK, GIRO }
    private tipoAnimacionRed m_animacionActual;
    private tipoAnimacionRed animacionRed;


	// Use this for initialization
    void Start()
    {
        m_mc = GetComponent<MovementController>();
        m_nm = GameObject.FindGameObjectWithTag("Network").GetComponent<NetworkManager>();
        m_info = GetComponent<Info>();
        animacionRed = tipoAnimacionRed.IDLE;
    }

    void FixedUpdate()
    {
        if (m_info.MINE && !m_info.IA)
        {
            Animaciones();
        }
        else
        {
            playAnimacionRed();
        }
    }

    private void Animaciones()
    {

        if (m_mc.moveDirection == Vector3.zero)
        {
            if (m_nm.SplitScreen)
            {
                if (m_info.ID % 2 == 0)
                {
                    if (Input.GetAxis("Horizontal1") != 0)
                    {
                        //girando quietos
                        GetComponent<Animation>()["run"].speed = velocidadAnimacionGiro;
                        GetComponent<Animation>().CrossFade("run", 0.2f, PlayMode.StopSameLayer);
                        m_animacionActual = tipoAnimacionRed.GIRO;

                    }
                    else
                    {
                        GetComponent<Animation>().CrossFade("idle");
                        m_animacionActual = tipoAnimacionRed.IDLE;
                        //animation.CrossFade("idle",0.3f,PlayMode.StopSameLayer);
                    }
                }
                else
                {
                    if (Input.GetAxis("Horizontal2") != 0)
                    {
                        //girando quietos
                        GetComponent<Animation>()["run"].speed = velocidadAnimacionGiro;
                        GetComponent<Animation>().CrossFade("run", 0.2f, PlayMode.StopSameLayer);
                        m_animacionActual = tipoAnimacionRed.GIRO;

                    }
                    else
                    {
                        GetComponent<Animation>().CrossFade("idle");
                        m_animacionActual = tipoAnimacionRed.IDLE;
                        //animation.CrossFade("idle",0.3f,PlayMode.StopSameLayer);
                    }
                }
            }
            else
            {
                if (Input.GetAxis("Horizontal1") != 0)
                {
                    //girando quietos
                    GetComponent<Animation>()["run"].speed = velocidadAnimacionGiro;
                    GetComponent<Animation>().CrossFade("run", 0.2f, PlayMode.StopSameLayer);
                    m_animacionActual = tipoAnimacionRed.GIRO;

                }
                else
                {
                    GetComponent<Animation>().CrossFade("idle");
                    m_animacionActual = tipoAnimacionRed.IDLE;
                    //animation.CrossFade("idle",0.3f,PlayMode.StopSameLayer);
                }
            }

        }
        else
        {
            if (m_mc.moveDirection.z > 0)
            {
                //hacia delante
                GetComponent<Animation>()["run"].speed = velocidadAnimacionDelante;
                m_animacionActual = tipoAnimacionRed.WALK;
                
            }
            else
            {
                if (m_mc.moveDirection.z < 0)
                {
                    //hacia atras
                    GetComponent<Animation>()["run"].speed = velocidadAnimacionAtras;
                    m_animacionActual = tipoAnimacionRed.MOONWALK;
                    //sendInformacionAnimacion(tipoAnimacionRed.MOONWALK);
                }
            }

            GetComponent<Animation>().CrossFade("run", 0.2f, PlayMode.StopSameLayer);
            m_animacionActual = tipoAnimacionRed.IDLE;
        }
        /*
        if (Input.GetKey(KeyCode.Mouse0))
        {
            animation.CrossFade("attack", 0.2f);
        }
        */


    }

    private void playAnimacionRed()
    {
        switch (animacionRed)
        {
            case tipoAnimacionRed.GIRO: GetComponent<Animation>()["run"].speed = velocidadAnimacionGiro;
                                        GetComponent<Animation>().CrossFade("run", 0.2f, PlayMode.StopSameLayer); break;
            case tipoAnimacionRed.WALK: GetComponent<Animation>()["run"].speed = velocidadAnimacionDelante;
                                        GetComponent<Animation>().CrossFade("run", 0.2f, PlayMode.StopSameLayer); break;
            case tipoAnimacionRed.MOONWALK: GetComponent<Animation>()["run"].speed = velocidadAnimacionAtras;
                                            GetComponent<Animation>().CrossFade("run", 0.2f, PlayMode.StopSameLayer); break;
            case tipoAnimacionRed.IDLE: GetComponent<Animation>()["idle"].speed = velocidadAnimacionIdle;
                                        GetComponent<Animation>().CrossFade("idle"); break;

        }
    }

    public void setAnimacionRed(tipoAnimacionRed t)
    {
        animacionRed = t;
    }
    public void setAnimacionRed(string info)
    {
        animacionRed = (tipoAnimacionRed)System.Enum.Parse(typeof(tipoAnimacionRed), info);
    }
    public tipoAnimacionRed getAnimacionActual()
    {
        return m_animacionActual;
    }


    

   
}
