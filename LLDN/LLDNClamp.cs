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

public class LLDNClamp : LLDNBase
{
    const int enumFull = 0;
    const int enumPos = 1;
    const int enumNeg = 2;

    static EnumVals clampEnum = 
        new EnumVals( 
            new EnumVals.Entry("full", enumFull , "Full",   "Clamp from [-1.0 to 1.0]."),
            new EnumVals.Entry("pos", enumPos, "Positive",  "Clamp from [0.0 to 1.0]."),
            new EnumVals.Entry("neg", enumNeg, "Negative",  "Clamp from [-1.0 to 0.0]."));

    ParamConnection input = null;
    ParamEnum clampType = null;

    public LLDNClamp()
        : base()
    { }

    public LLDNClamp(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal that will be clamped.";
        this.genParams.Add(this.input);

        this.clampType = new ParamEnum("Mode", "direction", clampEnum, enumFull);
        this.clampType.description = "How the audio signal will get clamped.";
        this.genParams.Add(this.clampType);
    }

    public override PxPre.Phonics.GenBase SpawnGenerator(
        float freq, 
        float beatsPerSec, 
        int samplesPerSec, 
        float amp,
        WiringDocument spawnFrom,
        WiringCollection collection)
    { 
        if(this.input.IsConnected() == false)
            return null;

        PxPre.Phonics.GenBase gb = 
            this.input.Reference.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                amp,
                spawnFrom,
                collection);

        if(gb != null)
        {
            switch(this.clampType.value)
            { 
                case enumFull:
                    return new PxPre.Phonics.GenClampFull(gb);
                case enumPos:
                    return new PxPre.Phonics.GenClampPos(gb);
                case enumNeg:
                    return new PxPre.Phonics.GenClampNeg(gb);
            }
        }

        return ZeroGen();
    }

    public override NodeType nodeType => NodeType.Clamp;

    public override LLDNBase CloneType()
    {
        return new LLDNClamp();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => "Clamps an audio signal.";
}
