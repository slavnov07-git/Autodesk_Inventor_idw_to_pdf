using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace idw_to_pdf
{
    public partial class InventorIdwToPdf : Form
    {
        public InventorIdwToPdf()
        {
            InitializeComponent();
            comboBox1.SelectedItem = "600";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            findIDWonDirectory(InPath.Text, out FileInfo[] Files);

            //InvApp.Documents.Open(@"C:\Проекты\Inventor\SZ\Workspaces\Рабочее пространство\Бункер\СК004.00.000.idw");
            //exportPDF(InvApp);
            if (Files != null)
            {
                label6.Text = $"Выполнено: 0 из {Files.Length}";
                OpenInventorFunc(out Inventor.Application InvApp);
                int count = 1;
                foreach (var file in Files)
                {
                    
                    listBox1.Items.Add(file.FullName + "\r\n");
                    InvApp.Documents.Open(file.FullName);
                    exportPDF(InvApp);
                    InvApp.ActiveDocument.Close(true);
                    label6.Text = $"Выполнено: {count} из {Files.Length}";
                    count++;
                    
                }
                try
                {
                    foreach (Process proc in Process.GetProcessesByName("Inventor"))
                    {
                        proc.Kill();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("В указанной папке чертежей не обнаружено!", Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }

        private void OpenInventorFunc(out Inventor.Application InvApp)
        {
            string ProcName = "Inventor.Application";
            try//получение обьекта приложения
            {

                InvApp = Marshal2.GetActiveObject(ProcName) as Inventor.Application;

            }
            catch
            {
                InvApp = Activator.CreateInstance(Type.GetTypeFromProgID(ProcName)) as Inventor.Application;
                //InvApp.Visible = VisibleInventor.Checked;
                InvApp.Visible = checkBox3.Checked;

            }
        }

        private void findIDWonDirectory(string path, out FileInfo[] Files)
        {

            if (InPath.Text == "")
            {
                InPath.Text = @"C:\";
            }

            if (OutPath.Text == "")
            {
                OutPath.Text = @"C:\idw_to_pdf\";
            }
            if (!System.IO.Directory.Exists(InPath.Text))
            {
                MessageBox.Show("В указанной директории чертежей не обнаружено");
            }

            bool existsOutPath = System.IO.Directory.Exists(OutPath.Text);
            if (!existsOutPath)
            {
                System.IO.Directory.CreateDirectory(OutPath.Text);
            }

            if (!System.IO.Directory.Exists(InPath.Text))
            {
                Files = null;
                return;
            }


            DirectoryInfo d = new DirectoryInfo(InPath.Text);
            Files = d.GetFiles("*.idw");

        }

        private void InPath_TextChanged(object sender, EventArgs e)
        {
            if (InPath.Text == "")
            {
                InPath.Text = @"C:\";
            }
        }

        private void exportPDF(Inventor.Application InvApp)
        {
            // Get the PDF translator Add-In.
            Inventor.TranslatorAddIn PDFAddin = InvApp.ApplicationAddIns.ItemById["{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}"] as Inventor.TranslatorAddIn;

            //Set a reference to the active document (the document to be published).

            Inventor.Document oDocument = InvApp.ActiveDocument;

            Inventor.TranslationContext oContext = InvApp.TransientObjects.CreateTranslationContext();
            oContext.Type = Inventor.IOMechanismEnum.kFileBrowseIOMechanism;

            // Create a NameValueMap object

            Inventor.NameValueMap oOptions = InvApp.TransientObjects.CreateNameValueMap();

            // Create a DataMedium object
            Inventor.DataMedium oDataMedium = InvApp.TransientObjects.CreateDataMedium();

            // Check whether the translator has 'SaveCopyAs' options
            try
            {
                if (PDFAddin.HasSaveCopyAsOptions[oDocument, oContext, oOptions])
                {
                    // Options for drawings...
                    if (checkBox1.Checked == true)
                        oOptions.Value["All_Color_AS_Black"] = 1;
                    else
                        oOptions.Value["All_Color_AS_Black"] = 0;

                    if(checkBox2.Checked == true)
                        oOptions.Value["Remove_Line_Weights"] = 1;
                    else
                        oOptions.Value["Remove_Line_Weights"] = 0;

                    if(comboBox1.Text == "600")
                        oOptions.Value["Vector_Resolution"] = 600;
                    if (comboBox1.Text == "400")
                        oOptions.Value["Vector_Resolution"] = 400;
                    if (comboBox1.Text == "1200")
                        oOptions.Value["Vector_Resolution"] = 1200;

                    oOptions.Value["Sheet_Range"] = Inventor.PrintRangeEnum.kPrintAllSheets;
                    oOptions.Value["Custom_Begin_Sheet"] = 2;
                    oOptions.Value["Custom_End_Sheet"] = 4;

                }
            }
            catch (Exception)
            {

                //throw;
            }

            //string filePath = oDocument.FullFileName;
            //filePath = filePath.Substring(0, filePath.Length - 4);
            string NameDocument = oDocument.DisplayName;
            NameDocument = NameDocument.Substring(0, NameDocument.Length - 4);
            //Set the destination file name
            //oDataMedium.FileName = filePath + ".pdf";

            oDataMedium.FileName = OutPath.Text + "\\" + NameDocument + ".pdf";

            //Publish document.
            PDFAddin.SaveCopyAs(oDocument, oContext, oOptions, oDataMedium);

            /*
            <Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">

              <PropertyGroup>
                <OutputType>WinExe</OutputType>
                <TargetFramework>netcoreapp3.1</TargetFramework>
                <RootNamespace>idw_to_pdf</RootNamespace>
                <UseWindowsForms>true</UseWindowsForms>
              </PropertyGroup>

              <ItemGroup>
                <Reference Include="Autodesk.Inventor.Interop">
                  <HintPath>..\..\..\..\..\..\Windows\Microsoft.NET\assembly\GAC_MSIL\Autodesk.Inventor.Interop\v4.0_26.1.0.0__d84147f8b4276564\autodesk.inventor.interop.dll</HintPath>
                </Reference>
              </ItemGroup>

            </Project> 
            */

        }

        private void OutPath_TextChanged(object sender, EventArgs e)
        {
            if (OutPath.Text == "")
            {
                OutPath.Text = @"C:\idw_to_pdf\";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            selectFolder(InPath, e);
            OutPath.Text = InPath.Text + @"\idw_to_pdf";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            selectFolder(OutPath, e);
        }

        void selectFolder (TextBox s, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult dialogResult = fbd.ShowDialog();
            if (fbd.SelectedPath != null)
            {
                s.Text = fbd.SelectedPath;
            }
            else
            {
                s.Text = @"C:\";
            }
        }
    }
}
