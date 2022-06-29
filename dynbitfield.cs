using System.Collections.Generic;
using System.Linq;

namespace ndot
{
    public struct dynbitfield
    {
        public List<byte> bytes;

        public uint Length { get => (uint)bytes.Count; /*set => bytes = new byte[value >= 0 ? value : 0];*/ }
        public uint Size { get => (Length) * 8; /*set => bytes = new byte[value / 8 >= 0 ? value / 8 : 0];*/ }


        public bool this[int index]
        {
            get
            {
                return get(index);
            }

            set
            {
                set(value, index);
            }
        }

        public void set(bool value, int at)
        {
            var bi = 7 - at % 8;
            at /= 8;

            if (at >= Length)
            {
                bytes.Add(0);
            }

            byte mask = (byte)(1 << bi);

            if (value)
                bytes[at] |= mask;
            else
                bytes[at] &= (byte)~mask;
        }

        public bool get(int at)
        {
            var bi = 7 - at % 8;
            at /= 8;

            byte mask = (byte)(1 << bi);
            return (bytes[at] & mask) == mask;
        }



        public byte[] ReversedArray()
        {
            byte[] rev = new byte[Length];

            for (var i = (int)Length; i > 0; i--)
            {
                rev[Length - i] = bytes.ElementAt(i - 1);
            }

            return rev;
        }

        public override string ToString()
        {
            string s = "";

            //for (int i = (int)Size - 1; i >= 0; i--)
            //{
            //}
            for (int i = 0; i < Size; i++)
            {
                s += (get(i) ? "1" : "0");

                if ((i + 1) % 8 == 0)
                    s += " ";
                if ((i + 1) % 64 == 0)
                    s += "\r\n";
            }
            return s;
        }
    }
}
