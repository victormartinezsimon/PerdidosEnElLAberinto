using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Interpolate : MonoBehaviour
{

    List<Vector3> positions = new List<Vector3>();
    List<float> rotations = new List<float>();
    List<bool> orientations = new List<bool>();

    public int _takePosEveryXUpadates = 2;

    public int _snapshotSize = 4;

    private int _numUpdate = 0;

    private NetworkManager m_nm;
    private AnimationController m_ac;
    private MovementController m_mc;
    private Info m_info;

    public float alpha = 0.01f;

    void Start()
    {
        m_nm = GameObject.FindGameObjectWithTag("Network").GetComponent<NetworkManager>();
        m_ac = GetComponent<AnimationController>();
        m_mc = GetComponent<MovementController>();
        m_info = GetComponent<Info>();
    }

    void FixedUpdate()
    {
        if (m_info.MINE)
        {
            if (++_numUpdate % _takePosEveryXUpadates == 0)
            {
                positions.Add(transform.position);
                rotations.Add(transform.rotation.eulerAngles.y);
                orientations.Add(m_mc.Delante);
                if (positions.Count >= _snapshotSize)
                {
                    string positionMsg = _SerializePosition();
                    string rotationMsg = _SerializeRotation();
                    string orientantionMsg = _SerializeOrientation();
                    positions.Clear();
                    rotations.Clear();
                    orientations.Clear();
                    m_nm.sendSnapShotPosition(positionMsg,m_info.ID);
                    m_nm.sendSnapShotRotation(rotationMsg, m_info.ID);
                    m_nm.sendSnapShotOrientation(orientantionMsg, m_info.ID);

                    

                }
            }
        }
        else
        {

            setAnimaciones();

            if (positions.Count > 0)
            {
                transform.position = positions[0];
                positions.RemoveAt(0);
            }
            if (rotations.Count > 0)
            {
                float rotacion = rotations[0];
                transform.rotation = Quaternion.Euler(new Vector3(0, rotacion, 0));
                rotations.RemoveAt(0);
            }
            if (orientations.Count > 0)
            {
                orientations.RemoveAt(0);
            }

        }
    }
    void setAnimaciones()
    {
        if (positions.Count > 0 && rotations.Count > 0 && orientations.Count > 0)
        {
            Vector3 dir = positions[0] - transform.position;
            
            if ( Mathf.Abs(dir.x) < alpha &&  Mathf.Abs(dir.y) < alpha && Mathf.Abs(dir.z) < alpha)
            {
                //idle o girando
                float difGrados = rotations[0] - transform.rotation.eulerAngles.y;
                if (difGrados <= alpha)
                {
                    m_ac.setAnimacionRed(AnimationController.tipoAnimacionRed.IDLE);
                }
                else
                {
                    m_ac.setAnimacionRed(AnimationController.tipoAnimacionRed.GIRO);
                }
            }
            else
            {
                //walk o moonwalk
                if (orientations[0])
                {
                    //delante
                    m_ac.setAnimacionRed(AnimationController.tipoAnimacionRed.WALK);
                }
                else
                {
                    //moonwalk
                    m_ac.setAnimacionRed(AnimationController.tipoAnimacionRed.MOONWALK);
                }
            }
        }       

    }


    void receiveSnapShotsPosition(string data)
    {
            _DeSerializePosition(data);
    }
    void receiveSnapShotsRotation(string data)
    {
        _DeSerializeRotation(data);
    }
    void receiveSnapShotsOrientation(string data)
    {
        _DeSerializeOrientation(data);
    }


    #region Helper Methods

    private static readonly char DATA_SEPARATOR = ';';

    private static readonly char VALUE_SEPARATOR = ',';

    /// <summary>
    /// Serialize the data to send on Snapshot
    /// </summary>
    /// <returns>Serialized data on string</returns>
    private string _SerializePosition()
    {
        string ret = "";
        //Position
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i];
            ret += pos.x.ToString() + VALUE_SEPARATOR;
            ret += pos.y.ToString() + VALUE_SEPARATOR;
            ret += pos.z.ToString();
            if (i < positions.Count - 1)
                ret += DATA_SEPARATOR;
        }
        return ret;
    }
    private string _SerializeRotation()
    {
        string ret = "";
        //Position
        for (int i = 0; i < rotations.Count; i++)
        {
            ret += rotations[i];
            if (i < rotations.Count - 1)
                ret += DATA_SEPARATOR;
        }
        return ret;
    }
    private string _SerializeOrientation()
    {
        string ret = "";
        //Position
        for (int i = 0; i < orientations.Count; i++)
        {
            ret += orientations[i];
            if (i < orientations.Count - 1)
                ret += DATA_SEPARATOR;
        }
        return ret;
    }
    
    /// <summary>
    /// Deserializes received data
    /// </summary>
    /// <param name="raw">Serialized data on string</param>
    private void _DeSerializePosition(string raw)
    {
        //Deserialize Position
        string[] splittedPositionData = raw.Split(DATA_SEPARATOR);
        positions.Clear();
        for (int i = 0; i < _snapshotSize; i++)
        {
            string[] splittedVector3 = splittedPositionData[i].Split(VALUE_SEPARATOR);
            Vector3 next = new Vector3(
            float.Parse(splittedVector3[0]), //x
            float.Parse(splittedVector3[1]), //y
            float.Parse(splittedVector3[2]));//z
            interpolateTillPosition(next);
            positions.Add(next);
        }
    }
    private void _DeSerializeRotation(string raw)
    {
        //Deserialize Position
        string[] splittedPositionData = raw.Split(DATA_SEPARATOR);
        rotations.Clear();
        for (int i = 0; i < _snapshotSize; i++)
        {
            float value = float.Parse(splittedPositionData[i]);
            interpolateTillRotation(value);
            rotations.Add(value);
        }
    }
    private void _DeSerializeOrientation(string raw)
    {
        //Deserialize Position
        string[] splittedPositionData = raw.Split(DATA_SEPARATOR);
        orientations.Clear();
        for (int i = 0; i < _snapshotSize; i++)
        {
            bool value = bool.Parse(splittedPositionData[i]);
            orientations.Add(value);
        }
    }
    void interpolateTillPosition(Vector3 next)
    {
        Vector3 basePos;
        // interpolamos las intermedias
        if (positions.Count > 0)
            basePos = positions[positions.Count - 1];
        else
            basePos = transform.position;
        Vector3 increment = (next - basePos) / (_takePosEveryXUpadates);
        for (int j = 1; j < _takePosEveryXUpadates; ++j)
        {
            positions.Add(basePos + j * increment);
        }
    }
    void interpolateTillRotation(float val)
    {
        float basePos;
        // interpolamos las intermedias
        if (rotations.Count > 0)
            basePos = rotations[rotations.Count - 1];
        else
            basePos = transform.rotation.eulerAngles.y;

        float diferencia = (val - basePos);
        //cocota enorme
        if (diferencia > 50)
        {
            diferencia = val - basePos - 360;
        }
        if (diferencia < -50)
        {
            diferencia = val - basePos + 360;
        }



        float increment = diferencia / (_takePosEveryXUpadates);
        for (int j = 1; j < _takePosEveryXUpadates; ++j)
        {
            float added = basePos + j * increment;
            added = added % 360;
            rotations.Add(added);
            //Debug.Log(added+"="+ basePos + " + "+ j+" * "+ increment);
        }
    }
    #endregion
}
