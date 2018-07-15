﻿using NUnit.Framework;
using System;
using UnityEngine;

namespace UnityEngine.Networking.Tests
{
    [TestFixture()]
    public class NetworkWriterTest
    {

        [Test]
        public void TestWritingSmallMessage()
        {
            // try serializing <32kb and see what happens
            NetworkWriter writer = new NetworkWriter();
            for (int i = 0; i < 30000 / 4; ++i)
                writer.Write(i);
            Assert.That(writer.Position, Is.EqualTo(30000));
        }

        [Test]
        public void TestWritingLargeMessage()
        {
            // try serializing <32kb and see what happens
            NetworkWriter writer = new NetworkWriter();
            for (int i = 0; i < 40000 / 4; ++i)
                writer.Write(i);
            Assert.That(writer.Position, Is.EqualTo(40000));
        }

        [Test]
        public void TestResetting()
        {
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage((short)1337);
            writer.Write(1);
            writer.Write(2);
            writer.Write(3);
            writer.Write(4);
            writer.FinishMessage();

            // try SeekZero and reset afterwards
            int messageSize = writer.Position;
            writer.SeekZero();
            Assert.That(writer.Position, Is.Zero);
            writer.Position = messageSize;

            // check if .ToArray() returns array until .Position
            writer.Position = 4;
            Assert.That(writer.ToArray().Length, Is.EqualTo(4));
        }



        [Test]
        public void TestWritingAndReading()
        {
            // write all simple types once
            NetworkWriter writer = new NetworkWriter();
            writer.StartMessage((short)1337);
            writer.Write((char)1);
            writer.Write((byte)2);
            writer.Write((sbyte)3);
            writer.Write((bool)true);
            writer.Write((short)4);
            writer.Write((ushort)5);
            writer.Write((int)6);
            writer.Write((uint)7);
            writer.Write((long)8L);
            writer.Write((ulong)9L);
            writer.Write((float)10);
            writer.Write((double)11);
            writer.Write((decimal)12);
            writer.Write((string)null);
            writer.Write((string)"");
            writer.Write((string)"13");
            writer.Write(new byte[] { 14, 15 }, 0, 2); // just the byte array, no size info etc.
            writer.WriteBytesAndSize((byte[])null); // [SyncVar] struct values can have uninitialized byte arrays, null needs to be supported
            writer.WriteBytesAndSize(new byte[] { 17, 18 }, 0, 2); // buffer, no-offset, count
            writer.WriteBytesAndSize(new byte[] { 19, 20, 21 }, 1, 2); // buffer, offset, count
            writer.WriteBytesAndSize(new byte[] { 22, 23 }, 0, 2); // size, buffer

            writer.FinishMessage();

            byte[] data = writer.ToArray();


            // read them
            NetworkReader reader = new NetworkReader(writer.ToArray());

            Assert.That(reader.ReadInt16(), Is.EqualTo(data.Length - sizeof(ushort) * 2)); // msgType
            Assert.That(reader.ReadUInt16(), Is.EqualTo(1337)); // contentSize (messasge.size - 4 bytes header)
            Assert.That(reader.ReadChar(), Is.EqualTo(1));
            Assert.That(reader.ReadByte(), Is.EqualTo(2));
            Assert.That(reader.ReadSByte(), Is.EqualTo(3));
            Assert.That(reader.ReadBoolean(), Is.True);
            Assert.That(reader.ReadInt16(), Is.EqualTo(4));
            Assert.That(reader.ReadUInt16(), Is.EqualTo(5));
            Assert.That(reader.ReadInt32(), Is.EqualTo(6));
            Assert.That(reader.ReadUInt32(), Is.EqualTo(7));
            Assert.That(reader.ReadInt64(), Is.EqualTo(8));
            Assert.That(reader.ReadUInt64(), Is.EqualTo(9));
            Assert.That(reader.ReadSingle(), Is.EqualTo(10));
            Assert.That(reader.ReadDouble(), Is.EqualTo(11));
            Assert.That(reader.ReadDecimal(), Is.EqualTo(12));
            Assert.That(reader.ReadString(), Is.Null); // writing null string should write null in HLAPI Pro ("" in original HLAPI)
            Assert.That(reader.ReadString(), Is.EqualTo(""));
            Assert.That(reader.ReadString(), Is.EqualTo("13"));

            Assert.That(reader.ReadBytes(2), Is.EqualTo(new byte[] { 14, 15 }));

            Assert.That(reader.ReadBytesAndSize(), Is.Null);

            Assert.That(reader.ReadBytesAndSize(), Is.EqualTo(new byte[] { 17, 18 }));

            Assert.That(reader.ReadBytesAndSize(), Is.EqualTo(new byte[] { 20, 21 }));

            Assert.That(reader.ReadBytesAndSize(), Is.EqualTo(new byte[] { 22, 23 }));

            reader.SeekZero();
            Assert.That(reader.Position, Is.Zero);

        }
    }
}
