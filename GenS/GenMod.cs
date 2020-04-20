using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenMod : GenBase
        {
            GenBase gma;
            GenBase gmb;

            public GenMod(float freq, double startTime, int samplesPerSec, GenBase gma, GenBase gmb)
                : base(freq, startTime, samplesPerSec, 1.0f)
            { 
                this.gma = gma;
                this.gmb = gmb;
            }

            public override void AccumulateImpl(float[] data, int size)
            {
                float [] fa = GetBufferA(size);
                float [] fb = GetBufferB(data.Length);
                //float [] fd = GetBufferD(data.Length);

                gma.Accumulate(fa, size);
                gmb.Accumulate(fb, size);

                for(int i = 0; i < size; ++i)
                    data[i] = fa[i] * fb[i] * this.amplitude;
            }

            public override PlayState Finished()
            {
                PlayState psA = this.gma.Finished();
                PlayState psB = this.gmb.Finished();

                // If we're modulating and one is signaling it's just going to
                // only return 0.0 from now one, we're done.
                if(psA == PlayState.Finished || psB == PlayState.Finished)
                    return PlayState.Finished;

                if(psA == PlayState.NotStarted && psB == PlayState.NotStarted)
                    return PlayState.NotStarted;

                if(psA == PlayState.Constant || psB == PlayState.Constant)
                    return PlayState.Constant;

                return PlayState.Playing;

            }
        }
    } 
}