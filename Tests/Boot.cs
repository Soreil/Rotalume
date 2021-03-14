
using NUnit.Framework;


namespace Tests
{

    internal class Boot
    {
        public void DoBootNoGPU()
        {
            var Proc = TestHelpers.NewCore();
            //Temporary write which sets the VBlank to always be the current GPU stage. 
            //This will let us boot without a GPU.
            Proc.Memory.Write(0xff44, 0x90);

            while (Proc.CPU.PC != 0x1d)
                Proc.Step();
            while (Proc.CPU.PC != 0x28)
                Proc.Step();
            while (Proc.CPU.PC != 0x98)
                Proc.Step();
            while (Proc.CPU.PC != 0x9c)
                Proc.Step();
            while (Proc.CPU.PC != 0xa1)
                Proc.Step();
            while (Proc.CPU.PC != 0xa3)
                Proc.Step();
            while (Proc.CPU.PC != 0xe0) //Start of logo check
                Proc.Step();
            while (Proc.CPU.PC != 0xf1) //logo checksum initialized
                Proc.Step();
            while (Proc.CPU.PC != 0xf9) //Past first subloop
                Proc.Step();
            while (Proc.CPU.PC != 0xfa) //Logo checksum validation
                Proc.Step();

            Proc.Step();
            Assert.AreNotEqual(0xfa, Proc.CPU.PC); //Logo if logo check failed and we are stuck

            while (Proc.CPU.PC != 0x100)
                Proc.Step();

            Assert.AreEqual(0x100, Proc.CPU.PC);
        }
    }
}
