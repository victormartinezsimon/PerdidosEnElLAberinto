using UnityEngine;
using System.Collections;

public class IA : MonoBehaviour 
{
    private MazeGenerator.Casilla[,] tablero;
    private int width;
    private int height;
    private int posX;
    private int posY;
    private float distanciaMediosCasillas;
    private Vector3 directorVelocidad;
    private int orientacionGiro;
    int orientacionActual = 0;//0N,1E,2S,3W

    private float timeParadoAcum = 0;
    private float timeGiro = 0;
    private MazeGenerator generator;
    private AnimationController m_animationController;
    private Info m_info;


    public float alphaMovimiento = 0.01f;
    public float alphaGiro = 5;
    public enum Giro { Derecha, Izquierda,Aleatorio }
    public Giro como_gira = Giro.Derecha;
    public bool haceTrampas = false;
    public float velocidad = 10;
    public float velocidadGiro = 0.55f;
    public float tiempoParado = 5;
    public float distanciaMaxima = 1.1f;
    public float timeOutGiro = 3;
    public string groundTag = "Ground";


    public enum estadoActual { Avanzado, Girando, Llegando, GirandoYAvanzando,Parado,DoNothing }
    public estadoActual m_estado = estadoActual.DoNothing;

	// Use this for initialization
	void Start () 
    {
        GameObject go = GameObject.FindGameObjectWithTag("Generator");
        generator=GameObject.FindGameObjectWithTag("Generator").GetComponent<MazeGenerator>();
        tablero = generator.Tablero;
        width = generator.Width;
        height = generator.Heigh;

        posX = (int)generator.casillaComienzo.x;
        posY = (int)generator.casillaComienzo.y;

        m_animationController = GetComponent<AnimationController>();
        m_info = GetComponent<Info>();

	}
	
	// Update is called once per frame
	void Update () 
    {
	    //si he llegado
        //-> en funcion de como_gira, selecciono el siguiente destino(si es un callejon, dar la vuelta
        //sino
        //-> interpolo teniendo en cuenta la velocidad ,girados mirando a destino...(giramos y cuando acabemos avanzamos?)

        switch (m_estado)
        {
            case estadoActual.Avanzado:

                if (heLlegado())
                {
                    m_estado = estadoActual.Llegando;
                }
                else
                {
                    if (meHePasado())
                    {
                        calculosGiroPasado();
                        m_estado = estadoActual.Girando;
                        m_animationController.setAnimacionRed(AnimationController.tipoAnimacionRed.IDLE);
                    }
                    else
                    {
                        transform.position += directorVelocidad * velocidad * Time.deltaTime;
                        transform.LookAt(generator.posicionCasilla(posX, posY));
                        m_animationController.setAnimacionRed(AnimationController.tipoAnimacionRed.WALK);
                    }                    
                }
                break;
            case estadoActual.Llegando:

                //calcular giro a realizar...
                calculosGiro();
                m_estado = estadoActual.Girando;
                m_animationController.setAnimacionRed(AnimationController.tipoAnimacionRed.IDLE);
                break;

            case estadoActual.Girando:
                
                if (giroFinalizado())
                {
                    
                        m_estado = estadoActual.Avanzado;
                        transform.position += directorVelocidad * velocidad * Time.deltaTime;
                        m_animationController.setAnimacionRed(AnimationController.tipoAnimacionRed.WALK);
                                 
                    
                }
                else
                {
                    //girar
                    Vector3 origen = transform.rotation.eulerAngles;
                    transform.Rotate(new Vector3(0,orientacionGiro * velocidadGiro * velocidad * Time.deltaTime,0));
                    origen.y += orientacionGiro * velocidadGiro * velocidad * Time.deltaTime;
                    transform.rotation = Quaternion.Euler(origen);
                    m_animationController.setAnimacionRed(AnimationController.tipoAnimacionRed.GIRO);

                    timeGiro += Time.deltaTime;

                    if (timeGiro >= timeOutGiro)
                    {
                        m_estado = estadoActual.Avanzado;
                        m_animationController.setAnimacionRed(AnimationController.tipoAnimacionRed.WALK);
                    }
                }
                
                break;

            case estadoActual.Parado:
                m_animationController.setAnimacionRed(AnimationController.tipoAnimacionRed.IDLE);
                timeParadoAcum += Time.deltaTime;
                if (timeParadoAcum >= tiempoParado)
                {
                    Random.seed = generator.Seed;
                    calculosGiro();
                    m_estado = estadoActual.Girando;
                }
                break;

        }

	}

