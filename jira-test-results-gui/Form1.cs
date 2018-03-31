using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Atlassian.Jira;
using System.IO;
using System.Xml;

namespace jira_test_results_gui
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textNunit.Text = this.openFileDialog1.FileName;
            }

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.url = this.textURL.Text;
            Properties.Settings.Default.user = this.textUser.Text;
            Properties.Settings.Default.pwd = this.textPassword.Text;
            Properties.Settings.Default.nunit = this.textNunit.Text;
            Properties.Settings.Default.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.textURL.Text = Properties.Settings.Default.url;
            this.textUser.Text = Properties.Settings.Default.user;
            this.textPassword.Text = Properties.Settings.Default.pwd;
            this.textNunit.Text = Properties.Settings.Default.nunit;

        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            string url = this.textURL.Text;
            string username = this.textUser.Text;
            string password = this.textPassword.Text;
            string xmlPath = this.textNunit.Text;

            this.textOutput.Clear();
            this.textOutput.AppendText("Starting Processing...\r\n");

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlPath);
                // get all the xml nodes for the Features
                XmlNodeList testSuitesName = xmlDoc.GetElementsByTagName("test-suite");

                for (int i = 0; i < testSuitesName.Count; i++)
                {
                    //Features have an xml attribute of 'TestFixture'
                    if (testSuitesName[i].Attributes["type"].Value == "TestFixture")
                    {
                        string featureName = testSuitesName[i].Attributes["name"].Value.Replace("Feature", "").Replace("_", "-");

                        try
                        {
                            //set the Jira 'Test Status' field value
                            var jira = Jira.CreateRestClient(url, username, password);
                            var issue = jira.Issues.GetIssueAsync(featureName).Result;
                            issue["Test Status"] = testSuitesName[i].Attributes["result"].Value;
                            issue.SaveChanges();
                            this.textOutput.AppendText("JIRA Issue: " + featureName + " Test Status Updated to: " + testSuitesName[i].Attributes["result"].Value + "\r\n");
                        }
                        catch (Exception err)
                        {
                            this.textOutput.AppendText(err.Message + " Error with issue: " + featureName + "\r\n");
                        }
                    }
                }
            this.textOutput.AppendText("Finished!\r\n");

        }
    }

}
