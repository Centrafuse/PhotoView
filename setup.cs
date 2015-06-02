using System;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using centrafuse.Plugins;

namespace photoview
{
	/*
	 * Setup class inherits from CFSetup
	 * so that it will not show up as a seperate
	 * plugin, but a dialog within a plugin.
	 * It uses the standard setup screens from the
	 * main application
	*/
	public class setup : CFSetup
	{
		public setup(CFPlugin ownerplugin)
		{
            CF_initSetup(ownerplugin, 1, 1);
            this.CF_updateText("TITLE", this.pluginLang.ReadField("/APPLANG/SETUP/HEADER"));
		}


        public override void CF_setupReadSettings(int currentpage, bool advanced)
        {
            try
            {
                int i = CFSetupButton.One;

                if (advanced)
                {
                    /*******************************************************************************************/
                    /*****  ADVANCED SETTINGS - PAGE 1  ********************************************************/
                    /*******************************************************************************************/
                    if (currentpage == 1)
                    {
                        /*
                         * this.CF_updateText("LABEL1", this.pluginLang.ReadField("/APPLANG/SETUP/LABEL1"));
						this.CF_updateText("LABEL2", this.pluginLang.ReadField("/APPLANG/SETUP/LABEL2"));
						this.CF_updateText("LABEL3", "");
						this.CF_updateText("LABEL4", "");
						this.CF_updateText("LABEL5", "");
						this.CF_updateText("LABEL6", "");
						this.CF_updateText("LABEL7", "");
						this.CF_updateText("LABEL8", "");

						this.CF_updateButtonText("BUTTON1", this.pluginLang.ReadField("/APPLANG/PHOTOVIEW/DISPLAYNAME"));

						if(this.pluginConfig.ReadField("/APPCONFIG/MEDIAPATHLOC") == "")
							this.CF_updateButtonText("BUTTON2", LanguageReader.GetText("APPLANG/GENERIC/NONE"));
						else
							this.CF_updateButtonText("BUTTON2", this.pluginConfig.ReadField("/APPCONFIG/MEDIAPATHLOC"));
						break;
                         * */

                        // TEXT BUTTONS (1-4)
                        ButtonHandler[i] = new CFSetupHandler(SetDisplayName);
                        ButtonText[i] = LanguageReader.GetText("/APPLANG/SETUP/DISPLAYNAME");
                        ButtonValue[i++] = pluginLang.ReadField("/APPLANG/PHOTOVIEW/DISPLAYNAME");

                        //ButtonHandler[i] = new CFSetupHandler(SetLocalMediaPath);
                        //ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/PHOTOPATH");
                        //ButtonValue[i++] = pluginConfig.ReadField("/APPCONFIG/MEDIAPATHLOC") == "" ? LanguageReader.GetText("APPLANG/GENERIC/NONE") : pluginConfig.ReadField("/APPCONFIG/MEDIAPATHLOC");

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        // BOOL BUTTONS (5-8)

                        //ButtonHandler[i] = new CFSetupHandler(SetAutoStart);
                        //ButtonText[i] = pluginLang.ReadField("/APPLANG/SETUP/AUTOSTART");
                        //ButtonValue[i++] = configxml.SelectSingleNode("/APPCONFIG/AUTOSTART").InnerText;
                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";
                    }
                }
                else
                {
                    /*******************************************************************************************/
                    /*****  BASIC SETTINGS - PAGE 1  ***********************************************************/
                    /*******************************************************************************************/
                    if (currentpage == 1)
                    {
                        // TEXT BUTTONS (1-4)
                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        // BOOL BUTTONS (5-8)

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";

                        ButtonHandler[i] = null;
                        ButtonText[i] = "";
                        ButtonValue[i++] = "";
                    }
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


#region Button Clicks

		private void SetDisplayName(ref object value)
        {
            try
            {
                /*
                 * Launches system OSK dialog, retrieves the results,
                 * and stores them in the configuration XML.
                */
                string resultvalue, resulttext;
                if (this.CF_systemDisplayDialog(CF_Dialogs.OSK, LanguageReader.GetText("/APPLANG/SETUP/ENTERDISPLAYNAME"), ButtonValue[(int)value], out resultvalue, out resulttext) == DialogResult.OK)
                {
                    ButtonValue[(int)value] = resultvalue;
                    pluginLang.WriteField("/APPLANG/PHOTOVIEW/DISPLAYNAME", resultvalue);
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

        private void SetLocalMediaPath(ref object value)
        {
            try
            {
                string resultvalue, resulttext;
                //object resultobject ;
                // Launches system OSK dialog, retrieves the results,
                // and stores them in the configuration XML.
                //CFControls.CFListViewItem[] lbi = new centrafuse.Plugins.CFControls.CFListViewItem[1] ;
                //lbi[0].Text = "Testing the list box" ;
                //if(this.CF_systemDisplayDialog(CF_Dialogs.FileBrowser, this.pluginLang.ReadField("/APPLANG/SETUP/BUTTON3TEXT"), "SYNC FOLDERS", out resultvalue, out resulttext, false, true, true, true) == DialogResult.OK) {
                //	this.CF_updateButtonText("BUTTON3", resultvalue);
                //	configxml.SelectSingleNode("/APPCONFIG/MEDIAPATHLOC").InnerText = HttpUtility.HtmlEncode(resultvalue);
                //}
                //if(this.CF_systemDisplayDialog(CF_Dialogs.FileBrowser, this.pluginLang.ReadField("/APPLANG/SETUP/BUTTON3TEXT"), "", "", out resultvalue, out resulttext, out resultobject, null, true, true, false, false, false, false, 1) == DialogResult.OK) {
                DialogResult dr;
                if (this.CF_getButtonText("BUTTON2") != LanguageReader.GetText("APPLANG/GENERIC/NONE"))
                {
                    if (Directory.Exists(this.CF_getButtonText("BUTTON2")))
                        dr = this.CF_systemDisplayDialog(CF_Dialogs.FileBrowser, this.pluginLang.ReadField("/APPLANG/SETUP/ENTERPHOTOPATH"), this.CF_getButtonText("BUTTON2"), out resultvalue, out resulttext, false, true, true, true);
                    else
                        dr = this.CF_systemDisplayDialog(CF_Dialogs.FileBrowser, this.pluginLang.ReadField("/APPLANG/SETUP/ENTERPHOTOPATH"), "", out resultvalue, out resulttext, false, true, true, true);
                }
                else dr = this.CF_systemDisplayDialog(CF_Dialogs.FileBrowser, this.pluginLang.ReadField("/APPLANG/SETUP/ENTERPHOTOPATH"), "", out resultvalue, out resulttext, false, true, true, true);
                if (dr == DialogResult.OK)
                {
                    ButtonValue[(int)value] = resultvalue;
                    pluginConfig.WriteField("/APPCONFIG/MEDIAPATHLOC", resultvalue);
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

		#endregion
	}
}

