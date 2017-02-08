using Caliburn.Micro;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using WaveformTimeline.Components;
using WaveformTimeline.Demo;

namespace WaveformTimeLine.Demo.Services
{
	/// <summary>
	/// The waveform generator used to create of wavrform timeline data.
	/// </summary>
	/// <remarks>
	/// I have written this based on a streaming control which used .NET2.0 background worker 
	/// multithreading code, this needs changing to TPL and .NET4.5 toolkits before deployment.
	/// </remarks>
	public class WaveformGenerator : PropertyChangedBase, IWaveformPlayer, IDisposable
	{
		private static WaveformGenerator instance;
		private readonly BackgroundWorker waveformGenerateWorker = new BackgroundWorker(); // TODO. Weak, change to TPL.
		private readonly int fftDataSize = (int)FFTDataSize.FFT2048;
		
		private bool inChannelTimerUpdate;
		private double channelLength;
		private double channelPosition;
		private bool inChannelSet;

		private WaveStream activeStream;
		private WaveChannel32 inputStream;
		private SampleAggregator sampleAggregator;
		private SampleAggregator waveformAggregator;

		private string pendingWaveformPath;
		private float[] fullLevelData;
		private float[] waveformData;

		private TimeSpan selectionStart;
		private TimeSpan selectionStop;
		private bool inSelectionSet;

		private const int waveformCompressedPointCount = 2000;
		private const int repeatThreshold = 200;

		#region Initialize.
		private WaveformGenerator()
		{
			waveformGenerateWorker.DoWork += WaveformGenerateWorker_DoWork;
			waveformGenerateWorker.RunWorkerCompleted += WaveformGenerateWorker_RunWorkerCompleted;
			waveformGenerateWorker.WorkerSupportsCancellation = true;
		}

		/// <summary>
		/// Get engine instance.
		/// </summary>
		public static WaveformGenerator Instance
		{
			get
			{
				if (instance == null)
					instance = new WaveformGenerator();
				return instance;
			}
		}
		#endregion // Initialize.

		#region Public Methods.
		/// <summary>
		/// Open the desired sound file and generate waveform data using NAudio.
		/// </summary>
		/// <param name="path"></param>
		public void OpenFile(string path)
		{
			if (ActiveStream != null)
			{
				SelectionBegin = TimeSpan.Zero;
				SelectionEnd = TimeSpan.Zero;
				ChannelPosition = 0;
			}
			StopAndCloseStream();

			if (System.IO.File.Exists(path))
			{
				try
				{
					ActiveStream = new Mp3FileReader(path);
					inputStream = new WaveChannel32(ActiveStream);
					inputStream.Sample += InputStream_Sample;

					sampleAggregator = new SampleAggregator(fftDataSize);
					ChannelLength = inputStream.TotalTime.TotalSeconds;
					GenerateWaveformData(path);
				}
				catch
				{
					ActiveStream = null;
				}
			}
		}
		#endregion // Public Methods.

		#region Waveform Generation.
		private class WaveformGenerationParams
		{
			public WaveformGenerationParams(int points, string path)
			{
				Points = points;
				Path = path;
			}

			public int Points { get; protected set; }
			public string Path { get; protected set; }
		}

		private void GenerateWaveformData(string path)
		{
			if (waveformGenerateWorker.IsBusy)
			{
				pendingWaveformPath = path;
				waveformGenerateWorker.CancelAsync();
				return;
			}

			if (!waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0)
			{
				waveformGenerateWorker.RunWorkerAsync(
					new WaveformGenerationParams(waveformCompressedPointCount, path));
			}
		}

