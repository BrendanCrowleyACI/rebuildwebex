using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
    
namespace rebuild
{  
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();
        DataRow dr;
        List<FileItems> ListaContenedor = new List<FileItems>();
        public Form1()
        {
            InitializeComponent();
        }
        string GetLastPart(string nombre)
        {
            int x=0;
            int p = 0;
            for (p = nombre.IndexOf("_", x); p != -1; x++)
                p = nombre.IndexOf("_", x);
            
            return nombre.Substring(x);
        }
        void ClearLog()
        {
            textLog.Clear();
			textLog.Text = "Webex Rebuild Tool v0.2\r\n" +
			"Based upon https://github.com/skuater/rebuildwebex (Spanish), this version has been translated to English with a few bugs fixed :)\r\n" +
			"======================\r\n" +
			"\r\n" +
			"How to use:\r\n" +
			"1. Click on the Play recording link in the email with subject \"recording of today's session ...\"\r\n" +
			"2. This opens the WebEx Network Recording Player, wait until video will be downloaded. There is a blue buffering indicator and it should be 100%\r\n" +
			"3. Copy contents of WebEx temp folder (%USERPROFILE%\\AppData\\Local\\Temp\\<several digits>) to a temporary location (e.g. C:\\Temp\\rebuild) when full recording is downloaded\r\n" +
			"4. Run rebuild.exe, provide temporary location from Step 3 into field Directory: and press Run button\r\n" +
			"5. Open resulted rebuild.arf file located in the temporary location with the WebEx Network Recording Player\r\n" +
			"6. Optional: Convert video to appropriate format for offline viewing: File > Convert Format (usually *.wmv or *.mp4)";

        }
        void toLog(string texto)
        {
            textLog.Text = textLog.Text + "\r\n" + texto;
            textLog.SelectionStart = textLog.TextLength;
            textLog.SelectionLength = 1;
            textLog.ScrollToCaret();
        } 
        void Rebuild(List<FileItems> ficherosEntrada,string ficheroSalida)
        {
            using (FileStream stream = new FileStream(ficheroSalida, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite))
            {
                BinaryReader reader = new BinaryReader(stream);
                BinaryWriter writer = new BinaryWriter(stream);
                ARF_HEADER arf_head = new ARF_HEADER();
                arf_head.e_magic = 0x00020001;
                arf_head.e_unknow = 0x00000000;
                arf_head.e_filesize = 0xadafea;
                arf_head.e_reserved0 = 0;
                arf_head.e_nsections = 0x1b;
                arf_head.e_reserved1 = 0;
                int video = 0;
                ARF_ITEMES[] arf_item = new ARF_ITEMES[arf_head.e_nsections];
                long Offset = Marshal.SizeOf(typeof(ARF_ITEMES)) * arf_head.e_nsections + Marshal.SizeOf(typeof(ARF_HEADER));
                for (int x = 0; x < ficherosEntrada.Count; x++)
                {
                    FileInfo f = new FileInfo(ficherosEntrada[x].FileName);
                    FileSystemInfo f1 = new FileInfo(ficherosEntrada[x].FileName);
                    long len = f.Length;
                    string nombre = f1.Name;
                    long id = (long)ficherosEntrada[x].id;
                    arf_item[x].e_id = (uint)id;
                    arf_item[x].e_sectionoffset = (uint)Offset;
                    arf_item[x].e_sectionlen=  (uint)f.Length;
                    
                    arf_item[x].e_indice = 0;
                  
                    if (id == 0x7010C || id == 0x7010d)
                        arf_item[x].e_indice = (uint)video;
                    if (id == 0x7010d)
                        video++;
                    arf_item[x].e_reserved1 = 0;
                    arf_item[x].e_reserved2 = 0;
                    arf_item[x].e_reserved3 = 0;
                    arf_item[x].e_reserved4 = 0;
                    Offset += f.Length + 1;
                }
                arf_head.e_filesize = (uint)(Offset - 1);
                writer.Write(getBytes(arf_head));
                for (int x = 0; x < arf_head.e_nsections; x++)
                    writer.Write(getBytes(arf_item[x]));
                Offset = 0;
                for (int x = 0; x < ficherosEntrada.Count; x++)
                {
                    FileStream streamOrigen = new FileStream(ficherosEntrada[x].FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    BinaryReader readerOrigen = new BinaryReader(streamOrigen);
                    byte[] bytes = readerOrigen.ReadBytes( (int)streamOrigen.Length);
                    writer.Write(bytes);
                    Offset += streamOrigen.Length;
                    //long  pad = (Offset % 4);
                    //if (pad!=0)
                    writer.Write((byte)0);//x);

                }
                writer.Flush();
                writer.Close();
                toLog("[=] Reconstructed file: " + ficheroSalida);
				toLog("[=] Finished :)");
                MessageBox.Show("Reconstruction completed","Finished",MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
          
        
        }
        DataRow AñadeArchivoAGrid(string archivo)
        {
           
            FileInfo f = null;
            FileSystemInfo f1 = null;
            f = new FileInfo(archivo);
            f1 = new FileInfo(archivo);
            dr = dt.NewRow();
            //Get File name of each file name
            dr["Real_Filename"] = f1.Name;
            dr["File_Name"] = GetLastPart(f1.Name);
            //Get File Type/Extension of each file 
            dr["File_Type"] = f1.Extension;
            //Get File Size of each file in KB format
            dr["File_Size"] = (f.Length).ToString();
            dr["Create_Date"] = (f1.CreationTime).ToString();
            return dr;
        }
        bool FillDataGrid(string ruta)
        {
            //FileInfo f =null;
            //FileSystemInfo f1 = null;
            ListaContenedor.Clear();
            dt.Clear();
            dt.Columns.Clear();
            //Add Data Grid Columns with name
            dt.Columns.Add("File_Name");
       
            dt.Columns.Add("Real_Filename");
            dt.Columns.Add("File_Type");
            dt.Columns.Add("File_Size");
            dt.Columns.Add("Create_Date");
            //
            if (Directory.Exists(ruta))
            {
                string[] Files_CFG = Directory.GetFiles(ruta, "*.conf", SearchOption.TopDirectoryOnly);
                string[] Files_STD = Directory.GetFiles(ruta, "*.std", SearchOption.TopDirectoryOnly).OrderBy(o => new FileInfo(o).Name).ToArray();
                string[] Files_WAV = Directory.GetFiles(ruta, "*.wav", SearchOption.TopDirectoryOnly);
                string[] Files_VID = Directory.GetFiles(ruta, "*_4_*.dat", SearchOption.TopDirectoryOnly).OrderBy(o => new FileInfo(o).Name).ToArray();
                string[] Files_VID_IDX = Directory.GetFiles(ruta, "*_4_*.idx", SearchOption.TopDirectoryOnly).OrderBy(o => new FileInfo(o).Name).ToArray(); ;

                string[] Files_FINMM = Directory.GetFiles(ruta, "*_6_*.dat", SearchOption.TopDirectoryOnly).OrderBy(o => new FileInfo(o).Name).ToArray(); ;
                string[] Files_FINMM_IDX = Directory.GetFiles(ruta, "*_6_*.idx", SearchOption.TopDirectoryOnly).OrderBy(o => new FileInfo(o).Name).ToArray(); ;
                string[] Files_FINMM_CAD = Directory.GetFiles(ruta, "*_6_*.cad", SearchOption.TopDirectoryOnly).OrderBy(o => new FileInfo(o).Name).ToArray(); ;
                string[] Files_FINMM_CAI = Directory.GetFiles(ruta, "*_6_*.cai", SearchOption.TopDirectoryOnly ).OrderBy(o => new FileInfo(o).Name).ToArray(); ;

                string[] Files_BACKUP = Directory.GetFiles(ruta, "*_21_*.dat", SearchOption.TopDirectoryOnly);
                string[] Files_BACKUP_IDX = Directory.GetFiles(ruta, "*_21_*.idx", SearchOption.TopDirectoryOnly);

             

                // Comprobamos el tener al menos un archivo wav y un archivo conf
                if (Files_CFG.Length != 1/* || Files_WAV.Length != 1*/)
                {
                    MessageBox.Show("Something has failed, the format has changed ....");
                    return false;
                }
                else
                {
                    // Si Existe el archivo de CHAT lo añadimos
                    if (Files_STD.Length >0)
                    {
                        toLog("[+] Chat file: " + Path.GetFileName(Files_STD[0]));
                        ListaContenedor.Add(new FileItems(Files_STD[0],  FileItems.tipoSegmento.chat));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_STD[0]));
                        if (Files_STD.Length > 1)
                        {
                            toLog("[+] file file: " + Path.GetFileName(Files_STD[1]));
                            ListaContenedor.Add(new FileItems(Files_STD[1], FileItems.tipoSegmento.file));
                            dt.Rows.Add(AñadeArchivoAGrid(Files_STD[1]));
                        }
                    }
                    else
                    {
                        //toLog("[-] Chat file: Not found.");
                    }
                    // Añadimos el archivo .CFG
                    if (Files_CFG.Length > 0)
                    {
                        toLog("[+] Config file: " + Path.GetFileName(Files_CFG[0]));
                        ListaContenedor.Add(new FileItems(Files_CFG[0], FileItems.tipoSegmento.cfg ));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_CFG[0]));
                    }
                    else
                    {
                        toLog("[-] Config file: Not found.");
                    }

                    // AÑADIMOS LOS SEGMENTOS DE VIDEO y INDICE VIDEO
                    if (Files_VID.Length > 0 && Files_VID_IDX.Length > 0 && Files_VID.Length == Files_VID_IDX.Length)
                    {
                        for (int x = 0; x < Files_VID.Length; x++)
                        {
                            // Añadimos VIDEO
                            toLog(string.Format(@"[+] Video file {0}/{1} : {2}", x + 1, Files_VID.Length, Path.GetFileName(Files_VID[x].ToString())));

                            ListaContenedor.Add(new FileItems(Files_VID[x], FileItems.tipoSegmento.video ));
                            dt.Rows.Add(AñadeArchivoAGrid(Files_VID[x]));

                            // Añadimos INDICE VIDEO
                            toLog(string.Format(@"[+] Index file {0}/{1} : {2}", x + 1, Files_VID_IDX.Length, Path.GetFileName(Files_VID[x].ToString())));
                            ListaContenedor.Add(new FileItems(Files_VID_IDX[x], FileItems.tipoSegmento.video_idx ));
                            dt.Rows.Add(AñadeArchivoAGrid(Files_VID_IDX[x]));

                        }
                    }
                    else
                    {
                        if (Files_VID.Length == 0)
                            toLog("[-] Video file: Not found.");
                        if (Files_VID_IDX.Length == 0)
                            toLog("[-] Index file: Not found.");
                        if (Files_VID.Length != Files_VID_IDX.Length)
                            toLog("[-] video or index file not pairs.");
                    }

                    // AÑADIMOS WAV
                    if (Files_WAV.Length == 1)
                    {
                        toLog("[+] Wav file: " + Path.GetFileName(Files_WAV[0]));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_WAV[0]));
                        ListaContenedor.Add(new FileItems(Files_WAV[0], FileItems.tipoSegmento.snd ));
                    }
                    else
                    {
                        if (Files_WAV.Length == 0)
                            toLog("[-] Wav file: Not found.");
                        else
                            toLog("[-] Wav file: Too many wav files.");

                    }


