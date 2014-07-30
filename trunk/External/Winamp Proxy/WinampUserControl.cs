// WinampUserControl.cs
// Written by David J. Stein, Esq., 9/5/2008
// See "License.html" for terms and conditions of use.
//
// This class is an extension of the WinampProxy class. It implements a very simple
// user control with some of the basic functions of the proxy (volume monitoring,
// play/pause, etc.) It's useful if you just need a quick-and-easy Winamp drop-in
// control for a project, or if you want to extend it to produce a more sophisticated
// control for a project.

#region Using declarations

using System;
using System.Windows.Forms;

#endregion

namespace WinampProxyNS {

  public partial class WinampUserControl : UserControl {

    #region Public members

    public WinampProxy proxy = new WinampProxy();

    #endregion

    #region Initialization code

    public WinampUserControl() {
      InitializeComponent();
    }

    private void WinampUserControl_Load(object sender, EventArgs e) {
      WinampInterface.StartWinamp();
    }

    #endregion

    #region Button-click event handlers

    private void btnEnqueue_Click(object sender, EventArgs e) {
      if (proxy == null)
        proxy = new WinampProxy();
      OpenFileDialog o = new OpenFileDialog();
      o.Filter = "MP3 Files (*.mp3)|*.mp3|WAV Files (*.wav)|*.wav|All Files (*.*)|*.*";
      o.Multiselect = true;
      if (o.ShowDialog() == DialogResult.OK) {
        foreach (string strFile in o.FileNames)
          WinampInterface.EnqueueTrack(strFile);
      }
    }

    private void btnBack_Click(object sender, EventArgs e) {
      WinampInterface.PrevTrack();
    }

    private void btnJumpBack_Click(object sender, EventArgs e) {
      WinampInterface.Rewind5Sec();
    }

    private void btnPlayPause_Click(object sender, EventArgs e) {
      if (proxy.enumWinampStatus == WinampStatusEnum.Playing)
        WinampInterface.Pause();
      else
        WinampInterface.Play();
    }

    private void btnJumpForward_Click(object sender, EventArgs e) {
      WinampInterface.Forward5Sec();
    }

    private void btnForward_Click(object sender, EventArgs e) {
      WinampInterface.NextTrack();
    }

    private void tbVolume_Scroll(object sender, EventArgs e) {
      if (tbVolume.Value != proxy.iVolume)
        proxy.SetVolume((int) tbVolume.Value);
    }

    #endregion

    #region Polling timer function (for updating track position and volume)

    private void timerUpdate_Tick(object sender, EventArgs e) {
      txtTrackName.Text = proxy.strCurrentTrackName;
      if (proxy.iCurrentTrackDuration != 0)
        pbProgress.Value = Math.Max(0, Math.Min(100, (proxy.iCurrentTrackPosition * 100 / proxy.iCurrentTrackDuration)));
      if (proxy.enumWinampStatus == WinampStatusEnum.Playing)
        btnPlayPause.Text = ";";  // this is the WebDings character for the "Pause" button
      else
        btnPlayPause.Text = "4";  // this is the WebDings character for the "Play" button
      if ((proxy.iVolume >= 0) && (tbVolume.Value != proxy.iVolume))
        tbVolume.Value = proxy.iVolume;
      // sync timer frequency with proxy frequency
      timerUpdate.Interval = proxy.GetPollingFrequency();
    }

    #endregion

  }

}
