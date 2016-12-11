/// <summary>  DRIVER.java (TCP DRIVER):
/// This is a test driver, including an Application and IP stub program to test 
/// a TCP implementation.
/// </summary>
using System;
using System.Windows.Forms;
namespace tcpproj
{
   
   
   /// <summary>******** Test control variables *****************************</summary>
   /// <summary>A TEST object defines a test to be run. </summary>
   class TEST
   {
      // Definitions
      
      // Table values
      public PRIMITIVE.PRIM_TYPE connect_type_; // Type of primitive to initiate connection
      public long connect_start_; // Time to start connection 
      public PRIMITIVE.PRIM_TYPE app_data_type_; // Type of app prim during connection
      public long app_data_start_; // Start time to start sending L3 data
      public int app_data_interval_; // Duration between sending L3 data
      public int push_flag_; // Push Flag
      public long ip_data_start_; // Start time to start sending L1 data
      public int ip_data_interval_; // Duration between sending L1 data
      public double error_; // error rate for layer 1 data
      public int ip_reply_; // How many frames should IP reply to?
      public long disc_start_; // Time to start disconnect 
      public PRIMITIVE.PRIM_TYPE disc_type_; // Type of disconnect 
      public short source_; // source port to use this test
      public short dest_; // dest port to use this test
      public short window_; // window advertisement
      public int other_test_; // Other test # to run in combination
      public short connect_id_; // connect_id to use for connection
      
      // Public Functions
      internal TEST(PRIMITIVE.PRIM_TYPE c_type, int c_start, PRIMITIVE.PRIM_TYPE a_d_type, int a_d_start, 
          int a_d_interval, int push_flag, int i_d_start, int i_d_interval, double er, int i_rep, 
          int d_start, PRIMITIVE.PRIM_TYPE d_type, int src, int dst, int wndw, int o_test, int id)
      {
         connect_type_ = c_type;
         connect_start_ = (long) c_start;
         app_data_type_ = a_d_type;
         app_data_start_ = (long) a_d_start;
         app_data_interval_ = a_d_interval;
         push_flag_ = push_flag;
         ip_data_start_ = (long) i_d_start;
         ip_data_interval_ = i_d_interval;
         error_ = er;
         ip_reply_ = i_rep;
         disc_start_ = (long) d_start;
         disc_type_ = d_type;
         source_ = (short) src;
         dest_ = (short) dst;
         window_ = (short) wndw;
         other_test_ = o_test;
         connect_id_ = (short) id;
      }
   }
   
   ////////////////////// TIMER TASK ////////////////////////////////
   /// <summary>  The Timer Class:
   /// The Timer task can process one timer at a time in its current version
   /// (This is all that is necessary for the project.)
   /// 
   /// </summary>
   class TIMER
   {
      // Private Static Attributes
      private static long set_time_;
      private static long expire_time_;
      private static long expire_force_;
      private static int id_;
      // Public Functions
      public TIMER()
      {
         set_time_ = expire_time_ = expire_force_ = - 1; id_ = 0;
      }
      
