namespace emulator.sound;

public class APUState
{
    public required NoiseChannelState Noise { get; init; }
    public required ToneChannelState Tone { get; init; }
    public required ToneSweepChannelState ToneSweep { get; init; }
    public required WaveChannelState Wave { get; init; }

    public required bool OutputToLeftTerminal { get; init; }
    public required bool OutputToRightTerminal { get; init; }
    public required int OutputVolumeLeft { get; init; }
    public required int OutputVolumeRight { get; init; }

    public required bool Sound1LeftOn { get; init; }
    public required bool Sound1RightOn { get; init; }
    public required bool Sound2LeftOn { get; init; }
    public required bool Sound2RightOn { get; init; }
    public required bool Sound3LeftOn { get; init; }
    public required bool Sound3RightOn { get; init; }
    public required bool Sound4LeftOn { get; init; }
    public required bool Sound4RightOn { get; init; }

    public required bool MasterSoundDisable { get; init; }
    public required int SoundClock { get; init; }
    public required int FrameSequencerState { get; init; }

    public required double CapacitorLeft { get; init; }
    public required double CapacitorRight { get; init; }
}