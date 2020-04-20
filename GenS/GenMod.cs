using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenMod : GenBase
{
    float [] buffer;
    GenBase gma;
    GenBase gmb;

    public GenMod(float freq, double startTime, int samplesPerSec, GenBase gma, GenBase gmb)
        : base(freq, startTime, samplesPerSec, 1.0f)
    { 
        this.gma = gma;
        this.gmb = gmb;
    }

    public override void AccumulateImpl(float[] data, int size)
    {
        float [] fa = GetBufferA(size);
        float [] fb = GetBufferB(data.Length);
        //float [] fd = GetBufferD(data.Length);

        gma.Accumulate(fa, size);
        gmb.Accumulate(fb, size);

        for(int i = 0; i < size; ++i)
            data[i] = fa[i] * fb[i] * this.amplitude;
    }
}
