using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptMaterials : MonoBehaviour
{

    public Material opt1, opt2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Material GetForceField(Color color, int opt)
    {
        switch (opt)
        {
            case 1:
                opt1.color = color;
                return opt1;
                break;
            case 2:
                opt2.color = color;
                return opt2;
                break;
            default:
                return opt1;


         }
    }

    //public Material GetGabor(Color color, int opt)
    //{
    //    switch (opt)
    //    {
    //        case 1:
    //            opt1.color = color;
    //            return opt1;
    //            break;
    //        case 2:
    //            opt2.color = color;
    //            return opt2;
    //            break;é
    //        default:
    //            return opt1;


    //    }
    //}
}
