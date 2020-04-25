using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenConstant : GenBase
        {
            float constant;

            public GenConstant(double startTime, int samplesPerSec, float constant)
                : base(0.0f, startTime, samplesPerSec, 1.0f)
            { 
                this.constant = constant;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                for (int i = 0; i < size; ++i)
                    data[i] += constant;
            }

            public override PlayState Finished()
            {
                return PlayState.Constant;
            }
        }
    }
}
