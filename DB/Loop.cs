using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class Loop : IEquatable<Loop>
	{
		public string Label { get; private set; }
		public double StartPositionInSamples { get; private set; }
		public double EndPositionInSamples { get; private set; }
		public double StartPositionInSeconds { get; set; }
		public double EndPositionInSeconds { get; set; }
		public bool StartPointSet { get; private set; }
		public bool EndPointSet { get; private set; }
		public byte Red { get; private set; }
		public byte Green { get; private set; }
		public byte Blue { get; private set; }


		public Loop(byte[] bytes, ref int offset)
		{
			byte labelLength = (byte)BitConverter.ToChar(bytes, offset);
			++offset;

			Label = Util.StringFromByteArray(bytes, offset, labelLength);
			offset += labelLength;

			// These are stored little-endian unlike with cue points and most other things, so no need to flip the byte order
			StartPositionInSamples = BitConverter.ToDouble(bytes, offset);
			offset += 8;
			EndPositionInSamples = BitConverter.ToDouble(bytes, offset);
			offset += 8;

			StartPointSet = bytes[offset++] == 1;
			EndPointSet = bytes[offset++] == 1;

			++offset;   // this value is the alpha color value, it's always FF
			Red = bytes[offset++];
			Green = bytes[offset++];
			Blue = bytes[offset++];
		}

		public static bool CheckAndSkipBlankLoop(byte[] bytes, ref int offset)
		{
			// verify first byte is 0 indicating no label hence no cue
			if (bytes[offset] == 0)
			{
				++offset;
				offset += 16;   // Skip start/end position, both being 8 byte doubles
				offset += 2;    // Skip start/end point set booleans
				offset += 4;	// Skip ARGB

				return true;
			}

			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Loop);
		}

		public bool Equals(Loop other)
		{
			return other != null &&
				   Label == other.Label &&
				   (Math.Abs(StartPositionInSamples - other.StartPositionInSamples) < 0.01) &&
				   (Math.Abs(EndPositionInSamples - other.EndPositionInSamples) < 0.01) &&
				   StartPointSet == other.StartPointSet &&
				   EndPointSet == other.EndPointSet &&
				   Red == other.Red &&
				   Green == other.Green &&
				   Blue == other.Blue;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Label, StartPositionInSamples, EndPositionInSamples, StartPointSet, EndPointSet, Red, Green, Blue);
		}

		public static bool operator ==(Loop left, Loop right)
		{
			return EqualityComparer<Loop>.Default.Equals(left, right);
		}

		public static bool operator !=(Loop left, Loop right)
		{
			return !(left == right);
		}
	}
}
