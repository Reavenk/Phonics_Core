using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenAbs : GenBase
        {
            GenBase input;

            public GenAbs(int samplesPerSec, GenBase input)
                : base(0.0f, 0.0, samplesPerSec, 1.0f)
            { 
                this.input = input;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(size, true);
                float [] a = fa.buffer;
                this.input.Accumulate(a, size, pcmFactory);

                for(int i = 0; i < size; ++i)
                {
                    if(a[i] < 0.0f)
                        data[i] -= a[i];
                    else
                        data[i] += a[i];
                }
            }

            public override PlayState Finished()
            {
                if(this.input == null)
                    return PlayState.Finished;

                return this.input.Finished();
            }
        }
    }
}

