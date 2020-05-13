using System.Collections.Generic;

namespace PxPre
{
    namespace Phonics
    {
        public class GenQuant : GenBase
        { 
            float factor;
            float invFactor;

            GenBase input;

            public GenQuant(int samplesPerSec, GenBase input, float factor)
                : base(0.0f, 0.0, samplesPerSec, 1.0f)
            { 
                this.factor = factor;
                this.invFactor = 1.0f / factor;
                this.input = input;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(size, true);
                float [] a = fa.buffer;
                this.input.Accumulate(a, size, pcmFactory);

                for(int i = 0; i < size; ++i)
                    data[i] += ((float)(int)(a[i] * factor))*invFactor;
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