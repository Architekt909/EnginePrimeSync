using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync
{
	public static class Util
	{
		public static byte[] FlipEndian(byte[] byteArray, int startOffset, int numBytesToRead, int pad = 0)
		{
			var bytes = new byte[numBytesToRead + pad];
			Array.Copy(byteArray, startOffset, bytes, 0, numBytesToRead);
			Array.Reverse(bytes);
			return bytes;
		}

		public static string StringFromByteArray(byte[] byteArray, int startOffset, int length)
		{
			// Strings aren't stored little endian or null terminated
			string str = "";

			for (int i = startOffset; i < startOffset + length; i++)
				str += (char)byteArray[i];

			return str;
		}
		

		public static byte[] DecompressBytes(byte[] byteArray)
		{
			using MemoryStream ms = new MemoryStream();
			ms.Write(byteArray);
			ms.Seek(0, SeekOrigin.Begin);

			// First 4 bytes are unique to the qt compression method and indicate the uncompressed length. 
			// Read these first as DeflateStream can't process them.
			var uncompressedLengthBytes = new byte[4];

			for (int i = 0; i < 4; i++)
				uncompressedLengthBytes[i] = (byte)ms.ReadByte();

			// It's stored as little endian but we need big endian so flip the array. Not actually using this atm, mainly doing this as an exercise to validate the data formatting info I found online
			Array.Reverse(uncompressedLengthBytes);
			var uncompressedLength = BitConverter.ToUInt32(uncompressedLengthBytes);

			// The next 2 bytes are zlib header details which we need to skip as DeflateStream doesn't use/understand them
			ms.ReadByte();
			ms.ReadByte();

			using var decompressor = new DeflateStream(ms, CompressionMode.Decompress);

			using MemoryStream outMs = new MemoryStream();
			decompressor.CopyTo(outMs);
			outMs.Position = 0;

			return outMs.GetBuffer();
		}
	}
}