      ////////////////////////// TIMER TASK
      /// <summary>This timer task supports one timer at a time! 
      /// It sends an EXPIRE_RETX_TIMER at the appropriate time
      /// if it receives a SET_RETX_TIMER.  
      /// </summary>
      public virtual void  timer()
      {
         PRIMITIVE prim;
         RETRANSMISSION_TIMER rprim;
         
         // Process all incoming primitives from TCP
         while (DRIVER.timerQ.size() > 0)
         {
            prim = (PRIMITIVE) DRIVER.timerQ.removeFirst();
            prim.print(PRIMITIVE.DIRECTION.TCP2TIMER);
            rprim = (RETRANSMISSION_TIMER) prim;
            switch (prim.PrimType)
            {
               
               case PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER: 
                  // request to set timer
                  if (expire_time_ != - 1)
                     MyPrint.SmartWriteLine("TIMER ERROR: Overriding previous timer!!");
                  set_time_ = rprim.SetTime;
                  expire_time_ = rprim.ExpireTime;
                  id_ = rprim.LocalConnect;
                  break;
               
               case PRIMITIVE.PRIM_TYPE.CLEAR_RETX_TIMER: 
                  // request to clear timer
                  expire_time_ = - 1;
                  break;
               
               case PRIMITIVE.PRIM_TYPE.SET_FORCE_TIMER: 
                  // request to set timer
                  if (expire_force_ != - 1)
                     MyPrint.SmartWriteLine("TIMER ERROR: Overriding previous timer!!");
                  set_time_ = rprim.SetTime;
                  expire_force_ = rprim.ExpireTime;
                  id_ = rprim.LocalConnect;
                  break;
               
               case PRIMITIVE.PRIM_TYPE.CLEAR_FORCE_TIMER: 
                  // request to clear timer
                  expire_force_ = - 1;
                  break;
               
               default: 
                  MyPrint.SmartWriteLine("TIMER ERROR: Invalid Prim!");
                  break;
               
            } // endswitch
            prim = null;
         }
         
         // Now check to see if the retx timer has expired...
         if (expire_time_ == DRIVER.gl_time())
         {
            // Timer has expired... send primitive to TCP
            rprim = new RETRANSMISSION_TIMER(id_, PRIMITIVE.PRIM_TYPE.EXPIRE_RETX_TIMER, set_time_, expire_time_);
            rprim.print(PRIMITIVE.DIRECTION.TIMER2TCP);
            DRIVER.send(DRIVER.appoutQ, rprim);
            expire_time_ = - 1;
            rprim = null;
         }
         
         // Now check to see if the force timer has expired...
         if (expire_force_ == DRIVER.gl_time())
         {
            // Timer has expired... send primitive to TCP
            rprim = new RETRANSMISSION_TIMER(id_, PRIMITIVE.PRIM_TYPE.EXPIRE_FORCE_TIMER, set_time_, expire_force_);
            rprim.print(PRIMITIVE.DIRECTION.TIMER2TCP);
            DRIVER.send(DRIVER.appoutQ, rprim);
            expire_force_ = - 1;
            rprim = null;
         }
      }
   }
   
   //////////////////////////////////////////////////////////////////////
   // DRIVER CLASS
   /////////////////////////////////////////////////////////////////////
   /// <summary>  DRIVER (TCP DRIVER):
   /// This is a test driver, including an Application and IP stub program used  
   /// to test a TCP implementation.
   /// Students shall NOT modify the Driver!   
   /// </summary>
   public class DRIVER
   {
      /*private void  InitBlock()
      {
         queue.addLast(prim);
         more_to_process_ = true;
         if (prim_ != null)
            MyPrint.SmartWrite("DRIVER: ERROR: Delay overlaid!/n");
         prim_ = prim;
         queue_ = queue;
         direction_ = direction;
      } */
      // Public Static Attributes of the Driver Program
      //
      /// <summary>Number of tests that students must run this semester </summary>
      public const int NR_TESTS = 5;
      public static int MyDebug = 0; // Debug is turned off
      
      /// <summary>The Queue that TCP reads from in order to obtain prims from Application </summary>
           public static LinkedList appoutQ = new LinkedList();
      
      /// <summary>The Queue that TCP reads from to obtain prims from IP </summary>
      public static LinkedList tcpinQ = new LinkedList();
      
      /// <summary>The Queue that TCP writes to to send prims to IP </summary>
      public static LinkedList tcpoutQ = new LinkedList();
      
      /// <summary>The Queue that TCP writes to to send prims to Application </summary>
      public static LinkedList appinQ = new LinkedList();
      
      /// <summary>The Queue that TCP writes to start/cancel a timer </summary>
      public static LinkedList timerQ = new LinkedList();
      
      // Private Attributes of the Driver Program
      /// <summary>The IP address of this (source) node </summary>
      private static readonly ADDRESS MY_IP_ADDR = new ADDRESS(16843009); // 1+256+65536+16777216
      
      /// <summary>The IP address of destination node </summary>
      private static readonly ADDRESS OTHER_IP_ADDR = new ADDRESS(33686018); // 2+512+131072+33554432
      
      // Information used by the Driver
      private char app_send_end_ = 'A';
      private System.String app_send_ = "Local sending packet ";
      private int app_send_len_ = 22;
      private char ip_send_end_ = 'a';
      private System.String ip_send_ = "Remote sending packet ";
      private int ip_send_len_ = 23;
      private static long gl_time_; // Current Time
      
