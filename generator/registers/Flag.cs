using System.Dynamic;

namespace generator
{
    public enum Flag
    {
        Z,
        NZ,
        N,
        NN,
        H,
        NH,
        C,
        NC
    }

    public enum FlagChange
    {
        Unaffected,
        Conditional,
        Set,
        Unset
    }

    public record FlagChangeOptions
    {
        FlagChange Zero;
        FlagChange Negative;
        FlagChange HalfCarry;
        FlagChange Carry;
    }

    public record FlagResult
    {
        bool Zero;
        bool Negative;
        bool HalfCarry;
        bool Carry;
    }
}
