using System.Collections.Generic;

namespace PxPre
{
    namespace Phonics
    {
        public class GenGateWave : GenBase
        { 
            public GenGateWave(float freq, double startTime, int samplesPerSec, float amplitude)
                : base(freq, startTime, samplesPerSec, amplitude)
            { }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                double tIt = this.CurTime;
                double incr = this.TimePerSample;
                for (int i = 0; i < size; ++i)
                {
                    double d = tIt * Freq % 1.0f;
                    data[i] += d > 0.5 ? this.amplitude : 0.0f;
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