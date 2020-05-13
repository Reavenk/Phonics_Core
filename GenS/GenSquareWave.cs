using System.Collections.Generic;

namespace PxPre
{
    namespace Phonics
    {
        public class GenSquareWave : GenBase
        {
            public GenSquareWave(float freq, double startTime, int samplesPerSec, float amplitude)
                : base(freq, startTime, samplesPerSec, amplitude)
            { }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                double tIt = this.CurTime;
                double incr = this.TimePerSample;
                for (int i = 0; i < size; ++i)
                {
                    float fVal = (float)((tIt * this.Freq) % 1.0);
                    data[i] += (fVal < 0.5f) ? -this.amplitude : this.amplitude;
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