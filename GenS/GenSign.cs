using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenSign : GenBase
        {
            GenBase input;

            public GenSign(int samplesPerSec, GenBase input)
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
                    float s = a[i];
                    if(s == 0.0)
                        data[i] = 0.0f;
                    else if(s > 0.0)
                        data[i] = 1.0f;
                    else
                        data[i] = -1.0f;
                }
            }

            public override PlayState Finished()
            {
                if(this.input == null)
                    return PlayState.Finished;

                return this.input.Finished();
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.input);
            }
        }
    }
}
