using System;
using tcpproj;
namespace tcpproj
{
   
   ///////////////////////////////////////////////////////////////////
   /// <summary>  TCB = Transport Layer Control Block:
   /// This is the file that students enhance to program TCP.
   /// TCB = Transport Layer Control Block: Each instance contains information 
   /// about one connection.
   /// </summary>
   ///////////////////////////////////////////////////////////////////
   
   public class TCB
   {
       ///////////////////////////// TCB FUNCTIONS
       // for format() data and length are optional parms
       virtual public int SourcePort
      {
         get { return (int) source_port_; }
      }
      virtual public int DestPort
      {
         get { return (int) dest_port_; }
      }
      virtual public STATE State
      {
         get {  return state_; }    
      }

   
      
      // Definitions
      
      // STATE Defintions
      /// <summary>A State Definition </summary>
      public enum STATE
      {
          CLOSED_STATE, LISTEN_STATE, SYN_SENT_STATE, SYN_RCVD_STATE, ESTABLISHED_STATE,
          FIN_WAIT_1_STATE, FIN_WAIT_2_STATE, CLOSE_WAIT_STATE, CLOSING_STATE, LAST_ACK_STATE,
          TIME_WAIT_STATE, NR_STATES
      };

      private static readonly System.String[] state_name = new System.String[]{"CLOSED", "LISTEN", "SYN_SENT", "SYN_RCVD", "ESTABLISHED", "FIN_WAIT_1", "FIN_WAIT_2", "CLOSE_WAIT", "CLOSING", "LAST_ACK", "TIME_WAIT"};
      
      // Private Attributes
      
      private STATE state_;
      private short local_connect_; // nick-name for connection
      private short source_port_;
      private short dest_port_;
      private ADDRESS dest_addr_ = new ADDRESS(); /// <summary>destination IP address </summary>
      private ADDRESS source_addr_ = new ADDRESS();
      ///////////////////// Send variables
      private int send_unacked_; /// <summary>send unacknowledged </summary>
      private int send_next_; /// <summary>send next (sequence number) </summary>
      private short send_window_; /// <summary>send window (you are sending) </summary>
      private int init_send_seq_; /// <summary>initial send sequence number </summary>
      ///////////////////// Receive variables
      private int rcv_next_; /// <summary>receive next </summary>
      private short rcv_window_; /// <summary>receive window (you received) </summary>
      private int init_rcv_seq_; /// <summary>inital receive sequence number </summary>
      //////////////////// Other variables
      private long rto_; /// <summary>retransmission timeout period </summary>
      private int increment_ = 1; // increment variable
      private int numberOfRetransmissions = 0;
      
      
      ///////////////////////////// Process_tcp_input()
      /// <summary>  Process TCP Input()
      /// This function processes all frames received from the
      /// remote side via IP.
      /// </summary>
      public virtual void  process_tcp_input(IP_PRIMITIVE iprim)
      {
         // Print the state first for debug purposes
         MyPrint.SmartWriteLine("\n    TCP State:" + state_name[(int)state_]);
         
         // Now process all the valid IP_DELIVERs
         SEGMENT s = iprim.remove_segment();
         
         switch (state_)
         {
            
            case STATE.CLOSED_STATE: 
               MyPrint.SmartWrite("TCP INPUT: ERROR: Unrecognized input in CLOSED STATE\n");
               break;
            
            
            case STATE.LISTEN_STATE: 
               listen_state_in(iprim, s);
               break;
            
            
            case STATE.SYN_RCVD_STATE:
               syn_rcvd(iprim, s);
                 
               // Should get an Ack in this state and transition to Established
               // Turn off the Retransmit timer
               // Send an OPEN SUCCESS to the application
               break;
            
             case STATE.SYN_SENT_STATE:
               SYN_SENT(iprim, s);
                             
               break;

             case   STATE.FIN_WAIT_1_STATE:
               fin_wait_1(iprim, s);
               break;

             case STATE.FIN_WAIT_2_STATE:
               fin_wait_2(iprim, s);
               break;

             case STATE.ESTABLISHED_STATE:
               established_in(iprim, s);              
               break;

             case STATE.LAST_ACK_STATE:
               last_ack(iprim, s);
               break;

         
            
            default: 
               MyPrint.SmartWrite("TCP Input: State Not Supported Yet!!!\n");
               break;
            
         } /* endswitch state */
         
         // Make sure memory is released
         iprim = null;
         s = null;
      } /* end process_tcp_input() */


