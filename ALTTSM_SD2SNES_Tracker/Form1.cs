using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Xml;
using System.Linq.Expressions;
using System.Diagnostics;
using System.IO;
using usb2snes;

/* Item Image Index
LTTP
2	Bombos0
3	Bombos1
4	Book0
5	Book1
6	boomerang0
7	Boomerang1
8	Boomerang2
9	Boomerang3
10	Bottle0
11	Bottle1
12	BowArrow0
13	BowArrow1
14	BowArrow2
15	BowArrow3
16	ByrnaCape0
17	ByrnaCape1
18	ByrnaCape2
19	ByrnaCape3
20	CaneOSomaria0
21	CaneOSomaria1
24	Crystal0
25	crystal1
26	Crystal2
29	Ether0
30	Ether1
31	Flippers0
32	Flippers1
33	Ganon0
34	Ganon1
35	Glove0
36	Glove1
37	Glove2
42	Hammer0
43	Hammer1
46	Hookshot0
47	HookShot1
52	Lamp0
53	Lamp1
54	LonkShoe0
55	LonkShoes1
56	Mirror0
57	Mirror1
58	MoonPearl0
59	MoonPearl1
64	mushpwdr0
65	MushPwdr1
66	MushPwdr2
67	MushPwdr3
68	Pendant0
69	Pendant1
70	Pendant2
71	Pendant3
76	Quake0
77	Quake1
80	Rods0
81	Rods1
82	Rods2
83	Rods3
86	ShovFlute0
87	ShovFlute1
88	ShovFlute2
89	ShovFlute3
98	Sword0
99	Sword1
100	Sword2
101	Sword3
102 Sword4

Super Metroid
60	MorphBall0
94	SpeedBooster0
84	ScrewAtk0
90	SpaceYump0
0	Bomb0
44	HighYumpBoots0
103	VSuit0
104 VSuit1
40	GSuit0
22	ChargeBeam0
48	IceBeam0
105	WaveBeam0
106 WaveBeam1
92	Spazer0
74	Plasma0
38	GrappleBeam0
96	SpringBall0
50	Kraid0
72	Phantoon0
27	Draygon0
78	Ridley0
62	MotherBrain0
107 Missile0
108 Missile1
109 SMissile0
110 SMissile1
111 PBomb0
112 PBomb1
*/

namespace ALTTSM_SD2SNES_Tracker
{
    public partial class Form1 : Form
    {
        // Initialize the SD2SNES core for communication via USB.
        core daCore = new core();

        // Holds 512 Bytes for holding data from SD2SNES.
        // string[] allData = new string[];
        byte[] data = new byte[512];
        
        // File output handler initialization for testing.
        string path = @"c:\temp\test.txt";

        // For checking which game is active. Uses 0xA173FE
        bool Game = true;

        // Active SM Data locations.
        // Range to grab items @ F509A4 & F509A5
        uint SMAItems = 0xF509A0;
        // Offset to find the total item values @ F509C0
        int TItmOS = 32;
        // Super Metroid Active Bosses
        uint SMABoss = 0xF5D820;

        // Active LTTP Data locations
        uint LTTPItems = 0xF5F340;

        // SRAM values for when a game is inactive.
        // Only updates when a save operation is invoked.
        // 0xA06000-7FFF stores LTTP data when Super Metroid is Active.
        // uint LTTPSRAM = 0xA06000;
        // 0xA16000-0xA17FFF stores Super Metroid data when LTTP is Active.
        // uint SMSRAM = 0xA16000;

        // SRAM location for end game flags.
        uint SRAM_SM_COMPLETED = 0xA17402 + 0x3EC000;
        uint SRAM_ALTTP_COMPLETED = 0xA17506 + 0x3EC000;

        // Temp Storage when in the other game.
        // OG Values = A17900 & A17B00
        // Vs original values SRAM Offset for SD2SNES = + 3EC000
        // SD2SNES Locations
        uint SMinLTTPTemp = 0xE03900;
        uint SMBossinLTTP = 0xE02070;
        // E03B40
        uint LTTPinSMTemp = 0xE03B40;

        // Fun Run Stuff? (Not implemented but this is where Link's movement speed values reside)
        // uint LTTPSpeed = 0x47E210;

        // coordinates to place first box in the grid
        static int x = 0, y = 0, columns = 8, rows = 7;

        int totalBoxes = rows * columns;

        List<PictureBox> pictureBoxList = new List<PictureBox>();

        public Form1()
        {
            InitializeComponent();

            // DumpAllRam(daCore, path, data);

            timer1.Enabled = true;
            timer1.Interval = 500;

            //DoubleBuffered = true;
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            // set up grid for images.
            for (int row = 0; row < rows; row++)
            {
                int curY = y + row * 40;
                for (int col = 0; col < columns; col++)
                {
                    int curX = x + 3 + col * 40;
                    PictureBox picture = new PictureBox
                    {
                        Name = "pictureBox" + (row + col),
                        Size = new Size(40, 40),
                        Location = new Point(curX, curY),
                        BorderStyle = BorderStyle.None,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Visible = true
                    };
                    this.Controls.Add(picture);
                    pictureBoxList.Add(picture);
                }
            }

            // initialize base images
            // Row 1
            pictureBoxList[0].Image = imageList1.Images[12];
            pictureBoxList[1].Image = imageList1.Images[6];
            pictureBoxList[2].Image = imageList1.Images[46];
            pictureBoxList[3].Image = imageList1.Images[64];
            pictureBoxList[4].Image = imageList1.Images[4];
            pictureBoxList[5].Image = imageList1.Images[68];
            pictureBoxList[6].Image = imageList1.Images[68];
            pictureBoxList[7].Image = imageList1.Images[68];

            // Row 2
            pictureBoxList[8].Image = imageList1.Images[52];
            pictureBoxList[9].Image = imageList1.Images[80];
            pictureBoxList[10].Image = imageList1.Images[2];
            pictureBoxList[11].Image = imageList1.Images[29];
            pictureBoxList[12].Image = imageList1.Images[76];
            pictureBoxList[13].Image = imageList1.Images[24];
            pictureBoxList[14].Image = imageList1.Images[24];
            pictureBoxList[15].Image = imageList1.Images[24];

            // Row 3
            pictureBoxList[16].Image = imageList1.Images[10];
            pictureBoxList[17].Image = imageList1.Images[42];
            pictureBoxList[18].Image = imageList1.Images[86];
            pictureBoxList[19].Image = imageList1.Images[16];
            pictureBoxList[20].Image = imageList1.Images[56];
            pictureBoxList[21].Image = imageList1.Images[24];
            pictureBoxList[22].Image = imageList1.Images[24];
            pictureBoxList[23].Image = imageList1.Images[24];

            // Row 4
            pictureBoxList[24].Image = imageList1.Images[20];
            pictureBoxList[25].Image = imageList1.Images[54];
            pictureBoxList[26].Image = imageList1.Images[35];
            pictureBoxList[27].Image = imageList1.Images[31];
            pictureBoxList[28].Image = imageList1.Images[58];
            pictureBoxList[29].Image = imageList1.Images[98];
            pictureBoxList[30].Image = imageList1.Images[24];
            pictureBoxList[31].Image = imageList1.Images[33];

            // Row 5
            pictureBoxList[32].Image = imageList1.Images[60];
            pictureBoxList[33].Image = imageList1.Images[94];
            pictureBoxList[34].Image = imageList1.Images[84];
            pictureBoxList[35].Image = imageList1.Images[90];
            pictureBoxList[36].Image = imageList1.Images[0];
            pictureBoxList[37].Image = imageList1.Images[44];
            pictureBoxList[38].Image = imageList1.Images[103];
            pictureBoxList[39].Image = imageList1.Images[40];

            // Row 6
            pictureBoxList[40].Image = imageList1.Images[22];
            pictureBoxList[41].Image = imageList1.Images[48];
            pictureBoxList[42].Image = imageList1.Images[105];
            pictureBoxList[43].Image = imageList1.Images[92];
            pictureBoxList[44].Image = imageList1.Images[74];
            pictureBoxList[45].Image = imageList1.Images[38];
            pictureBoxList[46].Image = imageList1.Images[96];
            //pictureBoxList[47].Image = imageList1.Images[1];

            // Row 7
            pictureBoxList[48].Image = imageList1.Images[107];
            pictureBoxList[49].Image = imageList1.Images[109];
            pictureBoxList[50].Image = imageList1.Images[111];
            pictureBoxList[51].Image = imageList1.Images[50];
            pictureBoxList[52].Image = imageList1.Images[72];
            pictureBoxList[53].Image = imageList1.Images[27];
            pictureBoxList[54].Image = imageList1.Images[78];
            pictureBoxList[55].Image = imageList1.Images[62];
        }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (daCore.Connected() == false)
        {
            daCore.Connect("COM3");
        }

