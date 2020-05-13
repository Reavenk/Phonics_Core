using System.Collections.Generic;

namespace PxPre
{ 
    namespace Phonics
    { 
        public class GenAdd : GenBase
        {
            GenBase gma;
            GenBase gmb;

            public GenAdd(double startTime, int samplesPerSec, GenBase gma, GenBase gmb)
                : base(0.0f, startTime, samplesPerSec, 1.0f)
            {
                this.gma = gma;
                this.gmb = gmb;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(size, true);
                FPCM fb = pcmFactory.GetFPCM(size, true);
                
                float [] a = fa.buffer;
                float [] b = fb.buffer;

                this.gma.Accumulate(a, size, pcmFactory);
                this.gmb.Accumulate(b, size, pcmFactory);

                for (int i = 0; i < size; ++i)
                    data[i] = a[i] + b[i] * this.amplitude;
            }

            public override PlayState Finished()
            {
                return ResolveTwoFinished(this.gma, this.gmb);
            }

            public override void ReportChildren(List<GenBase> lst)
            {
                lst.Add(this.gma);
                lst.Add(this.gmb);
            }
        }
    }
}
