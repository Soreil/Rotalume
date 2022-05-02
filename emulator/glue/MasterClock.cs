﻿namespace emulator;

internal class MasterClock
{
    private long clock;
    public void Tick(object? o, EventArgs e)
    {
        clock++;
    }

    public long Now() => clock;
}