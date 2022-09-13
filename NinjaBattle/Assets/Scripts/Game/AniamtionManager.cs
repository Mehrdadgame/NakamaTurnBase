using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AniamtionManager : MonoBehaviour
{

    public static AniamtionManager instance;

    public Image PageMatchMaking;
    public Animator AnimGoToUpMe;
    public Animator AnimGoToUpOpp;
    public Animator AnimIconOpp;
    public RectTransform IconMe;
    public RectTransform IconOpp;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        AnimIconOpp.Play("IconMatchMaking", 0, 0);
    }

 
}
