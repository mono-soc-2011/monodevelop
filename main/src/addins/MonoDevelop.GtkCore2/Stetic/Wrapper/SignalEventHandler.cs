
namespace Stetic
{
	public delegate void SignalEventHandler (object sender, SignalEventArgs args);
	
	public class SignalEventArgs: ObjectWrapperEventArgs
	{
		public SignalEventArgs (ObjectWrapper wrapper, Signal signal): base (wrapper)
		{
			Signal = signal;
			FrontendNotfied = false;
		}
	
		public Signal Signal { get; private set; }
		
		//flag determines if signal was handled and should not be propagate further
		public bool FrontendNotfied { get; set; }
	}
}
