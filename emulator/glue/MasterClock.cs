﻿namespace emulator.glue;

public class MasterClock
{
    private long clock;
    public void Tick() => clock++;

    public long Now() => clock;
}