		private void WaveformGenerateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled)
			{
				if (!waveformGenerateWorker.IsBusy && waveformCompressedPointCount != 0)
				{
					waveformGenerateWorker.RunWorkerAsync(new WaveformGenerationParams(
						waveformCompressedPointCount, pendingWaveformPath));
				}
			}
		}

		private void WaveformGenerateWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			WaveformGenerationParams waveformParams = e.Argument as WaveformGenerationParams;
			using (Mp3FileReader waveformMp3Stream = new Mp3FileReader(waveformParams.Path))
			using (WaveChannel32 waveformInputStream = new WaveChannel32(waveformMp3Stream))
			{
				float maxLeftPointLevel = float.MinValue;
				float maxRightPointLevel = float.MinValue;
				float[] waveformCompressedPoints = new float[waveformParams.Points];

				int frameLength = fftDataSize;
				int frameCount = (int)((double)waveformInputStream.Length / (double)frameLength);
				int waveformLength = frameCount * 2;
				byte[] readBuffer = new byte[frameLength];

				List<float> waveformData = new List<float>();
				List<int> waveMaxPointIndexes = new List<int>();
				waveformAggregator = new SampleAggregator(frameLength);
				waveformInputStream.Sample += WaveStream_Sample;
				
				for (int i = 1; i <= waveformParams.Points; i++)
				{
					waveMaxPointIndexes.Add((int)Math.Round(waveformLength *
						((double)i / (double)waveformParams.Points), 0));
				}

				int readCount = 0;
				int currentPointIndex = 0;
				while (currentPointIndex * 2 < waveformParams.Points)
				{
					waveformInputStream.Read(readBuffer, 0, readBuffer.Length);

					waveformData.Add(waveformAggregator.LeftMaxVolume);
					waveformData.Add(waveformAggregator.RightMaxVolume);

					if (waveformAggregator.LeftMaxVolume > maxLeftPointLevel)
						maxLeftPointLevel = waveformAggregator.LeftMaxVolume;

					if (waveformAggregator.RightMaxVolume > maxRightPointLevel)
						maxRightPointLevel = waveformAggregator.RightMaxVolume;

					if (readCount > waveMaxPointIndexes[currentPointIndex])
					{
						waveformCompressedPoints[(currentPointIndex * 2)] = maxLeftPointLevel;
						waveformCompressedPoints[(currentPointIndex * 2) + 1] = maxRightPointLevel;
						maxLeftPointLevel = float.MinValue;
						maxRightPointLevel = float.MinValue;
						currentPointIndex++;
					}

					// Update Gui after 3000 points.
					if (readCount % 3000 == 0)
					{
						float[] clonedData = (float[])waveformCompressedPoints.Clone();
						App.Current.Dispatcher.Invoke(new System.Action(() =>
						{
							WaveformData = clonedData;
						}));
					}

					if (waveformGenerateWorker.CancellationPending)
					{
						e.Cancel = true;
						break;
					}
					readCount++;
				}

				// Finailize rendering.
				float[] finalClonedData = (float[])waveformCompressedPoints.Clone();
				App.Current.Dispatcher.Invoke(new System.Action(() =>
				{
					fullLevelData = waveformData.ToArray();
					WaveformData = finalClonedData;
				}));

				waveformInputStream.Close();
				waveformMp3Stream.Close();
			}
		}
		#endregion // Waveform Generation.

		#region Event Handlers.
		private void InputStream_Sample(object sender, SampleEventArgs e)
		{
			sampleAggregator.Add(e.Left, e.Right);

			long repeatStartPosition = (long)((SelectionBegin.TotalSeconds / 
				ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);

			long repeatStopPosition = (long)((SelectionEnd.TotalSeconds / 
				ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);

			if (((SelectionEnd - SelectionBegin) >= TimeSpan.FromMilliseconds(repeatThreshold)) && 
				 ActiveStream.Position >= repeatStopPosition)
			{
				sampleAggregator.Clear();
				ActiveStream.Position = repeatStartPosition;
			}
		}

		private void WaveStream_Sample(object sender, SampleEventArgs e)
		{
			waveformAggregator.Add(e.Left, e.Right);
		}
		#endregion // Event Handlers.

		#region Properties.
		/// <summary>
		/// Gets the raw level data for the waveform.
		/// </summary>
		/// <remarks>
		/// Level data should be structured in an array where each sucessive index
		/// alternates between left or right channel data, starting with left. Index 0
		/// should be the first left level, index 1 should be the first right level, index
		/// 2 should be the second left level, etc.
		/// </remarks>
		public float[] WaveformData
		{
			get { return waveformData; }
			protected set
			{
				float[] oldValue = waveformData;
				waveformData = value;
				if (oldValue != waveformData)
					NotifyOfPropertyChange(() => WaveformData);
			}
		}

		/// <summary>
		/// Gets the total channel length in seconds.
		/// </summary>
		public double ChannelLength
		{
			get { return channelLength; }
			protected set
			{
				double oldValue = channelLength;
				channelLength = value;
				if (oldValue != channelLength)
					NotifyOfPropertyChange(() => ChannelLength);
			}
		}

		/// <summary>
		/// Gets or sets the current sound streams playback position.
		/// </summary>
		public double ChannelPosition
		{
			get { return channelPosition; }
			set
			{
				if (!inChannelSet)
				{
					inChannelSet = true; // Avoid recursion
					double oldValue = channelPosition;
					double position = Math.Max(0, Math.Min(value, ChannelLength));
					if (!inChannelTimerUpdate && ActiveStream != null)
						ActiveStream.Position = (long)((position / ActiveStream.TotalTime.TotalSeconds) * ActiveStream.Length);

					channelPosition = position;
					if (oldValue != channelPosition)
						NotifyOfPropertyChange(() => ChannelPosition);
					inChannelSet = false;
				}
			}
		}

		/// <summary>
		/// Gets or sets the starting time for a selection.
		/// </summary>
		public TimeSpan SelectionBegin
		{
			get { return selectionStart; }
			set
			{
				if (!inSelectionSet)
				{
					inSelectionSet = true;
					TimeSpan oldValue = selectionStart;
					selectionStart = value;
					if (oldValue != selectionStart)
						NotifyOfPropertyChange(() => SelectionBegin);
					inSelectionSet = false;
				}
			}
		}

		/// <summary>
		/// Gets or sets the ending time for a selection.
		/// </summary>
		public TimeSpan SelectionEnd
		{
			get { return selectionStop; }
			set
			{
				if (!inChannelSet)
				{
					inSelectionSet = true;
					TimeSpan oldValue = selectionStop;
					selectionStop = value;
					if (oldValue != selectionStop)
						NotifyOfPropertyChange(() => SelectionEnd);
					inSelectionSet = false;
				}
			}
		}
		
		/// <summary>
		/// Acess to the active stream.
		/// </summary>
		public WaveStream ActiveStream
		{
			get { return activeStream; }
			protected set
			{
				WaveStream oldValue = activeStream;
				activeStream = value;
				if (oldValue != activeStream)
					NotifyOfPropertyChange(() => ActiveStream);
			}
		}
		#endregion // Properties.

		#region IDisposable Implementation.
		private bool disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
					StopAndCloseStream();

				disposed = true;
			}
		}

		private void StopAndCloseStream()
		{
			if (activeStream != null)
			{
				inputStream.Close();
				inputStream = null;

				ActiveStream.Close();
				ActiveStream = null;
			}
		}
		#endregion // IDisposable Implementation.
	}
}
