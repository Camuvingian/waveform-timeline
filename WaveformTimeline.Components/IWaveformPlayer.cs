﻿using System;
using System.ComponentModel;

namespace WaveformTimeline.Components
{
	/// <summary>
	/// Provides access to sound player functionality needed to
	/// generate a Waveform.
	/// </summary>
	public interface IWaveformPlayer : INotifyPropertyChanged
	{
		/// <summary>
		/// Gets the raw level data for the waveform.
		/// </summary>
		/// <remarks>
		/// Level data should be structured in an array where each sucessive index
		/// alternates between left or right channel data, starting with left. Index 0
		/// should be the first left level, index 1 should be the first right level, index
		/// 2 should be the second left level, etc.
		/// </remarks>
		float[] WaveformData { get; }

		/// <summary>
		/// Gets or sets the current sound streams playback position.
		/// </summary>
		double ChannelPosition { get; set; }

		/// <summary>
		/// Gets the total channel length in seconds.
		/// </summary>
		double ChannelLength { get; }

		/// <summary>
		/// Gets or sets the starting time for a selection.
		/// </summary>
		TimeSpan SelectionBegin { get; set; }

		/// <summary>
		/// Gets or sets the ending time for a selection.
		/// </summary>
		TimeSpan SelectionEnd { get; set; }

		// TODO add the highlight regions here.
	}
}