    private bool heLlegado()
    {
        Vector3 resta = generator.posicionCasilla(posX,posY) - transform.position;        

        float val = resta.magnitude;

        if(val <= alphaMovimiento)
        {
            return true;
        }

        return false;
    }

    private void calculosGiro()
    {
        //calculamos cual es el siguietne destino

        bool encontrado = false;
        int orientacionDestino = 0;

        Giro gaux = como_gira;

        if (como_gira == Giro.Aleatorio)
        {
            if (Random.value < 0.5f)
            {
                gaux = Giro.Derecha;
            }
            else
            {
                gaux = Giro.Izquierda;
            }

        }


        if (gaux == Giro.Derecha)
        {
            orientacionDestino = (orientacionActual + 1 + 4) % 4;

            while (!encontrado)
            {
                if (hayHueco(orientacionDestino))
                {
                    encontrado = true;
                }
                else
                {
                    orientacionDestino = (orientacionDestino - 1 + 4) % 4;
                }                
            }
        }
        else
        {
            if (gaux == Giro.Izquierda)
            {
                orientacionDestino = (orientacionActual - 1 + 4) % 4; ;

                while (!encontrado)
                {
                    if (hayHueco(orientacionDestino))
                    {
                        encontrado = true;
                    }
                    else
                    {
                        orientacionDestino = (orientacionDestino + 1 + 4) % 4;
                    }
                }
            }
        }
        //aqui ya se a donde tengo que dirigirme
        //calculo destino y director velocidad


        if (gaux == Giro.Derecha)
        {
            orientacionGiro = 1;

            int minus = (orientacionActual - 1 + 4) % 4;
            if (minus == orientacionDestino)
            {
                orientacionGiro = -1;
            }

        }
        else
        {
            if (gaux == Giro.Izquierda)
            {
                orientacionGiro = -1;

                int minus = (orientacionActual + 1 + 4) % 4;
                if (minus == orientacionDestino)
                {
                    orientacionGiro = 1;
                }
            }
        }

        if (orientacionActual == orientacionDestino)
        {
            orientacionGiro = 0;
        }


        switch (orientacionDestino)
        {
            case 0:               
                posX += 1;               
                break;
            case 1:              
                posY += 1;
                break;
            case 2:             
                posX -= 1;
                break;
            case 3:              
                posY -= 1;
                break;
        }
        Vector3 destino = generator.posicionCasilla(posX, posY);
        orientacionActual = orientacionDestino;
        directorVelocidad = (destino - transform.position).normalized;

    }
    private void calculosGiroPasado()
    {
        Vector3 destino = generator.posicionCasilla(posX, posY);
        orientacionActual = (orientacionActual + 2 + 4) % 4;
        directorVelocidad = (destino - transform.position).normalized;
        orientacionGiro = 1;
    }
    private bool giroFinalizado()
    {
        Vector3 resta = generator.posicionCasilla(posX, posY) - transform.position;

        float angulo = Vector3.Angle(transform.forward, resta);
        //Debug.Log("giro=" + angulo);
        if (angulo <= alphaGiro)
        {
            return true;
        }

        if (orientacionGiro == 0)
        {
            return true;
        }

        return false;
    }

    private bool meHePasado()
    {
        Vector3 resta = generator.posicionCasilla(posX, posY) - transform.position;

        float val = resta.magnitude;

        float tam = generator.getTamCasilla();

        if (tam * distanciaMaxima < val)
        {
            return true;
        }


        return false;
    }

    private bool hayHueco(int orientacion)
    {
        //check no nos salimos
        switch (orientacion)
        {
            case 0: if (posX + 1 >= height) return false; break;
            case 1: if (posY + 1 >= width) return false; break;
            case 2: if (posX - 1 < 0) return false; break;
            case 3: if (posY - 1 < 0) return false; break;

        }

        MazeGenerator.Casilla casillaActual = tablero[posX, posY];

        switch (orientacion)
        {
            case 0: return !casillaActual.wallUp;
            case 1: return !casillaActual.wallRight;
            case 2: return !casillaActual.wallDown;
            case 3: return !casillaActual.wallLeft;
        }


        return false;
    }

    public void restart()
    {
        posX = 0;
        posY = 0;
        m_estado = estadoActual.Parado;

        //find the spawn point
        GameObject spawn = GameObject.Find("SpawnPoint" + m_info.ID);
        if (spawn != null)
        {
            gameObject.transform.position = spawn.transform.position;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == groundTag && m_estado==estadoActual.DoNothing)
        {
            m_estado = estadoActual.Parado;
        }

    }

}
