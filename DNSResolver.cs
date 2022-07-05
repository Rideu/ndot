

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ndot
{
    struct DnsFrame
    {

    }

    public struct DnsQuestion
    {
        public byte Offset;
        public string Hostname;
        public ushort Type;
        public ushort Class;

        public override string ToString()
        {
            return Hostname;
        }
    }


    public struct DnsAnswer
    {
        public int Offset;
        public string Name;
        public ushort Type;
        public ushort Class;
        public uint TTL;
        public ushort Length;
        public string Payload;
        public byte[] Address;
        internal ushort Preference;

        public string IPString()
        {
            return _ipstring = _ipstring ?? Address.Aggregate("", (s, v) => s += v.ToString() + '.').TrimEnd('.');
        }

        string _ipstring;

        public override string ToString()
        {
            return Payload ?? Address.Aggregate("", (s, v) => s += v.ToString() + '.').TrimEnd('.');
        }
    }
    public struct DnsAuthority
    {
        public byte Offset;
        public string Hostname;
        public ushort Type;
        public ushort Class;

        public override string ToString()
        {
            return Hostname;
        }

    }

    public struct DnsResponse
    {

        public bool isResponse;
        public bool recursiveAvailable;

        public List<DnsQuestion> Requests;
        public List<DnsAnswer> Answers;
        public List<DnsAuthority> Authorities;

        public static DnsResponse FromRaw(byte[] raw)
        {
            DnsResponse respFrame = default;


            var id = BitConverter.ToUInt16(new byte[2] { raw[1], raw[0] });

            respFrame.isResponse = (raw[2] & 0b_1000_0000) == 128;
            respFrame.recursiveAvailable = (raw[3] & 0b_1000_0000) == 128;

            var QSCount = BitConverter.ToUInt16(new byte[2] { raw[5], raw[4] });
            var ANCount = BitConverter.ToUInt16(new byte[2] { raw[7], raw[6] });
            var NSCount = BitConverter.ToUInt16(new byte[2] { raw[9], raw[8] });
            var ARCount = BitConverter.ToUInt16(new byte[2] { raw[11], raw[10] });

            int i = 12;
            var questions = new List<DnsQuestion>();
            var buf = "";
            byte qoffset = (byte)i;

            if (QSCount > 0)
                while (true)
                {
                    var len = raw[i];

                    if (len == 0)
                    {
                        questions.Add(new DnsQuestion
                        {
                            Offset = qoffset,
                            Hostname = buf.Trim('.'),
                            Type = BitConverter.ToUInt16(new byte[2] { raw[i + 2], raw[i + 1] }),
                            Class = BitConverter.ToUInt16(new byte[2] { raw[i + 4], raw[i + 3] }),
                        });
                        buf = "";
                        i += 4;
                        qoffset = (byte)i;
                        if (questions.Count == QSCount)
                            break;
                        else continue;
                    }

                    for (int c = 0; c < len; c++)
                    {
                        i++;
                        buf += (char)(raw[i]);
                    }
                    buf += '.';
                    //i += len;

                    i++;
                }

            i++;
            var dnsAnswers = new List<DnsAnswer>();

            if (ANCount > 0)
                while (true)
                {

                    var isptr = (raw[i + 0] & 0b_1100_0000) == 192;
                    string host = null;
                    if (isptr)
                    {
                        var offset = raw[i + 1];
                        host = questions.FirstOrDefault(n => n.Offset == offset).Hostname;

                        if (host == null)
                            host = dnsAnswers.FirstOrDefault(n => n.Offset == offset).Payload;
                    }

                    ushort AName = BitConverter.ToUInt16(new byte[2] { raw[i + 1], raw[i + 0] });
                    i += 2;
                    ushort AType = BitConverter.ToUInt16(new byte[2] { raw[i + 1], raw[i + 0] });
                    i += 2;
                    ushort AClass = BitConverter.ToUInt16(new byte[2] { raw[i + 1], raw[i + 0] });
                    i += 2;
                    uint ATTL = BitConverter.ToUInt32(new byte[4] { raw[i + 3], raw[i + 2], raw[i + 1], raw[i + 0] });
                    i += 4;

                    ushort ALength = BitConverter.ToUInt16(new byte[2] { raw[i + 1], raw[i + 0] });
                    i += 2;

                    ushort APreference = 0;

                    var APayload = "";

                    byte len = 0;
                    var aoffset = i;
                    byte[] addrBuf = null;

                    DnsAnswer danswer = default;

                    if (AType == 1 && ALength == 4)
                    {
                        addrBuf = new byte[4];
                        for (int c = 0; c < ALength; c++)
                        {
                            addrBuf[c] = raw[i + c];
                        }
                        danswer.Address = addrBuf;
                    }
                    else
                    if (AType == 5 || AType == 15)
                    {
                        int c = 0;
                        int shift = 0;
                        if (AType == 15)
                        {
                            APreference = BitConverter.ToUInt16(new byte[2] { raw[i + 1], raw[i + 0] });
                            i += 2;
                            shift = 2;
                        }
                        for (; c < ALength - shift; c++)
                        {
                            var b = raw[i + c];
                            if ((b) == 192)
                            {
                                var idx = raw[i + c + 1];
                                APayload += '.' + extractbyoffset(raw, idx);
                                c += 1;
                            }
                            else
                            if (len == 0)
                            {
                                len = b;
                                if (APayload.Length > 0) APayload += '.';
                            }
                            else
                            {
                                APayload += (char)b;
                                len--;
                            }
                        }
                    }
                    else
                    if (AType == 16)
                    {
                        int c = 0;
                        int shift = 0;
                        //var textLength = raw[i + 0];
                        //i += 1;
                        shift = 0;
                        for (; c < ALength - shift; c++)
                        {
                            var b = raw[i + c];
                            if ((b) == 192)
                            {
                                var idx = raw[i + c + 1];
                                APayload += '.' + extractbyoffset(raw, idx);
                                c += 1;
                            }
                            else
                            if (len == 0)
                            {
                                len = b;
                                //if (APayload.Length > 0) APayload += '.';
                            }
                            else
                            {
                                APayload += (char)b;
                                len--;
                            }
                        }
                    }
                    APayload = APayload.Trim('.');

                    i += ALength - (AType == 15 ? 2 : 0);

                    danswer.Type = AType;
                    danswer.Name = host;
                    danswer.Type = AType;
                    danswer.Class = AClass;
                    danswer.TTL = ATTL;
                    danswer.Length = ALength;
                    danswer.Payload = string.IsNullOrEmpty(APayload) ? null : APayload;
                    danswer.Preference = APreference;
                    danswer.Offset = aoffset;


                    dnsAnswers.Add(danswer);

                    if (dnsAnswers.Count == ANCount)
                        break;
                }


            var authorities = new List<DnsAuthority>();

            if (NSCount > 0)
            {

            }

            respFrame.Requests = questions;
            respFrame.Answers = dnsAnswers;
            respFrame.Authorities = authorities;

            return respFrame;
        }

        static string extractbyoffset(byte[] src, int from)
        {
            var buf = "";
            var len = src[from];
            var i = 1;
            byte b = 0;
            while (len >= 0 && len < 64)
            {
                b = src[i + from];

                if (b == 192)
                {
                    buf += '.' + extractbyoffset(src, src[i + from + 1]);
                }
                else
                if (len == 0)
                {
                    if (b > 0 && b < 64)
                    {

                        buf += '.';
                        len = (byte)(b + 1); // +1 will be substracted
                    }
                    else
                        break;
                }
                else
                    buf += (char)b;
                len--;
                i++;

            }

            return buf;
        }
    }
     
     
}
