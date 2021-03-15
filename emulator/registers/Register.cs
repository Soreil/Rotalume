namespace emulator
{
    public enum Register
    {
        A,
        B,
        C,
        D,
        E,
        F,
        H,
        L,
        HL //HL is a psuedoregister, it reads a byte from memory at location HL
    }
}
