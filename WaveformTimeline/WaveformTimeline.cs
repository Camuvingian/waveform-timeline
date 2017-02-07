using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WaveformTimeline.Models;

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
	[TemplatePart(Name = "PART_Selection", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Regions", Type = typeof(Canvas))]
	[TemplatePart(Name = "PART_Progress", Type = typeof(Canvas))]
	public class WaveformTimeline : Control 
	{
		private IWaveformPlayer waveformPlayer;

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

		private double startSelectionRegion = -1;
		private double endSelectionRegion = -1;

		private const int PROGRESS_TRIANGLE_WIDTH = 4;

		#region Ctor.
		static WaveformTimeline()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(WaveformTimeline), new FrameworkPropertyMetadata(typeof(WaveformTimeline)));
		}
		#endregion // Ctor.

		#region Public Methods and Event Handlers.
		/// <summary>
		/// Register the waveform player used to visualize the waveform.
		/// </summary>
		/// <param name="waveformPlayer">The waveform player to use for rendering.</param>
		public void RegisterWaveformPlayer(IWaveformPlayer waveformPlayer)
		{
			this.waveformPlayer = waveformPlayer;
			waveformPlayer.PropertyChanged += WaveformPlayer_PropertyChanged;
		}

		private void WaveformPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "WaveformData":
					UpdateWaveform();
					break;
				case "ChannelPosition":
					UpdateProgressIndicator();
					break;
				case "ChannelLength":
					startSelectionRegion = endSelectionRegion = -1;
					UpdateAllRegions();
					break;
				case "SelectionBegin":
					startSelectionRegion = waveformPlayer.SelectionBegin.TotalSeconds;
					UpdateSelectionRegion();
					break;
				case "SelectionEnd":
					endSelectionRegion = waveformPlayer.SelectionEnd.TotalSeconds;
					UpdateSelectionRegion();
					break;
				default:
					break;
			}
		}


		#endregion // Public Methods and Event Handlers.

		#region Private Utility and Rendering Methods.
		private void UpdateAllRegions()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Render the timeline.
		/// </summary>
		private void UpdateTimeline()
		{
			if (waveformPlayer == null || timelineCanvas == null)
				return;

			// Remove markers and textblocks.
			foreach (TextBlock textBlock in timestampTextBlocks)
				timelineCanvas.Children.Remove(textBlock);
			timestampTextBlocks.Clear();

			foreach (Line line in timelineTicks)
				timelineCanvas.Children.Remove(line);
			timelineTicks.Clear();

			double floor = timelineCanvas.RenderSize.Height - 1;
			timelineBackgroundRegion.Width = timelineCanvas.RenderSize.Width;
			timelineBackgroundRegion.Height = timelineCanvas.RenderSize.Height;

			// TODO We might need longer here. Do something more sophistocated? 
			TimelineIntervals intervals = new TimelineIntervals(5.0, 1.0);
			if (waveformPlayer.ChannelLength >= 120.0) 
				intervals = new TimelineIntervals(60.0, 15.0);
			else if (waveformPlayer.ChannelLength >= 60.0)
				intervals = new TimelineIntervals(30.0, 5.0);
			else if (waveformPlayer.ChannelLength >= 30.0)
				intervals = new TimelineIntervals(10.0, 2.0);

			if (waveformPlayer.ChannelLength < intervals.Minor)
				return;

			// TODO draw some stuff.
		}

		/// <summary>
		/// Render the waveform.
		/// </summary>
		private void UpdateWaveform()
		{
			
		}

		/// <summary>
		/// Here we create the triangular progress indicator.
		/// </summary>
		private void CreateProgressIndicator()
		{
			if (waveformPlayer == null || timelineCanvas == null || progressCanvas == null)
				return;

			const double x0 = 0.0;
			progressLine.X1 = x0;
			progressLine.X2 = x0;
			progressLine.Y1 = timelineCanvas.RenderSize.Height;
			progressLine.Y2 = progressCanvas.RenderSize.Height;

			// Here we render the progress arrow marker.
			PolyLineSegment indicator = new PolyLineSegment();
			indicator.Points.Add(new Point(x0, timelineCanvas.RenderSize.Height));
			indicator.Points.Add(new Point(
				x0 - PROGRESS_TRIANGLE_WIDTH / 2, 
				timelineCanvas.RenderSize.Height - PROGRESS_TRIANGLE_WIDTH));
			indicator.Points.Add(new Point(
				x0 + PROGRESS_TRIANGLE_WIDTH / 2,
				timelineCanvas.RenderSize.Height - PROGRESS_TRIANGLE_WIDTH));
			indicator.Points.Add(new Point(x0, timelineCanvas.RenderSize.Height));

			// Generate the geometry.
			PathFigure indicatorFigure = new PathFigure();
			indicatorFigure.Segments.Add(indicator);

			PathGeometry indicatorGeometry = new PathGeometry();
			indicatorGeometry.Figures.Add(indicatorFigure);

			progressIndicator.Data = indicatorGeometry;
			UpdateProgressIndicator();
		}

		/// <summary>
		/// Render the progress indicator in the correct location.
		/// </summary>
		private void UpdateProgressIndicator()
		{
			if (waveformPlayer == null || progressCanvas == null)
				return;

			double x = 0.0;
			if (waveformPlayer.ChannelLength != 0)
			{
				double progressPercent = waveformPlayer.ChannelPosition / waveformPlayer.ChannelLength;
				x = progressPercent * progressCanvas.RenderSize.Width;
			}
			progressLine.Margin = new Thickness(x, 0, 0, 0);
			progressIndicator.Margin = new Thickness(x, 0, 0, 0);
		}

		/// <summary>
		/// Render the selection region.
		/// </summary>
		private void UpdateSelectionRegion()
		{
			if (waveformPlayer == null || selectionCanvas == null)
				return;

			double startPercent = startSelectionRegion / waveformPlayer.ChannelLength;
			double startX = startPercent * selectionCanvas.RenderSize.Width;
			double endPercent = endSelectionRegion / waveformPlayer.ChannelLength;
			double endX = endPercent * selectionCanvas.RenderSize.Width;

			if (waveformPlayer.ChannelLength == 0 || endX <= startX)
			{
				selectionRegion.Width = selectionRegion.Height = 0;
				return;
			}
			selectionRegion.Margin = new Thickness(startX, 0, 0, 0);
			selectionRegion.Width = endX - startX;
			selectionRegion.Height = selectionCanvas.RenderSize.Height; // TODO Need to override templates.
		}

		private void UpdateWaveformCacheScaling()
		{
			if (waveformCanvas == null)
				return;

			// Here we use the BitmapCache class to improve rendering performance of this UIElement. 
			// We create a BitmapCache and assign it to the CacheMode property of a UIElement to cache 
			// the element and its subtree as a bitmap in video memory. This is useful when you need to 
			// animate, translate, or scale a UIElement as quickly as possible. This approach enables a 
			// tradeoff between performance and visual quality while content is cached.
			BitmapCache waveformCache = (BitmapCache)waveformCanvas.CacheMode;
			if (AutoScaleWaveformCache)
			{
				// TODO Get transform scale and render at that scale.
			}
			else
				waveformCache.RenderAtScale = 1.0;
		}

		private bool IsPointInSelectionRegion(Point p)
		{
			if (waveformPlayer.ChannelLength == 0)
				return false;

			double regionLeft = (waveformPlayer.SelectionBegin.TotalSeconds / 
				waveformPlayer.ChannelLength) * RenderSize.Width;
			double regionRight = (waveformPlayer.SelectionEnd.TotalSeconds /
				waveformPlayer.ChannelLength) * RenderSize.Width;

			return p.X >= regionLeft && p.X < regionRight;
		}
		#endregion // Private Utility and Rendering Methods.

		#region Dependency Properties Left Level Waveform Brushes.
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

		[Category("Brushes")]
		public Brush LeftLevelStrokeBrush
		{
			get { return (Brush)GetValue(LeftLevelStrokeBrushProperty); }
			set { SetValue(LeftLevelStrokeBrushProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="LeftLevelStrokeBrush" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty LeftLevelStrokeBrushProperty =
			 DependencyProperty.Register("LeftLevelStrokeBrush", typeof(Brush), typeof(WaveformTimeline),
				 new UIPropertyMetadata(new SolidColorBrush(Colors.Green), 
					 OnLeftLevelStrokeBrushChanged, OnCoerceLeftLevelStrokeBrush));

		private static void OnLeftLevelStrokeBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnLeftLevelStrokeBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="LeftLevelStrokeBrush"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="LeftLevelStrokeBrush"/></param>
		/// <param name="newValue">The new value of <see cref="LeftLevelStrokeBrush"/></param>
		protected virtual void OnLeftLevelStrokeBrushChanged(Brush oldValue, Brush newValue)
		{
			leftPath.Stroke = LeftLevelStrokeBrush;
			//UpdateWaveform();
		}

		private static object OnCoerceLeftLevelStrokeBrush(DependencyObject o, object baseValue)
		{
			WaveformTimeline waveformTimeline = o as WaveformTimeline;
			return waveformTimeline != null ?
				waveformTimeline.OnCoerceLeftLevelStrokeBrush((Brush)baseValue) :
				baseValue;
		}

		/// <summary>
		/// Implementations of this callback should check the value in baseValue and 
		/// determine based on either the value or the type whether this is a value that 
		/// needs to be further coerced.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="LeftLevelStrokeBrush"/></param>
		/// <returns>The adjusted value of <see cref="LeftLevelStrokeBrush"/></returns>
		protected virtual Brush OnCoerceLeftLevelStrokeBrush(Brush value)
		{
			return value;
		}
		#endregion // Dependency Properties Left Level Waveform Brushes.

		#region Dependency Properties Right Level Waveform Brushes.
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

		/// <summary>
		/// Called after the <see cref="RightLevelBrush"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="RightLevelBrush"/></param>
		/// <param name="newValue">The new value of <see cref="RightLevelBrush"/></param>
		protected virtual void OnRightLevelBrushChanged(Brush oldValue, Brush newValue)
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

		[Category("Brushes")]
		public Brush RightLevelStrokeBrush
		{
			get { return (Brush)GetValue(RightLevelStrokeBrushProperty); }
			set { SetValue(RightLevelStrokeBrushProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="RightLevelStrokeBrush" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty RightLevelStrokeBrushProperty =
			 DependencyProperty.Register("RightLevelStrokeBrush", typeof(Brush), typeof(WaveformTimeline),
				 new UIPropertyMetadata(new SolidColorBrush(Colors.Purple), 
					 OnRightLevelStrokeBrushChanged, OnCoerceRightLevelStrokeBrush));

		private static void OnRightLevelStrokeBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnRightLevelStrokeBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="RightLevelStrokeBrush"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="RightLevelStrokeBrush"/></param>
		/// <param name="newValue">The new value of <see cref="RightLevelStrokeBrush"/></param>
		protected virtual void OnRightLevelStrokeBrushChanged(Brush oldValue, Brush newValue)
		{
			rightPath.Stroke = RightLevelStrokeBrush;
			//UpdateWaveform();
		}

		private static object OnCoerceRightLevelStrokeBrush(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ?
				waveformTimeline.OnCoerceRightLevelStrokeBrush((Brush)baseValue) :
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="RightLevelStrokeBrush"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="RightLevelStrokeBrush"/></param>
		/// <returns>The adjusted value of <see cref="RightLevelStrokeBrush"/></returns>
		protected virtual Brush OnCoerceRightLevelStrokeBrush(Brush value)
		{
			return value;
		}
		#endregion // Dependency Properties Right Level Waveform Brushes.

		#region Dependency Properties ProgressBar.
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

		/// <summary>
		/// Get or sets the thickness of the progress indicator bar.
		/// </summary>
		[Category("Common")]
		public double ProgressBarThickness
		{
			get { return (double)GetValue(ProgressBarThicknessProperty); }
			set { SetValue(ProgressBarThicknessProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="ProgressBarThickness" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ProgressBarThicknessProperty =
			 DependencyProperty.Register("ProgressBarThickness", typeof(double), typeof(WaveformTimeline), 
				 new UIPropertyMetadata(2.0d, OnProgressBarThicknessChanged, OnCoerceProgressBarThickness));

		private static void OnProgressBarThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnProgressBarThicknessChanged((double)e.OldValue, (double)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="ProgressBarThickness"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="ProgressBarThickness"/></param>
		/// <param name="newValue">The new value of <see cref="ProgressBarThickness"/></param>
		protected virtual void OnProgressBarThicknessChanged(double oldValue, double newValue)
		{
			progressLine.StrokeThickness = ProgressBarThickness;
			//CreateProgressIndicator();
		}

		private static object OnCoerceProgressBarThickness(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ?
				waveformTimeline.OnCoerceProgressBarThickness((double)baseValue) :
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="ProgressBarThickness"/> when a new value is applied.
		/// </summary>
		/// <param name="baseValue">The value that was set on <see cref="ProgressBarThickness"/></param>
		/// <returns>The adjusted value of <see cref="ProgressBarThickness"/></returns>
		protected virtual double OnCoerceProgressBarThickness(double baseValue)
		{
			baseValue = Math.Max(baseValue, 0.0d);
			return baseValue;
		}
		#endregion // Dependency Properties ProgressBar.

		#region Dependency Properties CenterLine.
		/// <summary>
		/// Gets or sets a brush used to draw the center line separating left and right levels.
		/// </summary>
		[Category("Brushes")]
		public Brush CenterLineBrush
		{
			get { return (Brush)GetValue(CenterLineBrushProperty); }
			set { SetValue(CenterLineBrushProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="CenterLineBrush" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty CenterLineBrushProperty =
			 DependencyProperty.Register("CenterLineBrush", typeof(Brush), typeof(WaveformTimeline), 
				 new UIPropertyMetadata(new SolidColorBrush(Colors.Black), 
					 OnCenterLineBrushChanged, OnCoerceCenterLineBrush));


		private static void OnCenterLineBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnCenterLineBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="CenterLineBrush"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="CenterLineBrush"/></param>
		/// <param name="newValue">The new value of <see cref="CenterLineBrush"/></param>
		protected virtual void OnCenterLineBrushChanged(Brush oldValue, Brush newValue)
		{
			centerLine.Stroke = CenterLineBrush;
			//UpdateWaveform();
		}

		private static object OnCoerceCenterLineBrush(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ? 
				waveformTimeline.OnCoerceCenterLineBrush((Brush)baseValue) : 
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="CenterLineBrush"/> when a new value is applied.
		/// </summary>
		/// <param name="baseValue">The value that was set on <see cref="CenterLineBrush"/></param>
		/// <returns>The adjusted value of <see cref="CenterLineBrush"/></returns>
		protected virtual object OnCoerceCenterLineBrush(Brush baseValue)
		{
			return baseValue;
		}

		/// <summary>
		/// Gets or sets the thickness of the center line separating left and right levels.
		/// </summary>
		[Category("Common")]
		public double CenterLineThickness
		{
			get { return (double)GetValue(CenterLineThicknessProperty); }
			set { SetValue(CenterLineThicknessProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="CenterLineThickness" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty CenterLineThicknessProperty = 
			DependencyProperty.Register("CenterLineThickness", typeof(double), typeof(WaveformTimeline), 
				new UIPropertyMetadata(1.0d, OnCenterLineThicknessChanged, OnCoerceCenterLineThickness));

		private static void OnCenterLineThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnCenterLineThicknessChanged((double)e.OldValue, (double)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="CenterLineThickness"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="CenterLineThickness"/></param>
		/// <param name="newValue">The new value of <see cref="CenterLineThickness"/></param>
		protected virtual void OnCenterLineThicknessChanged(double oldValue, double newValue)
		{
			centerLine.StrokeThickness = CenterLineThickness;
			//UpdateWaveform();
		}

		private static object OnCoerceCenterLineThickness(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ?
				waveformTimeline.OnCoerceCenterLineThickness((double)baseValue) :
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="CenterLineThickness"/> when a new value is applied.
		/// </summary>
		/// <param name="baseValue">The value that was set on <see cref="CenterLineThickness"/></param>
		/// <returns>The adjusted value of <see cref="CenterLineThickness"/></returns>
		protected virtual double OnCoerceCenterLineThickness(double baseValue)
		{
			baseValue = Math.Max(baseValue, 0.0d);
			return baseValue;
		}
		#endregion // Dependency Properties CenterLine.

		#region Dependency Properties Selection Region.
		/// <summary>
		/// Gets or sets a brush used to draw the repeat region on the waveform.
		/// </summary>
		[Category("Brushes")]
		public Brush SelectionRegionBrush
		{
			get { return (Brush)GetValue(SelectionRegionBrushProperty); }
			set { SetValue(SelectionRegionBrushProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="SelectionRegionBrush"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SelectionRegionBrushProperty =
			DependencyProperty.Register("SelectionRegionBrush", typeof(Brush), typeof(WaveformTimeline),
				new UIPropertyMetadata(new SolidColorBrush(Color.FromArgb(129, 246, 255, 0)), 
					OnSelectionRegionBrushChanged, OnCoerceSelectionRegionBrush));

		private static void OnSelectionRegionBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnSelectionRegionBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="SelectionRegionBrush"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="SelectionRegionBrush"/></param>
		/// <param name="newValue">The new value of <see cref="SelectionRegionBrush"/></param>
		protected virtual void OnSelectionRegionBrushChanged(Brush oldValue, Brush newValue)
		{
			selectionRegion.Fill = SelectionRegionBrush;
			//UpdateSelectionRegion();
		}

		private static object OnCoerceSelectionRegionBrush(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ? 
				waveformTimeline.OnCoerceRepeatRegionBrush((Brush)baseValue) :
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="SelectionRegionBrush"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="SelectionRegionBrush"/></param>
		/// <returns>The adjusted value of <see cref="SelectionRegionBrush"/></returns>
		protected virtual Brush OnCoerceRepeatRegionBrush(Brush value)
		{
			return value;
		}

		/// <summary>
		/// Gets or sets a value that indicates whether selection regions will be created 
		/// via mouse drag across the waveform.
		/// </summary>
		[Category("Common")]
		public bool AllowSelectionRegions
		{
			get { return (bool)GetValue(AllowSelectionRegionsProperty); }
			set { SetValue(AllowSelectionRegionsProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="AllowSelectionRegions"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowSelectionRegionsProperty =
			DependencyProperty.Register("AllowSelectionRegions", typeof(bool), typeof(WaveformTimeline),
				new UIPropertyMetadata(true, OnAllowSelectionRegionsChanged, OnCoerceAllowSelectionRegions));

		private static void OnAllowSelectionRegionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnAllowSelectionRegionsChanged((bool)e.OldValue, (bool)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="AllowSelectionRegions"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="AllowSelectionRegions"/></param>
		/// <param name="newValue">The new value of <see cref="AllowSelectionRegions"/></param>
		protected virtual void OnAllowSelectionRegionsChanged(bool oldValue, bool newValue)
		{
			if (!newValue && waveformPlayer != null)
			{
				waveformPlayer.SelectionBegin = TimeSpan.Zero;
				waveformPlayer.SelectionEnd = TimeSpan.Zero;
			}
		}

		private static object OnCoerceAllowSelectionRegions(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ? 
				waveformTimeline.OnCoerceAllowSelectionRegions((bool)baseValue) : 
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="AllowSelectionRegions"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="AllowSelectionRegions"/></param>
		/// <returns>The adjusted value of <see cref="AllowSelectionRegions"/></returns>
		protected virtual bool OnCoerceAllowSelectionRegions(bool value)
		{
			return value;
		}
		#endregion // Dependency Properties Selection Region.

		#region Dependency Properties Timeline.
		/// <summary>
		/// Gets or sets a brush used to draw the tickmarks on the timeline.
		/// </summary>
		[Category("Brushes")]
		public Brush TimelineTickBrush
		{
			get { return (Brush)GetValue(TimelineTickBrushProperty);	}
			set { SetValue(TimelineTickBrushProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="TimelineTickBrush"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty TimelineTickBrushProperty = 
			DependencyProperty.Register("TimelineTickBrush", typeof(Brush), typeof(WaveformTimeline), 
				new UIPropertyMetadata(new SolidColorBrush(Colors.Black), 
					OnTimelineTickBrushChanged, OnCoerceTimelineTickBrush));

		private static void OnTimelineTickBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnTimelineTickBrushChanged((Brush)e.OldValue, (Brush)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="TimelineTickBrush"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="TimelineTickBrush"/></param>
		/// <param name="newValue">The new value of <see cref="TimelineTickBrush"/></param>
		protected virtual void OnTimelineTickBrushChanged(Brush oldValue, Brush newValue)
		{
			// UpdateTimeline();
		}

		private static object OnCoerceTimelineTickBrush(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ? 
				waveformTimeline.OnCoerceTimelineTickBrush((Brush)baseValue) : 
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="TimelineTickBrush"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="TimelineTickBrush"/></param>
		/// <returns>The adjusted value of <see cref="TimelineTickBrush"/></returns>
		protected virtual Brush OnCoerceTimelineTickBrush(Brush value)
		{
			return value;
		}
		#endregion // Dependency Properties Timeline.

		#region Dependency Properties for [Auto-Scale] Waveform.
		/// <summary>
		/// Gets or sets a value indicating whether the waveform should attempt to autoscale
		/// its render buffer in size. This code was adapted directly from WPFSoundVisualizationLib 
		/// which has no licence restrictions.
		/// </summary>
		/// <remarks>
		/// If true, the control will attempt to set the waveform's bitmap cache
		/// at a resolution based on the sum of all ScaleTransforms applied
		/// in the control's visual tree heirarchy. This can make the waveform appear
		/// less blurry if a ScaleTransform is applied at a higher level.
		/// The only ScaleTransforms that are considered here are those that have 
		/// uniform vertical and horizontal scaling (generally used to "zoom in"
		/// on a window or controls).
		/// </remarks>
		[Category("Common")]
		public bool AutoScaleWaveformCache
		{
			get { return (bool)GetValue(AutoScaleWaveformCacheProperty); }
			set { SetValue(AutoScaleWaveformCacheProperty, value); }
		}

		/// <summary>
		/// Identifies the <see cref="AutoScaleWaveformCache" /> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AutoScaleWaveformCacheProperty =
			DependencyProperty.Register("AutoScaleWaveformCache", typeof(bool), typeof(WaveformTimeline),
				new UIPropertyMetadata(false, OnAutoScaleWaveformCacheChanged, OnCoerceAutoScaleWaveformCache));

		private static void OnAutoScaleWaveformCacheChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			if (waveformTimeline != null)
				waveformTimeline.OnAutoScaleWaveformCacheChanged((bool)e.OldValue, (bool)e.NewValue);
		}

		/// <summary>
		/// Called after the <see cref="AutoScaleWaveformCache"/> value has changed.
		/// </summary>
		/// <param name="oldValue">The previous value of <see cref="AutoScaleWaveformCache"/></param>
		/// <param name="newValue">The new value of <see cref="AutoScaleWaveformCache"/></param>
		protected virtual void OnAutoScaleWaveformCacheChanged(bool oldValue, bool newValue)
		{
			//UpdateWaveformCacheScaling();
		}

		private static object OnCoerceAutoScaleWaveformCache(DependencyObject d, object baseValue)
		{
			WaveformTimeline waveformTimeline = d as WaveformTimeline;
			return waveformTimeline != null ? 
				waveformTimeline.OnCoerceAutoScaleWaveformCache((bool)baseValue) : 
				baseValue;
		}

		/// <summary>
		/// Coerces the value of <see cref="AutoScaleWaveformCache"/> when a new value is applied.
		/// </summary>
		/// <param name="value">The value that was set on <see cref="AutoScaleWaveformCache"/></param>
		/// <returns>The adjusted value of <see cref="AutoScaleWaveformCache"/></returns>
		protected virtual bool OnCoerceAutoScaleWaveformCache(bool value)
		{
			return value;
		}
		#endregion // Dependency Properties for [Auto-Scale] Waveform.

		// TODO. Add dependency properties for highlight regions. 


	}
}