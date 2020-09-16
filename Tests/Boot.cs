using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using generator;

using NUnit.Framework;


namespace Tests
{
    class Boot
    {
        public static byte[] LoadGameROM() => File.ReadAllBytes(@"..\..\..\rom\Tetris (World) (Rev A).gb");

        private BootBase Proc = new BootBase(BootBase.LoadBootROM(), LoadGameROM().ToList());

        [Test]
        public void DoBootNoGPU()
        {
            //Temporary write which sets the VBlank to always be the current GPU stage. 
            //This will let us boot without a GPU.
            Proc.dec.Storage.Write(0xff44, 0x90);

            while (Proc.PC != 0x1d)
                DoNextOP(Proc.dec);
            while (Proc.PC != 0x28)
                DoNextOP(Proc.dec);
            while (Proc.PC != 0x98)
                DoNextOP(Proc.dec);
            while (Proc.PC != 0x9c)
                DoNextOP(Proc.dec);
            while (Proc.PC != 0xa1)
                DoNextOP(Proc.dec);
            while (Proc.PC != 0xa3)
                DoNextOP(Proc.dec);
            while (Proc.PC != 0xe0) //Start of logo check
                DoNextOP(Proc.dec);
            while (Proc.PC != 0xf1) //logo checksum initialized
                DoNextOP(Proc.dec);
            while (Proc.PC != 0xf9) //Past first subloop
                DoNextOP(Proc.dec);
            while (Proc.PC != 0xfa) //Logo checksum validation
                DoNextOP(Proc.dec);

            DoNextOP(Proc.dec);
            Assert.AreNotEqual(0xfa, Proc.PC); //Logo if logo check failed and we are stuck

            while (Proc.PC != 0x100)
                DoNextOP(Proc.dec);

            Assert.AreEqual(0x100, Proc.PC);
        }

        private void DoNextOP(Decoder dec)
        {
            var op = Proc.Read();
            if (op != 0xcb)
            {
                dec.Op((Unprefixed)op)();
            }
            else
            {
                var CBop = Proc.Read();
                dec.Op((Cbprefixed)CBop)();
            }
        }
    }
}
