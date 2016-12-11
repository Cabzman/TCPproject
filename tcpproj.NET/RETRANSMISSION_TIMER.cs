using System;
namespace tcpproj
{
	
	//////////////////////////////////////////////////
	/// <summary> RETRANSMISSION_TIMER:
	/// These primitives are used to set and expire a timer,
	/// and thus can be sent to or received from TCP.
	/// Used for SET_, EXPIRE_, CLEAR_RETX_TIMER or FORCE_TIMER.
	/// </summary>
	//////////////////////////////////////////////////
	
	public class RETRANSMISSION_TIMER:PRIMITIVE
	{
		
		// Private attributes
		private long set_time_;
		/// <summary>   The time the timer was set 
        virtual public long SetTime
        {
            get { return set_time_; }
            set { set_time_ = value; }
        }	
		private long expire_time_;
		/// <summary>   The time the timer is to expire     
        virtual public long ExpireTime
        {
            get { return expire_time_; }
            set { expire_time_ = value; }
        }	
		// Public functions
		public RETRANSMISSION_TIMER(int id, PRIMITIVE.PRIM_TYPE prim_type, long set_time, long expire_time):base(id, prim_type)
		{
			set_time_ = set_time; expire_time_ = expire_time;
		}

		
		////////////// RETRANSMISSION_TIMER::print()
		/// <summary>Prints a Retransmission_Timer primitive </summary>
		public override void  print(DIRECTION direction)
		{
			base.print(direction);
			MyPrint.SmartWriteLine("\t\t Set_Time:" + set_time_ + "  Expire_Time:" + expire_time_);
		}
	}
}
