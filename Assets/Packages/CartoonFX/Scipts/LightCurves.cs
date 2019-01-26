using UnityEngine;
using System.Collections;

public class LightCurves : MonoBehaviour {
	public AnimationCurve LightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	public float GraphScaleX = 1, GraphScaleY = 1;

	private float startTime;
	private Light lightSource;

    private bool play = false;

	void Awake(){
        lightSource = GetComponent<Light>();
        if (!play)
        {
            Stop();
        }
	}
	
	void Update () {
        if (play)
        {
            var time = Time.time - startTime;
            var eval = LightCurve.Evaluate(time % GraphScaleX) * GraphScaleY;
            lightSource.intensity = eval;
        }
    }

    public void Play()
    {
        play = true;
		startTime = Time.time;
    }

    public void Stop()
    {
        play = false;
        lightSource.intensity = 0;
    }
}
