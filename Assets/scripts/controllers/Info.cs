using UnityEngine;
using System.Collections;

public class Info : MonoBehaviour 
{
    private bool m_ia=false;
    private int m_id=-1;
    private bool m_mine=false;
    
    public int ID
    {
        get { return m_id; }
        set { m_id = value; }
    }

    public bool IA
    {
        get { return m_ia; }
        set { m_ia = value; }
    }

    public bool MINE
    {
        get { return m_mine; }
        set { m_mine = value; }
    }
    
}
