﻿using emulator;

using NAudio.Wave;

namespace WPFFrontend.Audio;
internal class Provider : WaveProvider16
{
    public Provider(int SampleRate, emulator.Samples samples) : base(SampleRate, 2)
    {
        this.SampleRate = SampleRate;
        Samples = samples;
    }

    public int SampleRate { get; }
    public Samples Samples { get; }

    public override int Read(short[] buffer, int offset, int sampleCount) => Samples.GetSamples(buffer, offset, sampleCount, SampleRate);
}

public class Player
{
    private readonly Provider provider;
    private readonly WaveOut waveOut;

    public Player(Samples samples)
    {
        waveOut = new WaveOut();
        waveOut.DesiredLatency = 50;

        provider = new Provider(44100, samples);

        waveOut.Init(provider);
    }

    public bool Playing { get; internal set; }

    public void Play()
    {
        Playing = true;
        waveOut.Play();
    }

    public void Stop()
    {
        Playing = false;
        waveOut.Stop();
    }
}