      // Attribute defining TCP
      private TCP tcp_ = new TCP();
      // Attributes used to generate segments:
      private int seq_num_; // Remote side Send Sequence #
      private int ack_num_; // Remote side Ack Sequence #
      private int id_; // local_connection id
      // Attributes used by driver, timer and scheduler:
      private int test_; // Test number
      private TIMER timer_ = new TIMER(); // Timer Task
      private static bool more_to_process_; // If false, scheduler increments time
      // Delayed Primitive to send from Driver()
      private PRIMITIVE prim_ = null; // Primitive to send
      private LinkedList queue_ = new LinkedList();
      // Queue to put primitive onto
      private PRIMITIVE.DIRECTION direction_; // Direction for Print
      private System.Random generator_;
      
      // Public and Private Functions
      /// <summary>Returns the current time </summary>
      public static long gl_time()
      {
         return gl_time_;
      }
      
      /// <summary>Returns the IP address of this TCP </summary>
      public static ADDRESS myIPaddress()
      {
         return MY_IP_ADDR;
      }
      
      /////////////////////////// Test table Definition
      private static TEST[] table_;
      
      ////////////////////////////// DRIVER Constructor
      /// <summary>The Driver generates packets for TCP coming from the Application </summary>
      public DRIVER()
      {
         //InitBlock();
         generator_ = new System.Random();
         seq_num_ = 0;     // Force sequence number to zero
         if (seq_num_ < 0)
            seq_num_ = - seq_num_;
         ack_num_ = 0;
         id_ = 0;
         prim_ = null;
         direction_ = 0; // UNDEFINED
         table_ = new TEST[NR_TESTS];
/* connect             connect ap_data              ap_data  data  Psh IP_data data  IP   IP     Disc   Disc               Port    Win Other Cnct */
/* type                 start   type                 strt   intvl  flg start  intvl  err reply   Start  Type              SRC DEST dow Test  ID*/
table_[0] = new TEST( 
 PRIMITIVE.PRIM_TYPE.OPEN,  1, PRIMITIVE.PRIM_TYPE.RECEIVE, 40, 40,   0,  40,     9,   .0, 100,     8, PRIMITIVE.PRIM_TYPE.CLOSE, 33, 46, 100, 0, 1);
table_[1] = new TEST(
 PRIMITIVE.PRIM_TYPE.OPEN,  1, PRIMITIVE.PRIM_TYPE.SEND,   5,    4,   1,   0,     0,   .0, 100,    30, PRIMITIVE.PRIM_TYPE.CLOSE, 33, 46, 100, 0, 1);
table_[2] = new TEST(   
 PRIMITIVE.PRIM_TYPE.OPEN,  0, PRIMITIVE.PRIM_TYPE.SEND,   5,  100,   1,   0,     0,   .0,   0,   100, PRIMITIVE.PRIM_TYPE.CLOSE, 33, 46, 100, 0, 1);
table_[3] = new TEST(
PRIMITIVE.PRIM_TYPE.IP_DELIVER, 1, PRIMITIVE.PRIM_TYPE.SEND, 5,  4,   1,   0,     0,   .0, 100,    30, PRIMITIVE.PRIM_TYPE.IP_DELIVER, 12, 24, 18, 0, 1);
table_[4] = new TEST(
PRIMITIVE.PRIM_TYPE.IP_DELIVER, 1, PRIMITIVE.PRIM_TYPE.SEND, 6,  5,   1,   0,     0,   .0, 100,    30, PRIMITIVE.PRIM_TYPE.IP_DELIVER, 12, 24, 18, 1, 1);
}
      
      /////////////////////////// DRIVER SCHEDULER 
      /// <summary>   Driver Scheduler:
      /// This mini-exec calls the Driver, then Application, then TCP, then IP then
      /// back again in a simplified round-robin algorithm
      /// </summary>
      
      public virtual void  scheduler(int test, long duration)
      {
         /* Initialize variables */
         test_ = test;
         
         /* Runs the actual test */
         for (gl_time_ = 0; gl_time_ < duration; gl_time_++)
         {
            MyPrint.SmartWriteLine("Time = " + gl_time_);
            
            // generate any primitives for this time
            driver(test);
            // Now run the other test
            if (table_[test].other_test_ != 0)
               driver(table_[test].other_test_);
            
            do 
            {
               // A "round robin" sort-of scheduler!
               more_to_process_ = false;
               tcp_.tcp(); // student's code
               timer_.timer(); // generate any timer expirations
               application(); // process the application
               ip(); // process IP
            }
            while (more_to_process_);
         }
            MyPrint.SmartWriteLine("Simulation Done");
      }
      
