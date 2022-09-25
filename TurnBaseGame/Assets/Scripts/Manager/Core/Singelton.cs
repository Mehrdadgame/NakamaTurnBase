using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IInit { 
   public void Init();
}
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
