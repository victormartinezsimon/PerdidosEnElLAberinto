using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager:MonoBehaviour
{
    public string tagName = "ServidorPeridosEnElLaberinto";
    private HostData[] listaHost;

    public int numPlayers = 4;
    public string _ip = "127.0.0.1";
    private string _port = "25001";
    private string _porcentajeTraps = "10";
    private string m_player1Name = "Player";
    private string m_player2Name = "Player(2)";
    private Vector3 m_player1Color = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 m_player2Color = new Vector3(0.5f, 0.5f, 0.5f);
    private string separador = "#";
       
    /// <summary>
    /// the gameobjects to instantiate
    /// </summary>
    public GameObject jugadorGameObject;

    private int m_seed;    
    private int m_idTemporal=-1;
    private float timeAcum = 0;
   
    float anchoCampo;
    float altoCampo;
    public float anchoCampoBase = 5;
    public float altoCampoBase = 5;
    public float maximoAnchoAlto = 20.0f;
    public float minimoAnchoAlto = 5.0f;
    private bool error = false;
    private bool tabPulsado = false;

    private List<string> m_playersName= new List<string>();
    private List<Vector3> m_playersColours = new List<Vector3>();
    private int[] m_playersPosition;
    private List<GameObject> jugadoresInstanciados;
    private Vector3 prizeInstanciado;
    int playersReady = 0;
    public Color m_colorIA;

    #region GuiConfiguration
    public Texture2D texturaFondo;
    public Texture2D[] texturasPlayers;
    public int maximoCaracteres=10;
    public Texture2D m_texturaBoton;
    public Texture2D[] m_texturaFlecha;
    public Texture2D texturaTextField;
    public Font fuentBoton;
    public Texture2D[] m_checkBox;
    public Texture2D m_sepradorPantallas;
    #endregion

    public bool snapshot = false;
    int m_porcentajeQuitaVidas = 10;

    public bool playerInstanciado = false;

    private bool m_pantallaDividida = false;
    private bool m_withIA = false;

    #region elementosEscena
    public string playerColorName = "PlayerMenu";
    private GameObject playerColor;
    #endregion


    #region Accesores
    public int Ancho
    {
        get { return Mathf.RoundToInt(anchoCampo); }
    }
    public int Alto
    {
        get { return Mathf.RoundToInt(altoCampo); }
    }
    public int Seed
    {
        get { return m_seed; }
        set { m_seed = value; }
    }
    public int PorcentajeQuitaVidas
    {
        get { return m_porcentajeQuitaVidas; }
        set { m_porcentajeQuitaVidas = value; }
    }
    public bool SplitScreen
    {
        get { return m_pantallaDividida; }
    }
    #endregion

    /// <summary>
    /// None
    /// WaitingConnection-> in server when waiting for users
    /// CallToStart-> in server and client-> when all the stuff to load level and instantiate players
    /// Game-> in server and client-> in game
    /// RefreshHostList-> in client when presh button to refresh
    /// WaitingToStart-> in client, waiting to start the level
    /// </summary>
    private enum GameState{None,WaitingConnection,CallToStart,Game,WaitingToStart,DirectConnection,Results,Configuring,RefreshHost,Configuring2,PlayerInfo,PlayerInfo2};

    private GameState actualState = GameState.None;

    void Start()
    {
        GameObject[] aux = GameObject.FindGameObjectsWithTag("Network");
        if (aux.Length != 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);

        playerColor = GameObject.Find(playerColorName);
    }
    void Update()
    {
        if (playerColor == null)
        {
            playerColor = GameObject.Find(playerColorName);
        }


        if (actualState==GameState.RefreshHost)
        {
            if (MasterServer.PollHostList().Length > 0)
            {
                listaHost = MasterServer.PollHostList();
                checkHostList();
                MasterServer.ClearHostList();
            }                
        }
        
        if (actualState == GameState.Game)
        {
            timeAcum += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                tabPulsado = true;
            }
            if (Input.GetKeyUp(KeyCode.Tab))
            {
                tabPulsado = false;
            }
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                exitGame();
            }


        }

        if (actualState == GameState.None)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

    }
    public bool isClient()
    {
        return Network.isClient;
    }
    public void habilitarComponentes(bool val)
    {
        for (int i = 0; i < jugadoresInstanciados.Count; i++)
        {
            Info info = jugadoresInstanciados[i].GetComponent<Info>();

            if (info.MINE)
            {
                jugadoresInstanciados[i].GetComponent<MovementController>().enabled = val;
                jugadoresInstanciados[i].GetComponent<AnimationController>().enabled = val;
                if (snapshot)
                {
                    jugadoresInstanciados[i].GetComponent<TransformUpdate>().enabled = false;
                    jugadoresInstanciados[i].GetComponent<Interpolate>().enabled = val;
                }
                else
                {
                    jugadoresInstanciados[i].GetComponent<TransformUpdate>().enabled = val;
                    jugadoresInstanciados[i].GetComponent<Interpolate>().enabled = false;
                }
            }
        }


        
    }
    public void habilitarComponentes(bool val,GameObject go)
    {
        go.GetComponent<MovementController>().enabled = val;
        go.GetComponent<AnimationController>().enabled = val;
        if (snapshot)
        {
            go.GetComponent<TransformUpdate>().enabled = false;
            go.GetComponent<Interpolate>().enabled = val;
        }
        else
        {
            go.GetComponent<TransformUpdate>().enabled = val;
            go.GetComponent<Interpolate>().enabled = false;
        }
    }
    public void restartGame()
    {
        m_playersName.Clear();
        m_playersPosition = new int[numPlayers];
        if(jugadoresInstanciados!= null && jugadoresInstanciados.Count !=0)
            jugadoresInstanciados.Clear();
        tabPulsado = false;
        tabCalculado = false;
        anchoCampo = 10;
        altoCampo = 10;
        playersReady = 0;
        timeAcum = 0;
        listaHost = null;
        //MasterServer.ClearHostList();
        actualState = GameState.None;
        playerInstanciado = false;
        m_idTemporal = -1;
        m_pantallaDividida = false;
        m_withIA = false;
    }
  
    #region mensajes
    //funciones a llamar por los botones
    private void startServer()
    {
        NetworkConnectionError errorNet;
        int resta = 1;

        if (m_pantallaDividida)
        {
            resta++;
        }

        errorNet=Network.InitializeServer(numPlayers-resta, int.Parse(_port), !Network.HavePublicAddress());

        if (errorNet == NetworkConnectionError.CreateSocketOrThreadFailure)
        {
            error = true;
        }

    }
    private void refreshHostList()
    {
        MasterServer.ClearHostList();
        listaHost = null;
        actualState = GameState.RefreshHost;
        MasterServer.RequestHostList(tagName);
        

    }
    private void conectToServer(int i)
    {
        Network.Connect(listaHost[i]);
    }
    
    //MENSAJES
    public void OnServerInitialized()
    {
        //Debug.Log("Server initialized and ready");
        //esto es para que se nos pueda encontrar
        MasterServer.RegisterHost(tagName, "Servidor de: " + m_player1Name, "Juego de perdidos en el laberinto");
        m_playersName.Add(m_player1Name);
        m_playersColours.Add(m_player1Color);

        if (m_pantallaDividida)
        {
            m_playersName.Add(m_player2Name);
            m_playersColours.Add(m_player2Color);
        }

        actualState = GameState.WaitingConnection;
        error = false;
       
    }
    public void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.RegistrationSucceeded)
        {
            //Debug.Log("Ya se nos puede encontrar");
        }
    }
    public void OnConnectedToServer()
    {
        //llamado en un cliente
        Debug.Log("On connected to server");
        actualState = GameState.WaitingToStart;
    }
    public void OnPlayerConnected(NetworkPlayer player)
    {
        //llamado en el server
        if (actualState == GameState.WaitingConnection)
        {
            networkView.RPC("whatsYourInfo", player);
        }
        else
        {
            networkView.RPC("closeConnectionByServer",player);
            Network.CloseConnection(player, true);
        }
        
     }
    public void OnPlayerDisconnected(NetworkPlayer player) { }
    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        if (Network.isServer)
            Debug.Log("Local server connection disconnected");
        else
            if (info == NetworkDisconnection.LostConnection)
            {
                actualState = GameState.None;
            }
            else
            {
                if (actualState != GameState.Results)
                {
                    actualState = GameState.None;
                    Debug.Log("Successfully diconnected from the server");
                }
                
            }
    }
    [RPC]
    void closeConnectionByServer()
    {
        actualState = GameState.None;
        Application.LoadLevel("lobby");
        restartGame();
    }
    
    #endregion
    #region GUI
    void OnGUI()
    {
        switch (actualState)
        {
            case GameState.None: OnGuiNone(); break;
            case GameState.Game: OnGuiGame(); break;
            case GameState.CallToStart: OnGuiCallToStart(); break;
            case GameState.DirectConnection: OnGuiDirectConnection(); break;
            case GameState.RefreshHost: OnGuiRefreshHost(); break;
            case GameState.Results: OnGuiResults(); break;
            case GameState.WaitingConnection: OnGuiWaitingConnection(); break;
            case GameState.WaitingToStart: OnGuiWaitingToStart(); break;
            case GameState.Configuring: OnGuiConfiguring(); break;
            case GameState.Configuring2: OnGuiConfiguring2(); break;
            case GameState.PlayerInfo: OnGuiPlayerInfo(); break;
            case GameState.PlayerInfo2: OnGuiPlayerInfo2(); break;
        }
    }

    void OnGuiNone()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);
        
        /*
        construyeLabel(new Rect(0.3f * Screen.width, 0.2f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), "Player 1:",Color.black);
        m_player1Name = construyeTextField(new Rect(0.45f * Screen.width, 0.18f * Screen.height, 0.3f * Screen.width, 0.1f * Screen.height), m_player1Name);
        if (m_player1Name.Length > maximoCaracteres)
        {
            m_player1Name = m_player1Name.Substring(0, maximoCaracteres);
        }
        */

        if (construyeButton(new Rect(0.15f * Screen.width, 0.15f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height), 
            "Start Sever"))
        {
            anchoCampo = anchoCampoBase;
            altoCampo = altoCampoBase;
            actualState = GameState.Configuring;
        }
        if (construyeButton(new Rect(0.55f * Screen.width, 0.15f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                "Join to game"))
        {
            refreshHostList();
        }            
        if (construyeButton(new Rect(0.3f * Screen.width, 0.35f * Screen.height, 0.4f * Screen.width, 0.2f * Screen.height),
            "Direct Connection"))
        {
            actualState = GameState.DirectConnection;
        }
        if (construyeButton(new Rect(0.25f * Screen.width, 0.6f * Screen.height, 0.5f * Screen.width, 0.3f * Screen.height),
            "Personalizate Player(s)"))
        {
            actualState = GameState.PlayerInfo;
        }
        
    }
    
    bool tabCalculado = false;
    float anchoTab;
    float altoTab;
    void OnGuiGame() 
    {
        float origenX = 0.05f * Screen.width;
        float origenY = 0.05f * Screen.height;
        float separacion = 0.05f * Screen.height;
        float widthTexto = 0.1f * Screen.width;
        float heighTexto = 0.1f * Screen.height;

        if (!tabCalculado)
        {
            tabCalculado = true;            
            float yActual;
            float xActual;
            
            //yActual = origenY + aumento;
            yActual = origenY;
            xActual = float.MinValue;
            Vector2 size;

            GameObject spawn=GameObject.Find("SpawnPoint2");
            
            float distancia = (prizeInstanciado - spawn.transform.position).sqrMagnitude;
            int value = Mathf.RoundToInt(distancia);


            for (int i = 0; i < m_playersName.Count; i++)
            {
                size = getLabelSize(m_playersName[i]);
                xActual = Mathf.Max(xActual,size.x);
                yActual += size.y;

                string texto = value.ToString() + " m";
                size = getLabelSize(texto);
                xActual = Mathf.Max(xActual, size.x);
                yActual += size.y;

            }

            anchoTab = xActual + separacion * 2;
            altoTab = yActual + separacion;

        }

        if (tabPulsado)
        {
            calcularTodasLasDistancias();

            float yActual;
            float xActual;
            yActual = origenY;
            xActual = origenX;
           
            //fondo
            GUI.DrawTexture(new Rect(origenX,origenY,anchoTab,altoTab), texturaFondo);

            xActual += separacion;
            yActual += separacion;

            Vector2 size;
            
            for (int i = 0; i < m_playersName.Count; i++)
            {
                construyeLabel(new Rect(xActual, yActual, widthTexto, heighTexto), m_playersName[i], getColor(i));
                size = getLabelSize(m_playersName[i]);
                yActual += size.y;
                construyeLabel(new Rect(xActual, yActual, widthTexto, heighTexto), m_playersPosition[i].ToString() + " m", getColor(i));
                size = getLabelSize(m_playersPosition[i].ToString());
                yActual += size.y;
            }
            

        }


        if (m_pantallaDividida)
        {
            GUI.DrawTexture(new Rect(0.49f * Screen.width, 0, 0.02f * Screen.width, Screen.height), m_sepradorPantallas);
        }
               
    }
    private Color getColor(int id)
    {
        return new Color(m_playersColours[id].x, m_playersColours[id].y, m_playersColours[id].z);
    }

    void OnGuiCallToStart() { }
    void OnGuiDirectConnection()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);
        construyeLabel(new Rect(0.23555f * Screen.width, 0.3f * Screen.height, 0.3f * Screen.width, 0.1f * Screen.height),"IP",Color.black);
        construyeLabel(new Rect(0.72f * Screen.width, 0.3f * Screen.height, 0.3f * Screen.width, 0.1f * Screen.height), "Port", Color.black);

        _ip = construyeTextField(new Rect(0.1f * Screen.width, 0.4f * Screen.height, 0.3f * Screen.width, 0.1f * Screen.height), _ip);
        _port = construyeTextField(new Rect(0.6f * Screen.width, 0.4f * Screen.height, 0.3f * Screen.width, 0.1f * Screen.height), _port);


        if (construyeButton(new Rect(0.15f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                 "Connect"))
        {
            Network.Connect(_ip, int.Parse(_port));
        }

        if (construyeButton(new Rect(0.55f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                 "Back"))
        {
            actualState = GameState.None;
        }
    }
    void OnGuiRefreshHost()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);

        float origenY=0.1f*Screen.height;
        float separacionY=0.05f*Screen.height;
        float alto= 0.2f*Screen.height;

        float posY=origenY;

        if (listaHost != null && listaHost.Length > 0)
        {
            for (int i = 0; i < listaHost.Length; i++)
            {
                if (construyeButton(new Rect(0.2f*Screen.width,posY,0.6f*Screen.width,alto),  listaHost[i].gameName))
                {
                    conectToServer(i);
                }
                posY += separacionY + alto;
            }
        }
        else
        {
            construyeLabel(new Rect(Screen.width * 0.25f, Screen.height * 0.1f, Screen.width * 0.4f, Screen.height * 0.3f),
                "There are no servers avaliable", Color.black, 30);
        }

        if (construyeButton(new Rect(0.15f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
             "Refresh"))
        {
            refreshHostList();
        }

        if (construyeButton(new Rect(0.55f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                 "Back"))
        {
            listaHost = null;
            //MasterServer.ClearHostList();
            actualState = GameState.None;
        }
    }
    void OnGuiResults()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);

        if (construyeButton(new Rect(0.4f * Screen.width, 0.75f * Screen.height, 0.2f * Screen.width, 0.2f * Screen.height),
                 "Back"))
        {
            Network.Disconnect();
            restartGame();     
        }
        Vector2 tam = getLabelSize("Game Over");
        construyeLabel(new Rect(Screen.width*0.5f - tam.x/2, Screen.height*0.05f, tam.x, tam.y), "Game Over", Color.red,40);
        tam=getLabelSize("Total Time: " + Mathf.RoundToInt(timeAcum) + " s");
        construyeLabel(new Rect(Screen.width * 0.5f - tam.x / 2, Screen.height * 0.15f, tam.x, tam.y), "Total Time: " + Mathf.RoundToInt(timeAcum) + " s", Color.red, 40);

        float origenX = 0.1f * Screen.width;
        float origenY = 0.25f * Screen.height;
        float separacion = 0.05f * Screen.width;
        float tamImagenX = Screen.width - 2 * origenX - (numPlayers - 1) * separacion;
        tamImagenX = tamImagenX / numPlayers;
        float tamImagenY = 0.4f * Screen.height;
        float tamLabelY = 0.1f * Screen.height;

        for (int i = 0; i < numPlayers; i++)
        {
            if (i < m_playersName.Count)
            {
                //there is a player      
                float tamLabelX = getLabelSize(m_playersName[i]).x;
                float tamResultX = getLabelSize(m_playersPosition[i] + " m").x;
                GUI.DrawTexture(new Rect(origenX, origenY, tamImagenX, tamImagenY), texturasPlayers[i]);
                construyeLabel(new Rect(origenX + tamImagenX / 2 - tamLabelX / 2, origenY + tamImagenY * 1.0f, tamImagenX, tamLabelY), m_playersName[i], getColor(i));
                construyeLabel(new Rect(origenX + tamImagenX / 2 - tamResultX / 2, origenY + tamImagenY * 1.1f, tamImagenX, tamLabelY), m_playersPosition[i] + " m", getColor(i));

            }
            else
            {
                //no player
                GUI.DrawTexture(new Rect(origenX, origenY, tamImagenX, tamImagenY), texturasPlayers[texturasPlayers.Length - 1]);

            }
            origenX += tamImagenX + separacion;

        }



    }
    void OnGuiWaitingConnection()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);
       
        if (construyeButton(new Rect(0.15f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                 "Start Game"))
        {
            startGameServer();
        }

        if (construyeButton(new Rect(0.55f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                 "Back"))
        {
            Network.Disconnect();
            m_playersName.Clear();
            actualState = GameState.Configuring;
        }
        float origenX = 0.1f * Screen.width;
        float origenY = 0.25f * Screen.height;
        float separacion = 0.05f * Screen.width;
        float tamImagenX = Screen.width - 2 * origenX - (numPlayers - 1) * separacion;
        tamImagenX = tamImagenX / numPlayers;
        float tamImagenY = 0.4f * Screen.height;
        float tamLabelY= 0.1f*Screen.height;

        for (int i = 0; i < numPlayers; i++)
        {
            if (i < m_playersName.Count)
            {
                //there is a player      
                float tamLabelX = getLabelSize(m_playersName[i]).x;
                GUI.DrawTexture(new Rect(origenX,origenY, tamImagenX, tamImagenY), texturasPlayers[i]);
                construyeLabel(new Rect(origenX + tamImagenX / 2 - tamLabelX / 2, origenY + tamImagenY * 1.0f, tamImagenX, tamLabelY), m_playersName[i], getColor(i));
                               
            }
            else
            {
                //no player
                GUI.DrawTexture(new Rect(origenX, origenY, tamImagenX, tamImagenY), texturasPlayers[texturasPlayers.Length-1]);

            }
            origenX += tamImagenX + separacion;

        }

    }
    void OnGuiWaitingToStart()
    {
        GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), texturaFondo);
        

        if (construyeButton(new Rect(0.35f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                "Back"))
        {
            string msg = m_player1Name + separador;
            msg += m_player1Color.x + separador;
            msg += m_player1Color.y + separador;
            msg += m_player1Color.z;

            networkView.RPC("removeConnectedUser", RPCMode.Server, msg);
            Network.Disconnect();
            m_playersName.Clear();
            actualState = GameState.None;
        }
        float origenX = 0.1f * Screen.width;
        float origenY = 0.2f * Screen.height;
        float separacion = 0.05f * Screen.width;
        float tamImagenX = Screen.width - 2 * origenX - (numPlayers - 1) * separacion;
        tamImagenX = tamImagenX / numPlayers;
        float tamImagenY = 0.4f * Screen.height;
        float tamLabelY = 0.1f * Screen.height;

        for (int i = 0; i < numPlayers; i++)
        {
            if (i < m_playersName.Count)
            {
                //there is a player
                float tamLabelX = getLabelSize(m_playersName[i]).x;
                GUI.DrawTexture(new Rect(origenX, origenY, tamImagenX, tamImagenY), texturasPlayers[i]);
                construyeLabel(new Rect(origenX + tamImagenX / 2 - tamLabelX / 2, origenY + tamImagenY * 1.0f, tamImagenX, tamLabelY), m_playersName[i], getColor(i));
                
            }
            else
            {
                //no player
                GUI.DrawTexture(new Rect(origenX, origenY, tamImagenX, tamImagenY), texturasPlayers[texturasPlayers.Length - 1]);
            }
            origenX += tamImagenX + separacion;

        }
        
    }
    void OnGuiConfiguring()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        style.fontSize = 20;

        construyeLabel(new Rect(0.2f * Screen.width, 0.2f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Width: " + transformaValue(anchoCampo), Color.black);
        anchoCampo = GUI.HorizontalSlider(new Rect(0.2f * Screen.width, 0.3f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), anchoCampo, minimoAnchoAlto, maximoAnchoAlto);

        construyeLabel(new Rect(0.2f * Screen.width, 0.35f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Heigh: " + transformaValue(altoCampo), Color.black);
        altoCampo = GUI.HorizontalSlider(new Rect(0.2f * Screen.width, 0.45f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), altoCampo, minimoAnchoAlto, maximoAnchoAlto);

        construyeLabel(new Rect(0.2f * Screen.width, 0.55f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Port:", Color.black);
        _port = construyeTextField(new Rect(0.3f * Screen.width, 0.55f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), _port);

        construyeLabel(new Rect(0.45f * Screen.width, 0.55f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "% Traps:", Color.black);
        _porcentajeTraps = construyeTextField(new Rect(0.6f * Screen.width, 0.55f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), _porcentajeTraps);

        if (!int.TryParse(_porcentajeTraps, out m_porcentajeQuitaVidas))
        {
            m_porcentajeQuitaVidas = 0;
            _porcentajeTraps = "0";
        }
        else
        {
            if (m_porcentajeQuitaVidas >= 100)
            {
                m_porcentajeQuitaVidas = 99;
                _porcentajeTraps = "99";
            }
        }

        //button more options
        if (construyeButtonFlecha(new Rect(0.8f * Screen.width, 0.55f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height),1))
        {
            actualState = GameState.Configuring2;
        }
        //button more options
        if (construyeButtonFlecha(new Rect(0.8f * Screen.width, 0.2f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), 0))
        {
            actualState = GameState.Configuring;
        }

        
        if (construyeButton(new Rect(0.15f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
             "Start server"))
        {
            startServer();
        }

        if (construyeButton(new Rect(0.55f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
            "Back"))
        {
            Network.Disconnect();
            error = false;
            m_playersName.Clear();
            actualState = GameState.None;
        }

        if (error)
        {
            construyeLabel(new Rect(0.2f * Screen.width, 0.65f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "There was an error starting the server",Color.black);
        }
       
    }
    void OnGuiConfiguring2()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaFondo);
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        style.fontSize = 20;

        //button more options
        if (construyeButtonFlecha(new Rect(0.8f * Screen.width, 0.55f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), 1))
        {
            actualState = GameState.Configuring2;
        }
        //button more options
        if (construyeButtonFlecha(new Rect(0.8f * Screen.width, 0.2f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), 0))
        {
            actualState = GameState.Configuring;
        }

        construyeLabel(new Rect(0.2f * Screen.width, 0.3f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), "Add PC Players?", Color.black);
        m_withIA = checkBox(new Rect(0.5f * Screen.width, 0.31f * Screen.height, 0.05f * Screen.width, 0.05f * Screen.height), m_withIA);

        if (construyeButton(new Rect(0.15f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
             "Start server"))
        {
            startServer();
        }

        if (construyeButton(new Rect(0.55f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
            "Back"))
        {
            Network.Disconnect();
            error = false;
            m_playersName.Clear();
            actualState = GameState.None;
        }

        if (error)
        {
            construyeLabel(new Rect(0.2f * Screen.width, 0.65f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "There was an error starting the server", Color.black);
        }

    }
    void OnGuiPlayerInfo()
    {
        construyeLabel(new Rect(0.05f * Screen.width, 0.2f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), "Player 1:", Color.black);
        m_player1Name = construyeTextField(new Rect(0.2f * Screen.width, 0.18f * Screen.height, 0.3f * Screen.width, 0.1f * Screen.height), m_player1Name);
        if (m_player1Name.Length > maximoCaracteres)
        {
            m_player1Name = m_player1Name.Substring(0, maximoCaracteres);
        }
        construyeLabel(new Rect(0.2f * Screen.width, 0.3f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), "Color:", Color.black);

        construyeLabel(new Rect(0.2f * Screen.width, 0.4f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Red", Color.black);
        m_player1Color.x = GUI.HorizontalSlider(new Rect(0.15f * Screen.width, 0.5f * Screen.height, 0.2f * Screen.width, 0.1f * Screen.height), m_player1Color.x, 0, 1);

        construyeLabel(new Rect(0.2f * Screen.width, 0.55f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Green", Color.black);
        m_player1Color.y = GUI.HorizontalSlider(new Rect(0.15f * Screen.width, 0.65f * Screen.height, 0.2f * Screen.width, 0.1f * Screen.height), m_player1Color.y, 0, 1);

        construyeLabel(new Rect(0.2f * Screen.width, 0.7f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Blue", Color.black);
        m_player1Color.z = GUI.HorizontalSlider(new Rect(0.15f * Screen.width, 0.8f * Screen.height, 0.2f * Screen.width, 0.1f * Screen.height), m_player1Color.z, 0, 1);

        Renderer render = playerColor.GetComponentInChildren<Renderer>();
        render.material.SetVector("_ColorPlayer", new Vector4(m_player1Color.x, m_player1Color.y, m_player1Color.z, 1.0f));

        if (construyeButton(new Rect(0.4f * Screen.width, 0.75f * Screen.height, 0.2f * Screen.width, 0.2f * Screen.height),
                "Back"))
        {
            actualState = GameState.None;
        }
        if (construyeButton(new Rect(0.65f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                "Split Screen"))
        {
            actualState = GameState.PlayerInfo2;
            m_pantallaDividida = true;
        }
    }

    void OnGuiPlayerInfo2()
    {
        construyeLabel(new Rect(0.05f * Screen.width, 0.2f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), "Player 2:", Color.black);
        m_player2Name = construyeTextField(new Rect(0.2f * Screen.width, 0.18f * Screen.height, 0.3f * Screen.width, 0.1f * Screen.height), m_player2Name);
        if (m_player2Name.Length > maximoCaracteres)
        {
            m_player2Name = m_player2Name.Substring(0, maximoCaracteres);
        }

        if (m_player1Name == m_player2Name)
        {
            m_player2Name += "(2)";
        } 
        construyeLabel(new Rect(0.2f * Screen.width, 0.3f * Screen.height, 0.1f * Screen.width, 0.1f * Screen.height), "Color:", Color.black);

        construyeLabel(new Rect(0.2f * Screen.width, 0.4f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Red", Color.black);
        m_player2Color.x = GUI.HorizontalSlider(new Rect(0.15f * Screen.width, 0.5f * Screen.height, 0.2f * Screen.width, 0.1f * Screen.height), m_player2Color.x, 0, 1);

        construyeLabel(new Rect(0.2f * Screen.width, 0.55f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Green", Color.black);
        m_player2Color.y = GUI.HorizontalSlider(new Rect(0.15f * Screen.width, 0.65f * Screen.height, 0.2f * Screen.width, 0.1f * Screen.height), m_player2Color.y, 0, 1);

        construyeLabel(new Rect(0.2f * Screen.width, 0.7f * Screen.height, 0.5f * Screen.width, 0.1f * Screen.height), "Blue", Color.black);
        m_player2Color.z = GUI.HorizontalSlider(new Rect(0.15f * Screen.width, 0.8f * Screen.height, 0.2f * Screen.width, 0.1f * Screen.height), m_player2Color.z, 0, 1);

        Renderer render = playerColor.GetComponentInChildren<Renderer>();
        render.material.SetVector("_ColorPlayer", new Vector4(m_player2Color.x, m_player2Color.y, m_player2Color.z, 1.0f));

        if (construyeButton(new Rect(0.4f * Screen.width, 0.75f * Screen.height, 0.2f * Screen.width, 0.2f * Screen.height),
                "Confirm"))
        {
            actualState = GameState.None;
        }
        if (construyeButton(new Rect(0.65f * Screen.width, 0.75f * Screen.height, 0.3f * Screen.width, 0.2f * Screen.height),
                "Cancel"))
        {
            actualState = GameState.PlayerInfo;
            m_pantallaDividida = false;
        }
    }
     
    private int transformaValue(float f)
    {
        return Mathf.RoundToInt(f);
    }

    private bool construyeButton(Rect position, string texto,float margen=0.045f,float tam=0.1f)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.white;
        styleLabel.alignment = TextAnchor.MiddleCenter;
        styleLabel.font = fuentBoton;

        GUI.DrawTexture(position, m_texturaBoton);
        if (GUI.Button(new Rect(position.x + margen * Screen.width, 
            position.y + margen*Screen.height, 
            position.width - tam * Screen.width,
            position.height - tam * Screen.height), 
            texto, styleLabel))
        {
            return true;
        }
        return false;
    }
    private bool construyeButtonFlecha(Rect position,int indice)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.white;
        styleLabel.alignment = TextAnchor.MiddleCenter;
        styleLabel.font = fuentBoton;

        GUI.DrawTexture(position, m_texturaFlecha[indice]);
        if (GUI.Button(new Rect(position.x,
            position.y ,
            position.width ,
            position.height),"",styleLabel))
        {
            return true;
        }
        return false;
    }
    private void construyeLabel(Rect position, string texto,Color color,int tamFont=25)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = tamFont;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = color;
        styleLabel.font = fuentBoton;

        GUI.Label(position, texto,styleLabel);
    }
    private Vector2 getLabelSize(string texto)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.black;
        styleLabel.font = fuentBoton;

        return styleLabel.CalcSize(new GUIContent(texto));
    }
    private string construyeTextField(Rect position, string value,float separacion=0.05f)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.white;
        styleLabel.alignment = TextAnchor.MiddleCenter;
        styleLabel.font = fuentBoton;

        GUI.DrawTexture(position, texturaTextField);

        value = GUI.TextField(position, value,styleLabel);
        return value;
    }
    private bool checkBox(Rect position, bool valActual)
    {
        GUIStyle styleLabel = new GUIStyle();
        styleLabel.fontSize = 25;
        styleLabel.fontStyle = FontStyle.Bold;
        styleLabel.normal.textColor = Color.white;
        styleLabel.alignment = TextAnchor.MiddleCenter;
        styleLabel.font = fuentBoton;
        if (valActual)
        {
            GUI.DrawTexture(position, m_checkBox[0]);
        }
        else
        {
            GUI.DrawTexture(position, m_checkBox[1]);
        }
        if (GUI.Button(new Rect(position.x,
            position.y,
            position.width,
            position.height), "", styleLabel))
        {
            return !valActual;
        }
        return valActual;

    }


    #endregion
    #region Gestion Inicio partida
    //gestiones para empezar
    /// <summary>
    /// Called on server when he desires to start the game
    /// </summary>
    public void startGameServer()
    {
        if (Network.isServer)
        {
            Debug.Log("start game sever");

            int ticks=(int)System.DateTime.Now.Ticks;
            Seed = ticks;
            string msg = Ancho + separador + Alto + separador + Seed + separador + m_porcentajeQuitaVidas + separador + m_withIA;        
            networkView.RPC("startGameClient", RPCMode.Others,msg);
            sendAllID();

            Application.LoadLevel("maze");
            actualState = GameState.CallToStart;
        }        
    }
    /// <summary>
    /// Called on client when the servers says to start the game
    /// </summary>
    [RPC]
    public void startGameClient(string msg)
    {
        if (Network.isClient)
        {
            Debug.Log("starg game client");
            string[] msgSeparado = msg.Split(separador.ToCharArray());
            anchoCampo = float.Parse(msgSeparado[0]);
            altoCampo = float.Parse(msgSeparado[1]);
            Seed =int.Parse(msgSeparado[2]);
            PorcentajeQuitaVidas = int.Parse(msgSeparado[3]);
            m_withIA = bool.Parse(msgSeparado[4]);
            Application.LoadLevel("maze");
            actualState = GameState.CallToStart;
        }        
    }
    /*
    /// <summary>
    /// Called on server when he has just started the game
    /// </summary>
    public void mazeGeneratedServer()
    {
        if (Network.isServer)
        {
            for (int i = 1; i < Network.connections.Length+1; i++)//el 0 es siempre el server
            {
                //networkView.RPC("instanciaPlayerClient2", Network.connections[i - 1], i);//ojo al -1
                networkView.RPC("yourId", Network.connections[i-1], i);//ojo al -1
            }
            ID = 0;            
        }
    }
    */
  
    /// <summary>
    /// Called on server when he has just started the game
    /// </summary>
    public void sendAllID()
    {
        if (Network.isServer)
        {
            m_idTemporal = 0;
            
            for (int i = 1; i < Network.connections.Length + 1; i++)//el 0 es siempre el server
            {
                if (m_pantallaDividida)
                {
                    networkView.RPC("yourId", Network.connections[i - 1], i+1);//ojo al -1
                }
                else
                {
                    networkView.RPC("yourId", Network.connections[i - 1], i);//ojo al -1
                }
            }            
        }
    }
 
    public void instanciaAllPlayers()
    {
        for (int i = 0; i < m_playersName.Count; i++)
        {
            GameObject spawn = GameObject.Find("SpawnPoint" + i);
            if (spawn != null)
            {
                GameObject player = Instantiate(jugadorGameObject, spawn.transform.position, Quaternion.identity) as GameObject;
                Renderer render = player.GetComponentInChildren<Renderer>();
                render.material.SetVector("_ColorPlayer", new Vector4(m_playersColours[i].x, m_playersColours[i].y, m_playersColours[i].z,1.0f));
                Info info = player.GetComponent<Info>();
                info.ID = i;
                info.IA = false;
                if (m_pantallaDividida)
                {
                    if (i == m_idTemporal)
                    {
                        Camera camera = GameObject.FindGameObjectsWithTag("MainCamera")[0].camera;
                        camera.GetComponent<CameraController>().Target = player;

                        camera.rect = new Rect(0, 0, 0.5f, 1f);

                        playerInstanciado = true;
                        habilitarComponentes(false, player);
                        info.MINE = true;
                    }
                    else
                    {
                        if (i == m_idTemporal +1 )
                        {
                            Camera camera = GameObject.FindGameObjectsWithTag("MainCamera")[1].camera;
                            camera.GetComponent<CameraController>().Target = player;
                            camera.rect = new Rect(0.5f, 0, 0.5f, 1f);
                            camera.GetComponent<AudioListener>().enabled = false;

                            playerInstanciado = true;
                            habilitarComponentes(false, player);
                            info.MINE = true;
                            
                        }
                        else
                        {
                            info.MINE = false;
                        }
                        
                    }
                }
                else
                {
                    if (i == m_idTemporal)
                    {
                        GameObject[] cameras = GameObject.FindGameObjectsWithTag("MainCamera");
                        cameras[1].camera.enabled = false;
                        cameras[1].GetComponent<AudioListener>().enabled = false;
                        Camera camera = cameras[0].camera;
                        camera.GetComponent<CameraController>().Target = player;
                        playerInstanciado = true;
                        habilitarComponentes(false, player);
                        info.MINE = true;
                    }
                    else
                    {
                        info.MINE = false;
                    }
                }                
            }

        }

        if(m_withIA)
            instanciaIA();
        

        networkView.RPC("playerEndInstanciacion", RPCMode.Server);
        playerEndInstanciacion();//sto es por si somos el servidor
    }

    private void instanciaIA()
    {
        //creamos los extra
        int cuenta = 0;
        for (int i = m_playersName.Count; i < numPlayers; i++)
        {
            GameObject spawn = GameObject.Find("SpawnPoint" + i);
            if (spawn != null)
            {
                GameObject player = Instantiate(jugadorGameObject, spawn.transform.position, Quaternion.identity) as GameObject;
                Renderer render = player.GetComponentInChildren<Renderer>();
                render.material.SetVector("_ColorPlayer", new Vector4(m_colorIA.r,m_colorIA.g,m_colorIA.b,1.0f));
                string name = "IA" + "(" + cuenta + ")";
                while (m_playersName.Contains(name))
                {
                    cuenta++;
                    name = "IA" + "(" + cuenta + ")";
                }


                m_playersName.Add(name);
            }
        }
    }

    [RPC]
    public void yourId(int id)
    {
        m_idTemporal = id;//almaceno cual es el id
        networkView.RPC("idRecived", RPCMode.Server, id);
    }
    [RPC]
    public void idRecived(int id)
    {
        Debug.Log("id recibido desde: " + id);
    }

    /// <summary>
    /// Called on server when a client its ready
    /// </summary>
    [RPC]
    public void playerEndInstanciacion()
    {
        if (Network.isServer)
        {
            Debug.Log("Player instantiated...");
            playersReady++;            
            if (playersReady >= Network.connections.Length+1)//+1 xq el server no esta registrado
            {
                findPlayersInstantiated();
                Debug.Log("Start game en el sever");
                actualState = GameState.Game;
                networkView.RPC("StartGame", RPCMode.Others);
                m_playersPosition = new int[m_playersName.Count];
                timeAcum = 0;
                habilitarComponentes(true);
                if(m_withIA)
                    habilitarIA();
            }   
        }             
    }
    /// <summary>
    /// Called on client to begin the game
    /// </summary>
    [RPC]
    public void StartGame()
    {
        
        if (Network.isClient)
        {
            findPlayersInstantiated();
            Debug.Log("Start game en el client");
            actualState = GameState.Game;
            m_playersPosition = new int[m_playersName.Count];
            timeAcum = 0;

            habilitarComponentes(true);

        }        
    }

    private void findPlayersInstantiated()
    {
        jugadoresInstanciados = new List<GameObject>();

        bool encontrado = false;
        int cuenta = 0;
        while (!encontrado && cuenta <numPlayers)
        {
            string name = jugadorGameObject.name + "(Clone)";
            GameObject go=GameObject.Find(name);
            if (go != null)
            {
                jugadoresInstanciados.Add(go);
                if (cuenta != m_idTemporal)
                {
                    if (snapshot)
                    {
                        go.GetComponent<TransformUpdate>().enabled = false;
                    }
                    else
                    {
                        go.GetComponent<Interpolate>().enabled = false;
                    }
                }
                go.name = m_playersName[cuenta];
                cuenta++;
            }
            else
            {
                encontrado = true;
            }
        }

        GameObject prize=GameObject.FindGameObjectWithTag("Prize");
        if (prize != null)
        {
            prizeInstanciado= prize.transform.position;
        }

        for (int i = 0; i < jugadoresInstanciados.Count; i++)
        {
            for(int j= 0; j< jugadoresInstanciados.Count; j++)
            {
                if (i != j)
                {
                    Physics.IgnoreCollision(jugadoresInstanciados[i].collider, jugadoresInstanciados[j].collider);
                }
            }
            /*
            if (i != ID)
            {
                Physics.IgnoreCollision(jugadoresInstanciados[i].collider, jugadoresInstanciados[ID].collider);
            }
            */
        }



    }

    private void habilitarIA()
    {
        if (!isClient())
        {
            IA.Giro[] giros = new IA.Giro[numPlayers - 1];
            for (int i = 0; i < giros.Length; i++)
            {
                int indice = i % 3;

                switch (indice)
                {
                    case 0: giros[i] = IA.Giro.Derecha; break;
                    case 1: giros[i] = IA.Giro.Izquierda; break;
                    case 2: giros[i] = IA.Giro.Aleatorio; break;
                }

            }            
            
            int cuenta = 0;
            if (m_pantallaDividida)
            {
                for (int i = Network.connections.Length + 2; i < numPlayers; i++)//+2 for the player2
                {
                    IA componente = jugadoresInstanciados[i].GetComponent<IA>();
                    componente.enabled = true;
                    componente.como_gira = giros[cuenta];
                    Info info = jugadoresInstanciados[i].GetComponent<Info>();
                    info.IA = true;
                    info.MINE = true;
                    info.ID = i;


                    cuenta++;
                }
            }
            else
            {
                for (int i = Network.connections.Length + 1; i < numPlayers; i++)
                {
                    IA componente = jugadoresInstanciados[i].GetComponent<IA>();
                    componente.enabled = true;
                    componente.como_gira = giros[cuenta];
                    Info info = jugadoresInstanciados[i].GetComponent<Info>();
                    info.IA = true;
                    info.MINE = true;
                    info.ID = i;


                    cuenta++;
                }
            }
        }

    }

    #endregion
    #region pantalaEspera
    /// <summary>
    /// Called on client when a user is connected in server
    /// </summary>
    /// <param name="conectedUser"></param>
    [RPC]
    public void setConnectedUser(string conectedUser,  NetworkMessageInfo info)
    {
        if (Network.isServer)
        {
            if (m_playersName.Contains(conectedUser))
            {
                bool encontrado = false;
                int cuenta = 1;
                string newName= conectedUser;
                while (!encontrado)
                {
                    newName = conectedUser + "(" + cuenta + ")";
                    if (!m_playersName.Contains(newName))
                    {
                        encontrado = true;
                    }
                    else
                    {
                        cuenta++;
                    }
                }

                m_playersName.Add(newName);
                
                networkView.RPC("yourNewName", info.sender, newName);
            }
            else
            {
                m_playersName.Add(conectedUser);
            }
            
            string mensaje = "";
            for (int i = 0; i < m_playersName.Count; i++)
            {
                mensaje += m_playersName[i] + separador;
            }
            networkView.RPC("setConnectedUser", RPCMode.Others, mensaje);
        }
        else
        {
            m_playersName.Clear();//clear the list of players name
            string[] aux= conectedUser.Split(separador.ToCharArray());
            for (int i = 0; i < aux.Length; i++)
            {
                if(aux[i].Length>0)
                    m_playersName.Add(aux[i]);
            }

        }
                
    }

    /// <summary>
    /// Called on client when a name is repeated
    /// </summary>
    /// <param name="msg"></param>
    [RPC]
    public void yourNewName(string msg)
    {
        if(isClient())
            m_player1Name = msg;
    }

    /// <summary>
    /// Called on client to ask for all info
    /// </summary>
    [RPC]
    public void whatsYourInfo()
    {
        string msg = m_player1Name + separador;
        msg += m_player1Color.x + separador;
        msg += m_player1Color.y + separador;
        msg += m_player1Color.z;
        networkView.RPC("setNewConnectedUser", RPCMode.Server, msg);
    }

    /// <summary>
    /// called on server to refresh the info
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="info"></param>
    [RPC]
    public void setNewConnectedUser(string msg, NetworkMessageInfo info)
    {
        if (Network.isServer)
        {
            string[] msgSeparado = msg.Split(separador.ToCharArray());

            string name = msgSeparado[0];
            Vector3 color= new Vector3(float.Parse(msgSeparado[1]),float.Parse(msgSeparado[2]),float.Parse(msgSeparado[3]));


            if (m_playersName.Contains(name))
            {
                bool encontrado = false;
                int cuenta = 1;
                string newName = name;
                while (!encontrado)
                {
                    newName = name + "(" + cuenta + ")";
                    if (!m_playersName.Contains(newName))
                    {
                        encontrado = true;
                    }
                    else
                    {
                        cuenta++;
                    }
                }

                m_playersName.Add(newName);

                networkView.RPC("yourNewName", info.sender, newName);
            }
            else
            {
                m_playersName.Add(name);
            }

            m_playersColours.Add(color);

            string mensaje = "";
            for (int i = 0; i < m_playersName.Count; i++)
            {
                mensaje += m_playersName[i] + separador;
                mensaje += m_playersColours[i].x + separador;
                mensaje += m_playersColours[i].y + separador;
                mensaje += m_playersColours[i].z + separador;

            }
            networkView.RPC("updateInfoPlayers", RPCMode.Others, mensaje);
        }

    }

    /// <summary>
    /// called in all client to refresh info
    /// </summary>
    /// <param name="msg"></param>
    [RPC]
    public void updateInfoPlayers(string msg)
    {
        string[] texto = msg.Split(separador.ToCharArray());

        m_playersName.Clear();//clear the list of players name
        m_playersColours.Clear();

        int infoTotal = texto.Length / 4;

        for (int i = 0; i < infoTotal; i++)
        {
            int indice = i * 4;
            m_playersName.Add(texto[indice]);
            Vector3 color = new Vector3(float.Parse(texto[1]), float.Parse(texto[2]), float.Parse(texto[3]));
            m_playersColours.Add(color);
        }

    }


    /// <summary>
    /// called on server when a client disconnects
    /// </summary>
    /// <param name="conectedUser"></param>
    [RPC]
    public void removeConnectedUser(string msg)
    {
        string[] separacion = msg.Split(separador.ToCharArray());
        string name = separacion[0];

        Debug.Log("Desconexion de:" + name);
        int indice = m_playersName.IndexOf(name);
        m_playersName.Remove(name);
        m_playersColours.RemoveAt(indice);

        string mensaje = "";
        for (int i = 0; i < m_playersName.Count; i++)
        {
            mensaje += m_playersName[i] + separador;
            mensaje += m_playersColours[i].x + separador;
            mensaje += m_playersColours[i].y + separador;
            mensaje += m_playersColours[i].z + separador;

        }
        networkView.RPC("updateInfoPlayers", RPCMode.Others, mensaje);
    }
    #endregion
    #region fin juego
    public int calcularDistanciaAGanador(int id,int idGanador)
    {
        GameObject jugador = jugadoresInstanciados[id];
        GameObject ganador = jugadoresInstanciados[idGanador];
        float distancia = (ganador.transform.position - jugador.transform.position).sqrMagnitude;
        return Mathf.RoundToInt(distancia)/5;
    }
    public int calcularDistanciaPremio(int id)
    {
        if (jugadoresInstanciados != null && jugadoresInstanciados.Count>id)
        {
            GameObject jugador = jugadoresInstanciados[id];
            float distancia = (prizeInstanciado - jugador.transform.position).sqrMagnitude;
            return Mathf.RoundToInt(distancia)/5;
        }

        return 0;
    }
    /// <summary>
    /// Called when anyone has win the game 
    /// </summary>
    /// <param name="nameWinner"></param>
    [RPC]
    public void endGame2(string nameWinner)
    {
        habilitarComponentes(false);

        int idGanador=0;
        //shearch for the winner
        bool encontrado = false;
        while (!encontrado && idGanador < m_playersName.Count)
        {
            if (m_playersName[idGanador] == nameWinner)
            {
                encontrado = true;
            }
            else
            {
                idGanador++;
            }
        }

        for (int i = 0; i < m_playersName.Count; i++)
        {
            m_playersPosition[i] = calcularDistanciaAGanador(i,idGanador);
        }
        actualState = GameState.Results;
        Application.LoadLevel("lobby");

        networkView.RPC("endGame2", RPCMode.Others,nameWinner);
    }

    #endregion
    #region Transform

    /// <summary>
    /// Called on every client to send where and how is he to the others players
    /// </summary>
    /// <param name="posicion"></param>
    /// <param name="orientacion"></param>
    public void sendTransform(Vector3 posicion,Vector3 vectorVelocidad,float velocidad,Vector3 orientacion,float deltaGiro, float velGiro,int id)
    {
        if (actualState == GameState.Game)
        {
            //Debug.Log("Send posicion de: " + ID + " = " + posicion);
            string mensaje = id.ToString();
            mensaje += separador + posicion.x + separador + posicion.y + separador + posicion.z;//posicion
            mensaje += separador + vectorVelocidad.x + separador + vectorVelocidad.y + separador + vectorVelocidad.z;//velocidad
            mensaje += separador + velocidad;
            mensaje += separador + orientacion.x + separador + orientacion.y + separador + orientacion.z; //quaternion
            mensaje += separador + deltaGiro;
            mensaje += separador + velGiro;
            networkView.RPC("receiveTransform", RPCMode.Others, mensaje);

            m_playersPosition[id] = calcularDistanciaPremio(id);
        }        

    }

    /// <summary>
    /// information from other player to tell me where he is
    /// </summary>
    /// <param name="mensage"></param>
    [RPC]
    public void receiveTransform(string mensage)
    {
        if (actualState == GameState.Game)
        {
            string[] info = mensage.Split(separador.ToCharArray());
            int id = int.Parse(info[0]);
            Vector3 pos = new Vector3(float.Parse(info[1]), float.Parse(info[2]), float.Parse(info[3]));
            Vector3 vectorVelocidad = new Vector3(float.Parse(info[4]), float.Parse(info[5]), float.Parse(info[6]));
            float velocidad = float.Parse(info[7]);
            Vector3 rotacion = new Vector3(float.Parse(info[8]), float.Parse(info[9]), float.Parse(info[10]));
            float deltaGiro = float.Parse(info[11]);
            float velGiro = float.Parse(info[12]);

            if (jugadoresInstanciados != null && jugadoresInstanciados.Count != 0)
            {
                jugadoresInstanciados[id].SendMessage("SetPosition", pos);
                jugadoresInstanciados[id].SendMessage("setVelocidad", velocidad);
                jugadoresInstanciados[id].SendMessage("setVectorDirectorVelocidad", vectorVelocidad);
                jugadoresInstanciados[id].SendMessage("SetRotation", rotacion);
                jugadoresInstanciados[id].SendMessage("setDeltaGiro", deltaGiro);
                jugadoresInstanciados[id].SendMessage("setVelocidadGiro", velGiro);

                m_playersPosition[id] = calcularDistanciaPremio(id);

            }
        }     
        
    }
    #endregion
    #region Snapshot

    public void sendSnapShotPosition(string msg,int id)
    {
        msg = id + separador + msg;
        networkView.RPC("receiveSnapShotPosition", RPCMode.Others, msg);
    }
    [RPC]
    public void receiveSnapShotPosition(string msg)
    {
        string[] palabras = msg.Split(separador.ToCharArray());
        int id = int.Parse(palabras[0]);
        string ruta = palabras[1];
        if (jugadoresInstanciados != null && jugadoresInstanciados.Count > id && jugadoresInstanciados[id] != null)
            jugadoresInstanciados[id].SendMessage("receiveSnapShotsPosition", ruta);
    }

    public void sendSnapShotRotation(string msg,int id)
    {        
        msg = id + separador + msg;
        networkView.RPC("receiveSnapShotRotation", RPCMode.Others, msg);
    }
    [RPC]
    public void receiveSnapShotRotation(string msg)
    {
        string[] palabras = msg.Split(separador.ToCharArray());
        int id = int.Parse(palabras[0]);
        string ruta = palabras[1];
        if (jugadoresInstanciados != null && jugadoresInstanciados.Count > id && jugadoresInstanciados[id] != null)
            jugadoresInstanciados[id].SendMessage("receiveSnapShotsRotation", ruta);
    
    }

    public void sendSnapShotOrientation(string msg, int id)
    {
        msg = id + separador + msg;
        networkView.RPC("receiveSnapShotOrientation", RPCMode.Others, msg);
    }
    [RPC]
    public void receiveSnapShotOrientation(string msg)
    {
        string[] palabras = msg.Split(separador.ToCharArray());
        int id = int.Parse(palabras[0]);
        string ruta = palabras[1];
        if(jugadoresInstanciados != null && jugadoresInstanciados.Count > id && jugadoresInstanciados[id] != null)
            jugadoresInstanciados[id].SendMessage("receiveSnapShotsOrientation", ruta);
    }

    public void calcularTodasLasDistancias()
    {
        for (int i = 0; i < jugadoresInstanciados.Count; i++)
        {
            m_playersPosition[i] = calcularDistanciaPremio(i);
        }
    }

    #endregion
    #region exitGame
    public void exitGame()
    {
        if (isClient())
        {
            //que se vaya?
            networkView.RPC("clientOut", RPCMode.Others, m_idTemporal);
            killGame();
        }
        else
        {            
            networkView.RPC("killGame", RPCMode.Others);
            killGame();
        }

    }
    [RPC]
    public void killGame()
    {
        //poner un mensaje...
        Application.LoadLevel("lobby");
        Network.Disconnect();
        restartGame();   
    }

    [RPC]
    public void clientOut(int id)
    {
        Debug.Log("Client out:" + id);
    }

    #endregion
    #region checkHostList
    private void checkHostList()
    {
        removeCompletedServers();

    }
    private void removeCompletedServers()
    {
        List<int> borrar = new List<int>();
        for (int i = 0; i < listaHost.Length; i++)
        {
            HostData aux = listaHost[i];
            if (aux.connectedPlayers == numPlayers)
            {
                borrar.Add(i);
                Debug.Log("borrar host: " + i + " porque esta completo");
            }
        }

        int tam = listaHost.Length - borrar.Count;
        if (tam > 0)
        {
            HostData[] nuevo = new HostData[tam];
            int metido = 0;
            for (int i = 0; i < listaHost.Length; i++)
            {
                if (!borrar.Contains(i))
                {
                    nuevo[metido] = listaHost[i];
                    metido++;
                }
            }

            listaHost = nuevo;
        }
        else
        {
            listaHost = null;
        }
    }
    #endregion

}