      ////////////////////////////// SEND 
      /// <summary>  Send:
      /// This function sends a Primitive to the requested destination using a Queue.
      /// </summary>
      public static void send(LinkedList queue, PRIMITIVE prim)
        {
            queue.addLast(prim);
            more_to_process_ = true;
        }      
      ////////////////////////////// APPLICATION
      /// <summary> Application:
      /// This task prints primitives received from TCP.
      /// </summary>
      private void  application()
      {
         PRIMITIVE prim;
         
         if (MyDebug == 2)
            MyPrint.SmartWriteLine("Application");
         
         /* Print any received frames */
         while (DRIVER.appinQ.size() > 0)
         {
            prim = (PRIMITIVE) DRIVER.appinQ.removeFirst();
            prim.print(PRIMITIVE.DIRECTION.TCP2APP);
            
            // save off local connect ID 
            if (prim.PrimType == PRIMITIVE.PRIM_TYPE.OPEN)
               id_ = prim.LocalConnect;
            
            /* Deallocate prim */
            prim = null;
         }
      }
      
      ////////////////////////////////////////// Driver
      /// <summary>Driver:
      /// This test driver generates primitives for TCP according to the test schedule.
      /// </summary>
      private void  driver(int test)
      {
         PRIMITIVE prim;
         OPEN_PRIMITIVE oprim;
         IP_PRIMITIVE iprim;
         SEGMENT segment;
         int length;
         
         if (MyDebug == 2)
            MyPrint.SmartWrite("Driver\n");
         
         /* If something needed to be delayed a clock tick, send it now */
         if (prim_ != null)
         {
            if (prim_.PrimType == PRIMITIVE.PRIM_TYPE.IP_DELIVER)
            {
               // Update Ack#
               iprim = (IP_PRIMITIVE) prim_;
               iprim.s().ack_num_ = ack_num_;
            }
            prim_.print(direction_);
            send(queue_, prim_);
            prim_ = null;
         }
         
         /* Sends the Connect when time = connect_start_ */
         if (gl_time_ == table_[test].connect_start_)
         {
            switch (table_[test].connect_type_)
            {
               
               
               case PRIMITIVE.PRIM_TYPE.OPEN: 
                  // Create an Active open and send to TCP
                  oprim = new OPEN_PRIMITIVE(table_[test].connect_id_, table_[test].source_, table_[test].dest_, DRIVER.OTHER_IP_ADDR, table_[test].window_, OPEN_PRIMITIVE.OPEN_TYPE.ACTIVE_OPEN);
                  oprim.print(PRIMITIVE.DIRECTION.APP2TCP);
                  send(DRIVER.appoutQ, oprim);
                  break;
               
               
               case PRIMITIVE.PRIM_TYPE.IP_DELIVER: 
                  // Send Passive Open now to allow connection establishment
                  ADDRESS null_addr = new ADDRESS(0);
                  oprim = new OPEN_PRIMITIVE(table_[test].connect_id_, table_[test].source_, (short) 0, null_addr, table_[test].window_, OPEN_PRIMITIVE.OPEN_TYPE.PASSIVE_OPEN);
                  oprim.print(PRIMITIVE.DIRECTION.APP2TCP);
                  send(DRIVER.appoutQ, oprim);
                  
                  // Generate IP SYNC from Remote Side to send next clock tick
                  segment = format(null, SEGMENT.SYN, null, test);
                  iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_DELIVER, (short) table_[test].source_, DRIVER.OTHER_IP_ADDR, (short) table_[test].dest_, DRIVER.MY_IP_ADDR, SEGMENT.TCP_HDR_LEN, segment);
                  delay((PRIMITIVE) iprim, DRIVER.tcpinQ, PRIMITIVE.DIRECTION.IP2TCP);
                  // Increment sequence number to use next
                  seq_num_++;
                  break;
               
               case PRIMITIVE.PRIM_TYPE.ABORT: 
                  // Send Passive Open now to allow connection establishment
                  null_addr = new ADDRESS(0);
                  oprim = new OPEN_PRIMITIVE(table_[test].connect_id_, table_[test].source_, (short) 0, null_addr, table_[test].window_, OPEN_PRIMITIVE.OPEN_TYPE.PASSIVE_OPEN);
                  oprim.print(PRIMITIVE.DIRECTION.APP2TCP);
                  send(DRIVER.appoutQ, oprim);
                  
                  // Generate IP SYNC from Remote Side to send next clock tick
                  segment = format(null, (sbyte) (SEGMENT.SYN+SEGMENT.ACK), null, test);
                  iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_DELIVER, (short) table_[test].source_, DRIVER.OTHER_IP_ADDR, (short) table_[test].dest_, DRIVER.MY_IP_ADDR, SEGMENT.TCP_HDR_LEN, segment);
                  delay((PRIMITIVE) iprim, DRIVER.tcpinQ, PRIMITIVE.DIRECTION.IP2TCP);
                  // Increment sequence number to use next
                  seq_num_++;
                  break;
               
               default: 
                  System.Console.Error.WriteLine("Driver: Unknown connect prim request");
                  break;
               
            }
            return ; /* Don't generate more than one frame per time */
         }
         
