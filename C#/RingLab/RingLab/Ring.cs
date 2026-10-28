using System;
using System.Text;

namespace RingLab
{
    public class Ring
    {
        private class Node
        {
            public int data;
            public Node next;
            public Node prev;

            public Node(int val)
            {
                data = val;
            }
        }

        private Node current;
        private int count;

        public Ring()
        {
            Clear();
        }

        public Ring(int[] arr) : this()
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));

            for (int i = 0; i < arr.Length; i++)
                Write(arr[i]);
        }

        public Ring(Ring other) : this()
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            // копіюємо значення в нові вузли
            Node temp = other.current;
            for (int i = 0; i < other.count; i++)
            {
                Write(temp.data);
                temp = temp.next;
            }
        }

        public int Size
        {
            get { return count; }
        }

        public int Read()
        {
            if (current == null)
                throw new InvalidOperationException("The ring is empty.");

            return current.data;
        }

        public int ReadAndRemove()
        {
            int val = Read();

            if (count == 1)
            {
                Clear();
            }
            else
            {
                // з'єднуємо сусідні вузли без поточного
                current.prev.next = current.next;
                current.next.prev = current.prev;
                current = current.next;
                count--;
            }

            return val;
        }

        public void Write(int val)
        {
            Node newNode = new Node(val);

            if (current == null)
            {
                current = newNode;
                current.next = current;
                current.prev = current;
            }
            else
            {
                // вставляємо новий вузол перед поточним
                Node tail = current.prev;
                newNode.prev = tail;
                newNode.next = current;
                tail.next = newNode;
                current.prev = newNode;
            }

            count++;
        }

        public void MoveClockwise()
        {
            if (current != null)
                current = current.next;
        }

        public void MoveCounterclockwise()
        {
            if (current != null)
                current = current.prev;
        }

        public bool StrongEquals(Ring other)
        {
            if (other == null || count != other.count)
                return false;

            Node first = current;
            Node second = other.current;

            for (int i = 0; i < count; i++)
            {
                if (first.data != second.data)
                    return false;

                first = first.next;
                second = second.next;
            }

            return true;
        }

        public bool WeakEquals(Ring other)
        {
            if (other == null || count != other.count)
                return false;

            if (count == 0)
                return true;

            // пробуємо кожне початкове положення другого кільця
            Node start = other.current;
            for (int i = 0; i < count; i++)
            {
                Node first = current;
                Node second = start;
                int matched = 0;

                while (matched < count && first.data == second.data)
                {
                    matched++;
                    first = first.next;
                    second = second.next;
                }

                if (matched == count)
                    return true;

                start = start.next;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            Ring other = obj as Ring;
            return StrongEquals(other);
        }

        public override int GetHashCode()
        {
            // переповнення під час обчислення хешу дозволене
            unchecked
            {
                int hash = 17;
                Node temp = current;

                for (int i = 0; i < count; i++)
                {
                    hash = hash * 31 + temp.data;
                    temp = temp.next;
                }

                return hash;
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("[");
            Node temp = current;

            for (int i = 0; i < count; i++)
            {
                if (i > 0)
                    result.Append(", ");

                result.Append(temp.data);
                temp = temp.next;
            }

            result.Append("]");
            return result.ToString();
        }

        private void Clear()
        {
            current = null;
            count = 0;
        }
    }
}
