// MIT License
//
// Copyright(c) 2020 Pixel Precision LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Phonics
    {
        /// <summary>
        /// A Manager class for PCM generators. 
        /// 
        /// This class helps manage PCM generators, as well and binding them
        /// to a library of Unity AudioSources.
        /// </summary>
        public class GenManager
        {
            public class PlayingNote
            {
                /// <summary>
                /// The handle id of the playing note. Each playing note generates
                /// a unique handle id.
                /// </summary>
                public int id;

                // The generator is used to generate PCM for the clip, which plays
                // though Unity via the AudioSource.

                /// <summary>
                /// The generator generating the PCM data.
                /// </summary>
                public GenBase generator;

                /// <summary>
                /// The audio clip generating the data.
                /// </summary>
                public AudioClip clip;

                /// <summary>
                /// The audio source playing the data.
                /// </summary>
                public AudioSource source;
            }

            // The counter used to generate unique PlayeringNode ids
            private int ctr = 0;

            // The GameObject that new AudioSources will get added to.
            public readonly GameObject sourceHost;

            // The library of available AudioSources
            List<AudioSource> audioSources = new List<AudioSource>();

            // The active notes currently playing.
            Dictionary<int, PlayingNote> activeNotes =
                new Dictionary<int, PlayingNote>();

            // The active notes currently playing but are being released.
            List<PlayingNote> releasingNotes =
                new List<PlayingNote>();

            public GenManager(GameObject sourceHost)
            { 
                this.sourceHost = sourceHost;
            }

            /// <summary>
            /// Start a generator.
            /// </summary>
            /// <param name="gen">The </param>
            /// <param name="samples">The number of samples for the PCM buffer.</param>
            /// <param name="samplesPerSec">Samples per second for the PCM stream.</param>
            /// <returns></returns>
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

            /// <summary>
            /// Stops all notes, including released notes, from playing.
            /// 
            /// Used to implement the eStop.
            /// </summary>
            /// <returns>True if any notes were interrupted.</returns>
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

            /// <summary>
            /// Deconstructs the resources used for playing a note.
            /// 
            /// The resources will be absorbed back into the manager.
            /// </summary>
            /// <param name="pn">The note information to deconstruct.</param>
            private void StopPlayingNote(PlayingNote pn)
            {
                pn.source.Stop();
                pn.generator.DeconstructHierarchy();
                this.audioSources.Add(pn.source);
            }

            /// <summary>
            /// Manages if any released notes are finished being released.
            /// </summary>
            /// <returns>The number of notes finished being released.</returns>
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

            /// <summary>
            /// Checks if any notes are being played.
            /// </summary>
            /// <returns>True if any notes are being played, else false.</returns>
            public bool AnyPlaying()
            {
                return this.activeNotes.Count > 0 || this.releasingNotes.Count > 0;
            }
        }
    }
}