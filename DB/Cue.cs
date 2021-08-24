using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class Cue : IEquatable<Cue>
	{
		public string Label { get; private set; }
		public double PositionInSamples { get; private set; }
		public double PositionInSeconds { get; set; }
		public byte Red { get; private set; }
		public byte Green { get; private set; }
		public byte Blue { get; private set; }

		public Cue(byte[] bytes, ref int offset)
		{
			byte labelLength = (byte)BitConverter.ToChar(bytes, offset);
			++offset;

			Label = Util.StringFromByteArray(bytes, offset, labelLength);
			offset += labelLength;

			// will be -1 if none
			PositionInSamples = BitConverter.ToDouble(Util.FlipEndian(bytes, offset, 8));

			offset += 8;
			++offset;   // this value is the alpha color value, it's always FF
			Red = bytes[offset++];
			Green = bytes[offset++];
			Blue = bytes[offset++];
		}

		// Returns true if blank cue
		public static bool CheckAndSkipBlankCue(byte[] bytes, ref int offset)
		{
			// verify first byte is 0 indicating no label hence no cue
			if (bytes[offset] == 0)
			{
				++offset;
				offset += 8;    // skip 8 byte sample position values
				offset += 4;	// skip ARGB values

				return true;
			}

			return false;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Cue);
		}

		public bool Equals(Cue other)
		{
			return other != null &&
				   Label == other.Label &&
				   (Math.Abs(PositionInSamples - other.PositionInSamples) < 0.01) &&
				   Red == other.Red &&
				   Green == other.Green &&
				   Blue == other.Blue;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Label, PositionInSamples, Red, Green, Blue);
		}

		public static bool operator ==(Cue left, Cue right)
		{
			return EqualityComparer<Cue>.Default.Equals(left, right);
		}

		public static bool operator !=(Cue left, Cue right)
		{
			return !(left == right);
		}
	}
}
