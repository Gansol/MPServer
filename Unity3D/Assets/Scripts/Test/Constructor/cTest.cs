using UnityEngine;
using System.Collections;

public class cTest : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Human child = new Zombie("child", 10, false, true);
        child.SayHello();


        Human child2 = new Human("child2", 10, false);
        child2.SayHello();

        Zombie child3 = new Zombie("child3", 10, false, true);
        child3.SayHello();
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
