﻿// MIT License
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

public class LLDNSign : LLDNBase
{

    const int enumFull = 0;
    const int enumPos = 1;
    const int enumNeg = 2;

    static EnumVals outputEnum =
        new EnumVals(
            new EnumVals.Entry("full",  enumFull,   "Full",         "Negative values return -1.0 and positive values return 1.0."),
            new EnumVals.Entry("pos",   enumPos,    "Positive",     "Negative values return 0.0 and positive values return 1.0."),
            new EnumVals.Entry("neg",   enumNeg,    "Negative",     "Negative values return -1.0 and positive values return 0.0"));

    ParamConnection input;
    ParamEnum outputType = null;

    public LLDNSign()
        : base()
    { }
    
    public LLDNSign(string guid)
        : base(guid)
    { }

    protected override void _Init()
    { 
        this.input = new ParamConnection("Input", null);
        this.input.description = "The audio signal to process.";
        this.genParams.Add(this.input);

        this.outputType = new ParamEnum("Mode", "range", outputEnum, enumFull);
        this.outputType.description = "The range of the output.";
        this.genParams.Add(this.outputType);
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

        int mode = this.outputType.value;

        if(mode == enumPos)
            return new PxPre.Phonics.GenSignPos(samplesPerSec, gb);
        else if(mode == enumNeg)
            return new PxPre.Phonics.GenSignNeg(samplesPerSec, gb);
        else
            return new PxPre.Phonics.GenSign(samplesPerSec, gb);
    }

    public override NodeType nodeType => NodeType.Sign;

    public override LLDNBase CloneType()
    {
        return new LLDNSign();
    }

    public override Category GetCategory() => Category.Operations;

    public override string description => 
        "Get the sign of an audio signal, per element; the sign being whether a number is negative, zero, or positive.\n\n" +
        "The node does a broad quantization, where positive audio signal elements will either be 1.0 or 0.0f, and negative values will either be -1.0 or 0.0f, depending on the Mode parameter.";
}
