using System;

namespace WaveformTimeline.Demo.Helpers
{
	/// <summary>
	/// Class tht facilitates mapping from Views to ViewModels.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class ViewAttribute : Attribute
	{
		public ViewAttribute(Type viewType)
		{
			ViewType = viewType;
		}

		public object Context { get; set; }
		public Type ViewType { get; private set; }
	}
}