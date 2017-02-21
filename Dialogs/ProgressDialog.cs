namespace Win.Ui.Dialogs
{
	using System;
    using System.ComponentModel;
	using System.Windows.Forms;

	using DevTools.Application.UI.Interface;
	
	public class ProgressDialog : Form, IProgressBar
	{
		private ProgressBar progressbar1;
		public Exception ActionException { get; private set; }

		public ProgressDialog (string title) : base()
		{
			//this.WindowPosition = WindowPosition.CenterOnParent;

			progressbar1 = new ProgressBar ();
            progressbar1.Dock = DockStyle.Fill;
			//this.VBox.AddLabel ("progress", 0, true);
			this.Controls.Add (progressbar1);
			//this.VBox.AddLabel ("progress", 2, true);

			//if ((this.Child != null)) {
			//	this.Child.ShowAll ();
			//}

			this.Text = title;
			this.Width = 725;
			this.Height = 200;
			//this.Modal = true;

			this.ActionException = null;

			//this.Show ();
		}

		private BackgroundWorker _bw = null;

		public void ExecuteAction(System.Action<IProgressBar> action) {

			_bw = new BackgroundWorker();
			
			// define the event handlers
			_bw.DoWork += (sender, args) => {
                //try {
                    action(this);
                //} catch (Exception ex) {
                //    //OnFinished(sender, new RunWorkerCompletedEventArgs(null, ex, false));
                //}
			};
			_bw.RunWorkerCompleted += OnFinished;
            
			_bw.WorkerReportsProgress = true;
			_bw.ProgressChanged += OnWorkerProgress;

			_bw.RunWorkerAsync(); // starts the background worker
		}

		private void OnFinished(object sender, RunWorkerCompletedEventArgs args) {
			ActionException = args.Error;
			this.Close ();
		}

		protected void OnWorkerProgress(object sender, ProgressChangedEventArgs args) {
			progressbar1.Value = args.ProgressPercentage;
		}

		#region IProgressBar implementation
		public void ShowProgress (double fractionComplete)
		{
			_bw.ReportProgress (Convert.ToInt32( 100 * fractionComplete));
		}
		#endregion
	}
}