      ///////////////////////// last_ack()
      /// <summary> This function processes input from IP while in Listen State</summary>
      private void last_ack(IP_PRIMITIVE iprim, SEGMENT s)
      {
          RETRANSMISSION_TIMER rprim;
          PRIMITIVE aprim;
          if (s.getFlag(SEGMENT.ACK) == SEGMENT.ACK)
          {


              // set new state
              state_ = STATE.CLOSED_STATE;

              // Start retransmission timer
              rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.CLEAR_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
                    
              DRIVER.send(DRIVER.timerQ, rprim);

          }
          else
              MyPrint.SmartWrite("TCP_input: LISTEN: Unknown Prim\n");

          // eliminate any pointers lying around
          rprim = null;
      }


      ///////////////////////// Synrecived()
      /// <summary> This function processes input from IP while in Listen State</summary>
      private void syn_rcvd(IP_PRIMITIVE iprim, SEGMENT s)
      {
          RETRANSMISSION_TIMER rprim;
          PRIMITIVE aprim;
          if (s.getFlag(SEGMENT.ACK) == SEGMENT.ACK)
          {
           
             
              // set new state
              state_ = STATE.ESTABLISHED_STATE;

              // Start retransmission timer
              rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.CLEAR_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
           

              //new prim
              aprim = new PRIMITIVE(local_connect_, PRIMITIVE.PRIM_TYPE.OPEN);
              aprim.Code = PRIMITIVE.STATUS.SUCCESS;

              DRIVER.send(DRIVER.appinQ, aprim);
              DRIVER.send(DRIVER.timerQ, rprim);

          }
          else
              MyPrint.SmartWrite("TCP_input: LISTEN: Unknown Prim\n");

          // eliminate any pointers lying around
          rprim = null;
      }


      ///////////////////////// ESTBLISHEDIN
      /// <summary> This function processes input from IP while in established state </summary>
      private void established_in(IP_PRIMITIVE iprim, SEGMENT s)
      {
          RETRANSMISSION_TIMER rprim;

          if (s.getFlag(SEGMENT.ACK) == SEGMENT.ACK)
          {
          
    
              // set new state
              // no change in state with this
              // Start retransmission timer
              rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.CLEAR_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
              DRIVER.send(DRIVER.timerQ, rprim);
          }


          if (s.getFlag(SEGMENT.FIN) == SEGMENT.FIN)
          {

              // save off incoming information from remote TCP's SYN
              init_rcv_seq_ = s.SeqNum;
              rcv_next_ = s.SeqNum + 1;
              rcv_window_ = s.Window;
              source_port_ = s.DestPort;
              dest_port_ = s.SourcePort;
              dest_addr_ = iprim.SourceAddr;
              

              // now format ack reply
              format(s, (sbyte)(SEGMENT.ACK));
              
              // and send ack in primitive to remote side, reusing the iprim
              iprim.format(source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, s);
              DRIVER.send(DRIVER.tcpoutQ, iprim);
              // indicate iprim is given away, to ensure no deletion
              iprim = null;         
              
              //newState
              state_ = STATE.CLOSE_WAIT_STATE;
            
              rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.CLEAR_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
              DRIVER.send(DRIVER.timerQ, rprim);
          }




          else
              MyPrint.SmartWrite("TCP_input: LISTEN: Unknown Prim\n");

          // eliminate any pointers lying around
          rprim = null;
      }

      private void fin_wait_1(IP_PRIMITIVE iprim, SEGMENT s)
      {
          RETRANSMISSION_TIMER rprim;
          PRIMITIVE aprim;
          
          if (s.getFlag(SEGMENT.ACK) == SEGMENT.ACK)
          {
              // SYN segment

              // save off incoming information from remote TCP's SYN
              init_rcv_seq_ = s.SeqNum;
              rcv_next_ = s.SeqNum + 1;
              rcv_window_ = s.Window;
              source_port_ = s.DestPort;
              dest_port_ = s.SourcePort;
              dest_addr_ = iprim.SourceAddr;

              state_ = STATE.FIN_WAIT_2_STATE;

              // Start retransmission timer


              rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.CLEAR_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
              DRIVER.send(DRIVER.timerQ, rprim);
              //DRIVER.send(DRIVER.appinQ, aprim);
          }
          else
              MyPrint.SmartWrite("TCP_input: LISTEN: Unknown Prim\n");

          // eliminate any pointers lying around
          rprim = null;
      }

