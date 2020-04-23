using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class FPCMScopedFactory : IFPCMFactory
        {
            public readonly IFPCMFactory parent;

            public List<FPCM> allocated = 
                new List<FPCM>();

            public FPCMScopedFactory(IFPCMFactory parent)
            { 
                this.parent = parent;
            }

            public void ReleaseScope()
            { 
                foreach(FPCM fpcm in this.allocated)
                    fpcm.Release();

                this.allocated.Clear();
            }

            FPCM IFPCMFactory.GetFPCM(int samples, bool zero)
            { 
                FPCM ret = this.parent.GetFPCM(samples, zero);
                if(ret == null)
                    return null;

                this.allocated.Add(ret);
                return ret;
            }

            FPCM IFPCMFactory.GetGlobalFPCM(int samples, bool zero)
            { 
                return this.parent.GetGlobalFPCM(samples, zero);
            }
        }
    }
}
