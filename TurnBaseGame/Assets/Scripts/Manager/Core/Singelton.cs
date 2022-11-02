using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IInit { 
   public void Init();
}
/* > This class is a generic class that inherits from MonoBehaviour and implements the IInit interface.
It has a static property called instance that returns the type of the class. It also has a virtual
method called Init that checks if the instance is null and if it is, it sets the instance to the
class type. If the instance is not null, it destroys the gameObject */
public class Singelton<T> : MonoBehaviour , IInit where T : Component
{
    public static T instance { get; private set; }
    public virtual void Init()
    {
        if (instance == null)
        {
            instance = this as T;

        }
        else
            Destroy(gameObject);
    }
}