      private void fin_wait_2(IP_PRIMITIVE iprim, SEGMENT s)
      {
          RETRANSMISSION_TIMER rprim;
          

          if (s.getFlag(SEGMENT.FIN) == SEGMENT.FIN)
          {

              // SYN segment

              // save off incoming information from remote TCP's SYN
              init_rcv_seq_ = s.SeqNum;
              rcv_next_ = s.SeqNum + 1;
              rcv_window_ = s.Window;
              source_port_ = s.DestPort;
              dest_port_ = s.SourcePort;
              dest_addr_ = iprim.SourceAddr;

              // now format ack reply
              format(s, (sbyte)( SEGMENT.ACK));

              // and send ack in primitive to remote side, reusing the iprim
              iprim.format(source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, s);
              DRIVER.send(DRIVER.tcpoutQ, iprim);
              // indicate iprim is given away, to ensure no deletion
              iprim = null;         
              
              // Start retransmission timer


              rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_ * 2);
              DRIVER.send(DRIVER.timerQ, rprim);
           
              // set new state
              state_ = STATE.TIME_WAIT_STATE;
            



          }
          else
              MyPrint.SmartWrite("TCP_input: LISTEN: Unknown Prim\n");

          // eliminate any pointers lying around
          rprim = null;
      }
      

      
      ///////////////////////// Listen_State_in()
      /// <summary> This function processes input from IP while in Listen State</summary>
      private void  listen_state_in(IP_PRIMITIVE iprim, SEGMENT s)
      {
         RETRANSMISSION_TIMER rprim;
         
         if (s.getFlag(SEGMENT.SYN) == SEGMENT.SYN)
         {
            // SYN segment
            
            // save off incoming information from remote TCP's SYN
            init_rcv_seq_ = s.SeqNum;
            rcv_next_ = s.SeqNum + 1;
            rcv_window_ = s.Window;
            source_port_ = s.DestPort;
            dest_port_ = s.SourcePort;
            dest_addr_ = iprim.SourceAddr;
   
            // now format ack reply
            format(s, (sbyte) (SEGMENT.SYN + SEGMENT.ACK));
            
            // and send ack in primitive to remote side, reusing the iprim
            iprim.format(source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, s);
            DRIVER.send(DRIVER.tcpoutQ, iprim);
            // indicate iprim is given away, to ensure no deletion
            iprim = null;
            
            // set new state
            state_ = STATE.SYN_RCVD_STATE;
            
            // Start retransmission timer
            rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
            DRIVER.send(DRIVER.timerQ, rprim);
         }
         else
            MyPrint.SmartWrite("TCP_input: LISTEN: Unknown Prim\n");
         
         // eliminate any pointers lying around
         rprim = null;
      }

      
       /// <summary>
       /// SYN SENT
       /// </summary>
       /// <param name="iprim"></param>
       /// <param name="s"></param>
      private void SYN_SENT(IP_PRIMITIVE iprim, SEGMENT s)
      {
          RETRANSMISSION_TIMER rprim;
          PRIMITIVE aprim;
          if (s.getFlag(SEGMENT.SYN) == SEGMENT.SYN)
          {
              // SYN segment

              // save off incoming information from remote TCP's SYN
              init_rcv_seq_ = s.SeqNum;
              rcv_next_ = s.SeqNum + 1;
              rcv_window_ = s.Window;
              source_port_ = s.DestPort;
              dest_port_ = s.SourcePort;
              dest_addr_ = iprim.SourceAddr;

              // now format ack reply
              format(s, (sbyte)(SEGMENT.SYN + SEGMENT.ACK));

              // and send ack in primitive to remote side, reusing the iprim
              iprim.format(source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, s);
              DRIVER.send(DRIVER.tcpoutQ, iprim);
              // indicate iprim is given away, to ensure no deletion
              iprim = null;

              // set new state
              state_ = STATE.ESTABLISHED_STATE;

              //new prim
              aprim = new PRIMITIVE(local_connect_, PRIMITIVE.PRIM_TYPE.OPEN_RESPONSE);
              aprim.Code = PRIMITIVE.STATUS.SUCCESS;
              

              // Start retransmission timer
          

              rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.CLEAR_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
              DRIVER.send(DRIVER.timerQ, rprim);
              DRIVER.send(DRIVER.appinQ, aprim);
          }
          else
              MyPrint.SmartWrite("TCP_input: LISTEN: Unknown Prim\n");

          // eliminate any pointers lying around
          rprim = null;
      }
      
