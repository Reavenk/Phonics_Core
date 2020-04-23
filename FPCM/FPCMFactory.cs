using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class FPCMFactory : IFPCMFactory
        { 
            static FPCMFactory instance;
            static public FPCMFactory Instance 
            {
                get
                { 
                    if(instance == null)
                        instance = new FPCMFactory();

                    return instance;
                } 
            }

            private Dictionary<int, List<FPCM>> entries = 
                new Dictionary<int, List<FPCM>>();

            public FPCM GetFPCM(int samples)
            { 
                if(samples < 1)
                    return null;

                List<FPCM> lst;
                if(this.entries.TryGetValue(samples, out lst) == false)
                { 
                    FPCM newRet = new FPCM(this, new float[samples]);
                    return newRet;
                }

                int lastIdx = lst.Count - 1;
                FPCM ret = lst[lastIdx];
                lst.RemoveAt(lastIdx);

                if(lst.Count == 0)
                    this.entries.Remove(samples);

                return ret;
            }

            public bool ReturnFPCM(FPCM fpcm)
            { 
                if(fpcm.buffer == null || fpcm.buffer.Length == 0)
                    return false;

                int samples = fpcm.buffer.Length;

                List<FPCM> lst;
                if(this.entries.TryGetValue(samples, out lst) == false)
                { 
                    lst = new List<FPCM>();
                    this.entries.Add(samples, lst);
                }

                lst.Add(fpcm);
                return true;
            }

            FPCM IFPCMFactory.GetFPCM(int samples, bool zero)
            {
                FPCM fpcm = this.GetFPCM(samples);

                if(zero == true)
                    fpcm.Zero();

                return fpcm;
            }

            FPCM IFPCMFactory.GetGlobalFPCM(int samples, bool zero)
            {
                FPCM fpcm = this.GetFPCM(samples);

                if(zero == true)
                    fpcm.Zero();

                return fpcm;
            }
        }
    }
}
