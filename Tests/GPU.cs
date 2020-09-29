using System.IO;
using System.Linq;

using NUnit.Framework;


namespace Tests
{
    class GPU
    {
        public static byte[] LoadGameROM() => File.ReadAllBytes(@"..\..\..\rom\Tetris (World) (Rev A).gb");

        private BootBase Proc = new BootBase(BootBase.LoadBootROM(), LoadGameROM().ToList());

        [Ignore("No GPU yet")]
        public void DoBootGPU()
        {
            while (Proc.PC != 0x1d)
                Proc.DoNextOP();
            while (Proc.PC != 0x28)
                Proc.DoNextOP();
            while (Proc.PC != 0x98)
                Proc.DoNextOP();
            while (Proc.PC != 0x9c)
                Proc.DoNextOP();
            while (Proc.PC != 0xa1)
                Proc.DoNextOP();
            while (Proc.PC != 0xa3)
                Proc.DoNextOP();
            while (Proc.PC != 0xe0) //Start of logo check
                Proc.DoNextOP();
            while (Proc.PC != 0xf1) //logo checksum initialized
                Proc.DoNextOP();
            while (Proc.PC != 0xf9) //Past first subloop
                Proc.DoNextOP();
            while (Proc.PC != 0xfa) //Logo checksum validation
                Proc.DoNextOP();

            Proc.DoNextOP();
            Assert.AreNotEqual(0xfa, Proc.PC); //Logo if logo check failed and we are stuck

            while (Proc.PC != 0x100)
                Proc.DoNextOP();

            Assert.AreEqual(0x100, Proc.PC);
        }

        [Test]
        public void GPUFieldsGetSetDuringBoot()
        {
            //Sets GPU to be in VBlank
            Proc.dec.Storage.Write(0xff44, 0x90);
            while (Proc.PC != 0x100)
                Proc.DoNextOP();

            Assert.AreEqual(0x100, Proc.PC);
        }
    }
}
