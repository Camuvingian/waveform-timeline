using System;

namespace WaveformTimeline
{
	public interface IWaveformPlayer : ISoundPlayer
	{
		float[] WaveformData { get; }
		double ChannelPosition { get; set; }
		double ChannelLength { get; }
		TimeSpan SelectionBegin { get; set; }
		TimeSpan SelectionEnd { get; set; }
	}
}