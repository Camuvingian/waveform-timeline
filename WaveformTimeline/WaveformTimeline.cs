using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WaveformTimeline
{
	/// <summary>
	/// Control that displays a stereo waveform and allows the users to select/highlight a timespan. 
	/// The control also allows regions to be marked indicating parts of the timeline to be 
	/// considered by the user. 
	/// </summary>
	[DisplayName("Waveform Timeline")]
	[Description("Controls which displays a sterio waveform and alows the user to select a timespan.")]
	[ToolboxItem(true)]
	[TemplatePart(Name = "PART_Waveform", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Timeline", Type = typeof(Canvas))] 
	[TemplatePart(Name =	"PART_Selection", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Regions", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Progress", Type = typeof(Canvas))]
	public class WaveformTimeline : Control 
	{
		private IWaveformPlayer soundPlayer;

		private Canvas waveformCanvas;
		private Canvas timelineCanvas;
		private Canvas selectionCanvas;
		private Canvas regionsCanvas;
		private Canvas progressCanvas;

		private readonly Path leftPath = new Path();
		private readonly Path rightPath = new Path();
		private readonly Path progressIndicator = new Path();

		private readonly Line centerLine = new Line();
		private readonly Line progressLine = new Line();

		private readonly List<Line> timelineTicks = new List<Line>();
		private readonly List<TextBlock> timestampTextBlocks = new List<TextBlock>();

		private readonly Rectangle selectionRegion = new Rectangle();
		private readonly Rectangle timelineBackgroundRegion = new Rectangle();
		private readonly List<Rectangle> highlightRegions = new List<Rectangle>();

		#region Ctor.
		static WaveformTimeline()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(WaveformTimeline), new FrameworkPropertyMetadata(typeof(WaveformTimeline)));
		}
		#endregion // Ctor.

		#region Dependency Property Left Level Waveform Brush.
		[Category("Brushes")]
		public Brush LeftLevelBrush
		{
			get { return (Brush)GetValue(LeftLevelBrushProperty); }
			set { SetValue(LeftLevelBrushProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="LeftLevelBrush" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty LeftLevelBrushProperty =
			 DependencyProperty.Register("LeftLevelBrush", typeof(Brush), typeof(WaveformTimeline),
				 new UIPropertyMetadata(new SolidColorBrush(Colors.Green), OnLeftLevelBrushChanged, OnCoerceLeftLevelBrush));

		private static void OnLeftLevelBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnLeftLevelBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="LeftLevelBrush"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="LeftLevelBrush"/></param>
		/// <param name="newValue">The new value of <see cref="LeftLevelBrush"/></param>
		protected virtual void OnLeftLevelBrushChanged(Brush oldValue, Brush newValue)
		{
			leftPath.Fill = LeftLevelBrush;
			//UpdateWaveform();
		}

		private static object OnCoerceLeftLevelBrush(DependencyObject o, object baseValue)
		{
			WaveformTimeline waveformTimeline = o as WaveformTimeline;
			return waveformTimeline != null ?
				waveformTimeline.OnCoerceLeftLevelBrush((Brush)baseValue) :
				baseValue;
		}

		/// <summary>
		/// Implementations of this callback should check the value in baseValue and 
		/// determine based on either the value or the type whether this is a value that 
		/// needs to be further coerced.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="LeftLevelBrush"/></param>
		/// <returns>The adjusted value of <see cref="LeftLevelBrush"/></returns>
		protected virtual Brush OnCoerceLeftLevelBrush(Brush value)
		{
			return value;
		}
		#endregion // Dependency Property Left Level Waveform Brush.

		#region Dependency Property Right Level Waveform Brush.
		[Category("Brushes")]		
		public Brush RightLevelBrush
		{
			get { return (Brush)GetValue(RightLevelBrushProperty); }
			set { SetValue(RightLevelBrushProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="RightLevelBrush" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RightLevelBrushProperty =
			 DependencyProperty.Register("RightLevelBrush", typeof(Brush), typeof(WaveformTimeline),
				 new UIPropertyMetadata(new SolidColorBrush(Colors.Purple), OnRightLevelBrushChanged, OnCoerceRightLevelBrush));

		private static void OnRightLevelBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnRightLevelBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
		}

		private void OnRightLevelBrushChanged(Brush oldValue, Brush newValue)
		{
			rightPath.Fill = RightLevelBrush;
			//UpdateWaveform();
		}

		private static object OnCoerceRightLevelBrush(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ?
				waveformTimeline.OnCoerceRightLevelBrush((Brush)baseValue) : 
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="RightLevelBrush"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="RightLevelBrush"/></param>
		/// <returns>The adjusted value of <see cref="RightLevelBrush"/></returns>
		protected virtual Brush OnCoerceRightLevelBrush(Brush value)
		{
			return value;
		}
		#endregion // Dependency Property Right Level Waveform Brush.

		#region Dependency Property ProgressBar Brush.
		[Category("Brushes")]
		public Brush ProgressBarBrush
		{
			get { return (Brush)GetValue(ProgressBarBrushProperty); }
			set { SetValue(ProgressBarBrushProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ProgressBarBrush.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ProgressBarBrushProperty =
			 DependencyProperty.Register("ProgressBarBrush", typeof(Brush), typeof(WaveformTimeline), 
					new UIPropertyMetadata(new SolidColorBrush(Colors.Gold), OnProgressBrushChanged, OnCoerceProgressBrush));

		private static void OnProgressBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnProgressBarBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="ProgressBarBrush"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="ProgressBarBrush"/></param>
		/// <param name="newValue">The new value of <see cref="ProgressBarBrush"/></param>
		protected virtual void OnProgressBarBrushChanged(Brush oldValue, Brush newValue)
		{
			progressIndicator.Fill = ProgressBarBrush;
			progressLine.Stroke = ProgressBarBrush;
			//CreateProgressIndicator(); //TODO
		}

		private static object OnCoerceProgressBrush(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ? 
				waveformTimeline.OnCoerceProgressBarBrush((Brush)baseValue) : 
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="ProgressBarBrush"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="ProgressBarBrush"/></param>
		/// <returns>The adjusted value of <see cref="ProgressBarBrush"/></returns>
		protected virtual Brush OnCoerceProgressBarBrush(Brush value)
		{
			return value;
		}
		#endregion // Dependency Property ProgressBar Brush.



	}
}
