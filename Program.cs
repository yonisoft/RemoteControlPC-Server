using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

namespace volume
{
    class Program
    {
        static int fixednum = 0; // fixed the addr.tostring legnth.
        static int addrchoosed = 0; // the chooseaddr int convert.
        static string chooseaddr = "0"; // the readline var.

        ///list of ip vars///
        static String strHostName = string.Empty;
        static IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
        static IPAddress[] addr = ipEntry.AddressList;
        /////////////

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        static string[] packets = new string[99999999];
        static void Main()
        {
            Console.WriteLine("choose ip address for remote:\n");
            for (int i = 0; i < addr.Length; i++)
            {
                fixednum++;
                Console.WriteLine(i + ". IP Address: {0}", addr[i].ToString());
            }
            fixednum--;
            Rewrite:
            chooseaddr = Console.ReadLine();
            try
            {
                addrchoosed = Convert.ToInt32(chooseaddr);
                if (addrchoosed > fixednum)
                {
                    Console.WriteLine("You can choose only number between 0-" + fixednum);
                    goto Rewrite;
                }
            }
            catch
            {
                Console.WriteLine("You can choose only number between 0-" + fixednum);
                goto Rewrite;
            }
            startserver();
        }

        public static void startserver()
        {
            int vol;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse(addr[addrchoosed].ToString());
                // TcpListener server = new TcpListener(port);
                TcpListener server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;
                Console.WriteLine(localAddr+":"+ port);
                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;
                    int n = 0;

                    // Loop to receive all the data sent by the client.
                    try
                    {
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = Encoding.ASCII.GetString(bytes, 0, i);
                            Console.WriteLine(String.Format("Received: {0}", data));
                            string phrase = data;
                            string[] words = phrase.Split(' ');

                            foreach (var word in words)
                            {
                                packets[n] = word;
                                Console.WriteLine(packets[n]);
                                n++;
                            }

                            if (packets[0].Equals("05"))
                            {
                                Console.WriteLine("got the 05");
                            }

                            if (packets[0].Equals("01"))
                            {
                                Console.WriteLine("got the 01");
                                PauseResume();
                            }

                            if (packets[0].Equals("02"))
                            {
                                Console.WriteLine("got the 02");
                                vol = Convert.ToInt32(packets[1]);
                                Console.WriteLine(vol);
                                VolumeSET(vol);
                            }

                            if (packets[0].Equals("03"))
                            {
                                Console.WriteLine("got the 03");
                                Back();
                            }

                            if (packets[0].Equals("04"))
                            {
                                Console.WriteLine("got the 04");
                                Forward();
                            }

                            // Process the data sent by the client.
                            data = data.ToUpper();

                            byte[] msg = Encoding.ASCII.GetBytes(data);

                            // Send back a response.
                            stream.Write(msg, 0, msg.Length);
                            Console.WriteLine(String.Format("Sent: {0}", data));
                        }
                    }
                    catch
                    {
                        startserver();
                    }

                    // Shutdown and end connection
                    client.Close();
                    continue;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public static void VolumeSET(int vol)
        {
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(3);
                keybd_event((byte)Keys.VolumeDown, 0, 0, 0);
            }

            vol = vol / 2;
            for (int b = 0;b < vol; b++)
            {
                Thread.Sleep(10);
                keybd_event((byte)Keys.VolumeUp, 0, 0, 0);
            }
        }

        public static void PauseResume()
        {
            Console.WriteLine("pause");
            SendKeys.SendWait(" ");
            Thread.Sleep(100);
        }

        public static void Back()
        {
            Console.WriteLine("Back");
            SendKeys.SendWait("{LEFT}");
            Thread.Sleep(100);
        }

        public static void Forward()
        {
            Console.WriteLine("Forward");
            SendKeys.SendWait("{RIGHT}");
            Thread.Sleep(100);
        }
    }
}