        if (daCore.Connected() == true)
        {
            // Code for showing in game timer of Super Metroid.
            //daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, TMB, (uint)512); //(uint)(0xF509DA), (uint)512);
            //daCore.GetData(data, 0, 512);
            //int ms = (data[0] + (data[1] << 8)) * (1000 / 60);
            //int sec = data[2] + (data[3] << 8);
            //int min = data[4] + (data[5] << 8);
            //int hr = data[6] + (data[7] << 8);
            //var SMTime = new TimeSpan(0, hr, min, sec);

            // Check which game is active. (True = SM, False = LTTP)
            daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, (uint)0xE033FE, (uint)512);
            daCore.GetData(data, 0, 512);
            //MessageBox.Show(data[0].ToString());

            if (data[0].ToString() == "0")
                Game = false;
            else
                Game = true;

            // Active SM Logic
            if (Game == true)
            {
                daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, SMAItems, (uint)512);
                daCore.GetData(data, 0, 512);

                /*F509A4 Bits = Collected Items
                 00101111 = 2F
                1 - Varia
                2 - Spring Ball
                3 - Morph Ball
                4 - Screw Attack
                5 - NA
                6 - Gravity Suit 
                2F = 00101111 = 45
                */
                int curItmSM = data[4]; // Current SM Item Status from F009A4

