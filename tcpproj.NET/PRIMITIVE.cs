using System;
namespace tcpproj
{
	
	///////////////////////////////////////////////////////////
	/// <summary> PRIMITIVE:
	/// Primitives are used to send commands between protocol layers.
	/// There is an inheritance structure and all Primitives inherit from
	/// the simple PRIMITIVE.  PRIMITIVE functions include formatting and
	/// printing.  Some primitives point to a SEGMENT.  The base class
	/// Primitive is used for RECEIVE, CLOSE, ABORT, CLOSING, OPEN_RESPONSE, 
	/// and CLOSE_RESPONSE  
	/// </summary>
	/////////////////////////////////////////////////////
	
	public class PRIMITIVE
	{
		// Definitions Print directions
		/// <summary>Definition for a Print Direction </summary>
        public enum DIRECTION
        {
            IP2TCP = 1, TCP2IP = 2, APP2TCP = 3, TCP2APP = 4, TCP2TIMER = 5,
            TIMER2TCP = 6, L2DEBUG = 7
        };
		
		// Definitions for bytes
		/// <summary>Name of a Primitive type </summary>
        public enum PRIM_TYPE
        {
            UNDEFINED, OPEN, SEND, RECEIVE, CLOSE, ABORT, CLOSING, OPEN_RESPONSE,
            RECEIVE_RESPONSE, CLOSE_RESPONSE, SET_RETX_TIMER, EXPIRE_RETX_TIMER, CLEAR_RETX_TIMER,
            SET_FORCE_TIMER, EXPIRE_FORCE_TIMER, CLEAR_FORCE_TIMER, IP_DELIVER, IP_SEND, NR_PRIMS
        };

		
		/// <summary>ASCII names for a Primitive type </summary>
		//UPGRADE_NOTE: Final was removed from the declaration of 'prim_name'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
		public static readonly System.String[] prim_name = new System.String[]{"UNDEFINED", "OPEN", "SEND", "RECEIVE", "CLOSE", "ABORT", "CLOSING", "OPEN_RESPONSE", "RECEIVE_RESPONSE", "CLOSE_RESPONSE", "SET_RETX_TIMER", "EXPIRE_RETX_TIMER", "CLEAR_RETX_TIMER", "SET_FORCE_TIMER", "EXPIRE_FORCE_TIMER", "CLEAR_FORCE_TIMER", "IP_DELIVER", "IP_SEND"};
		
		// Definitions for Status Codes
		/// <summary>Return codes for Primitives:  Success or Failure </summary>
        public enum STATUS { SUCCESS = 1, FAILURE = -1 };
		
		// Protetected Attributes
		/// <summary>Primitive Type (example: IP_DELIVER, IP_SEND, OPEN, CLOSE_RESPONSE ...) </summary>
		protected internal PRIM_TYPE prim_type_;
        virtual public PRIM_TYPE PrimType
        {
            get { return prim_type_; }
            set { prim_type_ = value; }
        }						
		
		/// <summary>Local connect ID </summary>
		protected internal int local_connect_;
        virtual public int LocalConnect
        {
            get { return local_connect_; }
            set { local_connect_ = value; }
        }						
		
		/// <summary>With data: Indicates byte count; without: augments TCP window. </summary>
		protected internal int byte_count_;
        virtual public int ByteCount
        {
            get { return byte_count_; }
            set { byte_count_ = value; }
        }						
		
		/// <summary>Application-specific:  Used for status (SUCCESS/FAILURE) in Response Prims </summary>
		protected internal STATUS code_;
        virtual public STATUS Code
        {
            get { return code_; }
            set { code_ = value; }
        }						
		
		// Public Functions
		internal PRIMITIVE(int id, PRIM_TYPE prim)
		{
			local_connect_ = id; prim_type_ = prim;
			byte_count_ = 0; code_ = 0;
		}
		internal PRIMITIVE(int id, PRIM_TYPE prim, int byte_count)
		{
			local_connect_ = id; prim_type_ = prim;
			byte_count_ = byte_count; code_ = 0;
		}
		internal PRIMITIVE(int id, PRIM_TYPE prim, int byte_count, STATUS code)
		{
			local_connect_ = id; prim_type_ = prim;
			byte_count_ = byte_count; code_ = code;
		}
		public virtual void  clear()
		{
			local_connect_ = 0; byte_count_ = 0; code_ = 0;
		}
		
		// accessors
		/// <summary>Returns the byte_count of the data (including headers) </summary>
		public virtual int byte_count()
		{
			return byte_count_;
		}
		/// <summary>Sets the byte count of the SEGMENT (including headers/data) </summary>
		public virtual void  set_byte_count(int length)
		{
			byte_count_ = length;
		}

		
		///////////////////////// PRIMITIVE FUNCTIONS //////////////
		//////////// Print
		/// <summary>Prints a Primitive </summary>
		public virtual void  print(DIRECTION direction)
		{
			// Print where the Primitive is going to
			MyPrint.SmartWrite("\n");
			switch (direction)
			{
				// print the source & direction	
				case DIRECTION.IP2TCP: 
					MyPrint.SmartWrite("IP->TCP     ");
					break;
				
				case DIRECTION.TCP2IP: 
					MyPrint.SmartWrite("IP<-TCP     ");
					break;
				
				case DIRECTION.APP2TCP: 
					MyPrint.SmartWrite("    TCP<-APP");
					break;
				
				case DIRECTION.TCP2APP: 
					MyPrint.SmartWrite("    TCP->APP");
					break;
				
				case DIRECTION.TCP2TIMER: 
					MyPrint.SmartWrite("    TCP->TIM");
					break;
				
				case DIRECTION.TIMER2TCP: 
					MyPrint.SmartWrite("    TCP<-TIM");
					break;
				
				case DIRECTION.L2DEBUG: 
					MyPrint.SmartWrite("LAYER2 DEBUG");
					break;
				}
			
			// Print the Prim name
			MyPrint.SmartWriteLine("  PRIMITIVE = " + prim_name[(int) PrimType] + "  Local_ID:" + local_connect_);
            MyPrint.SmartWrite("\t\t"); 
            if (code_ == STATUS.SUCCESS)
				MyPrint.SmartWrite("   Status:SUCCESS");
			else if (code_ == STATUS.FAILURE)
				MyPrint.SmartWrite("  Status:FAILURE");
			else if (code_ != 0)
				MyPrint.SmartWrite("   Code:" + code_);
            MyPrint.SmartWriteLine(" Byte_Count:" + byte_count_); 
		}
	}
}
