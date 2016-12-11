using System;
namespace tcpproj
{
	/// <summary> ADDRESS:
	/// IP ADDRESS Definitions 
	/// Stores and prints an IP Address within a Prim or TCB
	/// </summary>
	
	public class ADDRESS
	{
		// Private Attributes
		/// <summary>The IP address is stored as an integer </summary>
		private int addr_;
		// Public Functions
		public ADDRESS()
		{
			addr_ = 0;
		}
		public ADDRESS(int value_Renamed)
		{
			addr_ = value_Renamed;
		}
		public virtual int get_IP_addr()
		{
			return addr_;
		}
		public virtual void  set_IP_addr(int value_Renamed)
		{
			addr_ = value_Renamed;
		}
		
		///////////// IP_ADDRESS OUTPUT 
		/// <summary>Return a string of the IP address in proper form for printing: n.n.n.n </summary>
		public virtual System.String address()
		{
			return ((addr_ >> 24) & 0xff) + "." + ((addr_ >> 16) & 0xff) + "." + ((addr_ >> 8) & 0xff) + "." + (addr_ & 0xff);
		}
	}
}