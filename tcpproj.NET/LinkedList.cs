using System;
namespace tcpproj
{
    /// LinkedListElement
    /// Generic FIFO Linked List
    /// 
    public class LinkedListElement
    {
        public LinkedListElement(PRIMITIVE prim)
        {
            data = prim;
        }
        public LinkedListElement(PRIMITIVE prim, LinkedListElement end)
        {
            data = prim;
            next = null;
            end.Next = this;
            this.previous = end;
        }
        public LinkedListElement Next
        {
            get { return next; }
            set { next = value; }
        }
        public LinkedListElement Previous
        {
            get { return previous; }
            set { previous = value; }
        }
        public PRIMITIVE Data
        {
            get { return data; }
            set { data = value; }
        }
        private LinkedListElement next, previous;
        private PRIMITIVE data;
    }
    public class LinkedList
    {
        public LinkedList()
        {
            head = null;
            this.length = 0;
        }
        public LinkedList(PRIMITIVE val)
        {
            head = new LinkedListElement(val);
            head.Next = head.Previous = null;
            this.length = 1;
        }
        public int size()
        {
            if (head == null) return 0;
            else return this.length;
        }
        public void addLast(PRIMITIVE val)
        {
            if (size() == 0)
            {
                head = new LinkedListElement(val);
                head.Next = head.Previous = null;
                ++length;
            }
            else
            {
                LinkedListElement end = head;
                while (end.Next != null)
                {
                    end = end.Next;
                }
                new LinkedListElement(val, end);
                ++length;
            }
        }

        public PRIMITIVE removeFirst()
        {
            if (size() == 0)
                throw new System.Exception("Empty LinkedList");
            PRIMITIVE p = head.Data;
            head = head.Next;
            if (head != null)
            {
                head.Previous = null;
            }
            --this.length;
            return p;
        }

        public LinkedListElement Head
        {
            get { return head; }
        }

        public void Clear()
        {
            head = null;
            length = 0;
        }
        public LinkedListElement Find(PRIMITIVE val)
        {
            LinkedListElement h = head;
            while (h != null)
            {
                if (val.Equals(h.Data)) break;
                h = h.Next;
            }
            return h;
        }
        public LinkedListElement Delete(LinkedListElement h)
        {
            if (h == null) return null;

            if (length == 1)
            {
                LinkedListElement headElement = head;
                head = null;
                --length;
                return headElement;
            }

            LinkedListElement np = h.Next, pp = h.Previous;
            if (np != null)
                np.Previous = pp;
            if (pp != null)
            {
                pp.Next = np;
            }
            else
            {
                head = h.Next;
            }
            --length;
            return h;
        }
        public LinkedListElement Delete(PRIMITIVE p)
        {
            return Delete(Find(p));
        }

        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        public override string ToString()
        {
            return ToStringRecursive(head);
        }
        static string ToStringRecursive(LinkedListElement first)
        {
            if (first == null) return "";
            else return (first.Data + "\n") + ToStringRecursive(first.Next);
        }
        private LinkedListElement head;
        private int length;

    }
}
