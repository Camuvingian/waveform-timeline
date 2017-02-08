namespace WaveformTimeline.Demo.Services
{
	/// <summary>
	/// For dependency injection, service location is considered an anti-pattern, but we don't 
	/// care for our non-enterprise application! 
	/// </summary>
	public interface IServiceLocator
	{
		T GetInstance<T>() where T : class;
	}
}