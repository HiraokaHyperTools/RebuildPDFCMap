using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibRebuildPDFCMap.Helpers
{
    internal class BigBinaryReader
    {
        public BigBinaryReader(Stream stream)
        {
            BaseStream = stream;
        }

        public Stream BaseStream { get; }

        public virtual byte ReadByte()
        {
            var v = BaseStream.ReadByte();
            if (v < 0)
            {
                throw new EndOfStreamException();
            }
            return (byte)v;
        }

        public virtual short ReadInt16() => (short)ReadUInt16();

        public virtual ushort ReadUInt16()
        {
            var buffer = new byte[2];
            if (BaseStream.Read(buffer, 0, 2) != 2)
            {
                throw new EndOfStreamException();
            }
            return (ushort)(((ushort)buffer[0] << 8) | ((ushort)buffer[1]));
        }

        public virtual int ReadInt32() => (int)ReadUInt32();

        public virtual uint ReadUInt32()
        {
            var buffer = new byte[4];
            if (BaseStream.Read(buffer, 0, 4) != 4)
            {
                throw new EndOfStreamException();
            }
            return (uint)((((uint)buffer[0]) << 24) | (((uint)buffer[1]) << 16) | (((uint)buffer[2]) << 8) | ((uint)buffer[3]));
        }
    }
}
