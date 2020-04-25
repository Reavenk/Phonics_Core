using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenLerp : GenBase
        { 
            GenBase gma;
            GenBase gmb;
            GenBase gmFactor;

            public GenLerp(double startTime, int samplesPerSc, GenBase gma, GenBase gmb, GenBase gmFactor)
                : base(0.0f, startTime, samplesPerSc, 1.0f)
            { 
                this.gma = gma;
                this.gmb = gmb;
                this.gmFactor = gmFactor;
            }

            public override void AccumulateImpl(float[] data, int size, IFPCMFactory pcmFactory)
            {
                FPCM fa = pcmFactory.GetFPCM(size, true);
                FPCM fb = pcmFactory.GetFPCM(size, true);
                FPCM ff = pcmFactory.GetFPCM(size, true);

                float [] a = fa.buffer;
                float [] b = fb.buffer;
                float [] f = ff.buffer;

                this.gma.Accumulate(a, size, pcmFactory);
                this.gmb.Accumulate(b, size, pcmFactory);
                this.gmFactor.Accumulate(f, size, pcmFactory);

                for(int i = 0; i < size; ++i)
                    data[i] += a[i] + (b[i] - a[i]) * f[i];
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