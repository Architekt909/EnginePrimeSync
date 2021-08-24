using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class Track : DbObject, IEquatable<Track>
	{
		public string Path { get; set; }
		public string Filename { get; private set; }

		private byte[] _trackDataBlob;
		public byte[] TrackDataBlob
		{
			get => _trackDataBlob;
			set
			{
				_trackDataBlob = value;
				DecodeTrackData();
			}
		}

		private byte[] _cueBlob;
		public byte[] CueBlob
		{
			get => _cueBlob;
			set
			{
				_cueBlob = value;
				DecodeCueData();
			}
		}

		private byte[] _loopBlob;
		public byte[] LoopBlob
		{
			get => _loopBlob;
			set
			{
				_loopBlob = value;
				DecodeLoopData();
			}
		}

		public string Artist { get; set; }
		public string Album { get; set; }
		public string Genre { get; set; }
		public string Comment { get; set; }
		public double SampleRate { get; private set; }
		public ulong LengthInSamples { get; private set; }
		public double LengthInSeconds => LengthInSamples / SampleRate;

		public List<Cue> Cues { get; private set; } = new List<Cue>();
		public List<Loop> Loops { get; private set; } = new List<Loop>();
		
		public Track(int id, string path, string fileName) : base(id)
		{
			Path = path;
			Filename = fileName;
		}

		private void DecodeTrackData()
		{
			// first 8 bytes are sample rate in HZ
			SampleRate = BitConverter.ToDouble(Util.FlipEndian(_trackDataBlob, 0, 8));
			LengthInSamples = BitConverter.ToUInt64(Util.FlipEndian(_trackDataBlob, 8, 8));
		}

		private void DecodeCueData()
		{
			// This should always be 8
			ulong numCues = BitConverter.ToUInt64(Util.FlipEndian(_cueBlob, 0, 8));
			int offset = 8;

			for (uint i = 0; i < numCues; i++)
			{
				if (Cue.CheckAndSkipBlankCue(_cueBlob, ref offset))
					continue;

				var cue = new Cue(_cueBlob, ref offset);
				cue.PositionInSeconds = cue.PositionInSamples / SampleRate;
				Cues.Add(cue);
			}
		}

		private void DecodeLoopData()
		{
			int numLoops = _loopBlob[0];    // should always be 8
			
			// 7 bytes of padding
			int offset = 8;

			for (int i = 0; i < numLoops; i++)
			{
				if (Loop.CheckAndSkipBlankLoop(_loopBlob, ref offset))
					continue;

				var loop = new Loop(_loopBlob, ref offset);
				loop.StartPositionInSeconds = loop.StartPositionInSamples / SampleRate;
				loop.EndPositionInSeconds = loop.EndPositionInSamples / SampleRate;
				Loops.Add(loop);
			}
		}

		// Less strict than Equals. Used to just see if the tracks represent the same file
		public bool IsSameAsTrack(Track t)
		{
			return t != null &&
				   Id == t.Id &&
				   Name == t.Name &&
				   Filename == t.Filename &&
				   Name == t.Name;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as Track);
		}

		public bool Equals(Track other)
		{
			return other != null &&
				   Id == other.Id &&
				   Name == other.Name &&
				   Path == other.Path &&
				   Filename == other.Filename &&
				   EqualityComparer<byte[]>.Default.Equals(_trackDataBlob, other._trackDataBlob) &&
				   EqualityComparer<byte[]>.Default.Equals(_cueBlob, other._cueBlob) &&
				   EqualityComparer<byte[]>.Default.Equals(_loopBlob, other._loopBlob) &&
				   Artist == other.Artist &&
				   Album == other.Album &&
				   Genre == other.Genre &&
				   Comment == other.Comment &&
				   (Math.Abs(SampleRate - other.SampleRate) < 1.0) &&
				   LengthInSamples == other.LengthInSamples &&
				   (Math.Abs(LengthInSeconds - other.LengthInSeconds) < 1.0) &&
				   EqualityComparer<List<Cue>>.Default.Equals(Cues, other.Cues) &&
				   EqualityComparer<List<Loop>>.Default.Equals(Loops, other.Loops);
		}

		public override int GetHashCode()
		{
			HashCode hash = new HashCode();
			hash.Add(Id);
			hash.Add(Name);
			hash.Add(Path);
			hash.Add(Filename);
			hash.Add(_trackDataBlob);
			hash.Add(_cueBlob);
			hash.Add(_loopBlob);
			hash.Add(Artist);
			hash.Add(Album);
			hash.Add(Genre);
			hash.Add(Comment);
			hash.Add(SampleRate);
			hash.Add(LengthInSamples);
			hash.Add(LengthInSeconds);
			hash.Add(Cues);
			hash.Add(Loops);
			return hash.ToHashCode();
		}

		public static bool operator ==(Track left, Track right)
		{
			return EqualityComparer<Track>.Default.Equals(left, right);
		}

		public static bool operator !=(Track left, Track right)
		{
			return !(left == right);
		}
	}
}
