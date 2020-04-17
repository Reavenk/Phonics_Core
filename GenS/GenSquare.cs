using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenSquare : GenBase
{
    public GenSquare(float freq, double startTime, int samplesPerSec, float amplitude)
        : base(freq, startTime, samplesPerSec, amplitude)
    { }

    public override void AccumulateImpl(float[] data, int size)
    {
        double tIt = this.CurTime;
        double incr = this.TimePerSample;
        for (int i = 0; i < size; ++i)
        {
            float fVal = (float)((tIt * this.Freq) % 1.0);
            data[i] += (fVal < 0.5f) ? -this.amplitude : this.amplitude;
            tIt += incr;
        }
    }
}
