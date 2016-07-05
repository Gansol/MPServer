using UnityEngine;
using System.Collections;

public class CorountineTest : MonoBehaviour
{
    public IEnumerator aa(Component c)
    {
        c.SendMessage("bb");
        yield return "A";
    }
}
