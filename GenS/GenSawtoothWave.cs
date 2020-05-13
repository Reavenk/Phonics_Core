using System.Collections.Generic;

namespace PxPre
{
    namespace Phonics
    {
        public class GenSawtoothWave : GenBase
        {
            public GenSawtoothWave(float freq, double startTime, int samplesPerSec, float amplitude)
                : base(freq, startTime, samplesPerSec, amplitude)
            { }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                double tIt = this.CurTime;
                double incr = this.TimePerSample;
                for (int i = 0; i < size; ++i)
                {
                    data[i] += -this.amplitude + (float)((tIt * this.Freq) % 1.0) * this.amplitude * 2.0f;
                    tIt += incr;
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