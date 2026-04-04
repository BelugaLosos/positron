using Positron.Client.DataTransferObjects;
using Positron.Client.GameEntities.Premitive;
using Positron.Serialzier;
using UnityEngine;

public class Testt : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //byte[] testArr = new byte[] { 151, 206, 0, 0, 0, 15, 206, 0, 0, 0, 16, 145, 150, 207, 0, 0, 0, 0, 0, 0, 0, 3, 207, 0, 0, 0, 0, 0, 0, 0, 4, 206, 0, 0, 0, 1, 206, 0, 0, 0, 2, 147, 202, 64, 160, 0, 0, 202, 64, 192, 0, 0, 202, 64, 224, 0, 0, 147, 202, 65, 0, 0, 0, 202, 65, 16, 0, 0, 202, 65, 32, 0, 0, 145, 206, 0, 0, 0, 17, 145, 206, 0, 0, 0, 18, 145, 149, 207, 0, 0, 0, 0, 0, 0, 0, 0, 206, 0, 0, 0, 0, 205, 0, 0, 195, 148, 196, 4, 106, 111, 112, 97, 145, 150, 206, 0, 0, 0, 11, 206, 0, 0, 0, 12, 205, 0, 13, 204, 14, 165, 115, 114, 97, 107, 97, 147, 196, 3, 102, 102, 102 };
        //var s = new MsgPackSerializer();
        //Debug.Log(testArr.Length);
        //s.Init();
        //GameTickPacket packet = s.Deserialize<GameTickPacket>(testArr);

        //byte[] testArr = new byte[] { 147, 202, 63, 128, 0, 0, 202, 63, 128, 0, 0, 202, 63, 128, 0, 0 };
        //                          145 147  202  63  128  0  0  202  63  128  0  0  202  63  128  0  0
        //                          145 147  202  63  128  0  0  202  63  128  0  0  202  63  128  0  0
        //var s = new MsgPackSerializer();
        //Debug.Log(testArr.Length);
        //NetVector3 packet = s.Deserialize<NetVector3>(testArr);

        var s = new MsgPackSerializer();
        string arr = "";

        foreach(byte b in s.Serialize(new NetVector3(1, 1, 1)))
        {
            arr += " " + b.ToString();
        }

        Debug.Log(arr);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