      ///////////////////////////// Process TCP Output
      /// <summary>  Process TCP Output()
      /// This function processes all primitives from the
      /// application or timer task,
      /// which are destined for transmission to the remote side.
      /// </summary>
      public virtual void  process_tcp_output(PRIMITIVE prim)
      {
         // Print the state first for debug purposes
         MyPrint.SmartWriteLine("\n    TCP State:" + state_name[(int) state_]);
         
         switch (state_)
         {
            
            case STATE.CLOSED_STATE: 
               closed_state_out(prim);
               break;

             case STATE.ESTABLISHED_STATE:
                 established_out(prim);
             
                 break;

             case STATE.TIME_WAIT_STATE:
                 timewait2(prim);               
                 break;

             case STATE.SYN_SENT_STATE:
                 synsentout(prim);
                 break;

             case   STATE.CLOSE_WAIT_STATE:
                 close_wait(prim);
                 break;
            
            
            default:  // any other state for now goes here
               MyPrint.SmartWrite("TCP Output: State Not Supported Yet!\n");
               //prim.print(PRIMITIVE.L2DEBUG);
               break;
            
         } /* endswitch state */
         // if prim still hanging around, delete it.
         prim = null;
      } /* end process_tcp_output */


      ////////////////////////// close_wait()
      /// <summary> This processes output from the Application in Closed State</summary>
      private void close_wait(PRIMITIVE prim)
      {
          IP_PRIMITIVE iprim;
          PRIMITIVE oprim;
          RETRANSMISSION_TIMER rprim;


          switch (prim.PrimType)
          {

              case PRIMITIVE.PRIM_TYPE.CLOSE:
                  // Format the SYNCHRONIZE segment
                  SEGMENT s = new SEGMENT();
                  format(s, SEGMENT.FIN);
                  // Format the IP_SEND primitive for IP
                  iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_SEND, source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, s);
                  DRIVER.send(DRIVER.tcpoutQ, iprim);
                

                      // Start retransmission timer
                      rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
                      DRIVER.send(DRIVER.timerQ, rprim);
                  
                            
                      state_ = STATE.LAST_ACK_STATE;
                  break;

              default:
                  MyPrint.SmartWriteLine("TCP Output Closed State: Received Prim: " + prim.PrimType);

                  //prim.print(PRIMITIVE.L2DEBUG);
                  break;

          } /* endswitch primtype */
          rprim = null;
          iprim = null;
          oprim = null;
      }



      ////////////////////////// synsent()
      /// <summary> This processes output from the Application in syn sent state</summary>
      private void synsentout(PRIMITIVE prim)
      {
          IP_PRIMITIVE iprim;
          OPEN_PRIMITIVE oprim;
          RETRANSMISSION_TIMER rprim;
          PRIMITIVE aprim;

          switch (prim.PrimType)
          {

              case PRIMITIVE.PRIM_TYPE.EXPIRE_RETX_TIMER:
                 
                  SEGMENT sa = new SEGMENT();

                  if (numberOfRetransmissions >= 5)
                  {

                      iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_SEND, source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, sa);
                      aprim = new PRIMITIVE(local_connect_, PRIMITIVE.PRIM_TYPE.ABORT);
                      format(sa, SEGMENT.RST);

                      DRIVER.send(DRIVER.tcpoutQ, iprim);                     
                      DRIVER.send(DRIVER.appinQ, aprim);

                      state_ = STATE.CLOSED_STATE;
                      MyPrint.SmartWriteLine("Entering Closed State");
                 
                      break;
                  }


               
                  
                  
                  iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER, source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, sa);
                  
                  format(sa, SEGMENT.SYN);
                  
                  
                  iprim.format(source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, sa);
                  
                 // print();
                  
                  // Start retransmission timer
                  rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_ + (2 * numberOfRetransmissions));
                  
                  //sending to que
                  DRIVER.send(DRIVER.tcpoutQ, iprim);
                  DRIVER.send(DRIVER.timerQ, rprim);
                  numberOfRetransmissions++;
                  break;

              default:
                  MyPrint.SmartWriteLine("TCP Output Closed State: Received Prim: " + prim.PrimType);

                  //prim.print(PRIMITIVE.L2DEBUG);
                  break;

          } /* endswitch primtype */
          rprim = null;
          iprim = null;
          oprim = null;
      }
      
      ////////////////////////// Closed_State_Out()
      /// <summary> This processes output from the Application in Closed State</summary>
      private void  closed_state_out(PRIMITIVE prim)
      {
         IP_PRIMITIVE iprim;
         OPEN_PRIMITIVE oprim;
         RETRANSMISSION_TIMER rprim;
        
         
         switch (prim.PrimType)
         {
            
            case PRIMITIVE.PRIM_TYPE.OPEN: 
               // save off info from primitive
               oprim = (OPEN_PRIMITIVE) prim;
               source_port_ = oprim.SourcePort;
               send_window_ = (short) oprim.ByteCount;
               dest_port_ = oprim.DestPort;
               dest_addr_.set_IP_addr(oprim.DestAddr);
               if (oprim.OpenType == OPEN_PRIMITIVE.OPEN_TYPE.ACTIVE_OPEN)
               {
                  // Format the SYNCHRONIZE segment
                  SEGMENT s = new SEGMENT();
                  format(s, SEGMENT.SYN);
                  // Format the IP_SEND primitive for IP
                  iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_SEND, source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, s);
                 
                  DRIVER.send(DRIVER.tcpoutQ, iprim);
                  // Set the new state
                  state_ = STATE.SYN_SENT_STATE;
                  // Start retransmission timer
                  rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
                  DRIVER.send(DRIVER.timerQ, rprim);
               }
               // Passive open - wait for remote connection
               else
                  state_ = STATE.LISTEN_STATE;
               break;
            
            default: 
               MyPrint.SmartWriteLine("TCP Output Closed State: Received Prim: " + prim.PrimType);
             
               //prim.print(PRIMITIVE.L2DEBUG);
               break;
            
         } /* endswitch primtype */
         rprim = null;
         iprim = null;
         oprim = null;
      }

      ////////////////////////// establishedstate()
      /// <summary> This processes output from the Application in established state</summary>
      private void established_out(PRIMITIVE prim)
      {
          IP_PRIMITIVE iprim;
          PRIMITIVE oprim;
          RETRANSMISSION_TIMER rprim;
       
         

          switch (prim.PrimType)
          {

              case PRIMITIVE.PRIM_TYPE.CLOSE:
                  // save off info from primitive
   
                      // Format the SYNCHRONIZE segment
                         SEGMENT s = new SEGMENT();
                      format(s, SEGMENT.FIN);
                      // Format the IP_SEND primitive for IP
                      iprim = new IP_PRIMITIVE(PRIMITIVE.PRIM_TYPE.IP_SEND, source_port_, source_addr_, dest_port_, dest_addr_, SEGMENT.TCP_HDR_LEN, s);
                      DRIVER.send(DRIVER.tcpoutQ, iprim);
                      // Set the new state
                      state_ = STATE.FIN_WAIT_1_STATE;
                      // Start retransmission timer
                      rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
                      DRIVER.send(DRIVER.timerQ, rprim);
     
                  break;
              case PRIMITIVE.PRIM_TYPE.SEND:
                  iprim = (IP_PRIMITIVE) prim;
                  SEGMENT sa = iprim.Segment;
                  
                  
                 // MyPrint.SmartWriteLine(sa.data_.ToString());
                //  MyPrint.SmartWrite("Data offset is " +  sa.dataOffset_.ToString()); 
                  format(sa, SEGMENT.PUSH, sa.data_.ToString(), SEGMENT.TCP_HDR_LEN);
                 // MyPrint.SmartWrite("Data offset is " + sa.dataOffset_.ToString());
                  sa.seq_num_ = sa.seq_num_ + increment_;
                  increment_ += sa.dataOffset_;
                  //MyPrint.SmartWrite(sa.seq_num_.ToString());
                  iprim.format(source_port_, source_addr_, dest_port_, dest_addr_, sa.dataOffset_,sa);                                
                                       
                        		
                  DRIVER.send(DRIVER.tcpoutQ, iprim);
                  //setting retransmission timer
                  rprim = new RETRANSMISSION_TIMER(local_connect_, PRIMITIVE.PRIM_TYPE.SET_RETX_TIMER, DRIVER.gl_time(), DRIVER.gl_time() + rto_);
                  DRIVER.send(DRIVER.timerQ, rprim);
                 
                  //MyPrint.SmartWriteLine("Recived the Send prim");  


              break;


              default:
                  MyPrint.SmartWriteLine("TCP Output Closed State: Received Prim: " + prim.PrimType);                 
                  //prim.print(PRIMITIVE.L2DEBUG);
                  break;

          } /* endswitch primtype */
          rprim = null;
          iprim = null;
          oprim = null;
      }

      ///////////////////////// timewait state()
      /// <summary> This processes output from the Application in established state</summary>
      private void timewait2(PRIMITIVE prim)
      {
          IP_PRIMITIVE iprim;
          PRIMITIVE oprim;
          RETRANSMISSION_TIMER rprim;


          switch (prim.PrimType)
          {

              case PRIMITIVE.PRIM_TYPE.EXPIRE_RETX_TIMER:
                  
                  state_ = STATE.CLOSED_STATE;
                  MyPrint.SmartWriteLine("\n    TCP State:" + state_name[(int)state_]);

                  break;

              default:
                  MyPrint.SmartWriteLine("TCP Output Closed State: Received Prim: " + prim.PrimType);
                  //prim.print(PRIMITIVE.L2DEBUG);
                  break;

          } /* endswitch primtype */
          rprim = null;
          iprim = null;
          oprim = null;
      }
      
      ///////////////////////////// tcb_init
      /// <summary> TCB Init:
      /// Initializes the Transport Control Block for a new connection
      /// </summary>
      public virtual void  tcb_init(short local_connect)
      {
         state_ = STATE.CLOSED_STATE;
         source_port_ = dest_port_ = 0;
         source_addr_ = DRIVER.myIPaddress();
         dest_addr_.set_IP_addr(0);
         local_connect_ = local_connect;
         System.Random generator = new System.Random();
         send_unacked_ = send_next_ = init_send_seq_ = 0;
         send_window_ = rcv_window_ = 0;
         rcv_next_ = init_rcv_seq_ = 0;
         rto_ = 1;
         numberOfRetransmissions = 0;
      }
      
      /// <summary> Format():
      /// Formats a segment's TCP header from the Transport Control Block
      /// This method will nicely set up a SEGMENT for you.  If you are sending
      /// app. data, use the 4-parameter call.  Otherwise use the 2-parm call.
      /// </summary>
      public virtual void  format(SEGMENT s, sbyte flags)
      {
         format(s, flags, null, 0);
      }
      public virtual void  format(SEGMENT s, sbyte flags, System.String data, int length)
      {
         s.SourcePort = source_port_;
         s.DestPort = dest_port_;
         s.SeqNum = send_next_;
         s.AckNum = rcv_next_;
         s.Flags = flags;
         s.DataOffset = (sbyte) 5;
         s.Window = send_window_;
         s.UrgentPtr = 0;
         s.Data = data;
         
      }
      
      ///////////////////////// Print()
      /// <summary> Print():
      /// For debug reasons, prints a Transport Control Block
      /// You do not need to use it, but may choose to print a packet for debug reasons.
      /// </summary>
      public virtual void  print()
      {
         MyPrint.SmartWriteLine("\nTCB " + local_connect_);
         MyPrint.SmartWriteLine(" State:" + state_name[(int)state_] + "  Source Port:" + source_port_ + " Address:" + source_addr_ + "  Dest Port:" + dest_port_ + " Address:" + dest_addr_);
         MyPrint.SmartWriteLine(" SEND: Next:" + send_next_ + "  Unacked:" + send_unacked_ + "  Window:" + send_window_ + "  ISS:" + init_send_seq_);
         MyPrint.SmartWriteLine(" RECEIVE: Next:" + rcv_next_ + "  Window:" + rcv_window_ + "  IRS:" + init_rcv_seq_);
         MyPrint.SmartWriteLine("Number of retransmissions: " + numberOfRetransmissions);
      }
   }
   
}