         /* Send any disconnect out when time = disconnect_start*/
         if (gl_time_ == table_[test].disc_start_)
         {
            /* Turn off any data frames */
            table_[test].app_data_start_ = - 1;
            switch (table_[test].disc_type_)
            {
               
               case PRIMITIVE.PRIM_TYPE.CLOSE: 
                  // Send CLOSE now
                  prim = new PRIMITIVE(table_[test].connect_id_, PRIMITIVE.PRIM_TYPE.CLOSE);
                  prim.print(PRIMITIVE.DIRECTION.APP2TCP);
                  send(DRIVER.appoutQ, prim);
                  
                  // Send FIN next clock tick
                  segment = format(null, SEGMENT.FIN, test);
                  iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_DELIVER, (short) 0, DRIVER.OTHER_IP_ADDR, (short) 0, DRIVER.MY_IP_ADDR, SEGMENT.TCP_HDR_LEN, segment);
                  delay((PRIMITIVE) iprim, DRIVER.tcpinQ, PRIMITIVE.DIRECTION.IP2TCP);
                  // sequence number can never get updated ...
                  break;
               
               case PRIMITIVE.PRIM_TYPE.ABORT: 
                  // Send ABORT now
                  prim = new PRIMITIVE(table_[test].connect_id_, PRIMITIVE.PRIM_TYPE.ABORT);
                  prim.print(PRIMITIVE.DIRECTION.APP2TCP);
                  send(DRIVER.appoutQ, prim);
                  break;
               
               case PRIMITIVE.PRIM_TYPE.IP_DELIVER: 
                  // Format FIN to send to local TCP now
                  segment = format(null, SEGMENT.FIN, test);
                  iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_DELIVER, (short) 0, DRIVER.OTHER_IP_ADDR, (short) 0, DRIVER.MY_IP_ADDR, SEGMENT.TCP_HDR_LEN, segment);
                  iprim.print(PRIMITIVE.DIRECTION.IP2TCP);
                  send(DRIVER.tcpinQ, iprim);
                  // Increment sequence number to use next
                  seq_num_++;
                  
                  // Send CLOSE next clock tick
                  prim = new PRIMITIVE(table_[test].connect_id_, PRIMITIVE.PRIM_TYPE.CLOSE);
                  delay(prim, DRIVER.appoutQ, PRIMITIVE.DIRECTION.APP2TCP);
                  break;
               
