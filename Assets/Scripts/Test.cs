using System.Collections;
using UnityEngine;
using Kaede2.MasterData;

namespace Kaede2
{
    public class Test : MonoBehaviour
    {
        IEnumerator Start()
        {
            MasterZukanFairyProfile data = null;
            yield return BaseMasterData.Load<MasterZukanFairyProfile>(d => { data = d; });

            Debug.Log(JsonUtility.ToJson(data.zukanProfile[0], true));
        }
    }
}