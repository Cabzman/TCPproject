using System;
namespace tcpproj
{
	
	/// <summary> DATA_PRIMITIVE:
	/// The Data Primitive carries application-layer data
	/// in the Segment.  The IP_Primitive inherits from this
	/// primitive type.  Use the IP_Primitive instead, since
	/// it holds an IP header in addition.
	/// </summary>
	public class DATA_PRIMITIVE:PRIMITIVE
	{
		// Public Attributes
		private int push_flag_; // Push Flag
        virtual public int PushFlag
        {
            get { return push_flag_; }
            set { push_flag_ = value; }
        }			
		/// <summary>The Segment which contains protocol headers and application data </summary>
		private SEGMENT s_;
        virtual public SEGMENT Segment
        {
            get { return s_; }
            set { s_ = value; }
        }			
		// Public Functions
		public DATA_PRIMITIVE(int id, PRIM_TYPE prim, int byte_count, SEGMENT s):base(id, prim, byte_count)
		{
			s_ = s; push_flag_ = 1;
		}
		public DATA_PRIMITIVE(int id, PRIM_TYPE prim, int byte_count, SEGMENT s, int push):base(id, prim, byte_count)
		{
			s_ = s; push_flag_ = push;
		}
		
		/// <summary>Discards the SEGMENT </summary>
		public virtual void  dispose()
		{
			s_ = null;
		}
		
		/// <summary>Clears the PRIMITIVE </summary>
		public override void  clear()
		{
			base.clear(); push_flag_ = 0;
		}
		
		// accessors
		/// <summary>Removes the SEGMENT from the DATA_PRIM. </summary>
		public virtual SEGMENT remove_segment()
		// indicate segment given away, once given
		{
			SEGMENT t = s_; s_ = null; return t;
		}
		
		/// <summary>Returns a SEGMENT without removal </summary>
		public virtual SEGMENT s()
		{
			return s_;
		}
		
		//////////////////// DATA_PRIMITIVE::print()
		/// <summary>Prints a Data_Primitive </summary>
		public override void  print(DIRECTION direction)
		{
			base.print(direction);
			// Print Segment
			if (s_ == null)
				MyPrint.SmartWriteLine("\t\tNo Segment Received!!!");
			else
			{
				MyPrint.SmartWriteLine("\t\t Push:" + push_flag_);
				switch (direction)
				{
					
					case DIRECTION.IP2TCP: 
					case DIRECTION.TCP2IP: 
						s_.print_segment(byte_count_);
						break;
					
					default: 
						s_.print_data(byte_count_);
						break;
					
				}
			}
		}
	}
}