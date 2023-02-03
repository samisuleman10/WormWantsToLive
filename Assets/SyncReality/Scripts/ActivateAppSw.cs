using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAppSw : MonoBehaviour
{

    private bool m_aswEnabled = false;
    // Start is called before the first frame update
    void Start()
    {
        m_aswEnabled = !m_aswEnabled;
        OVRManager.SetSpaceWarp(m_aswEnabled);
        OVRManager.foveatedRenderingLevel = OVRManager.FoveatedRenderingLevel.HighTop;
    }

    // Update is called once per frame
    void Update()
    {
     /*   if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            m_aswEnabled = !m_aswEnabled;
            OVRManager.SetSpaceWarp(m_aswEnabled);
        }
        
        if (OVRInput.GetDown(OVRInput.Button.One))
                {

                    if (OVRManager.fixedFoveatedRenderingLevel == OVRManager.FixedFoveatedRenderingLevel.HighTop)
                    {
                        OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Off;    
                        
                    }
                    else{
                    OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.HighTop;
                    //OVRManager.useDynamicFixedFoveatedRendering = true;
                    }
                }
        */
    }
    
    
}
