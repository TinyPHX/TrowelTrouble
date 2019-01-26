using UnityEngine;
using System.Collections.Generic;

public class Demo : MonoBehaviour
{
	public GameObject[] m_CartoonObjs;
    public GameObject[] m_CartoonObjs2;
    public Texture2D m_Ramp;
	public enum EDarkStyle { EDS_Flat = 0, EDS_1, EDS_2, EDS_3, EDS_4 };
	public EDarkStyle m_DarkStyle = EDarkStyle.EDS_Flat;
	[Range(0f, 0.5f)] public float m_StylizedDarkStart = 0.5f;
	[Range(0.51f, 1f)] public float m_StylizedDarkEnd = 0.6f;
	public Texture2D[] m_StylizedMaps;
	
    void Start ()
	{
		// only set ramp at start...since "ramp editor" need set ramp texture too.
		for (int i = 0; i < m_CartoonObjs.Length; i++)
		{
			MeshRenderer rd = m_CartoonObjs[i].GetComponent<MeshRenderer>();
			rd.material.SetTexture ("_RampTex", m_Ramp);
		}
	}
	void Update ()
    {
		for (int i = 0; i < m_CartoonObjs.Length; i++)
		{
			MeshRenderer rd = m_CartoonObjs[i].GetComponent<MeshRenderer>();
			if (m_DarkStyle == EDarkStyle.EDS_Flat) {
				rd.material.DisableKeyword ("NCE_STYLIZED");
			} else {
				rd.material.EnableKeyword ("NCE_STYLIZED");
				rd.material.SetTexture ("_StylizedTex", m_StylizedMaps[(int)m_DarkStyle - 1]);
			}
			rd.material.SetFloat ("_StylizedTexStart", m_StylizedDarkStart);
			rd.material.SetFloat ("_StylizedTexEnd", m_StylizedDarkEnd);
        }

        for (int i = 0; i < m_CartoonObjs2.Length; i++)
        {
            SkinnedMeshRenderer rd = m_CartoonObjs2[i].GetComponent<SkinnedMeshRenderer>();
            if (m_DarkStyle == EDarkStyle.EDS_Flat)
            {
                rd.material.DisableKeyword("NCE_STYLIZED");
            }
            else
            {
                rd.material.EnableKeyword("NCE_STYLIZED");
                rd.material.SetTexture("_StylizedTex", m_StylizedMaps[(int)m_DarkStyle - 1]);
            }
            rd.material.SetFloat("_StylizedTexStart", m_StylizedDarkStart);
            rd.material.SetFloat("_StylizedTexEnd", m_StylizedDarkEnd);

            rd.material.EnableKeyword("NCE_STYLIZED");
        }

    }
	void OnGUI()
	{
		GUI.Box (new Rect (10, 10, 240, 26), "NPR Cartoon Effect Demo");
	}
}
