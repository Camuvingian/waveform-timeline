using Caliburn.Micro;
using MahApps.Metro.Controls;
using WaveformTimeline.Demo.Services;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace WaveformTimeline.Demo.Helpers
{
	public delegate void StartupTask();

	/// <summary>
	/// Don't worry about this stuff, this just sets up the UI to look metro like! 
	/// It also provides the ability to change the look and feel. 
	/// </summary>
	public class StartupTasks
	{
		private readonly IServiceLocator serviceLocator;

		[ImportingConstructor]
		public StartupTasks(IServiceLocator serviceLocator)
		{
			this.serviceLocator = serviceLocator;
		}

		[Export(typeof(StartupTask))]
		public void ApplyBindingScopeOverride()
		{
			var getNamedElements = BindingScope.GetNamedElements;
			BindingScope.GetNamedElements = o =>
			{
				var metroWindow = o as MetroWindow;
				if (metroWindow == null)
				{
					return getNamedElements(o);
				}

				var list = new List<FrameworkElement>(getNamedElements(o));
				var type = o.GetType();
				var fields =
					 o.GetType()
					  .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
					  .Where(f => f.DeclaringType == type);
				var flyouts =
					 fields.Where(f => f.FieldType == typeof(FlyoutsControl))
							 .Select(f => f.GetValue(o))
							 .Cast<FlyoutsControl>();
				list.AddRange(flyouts);
				return list;
			};
		}

		[Export(typeof(StartupTask))]
		public void ApplyViewLocatorOverride()
		{
			var viewLocator = this.serviceLocator.GetInstance<IViewLocator>();
			Caliburn.Micro.ViewLocator.GetOrCreateViewType = viewLocator.GetOrCreateViewType;
		}
	}
}