                    // Añadimos FIN_MM
                    if (Files_FINMM.Length == 1)
                    {
                        toLog("[+] MM_END file: " + Path.GetFileName(Files_FINMM[0]));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_FINMM[0]));
                        ListaContenedor.Add(new FileItems(Files_FINMM[0], FileItems.tipoSegmento.mmfin) );
                    }
                    else
                    {
                        if (Files_FINMM.Length == 0)
                            toLog("[-] MM_END file: Not found.");
                        else
                            toLog("[-] MM_END file: Too many wav files.");

                    }
                    // Añadimos FIN_MM_IDX
                    if (Files_FINMM_IDX.Length == 1)
                    {
                        toLog("[+] MM_IDX file: " + Path.GetFileName(Files_FINMM_IDX[0]));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_FINMM_IDX[0]));
                        ListaContenedor.Add(new FileItems(Files_FINMM_IDX[0], FileItems.tipoSegmento.mmfin_idx) );
                    }
                    else
                    {
                        if (Files_FINMM_IDX.Length == 0)
                            toLog("[-] MM_IDX end file: Not found.");
                        else
                            toLog("[-] MM_IDX end file: Too many MM_IDX files.");

                    }
                    if (Files_FINMM_CAD.Length == 1)
                    {
                        toLog("[+] MM_CAD file: " + Path.GetFileName(Files_FINMM_CAD[0]));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_FINMM_CAD[0]));
                        ListaContenedor.Add(new FileItems(Files_FINMM_CAD[0], FileItems.tipoSegmento.mmfin_cad));
                    }
                    if (Files_FINMM_CAI.Length == 1)
                    {
                        toLog("[+] MM_CAI file: " + Path.GetFileName(Files_FINMM_CAI[0]));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_FINMM_CAI[0]));
                        ListaContenedor.Add(new FileItems(Files_FINMM_CAI[0], FileItems.tipoSegmento.mmfin_cai));
                    }

                    // Añadimos BACKUP
                    if (Files_BACKUP.Length == 1)
                    {
                        toLog("[+] BACKUP file: " + Path.GetFileName(Files_BACKUP[0]));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_BACKUP[0]));
                        ListaContenedor.Add(new FileItems(Files_BACKUP[0],  FileItems.tipoSegmento.backup) );
                    }
                    else
                    {
                        if (Files_BACKUP.Length == 0)
                            toLog("[-] BACKUP file: Not found.");
                        else
                            toLog("[-] BACKUP file: Too many wav files.");

                    }


                    // Añadimos BACKUP IDX
                    if (Files_BACKUP_IDX.Length == 1)
                    {
                        toLog("[+] BACKUP_IDX file: " + Path.GetFileName(Files_BACKUP_IDX[0]));
                        dt.Rows.Add(AñadeArchivoAGrid(Files_BACKUP_IDX[0]));
                        ListaContenedor.Add(new FileItems(Files_BACKUP_IDX[0], FileItems.tipoSegmento.backup_idx));
                    }
                    else
                    {
                        if (Files_BACKUP_IDX.Length == 0)
                            toLog("[-] BACKUP_IDX file: Not found.");
                        else
                            toLog("[-] BACKUP_IDX file: Too many wav files.");

                    }

                }
                if (dt.Rows.Count > 0)
                {
                    //Finally Add DataTable into DataGridView
                    dataGridView1.DataSource = dt;
                    dataGridView1.Columns["File_Size"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dataGridView1.Columns["File_Size"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    dataGridView1.Columns["Real_Filename"].Visible = false;
                }
            }
            else
                toLog("[Error] Folder not found.");
            try
            {
                dataGridView1.Columns[0].Width = 140;
                dataGridView1.Columns[1].Width = 80;
                dataGridView1.Columns[2].Width = 74;
                dataGridView1.Columns[3].Width = 70;
                dataGridView1.Columns[4].Width = 150;
            }
            catch(Exception e)
            {
				toLog("[-] Exception: " + e);
            }
            return true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                textRuta.Text = fbd.SelectedPath;
            else
                textRuta.Text = "";
        }
        void Go()
        {
            ClearLog();
            toLog("[+] Obtaining files.");
            string ruta = textRuta.Text;
            if (ruta.Length > 0)
            {
                if (FillDataGrid(ruta))
                    Rebuild(ListaContenedor, ruta + "\\rebuild.arf");
                else
                    toLog("[-] The required files could not be found.");
            }
            else
                toLog("[-] Select a directory.");

        }
        public  T FromBinaryReader<T>(BinaryReader reader)
        {
            // Read in a byte array
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

            // Pin the managed memory while, copy it out the data, then unpin it
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }
        public  byte[] getBytes<T>(T str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ClearLog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Go();
        }

        

    }
    public class FileItems
    {
        public enum tipoSegmento
        {
            chat =      0x70100,
            file =      0x70103,
            cfg =       0x70112,
            video =     0x7010c,
            video_idx = 0x7010d,
            snd =       0x70105,
            mmfin =     0x70114,
            mmfin_idx = 0x70115,
            mmfin_cad = 0x7010A,
            mmfin_cai = 0x7010B,
            backup =    0x70110,
            backup_idx =0x70111

        };

        public string FileName;
        public tipoSegmento id;
        public FileItems(string filename, tipoSegmento ts)
        {
            FileName = filename;
            id = ts;
        }
    }
    public struct ARF_HEADER
    {    
        public UInt32 e_magic;                // Magic number 
        public UInt32 e_unknow;               // Posible ID
        public UInt32 e_filesize;             // File size
        public UInt32 e_reserved0;            // 
        public UInt32 e_nsections;            // number of sections
        public UInt32 e_reserved1;            // 
        
    }
    public struct ARF_ITEMES
    {
        public UInt32 e_id;                
        public UInt32 e_indice;               
        public UInt32 e_sectionlen;              
        public UInt32 e_reserved1;               
        public UInt32 e_sectionoffset;           
        public UInt32 e_reserved2;               
        public UInt32 e_reserved3;               
        public UInt32 e_reserved4;               

    }


}
