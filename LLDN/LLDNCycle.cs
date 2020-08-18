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

public class LLDNCycle : LLDNBase
{
    static EnumVals offsetEnum = 
        new EnumVals( 
            new EnumVals.Entry("pass",      (int)PxPre.Phonics.GenCycle.OffsetPass.Pass ,   "Pass",     "Audio plays like normal."),
            new EnumVals.Entry("silent",    (int)PxPre.Phonics.GenCycle.OffsetPass.Silent,  "Silent",   "Time passes for audio signal but isn't audible."),
            new EnumVals.Entry("hold",      (int)PxPre.Phonics.GenCycle.OffsetPass.Hold,    "Hold",     "Time is paused for audio signal."));

    ParamConnection input;
    ParamTimeLen offset;
    ParamTimeLen recordAmt;
    ParamEnum offsetType = null;

    public LLDNCycle()
        : base()
    { }

    public LLDNCycle(string guid)
        : base(guid)
    { }

    protected override void _Init()
    {
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal to record and playback.";
        this.genParams.Add(this.input);

        this.offset = new ParamTimeLen("Offset", 0.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.offset.description = "The amount of time to pass before recording. What kind of behaviour to perform during this time is controlled by the Offset Ty parameter.";
        this.genParams.Add(this.offset);

        this.offsetType = new ParamEnum("Offset Ty", "Offset Type", offsetEnum, 0);
        this.offsetType.description = "How to process the audio signal while offsetting.";
        this.genParams.Add(this.offsetType);

        this.recordAmt = new ParamTimeLen("Amount", 1.0f, ParamTimeLen.TimeLenType.Seconds, ParamTimeLen.WidgetTime);
        this.recordAmt.description = "The amount of time to record.";
        this.genParams.Add(this.recordAmt);

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
            return ZeroGen();

        PxPre.Phonics.GenBase gb =
            this.input.Reference.SpawnGenerator(
                freq, 
                beatsPerSec, 
                samplesPerSec, 
                amp,
                spawnFrom,
                collection);

        float offset = this.offset.GetWavelength(freq, beatsPerSec);
        float record = this.recordAmt.GetWavelength(freq, beatsPerSec);
            
        PxPre.Phonics.GenCycle.OffsetPass offsetType = PxPre.Phonics.GenCycle.OffsetPass.Pass;
        switch(this.offsetType.value)
        { 
            default:
            case (int)PxPre.Phonics.GenCycle.OffsetPass.Pass:
                offsetType = PxPre.Phonics.GenCycle.OffsetPass.Pass;
                break;

            case (int)PxPre.Phonics.GenCycle.OffsetPass.Silent:
                offsetType = PxPre.Phonics.GenCycle.OffsetPass.Silent;
                break;

            case (int)PxPre.Phonics.GenCycle.OffsetPass.Hold:
                offsetType = PxPre.Phonics.GenCycle.OffsetPass.Hold;
                break;
        }

        return 
            new PxPre.Phonics.GenCycle( 
                (int)(offset * samplesPerSec), 
                (int)(record * samplesPerSec), 
                offsetType,
                gb);
    }

    public override NodeType nodeType => NodeType.Cycle;

    public override LLDNBase CloneType()
    {
        return new LLDNCycle();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "After a certain amount of time, start recording and replaying a small window of time in a loop.";
}
