using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Utils : NetworkBehaviour{

	static int maxHeight = 150;
	static float smooth = 0.01f;
	static int octaves = 4;
	static float persistence = 0.5f;

    public static bool browninanDouble = true;

    public static int seed = 1;

	public static int GenerateStoneHeight(float x, float z)
	{
		float height = Map(0,maxHeight-5, 0, 1, fBM(x*smooth*2,z*smooth*2,octaves+1,persistence,seed));
		return (int) height;
	}

	public static int GenerateHeight(float x, float z)
	{
		float height = Map(0,maxHeight, 0, 1, fBM(x*smooth,z*smooth,octaves,persistence,seed));
		return (int) height;
	}

    public static float fBM3D(float x, float y, float z, float sm, int oct)
    {
        if (browninanDouble) {
            float XY = fBM(x * sm, y * sm, oct, 0.5f, seed);
            float YZ = fBM(y * sm, z * sm, oct, 0.5f, seed);
            float XZ = fBM(x * sm, z * sm, oct, 0.5f, seed);

            float YX = fBM(y * sm, x * sm, oct, 0.5f, seed);
            float ZY = fBM(z * sm, y * sm, oct, 0.5f, seed);
            float ZX = fBM(z * sm, x * sm, oct, 0.5f, seed);

            return (XY + YZ + XZ + YX + ZY + ZX) / 6.0f;
        } else {
            float XY = fBM(x * sm, y * sm, oct, 0.5f, seed);
            float YZ = fBM(y * sm, z * sm, oct, 0.5f, seed);
            float XZ = fBM(x * sm, z * sm, oct, 0.5f, seed);
            return (XY + YZ + XZ ) / 3.0f;

        }

    }

	static float Map(float newmin, float newmax, float origmin, float origmax, float value)
    {
        return Mathf.Lerp (newmin, newmax, Mathf.InverseLerp (origmin, origmax, value));
    }

    static float fBM(float x, float z, int oct, float pers,int seed)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        float offset = 32000f;


        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[oct];


        for(int i = 0; i < oct; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetZ = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);
        }



        for(int i = 0; i < oct ; i++) 
        {

            float sampleX = x * frequency + octaveOffsets[i].x;
            float sampleZ = z * frequency + octaveOffsets[i].y;


            total += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;

            maxValue += amplitude;

            amplitude *= pers;
            frequency *= 2;
        }

        return total/maxValue;
    }
    


}
