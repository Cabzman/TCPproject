using System;
namespace tcpproj
{
	
	/////////////////////////////////////////////////////////////////
	/// <summary> SEGMENT:
	/// The Segment carries the Application Data and attached protocol headers.
	/// </summary>
	
	public class SEGMENT
	{

		
		// Public Definitions
		/// <summary>Static define for length of TCP Header </summary>
		public const int TCP_HDR_LEN = 20;
		
		/// <summary>Maximum size (in bytes) of application data </summary>
		public const sbyte MAX_DATA = 125;
		
		/// <summary>Bit flag for TCP </summary>
		public const sbyte FIN = 1;     // Finish
		public const sbyte SYN = 2;	// Synchronize
		public const sbyte RST = 4;	// Reset
		public const sbyte PUSH = 8;	// Push
		public const sbyte ACK = 16;	// Acknowledgment
		public const sbyte URG = 32;	// Urgent
		
		// public attributes
		/// <summary>Source Port: When 0, TCP hdr not initialized and will not be printed. </summary>
		public short source_port_;
        virtual public short SourcePort
        {
            get { return source_port_; }
            set { source_port_ = (sbyte) value; }
        }		
		/// <summary>Destination Port </summary>
		public short dest_port_;
        virtual public short DestPort
        {
            get { return dest_port_; }
            set { dest_port_ = value; }
        }		
		
		/// <summary>Send Sequence Number </summary>
		public int seq_num_;
        virtual public int SeqNum
        {
            get { return seq_num_; }
            set { seq_num_ = value; }
        }		
		
		/// <summary>Acknowledge Sequence Number </summary>
		public int ack_num_;
        virtual public int AckNum
        {
            get { return ack_num_; }
            set { ack_num_ = value; }
        }				
		/// <summary>TCP Header Length </summary>
		public sbyte dataOffset_;
		virtual public sbyte DataOffset
		{
			get {	return (sbyte) (dataOffset_ >> 4); }	
			set {   dataOffset_ = (sbyte) (value << 4);	}	
		}		
		
		/// <summary>Can store: SYN, FIN, RST, PSH, ACK, URG </summary>
		public sbyte flags_;
		/// <summary>Ors one or more flags as a parameter </summary>
		virtual public sbyte Flags
		{
            get { return flags_; }
			set { flags_ = value; }
		}

		/// <summary>Window advertisement </summary>
		public short window_;
        virtual public short Window
        {
            get { return window_; }
            set { window_ = value; }
        }	
		/// <summary>Checksum: No need to set this </summary>
		public short checksum_;
		
		/// <summary>Urgent pointer: Always set to zero </summary>
		public short urgent_ptr_;
        virtual public short UrgentPtr
        {
            get { return urgent_ptr_; }
            set { urgent_ptr_ = value; }
        }			
		/// <summary>Used to store Application data or TCP options </summary>
		public System.String data_;
        virtual public String Data
        {
            get { return data_; }
            set { data_ = value; }
        }		
		
		///////////////////////////// SEGMENT FUNCTIONS ///////////
		/// <summary>Clears source & destination ports </summary>
		public SEGMENT()
		{
			source_port_ = dest_port_ = 0;
            urgent_ptr_ = 0;
            checksum_ = 0;
		}
		public SEGMENT(System.String data)
		{
			source_port_ = dest_port_ = 0;
            checksum_ = urgent_ptr_ = 0;
			data_ = data;
		}
		public virtual void  setFlag(sbyte flag)
		{
			flags_ |= flag;
		}
		
		/// <summary>Clears one or more flags passed as a parameter </summary>
		public virtual void  clearFlag(sbyte flag)
		{
			flags_ = (sbyte) (flags_ & ~flag);
		}
		
		/// <summary>Returns a 1 or 0 as the value of a specific flag </summary>
		public virtual sbyte printFlag(sbyte flag)
		{
			if ((flags_ & flag) > 0)
                return 1;
            else return 0;
		}

        /// <summary>Returns the value of a specific flag </summary>
        public virtual sbyte getFlag(sbyte flag)
        {
            return (sbyte)(flags_ & flag);
        }

        //////////  Print Segment
		/// <summary>This function prints a Segment in a formatted way. </summary>
		public virtual void  print_segment(int length)
		{
			if (getFlag(SYN) == SYN && getFlag(ACK) == 0)
				MyPrint.SmartWrite("\t\t TCP SEGMENT: SYN \n");
			else if (getFlag(SYN) == SYN && getFlag(ACK) == ACK)
				MyPrint.SmartWrite("\t\t TCP SEGMENT: SYN + ACK \n");
			else if (getFlag(FIN) == FIN && getFlag(ACK) == 0)
				MyPrint.SmartWrite("\t\t TCP SEGMENT: FIN\n");
			else if (getFlag(FIN) == FIN && getFlag(ACK) == ACK)
				MyPrint.SmartWrite("\t\t TCP SEGMENT: FIN + ACK\n");
			else if (getFlag(ACK) == ACK && length == TCP_HDR_LEN)
				MyPrint.SmartWrite("\t\t TCP SEGMENT: ACK\n");
			else if (getFlag(ACK) == ACK && data_.Length != 0)
				MyPrint.SmartWriteLine("\t\t TCP SEGMENT: DATA + ACK");
			else
				MyPrint.SmartWriteLine("\t\t TCP SEGMENT: DATA  ");
			
			MyPrint.SmartWriteLine("\t\t   Source_Port:" + source_port_ + "  Dest_Port:" + dest_port_);
			MyPrint.SmartWriteLine("\t\t   Send_Seq#:" + seq_num_ + "  Ack_Seq#:" + ack_num_);
			MyPrint.SmartWriteLine("\t\t   Flags: Urgent:" + printFlag(URG) + "  Ack:" + printFlag(ACK) + "  Push:" + printFlag(PUSH) + "  Reset:" + printFlag(RST) + "  Syn:" + printFlag(SYN) + "  Fin:" + printFlag(FIN));
			MyPrint.SmartWriteLine("\t\t   Data_Offset:" + (int) DataOffset + "  Window:" + window_ + "  Urgent_Ptr:" + urgent_ptr_);
			print_data(length - TCP_HDR_LEN);
		}
		
		////////////////// SEGMENT::print_data()
		/// <summary>Prints application data within a Segment </summary>
		public virtual void  print_data(int length)
		{
			MyPrint.SmartWrite("\t\t     APPL DATA:");
			if (length > 0)
				MyPrint.SmartWrite(data_);
			else
				MyPrint.SmartWrite(" None");
			MyPrint.SmartWrite("\n");
		}
		
		/////////////////////////// Driver Only!!!: Swap & Ack
		/// <summary>Off-Limits: This function sends an ack to a data segment from TCP.
		/// It operates on a Segment, but is a Driver-only function: off limits for TCP. 
		/// </summary>
		public virtual int swap_and_ack(short window, int seq_num, int ack_num)
		{
			// swap source and destination port addresses
			short temp = source_port_;
			source_port_ = dest_port_;
			dest_port_ = temp;
			
			// Send next & updated ack sequence numbers.
			seq_num_ = seq_num;
			ack_num_ = ack_num;
			
			// Set Ack flag - and SYN flag if SYN set
			if (getFlag(SYN) == SYN && getFlag(ACK) == 0)
				seq_num++;
			// clear all flags
			else
				Flags = (sbyte) 0;
			setFlag(ACK);
			DataOffset = (sbyte) 5;
			
			// Set default Window, urgent ptr. (Options are optional and not included)
			window_ = window; //  table[test_].window;
			urgent_ptr_ = 0;
			return seq_num;
		}
	}
}
