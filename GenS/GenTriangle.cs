using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenTriangle : GenBase
        {
            public GenTriangle(float freq, double startTime, int samplesPerSec, float amplitude)
                : base(freq, startTime, samplesPerSec, amplitude)
            { }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                double tIt = this.CurTime;
                double incr = this.TimePerSample;
                for (int i = 0; i < size; ++i)
                {
                    float fVal = (float)((tIt * this.Freq) % 1.0) * 2.0f;

                    if(fVal > 1.0f)
                        fVal = 1.0f - (fVal - 1.0f);

                    data[i] += (fVal - 0.5f) * 2.0f * this.amplitude;
                    tIt += incr;
                }
            }

            public override PlayState Finished()
            { 
                return PlayState.Constant;
            }
        }
    }
}