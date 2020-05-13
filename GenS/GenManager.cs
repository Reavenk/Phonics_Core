using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        public class GenManager
        {
            public class PlayingNote
            {
                public int id;
                public GenBase generator;
                public AudioClip clip;
                public AudioSource source;
            }

            private int ctr = 0;

            public readonly GameObject sourceHost;

            List<AudioSource> audioSources = new List<AudioSource>();

            Dictionary<int, PlayingNote> activeNotes =
                new Dictionary<int, PlayingNote>();

            List<PlayingNote> releasingNotes =
                new List<PlayingNote>();

            public GenManager(GameObject sourceHost)
            { 
                this.sourceHost = sourceHost;
            }

            public int StartGenerator(GenBase gen, int samples, int samplesPerSec)
            {
                AudioSource source = null;
                if (this.audioSources.Count > 0)
                {
                    int lastIdx = this.audioSources.Count - 1;
                    source = this.audioSources[lastIdx];
                    this.audioSources.RemoveAt(lastIdx);
                }

                if (source == null)
                    source = this.sourceHost.AddComponent<AudioSource>();

                int retId = this.ctr;
                ++this.ctr;

                PlayingNote pn = new PlayingNote();
                pn.id = retId;
                pn.generator = gen;
                pn.clip =
                    AudioClip.Create(
                        "StreamedKeyNote",
                        samples,
                        1, 
                        samplesPerSec,
#if UNITY_WEBGL && !UNITY_EDITOR
                        false,
#else
                        true,
#endif
                        pn.generator.ReaderCallback);
                pn.source = source;
                pn.source.loop = true;
                pn.source.clip = pn.clip;
                pn.source.Play();

                this.activeNotes.Add(retId, pn);

                return retId;
            }

            public bool StopNote(int idx, bool release = true)
            {
                PlayingNote pn;
                if (this.activeNotes.TryGetValue(idx, out pn) == false)
                    return false;

                // It normally shouldn't be null, but can be if we instantly close
                // the app while notes are still playing.
                if (pn.source != null)
                {
                    bool rm = true;
                    if (release == true)
                    {
                        pn.generator.ReleaseHierarchy();
                        // If it's releasing, or needs to finish up regarless or release state,
                        // it's finished mode will be playing instead of anything else.
                        PxPre.Phonics.PlayState psfin = pn.generator.Finished();
                        if (psfin == PlayState.Playing)
                        {
                            // Don't remove it, transfer it to the list of released playing notes.
                            rm = false;
                            this.releasingNotes.Add(pn);
                        }
                    }

                    if (rm == true)
                        this.StopPlayingNote(pn);
                }
                this.activeNotes.Remove(idx);
                return true;
            }

            public bool StopAllNotes()
            {
                bool stoppedAny = false;
                foreach (KeyValuePair<int, PlayingNote> kvp in this.activeNotes)
                {
                    this.StopPlayingNote(kvp.Value);
                    stoppedAny = true;
                }

                this.activeNotes.Clear();

                foreach (PlayingNote pn in this.releasingNotes)
                {
                    this.StopPlayingNote(pn);
                    stoppedAny = true;
                }
                
                this.releasingNotes.Clear();
                return stoppedAny;
            }

            private void StopPlayingNote(PlayingNote pn)
            {
                pn.source.Stop();
                pn.generator.DeconstructHierarchy();
                this.audioSources.Add(pn.source);
            }

            public int CheckFinishedRelease()
            { 
                int finished = 0;

                for (int i = this.releasingNotes.Count - 1; i >= 0; --i)
                {
                    PlayingNote pn = this.releasingNotes[i];
                    PxPre.Phonics.PlayState psfin = pn.generator.Finished();
                    if (psfin != PlayState.Playing)
                    {
                        pn.source.Stop();
                        pn.generator.DeconstructHierarchy();
                        this.audioSources.Add(pn.source);

                        this.releasingNotes.RemoveAt(i);
                        ++finished;
                    }
                }

                return finished;
            }

            public bool AnyPlaying()
            {
                return this.activeNotes.Count > 0 || this.releasingNotes.Count > 0;
            }
        }
    }
}