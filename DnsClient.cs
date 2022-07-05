using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ndot
{
    public static class DnsClient
    {
        static ushort IDIncrement = 1024;
        //static UdpClient dnsClient = new UdpClient();

        static DnsClient()
        {
            //dnsClient.Connect(IPAddress.Parse("8.8.8.8"), 53);
        }

        public static Task<DnsResponse> ResolveNameAsync(string hostname, DnsRequestType reqtype = DnsRequestType.A, bool secure = true, int dnsport = 53)
        {
            dynbitfield bf = bitfield_ctor(hostname, reqtype);

            var length = BitConverter.GetBytes((ushort)bf.bytes.Count).Rev(); // [5], 4.2.2. TCP usage

            var frame = bf.bytes.ToArray();


            if (secure)
            {

                TcpClient dnsClient = new TcpClient();

                dnsClient.Connect(IPAddress.Parse(secureDNSAddress), 853);

                var ns = dnsClient.GetStream();
                SslStream ssl = new SslStream(ns);
                ssl.AuthenticateAsClient(clientOpts);

                byte[] recv = new byte[512];
                var read = ssl.ReadAsync(recv, 0, recv.Length).ContinueWith<DnsResponse>((t) =>
                {
                    var size = BitConverter.ToUInt16(new byte[2] { recv[1], recv[0] });
                    var raw = new byte[size];

                    for (int i = 0; i < raw.Length; i++)
                    {
                        raw[i] = recv[i + 2];
                    }

                    return DnsResponse.FromRaw(raw);
                });



                ssl.Write(length, 0, length.Length);
                ssl.Write(frame, 0, frame.Length);



                return read;
            }
            else
            {

                UdpClient dnsClient = new UdpClient();
                dnsClient.Connect(IPAddress.Parse(defaultDNSAddress), dnsport);

                

                //byte[] recv = new byte[512];
                var t = dnsClient.ReceiveAsync().ContinueWith<DnsResponse>((r) =>
                {
                    try
                    {

                        var udpr = r.Result;
                        var resp = DnsResponse.FromRaw(udpr.Buffer);
                        return resp;
                    }
                    catch (Exception e)
                    {
                        return default;
                    }
                });

                dnsClient.Send(frame, frame.Length);

                return t;
            }
        }

        static dynbitfield bitfield_ctor(string hostname, DnsRequestType reqtype = DnsRequestType.A)
        {
            dynbitfield bf = new dynbitfield();
            bf.bytes = new List<byte>(14);


            for (int i = 0; i < 12; i++)
                bf.bytes.Add(0);

            // === HEADER START ===

            IDIncrement++;
            var id = BitConverter.GetBytes(IDIncrement).Rev();
            bf.bytes[0] = id[0];
            bf.bytes[1] = id[1];

            var isresponse = false;
            if (isresponse)
                bf.bytes[2] |= (byte)0b_1000_0000; // QR: 0 = query, 1 = response

            var istruncated = false;
            if (istruncated)
                bf.bytes[2] |= (byte)0b_0000_0010; // TC: 0 = not truncated, 1 = truncated

            var dorecursive = true;
            if (dorecursive)
                bf.bytes[2] |= (byte)0b_0000_0001; // RD: 0 = recursion not desired, 1 = else

            var Z = false;
            if (Z)
                bf.bytes[3] |= (byte)0b_0100_0000; // Z: reserved

            var nonauth = false;
            if (nonauth)
                bf.bytes[3] |= (byte)0b_0001_0000; // NA: 0 = don't send non-auth data, 1 = else

            var QCount = BitConverter.GetBytes((ushort)1).Rev(); // Questions Count [u16]
            bf.bytes[4] = QCount[0];
            bf.bytes[5] = QCount[1];

            var ANCount = BitConverter.GetBytes((ushort)0).Rev(); // Answer Record Count [u16]
            bf.bytes[6] = ANCount[0];
            bf.bytes[7] = ANCount[1];

            var NSCount = BitConverter.GetBytes((ushort)0).Rev(); // Authority Record Count [u16]
            bf.bytes[8] = NSCount[0];
            bf.bytes[9] = NSCount[1];

            var ARCount = BitConverter.GetBytes((ushort)0).Rev(); // Additional Record Count [u16]
            bf.bytes[10] = ARCount[0];
            bf.bytes[11] = ARCount[1];


            // === HEADER END ===


            // === QUESTION START ===

            var split = hostname.Split('.');

            foreach (var s in split)
            {
                var len = (byte)s.Length;
                bf.bytes.Add(len);
                bf.bytes.AddRange(s.UTF8Encode());
            }
            bf.bytes.Add(0);

            byte[] QType = BitConverter.GetBytes((ushort)reqtype).Rev();

            bf.bytes.AddRange(QType);

            byte QClass0 = 0;
            byte QClass1 = 1;

            byte[] QClass = new byte[2] { QClass0, QClass1 };

            bf.bytes.AddRange(QClass);

            // === QUESTION END ===

            return bf;
        }

        public static string defaultDNSAddress = NetworkInterface.GetAllNetworkInterfaces()[0].GetIPProperties().DnsAddresses[0].ToString();

        public static string secureDNSAddress = "8.8.4.4";

        static SslClientAuthenticationOptions clientOpts = new SslClientAuthenticationOptions
        {
            EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            TargetHost = secureDNSAddress,
            RemoteCertificateValidationCallback = CheckDnsServerCertCallback,
        };

        static bool CheckDnsServerCertCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {

                var c2 = certificate as X509Certificate2;

                if (c2 != null)
                {
                    var veresult = c2.Verify();
                    return veresult;
                }

                return true;
            }
            return false;
        }
    }
}