                // Convert to Binary string for item check.
                string curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(2, 1) == "1" && pictureBoxList[39].Image != imageList1.Images[41])
                    pictureBoxList[39].Image = imageList1.Images[41]; // Gravity
                else if (curItmBin.Substring(2, 1) == "0" && pictureBoxList[39].Image != imageList1.Images[40])
                    pictureBoxList[39].Image = imageList1.Images[40];
                if (curItmBin.Substring(4, 1) == "1" && pictureBoxList[34].Image != imageList1.Images[85])
                    pictureBoxList[34].Image = imageList1.Images[85]; // Screw Attack
                else if (curItmBin.Substring(4, 1) == "0" && pictureBoxList[34].Image != imageList1.Images[84])
                    pictureBoxList[34].Image = imageList1.Images[84];
                if (curItmBin.Substring(5, 1) == "1" && pictureBoxList[32].Image != imageList1.Images[61])
                    pictureBoxList[32].Image = imageList1.Images[61]; // Morph
                else if (curItmBin.Substring(5, 1) == "0" && pictureBoxList[32].Image != imageList1.Images[60])
                    pictureBoxList[32].Image = imageList1.Images[60];
                if (curItmBin.Substring(6, 1) == "1" && pictureBoxList[46].Image != imageList1.Images[97])
                    pictureBoxList[46].Image = imageList1.Images[97]; // Spring Ball
                else if (curItmBin.Substring(6, 1) == "0" && pictureBoxList[46].Image != imageList1.Images[96])
                    pictureBoxList[46].Image = imageList1.Images[96];
                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[38].Image != imageList1.Images[104])
                    pictureBoxList[38].Image = imageList1.Images[104]; // Varia
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[38].Image != imageList1.Images[103])
                    pictureBoxList[38].Image = imageList1.Images[103];

                curItmSM = data[5]; // Minus 2 due to offset in SRAM.

                // Convert to Binary string for item check.
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(1, 1) == "1" && pictureBoxList[45].Image != imageList1.Images[39])
                    pictureBoxList[45].Image = imageList1.Images[39]; // Grapple
                else if (curItmBin.Substring(1, 1) == "0" && pictureBoxList[45].Image != imageList1.Images[38])
                    pictureBoxList[45].Image = imageList1.Images[38];
                if (curItmBin.Substring(2, 1) == "1" && pictureBoxList[33].Image != imageList1.Images[95])
                    pictureBoxList[33].Image = imageList1.Images[95]; // Speed Booster
                else if (curItmBin.Substring(2, 1) == "0" && pictureBoxList[33].Image != imageList1.Images[94])
                    pictureBoxList[33].Image = imageList1.Images[94];
                if (curItmBin.Substring(3, 1) == "1" && pictureBoxList[36].Image != imageList1.Images[1])
                    pictureBoxList[36].Image = imageList1.Images[1]; // Bomb
                else if (curItmBin.Substring(3, 1) == "0" && pictureBoxList[36].Image != imageList1.Images[0])
                    pictureBoxList[36].Image = imageList1.Images[0];
                if (curItmBin.Substring(6, 1) == "1" && pictureBoxList[35].Image != imageList1.Images[91])
                    pictureBoxList[35].Image = imageList1.Images[91]; // Space Yump
                else if (curItmBin.Substring(6, 1) == "0" && pictureBoxList[35].Image != imageList1.Images[90])
                    pictureBoxList[35].Image = imageList1.Images[90];
                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[37].Image != imageList1.Images[45])
                    pictureBoxList[37].Image = imageList1.Images[45]; // Hi Yump Boots
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[37].Image != imageList1.Images[44])
                    pictureBoxList[37].Image = imageList1.Images[44];

                /* F509A9 = Charge Beam
                Charge = 00010000 = 10 */
                curItmSM = data[9]; // Minus 2 due to offset in SRAM.
                curItmBin = GetIntBinaryString(curItmSM);
                //MessageBox.Show(curItmBin);
                if (curItmBin.Substring(3, 1) == "1" && pictureBoxList[40].Image != imageList1.Images[23])
                    pictureBoxList[40].Image = imageList1.Images[23]; // Charge Beam
                else if (curItmBin.Substring(3, 1) == "0" && pictureBoxList[40].Image != imageList1.Images[22])
                    pictureBoxList[40].Image = imageList1.Images[22];

                /* F509A8 = Beams
                Wave    = 00000001 = 01
                Ice     = 00000010 = 02
                Spazer  = 00000100 = 04
                Plasma  = 00001000 = 08 */

                curItmSM = data[8]; // Minus 2 due to offset in SRAM.
                curItmBin = GetIntBinaryString(curItmSM);
                // Plasma
                if (curItmBin.Substring(4, 1) == "1" && pictureBoxList[44].Image != imageList1.Images[75])
                    pictureBoxList[44].Image = imageList1.Images[75];
                else if (curItmBin.Substring(4, 1) == "0" && pictureBoxList[44].Image != imageList1.Images[74])
                    pictureBoxList[44].Image = imageList1.Images[74];
                // Spazer
                if (curItmBin.Substring(5, 1) == "1" && pictureBoxList[43].Image != imageList1.Images[93])
                    pictureBoxList[43].Image = imageList1.Images[93];
                else if (curItmBin.Substring(5, 1) == "0" && pictureBoxList[43].Image != imageList1.Images[92])
                    pictureBoxList[43].Image = imageList1.Images[92];
                // Ice
                if (curItmBin.Substring(6, 1) == "1" && pictureBoxList[41].Image != imageList1.Images[49])
                    pictureBoxList[41].Image = imageList1.Images[49];
                else if (curItmBin.Substring(6, 1) == "0" && pictureBoxList[41].Image != imageList1.Images[48])
                    pictureBoxList[41].Image = imageList1.Images[48];
                // Wave
                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[42].Image != imageList1.Images[106])
                    pictureBoxList[42].Image = imageList1.Images[106];
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[42].Image != imageList1.Images[105])
                    pictureBoxList[42].Image = imageList1.Images[105];

                // Display Total SM Item Counts. 
                // Note: The + TItmOS is an offset from the originally called memory value.
                // Calling the highlighted image when > 0.
                int tM = data[7 + TItmOS] + data[8 + TItmOS]; // Total Missiles
                if (tM > 0 && pictureBoxList[48].Image != imageList1.Images[108])
                    pictureBoxList[48].Image = imageList1.Images[108];
                else if (tM == 0 && pictureBoxList[48].Image != imageList1.Images[107])
                    pictureBoxList[48].Image = imageList1.Images[107];

                int tSM = data[15 + TItmOS] + (data[12 + TItmOS]); // Total Super Missiles
                if (tSM > 0 && pictureBoxList[49].Image != imageList1.Images[110])
                    pictureBoxList[49].Image = imageList1.Images[110];
                else if (tSM == 0 && pictureBoxList[49].Image != imageList1.Images[109])
                    pictureBoxList[49].Image = imageList1.Images[109];

                int tPB = data[16 + TItmOS]; // Total Pwr Bombs.
                if (tPB > 0 && pictureBoxList[50].Image != imageList1.Images[112])
                    pictureBoxList[50].Image = imageList1.Images[112];
                else if (tPB == 0 && pictureBoxList[50].Image != imageList1.Images[111])
                    pictureBoxList[50].Image = imageList1.Images[111];

                // Display Maximum SM Ammo.
                Lbl_MT.Text = tM.ToString();
                Lbl_SMT.Text = tSM.ToString();
                Lbl_PBT.Text = tPB.ToString();

                /* Boss Status
                D828-D82F
                Primary bosses dead Value = 1
                D829 = Kraid
                D82A = Ridley
                D82B = Phantoon
                D82C = Draygon
                // Hex 0E = Binary 00001111
                */

                // Fetch Memory for SM Boss Status
                daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, SMABoss, (uint)512);
                daCore.GetData(data, 0, 512);

                curItmSM = data[9]; // Kraid
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[51].Image != imageList1.Images[51])
                    pictureBoxList[51].Image = imageList1.Images[51]; // Defeated
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[51].Image != imageList1.Images[50])
                    pictureBoxList[51].Image = imageList1.Images[50];

                curItmSM = data[10]; // Ridley
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[54].Image != imageList1.Images[79])
                    pictureBoxList[54].Image = imageList1.Images[79]; // Defeated
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[54].Image != imageList1.Images[78])
                    pictureBoxList[54].Image = imageList1.Images[78];

                curItmSM = data[11]; // Phantoon
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[52].Image != imageList1.Images[73])
                    pictureBoxList[52].Image = imageList1.Images[73]; // Defeated
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[52].Image != imageList1.Images[72])
                    pictureBoxList[52].Image = imageList1.Images[72];

                curItmSM = data[12]; // Draygon
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[53].Image != imageList1.Images[28])
                    pictureBoxList[53].Image = imageList1.Images[28]; // Defeated
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[53].Image != imageList1.Images[27])
                    pictureBoxList[53].Image = imageList1.Images[27];

                // ************************************************************** //
                // In SM LTTP Logic

                daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, LTTPinSMTemp, (uint)512);
                daCore.GetData(data, 0, 512);

                int[] curItmLTTP = new int[25];
                // No Item | Item | Additional States...
                curItmLTTP[0] = data[0];   // 0|Bow|B+Arrow|Silv|B+Silv
                curItmLTTP[1] = data[1];   // 0|BlueBmrng|RedBmrng
                curItmLTTP[2] = data[2];   // 0|HookShot
                curItmLTTP[3] = data[4];   // 0|Shroom|M.Powd|?PowderShroom
                curItmLTTP[4] = data[14];  // 0|Book of Mudora
                curItmLTTP[5] = data[52];  // Bitwise 00000|G|B|R Pendant

                curItmLTTP[6] = data[10];  // 0|Lamp
                curItmLTTP[7] = data[5];   // 0|FireRod
                curItmLTTP[8] = data[6];   // 0|IceRod
                curItmLTTP[9] = data[7];   // 0|Bombos
                curItmLTTP[10] = data[8];   // 0|Ether
                curItmLTTP[11] = data[9];   // 0|Quake
                                            //curItmLTTP[24]*3 Crystals

                curItmLTTP[12] = data[15];  // 0|Bottle1|2|3|4
                curItmLTTP[13] = data[11];  // 0|Hammer
                curItmLTTP[14] = data[12];  // 0|Shovel|Flute|Flute+Birb
                curItmLTTP[15] = data[17];  // 0|CoByrna
                curItmLTTP[16] = data[18];  // 0|Magic Cape
                curItmLTTP[17] = data[19];  // 0|Mg.Scroll|Mirror
                                            //curItmLTTP[24]*3 Crystals

                curItmLTTP[18] = data[16];  // 0|CoSomaria
                curItmLTTP[19] = data[21];  // 0|Lonk Shoes
                curItmLTTP[20] = data[20];  // 0|PwrGlove|Titans Mitt
                curItmLTTP[21] = data[22];  // 0|Flippers
                curItmLTTP[22] = data[23];  // 0|Moon Pearl
                curItmLTTP[23] = data[25];  // 0|Fighter's|Master|Tempered|Gold Sword
                                            //curItmLTTP[24]*1 Crystal

                // Check all active LTTP item states & update imgs.
                if (curItmLTTP[0] == 0 && pictureBoxList[0].Image != imageList1.Images[12])
                    pictureBoxList[0].Image = imageList1.Images[12]; // No Bow
                else if (curItmLTTP[0] == 2 && pictureBoxList[0].Image != imageList1.Images[13])
                    pictureBoxList[0].Image = imageList1.Images[13]; // Bow+Arrow
                else if (curItmLTTP[0] == 3 && pictureBoxList[0].Image != imageList1.Images[14])
                    pictureBoxList[0].Image = imageList1.Images[14]; // Silvers
                else if (curItmLTTP[0] == 4 && pictureBoxList[0].Image != imageList1.Images[15])
                    pictureBoxList[0].Image = imageList1.Images[15]; // Bow+Silvers

                if (curItmLTTP[1] == 0 && pictureBoxList[1].Image != imageList1.Images[6])
                    pictureBoxList[1].Image = imageList1.Images[6]; // No Bmrg
                else if (curItmLTTP[1] == 1 && pictureBoxList[1].Image != imageList1.Images[7])
                    pictureBoxList[1].Image = imageList1.Images[7]; // Blue
                else if (curItmLTTP[1] == 2 && pictureBoxList[1].Image != imageList1.Images[8])
                    pictureBoxList[1].Image = imageList1.Images[8]; // Red
                else if (curItmLTTP[1] == 3 && pictureBoxList[1].Image != imageList1.Images[9])
                    pictureBoxList[1].Image = imageList1.Images[9]; // R&B

                if (curItmLTTP[2] == 0 && pictureBoxList[2].Image != imageList1.Images[46])
                    pictureBoxList[2].Image = imageList1.Images[46]; // No HookShot
                else if (curItmLTTP[2] == 1 && pictureBoxList[2].Image != imageList1.Images[47])
                    pictureBoxList[2].Image = imageList1.Images[47]; // HookShot

                if (curItmLTTP[3] == 0 && pictureBoxList[3].Image != imageList1.Images[64])
                    pictureBoxList[3].Image = imageList1.Images[64]; // No MushPwdr
                else if (curItmLTTP[3] == 1 && pictureBoxList[3].Image != imageList1.Images[65])
                    pictureBoxList[3].Image = imageList1.Images[65]; // Mush
                else if (curItmLTTP[3] == 2 && pictureBoxList[3].Image != imageList1.Images[66])
                    pictureBoxList[3].Image = imageList1.Images[66]; // Pwdr
                else if (curItmLTTP[3] == 3 && pictureBoxList[3].Image != imageList1.Images[67])
                    pictureBoxList[3].Image = imageList1.Images[67]; // MushPwdr

                if (curItmLTTP[4] == 0 && pictureBoxList[4].Image != imageList1.Images[4])
                    pictureBoxList[4].Image = imageList1.Images[4]; // No Mudora
                else if (curItmLTTP[4] == 1 && pictureBoxList[4].Image != imageList1.Images[5])
                    pictureBoxList[4].Image = imageList1.Images[5]; // Book of Mudora

                string curLTTPItmBin = GetIntBinaryString(curItmLTTP[5]); // Bitwise 00000|G|B|R Pendant
                if (curLTTPItmBin.Substring(5, 1) == "1" && pictureBoxList[5].Image != imageList1.Images[69])
                    pictureBoxList[5].Image = imageList1.Images[69]; // Courage Pendant
                else if (curLTTPItmBin.Substring(5, 1) == "0" && pictureBoxList[5].Image != imageList1.Images[68])
                    pictureBoxList[5].Image = imageList1.Images[68];
                if (curLTTPItmBin.Substring(6, 1) == "1" && pictureBoxList[6].Image != imageList1.Images[70])
                    pictureBoxList[6].Image = imageList1.Images[70]; // Wisdom Pendant
                else if (curLTTPItmBin.Substring(6, 1) == "0" && pictureBoxList[6].Image != imageList1.Images[68])
                    pictureBoxList[6].Image = imageList1.Images[68];
                if (curLTTPItmBin.Substring(7, 1) == "1" && pictureBoxList[7].Image != imageList1.Images[71])
                    pictureBoxList[7].Image = imageList1.Images[71]; // Strength Pendant
                else if (curLTTPItmBin.Substring(7, 1) == "0" && pictureBoxList[7].Image != imageList1.Images[68])
                    pictureBoxList[7].Image = imageList1.Images[68];

                if (curItmLTTP[6] == 0 && pictureBoxList[8].Image != imageList1.Images[52])
                    pictureBoxList[8].Image = imageList1.Images[52]; // No Lamp
                else if (curItmLTTP[6] == 1 && pictureBoxList[8].Image != imageList1.Images[53])
                    pictureBoxList[8].Image = imageList1.Images[53]; // Lamp

                if (curItmLTTP[7] == 0 && curItmLTTP[8] == 0 && pictureBoxList[9].Image != imageList1.Images[80])
                    pictureBoxList[9].Image = imageList1.Images[80]; // No Rods
                else if (curItmLTTP[7] == 1 && curItmLTTP[8] == 0 && pictureBoxList[9].Image != imageList1.Images[81])
                    pictureBoxList[9].Image = imageList1.Images[81]; // Fire Only
                else if (curItmLTTP[7] == 0 && curItmLTTP[8] == 1 && pictureBoxList[9].Image != imageList1.Images[82])
                    pictureBoxList[9].Image = imageList1.Images[82]; // Ice Only
                else if (curItmLTTP[7] == 1 && curItmLTTP[8] == 1 && pictureBoxList[9].Image != imageList1.Images[83])
                    pictureBoxList[9].Image = imageList1.Images[83]; // Fire & Ice

                if (curItmLTTP[9] == 0 && pictureBoxList[10].Image != imageList1.Images[2])
                    pictureBoxList[10].Image = imageList1.Images[2]; // No Bombos
                else if (curItmLTTP[9] == 1 && pictureBoxList[10].Image != imageList1.Images[3])
                    pictureBoxList[10].Image = imageList1.Images[3]; // Bombos

                if (curItmLTTP[10] == 0 && pictureBoxList[11].Image != imageList1.Images[29])
                    pictureBoxList[11].Image = imageList1.Images[29]; // No Ether
                else if (curItmLTTP[10] == 1 && pictureBoxList[11].Image != imageList1.Images[30])
                    pictureBoxList[11].Image = imageList1.Images[30]; // Ether

                if (curItmLTTP[11] == 0 && pictureBoxList[12].Image != imageList1.Images[76])
                    pictureBoxList[12].Image = imageList1.Images[76]; // No Quake
                else if (curItmLTTP[11] == 1 && pictureBoxList[12].Image != imageList1.Images[77])
                    pictureBoxList[12].Image = imageList1.Images[77]; // Quake

                // Bitwise 00111111 = 00|SWoods|TTown|SwmP|TR|IPal|DrkP|MisMire Crystals
                curItmLTTP[24] = data[58];

                // Handle Palace Logic
                string CrystalState = GetIntBinaryString(curItmLTTP[24]);
                if (CrystalState.Substring(6, 1) == "1" && pictureBoxList[13].Image != imageList1.Images[25])
                    pictureBoxList[13].Image = imageList1.Images[25]; // Dark Palace
                else if (CrystalState.Substring(6, 1) == "0" && pictureBoxList[13].Image != imageList1.Images[24])
                    pictureBoxList[13].Image = imageList1.Images[24];
                if (CrystalState.Substring(3, 1) == "1" && pictureBoxList[14].Image != imageList1.Images[25])
                    pictureBoxList[14].Image = imageList1.Images[25]; // Swamp Palace
                else if (CrystalState.Substring(3, 1) == "0" && pictureBoxList[14].Image != imageList1.Images[24])
                    pictureBoxList[14].Image = imageList1.Images[24];
                if (CrystalState.Substring(1, 1) == "1" && pictureBoxList[15].Image != imageList1.Images[25])
                    pictureBoxList[15].Image = imageList1.Images[25]; // Skull Woods
                else if (CrystalState.Substring(1, 1) == "0" && pictureBoxList[15].Image != imageList1.Images[24])
                    pictureBoxList[15].Image = imageList1.Images[24];
                if (CrystalState.Substring(2, 1) == "1" && pictureBoxList[21].Image != imageList1.Images[25])
                    pictureBoxList[21].Image = imageList1.Images[25]; // Thieves_Town
                else if (CrystalState.Substring(2, 1) == "0" && pictureBoxList[21].Image != imageList1.Images[24])
                    pictureBoxList[21].Image = imageList1.Images[24];
                if (CrystalState.Substring(5, 1) == "1" && pictureBoxList[22].Image != imageList1.Images[26])
                    pictureBoxList[22].Image = imageList1.Images[26]; // Ice Palace
                else if (CrystalState.Substring(5, 1) == "0" && pictureBoxList[22].Image != imageList1.Images[24])
                    pictureBoxList[22].Image = imageList1.Images[24];
                if (CrystalState.Substring(7, 1) == "1" && pictureBoxList[23].Image != imageList1.Images[26])
                    pictureBoxList[23].Image = imageList1.Images[26]; // Misery Mire
                else if (CrystalState.Substring(7, 1) == "0" && pictureBoxList[23].Image != imageList1.Images[24])
                    pictureBoxList[23].Image = imageList1.Images[24];
                if (CrystalState.Substring(4, 1) == "1" && pictureBoxList[30].Image != imageList1.Images[25])
                    pictureBoxList[30].Image = imageList1.Images[25]; // Turtle Rock
                else if (CrystalState.Substring(4, 1) == "0" && pictureBoxList[25].Image != imageList1.Images[24])
                    pictureBoxList[30].Image = imageList1.Images[24];

                if (curItmLTTP[12] == 0 && pictureBoxList[16].Image != imageList1.Images[10])
                    pictureBoxList[16].Image = imageList1.Images[10]; // No Bottle
                else if (curItmLTTP[12] == 1 && pictureBoxList[16].Image != imageList1.Images[11])
                    pictureBoxList[16].Image = imageList1.Images[11]; // Bottle

                if (curItmLTTP[13] == 0 && pictureBoxList[17].Image != imageList1.Images[42])
                    pictureBoxList[17].Image = imageList1.Images[42]; // No Hammer
                else if (curItmLTTP[13] == 1 && pictureBoxList[17].Image != imageList1.Images[43])
                    pictureBoxList[17].Image = imageList1.Images[43]; // Hammer

                if (curItmLTTP[14] == 0 && pictureBoxList[18].Image != imageList1.Images[86])
                    pictureBoxList[18].Image = imageList1.Images[86]; // No ShovFlute
                else if (curItmLTTP[14] == 1 && pictureBoxList[18].Image != imageList1.Images[87])
                    pictureBoxList[18].Image = imageList1.Images[87]; // Shov
                else if (curItmLTTP[14] == 2 && pictureBoxList[18].Image != imageList1.Images[88])
                    pictureBoxList[18].Image = imageList1.Images[88]; // Flute
                else if (curItmLTTP[14] == 3 && pictureBoxList[18].Image != imageList1.Images[89])
                    pictureBoxList[18].Image = imageList1.Images[89]; // ShovFlute

                if (curItmLTTP[15] == 0 && curItmLTTP[16] == 0 && pictureBoxList[19].Image != imageList1.Images[16])
                    pictureBoxList[19].Image = imageList1.Images[16]; // No ByrnaCape
                else if (curItmLTTP[15] == 1 && curItmLTTP[16] == 0 && pictureBoxList[19].Image != imageList1.Images[17])
                    pictureBoxList[19].Image = imageList1.Images[17]; // Byrna
                else if (curItmLTTP[15] == 0 && curItmLTTP[16] == 1 && pictureBoxList[19].Image != imageList1.Images[18])
                    pictureBoxList[19].Image = imageList1.Images[18]; // Cape
                else if (curItmLTTP[15] == 1 && curItmLTTP[16] == 1 && pictureBoxList[19].Image != imageList1.Images[19])
                    pictureBoxList[19].Image = imageList1.Images[19]; // ByrnaCape

                if (curItmLTTP[17] == 0 && pictureBoxList[20].Image != imageList1.Images[56])
                    pictureBoxList[20].Image = imageList1.Images[56]; // No Mirror
                else if (curItmLTTP[17] == 2 && pictureBoxList[20].Image != imageList1.Images[57])
                    pictureBoxList[20].Image = imageList1.Images[57]; // Mirror

                if (curItmLTTP[18] == 0 && pictureBoxList[24].Image != imageList1.Images[20])
                    pictureBoxList[24].Image = imageList1.Images[20]; // No CaneOSomaria
                else if (curItmLTTP[18] == 1 && pictureBoxList[24].Image != imageList1.Images[21])
                    pictureBoxList[24].Image = imageList1.Images[21]; // CaneOSomaria

                if (curItmLTTP[19] == 0 && pictureBoxList[25].Image != imageList1.Images[54])
                    pictureBoxList[25].Image = imageList1.Images[54]; // No Lonk Shoe
                else if (curItmLTTP[19] == 1 && pictureBoxList[25].Image != imageList1.Images[55])
                    pictureBoxList[25].Image = imageList1.Images[55]; // Lonk Shoe

                if (curItmLTTP[20] == 0 && pictureBoxList[26].Image != imageList1.Images[35])
                    pictureBoxList[26].Image = imageList1.Images[35]; // No Gloves
                else if (curItmLTTP[20] == 1 && pictureBoxList[26].Image != imageList1.Images[36])
                    pictureBoxList[26].Image = imageList1.Images[36]; // Power Glove
                else if (curItmLTTP[20] == 2 && pictureBoxList[26].Image != imageList1.Images[37])
                    pictureBoxList[26].Image = imageList1.Images[37]; // Titan's Mitt

                if (curItmLTTP[21] == 0 && pictureBoxList[27].Image != imageList1.Images[31])
                    pictureBoxList[27].Image = imageList1.Images[31]; // No Flippers
                else if (curItmLTTP[21] == 1 && pictureBoxList[27].Image != imageList1.Images[32])
                    pictureBoxList[27].Image = imageList1.Images[32]; // Flippers

                if (curItmLTTP[22] == 0 && pictureBoxList[28].Image != imageList1.Images[58])
                    pictureBoxList[28].Image = imageList1.Images[58]; // No Moon Pearl
                else if (curItmLTTP[22] == 1 && pictureBoxList[28].Image != imageList1.Images[59])
                    pictureBoxList[28].Image = imageList1.Images[59]; // Moon Pearl

                if (curItmLTTP[23] == 0 && pictureBoxList[29].Image != imageList1.Images[98])
                    pictureBoxList[29].Image = imageList1.Images[98]; // No Swd
                else if (curItmLTTP[23] == 1 && pictureBoxList[29].Image != imageList1.Images[99])
                    pictureBoxList[29].Image = imageList1.Images[99]; // Fighter's Sword
                else if (curItmLTTP[23] == 2 && pictureBoxList[29].Image != imageList1.Images[100])
                    pictureBoxList[29].Image = imageList1.Images[100]; // Master Sword
                else if (curItmLTTP[23] == 3 && pictureBoxList[29].Image != imageList1.Images[101])
                    pictureBoxList[29].Image = imageList1.Images[101]; // Tempered Sword
                else if (curItmLTTP[23] == 4 && pictureBoxList[29].Image != imageList1.Images[102])
                    pictureBoxList[29].Image = imageList1.Images[102]; // Butter Sword

                daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, SRAM_SM_COMPLETED, (uint)512);
                daCore.GetData(data, 0, 512);

                // Is Super Metroid Complete?
                if (data[0] > 0 && pictureBoxList[55].Image != imageList1.Images[63])
                    pictureBoxList[55].Image = imageList1.Images[63];
                else if (data[0] == 0 && pictureBoxList[55].Image != imageList1.Images[62])
                    pictureBoxList[55].Image = imageList1.Images[62];

                // LTTP's Completion flag is +260 away from Super Metroid.
                // LTTP Complete?
                if (data[260] > 0 && pictureBoxList[31].Image != imageList1.Images[34])
                    pictureBoxList[31].Image = imageList1.Images[34];
                else if (data[260] == 0 && pictureBoxList[31].Image != imageList1.Images[33])
                    pictureBoxList[31].Image = imageList1.Images[33];
            }
            else
            {
                // LTTP Running Logic

                daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, LTTPItems, (uint)512);
                daCore.GetData(data, 0, 512);

                int[] curItmLTTP = new int[25];
                                            // No Item | Item | Additional States...
                curItmLTTP[0]  = data[0];   // 0|Bow|B+Arrow|Silv|B+Silv
                curItmLTTP[1]  = data[1];   // 0|BlueBmrng|RedBmrng
                curItmLTTP[2]  = data[2];   // 0|HookShot
                curItmLTTP[3]  = data[4];   // 0|Shroom|M.Powd|?PowderShroom
                curItmLTTP[4]  = data[14];  // 0|Book of Mudora
                curItmLTTP[5]  = data[52];  // Bitwise 00000|G|B|R Pendant

                curItmLTTP[6]  = data[10];  // 0|Lamp
                curItmLTTP[7]  = data[5];   // 0|FireRod
                curItmLTTP[8]  = data[6];   // 0|IceRod
                curItmLTTP[9]  = data[7];   // 0|Bombos
                curItmLTTP[10] = data[8];   // 0|Ether
                curItmLTTP[11] = data[9];   // 0|Quake
                                            //curItmLTTP[24]*3 Crystals

                curItmLTTP[12] = data[15];  // 0|Bottle1|2|3|4
                curItmLTTP[13] = data[11];  // 0|Hammer
                curItmLTTP[14] = data[12];  // 0|Shovel|Flute|Flute+Birb
                curItmLTTP[15] = data[17];  // 0|CoByrna
                curItmLTTP[16] = data[18];  // 0|Magic Cape
                curItmLTTP[17] = data[19];  // 0|Mg.Scroll|Mirror
                                            //curItmLTTP[24]*3 Crystals

                curItmLTTP[18] = data[16];  // 0|CoSomaria
                curItmLTTP[19] = data[21];  // 0|Lonk Shoes
                curItmLTTP[20] = data[20];  // 0|PwrGlove|Titans Mitt
                curItmLTTP[21] = data[22];  // 0|Flippers
                curItmLTTP[22] = data[23];  // 0|Moon Pearl
                curItmLTTP[23] = data[25];  // 0|Fighter's|Master|Tempered|Gold Sword
                                            //curItmLTTP[24]*1 Crystal

                // Check all active LTTP item states & update imgs.
                if (curItmLTTP[0] == 0 && pictureBoxList[0].Image != imageList1.Images[12])
                    pictureBoxList[0].Image = imageList1.Images[12]; // No Bow
                else if (curItmLTTP[0] == 2 && pictureBoxList[0].Image != imageList1.Images[13])
                    pictureBoxList[0].Image = imageList1.Images[13]; // Bow+Arrow
                else if (curItmLTTP[0] == 3 && pictureBoxList[0].Image != imageList1.Images[14])
                    pictureBoxList[0].Image = imageList1.Images[14]; // Silvers
                else if (curItmLTTP[0] == 4 && pictureBoxList[0].Image != imageList1.Images[15])
                    pictureBoxList[0].Image = imageList1.Images[15]; // Bow+Silvers

                if (curItmLTTP[1] == 0 && pictureBoxList[1].Image != imageList1.Images[6])
                    pictureBoxList[1].Image = imageList1.Images[6]; // No Bmrg
                else if (curItmLTTP[1] == 1 && pictureBoxList[1].Image != imageList1.Images[7])
                    pictureBoxList[1].Image = imageList1.Images[7]; // Blue
                else if (curItmLTTP[1] == 2 && pictureBoxList[1].Image != imageList1.Images[8])
                    pictureBoxList[1].Image = imageList1.Images[8]; // Red
                else if (curItmLTTP[1] == 3 && pictureBoxList[1].Image != imageList1.Images[9])
                    pictureBoxList[1].Image = imageList1.Images[9]; // R&B

                if (curItmLTTP[2] == 0 && pictureBoxList[2].Image != imageList1.Images[46])
                    pictureBoxList[2].Image = imageList1.Images[46]; // No HookShot
                else if (curItmLTTP[2] == 1 && pictureBoxList[2].Image != imageList1.Images[47])
                    pictureBoxList[2].Image = imageList1.Images[47]; // HookShot

                if (curItmLTTP[3] == 0 && pictureBoxList[3].Image != imageList1.Images[64])
                    pictureBoxList[3].Image = imageList1.Images[64]; // No MushPwdr
                else if (curItmLTTP[3] == 1 && pictureBoxList[3].Image != imageList1.Images[65])
                    pictureBoxList[3].Image = imageList1.Images[65]; // Mush
                else if (curItmLTTP[3] == 2 && pictureBoxList[3].Image != imageList1.Images[66])
                    pictureBoxList[3].Image = imageList1.Images[66]; // Pwdr
                else if (curItmLTTP[3] == 3 && pictureBoxList[3].Image != imageList1.Images[67])
                    pictureBoxList[3].Image = imageList1.Images[67]; // MushPwdr

                if (curItmLTTP[4] == 0 && pictureBoxList[4].Image != imageList1.Images[4])
                    pictureBoxList[4].Image = imageList1.Images[4]; // No Mudora
                else if (curItmLTTP[4] == 1 && pictureBoxList[4].Image != imageList1.Images[5])
                    pictureBoxList[4].Image = imageList1.Images[5]; // Book of Mudora

                string curLTTPItmBin = GetIntBinaryString(curItmLTTP[5]); // Bitwise 00000|G|B|R Pendant
                if (curLTTPItmBin.Substring(5, 1) == "1" && pictureBoxList[5].Image != imageList1.Images[69])
                    pictureBoxList[5].Image = imageList1.Images[69]; // Courage Pendant
                else if (curLTTPItmBin.Substring(5, 1) == "0" && pictureBoxList[5].Image != imageList1.Images[68])
                    pictureBoxList[5].Image = imageList1.Images[68];
                if (curLTTPItmBin.Substring(6, 1) == "1" && pictureBoxList[6].Image != imageList1.Images[70])
                    pictureBoxList[6].Image = imageList1.Images[70]; // Wisdom Pendant
                else if (curLTTPItmBin.Substring(6, 1) == "0" && pictureBoxList[6].Image != imageList1.Images[68])
                    pictureBoxList[6].Image = imageList1.Images[68];
                if (curLTTPItmBin.Substring(7, 1) == "1" && pictureBoxList[7].Image != imageList1.Images[71])
                    pictureBoxList[7].Image = imageList1.Images[71]; // Strength Pendant
                else if (curLTTPItmBin.Substring(7, 1) == "0" && pictureBoxList[7].Image != imageList1.Images[68])
                    pictureBoxList[7].Image = imageList1.Images[68];

                if (curItmLTTP[6] == 0 && pictureBoxList[8].Image != imageList1.Images[52])
                    pictureBoxList[8].Image = imageList1.Images[52]; // No Lamp
                else if (curItmLTTP[6] == 1 && pictureBoxList[8].Image != imageList1.Images[53])
                    pictureBoxList[8].Image = imageList1.Images[53]; // Lamp

                if (curItmLTTP[7] == 0 && curItmLTTP[8] == 0 && pictureBoxList[9].Image != imageList1.Images[80])
                    pictureBoxList[9].Image = imageList1.Images[80]; // No Rods
                else if (curItmLTTP[7] == 1 && curItmLTTP[8] == 0 && pictureBoxList[9].Image != imageList1.Images[81])
                    pictureBoxList[9].Image = imageList1.Images[81]; // Fire Only
                else if (curItmLTTP[7] == 0 && curItmLTTP[8] == 1 && pictureBoxList[9].Image != imageList1.Images[82])
                    pictureBoxList[9].Image = imageList1.Images[82]; // Ice Only
                else if (curItmLTTP[7] == 1 && curItmLTTP[8] == 1 && pictureBoxList[9].Image != imageList1.Images[83])
                    pictureBoxList[9].Image = imageList1.Images[83]; // Fire & Ice

                if (curItmLTTP[9] == 0 && pictureBoxList[10].Image != imageList1.Images[2])
                    pictureBoxList[10].Image = imageList1.Images[2]; // No Bombos
                else if (curItmLTTP[9] == 1 && pictureBoxList[10].Image != imageList1.Images[3])
                    pictureBoxList[10].Image = imageList1.Images[3]; // Bombos

                if (curItmLTTP[10] == 0 && pictureBoxList[11].Image != imageList1.Images[29])
                    pictureBoxList[11].Image = imageList1.Images[29]; // No Ether
                else if (curItmLTTP[10] == 1 && pictureBoxList[11].Image != imageList1.Images[30])
                    pictureBoxList[11].Image = imageList1.Images[30]; // Ether

                if (curItmLTTP[11] == 0 && pictureBoxList[12].Image != imageList1.Images[76])
                    pictureBoxList[12].Image = imageList1.Images[76]; // No Quake
                else if (curItmLTTP[11] == 1 && pictureBoxList[12].Image != imageList1.Images[77])
                    pictureBoxList[12].Image = imageList1.Images[77]; // Quake

                // Bitwise 00111111 = 00|SWoods|TTown|SwmP|TR|IPal|DrkP|MisMire Crystals
                curItmLTTP[24] = data[58];

                // Handle Palace Logic
                string CrystalState = GetIntBinaryString(curItmLTTP[24]);
                if (CrystalState.Substring(6, 1) == "1" && pictureBoxList[13].Image != imageList1.Images[25])
                    pictureBoxList[13].Image = imageList1.Images[25]; // Dark Palace
                else if (CrystalState.Substring(6, 1) == "0" && pictureBoxList[13].Image != imageList1.Images[24])
                    pictureBoxList[13].Image = imageList1.Images[24];
                if (CrystalState.Substring(3, 1) == "1" && pictureBoxList[14].Image != imageList1.Images[25])
                    pictureBoxList[14].Image = imageList1.Images[25]; // Swamp Palace
                else if (CrystalState.Substring(3, 1) == "0" && pictureBoxList[14].Image != imageList1.Images[24])
                    pictureBoxList[14].Image = imageList1.Images[24];
                if (CrystalState.Substring(1, 1) == "1" && pictureBoxList[15].Image != imageList1.Images[25])
                    pictureBoxList[15].Image = imageList1.Images[25]; // Skull Woods
                else if (CrystalState.Substring(1, 1) == "0" && pictureBoxList[15].Image != imageList1.Images[24])
                    pictureBoxList[15].Image = imageList1.Images[24];
                if (CrystalState.Substring(2, 1) == "1" && pictureBoxList[21].Image != imageList1.Images[25])
                    pictureBoxList[21].Image = imageList1.Images[25]; // Thieves_Town
                else if (CrystalState.Substring(2, 1) == "0" && pictureBoxList[21].Image != imageList1.Images[24])
                    pictureBoxList[21].Image = imageList1.Images[24];
                if (CrystalState.Substring(5, 1) == "1" && pictureBoxList[22].Image != imageList1.Images[26])
                    pictureBoxList[22].Image = imageList1.Images[26]; // Ice Palace
                else if (CrystalState.Substring(5, 1) == "0" && pictureBoxList[22].Image != imageList1.Images[24])
                    pictureBoxList[22].Image = imageList1.Images[24];
                if (CrystalState.Substring(7, 1) == "1" && pictureBoxList[23].Image != imageList1.Images[26])
                    pictureBoxList[23].Image = imageList1.Images[26]; // Misery Mire
                else if (CrystalState.Substring(7, 1) == "0" && pictureBoxList[23].Image != imageList1.Images[24])
                    pictureBoxList[23].Image = imageList1.Images[24];
                if (CrystalState.Substring(4, 1) == "1" && pictureBoxList[30].Image != imageList1.Images[25])
                    pictureBoxList[30].Image = imageList1.Images[25]; // Turtle Rock
                else if (CrystalState.Substring(4, 1) == "0" && pictureBoxList[25].Image != imageList1.Images[24])
                    pictureBoxList[30].Image = imageList1.Images[24];

                if (curItmLTTP[12] == 0 && pictureBoxList[16].Image != imageList1.Images[10])
                    pictureBoxList[16].Image = imageList1.Images[10]; // No Bottle
                else if (curItmLTTP[12] == 1 && pictureBoxList[16].Image != imageList1.Images[11])
                    pictureBoxList[16].Image = imageList1.Images[11]; // Bottle

                if (curItmLTTP[13] == 0 && pictureBoxList[17].Image != imageList1.Images[42])
                    pictureBoxList[17].Image = imageList1.Images[42]; // No Hammer
                else if (curItmLTTP[13] == 1 && pictureBoxList[17].Image != imageList1.Images[43])
                    pictureBoxList[17].Image = imageList1.Images[43]; // Hammer

                if (curItmLTTP[14] == 0 && pictureBoxList[18].Image != imageList1.Images[86])
                    pictureBoxList[18].Image = imageList1.Images[86]; // No ShovFlute
                else if (curItmLTTP[14] == 1 && pictureBoxList[18].Image != imageList1.Images[87])
                    pictureBoxList[18].Image = imageList1.Images[87]; // Shov
                else if (curItmLTTP[14] == 2 && pictureBoxList[18].Image != imageList1.Images[88])
                    pictureBoxList[18].Image = imageList1.Images[88]; // Flute
                else if (curItmLTTP[14] == 3 && pictureBoxList[18].Image != imageList1.Images[89])
                    pictureBoxList[18].Image = imageList1.Images[89]; // ShovFlute

                if (curItmLTTP[15] == 0 && curItmLTTP[16] == 0 && pictureBoxList[19].Image != imageList1.Images[16])
                    pictureBoxList[19].Image = imageList1.Images[16]; // No ByrnaCape
                else if (curItmLTTP[15] == 1 && curItmLTTP[16] == 0 && pictureBoxList[19].Image != imageList1.Images[17])
                    pictureBoxList[19].Image = imageList1.Images[17]; // Byrna
                else if (curItmLTTP[15] == 0 && curItmLTTP[16] == 1 && pictureBoxList[19].Image != imageList1.Images[18])
                    pictureBoxList[19].Image = imageList1.Images[18]; // Cape
                else if (curItmLTTP[15] == 1 && curItmLTTP[16] == 1 && pictureBoxList[19].Image != imageList1.Images[19])
                    pictureBoxList[19].Image = imageList1.Images[19]; // ByrnaCape

                if (curItmLTTP[17] == 0 && pictureBoxList[20].Image != imageList1.Images[56])
                    pictureBoxList[20].Image = imageList1.Images[56]; // No Mirror
                else if (curItmLTTP[17] == 2 && pictureBoxList[20].Image != imageList1.Images[57])
                    pictureBoxList[20].Image = imageList1.Images[57]; // Mirror

                if (curItmLTTP[18] == 0 && pictureBoxList[24].Image != imageList1.Images[20])
                    pictureBoxList[24].Image = imageList1.Images[20]; // No CaneOSomaria
                else if (curItmLTTP[18] == 1 && pictureBoxList[24].Image != imageList1.Images[21])
                    pictureBoxList[24].Image = imageList1.Images[21]; // CaneOSomaria

                if (curItmLTTP[19] == 0 && pictureBoxList[25].Image != imageList1.Images[54])
                    pictureBoxList[25].Image = imageList1.Images[54]; // No Lonk Shoe
                else if (curItmLTTP[19] == 1 && pictureBoxList[25].Image != imageList1.Images[55])
                    pictureBoxList[25].Image = imageList1.Images[55]; // Lonk Shoe

                if (curItmLTTP[20] == 0 && pictureBoxList[26].Image != imageList1.Images[35])
                    pictureBoxList[26].Image = imageList1.Images[35]; // No Gloves
                else if (curItmLTTP[20] == 1 && pictureBoxList[26].Image != imageList1.Images[36])
                    pictureBoxList[26].Image = imageList1.Images[36]; // Power Glove
                else if (curItmLTTP[20] == 2 && pictureBoxList[26].Image != imageList1.Images[37])
                    pictureBoxList[26].Image = imageList1.Images[37]; // Titan's Mitt

                if (curItmLTTP[21] == 0 && pictureBoxList[27].Image != imageList1.Images[31])
                    pictureBoxList[27].Image = imageList1.Images[31]; // No Flippers
                else if (curItmLTTP[21] == 1 && pictureBoxList[27].Image != imageList1.Images[32])
                    pictureBoxList[27].Image = imageList1.Images[32]; // Flippers

                if (curItmLTTP[22] == 0 && pictureBoxList[28].Image != imageList1.Images[58])
                    pictureBoxList[28].Image = imageList1.Images[58]; // No Moon Pearl
                else if (curItmLTTP[22] == 1 && pictureBoxList[28].Image != imageList1.Images[59])
                    pictureBoxList[28].Image = imageList1.Images[59]; // Moon Pearl

                if (curItmLTTP[23] == 0 && pictureBoxList[29].Image != imageList1.Images[98])
                    pictureBoxList[29].Image = imageList1.Images[98]; // No Swd
                else if (curItmLTTP[23] == 1 && pictureBoxList[29].Image != imageList1.Images[99])
                    pictureBoxList[29].Image = imageList1.Images[99]; // Fighter's Sword
                else if (curItmLTTP[23] == 2 && pictureBoxList[29].Image != imageList1.Images[100])
                    pictureBoxList[29].Image = imageList1.Images[100]; // Master Sword
                else if (curItmLTTP[23] == 3 && pictureBoxList[29].Image != imageList1.Images[101])
                    pictureBoxList[29].Image = imageList1.Images[101]; // Tempered Sword
                else if (curItmLTTP[23] == 4 && pictureBoxList[29].Image != imageList1.Images[102])
                    pictureBoxList[29].Image = imageList1.Images[102]; // Butter Sword


                // ***************************************************************** //

                // In LTTP SM Logic
                // Applied main offset to SMinLTTPTemp during initialization way above.
                daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, SMinLTTPTemp, (uint)512);
                daCore.GetData(data, 0, 512);

                int curItmSM = data[4 - 2]; // Minus 2 due to offset in SRAM.

                // Convert to Binary string for item check.
                string curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(2, 1) == "1" && pictureBoxList[39].Image != imageList1.Images[41])
                    pictureBoxList[39].Image = imageList1.Images[41]; // Gravity
                else if (curItmBin.Substring(2, 1) == "0" && pictureBoxList[39].Image != imageList1.Images[40])
                    pictureBoxList[39].Image = imageList1.Images[40];
                if (curItmBin.Substring(4, 1) == "1" && pictureBoxList[34].Image != imageList1.Images[85])
                    pictureBoxList[34].Image = imageList1.Images[85]; // Screw Attack
                else if (curItmBin.Substring(4, 1) == "0" && pictureBoxList[34].Image != imageList1.Images[84])
                    pictureBoxList[34].Image = imageList1.Images[84];
                if (curItmBin.Substring(5, 1) == "1" && pictureBoxList[32].Image != imageList1.Images[61])
                    pictureBoxList[32].Image = imageList1.Images[61]; // Morph
                else if (curItmBin.Substring(5, 1) == "0" && pictureBoxList[32].Image != imageList1.Images[60])
                    pictureBoxList[32].Image = imageList1.Images[60];
                if (curItmBin.Substring(6, 1) == "1" && pictureBoxList[46].Image != imageList1.Images[97])
                    pictureBoxList[46].Image = imageList1.Images[97]; // Spring Ball
                else if (curItmBin.Substring(6, 1) == "0" && pictureBoxList[46].Image != imageList1.Images[96])
                    pictureBoxList[46].Image = imageList1.Images[96];
                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[38].Image != imageList1.Images[104])
                    pictureBoxList[38].Image = imageList1.Images[104]; // Varia
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[38].Image != imageList1.Images[103])
                    pictureBoxList[38].Image = imageList1.Images[103];

                curItmSM = data[5 - 2]; // Minus 2 due to offset in SRAM.

                // Convert to Binary string for item check.
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(1, 1) == "1" && pictureBoxList[45].Image != imageList1.Images[39])
                    pictureBoxList[45].Image = imageList1.Images[39]; // Grapple
                else if (curItmBin.Substring(1, 1) == "0" && pictureBoxList[45].Image != imageList1.Images[38])
                    pictureBoxList[45].Image = imageList1.Images[38];
                if (curItmBin.Substring(2, 1) == "1" && pictureBoxList[33].Image != imageList1.Images[95])
                    pictureBoxList[33].Image = imageList1.Images[95]; // Speed Booster
                else if (curItmBin.Substring(2, 1) == "0" && pictureBoxList[33].Image != imageList1.Images[94])
                    pictureBoxList[33].Image = imageList1.Images[94];
                if (curItmBin.Substring(3, 1) == "1" && pictureBoxList[36].Image != imageList1.Images[1])
                    pictureBoxList[36].Image = imageList1.Images[1]; // Bomb
                else if (curItmBin.Substring(3, 1) == "0" && pictureBoxList[36].Image != imageList1.Images[0])
                    pictureBoxList[36].Image = imageList1.Images[0];
                if (curItmBin.Substring(6, 1) == "1" && pictureBoxList[35].Image != imageList1.Images[91])
                    pictureBoxList[35].Image = imageList1.Images[91]; // Space Yump
                else if (curItmBin.Substring(6, 1) == "0" && pictureBoxList[35].Image != imageList1.Images[90])
                    pictureBoxList[35].Image = imageList1.Images[90];
                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[37].Image != imageList1.Images[45])
                    pictureBoxList[37].Image = imageList1.Images[45]; // Hi Yump Boots
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[37].Image != imageList1.Images[44])
                    pictureBoxList[37].Image = imageList1.Images[44];

                /* F509A9 = Charge Beam
                Charge = 00010000 = 10 */
                curItmSM = data[9 - 2]; // Minus 2 due to offset in SRAM.
                curItmBin = GetIntBinaryString(curItmSM);
                //MessageBox.Show(curItmBin);
                if (curItmBin.Substring(3, 1) == "1" && pictureBoxList[40].Image != imageList1.Images[23])
                    pictureBoxList[40].Image = imageList1.Images[23]; // Charge Beam
                else if (curItmBin.Substring(3, 1) == "0" && pictureBoxList[40].Image != imageList1.Images[22])
                    pictureBoxList[40].Image = imageList1.Images[22];

                /* F509A8 = Beams
                Wave    = 00000001 = 01
                Ice     = 00000010 = 02
                Spazer  = 00000100 = 04
                Plasma  = 00001000 = 08 */

                curItmSM = data[8 - 2]; // Minus 2 due to offset in SRAM.
                curItmBin = GetIntBinaryString(curItmSM);
                // Plasma
                if (curItmBin.Substring(4, 1) == "1" && pictureBoxList[44].Image != imageList1.Images[75])
                    pictureBoxList[44].Image = imageList1.Images[75];
                else if (curItmBin.Substring(4, 1) == "0" && pictureBoxList[44].Image != imageList1.Images[74])
                    pictureBoxList[44].Image = imageList1.Images[74];
                // Spazer
                if (curItmBin.Substring(5, 1) == "1" && pictureBoxList[43].Image != imageList1.Images[93])
                    pictureBoxList[43].Image = imageList1.Images[93];
                else if (curItmBin.Substring(5, 1) == "0" && pictureBoxList[43].Image != imageList1.Images[92])
                    pictureBoxList[43].Image = imageList1.Images[92];
                // Ice
                if (curItmBin.Substring(6, 1) == "1" && pictureBoxList[41].Image != imageList1.Images[49])
                    pictureBoxList[41].Image = imageList1.Images[49];
                else if (curItmBin.Substring(6, 1) == "0" && pictureBoxList[41].Image != imageList1.Images[48])
                    pictureBoxList[41].Image = imageList1.Images[48];
                // Wave
                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[42].Image != imageList1.Images[106])
                    pictureBoxList[42].Image = imageList1.Images[106];
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[42].Image != imageList1.Images[105])
                    pictureBoxList[42].Image = imageList1.Images[105];

                // Display Total SM Item Counts. 
                // Note: The + TItmOS is an offset from the originally called memory value.
                // Calling the highlighted image when > 0.
                int tM = data[7 + TItmOS - 2] + data[8 + TItmOS - 2]; // Total Missiles
                if (tM > 0 && pictureBoxList[48].Image != imageList1.Images[108])
                    pictureBoxList[48].Image = imageList1.Images[108];
                else if (tM == 0 && pictureBoxList[48].Image != imageList1.Images[107])
                    pictureBoxList[48].Image = imageList1.Images[107];

                int tSM = data[15 + TItmOS - 2] + (data[12 + TItmOS - 2]); // Total Super Missiles
                if (tSM > 0 && pictureBoxList[49].Image != imageList1.Images[110])
                    pictureBoxList[49].Image = imageList1.Images[110];
                else if (tSM == 0 && pictureBoxList[49].Image != imageList1.Images[109])
                    pictureBoxList[49].Image = imageList1.Images[109];

                int tPB = data[16 + TItmOS - 2]; // Total Pwr Bombs.
                if (tPB > 0 && pictureBoxList[50].Image != imageList1.Images[112])
                    pictureBoxList[50].Image = imageList1.Images[112];
                else if (tPB == 0 && pictureBoxList[50].Image != imageList1.Images[111])
                    pictureBoxList[50].Image = imageList1.Images[111];

                // Display Maximum SM Ammo.
                Lbl_MT.Text = tM.ToString();
                Lbl_SMT.Text = tSM.ToString();
                Lbl_PBT.Text = tPB.ToString();

                // Fetch Memory for SM Boss Status
                daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, SMBossinLTTP, (uint)512);
                daCore.GetData(data, 0, 512);

                curItmSM = data[9]; // Kraid
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[51].Image != imageList1.Images[51])
                    pictureBoxList[51].Image = imageList1.Images[51]; // Defeated
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[51].Image != imageList1.Images[50])
                    pictureBoxList[51].Image = imageList1.Images[50];

                curItmSM = data[10]; // Ridley
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[54].Image != imageList1.Images[79])
                    pictureBoxList[54].Image = imageList1.Images[79]; // Defeated
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[54].Image != imageList1.Images[78])
                    pictureBoxList[54].Image = imageList1.Images[78];

                curItmSM = data[11]; // Phantoon
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[52].Image != imageList1.Images[73])
                    pictureBoxList[52].Image = imageList1.Images[73]; // Defeated
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[52].Image != imageList1.Images[72])
                    pictureBoxList[52].Image = imageList1.Images[72];

                curItmSM = data[12]; // Draygon
                curItmBin = GetIntBinaryString(curItmSM);

                if (curItmBin.Substring(7, 1) == "1" && pictureBoxList[53].Image != imageList1.Images[28])
                    pictureBoxList[53].Image = imageList1.Images[28]; // Defeated
                else if (curItmBin.Substring(7, 1) == "0" && pictureBoxList[53].Image != imageList1.Images[27])
                    pictureBoxList[53].Image = imageList1.Images[27];

                daCore.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, SRAM_SM_COMPLETED, (uint)512);
                daCore.GetData(data, 0, 512);

                // Is Super Metroid Complete?
                if (data[0] > 0 && pictureBoxList[55].Image != imageList1.Images[63])
                    pictureBoxList[55].Image = imageList1.Images[63];
                else if (data[0] == 0 && pictureBoxList[55].Image != imageList1.Images[62])
                    pictureBoxList[55].Image = imageList1.Images[62];

                // LTTP's Completion flag is +260 away from Super Metroid.
                // LTTP Complete?
                if (data[260] > 0 && pictureBoxList[31].Image != imageList1.Images[34])
                    pictureBoxList[31].Image = imageList1.Images[34];
                else if (data[260] == 0 && pictureBoxList[31].Image != imageList1.Images[33])
                    pictureBoxList[31].Image = imageList1.Images[33];
            }
        }
    }

        static string GetIntBinaryString(int n)
        {
            char[] b = new char[8];
            int pos = 7;
            int i = 0;

            while (i < 8)
            {
                if ((n & (1 << i)) != 0)
                    b[pos] = '1';
                else
                    b[pos] = '0';

                pos--;
                i++;
            }
            return new string(b);
        }

        // Dumps all the RAM from your entire SD2SNES. 
        // Core object from USB2SNES.DLL, txt Path, 512 byte array.
        // NOTE!: This takes a few minutes. Do not open the txt while this is running.
        static void DumpAllRam(core CORE, string path, byte[] bArr)
        {
            // initialize a string used below to
            // hold 512 Bytes at a time and push the data into the txt file.
            string aData = ""; 

            // Checks to make sure the given file exists.
            // Creates it if it does not.
            if (!File.Exists(path))
            {
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.Write("");
                    sw.Dispose();
                }
            }

            // Connect to SD2SNES Core if it is not already connected.
            if (CORE.Connected() == false)
            {
                CORE.Connect("COM3");
            }

            // Construct 1 512 byte chunk of data in aData and output it to txt file (path).
            if (CORE.Connected() == true)
            {
                for (int i = 0; i < 32768; i++) // 32768; i++)
                {
                    CORE.SendCommand(usbint_server_opcode_e.GET, usbint_server_space_e.SNES, usbint_server_flags_e.STREAM_BURST, (uint)(i*512), (uint)512);
                    CORE.GetData(bArr, 0, 512);
                    for (int j = 0; j < 512; j++)
                    {
                        // 000000 | 00
                        if (j == 0)
                            aData += "\n" + ((j) + i * 512).ToString("X6") + " | " + bArr[j].ToString("X2");
                        // 000000 | 00 01... 0F + "\n"
                        else if (j % 16 == 0 && j != 0)
                            aData += "\n" + ((j) + i * 512).ToString("X6") + " | " + bArr[j].ToString("X2");
                        else
                            aData += " " + bArr[j].ToString("X2");
                    }
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.Write(aData);
                        aData = "";
                        sw.Dispose();
                        // CORE.ClearData();
                    }
                }
            }
        }
    }
}
 
 
 