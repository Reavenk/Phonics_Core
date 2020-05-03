using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenAmplify : GenBase
        { 
            GenBase input;
            float amount;

            public GenAmplify(int samplesPerSec, GenBase input, float amount)
                : base(0.0f, 0.0, samplesPerSec, 1.0f)
            { 
                this.input = input;
                this.amount = amount;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(size, true);
                float[] a = fa.buffer;

                this.input.Accumulate(a, size, pcmFactory);

                for (int i = 0; i < size; ++i)
                {
                    data[i] += a[i] * this.amount;
                }
            }

            public override PlayState Finished()
            {
                if (this.input == null)
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
