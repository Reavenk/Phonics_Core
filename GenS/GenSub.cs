using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenSub : GenBase
        {
            GenBase gma;
            GenBase gmb;

            public GenSub(double startTime, int samplesPerSec, GenBase gma, GenBase gmb)
                : base(0.0f, startTime, samplesPerSec, 1.0f)
            {
                this.gma = gma;
                this.gmb = gmb;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(data.Length, true);
                FPCM fb = pcmFactory.GetFPCM(data.Length, true);
                
                float [] a = fa.buffer;
                float [] b = fb.buffer;

                gma.Accumulate(a, size, pcmFactory);
                gmb.Accumulate(b, size, pcmFactory);

                for (int i = 0; i < size; ++i)
                    data[i] = a[i] + b[i] * this.amplitude;
            }

            public override PlayState Finished()
            {
                PlayState psA = this.gma.Finished();
                PlayState psB = this.gmb.Finished();

                // If we're modulating and one is signaling it's just going to
                // only return 0.0 from now one, we're done.
                if (psA == PlayState.Finished || psB == PlayState.Finished)
                    return PlayState.Finished;

                if (psA == PlayState.NotStarted && psB == PlayState.NotStarted)
                    return PlayState.NotStarted;

                if (psA == PlayState.Constant || psB == PlayState.Constant)
                    return PlayState.Constant;

                return PlayState.Playing;

            }
        }
    }
}
