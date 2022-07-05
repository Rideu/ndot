
using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ndot
{

    /// <summary>
    /// Provides DNS-to-DoT (DNS over TLS) conversion mechanism 
    /// </summary>

    public class DoTConverter
    {
        static DoTConverter staticDoT;
        public static DoTConverter StaticInstance()
        {
            return (staticDoT = staticDoT ?? new DoTConverter());
        }

        TcpClient dnsClient = new TcpClient();
        SslStream ssl;

        public void Open(int port = 53)
        {
            this.port = port;
            connectToDoT();

            listenOne();
        }

        ManualResetEvent connectionBlock = new ManualResetEvent(false);

        void connectToDoT()
        {

            connectionBlock.Reset();

            if (dnsClient != null)
            {
                if (dnsClient.Connected)
                {
                    dnsClient.Close();
                }
            }
            dnsClient?.Dispose();
            dnsClient = null;

            while (dnsClient == null && !(dnsClient?.Connected ?? false))
            {

                try
                {

                    dnsClient = new TcpClient();
                    dnsClient.Connect(IPAddress.Parse(baseDNSAddress), 853); // [3], 3.1. Session Initiation
                }
                catch (Exception e)
                {

                    Log.LogMsgTag($"[DoTC]", $"Remote DoT connection fail. Retrying...", ConsoleColor.Red, ConsoleColor.Gray);
                    dnsClient.Close();
                    dnsClient = null;
                    Thread.Sleep(5000);
                }

            }
            Log.LogMsgTag($"[DoTC]", $"Remote DoT is intact. Securing the connection...", ConsoleColor.Green, ConsoleColor.Gray);

            var ns = dnsClient.GetStream();
            ssl = new SslStream(ns);
            while (!ssl.IsAuthenticated)
            {
                try
                {

                    ssl.AuthenticateAsClient(clientOpts);
                    Log.LogMsgTag($"[DoTC]", $"Connection secured", ConsoleColor.Green, ConsoleColor.Gray);
                    connectionBlock.Set();
                }
                catch (AuthenticationException e)
                {
                    Log.LogMsgTag($"[DoTC]", $"Fail: Authentication failure ({e.GetExceptionRecur()})", ConsoleColor.Red, ConsoleColor.Gray);
                }
                catch (Exception e)
                {
                    throw;
                }
            }

        }

        UdpClient lastClient;
        int port = 53;

        Task listenOne()
        {
            var t = Task.Run(() =>
            {

                var l = IPEndPoint.Parse("0.0.0.0");
                l.Port = port;
                UdpClient udpListener = null;
                try
                {

                    udpListener = new UdpClient(l);
                    lastClient = udpListener;
                    var raw = udpListener.Receive(ref l);

                    if (!udpListener.Client.Connected)
                        udpListener.Connect(l);
                    OnMessage(raw, l, udpListener);
                }
                catch (Exception e)
                {

                    if (udpListener != null && udpListener.Client.Connected)
                        udpListener.Close();
                    if (lastClient != null)
                    {
                        lastClient.Close();
                        lastClient = null;
                    }

                    udpListener = null;

                    Log.LogMsgTag($"[DoTC]", $"EXC: {e.GetExceptionRecur()}", ConsoleColor.Red, ConsoleColor.Gray);
                }

                listenOne();
            });

            return t;
        }


        void OnMessage(byte[] raw, IPEndPoint remote, UdpClient udpListener)
        {

            try
            {

                if (raw.Length > 512)
                {

                    udpListener.Close();
                    return;
                }

                Log.LogMsgTag($"[DoTC]", $"Incoming retransmisson", ConsoleColor.Green, ConsoleColor.Gray);


                byte[] recv = new byte[1024 * 16];
                var read = ssl.ReadAsync(recv, 0, recv.Length).ContinueWith<DnsResponse>((t) =>
                {

                    var dnc = dnsClient;

                    if (t.Result == 0)
                    {
                        Log.LogMsgTag($"[DoTC]", $"Remote DoT is closed. Reconnecting...", ConsoleColor.Yellow, ConsoleColor.Gray);
                        connectToDoT();
                        udpListener.Close();


                        return default;
                    }
                    else
                    {


                        DnsResponse resp = default;
                        var size = BitConverter.ToUInt16(new byte[2] { recv[1], recv[0] });
                        if (size > 0 && size <= recv.Length)
                        {

                            var raw = new byte[size];

                            for (int i = 0; i < raw.Length; i++)
                            {
                                raw[i] = recv[i + 2];
                            }
                            udpListener.Send(raw);
                            udpListener.Close();


                            try
                            {

                                resp = DnsResponse.FromRaw(raw);

                                foreach (var a in resp.Answers)
                                {
                                    Log.LogMsgTag($"[DoTC]", $"Name: {a.Name} - {a.Address.Print(".")}", ConsoleColor.Green, ConsoleColor.Gray);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.LogMsgTag($"[DoTC]", $"Failed to parse response", ConsoleColor.Yellow, ConsoleColor.Gray);
                            }
                        }
                        else
                        {

                            udpListener.Send(recv);
                            udpListener.Close();
                        }

                        Log.LogMsgTag($"[DoTC]", $"Sending back...\r\n", ConsoleColor.Green, ConsoleColor.Gray);


                        return resp;
                    }
                });


                var length = BitConverter.GetBytes((ushort)raw.Length).Rev(); // [2], 4.2.2. TCP usage

                connectionBlock.WaitOne();
                if (!dnsClient.Client.Connected)
                    connectToDoT();

                ssl.Write(length, 0, length.Length);
                ssl.Write(raw, 0, raw.Length);

                var result = read.Result;

            }
            catch (Exception e)
            {

                if (udpListener != null)
                    udpListener.Close();
                Log.LogMsgTag($"[DoTC:F]", $"EXC: {e.GetExceptionRecur()}", ConsoleColor.Red, ConsoleColor.Gray);
            }
        }



        public static string baseDNSAddress = "8.8.4.4"; // Google Public DNS

        static SslClientAuthenticationOptions clientOpts = new SslClientAuthenticationOptions
        {
            EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            TargetHost = baseDNSAddress,
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
