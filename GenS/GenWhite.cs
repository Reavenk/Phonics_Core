using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenWhite : GenBase
        {
            const int NoiseBufferSz = 44000;
            static int noiseIt = 0;
            static float [] bakedNoise = null;

            public static void BakeNoise()
            { 
                if(bakedNoise != null)
                    return;

                bakedNoise = new float[NoiseBufferSz];
                for(int i = 0; i < NoiseBufferSz; ++i)
                    bakedNoise[i] = Random.value;
            }

            // We throw away the frequency parameter, it does nothing for us.
            public GenWhite(double startTime, int samplesPerSec, float amplitude)
                : base(0.0f, startTime, samplesPerSec, amplitude)
            { }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                double tIt = this.CurTime;
                double incr = this.TimePerSample;
                for (int i = 0; i < size; ++i)
                {
                    data[i] += bakedNoise[noiseIt] * this.amplitude;
                    noiseIt = (noiseIt + 1) % NoiseBufferSz;
                }
            }

            public override PlayState Finished()
            {
                return PlayState.Constant;
            }

            public override void ReportChildren(List<GenBase> lst)
            {}
        }
    }
}