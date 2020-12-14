using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Writeleft
{
	public partial class TextEditorForm : Form
	{
		public string filename = "";
		public bool isDirty;

		/// <summary>
		/// Fired when the user requests the file to be written to disk. Argument is the filename.
		/// </summary>
		public Action<string> OnSaveRequested;

		/// <summary>
		/// Initialize a new TextEditorForm.
		/// </summary>
		public TextEditorForm()
		{
			InitializeComponent();

			Fctb_Main.AllowInsertRemoveLines = true;
			Fctb_Main.Focus();
		}

		/// <summary>
		/// Save the file to disk. Open a save file dialog if the file does not already exist on disk.
		/// </summary>
		private void SaveFile()
		{
			if (filename.Length == 0) OpenSaveDialog();
			if (filename.Length == 0) return;

			OnSaveRequested?.Invoke(filename);

			isDirty = false;
		}

		/// <summary>
		/// Display a save file dialog to the user and set <see cref="filename"/> to the result.
		/// </summary>
		private void OpenSaveDialog()
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.FileName = filename;

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				filename = sfd.FileName;
			}
		}

		/// <summary>
		/// Set the title bar text using the user's current settings and add a dirty asterisk if required.
		/// </summary>
		private void UpdateTitleBarText()
		{
			Text = $"{filename}{(isDirty ? "*" : "")}";
		}

		/// <summary>
		/// Exit Writeleft.
		/// </summary>
		private void TsBtn_File_Exit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		/// <summary>
		/// Ask the user if they really meant to close the application.
		/// </summary>
		private void TextEditorForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(Fctb_Main.Text))
				return;

			e.Cancel = MessageBox.Show("Unsaved work will be lost forever. Do you want to save this file before closing?",
				"Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

			if (e.Cancel) SaveFile();
		}

		/// <summary>
		/// Start a new instance of Writeleft.
		/// </summary>
		private void TsBtn_File_NewWindow_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
		}

		/// <summary>
		/// Clear <see cref="Fctb_Main"/> and <see cref="filename"/>, but ask the user to save their current file first.
		/// </summary>
		private void TsBtn_File_New_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(Fctb_Main.Text))
				return;

			if (MessageBox.Show("Unsaved work will be lost forever. Do you want to save this file?",
				"Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				SaveFile();
				return;
			}

			isDirty = false;
			filename = "";
			Fctb_Main.Text = "";
			UpdateTitleBarText();
		}

		/// <summary>
		/// Set <see cref="isDirty"/> to true.
		/// </summary>
		private void Fctb_Main_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
		{
			if (!isDirty)
			{
				isDirty = true;
				UpdateTitleBarText();
			}
		}

		/// <summary>
		/// Ask the user to open a file.
		/// </summary>
		private void TsBtn_File_Open_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(Fctb_Main.Text) && MessageBox.Show("Unsaved work will be lost forever. Do you want to save this file?",
				"Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
			{
				SaveFile();
				return;
			}

			OpenFileDialog ofd = new OpenFileDialog();
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				filename = ofd.FileName;
				isDirty = false;
				UpdateTitleBarText();
			}
		}

		private void TsBtn_Edit_Cut_Click(object sender, EventArgs e) => Fctb_Main.Cut();
		private void TsBtn_Edit_Copy_Click(object sender, EventArgs e) => Fctb_Main.Copy();
		private void TsBtn_Edit_Undo_Click(object sender, EventArgs e) => Fctb_Main.Undo();
		private void TsBtn_Edit_Redo_Click(object sender, EventArgs e) => Fctb_Main.Redo();
		private void TsBtn_Edit_Paste_Click(object sender, EventArgs e) => Fctb_Main.Paste();
		private void TsBtn_Edit_SelectAll_Click(object sender, EventArgs e) => Fctb_Main.SelectAll();

		/// <summary>
		/// Insert long date/time at the current caret position
		/// </summary>
		private void TsBtn_Edit_Insert_LongDateTime_Click(object sender, EventArgs e)
		{
			DateTime d = DateTime.Now;
			Fctb_Main.InsertText($"{d.ToLongTimeString()}, {d.ToLongDateString()}");
		}

		/// <summary>
		/// Insert short date/time at current caret position
		/// </summary>
		private void TsBtn_Edit_Insert_ShortDateTime_Click(object sender, EventArgs e) => Fctb_Main.InsertText(DateTime.Now.ToString());

		/// <summary>
		/// Insert Unix time at the current caret position
		/// </summary>
		private void TsBtn_Edit_Insert_UnixDateTime_Click(object sender, EventArgs e) => Fctb_Main.InsertText(((int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString());

		/// <summary>
		/// Duplicate the line that the caret is currently on.
		/// </summary>
		private void TsBtn_Edit_DuplicateLine_Click(object sender, EventArgs e)
		{
			if (!Fctb_Main.Selection.IsEmpty) // don't bother duplicating a multiline selection, that's what copy/paste is for
				return;

			FastColoredTextBoxNS.Range origSel = Fctb_Main.Selection;
			string line = Fctb_Main.GetLineText(Fctb_Main.Selection.ToLine);

			Fctb_Main.Selection.Start = Fctb_Main.GetLine(Fctb_Main.Selection.ToLine).End;
			Fctb_Main.InsertText("\r\n");
			Fctb_Main.InsertText(line);
			Fctb_Main.Selection = origSel;
		}
	}
}
