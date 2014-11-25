using UnityEngine;
using System.Collections;

public class GestorRandom 
{
	private static GestorRandom gr;

	private bool solicitadoNumero;
	private int m_rangoMenor;
	private int m_rangoMayor;
	private int numeroAleatorio;
	private bool numeroGenerado;


    public static GestorRandom getInstance()
    {
        if (gr == null)
            gr = new GestorRandom();
        return gr;
    }

	public static void deleteInstance()
	{
		if(gr!=null)
		{
			gr=null;
		}
	}

	private GestorRandom()
	{
		solicitadoNumero = false;
		numeroGenerado = false;
	}
    public void setSeed(int semilla)
    {
        Random.seed=semilla;        
    }


    /// <summary>
    /// Must be called in all tick->
    /// Generates a random number when it's needed
    /// </summary>
	public void generaNumero()
	{
        
		if(solicitadoNumero)
		{
			numeroAleatorio= Random.Range(m_rangoMenor,m_rangoMayor);
			numeroGenerado=true;
		}
        
	}
    /// <summary>
    /// Returns a random number
    /// </summary>
    /// <param name="rangoMenor"></param>
    /// <param name="rangoMayor"></param>
    /// <returns></returns>
	public int getNumeroAleatorio(int rangoMenor, int rangoMayor)
	{
		if(!solicitadoNumero)
		{
			m_rangoMayor = rangoMayor;
			m_rangoMenor = rangoMenor;
			solicitadoNumero = true;
			numeroGenerado = false;
			while (!numeroGenerado);
			solicitadoNumero = false;
			return numeroAleatorio;
		}
		Debug.Log ("Error con los numeros aleatorios");
		return 0;
	}

    public int getNumeroAleatorioNoThread(int rangoMenor, int rangoMayor)
    {      
       return Random.Range(m_rangoMenor, m_rangoMayor);     
    }
}
