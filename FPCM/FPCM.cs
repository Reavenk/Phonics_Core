using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class FPCM
        {
            public FPCMFactory factory;
            public float [] buffer;

            public FPCM(FPCMFactory factory, float [] buffer)
            {
                this.factory = factory;
                this.buffer = buffer;
            }

            public bool CheckMinsize(int size)
            { 
                if(size < 1)
                    return false;

                if(this.buffer.Length < size)
                {
                    this.buffer = new float[size];
                    return true;
                }

                return false;
            }

            public bool Release()
            { 
                if(this.factory == null)
                    return false;

                return this.factory.ReturnFPCM(this);
            }

            public void Zero()
            { 
                this.Zero(this.buffer.Length);
            }

            public void Zero(int samples)
            { 
                for(int i = 0; i < samples; ++i)
                    this.buffer[i] = 0.0f;
            }
        }
    }
}