               default: 
                  System.Console.Error.WriteLine("Driver: Unknown disconnect prim request");
                  break;
               
            }
         }
         
         /* If it is time, send another application data frame */
         if ((table_[test].app_data_type_ != 0) && (gl_time_ >= table_[test].app_data_start_) && (gl_time_ < table_[test].disc_start_) && ((gl_time_ - table_[test].app_data_start_) % table_[test].app_data_interval_) == 0)
         {
            /* Allocate a packet */
            switch (table_[test].app_data_type_)
            {
               
               case PRIMITIVE.PRIM_TYPE.SEND: 
                  // Create a SEND Primitive
                  segment = new SEGMENT(app_send_ + app_send_end_);
                  length = app_send_len_;
                  // increment the packet number in the data
                  app_send_end_++;
                  iprim = new IP_PRIMITIVE(table_[test].connect_id_, PRIMITIVE.PRIM_TYPE.SEND, length, segment, table_[test].push_flag_);
                  iprim.print(PRIMITIVE.DIRECTION.APP2TCP);
                  send(DRIVER.appoutQ, iprim);
                  break;
               
               case PRIMITIVE.PRIM_TYPE.RECEIVE: 
                  // Create a RECEIVE Primitive
                  prim = new PRIMITIVE(table_[test].connect_id_, PRIMITIVE.PRIM_TYPE.RECEIVE, table_[test].window_);
                  prim.print(PRIMITIVE.DIRECTION.APP2TCP);
                  send(DRIVER.appoutQ, prim);
                  break;
               
               default: 
                  MyPrint.SmartWrite("Driver: ERROR: Unknown primitive to queue!\n");
                  System.Environment.Exit(200);
                  break;
               
            }
         }
         
         /* If it is time, send another IP data frame */
         if ((table_[test].ip_data_interval_ != 0) && (gl_time_ >= table_[test].ip_data_start_) && (gl_time_ < table_[test].disc_start_) && ((gl_time_ - table_[test].ip_data_start_) % table_[test].ip_data_interval_) == 0)
         {
            
            // Generate a probability of missing segment, to simulate an out-of-order segment
            double percenterror = table_[test].error_;
            double rand = generator_.NextDouble();
            if (rand >= percenterror)
            {
               // no error occurred - packet is in order
               segment = format(null, SEGMENT.ACK, ip_send_ + ip_send_end_, test);
               ip_send_end_++;
               length = SEGMENT.TCP_HDR_LEN + ip_send_len_;
               // Increment sequence number to use next
               seq_num_ += ip_send_len_;
            }
            else
            {
               // error occurred - packet is out of order
               char ip_error_end = (char) (ip_send_end_ + 1);
               segment = format(null, SEGMENT.ACK, ip_send_ + ip_error_end, test);
               length = SEGMENT.TCP_HDR_LEN + ip_send_len_;
               segment.seq_num_ = segment.seq_num_ + ip_send_len_;
            }
            
            // Now deliver segment to TCP from IP
            iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_DELIVER, (short) 0, DRIVER.OTHER_IP_ADDR, (short) 0, DRIVER.MY_IP_ADDR, length, segment);
            iprim.print(PRIMITIVE.DIRECTION.IP2TCP);
            send(DRIVER.tcpinQ, iprim);
         }
      }
      
      /////////////////////////////// Delay
      /// <summary> Some TCP operations require Primitives from both Application and IP:
      /// Save a prim to send out in next clock tick   
      /// </summary>
      //UPGRADE_ISSUE: The following fragment of code could not be parsed and was not converted. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1156'"
        private void delay(PRIMITIVE prim, LinkedList queue, PRIMITIVE.DIRECTION direction)
        {
           if (prim_!=null) MyPrint.SmartWrite( "DRIVER: ERROR: Delay overlaid!/n");
           prim_ = prim;
           queue_ = queue;
           direction_ = direction;
        }
      
      //////////////////////////////// STUB
      /// <summary> IP Stub:
      /// Prints any primitives received from TCP.  Sends back responses when appropriate
      /// </summary>
      private void  ip()
      {
         PRIMITIVE prim;
         IP_PRIMITIVE iprim;
         SEGMENT s;
         
         if (MyDebug == 2)
            MyPrint.SmartWrite("Internet Protocol\n");
         
         while (DRIVER.tcpoutQ.size() > 0)
         {
            prim = (PRIMITIVE) DRIVER.tcpoutQ.removeFirst();
            prim.print(PRIMITIVE.DIRECTION.TCP2IP);
            iprim = (IP_PRIMITIVE) prim;
            switch (prim.PrimType)
            {
               
               case PRIMITIVE.PRIM_TYPE.IP_SEND: 
                  // IP_SENDs from TCP should always be at least TCP_HDR_LEN size large!!!
                  s = iprim.remove_segment();
                  if (iprim.byte_count() < SEGMENT.TCP_HDR_LEN)
                     MyPrint.SmartWrite("IP: ERROR: Where is TCP Header???/n");
                  ack_num_ = s.seq_num_ + iprim.byte_count() - SEGMENT.TCP_HDR_LEN;
                  if (s.getFlag(SEGMENT.FIN) == SEGMENT.FIN || s.getFlag(SEGMENT.SYN) == SEGMENT.SYN)
                  {
                     ack_num_++;
                  }
                  if (s.getFlag(SEGMENT.FIN) == SEGMENT.FIN || s.getFlag(SEGMENT.SYN) == SEGMENT.SYN || iprim.byte_count() > SEGMENT.TCP_HDR_LEN)
                  {
                     // Needs a reply - decrement # replies
                     if (--table_[test_].ip_reply_ > 0)
                     {
                                seq_num_ = s.swap_and_ack(table_[test_].window_, seq_num_, ack_num_);
                        iprim.clear();
                        iprim.DestPort = 0;
                        iprim.DestAddr = DRIVER.MY_IP_ADDR;
                        iprim.SourcePort = 0;
                        iprim.SourceAddr = DRIVER.OTHER_IP_ADDR;
                        iprim.PrimType=PRIMITIVE.PRIM_TYPE.IP_DELIVER;
                        iprim.Segment = s;
                        iprim.set_byte_count(SEGMENT.TCP_HDR_LEN);
                        iprim.print(PRIMITIVE.DIRECTION.IP2TCP);
                        send(DRIVER.tcpinQ, iprim);
                        prim = iprim = null;
                     }
                  }
                  break;
               
               default: 
                  MyPrint.SmartWrite("IP: Illegal Primitive Received!/n");
                  break;
               
            } /* endswitch prim type */
            prim = null;
         }
      } /* end ip() */
      
      /////////////////////////////// Driver Format()
      /// <summary> Formats a Segment to be received by TCP.
      /// This format() is to be used by the driver NOT TCP.
      /// </summary>
      
      private SEGMENT format(SEGMENT s, sbyte flags, int test)
      {
         return format(s, flags, null, test);
      }
      
      private SEGMENT format(SEGMENT s, sbyte flags, System.String data, int test)
      {
         // If a segment is not already allocated, allocate one
         if (s == null)
            s = new SEGMENT();
         // The segment is coming from the remote side
         s.source_port_ = table_[test].dest_;
         s.dest_port_ = table_[test].source_;
         // Seq# and Ack# are stored in the driver
         s.seq_num_ = seq_num_;
         s.ack_num_ = ack_num_;
         // Default parameters for window, urgent_ptr, options
         s.window_ = table_[test].window_;
         s.urgent_ptr_ = 0;
         // Passed parameters for flags, data, length.
         s.Flags = flags;
         s.DataOffset = (sbyte) 5;
         if (data == null || data.Length == 0)
            return s;
         s.data_ = data;
         return s;
      }
      
      
      ////////////////////////////////////////////////////////////////////
      /// <summary> The Main() program gets test parameters and begins to run test.</summary>
      [STAThread]
      public static void  Main(System.String[] args)
      {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            MyPrint.SmartWriteLine("Welcome to the TCP Debugger"); 
            int test = 0;
         int count = 35;
         System.IO.StreamReader cin = new System.IO.StreamReader(new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(System.Console.OpenStandardInput(), System.Text.Encoding.Default).CurrentEncoding);
         
         // Get the parameters for the test
         System.Console.Error.Write("Welcome to TCP debugger\n");
         System.Console.Error.Write("Enter Test Number:");
         try
         {
            System.String input = cin.ReadLine();
            test = System.Int32.Parse(input);
            string asciitext = "testout" + (test+1).ToString() + ".txt";
            MyPrint.SmartOpen(asciitext);
            System.Console.Error.WriteLine(asciitext);
            System.Console.Error.Write("Enter Length of Time to Run:");
            input = cin.ReadLine();
            count = System.Int32.Parse(input);
                StartTest(test, count);
         }
         catch (System.IO.IOException e)
         {
            System.Console.Error.WriteLine("Problem reading string" + e);
         }
      }

        public static void StartTest(int testNr, int duration)
        {
            DRIVER mini_os = new DRIVER();
            MyPrint.SmartOpen("testout" + testNr + ".txt");
            MyPrint.SmartWriteLine("Starting the TCP Debugger");
            if (testNr > DRIVER.NR_TESTS || testNr < 1)
            {
                testNr = 1;
                MyPrint.SmartWrite("Test Number forced to test 1\n");
            }

            // Runs the actual test 
            MyPrint.SmartWriteLine("Will run test " + testNr + " for length of time " + duration); 
            mini_os.scheduler(testNr - 1, duration);

            MyPrint.SmartWriteLine("Test completed");
            MyPrint.SmartClose();
        }
   }
}
