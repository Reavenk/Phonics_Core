using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenWhite : GenBase
        {
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
                    data[i] += Random.value;
                }
            }

            public override PlayState Finished()
            {
                return PlayState.Constant;
            }
        }
    }
}