using System.Collections.Generic;

namespace PxPre
{
    namespace Phonics
    {
        public class GenSquare : GenBase
        {
            GenBase input;

            public GenSquare(GenBase input)
                : base(0.0f, 0.0, 0, 1.0f)
            { 
                this.input = input;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(size, true);
                float[] a = fa.buffer;
                this.input.Accumulate(a, size, pcmFactory);

                for (int i = 0; i < size; ++i)
                    data[i] += a[i] * a[i];
            }

            public override PlayState Finished()
            {
                if(this.input == null)
                    return PlayState.Finished;

                return this.input.Finished();
            }

            public override void ReportChildren(List<GenBase> lst)
            { 
                if(this.input != null)
                    lst.Add(this.input);
            }
        }
    }
}
