using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenMAD : GenBase
        {
            GenBase input;
            float mul;
            float add;

            public GenMAD(GenBase input, float mul, float add)
                : base(0.0f, 0.0, 0, 1.0f)
            { 
                this.input = input;
                this.mul = mul;
                this.add = add;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(size, true);
                float[] a = fa.buffer;
                this.input.Accumulate(a, size, pcmFactory);

                for (int i = 0; i < size; ++i)
                    data[i] += a[i] * this.mul + this.add;
            }

            public override PlayState Finished()
            {
                if(input == null)
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