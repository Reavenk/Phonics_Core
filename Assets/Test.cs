using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Phonics;

public class Test : MonoBehaviour
{
    AudioClip clip = null;
    AudioSource source = null;

    GenBase gso = null;
    float amplitude = 0.8f;

    private void Awake()
    {
        this.source = this.GetComponent<AudioSource>();
        if(this.source == null)
            this.source = this.gameObject.AddComponent<AudioSource>();

        for(int i = 0; i < 12 * 6; ++i)
        { 
            int octaveOut;
            WesternFreqUtils.Note noteout;

            WesternFreqUtils.GetSetWestKeyInfo(i, out noteout, out octaveOut);
            Debug.Log( $"The tone {i} converts to {noteout}{octaveOut}.");

            int keynum = WesternFreqUtils.GetStdWestKey(noteout, octaveOut);
            if(keynum ==  i)
                Debug.Log($"The tone {noteout}{octaveOut} is successfuly invertible.");
            else
                Debug.LogError($"The tone {noteout}{octaveOut} FAILED the invertibility test. Expected {i}, but got {keynum}.");
            
            float fr = WesternFreqUtils.GetStdWestFrequency(noteout, octaveOut);
            Debug.Log( $"The key {i} called {noteout}{octaveOut} results in frequency {fr}.");
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void DoAudio(WesternFreqUtils.Note note, int octave, bool env)
    { 
        if(this.source.isPlaying == true)
            this.source.Stop();

        const int sampsSec = 22000;
        if(this.clip == null || this.clip.frequency != sampsSec || this.clip.length != 1.0f)
            this.clip = AudioClip.Create("Note", sampsSec, 1, sampsSec, false);

        float [] rf = new float[sampsSec];
        float freq = WesternFreqUtils.GetStdWestFrequency(note, octave);

        WesternFreqUtils.SetTri(rf, 0, rf.Length, 0.0f, 0.0f, freq, 1.0f, sampsSec);

        //if (env == true)
        //    Generator.SetThSquare(rf, 0, rf.Length, 0.0f, 0.0f, freq, sampsSec, 0.057351944f, 7.516838921f, -16.93430382f, 9.42103883f);
        //else
        //    Generator.SetThSquare(rf, 0, rf.Length, 0.0f, 0.0f, freq, 1.0f, sampsSec);

        this.clip.SetData(rf, 0);
            
        this.source.clip = this.clip;

        this.source.Play();
    }
     
    private void OnGUI()
    {
        float width = 20.0f;
        float height = 100.0f;

        bool env = true;

        if (GUILayout.Button("A") == true)
            DoAudio(WesternFreqUtils.Note.A, 4, env);

        if (GUILayout.Button("As") == true)
            DoAudio(WesternFreqUtils.Note.As, 4, env);

        if (GUILayout.Button("B") == true)
            DoAudio(WesternFreqUtils.Note.B, 4, env);

        if (GUILayout.Button("C") == true)
            DoAudio(WesternFreqUtils.Note.C, 4, env);

        if (GUILayout.Button("Cs") == true)
            DoAudio(WesternFreqUtils.Note.Cs, 4, env);

        if (GUILayout.Button("D") == true)
            DoAudio(WesternFreqUtils.Note.D, 4, env);

        if (GUILayout.Button("E") == true)
            DoAudio(WesternFreqUtils.Note.E, 4, env);

        if (GUILayout.Button("Es") == true)
            DoAudio(WesternFreqUtils.Note.Es, 4, env);

        if (GUILayout.Button("F") == true)
            DoAudio(WesternFreqUtils.Note.F, 4, env);

        if (GUILayout.Button("Fs") == true)
            DoAudio(WesternFreqUtils.Note.Fs, 4, env);

        if (GUILayout.Button("G") == true)
            DoAudio(WesternFreqUtils.Note.G, 4, env);

        if (GUILayout.Button("Gs") == true)
            DoAudio(WesternFreqUtils.Note.Gs, 4, env);

        this.amplitude = GUILayout.HorizontalSlider(this.amplitude, 0.0f, 1.0f, GUILayout.Width(200.0f));
        if(this.gso != null && this.amplitude != this.gso.amplitude)
            this.gso.amplitude = this.amplitude;

        if (GUILayout.Button("Sin") == true)
        {

            this.gso = new GenSine(440.0f, 0.0, 44000, 0.8f);

            this.clip = AudioClip.Create("Testname", 44000, 1, 44000, true, gso.ReaderCallback, gso.SetPositionCallback);
            this.source.clip = this.clip;
            this.source.loop = true;
            this.source.Play();
        }

        if (GUILayout.Button("Saw") == true)
        {

            this.gso = new GenSawtooth(440.0f, 0.0, 44000, 0.8f);

            this.clip = AudioClip.Create("Testname", 44000, 1, 44000, true, gso.ReaderCallback, gso.SetPositionCallback);
            this.source.clip = this.clip;
            this.source.loop = true;
            this.source.Play();
        }

        if (GUILayout.Button("Saw") == true)
        {

            this.gso = new GenSquare(440.0f, 0.0, 44000, 0.8f);

            this.clip = AudioClip.Create("Testname", 44000, 1, 44000, true, gso.ReaderCallback, gso.SetPositionCallback);
            this.source.clip = this.clip;
            this.source.loop = true;
            this.source.Play();
        }

        if (GUILayout.Button("Tri") == true)
        {

            this.gso = new GenTriangle(440.0f, 0.0, 44000, 0.8f);

            this.clip = AudioClip.Create("Testname", 44000, 1, 44000, true, gso.ReaderCallback, gso.SetPositionCallback);
            this.source.clip = this.clip;
            this.source.loop = true;
            this.source.Play();
        }

        if (GUILayout.Button("LFO") == true)
        {

            GenSine gsa = new GenSine(440.0f, 0.0, 44000, 0.8f);
            GenSine gsb = new GenSine(440.0f / 64.0f, 0.0f, 44000, 1.0f);
            GenSine gsc = new GenSine(440.0f / 256.0f, 0.0f, 44000, 1.0f);

            GenMod gmo = new GenMod(440.0f, 0.0, 44000, gsa, gsb);
            this.gso = new GenMod(440.0f, 0.0, 44000, gmo, gsc);

            this.clip = AudioClip.Create("Testname", 11000, 1, 44000, true, gso.ReaderCallback, gso.SetPositionCallback);
            this.source.clip = this.clip;
            this.source.loop = true;
            this.source.Play();
        }
    }
}
