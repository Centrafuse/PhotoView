using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text;
using System.Threading;
using centrafuse.Plugins;
using System.Data;

using CFControlsExtender.Base;
using CFControlsExtender.Imaging;
using CFControlsExtender.ItemsBuilder;
using CFControlsExtender.Listview;

namespace photoview
{
	public class photoview : CFPlugin
	{
		// Global variables
		private setup mysetup;
        private bool restartslideshow = false;
		private System.Windows.Forms.Timer updateTimer;
		private static string mediapathloc = "";
        public static bool inslideshow = false;
        private slideshow slideform = null;
        private CFControls.CFAdvancedList listPics;
        private BindingSource picBindingSource;
        private DataTable dtPics;
        private System.Windows.Forms.Timer pagingTimer;
        private CFControls.CFListView.PagingDirection pagingDirection = CFControls.CFListView.PagingDirection.DOWN;

        private bool deletemode = false;
        delegate void voidDelegate();
        delegate void stringDelegate(string str);

		// Plugin constructor.
		public photoview()
		{
			slideform = new slideshow();
            slideform.Owner = this.Owner;
            slideform.TopMost = true;
		}

#region CFPlugin methods

		// Initializes the plugin.  This is called from the main application
		// when the plugin is first loaded.
		public override void CF_pluginInit()
		{
			try
            {
                this.CF_params.hasBasicSettings = false;
                this.CF_params.supportsRearScreen = true;
                this.CF_params.pluginShowBusyOnLoad = true;

                this.CF3_initPlugin("PhotoView", true);

				// Loads Settings
				LoadSettings();

                this.picBindingSource = new BindingSource();
                dtPics = new DataTable("Pics");
                dtPics.Columns.Add("DisplayName", System.Type.GetType("System.String"));
                dtPics.Columns.Add("ThumbPath", System.Type.GetType("System.String"));
                dtPics.Columns.Add("ImagePath", System.Type.GetType("System.String"));

                this.pagingTimer = new System.Windows.Forms.Timer();
                this.pagingTimer.Interval = 850;
                this.pagingTimer.Enabled = false;
                this.pagingTimer.Tick += new EventHandler(pagingTimer_Tick);

                // All controls should be created or setup in CF_localskinsetup.
                // This method is also called when the resolution or skin has changed.
                this.CF_localskinsetup();

                // Create timer to update weather data
				updateTimer = new System.Windows.Forms.Timer();
				updateTimer.Interval = 4000 ;
				updateTimer.Tick += new EventHandler(updateTimer_Tick);
				updateTimer.Enabled = false;

                this.CF_events.CFPowerModeChanged += new CFPowerModeChangedEventHandler(photoview_CF_Event_CFPowerModeChanged);
                this.VisibleChanged += new EventHandler(PhotoView_VisibleChanged);
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

		
        public override void CF_localskinsetup()
		{
			try 
            {
                CF3_initSection("PhotoView");
                
                this.listPics = this.advancedlistArray[CF_getAdvancedListID("MAINPANEL")];

                if (this.listPics != null)
                {
                    this.listPics.Click += new EventHandler<ThrowScrollPanelMouseEventArgs>(pictureLst_Click);
                    this.listPics.SelectedIndexChanged += new EventHandler<ItemArgs>(pictureLst_SelectedIndexChanged);
                    this.listPics.DataBinding = picBindingSource;
                    this.picBindingSource.DataSource = this.dtPics;
                    
                    this.listPics.TemplateID = "default";
                    this.listPics.Update();
                }

				UpdateListCount();

                this.slideform.Bounds = CFTools.AllScreens[this.CF_displayHooks.displayNumber - 1].Bounds;
                this.slideform.picVis.Bounds = CFTools.AllScreens[this.CF_displayHooks.displayNumber - 1].Bounds;
#if WindowsCE
                // We don't have email in CE.  Disable the button for now.
                CF_setButtonEnableFlag("Centrafuse.PhotoView.Email", false);
#endif
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


        public override bool CF_pluginCMLCommand(string command, string[] strparams, CF_ButtonState state, int zone)
        {
            CFTools.writeLog("PHOTO", "CF_pluginCMLCommand", "action = " + command + ", state = " + state);

            try
            {
                command = command.ToLower().Replace("centrafuse.", "").Trim();

                switch (command)
                {
                    case "photoview.email":
                        if (state >= CF_ButtonState.Click)
                            email_Click();
                        return true;
                    case "photoview.load":
                        if (state >= CF_ButtonState.Click)
                            load_Click();
                        return true;
                    case "photoview.slideshow":
                        if (state >= CF_ButtonState.Click)
                            slideshow_Click();
                        return true;
                    case "photoview.view":
                        if (state >= CF_ButtonState.Click)
                            pictureLst_Click(true);
                        return true;
                    case "photoview.rotate":
                        if (state >= CF_ButtonState.Click)
                            rotate_Click();
                        return true;
                    case "photoview.pageup":
                        if (state == CF_ButtonState.Down)
                        {
                            up_Click();
                            pagingTimer.Enabled = true;
                        }
                        else if (state == CF_ButtonState.Click)
                            pagingTimer.Enabled = false;
                        return true;
                    case "photoview.pagedown":
                        if (state == CF_ButtonState.Down)
                        {
                            down_Click();
                            pagingTimer.Enabled = true;
                        }
                        else if (state == CF_ButtonState.Click)
                            pagingTimer.Enabled = false;
                        return true;
                    case "photoview.delete":
                        if (state >= CF_ButtonState.Click)
                            deletePicture(true);
                        return true;
                    case "photoview.deletemode":
                        if (state >= CF_ButtonState.Click)
                            ToggleDeleteMode();
                        return true;
                    case "photoview.setwallpaper":
                        if (state >= CF_ButtonState.Click)
                            SetWallPaper();
                        return true;
                }
            }
            catch { }

            return false;
        }


		// This is called by the system when it exits or the plugin has been deleted.
		public override void CF_pluginClose()
		{
            try
            {
                if (this.slideform != null)
                    this.slideform.Dispose();
                this.Dispose();
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		// This is called by the system when a button with this plugin action has been clicked.
		public override void CF_pluginShow()
		{
            try
            {
                if (photoview.mediapathloc != "" && photoview.mediapathloc == this.CF_getConfigSetting(CF_ConfigSettings.PicturePath))
                {
                    if (dtPics.Rows.Count == 0)
                    {
                        dtPics.Rows.Clear();

                        string[] foldersplit = photoview.mediapathloc.Split('|');
                        for (int i = 0; i < foldersplit.Length; i++)
                        {
                            ParseFolders(foldersplit[i]);
                        }
                    }
                    this.Visible = true;
                }
                else
                {
                    string picturepath = "";
                    if (this.CF_getConfigSetting(CF_ConfigSettings.PicturePath) != "")
                        picturepath = this.CF_getConfigSetting(CF_ConfigSettings.PicturePath);
                    else
                        picturepath = CFTools.SystemPicturePath;

                    if (picturepath != null && picturepath != "")
                    {
                        this.pluginConfig.WriteField("/APPCONFIG/MEDIAPATHLOC", picturepath);
                        photoview.mediapathloc = picturepath;

                        dtPics.Rows.Clear();

                        string[] foldersplit = photoview.mediapathloc.Split('|');
                        for (int i = 0; i < foldersplit.Length; i++)
                        {
                            ParseFolders(foldersplit[i]);
                        }

                        this.Visible = true;
                    }
                    else
                    {
                        this.CF_systemCommand(CF_Actions.HIDEINFO, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
                        this.Visible = true;

                        this.CF_systemDisplayDialog(CF_Dialogs.OkBox, this.pluginLang.ReadField("/APPLANG/PHOTOVIEW/NOPATHSET"));
                        this.CF_systemCommand(CF_Actions.EXTAPPCLOSE, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
                    }
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}


		// This is called by the system when the plugin setup is clicked.
		public override DialogResult CF_pluginShowSetup()
		{
			// Return DialogResult.OK for the main application
			// to update from plugin changes.
			DialogResult returnvalue = DialogResult.Cancel;
			try
            {
				/* Creates a new plugin setup instance.
				 * If you create a CFDialog or CFSetup you must
				 * set its MainForm property to the main plugins
				 * MainForm property. */
				mysetup = new setup(this);
				returnvalue = mysetup.ShowDialog(this);
				if(returnvalue == DialogResult.OK) 
                {
					// Reloads plugin configuration.
					LoadSettings();
				}
				mysetup.Close();
				mysetup = null;
			}
			catch(Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

			return returnvalue;
		}


        public override string CF_pluginDefaultConfig()
        {
            return "<APPCONFIG>" + Environment.NewLine +
                    "    <SKIN>Clean</SKIN>" + Environment.NewLine +
                    "    <APPLANG>English</APPLANG>" + Environment.NewLine +
                    "    <MEDIAPATHLOC></MEDIAPATHLOC>" + Environment.NewLine +
                    "</APPCONFIG>" + Environment.NewLine;
        }

#endregion

#region System Functions

		public void LoadSettings()
		{
			// The display name is shown in the application to represent the plugin.  This sets the display name from the configuration file.
			this.CF_params.displayName = this.pluginLang.ReadField("/APPLANG/PHOTOVIEW/DISPLAYNAME");
            this.CF_params.settingsDisplayDesc = this.pluginLang.ReadField("/APPLANG/PHOTOVIEW/DESCRIPTION");

			photoview.mediapathloc = this.pluginConfig.ReadField("/APPCONFIG/MEDIAPATHLOC");
		}

		private void ProcessDirFiles(string dirName)
		{
            try
            {
				DirectoryInfo dir = new DirectoryInfo(dirName) ;
				if (!dir.Exists)
					return ;

				FileInfo[] files = dir.GetFiles();

				foreach (FileInfo file in files) 
				{
                    if (CFTools.checkExtension(Path.GetExtension(file.FullName), CFTools.ExtensionType.Pictures) && Path.GetExtension(file.FullName) != ".thmb")
                    {
                        string thumbfilename = Path.Combine(Path.GetDirectoryName(file.FullName), Path.GetFileNameWithoutExtension(file.FullName) + ".thmb");
                        if (!File.Exists(thumbfilename))
                            CFTools.CreateImageThumb(file.FullName);

                        DataRow row = null;

                        row = dtPics.NewRow();
                        row["DisplayName"] = Path.GetFileNameWithoutExtension(file.FullName);
                        row["ThumbPath"] = thumbfilename;
                        row["ImagePath"] = file.FullName;
                        dtPics.Rows.Add(row);
                    }
				}

                UpdateListCount();
                listPics.Update();
			}
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
			return ;
		}


        public void ParseFolders(string rootFolder)
		{
            try
            {
                this.CF_updateText("THUMBSHEADER", Path.GetDirectoryName(rootFolder));

                string[] subdirs = Directory.GetDirectories(rootFolder);
                if (subdirs.Length == 0)
                {
                    // process this folder
                    ProcessDirFiles(rootFolder);
                }
                else
                {
                    for (int i = 0; i < subdirs.Length; i++)
                        ParseFolders(subdirs[i]);
                    // process this folder
                    ProcessDirFiles(rootFolder);
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
			return ;
		}


        private void UpdateListCount()
        {
            try
            {
                if (dtPics.Rows.Count > 0)
                    this.CF_updateText("THUMBSLISTCOUNT", (picBindingSource.Position + 1) + "/" + dtPics.Rows.Count);
                else
                    this.CF_updateText("THUMBSLISTCOUNT", "0/0");
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

#endregion

#region Click Events

        private void pictureLst_Click(object sender, ThrowScrollPanelMouseEventArgs e)
        {
            pictureLst_Click(false);
        }

        private void pictureLst_Click(bool loadimage)
        {
            if (deletemode && !loadimage)
            {
                deletePicture(true);
            }
            else if (!deletemode && loadimage)
            {
                string strImage = GetSelectedImagePath();

                try
                {
#if !WindowsCE
                    if (strImage == "")
                        return;

                    FileStream fullimageStream = new FileStream(strImage, FileMode.Open, FileAccess.Read);
                    Image newimage = Image.FromStream(fullimageStream);
                    fullimageStream.Close();
#else
                    Image newimage = CFTools.ImageFromFile(strImage);
#endif

                    slideform.updateImage(newimage);
                    slideform.Visible = true;
                    //slideShow.Focus();
                }
                catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
            }
        }


        private void deletePicture(bool confirm)
        {
            try
            {
                if (!confirm || (confirm && this.CF_systemDisplayDialog(CF_Dialogs.YesNo, this.pluginLang.ReadField("/APPLANG/PHOTOVIEW/DELETECONFIRM")) == DialogResult.OK))
                {
                    string strImage = GetSelectedImagePath();

                    if (strImage == "")
                        return;

                    if (File.Exists(strImage))
                    {
                        File.Delete(strImage);
                        this.listPics.Update();

                        this.picBindingSource.CurrencyManager.List.RemoveAt(this.picBindingSource.CurrencyManager.Position);
                    }
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }               
        }

        private void pictureLst_SelectedIndexChanged(object sender, ItemArgs e)
        {
            UpdateListCount();
        }

		private void up_Click()
		{
            pagingDirection = CFControls.CFListView.PagingDirection.UP;
            this.listPics.PageUp();
		}


        private void down_Click()
        {
            pagingDirection = CFControls.CFListView.PagingDirection.DOWN;
            this.listPics.PageDown();
        }


        private void SetWallPaper()
        {
            try
            {
                string strImage = GetSelectedImagePath();
                this.CF3_setMainBackground(strImage);
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


        private void ToggleDeleteMode()
        {
            try
            {
                deletemode = !deletemode;
                int currentposition = this.picBindingSource.CurrencyManager.Position;

                if (deletemode)
                {
                    this.listPics.TemplateID = "DeleteMode";
                    CF_setButtonOn("Centrafuse.PhotoView.Delete");
                }
                else
                {
                    this.listPics.TemplateID = "Default";
                    CF_setButtonOff("Centrafuse.PhotoView.Delete");
                }

                this.listPics.Update();
                this.picBindingSource.CurrencyManager.Position = currentposition;
                this.listPics.Update();
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

		private void slideshow_Click()
		{
            try
            {
                photoview.inslideshow = true;
                this.slideform.Visible = true;

                string strFirstPhoto = GetSelectedImagePath();

                if (strFirstPhoto == "")
                {
                    if (dtPics.Rows.Count <= 0)
                        return;

                    strFirstPhoto = GetImagePathAtIndex(0);
                    if (strFirstPhoto == "")
                        return;     // Should not happen
                }

#if !WindowsCE
                FileStream fullimageStream = new FileStream(strFirstPhoto, FileMode.Open, FileAccess.Read);
                Image newimage = Image.FromStream(fullimageStream);
                fullimageStream.Close();
#else
                Image newimage = CFTools.ImageFromFile(strFirstPhoto);
#endif

                slideform.updateImage(newimage);
                updateTimer.Enabled = true;
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
		}

        public bool RotateImage(string strFile)
        {
            try
            {
#if !WindowsCE
                Image img = CFTools.ImageFromFile(strFile);
                img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                img.Save(strFile);

                string thumbfilename = Path.Combine(Path.GetDirectoryName(strFile), Path.GetFileNameWithoutExtension(strFile) + ".thmb");

                if (File.Exists(thumbfilename))
                {
                    Image imgthmb = Image.FromFile(thumbfilename);
                    imgthmb.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    imgthmb.Save(thumbfilename);
                }

                return true;
#else
                ImageFormat imageFormat;
                if (strFile.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                    imageFormat = ImageFormat.Jpeg;
                else if (strFile.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                    imageFormat = ImageFormat.Bmp;
                else if (strFile.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    imageFormat = ImageFormat.Png;
                else if (strFile.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                    imageFormat = ImageFormat.Gif;
                else
                    return false;

                Bitmap bmp = new Bitmap(strFile);
                Image rotated = Rotator.RotateImage(90, bmp);
                bmp.Dispose();
                rotated.Save(strFile, imageFormat);

                string thumbfilename = Path.Combine(Path.GetDirectoryName(strFile), Path.GetFileNameWithoutExtension(strFile) + ".thmb");
                if (File.Exists(thumbfilename))
                {
                    bmp = new Bitmap(thumbfilename);
                    rotated = Rotator.RotateImage(90, bmp);
                    bmp.Dispose();
                    rotated.Save(thumbfilename, imageFormat);
                }               

                return true;
#endif
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
            
            return false;
        }

        private void rotate_Click()
        {
            try
            {
                string strImage = GetSelectedImagePath();
                if (strImage == "")
                    return;

                this.RotateImage(strImage);
                listPics.Update();
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


        private void email_Click()
        {
            try
            {
                if (dtPics.Rows.Count == 0)
                    return;

                if (GetSelectedImagePath() == "")
                    return; // Should not happen

                CF_systemCommand(CF_Actions.PLUGIN, "EMAIL", "SENDPICFILE", GetSelectedImagePath(), this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


        private void load_Click()
        {
            try
            {
                string resultvalue, resulttext;
                DialogResult dr = this.CF_systemDisplayDialog(CF_Dialogs.FileBrowser, this.pluginLang.ReadField("/APPLANG/SETUP/ENTERPHOTOPATH"), photoview.mediapathloc, out resultvalue, out resulttext, false, true, true, true);
                if (dr == DialogResult.OK)
                {
                    CF_systemCommand(CF_Actions.SHOWINFO, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
                    dtPics.Rows.Clear();

                    ParseFolders(resultvalue);
                    this.pluginConfig.WriteField("MEDIAPATHLOC", resultvalue);
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

            CF_systemCommand(CF_Actions.HIDEINFO, this.CF_displayHooks.rearScreen ? "REARSCREEN" : "");
        }

#endregion

#region CF Callbacks

		private void updateTimer_Tick(object sender, EventArgs e)
		{
            try
            {
                if (!photoview.inslideshow)
                {
                    updateTimer.Enabled = false;
                    return;
                }

                if (picBindingSource.Position < dtPics.Rows.Count - 1)
                    picBindingSource.Position++;
                else
                    picBindingSource.Position = 0;

                if (GetSelectedImagePath() == "")
                    return; // Should not happen

#if !WindowsCE
                FileStream fullimageStream = new FileStream(GetSelectedImagePath(), FileMode.Open, FileAccess.Read);
                Image newimage = Image.FromStream(fullimageStream);
                fullimageStream.Close();
#else
                Image newimage = CFTools.ImageFromFile(GetSelectedImagePath());
#endif

                slideform.updateImage(newimage);
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

            return;
		}


        private void PhotoView_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
            {
                if (photoview.inslideshow)
                {
                    updateTimer.Enabled = false;
                    photoview.inslideshow = false;
                    this.slideform.Visible = false;
                }
            }
        }

        private void photoview_CF_Event_CFPowerModeChanged(object sender, CFPlugin.CFPowerModeChangedEventArgs e)
        {
            try
            {
                if (e.Mode == CFPowerModes.Suspend)
                {
                    if (updateTimer.Enabled == true)
                        restartslideshow = true;

                    updateTimer.Enabled = false;
                }
                else if (e.Mode == CFPowerModes.Resume)
                {
                    if (restartslideshow)
                    {
                        updateTimer.Enabled = true;
                        restartslideshow = false;
                    }
                }
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

#endregion

        private void pagingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (pagingDirection == CFControls.CFListView.PagingDirection.DOWN)
                    this.listPics.PageDown();
                else
                    this.listPics.PageUp();
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }

        private string GetSelectedImagePath()
        {
            try
            {
                if (listPics.SelectedItems.Count <= 0)
                    return "";

                int nSelected = listPics.SelectedItems[0];

                if (nSelected < 0)
                    return "";

                return GetImagePathAtIndex(nSelected);
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

            return "";
        }

        private string GetImagePathAtIndex(int nIndex)
        {
            try
            {
                if (nIndex < 0)
                    return "";

                if (nIndex > dtPics.Rows.Count - 1)
                    return "";

                return dtPics.Rows[nIndex]["ImagePath"].ToString();
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }

            return "";
        }
	}
}