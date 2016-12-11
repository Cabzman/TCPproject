using System;
using tcpproj;
namespace tcpproj
{
	
	//////////////////////////////////////////////////////////
	/// <summary> OPEN_PRIMITIVE:
	/// Application requests TCP to establish a connection.
	/// TCP shall send OPEN_RESPONSE to Application when connection established.
	/// </summary>
	public class OPEN_PRIMITIVE:PRIMITIVE
	{
		///////////// Definitions
		// Open_Type definitions
		/// <summary>Active_Open = Source-initiated; Passive_Open = Dest-initiated </summary>
        public enum OPEN_TYPE { ACTIVE_OPEN = 1, PASSIVE_OPEN = 2 };

		///////////// Protected Attributes
		/// <summary>Source Port Address </summary>
		protected internal short source_port_;
        virtual public short SourcePort
        {
            get { return source_port_; }
            set { source_port_ = value; }
        }
        /// <summary>Destination Port Address </summary>
		protected internal short dest_port_;
        virtual public short DestPort
        {
            get { return dest_port_; }
            set { dest_port_ = value; }
        }		
		/// <summary>Destination IP address </summary>
		protected internal ADDRESS dest_addr_;
        virtual public int DestAddr
        {
            get { return dest_addr_.get_IP_addr(); }
            set { dest_addr_.set_IP_addr(value); }
        }		
		
		/// <summary>PASSIVE or ACTIVE </summary>
		protected internal OPEN_TYPE open_type_;
        virtual public OPEN_TYPE OpenType
        {
            get { return open_type_; }
            set { open_type_ = value; }
        }		
		///////////// Public Functions
		public OPEN_PRIMITIVE(int connect_id, short source_port, short dest_port, ADDRESS dest_addr, short byte_count, OPEN_TYPE open_type):base((short) connect_id, PRIMITIVE.PRIM_TYPE.OPEN, byte_count)
		{
			source_port_ = source_port;
			dest_port_ = dest_port;
			dest_addr_ = dest_addr;
			open_type_ = open_type;
		}
		
		/// <summary>Returns the initial receive window size </summary>
		public virtual int Window()
		{
			return byte_count_;
		}

		
		///////////////////// OPEN_PRIMITIVE::print()
		/// <summary>Prints an OPEN_PRIMITIVE </summary>
		public override void  print(DIRECTION direction)
		{
			base.print(direction);
			MyPrint.SmartWrite("\t\t Source_Port:" + source_port_ + "  Dest_Port:" + dest_port_);
			MyPrint.SmartWrite("  Dest_Addr:" + dest_addr_.address());
			MyPrint.SmartWriteLine("  Window:" + byte_count_);
			MyPrint.SmartWrite("\t\t Open_Type:");
			if (open_type_ == OPEN_TYPE.ACTIVE_OPEN)
				MyPrint.SmartWriteLine("ACTIVE_OPEN");
			else
				MyPrint.SmartWriteLine("PASSIVE_OPEN");
		}
	}
}
