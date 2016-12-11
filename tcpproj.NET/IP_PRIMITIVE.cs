using System;
namespace tcpproj
{
	
	/// <summary> IP_PRIMITIVE:
	/// The IP_PRIMITIVE is a primitive that carries a SEGMENT.
	/// Any primitives sent to or from IP should be this type of prim.
	/// Used for SEND, RECEIVE_RESPONSE, IP_DELIVER, IP_SEND prim types.
	/// </summary>
	
	public class IP_PRIMITIVE:DATA_PRIMITIVE
	{
		
		// Public Attributes
		/// <summary>Defines the Source Port (or application).  </summary>
		public short source_port_ = 0;
        virtual public short SourcePort
        {
            get { return source_port_; }
            set { source_port_ = value; }
        }		
		
		/// <summary>Defines the Destination Port (or application) </summary>
		public short dest_port_ = 0;
        virtual public short DestPort
        {
            get { return dest_port_; }
            set { dest_port_ = value; }
        }		
		/// <summary>Source IP address </summary>
		public ADDRESS source_addr_ = null;
        virtual public ADDRESS SourceAddr
        {
            get { return source_addr_; }
            set { source_addr_ = value; }
        }			
		/// <summary>Destination IP address </summary>
		public ADDRESS dest_addr_ = null;
        virtual public ADDRESS DestAddr
        {
            get { return dest_addr_; }
            set { dest_addr_ = value; }
        }			
		// Public Functions
		// Constructor for Application Driver to generate, with room for IP header
		public IP_PRIMITIVE(int id, PRIM_TYPE prim, int byte_count, SEGMENT s):base(id, prim, byte_count, s, 0)
		{
		}
		public IP_PRIMITIVE(int id, PRIM_TYPE prim, int byte_count, SEGMENT s, int push):base(id, prim, byte_count, s, push)
		{
		}
		
		// Constructor for stub to generate: an IP datagram
		public IP_PRIMITIVE(PRIM_TYPE prim, short source_port, ADDRESS source_addr, short dest_port, ADDRESS dest_addr, int length, SEGMENT s):base(0, prim, length, s)
		{
			source_port_ = source_port;
			source_addr_ = source_addr;
			dest_port_ = dest_port; dest_addr_ = dest_addr;
		}
		public override void  clear()
		{
			base.clear(); source_port_ = dest_port_ = 0;
		}
		
		/// <summary>Formats an IP_SEND.  Attaches segment. Initializes IP_PRIM header </summary>
		public virtual void  format(short source_port, ADDRESS source_addr, short dest_port, ADDRESS dest_addr, int length, SEGMENT s)
		{
			clear();
			prim_type_ = PRIM_TYPE.IP_SEND;
			source_port_ = source_port;
			source_addr_ = source_addr;
			dest_port_ = dest_port;
			dest_addr_ = dest_addr;
			Segment = s;
			byte_count_ = length;
		}
		
		////////////// IP_PRIMITIVE::print()
		/// <summary>Print an IP_PRIMITIVE in formatted way </summary>
		public override void  print(DIRECTION direction)
		{
			base.print(direction);
			// if DATA_PRIM only, don't print IP_PRIM info
			switch (direction)
			{
				
				case DIRECTION.IP2TCP: 
				case DIRECTION.TCP2IP: 
					MyPrint.SmartWrite("\t\t Source Port:" + source_port_ + " Addr:" + source_addr_.address());
					MyPrint.SmartWriteLine("  Dest Port:" + dest_port_ + " Addr:" + dest_addr_.address());
					break;
				}
		}
	}
}
