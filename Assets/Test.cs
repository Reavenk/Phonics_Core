using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Phonics;

public class Test : MonoBehaviour
{
    AudioClip clip = null;
    AudioSource source = null;

    private void Awake()
    {
        this.source = this.GetComponent<AudioSource>();
        if(this.source == null)
            this.source = this.gameObject.AddComponent<AudioSource>();

        for(int i = 0; i < 12 * 6; ++i)
        { 
            int octaveOut;
            Generator.Note noteout;

            Generator.GetSetWestKeyInfo(i, out noteout, out octaveOut);
            Debug.Log( $"The tone {i} converts to {noteout}{octaveOut}.");

            int keynum = Generator.GetStdWestKey(noteout, octaveOut);
            if(keynum ==  i)
                Debug.Log($"The tone {noteout}{octaveOut} is successfuly invertible.");
            else
                Debug.LogError($"The tone {noteout}{octaveOut} FAILED the invertibility test. Expected {i}, but got {keynum}.");
            
            float fr = Generator.GetStdWestFrequency(noteout, octaveOut);
            Debug.Log( $"The key {i} called {noteout}{octaveOut} results in frequency {fr}.");
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void DoAudio(Generator.Note note, int octave, bool env)
    { 
        if(this.source.isPlaying == true)
            this.source.Stop();

        const int sampsSec = 22000;
        if(this.clip == null || this.clip.frequency != sampsSec || this.clip.length != 1.0f)
            this.clip = AudioClip.Create("Note", sampsSec, 1, sampsSec, false);

        float [] rf = new float[sampsSec];
        float freq = Generator.GetStdWestFrequency(note, octave);

        Generator.SetTri(rf, 0, rf.Length, 0.0f, 0.0f, freq, 1.0f, sampsSec);

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
            DoAudio(Generator.Note.A, 4, env);

        if (GUILayout.Button("As") == true)
            DoAudio(Generator.Note.As, 4, env);

        if (GUILayout.Button("B") == true)
            DoAudio(Generator.Note.B, 4, env);

        if (GUILayout.Button("C") == true)
            DoAudio(Generator.Note.C, 4, env);

        if (GUILayout.Button("Cs") == true)
            DoAudio(Generator.Note.Cs, 4, env);

        if (GUILayout.Button("D") == true)
            DoAudio(Generator.Note.D, 4, env);

        if (GUILayout.Button("E") == true)
            DoAudio(Generator.Note.E, 4, env);

        if (GUILayout.Button("Es") == true)
            DoAudio(Generator.Note.Es, 4, env);

        if (GUILayout.Button("F") == true)
            DoAudio(Generator.Note.F, 4, env);

        if (GUILayout.Button("Fs") == true)
            DoAudio(Generator.Note.Fs, 4, env);

        if (GUILayout.Button("G") == true)
            DoAudio(Generator.Note.G, 4, env);

        if (GUILayout.Button("Gs") == true)
            DoAudio(Generator.Note.Gs, 4, env);
    }
}
