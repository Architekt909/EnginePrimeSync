using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnginePrimeSync.DB
{
	public class Track
	{
		public int Id { get; private set; }
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

		public string Title { get; set; }
		public string Artist { get; set; }
		public string Album { get; set; }
		public string Genre { get; set; }
		public string Comment { get; set; }
		public double SampleRate { get; private set; }
		public ulong LengthInSamples { get; private set; }
		public double LengthInSeconds => LengthInSamples / SampleRate;

		public List<Cue> Cues { get; private set; } = new List<Cue>();
		public List<Loop> Loops { get; private set; } = new List<Loop>();
		
		public Track(int id, string path, string fileName)
		{
			Id = id;
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

	}
}
