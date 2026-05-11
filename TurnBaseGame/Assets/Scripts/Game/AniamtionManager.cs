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

    [Header("Avatar Images (assign the Image on each icon GameObject)")]
    public Image AvatarImageMe;
    public Image AvatarImageOpp;

    void Awake()
    {
        instance = this;
        AnimIconOpp.Play("IconMatchMaking", 0, 0);
    }
}
