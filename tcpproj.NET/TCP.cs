using System;
using tcpproj;
namespace tcpproj
{
   
   ///////////////////////////////////////////////////////////////////
   /// <summary> TCP = Transport Control Protocol:
   /// This class is the highest level for the TCP protocol.  It
   /// fetches a primitive, finds the correct TCB, and then processes the
   /// TCB.
   /// </summary>
   ///////////////////////////////////////////////////////////////////
   public class TCP
   {
      
      // Create an array of TCBs
      private static int NR_TCBS = 10;
      private TCB[] tcb_ = null;
      
      /// <summary>This Constructor creates and initializes the TCBs </summary>
      public TCP()
      {
         // Create and initialize the TCBs
         tcb_ = new TCB[NR_TCBS];
         for (short i = 0; i < NR_TCBS; i++)
         {
            tcb_[i] = new TCB();
            tcb_[i].tcb_init(i);
         }
      }
      
      /////////////////////////////  tcp()
      /// <summary> Transport Layer Protocol Logic: Highest Level.  
      /// Dequeues primitives off the tcpinQ and appoutQ and processes them.
      /// </summary>
      public virtual void  tcp()
      {
         PRIMITIVE prim;
         
         // Dequeue from IP and process primitives
         while (DRIVER.tcpinQ.size() > 0)
         {
            prim = (PRIMITIVE) DRIVER.tcpinQ.removeFirst();
            
            // First catch any mistakes ....
            if (prim.PrimType != PRIMITIVE.PRIM_TYPE.IP_DELIVER)
            {
               MyPrint.SmartWrite("Unrecognized PRIM TYPE!\n");
                    MyPrint.SmartWriteLine("Unrecognized PRIM TYPE!");
               prim.print(PRIMITIVE.DIRECTION.L2DEBUG);
               prim = null;
               return ;
            }
            IP_PRIMITIVE iprim = (IP_PRIMITIVE) prim;
            int index = - 1;
            // Find the correct TCB based on source/dest address
            for (int i = 0; i < NR_TCBS; i++)
            {
               if (tcb_[i].SourcePort == iprim.s().dest_port_ && (tcb_[i].DestPort == iprim.s().source_port_ || tcb_[i].DestPort == 0))
                  index = i;
            }
            // If new session must find new TCB
            if (index == - 1)
               for (int i = 0; i < NR_TCBS; i++)
               {
                  if (tcb_[i].State == TCB.STATE.LISTEN_STATE)
                     index = i;
               }
            iprim.LocalConnect = index;
            if (index != - 1)
               tcb_[index].process_tcp_input(iprim);
         }
         
         // Dequeue from Application/Timer and process primitives
         while (DRIVER.appoutQ.size() > 0)
         {
            prim = (PRIMITIVE) DRIVER.appoutQ.removeFirst();
            tcb_[prim.LocalConnect].process_tcp_output(prim);
         }
      }
   }
   
}
