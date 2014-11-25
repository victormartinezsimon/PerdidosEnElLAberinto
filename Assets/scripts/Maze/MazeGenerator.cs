using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class MazeGenerator: MonoBehaviour
{
	public struct Casilla
	{
		public bool wallUp;
        public bool wallDown;
        public bool wallLeft;
        public bool wallRight;        
        public bool visited;
        public bool trampas;
	}

	private Casilla[,] tablero;
    private int width;
    private int height;
    private int m_seed;

    #region accesores
    public Casilla[,] Tablero
    {
        get { return tablero; }
    }
    public int Width
    {
        get { return width; }
    }
    public int Heigh
    {
        get { return height; }
    }
    public int Seed
    {
        get { return m_seed; }
    }
    #endregion

    private List<Vector4> pendientesDeAnalizar;
    
    public Vector2 casillaComienzo = new Vector2(0, 0);


    public GameObject pared;
   // public GameObject paredGirada;
    public GameObject interserctor;
    public GameObject tesoro;
    public GameObject suelo;
    public GameObject huecoPasillo;
    public GameObject paredQuitaVidas;
    public GameObject killPlane;

    private enum MazeState { THREAD, INSTANCIANDO, FIN,WAITING_SERVER,DONOTHING}
    private MazeState estadoConstruccion;

    private NetworkManager networkManager;

    public Vector3 origenMaze = new Vector3(0, 0, 0);

    int porcentajeQuitaVidas = 10;

	// Use this for initialization
	void Start () 
	{
        networkManager = GameObject.FindGameObjectWithTag("Network").GetComponent<NetworkManager>();
        width = networkManager.Ancho;
        height = networkManager.Alto;
        m_seed = networkManager.Seed;
        Random.seed = Seed;
        porcentajeQuitaVidas = networkManager.PorcentajeQuitaVidas;
        //noWallNoThread();
        generateMazeKruskalNoThread();
        instanciarLaberintoNoNetWork();
        instanciarSueloNoNetWork();
        colocarHuecosPasilloNoNetWork();
        instanciaKillPlane();
        estadoConstruccion = MazeState.FIN;

        /*
        if (networkManager.isClient())
        {
            estadoConstruccion = MazeState.WAITING_SERVER;
            networkManager.listaCallbacksMazeConstruido += mazeInstantiatedServer;
        }
        else
        {
            //GestorRandom.getInstance().setSeed(networkManager.Seed);
            //th = new Thread(generateMazePrim);
            //th = new Thread(generateMazeKruskal);
            //th = new Thread(noWall);
            //th.Start();
            //Debug.Log("Thread lanzado");
            Random.seed = networkManager.Seed;
            porcentajeQuitaVidas = networkManager.PorcentajeQuitaVidas;
            generateMazeKruskalNoThread();
            instanciarLaberintoNetWork();
            instanciarSueloNetWork();
            colocarHuecosPasilloNetWork();
            networkManager.mazeGeneratedServer();
            estadoConstruccion = MazeState.FIN;
        }   
        */
    }
	
	// Update is called once per frame
	void Update ()
    {
        switch (estadoConstruccion)
        {
            case MazeState.THREAD: GestorRandom.getInstance().generaNumero(); break;
            case MazeState.INSTANCIANDO: break; 
            case MazeState.WAITING_SERVER:
                Debug.Log("Waitig building from server");break;

            case MazeState.FIN:

                if (!networkManager.playerInstanciado)
                {
                    networkManager.instanciaAllPlayers();
                    estadoConstruccion = MazeState.DONOTHING;
                }
                break;

        }

	}

    private void generateMazePrimThread()
    {
        estadoConstruccion = MazeState.THREAD;
        tablero = new Casilla[height, width];

        //inizialization to all walls and no visited
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                tablero[i, j].wallDown = true;
                tablero[i, j].wallUp = true;
                tablero[i, j].wallLeft= true;
                tablero[i, j].wallRight = true;
                tablero[i, j].visited = false;
                tablero[i, j].trampas = false;
            }
        }

        pendientesDeAnalizar = new List<Vector4>();

        pendientesDeAnalizar.Add(new Vector4(casillaComienzo.x-1,casillaComienzo.y,casillaComienzo.x,casillaComienzo.y));

        GestorRandom gr = GestorRandom.getInstance();

        while (pendientesDeAnalizar.Count != 0)
        {
            //we take one randomally
            int index = gr.getNumeroAleatorio(0, pendientesDeAnalizar.Count);
            Vector4 elemSelected = pendientesDeAnalizar[index];
            pendientesDeAnalizar.RemoveAt(index);

            Vector2 origin= new Vector2(elemSelected.x,elemSelected.y);
            Vector2 destiny= new Vector2(elemSelected.z,elemSelected.w);

            if(!tablero[(int)destiny.x, (int)destiny.y].visited)
            {
                tablero[(int)destiny.x, (int)destiny.y].visited = true;
                //if the destiny isn't visited yet
                //remove the wall(carefully with the first case
                //add the destiny 4 wall to the list if they dont go outside the maze
                if(origin.x>=0 &&  origin.y>= 0 && origin.x< height && origin.y< width)
                {
                   if(origin.x == destiny.x)
                   {//left or right
                       if(origin.y> destiny.y)
                       {
                           //left
                           tablero[(int)origin.x, (int)origin.y].wallLeft=false;
                       }
                       else
                       {
                           //right
                           tablero[(int)origin.x, (int)origin.y].wallRight=false; 
                       }
                   }
                   else
                   {//up or down
                       if(origin.x> destiny.x)
                       {
                           //up
                           tablero[(int)origin.x, (int)origin.y].wallUp=false;
                       }
                       else
                       {
                           //down
                           tablero[(int)origin.x, (int)origin.y].wallDown=false;
                       }
                   }

                   //remove wall from destiny to origin
                   //caution-> when left-> right, and when up->down
                   if (origin.x == destiny.x)
                   {//left or right
                       if (origin.y > destiny.y)
                       {
                           //right
                           tablero[(int)destiny.x, (int)destiny.y].wallRight = false;
                       }
                       else
                       {
                           //left
                           tablero[(int)destiny.x, (int)destiny.y].wallLeft = false;
                       }
                   }
                   else
                   {//up or down
                       if (origin.x > destiny.x)
                       {
                           //down
                           tablero[(int)destiny.x, (int)destiny.y].wallDown = false;
                       }
                       else
                       {
                           //up
                           tablero[(int)destiny.x, (int)destiny.y].wallUp = false;
                       }
                   }

                }

                //add up
                if(destiny.x-1 >= 0)
                {
                    Vector4 add= new Vector4(destiny.x,destiny.y,destiny.x-1,destiny.y);
                    pendientesDeAnalizar.Add(add);
                }

                //add down
                if(destiny.x+1 < height)
                {
                    Vector4 add= new Vector4(destiny.x,destiny.y,destiny.x+1,destiny.y);
                    pendientesDeAnalizar.Add(add);
                }
                //add left
                if(destiny.y-1 >= 0)
                {
                    Vector4 add= new Vector4(destiny.x,destiny.y,destiny.x,destiny.y-1);
                    pendientesDeAnalizar.Add(add);
                }

                //add right
                if(destiny.y+1 < width)
                {
                    Vector4 add= new Vector4(destiny.x,destiny.y,destiny.x,destiny.y+1);
                    pendientesDeAnalizar.Add(add);
                }

            }
                       
        }
        estadoConstruccion = MazeState.INSTANCIANDO;
      
    }
    private void generateMazePrimNoThread()
    {
        tablero = new Casilla[height, width];

        //inizialization to all walls and no visited
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                tablero[i, j].wallDown = true;
                tablero[i, j].wallUp = true;
                tablero[i, j].wallLeft = true;
                tablero[i, j].wallRight = true;
                tablero[i, j].visited = false;
                tablero[i, j].trampas = false;
            }
        }

        pendientesDeAnalizar = new List<Vector4>();

        pendientesDeAnalizar.Add(new Vector4(casillaComienzo.x - 1, casillaComienzo.y, casillaComienzo.x, casillaComienzo.y));

        while (pendientesDeAnalizar.Count != 0)
        {
            //we take one randomally
            int index = Random.Range(0, pendientesDeAnalizar.Count);
            Vector4 elemSelected = pendientesDeAnalizar[index];
            pendientesDeAnalizar.RemoveAt(index);

            Vector2 origin = new Vector2(elemSelected.x, elemSelected.y);
            Vector2 destiny = new Vector2(elemSelected.z, elemSelected.w);

            if (!tablero[(int)destiny.x, (int)destiny.y].visited)
            {
                tablero[(int)destiny.x, (int)destiny.y].visited = true;
                //if the destiny isn't visited yet
                //remove the wall(carefully with the first case
                //add the destiny 4 wall to the list if they dont go outside the maze
                if (origin.x >= 0 && origin.y >= 0 && origin.x < height && origin.y < width)
                {
                    if (origin.x == destiny.x)
                    {//left or right
                        if (origin.y > destiny.y)
                        {
                            //left
                            tablero[(int)origin.x, (int)origin.y].wallLeft = false;
                        }
                        else
                        {
                            //right
                            tablero[(int)origin.x, (int)origin.y].wallRight = false;
                        }
                    }
                    else
                    {//up or down
                        if (origin.x > destiny.x)
                        {
                            //up
                            tablero[(int)origin.x, (int)origin.y].wallUp = false;
                        }
                        else
                        {
                            //down
                            tablero[(int)origin.x, (int)origin.y].wallDown = false;
                        }
                    }

                    //remove wall from destiny to origin
                    //caution-> when left-> right, and when up->down
                    if (origin.x == destiny.x)
                    {//left or right
                        if (origin.y > destiny.y)
                        {
                            //right
                            tablero[(int)destiny.x, (int)destiny.y].wallRight = false;
                        }
                        else
                        {
                            //left
                            tablero[(int)destiny.x, (int)destiny.y].wallLeft = false;
                        }
                    }
                    else
                    {//up or down
                        if (origin.x > destiny.x)
                        {
                            //down
                            tablero[(int)destiny.x, (int)destiny.y].wallDown = false;
                        }
                        else
                        {
                            //up
                            tablero[(int)destiny.x, (int)destiny.y].wallUp = false;
                        }
                    }

                }

                //add up
                if (destiny.x - 1 >= 0)
                {
                    Vector4 add = new Vector4(destiny.x, destiny.y, destiny.x - 1, destiny.y);
                    pendientesDeAnalizar.Add(add);
                }

                //add down
                if (destiny.x + 1 < height)
                {
                    Vector4 add = new Vector4(destiny.x, destiny.y, destiny.x + 1, destiny.y);
                    pendientesDeAnalizar.Add(add);
                }
                //add left
                if (destiny.y - 1 >= 0)
                {
                    Vector4 add = new Vector4(destiny.x, destiny.y, destiny.x, destiny.y - 1);
                    pendientesDeAnalizar.Add(add);
                }

                //add right
                if (destiny.y + 1 < width)
                {
                    Vector4 add = new Vector4(destiny.x, destiny.y, destiny.x, destiny.y + 1);
                    pendientesDeAnalizar.Add(add);
                }

            }

        }
        estadoConstruccion = MazeState.INSTANCIANDO;

    }
   
    private void generateMazeKruskalThread()
    {
        estadoConstruccion = MazeState.THREAD;
        tablero = new Casilla[height, width];
        List<Vector4> listaParedes = new List<Vector4>();
        List<HashSet<int>> arraySet = new List<HashSet<int>>();
        int cuenta = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                tablero[i, j].wallDown = true;
                tablero[i, j].wallUp = true;
                tablero[i, j].wallLeft = true;
                tablero[i, j].wallRight = true;
                tablero[i, j].visited = false;
                tablero[i, j].trampas = false;

                HashSet<int> aux = new HashSet<int>();
                aux.Add(cuenta);                
                arraySet.Add(aux);
                cuenta++;
                if (i - 1 >= 0)
                {
                    //up
                    listaParedes.Add(new Vector4(i, j, i - 1, j));
                }
                if (j - 1 >= 0)
                {
                    //left
                    listaParedes.Add(new Vector4(i, j, i, j-1));
                }
                if (i + 1 < height)
                {
                    //down
                    listaParedes.Add(new Vector4(i, j, i + 1, j));
                }
                if (j + 1 < width)
                {
                    //right
                    listaParedes.Add(new Vector4(i, j, i,j + 1));
                }

            }
        }
        GestorRandom gr = GestorRandom.getInstance();
        while (listaParedes.Count != 0)
        {
            int paredAEliminarIndex = gr.getNumeroAleatorio(0, listaParedes.Count);
            Vector4 paredAEliminar = listaParedes[paredAEliminarIndex];
            Vector4 popuesta = new Vector4(paredAEliminar.z, paredAEliminar.w, paredAEliminar.x, paredAEliminar.y);
            listaParedes.RemoveAt(paredAEliminarIndex);
            listaParedes.Remove(popuesta);

            int idOrigen = (int)(paredAEliminar.x * width + paredAEliminar.y);
            int idDestino = (int)(paredAEliminar.z * width + paredAEliminar.w);

            int idSetOrigen = findInSet(idOrigen, arraySet);
            int idSetDestino = findInSet(idDestino, arraySet);

            if (idSetDestino!= idSetOrigen)
            {                
                HashSet<int> setDestino = arraySet[idSetDestino];
                arraySet[idSetOrigen].UnionWith(setDestino);
                arraySet.RemoveAt(idSetDestino);
                
                

                Vector2 origin = new Vector2(paredAEliminar.x , paredAEliminar.y);
                Vector2 destiny = new Vector2(paredAEliminar.z, paredAEliminar.w);

                //eliminar la pared
                if (origin.x == destiny.x)
                {//left or right
                    if (origin.y > destiny.y)
                    {
                        //left
                        tablero[(int)origin.x, (int)origin.y].wallLeft = false;
                    }
                    else
                    {
                        //right
                        tablero[(int)origin.x, (int)origin.y].wallRight = false;
                    }
                }
                else
                {//up or down
                    if (origin.x > destiny.x)
                    {
                        //up
                        tablero[(int)origin.x, (int)origin.y].wallUp = false;
                    }
                    else
                    {
                        //down
                        tablero[(int)origin.x, (int)origin.y].wallDown = false;
                    }
                }

                //remove wall from destiny to origin
                //caution-> when left-> right, and when up->down
                if (origin.x == destiny.x)
                {//left or right
                    if (origin.y > destiny.y)
                    {
                        //right
                        tablero[(int)destiny.x, (int)destiny.y].wallRight = false;
                    }
                    else
                    {
                        //left
                        tablero[(int)destiny.x, (int)destiny.y].wallLeft = false;
                    }
                }
                else
                {//up or down
                    if (origin.x > destiny.x)
                    {
                        //down
                        tablero[(int)destiny.x, (int)destiny.y].wallDown = false;
                    }
                    else
                    {
                        //up
                        tablero[(int)destiny.x, (int)destiny.y].wallUp = false;
                    }
                }

            }
        }
       
        estadoConstruccion = MazeState.INSTANCIANDO;
        
    }
    private void generateMazeKruskalNoThread()
    {

        tablero = new Casilla[height, width];
        List<Vector4> listaParedes = new List<Vector4>();
        List<HashSet<int>> arraySet = new List<HashSet<int>>();
        int cuenta = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                tablero[i, j].wallDown = true;
                tablero[i, j].wallUp = true;
                tablero[i, j].wallLeft = true;
                tablero[i, j].wallRight = true;
                tablero[i, j].visited = false;
                tablero[i, j].trampas = false;

                HashSet<int> aux = new HashSet<int>();
                aux.Add(cuenta);
                arraySet.Add(aux);
                cuenta++;
                if (i - 1 >= 0)
                {
                    //down
                    listaParedes.Add(new Vector4(i, j, i - 1, j));
                }
                if (j - 1 >= 0)
                {
                    //left
                    listaParedes.Add(new Vector4(i, j, i, j - 1));
                }
                if (i + 1 < height)
                {
                    //up
                    listaParedes.Add(new Vector4(i, j, i + 1, j));
                }
                if (j + 1 < width)
                {
                    //right
                    listaParedes.Add(new Vector4(i, j, i, j + 1));
                }

            }
        }
        while (listaParedes.Count != 0)
        {
            int paredAEliminarIndex = Random.Range(0, listaParedes.Count);
            Vector4 paredAEliminar = listaParedes[paredAEliminarIndex];
            Vector4 popuesta = new Vector4(paredAEliminar.z, paredAEliminar.w, paredAEliminar.x, paredAEliminar.y);
            listaParedes.RemoveAt(paredAEliminarIndex);
            listaParedes.Remove(popuesta);

            int idOrigen = (int)(paredAEliminar.x * width + paredAEliminar.y);
            int idDestino = (int)(paredAEliminar.z * width + paredAEliminar.w);

            int idSetOrigen = findInSet(idOrigen, arraySet);
            int idSetDestino = findInSet(idDestino, arraySet);

            if (idSetDestino != idSetOrigen)
            {
                HashSet<int> setDestino = arraySet[idSetDestino];
                arraySet[idSetOrigen].UnionWith(setDestino);
                arraySet.RemoveAt(idSetDestino);



                Vector2 origin = new Vector2(paredAEliminar.x, paredAEliminar.y);
                Vector2 destiny = new Vector2(paredAEliminar.z, paredAEliminar.w);

                //eliminar la pared
                if (origin.x == destiny.x)
                {//left or right
                    if (origin.y > destiny.y)
                    {
                        //left
                        tablero[(int)origin.x, (int)origin.y].wallLeft = false;                    
                    }
                    else
                    {
                        //right
                        tablero[(int)origin.x, (int)origin.y].wallRight = false;
                    }
                }
                else
                {//up or down
                    if (origin.x > destiny.x)
                    {
                        //up
                        tablero[(int)origin.x, (int)origin.y].wallDown = false;
                    }
                    else
                    {
                        //down
                        tablero[(int)origin.x, (int)origin.y].wallUp = false;
                    }
                }

                //remove wall from destiny to origin
                //caution-> when left-> right, and when up->down
                if (origin.x == destiny.x)
                {//left or right
                    if (origin.y > destiny.y)
                    {
                        //right
                        tablero[(int)destiny.x, (int)destiny.y].wallRight = false;
                    }
                    else
                    {
                        //left
                        tablero[(int)destiny.x, (int)destiny.y].wallLeft = false;
                    }
                }
                else
                {//up or down
                    if (origin.x > destiny.x)
                    {
                        //down
                        tablero[(int)destiny.x, (int)destiny.y].wallUp = false;
                    }
                    else
                    {
                        //up
                        tablero[(int)destiny.x, (int)destiny.y].wallDown = false;
                    }
                }

            }
        }

        estadoConstruccion = MazeState.INSTANCIANDO;

    }

    private void noWallThread()
    {
        estadoConstruccion = MazeState.THREAD;
        tablero = new Casilla[height, width];

        //inizialization to all walls and no visited
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                tablero[i, j].wallDown = false;
                tablero[i, j].wallUp = false;
                tablero[i, j].wallLeft = false;
                tablero[i, j].wallRight = false;
                tablero[i, j].visited = false;
            }
        }
        estadoConstruccion = MazeState.INSTANCIANDO;
    }
    private void noWallNoThread()
    {
        tablero = new Casilla[height, width];

        //inizialization to all walls and no visited
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                tablero[i, j].wallDown = false;
                tablero[i, j].wallUp = false;
                tablero[i, j].wallLeft = false;
                tablero[i, j].wallRight = false;
                tablero[i, j].visited = false;
            }
        }
        estadoConstruccion = MazeState.INSTANCIANDO;
    }

    private bool esValidaTrampa(int coordX, int coordY)
    {
        if (coordX == 0 || coordY == 0 || coordX == height - 1 || coordY == width - 1)
            return false;
        return true;
    }
    private int findInSet(int id, List<HashSet<int>> arraySet)
    {
        for (int i = 0; i < arraySet.Count; i++)
        {
            if (arraySet[i].Contains(id))
            {
                return i;
            }
        }
        return -1;
    }

    private void instanciarLaberintoNetWork()
    {


        float posX = origenMaze.x;
        float posY = origenMaze.y;
        float posZ = origenMaze.z;

        float aumentoXPared = pared.renderer.bounds.size.x;
        float aumentoYPared = pared.renderer.bounds.size.y;
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoXInterseccion = interserctor.renderer.bounds.size.x;
        float aumentoYInterseccion = interserctor.renderer.bounds.size.y;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;

        
        posX = origenMaze.x;
        posZ = origenMaze.z;
        //instanciacion de los intersectores
        for (int i = 0; i < height + 1; i++)
        {
            for (int j = 0; j < width + 1; j++)
            {
                Vector3 posicion = new Vector3(posX, posY, posZ);
                posicion.x = posicion.x + aumentoXInterseccion / 2;
                posicion.y = posicion.y + aumentoYInterseccion / 2;
                posicion.z = posicion.z + aumentoZInterseccion / 2;

                Network.Instantiate(interserctor, posicion, Quaternion.identity, 1);
                posX = posX + aumentoXInterseccion + aumentoZPared;
            }
            posX = 0;
            posZ = posZ + aumentoZPared + aumentoZInterseccion;

        }

        //abajo
        posX = origenMaze.x + aumentoXInterseccion;
        posZ = origenMaze.z;
        for (int i = 0; i < width; i++)
        {
            Vector3 posicion = new Vector3(posX, posY, posZ);
            posicion.x = posicion.x + aumentoZPared / 2;
            posicion.y = posicion.y + aumentoYPared / 2;
            posicion.z = posicion.z + aumentoXPared / 2;
            Quaternion q = Quaternion.Euler(0, 90, 0);
            Network.Instantiate(pared, posicion, q, 1);
            posX = posX + aumentoZPared + aumentoXInterseccion;
        }

        //izquierda
        posX = origenMaze.x;
        posZ = origenMaze.z + aumentoZInterseccion;

        for (int i = 0; i < height; i++)
        {
            Vector3 pos = new Vector3(posX, posY, posZ);
            pos.x = pos.x + aumentoXPared / 2;
            pos.y = pos.y + aumentoYPared / 2;
            pos.z = pos.z + aumentoZPared / 2;

            Network.Instantiate(pared, pos, Quaternion.identity, 1);
            posZ = posZ + aumentoZPared + aumentoZInterseccion;
        }

        //paredes de la derecha
        posX = origenMaze.x + aumentoXInterseccion + aumentoZPared;
        posZ = origenMaze.z + aumentoZInterseccion;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Casilla c = tablero[i, j];
                if (c.wallRight)
                {
                    Vector3 pos = new Vector3(posX, posY, posZ);
                    pos.x = pos.x + aumentoXPared / 2;
                    pos.y = pos.y + aumentoYPared / 2;
                    pos.z = pos.z + aumentoZPared / 2;

                    int value=Random.Range(0,100);
                    if (value <= porcentajeQuitaVidas && j != width-1 && i!= height-1)
                    {
                        tablero[i, j].trampas = true;
                        Network.Instantiate(paredQuitaVidas, pos, Quaternion.identity, 1);
                    }
                    else
                    {
                        Network.Instantiate(pared, pos, Quaternion.identity, 1);
                    }

                    
                }
                posX = posX + aumentoZPared + aumentoXInterseccion;
            }
            posZ = posZ + aumentoZPared + aumentoZInterseccion;
            posX = aumentoXInterseccion + aumentoZPared;
        }

        //paredes de arriba

        posX = origenMaze.x + aumentoXInterseccion;
        posZ = origenMaze.z + aumentoXInterseccion + aumentoZPared;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Casilla c = tablero[i, j];
                if (c.wallDown)
                {
                    Vector3 pos = new Vector3(posX, posY, posZ);
                    pos.x = pos.x + aumentoZPared / 2;
                    pos.y = pos.y + aumentoYPared / 2;
                    pos.z = pos.z + aumentoXPared / 2;
                    Quaternion q = Quaternion.Euler(0, 90, 1);

                    int value = Random.Range(0, 100);
                    if (value <= porcentajeQuitaVidas && j != width - 1 && i != height - 1)
                    {
                        tablero[i, j].trampas = true;
                        Network.Instantiate(paredQuitaVidas, pos, q, 1);
                    }
                    else
                    {
                        Network.Instantiate(pared, pos, q, 1);
                    }


                    
                }
                posX = posX + aumentoZPared + aumentoXInterseccion;
            }
            posZ = posZ + aumentoZPared + aumentoZInterseccion;
            posX = aumentoXInterseccion;

        }
        //tesoro

        posX = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * (width - 1)) + aumentoZPared / 2 + aumentoZInterseccion; ;
        posZ = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * (height - 1)) + aumentoZPared / 2 + aumentoZInterseccion; ; 
        {

            Vector3 pos = new Vector3(posX, 1, posZ);
            pos.x = pos.x + aumentoZPared / 2;
            pos.y = pos.y + aumentoYPared / 2;
            pos.z = pos.z + aumentoXPared / 2;
            //Quaternion q = Quaternion.Euler(90, 0, 0);
            Network.Instantiate(tesoro, pos, Quaternion.identity, 1);
        }
    }
    private void colocarHuecosPasilloNetWork()
    {
        float aumentoXPared = pared.renderer.bounds.size.x;
        float aumentoYPared = pared.renderer.bounds.size.y;
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoXInterseccion = interserctor.renderer.bounds.size.x;
        float aumentoYInterseccion = interserctor.renderer.bounds.size.y;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;

        //only vertical
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float posX = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * i) + aumentoXInterseccion - aumentoXInterseccion;
                float posZ = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * j) + aumentoZInterseccion;

                Vector3 pos = new Vector3(posX, 0, posZ);
                pos.x = pos.x + aumentoXPared / 2;
                pos.y = pos.y + huecoPasillo.renderer.bounds.size.y / 2;
                pos.z = pos.z + aumentoZPared / 2;
                Network.Instantiate(huecoPasillo, pos, Quaternion.identity, 4);

            }
        }
        //only horizontal
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float posX = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * i) + aumentoXInterseccion;
                float posZ = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * j) + aumentoZInterseccion - aumentoXInterseccion;

                Vector3 pos = new Vector3(posX, 0, posZ);
                pos.x = pos.x + aumentoZPared / 2;
                pos.y = pos.y + huecoPasillo.renderer.bounds.size.y / 2;
                pos.z = pos.z + aumentoXPared / 2;
                Quaternion q = Quaternion.Euler(0, 90, 0);

                Network.Instantiate(huecoPasillo, pos, q, 4);

            }
        }
    }
    private void instanciarSueloNetWork()
    {

        float aumentoXPared = pared.renderer.bounds.size.x;
        float aumentoYPared = pared.renderer.bounds.size.y;
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoXInterseccion = interserctor.renderer.bounds.size.x;
        float aumentoYInterseccion = interserctor.renderer.bounds.size.y;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;

        float aumentoXSuelo = suelo.renderer.bounds.size.x;
        float aumentoYSuelo = suelo.renderer.bounds.size.y;
        float aumentoZSuelo = suelo.renderer.bounds.size.z;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float posX = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * i) + aumentoXInterseccion;
                float posZ = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * j) + aumentoZInterseccion;

                Vector3 pos = new Vector3(posX, 0, posZ);

                pos.x = pos.x + aumentoXSuelo / 2;
                pos.y = pos.y + aumentoYSuelo / 2;
                pos.z = pos.z + aumentoZSuelo / 2;
                Network.Instantiate(suelo, pos, Quaternion.identity, 4);
            }
        }
    }

    private void instanciarLaberintoNoNetWork()
    {
        float posX = origenMaze.x;
        float posY = origenMaze.y;
        float posZ = origenMaze.z;

        float aumentoXPared = pared.renderer.bounds.size.x;
        float aumentoYPared = pared.renderer.bounds.size.y;
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoXInterseccion = interserctor.renderer.bounds.size.x;
        float aumentoYInterseccion = interserctor.renderer.bounds.size.y;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;


        posX = origenMaze.x;
        posZ = origenMaze.z;
        //instanciacion de los intersectores
        for (int i = 0; i < height + 1; i++)
        {
            for (int j = 0; j < width + 1; j++)
            {
                Vector3 posicion = new Vector3(posX, posY, posZ);
                posicion.x = posicion.x + aumentoXInterseccion / 2;
                posicion.y = posicion.y + aumentoYInterseccion / 2;
                posicion.z = posicion.z + aumentoZInterseccion / 2;

                Instantiate(interserctor, posicion, Quaternion.identity);
                posX = posX + aumentoXInterseccion + aumentoZPared;
            }
            posX = 0;
            posZ = posZ + aumentoZPared + aumentoZInterseccion;

        }

        //abajo
        posX = origenMaze.x + aumentoXInterseccion;
        posZ = origenMaze.z;
        for (int i = 0; i < width; i++)
        {
            if (tablero[0,i].wallDown)
            {
                Vector3 posicion = new Vector3(posX, posY, posZ);
                posicion.x = posicion.x + aumentoZPared / 2;
                posicion.y = posicion.y + aumentoYPared / 2;
                posicion.z = posicion.z + aumentoXPared / 2;
                Quaternion q = Quaternion.Euler(0, 90, 0);
                GameObject go = Instantiate(pared, posicion, q) as GameObject;
                go.name = "(" + " 0" + i + ")->" + "(" + "-1" + i + ")";
            }

            
            posX = posX + aumentoZPared + aumentoXInterseccion;
        }

        
        //izquierda
        posX = origenMaze.x;
        posZ = origenMaze.z + aumentoZInterseccion;

        for (int i = 0; i < height; i++)
        {
            if (tablero[i,0].wallLeft)
            {
                Vector3 pos = new Vector3(posX, posY, posZ);
                pos.x = pos.x + aumentoXPared / 2;
                pos.y = pos.y + aumentoYPared / 2;
                pos.z = pos.z + aumentoZPared / 2;

                GameObject go = Instantiate(pared, pos, Quaternion.identity) as GameObject;
                go.name = "(" + i + " 0" + ")->" + "(" + i + "-1" + ")";
            }
           
            posZ = posZ + aumentoZPared + aumentoZInterseccion;
        }
        
        //paredes de la derecha
        posX = origenMaze.x + aumentoXInterseccion + aumentoZPared;
        posZ = origenMaze.z + aumentoZInterseccion;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Casilla c = tablero[i, j];
                if (c.wallRight)
                {
                    Vector3 pos = new Vector3(posX, posY, posZ);
                    pos.x = pos.x + aumentoXPared / 2;
                    pos.y = pos.y + aumentoYPared / 2;
                    pos.z = pos.z + aumentoZPared / 2;

                    int value = Random.Range(0, 100);
                    if (value <= porcentajeQuitaVidas && j != width - 1 && i != height - 1)
                    {
                        tablero[i, j].trampas = true;
                        GameObject go = Instantiate(paredQuitaVidas, pos, Quaternion.identity) as GameObject;
                        go.name = "(" + i + " " + j + ")" + "->" + "(" + i + " " + (j + 1).ToString() + ")";
                    }
                    else
                    {
                        GameObject go = Instantiate(pared, pos, Quaternion.identity) as GameObject;
                        go.name = "(" + i + " " + j + ")" + "->" + "(" + i + " " + (j+1).ToString() + ")";
                    }


                }
                posX = posX + aumentoZPared + aumentoXInterseccion;
            }
            posZ = posZ + aumentoZPared + aumentoZInterseccion;
            posX = aumentoXInterseccion + aumentoZPared;
        }
        
        //paredes de arriba

        posX = origenMaze.x + aumentoXInterseccion;
        posZ = origenMaze.z + aumentoXInterseccion + aumentoZPared;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Casilla c = tablero[i, j];
                if (c.wallUp)
                {
                    Vector3 pos = new Vector3(posX, posY, posZ);
                    pos.x = pos.x + aumentoZPared / 2;
                    pos.y = pos.y + aumentoYPared / 2;
                    pos.z = pos.z + aumentoXPared / 2;
                    Quaternion q = Quaternion.Euler(0, 90, 1);

                    int value = Random.Range(0, 100);
                    if (value <= porcentajeQuitaVidas && j != width - 1 && i != height - 1)
                    {
                        tablero[i, j].trampas = true;
                        GameObject go = Instantiate(paredQuitaVidas, pos, q) as GameObject;
                        go.name = "(" + i + " " + j + ")" + "->" + "(" + (i + 1).ToString() + " " + j + ")";
                    }
                    else
                    {
                        GameObject go = Instantiate(pared, pos, q) as GameObject;
                        go.name = "(" + i + " " + j + ")" + "->" + "(" + (i+1).ToString() + " " + j + ")";
                    }



                }
                posX = posX + aumentoZPared + aumentoXInterseccion;
            }
            posZ = posZ + aumentoZPared + aumentoZInterseccion;
            posX = aumentoXInterseccion;

        }
        
        //tesoro

        posX = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * (width - 1)) + aumentoZPared / 2 + aumentoZInterseccion; ;
        posZ = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * (height - 1)) + aumentoZPared / 2 + aumentoZInterseccion; ;
        {

            Vector3 pos = new Vector3(posX, 1, posZ);
            //pos.x = pos.x + aumentoZPared / 2;
            pos.y = pos.y + aumentoYPared / 2;
            //pos.z = pos.z + aumentoXPared / 2;
            //Quaternion q = Quaternion.Euler(90, 0, 0);
            Instantiate(tesoro, pos, Quaternion.identity);
        }
    }
    private void colocarHuecosPasilloNoNetWork()
    {
        float aumentoXPared = pared.renderer.bounds.size.x;
        float aumentoYPared = pared.renderer.bounds.size.y;
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoXInterseccion = interserctor.renderer.bounds.size.x;
        float aumentoYInterseccion = interserctor.renderer.bounds.size.y;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;

        //only vertical
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float posX = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * i) + aumentoXInterseccion - aumentoXInterseccion;
                float posZ = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * j) + aumentoZInterseccion;

                Vector3 pos = new Vector3(posX, 0, posZ);
                pos.x = pos.x + aumentoXPared / 2;
                pos.y = pos.y + huecoPasillo.renderer.bounds.size.y / 2;
                pos.z = pos.z + aumentoZPared / 2;
                Instantiate(huecoPasillo, pos, Quaternion.identity);

            }
        }
        //only horizontal
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float posX = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * i) + aumentoXInterseccion;
                float posZ = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * j) + aumentoZInterseccion - aumentoXInterseccion;

                Vector3 pos = new Vector3(posX, 0, posZ);
                pos.x = pos.x + aumentoZPared / 2;
                pos.y = pos.y + huecoPasillo.renderer.bounds.size.y / 2;
                pos.z = pos.z + aumentoXPared / 2;
                Quaternion q = Quaternion.Euler(0, 90, 0);

                Instantiate(huecoPasillo, pos, q);

            }
        }
    }
    private void instanciarSueloNoNetWork()
    {

        float aumentoXPared = pared.renderer.bounds.size.x;
        float aumentoYPared = pared.renderer.bounds.size.y;
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoXInterseccion = interserctor.renderer.bounds.size.x;
        float aumentoYInterseccion = interserctor.renderer.bounds.size.y;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;

        float aumentoXSuelo = suelo.renderer.bounds.size.x;
        float aumentoYSuelo = suelo.renderer.bounds.size.y;
        float aumentoZSuelo = suelo.renderer.bounds.size.z;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float posX = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * i) + aumentoXInterseccion;
                float posZ = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * j) + aumentoZInterseccion;

                Vector3 pos = new Vector3(posX, 0, posZ);

                pos.x = pos.x + aumentoXSuelo / 2;
                pos.y = pos.y + aumentoYSuelo / 2;
                pos.z = pos.z + aumentoZSuelo / 2;
                Instantiate(suelo, pos, Quaternion.identity);
            }
        }
    }

    private void instanciaKillPlane()
    {
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;

        float ancho = width * getTamCasilla() + aumentoZPared;
        float alto = height * getTamCasilla() + aumentoZPared;

        killPlane.transform.localScale = new Vector3(ancho*0.2f, 1, alto*0.2f);

        killPlane.transform.position = new Vector3(ancho/2, -10, alto/2);
    }

    public void mazeInstantiatedServer()
    {
        estadoConstruccion = MazeState.FIN;
    }
    public Vector3 posicionCasilla(int x, int z)
    {
        Vector3 devolver = new Vector3();

        float aumentoXPared = pared.renderer.bounds.size.x;
        float aumentoYPared = pared.renderer.bounds.size.y;
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoXInterseccion = interserctor.renderer.bounds.size.x;
        float aumentoYInterseccion = interserctor.renderer.bounds.size.y;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;

        devolver.x = origenMaze.x + ((aumentoZPared + aumentoXInterseccion) * (z)) + aumentoZPared / 2 + aumentoZInterseccion;
        devolver.z = origenMaze.z + ((aumentoZPared + aumentoZInterseccion) * (x)) + aumentoZPared / 2 + aumentoZInterseccion;
        devolver.y = 1.09f;

        return devolver;
    }
    public float getTamCasilla()
    {
        float aumentoZPared = pared.renderer.bounds.size.z;
        float aumentoZInterseccion = interserctor.renderer.bounds.size.z;

        return aumentoZPared + aumentoZInterseccion;
    }

}
