using System;
using System.Collections.Generic;
using System.IO; 
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MKFileStream
{
    class Class1
    {

        public int port,ulevel,plevel;
        public string       ip,filestr,somename;
        public List<string> seedlist;
        FileStream fs; //测试使用
        public uint contentLength, totalPacket, contentMs,magic,SamplesCount,Samplerate;
        TcpClient tcp;
        List<UInt16> Packlist;
        public Class1() {
            seedlist = new List<string>(100);
        }
        public void run() { //此函数没有采用异步通信  有洁癖的可以改成 异步，并抓取异常处理

            /*
             此处 示例 为 发送一个文件， 如果是发送多个文件; 播放完第一个文件  再 解析第二个文件 期间 TCp 不需要断开重连 数据接着发送Contents 部分就OK ;当所有的文件发送完成再断开tcp连接;
             */
            fs = new FileStream(filestr, FileMode.Open); 
            byte[] temp = new byte[4];
            int res,wes;
            res = fs.Read(temp,0,4);//1 magic
            if (res == 4)
            {
                magic = (uint)(temp[3] | temp[2] << 8 | temp[1] << 16 | temp[0] << 24);
                Console.WriteLine("magic:"+magic.ToString());
            } 
            res = fs.Read(temp, 0, 4);//2 SamplesCount
            if (res == 4)
            {
                SamplesCount = (uint)(temp[3] | temp[2] << 8 | temp[1] << 16 | temp[0] << 24);
                Console.WriteLine("SamplesCount:" + SamplesCount.ToString());
            } 
            res = fs.Read(temp,0,4);//3 contentLength
            if (res == 4)
            {
                contentLength = (uint)(temp[3] | temp[2] << 8 | temp[1] << 16 | temp[0] << 24);
                Console.WriteLine("contentLength:" + contentLength.ToString());
            } 
            res = fs.Read(temp, 0, 4);//4 totalPacket
            if (res == 4)
            {
                totalPacket = (uint)(temp[3] | temp[2] << 8 | temp[1] << 16 | temp[0] << 24);
                Console.WriteLine("totalPacket:" + totalPacket.ToString());
            } 
            res = fs.Read(temp, 0, 4);//5 contentMs
            if (res == 4)
            {
                contentMs = (uint)(temp[3] | temp[2] << 8 | temp[1] << 16 | temp[0] << 24);
                Console.WriteLine("contentMs:" + contentMs.ToString());
            } 
            res = fs.Read(temp, 0, 4);//6 Samplerate
            if (res == 4)
            {
                Samplerate = (uint)(temp[3] | temp[2] << 8 | temp[1] << 16 | temp[0] << 24);
                Console.WriteLine("Samplerate:" + Samplerate.ToString());
            }

            Packlist = new List<UInt16>((int)totalPacket);

            byte[] contents = new byte[contentLength];
            res = fs.Read(contents,0,(int)contentLength);
            if (res != contentLength)
            {
                Console.WriteLine("Contents read Error" );
            }
            else {
                Console.WriteLine("fs.Position;~!" + fs.Position.ToString());
            }        
            UInt32 packrecheck = 0;
            for ( int i = 0 ; i< this.totalPacket ; i++ ) {
                uint d;
                byte[] pt = new byte[2];
                res = fs.Read(pt, 0, 2);
                if (res == 2)
                {
                    d = (uint)((UInt32)pt[1] + (UInt32)pt[0]*256 );
                    packrecheck += d;
                    Console.WriteLine("pack:" + i.ToString() +"==>" + d.ToString() );
                    Packlist.Add((UInt16)d);
                }
                else
                {
                    Console.WriteLine("File Error~!"); //shut down
                    break;
                }
            }
            if (packrecheck!=contentLength)
            {
                Console.WriteLine("File ERRor");
            }

            tcp = new TcpClient();
            tcp.Connect(this.ip, this.port);
            if (tcp.Connected)
            {
                NetworkStream streamnet = tcp.GetStream();
                streamnet.ReadTimeout = 3000;
                byte[] byt = new byte[10000];
                Random r = new Random();
                int b = r.Next(1, 999);
                String seelist = " ";
                for (int n = 0; n < this.seedlist.Count; n++)
                {
                    if (this.seedlist[n] != "")
                    {
                        seelist += "\"" + this.seedlist[n] + "\",";
                    }
                }
                if (this.seedlist.Count > 0)
                    seelist = seelist.Remove(seelist.LastIndexOf(","), 1);
                else
                    Console.WriteLine("Seed Missed");

                Console.WriteLine(seelist);
                string data = "{\"cmd\":\"PLAYLIST\",\"ulevel\": " + ulevel.ToString() + ",\"plevel\":" + plevel.ToString() + ",\"Umask\":\"" + somename + "\",\"Umagic\":" + b.ToString() + ",\"snlist\":[ " + seelist + "]}";
                string header = data.Length.ToString();
                string datasend = header + "\n" + data;
                byte[] forsend = System.Text.Encoding.Default.GetBytes(datasend);
                streamnet.Write(forsend, 0, forsend.Length);
                Console.WriteLine(datasend);
                res = streamnet.Read(byt, 0, 10000);
                if (res == 4)
                {
                    string reb = Encoding.ASCII.GetString(byt, 0, 4);
                    if (reb == "WELL")
                    {
                        streamnet.ReadTimeout = 1;                        
                        Console.WriteLine("NowCould do flush");
                        int jk = 0;
                        int packpin_ = 0;
                        do
                        {
                            streamnet.Write(contents, packpin_, this.Packlist[jk]);
                            packpin_ += this.Packlist[jk];
                            jk++;
                           
                            Thread.Sleep((int)this.contentMs);
                           
                        } while ( jk < this.Packlist.Count);
                    }
                    else if (reb == "DISS")
                    {
                        Console.WriteLine("Net Work JSON error 03");
                    }
                    else
                    {
                        Console.WriteLine("Net Work Sth error 02");
                    }
                }
                else
                {
                    Console.WriteLine("Net Work Sth error 01");
                }
                streamnet.Close();

            }else {
                Console.WriteLine("Net Work Sth error 00");
            }
            this.tcp.Close();

        }
    }
}
