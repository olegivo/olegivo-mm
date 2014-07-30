// WinampProxy.cs
// Written by David J. Stein, Esq., 9/5/2008
// See "License.html" for terms and conditions of use.
//
// This class is an extension of the WinampInterface class. It adds some functionality by polling
// the state of the Winamp interface via an internal polling timer. The information can then be
// accessed on an as-needed basis by hosting applications. Moreover (and more interestingly), this
// class offers some subscribable events that are raised upon significant state changes of Winamp
// (track changes, start/stop, etc.)

#region Using declarations

using System;
using System.Collections.Generic;
using System.Timers;

#endregion

namespace WinampProxyNS {

  public class WinampProxy : WinampInterface {

    #region Delegate declaration for event subscription

    public delegate void WinampProxyEventDelegate();

    #endregion

    #region Internal variables and initialization code

    public WinampProxy() {
      timerProxy.Interval = 100;  // timer interval
      timerProxy.AutoReset = true;
      timerProxy.Elapsed += new ElapsedEventHandler(timerProxy_Tick);
      timerProxy.Start();
    }

    protected Timer timerProxy = new Timer();
    protected bool bPolling = false;
    protected int iRunningInternal = -1;  // -1 = we haven't checked before; 0 = no; 1 = yes
    protected WinampStatusEnum enumWinampStatusInternal = WinampStatusEnum.Unknown;
    protected bool bEnqueuePromptIssued = false;
    protected int iLastEnqueuedTrackChecked = -1;
    protected bool bVolumeKnown = false;
    protected int iVolumeInternal = 0;
    protected int iPlaylistCountInternal = 0;
    protected int iPlaylistTrackNumberInternal = 0;
    protected string strCurrentTrackNameInternal = "";
    protected int iCurrentTrackDurationInternal = 0;
    protected int iCurrentTrackPositionInternal = 0;

    #endregion

    #region Internal polling function

    protected void timerProxy_Tick(object o, ElapsedEventArgs args) {
      if (bPolling == false) {
        bPolling = true;

        // currently running info
        bool bWinampRunning = WinampInterface.IsWinampRunning();
        if (bWinampRunning) {
          if ((iRunningInternal == 0) && (eventWinampInstanceCreated != null))
            eventWinampInstanceCreated.Invoke();
          iRunningInternal = 1;
        } else {
          if ((iRunningInternal == 1) && (eventWinampInstanceClosed != null))
            eventWinampInstanceClosed.Invoke();
          iRunningInternal = 0;
        }

        if (!bWinampRunning) {  // wipe out info, since Winamp is closed
          iPlaylistCountInternal = 0;
          iPlaylistTrackNumberInternal = 0;
          enumWinampStatusInternal = WinampStatusEnum.Unknown;
          iCurrentTrackDurationInternal = 0;
          iCurrentTrackPositionInternal = 0;
          strCurrentTrackNameInternal = "";
        } else { // poll for additional info

          // status info
          WinampStatusEnum status = WinampInterface.GetWinampStatus();
          if (status != enumWinampStatusInternal) {
            if ((status == WinampStatusEnum.Playing) && (eventPlayStarted != null))
              eventPlayStarted.Invoke();
            else if ((enumWinampStatusInternal == WinampStatusEnum.Playing) && (status != WinampStatusEnum.Playing) && (eventPlayStopped != null))
              eventPlayStopped.Invoke();
            enumWinampStatusInternal = status;
          }

          // playlist size, current track index, and track position
          iPlaylistCountInternal = WinampInterface.GetPlaylistCount();
          iPlaylistTrackNumberInternal = WinampInterface.GetPlaylistPosition();
          iCurrentTrackPositionInternal = WinampInterface.GetTrackPosition();

          // current track name and duration
          string strTrackName = WinampInterface.GetTrackName();
          int iTrackDuration = WinampInterface.GetTrackLength();
          // compare this info to previous info to see if we've changed tracks
          // NOTE: We don't consider PlaylistPosition to be relevant here. The playlist can change if earlier entries get
          // deleted, etc., and we don't want to raise a false positive.
          if (((strTrackName != "Paused") && (strTrackName != "Stopped") && (strTrackName != strCurrentTrackNameInternal) || (iTrackDuration != iCurrentTrackDurationInternal)) && (iTrackDuration > 0)) {
            bEnqueuePromptIssued = false;
            strCurrentTrackNameInternal = strTrackName;
            iCurrentTrackDurationInternal = iTrackDuration;
            if ((enumWinampStatus == WinampStatusEnum.Playing) && (eventTrackChanged != null))
              eventTrackChanged.Invoke();
          }

          // enqueue prompt
          if (enumWinampStatus != WinampStatusEnum.Playing) {
            bEnqueuePromptIssued = false;
            iLastEnqueuedTrackChecked = -1;
          } else {
            if ((iPlaylistTrackNumberInternal < iPlaylistCountInternal - 1) || (iCurrentTrackDurationInternal - iCurrentTrackPositionInternal >= iEnqueuePromptMilliseconds) || (iLastEnqueuedTrackChecked != iPlaylistTrackNumberInternal))
              bEnqueuePromptIssued = false;  // prepare to issue an enqueue prompt in the future
            else if (bEnqueuePromptIssued == false) {
              bEnqueuePromptIssued = true;
              if (eventEnqueuePrompt != null)
                eventEnqueuePrompt.Invoke();
            }
            iLastEnqueuedTrackChecked = iPlaylistTrackNumberInternal;
          }
        }
        bPolling = false;
      }
    }

    #endregion

    #region Public events and variables (all read-only)

    public event WinampProxyEventDelegate eventWinampInstanceCreated;
    public event WinampProxyEventDelegate eventWinampInstanceClosed;
    public event WinampProxyEventDelegate eventPlayStopped;
    public event WinampProxyEventDelegate eventPlayStarted;
    public event WinampProxyEventDelegate eventEnqueuePrompt;
    public event WinampProxyEventDelegate eventTrackChanged;

    /// <summary>The current state of Winamp. </summary>
    public bool bRunning { get { return (iRunningInternal > 0 ? true : false); } }

    /// <summary>A more detailed description of the state of Winamp (Paused, Stopped, etc.)</summary>
    public WinampStatusEnum enumWinampStatus { get { return enumWinampStatusInternal; } }

    /// <summary>The number of tracks in the current Winamp playlist.</summary>
    public int iPlaylistCount { get { return iPlaylistCountInternal; } }

    /// <summary>The zero-based index of the currently playing track in the Winamp playlist.</summary>
    public int iPlaylistTrackNumber { get { return iPlaylistTrackNumberInternal; } }

    /// <summary>The name of the currently playing track.</summary>
    public string strCurrentTrackName { get { return strCurrentTrackNameInternal; } }

    /// <summary>The duration (in milliseconds) of the currently playing track.</summary>
    public int iCurrentTrackDuration { get { return iCurrentTrackDurationInternal; } }

    /// <summary>The track position (in milliseconds) of the currently playing track.</summary>
    public int iCurrentTrackPosition { get { return iCurrentTrackPositionInternal; } }

    /// <summary>
    /// The volume of Winamp between 0 and 255.
    /// IMPORTANT: Due to limitations in the Winamp SDK, this value is NOT reliable - and may not even be known. See the WinampProxy documentation for more info.
    /// </summary>
    public int iVolume { get { return (bVolumeKnown ? -1 : iVolumeInternal); } }

    /// <summary>Specifies when the eventEnqueuePrompt event is raised, in relation to the end of the playlist.</summary>
    public int iEnqueuePromptMilliseconds = 1000;
    
    #endregion

    #region Public methods

    /// <summary>This function increases the master volume of Winamp by four point (on a scale of 0 to 255.)</summary>
    new public void VolumeUp() {
      if (iVolumeInternal < 255) {
        if (bVolumeKnown)
          iVolumeInternal = Math.Min(255, iVolumeInternal + 4);
      }
      WinampInterface.VolumeUp();
    }

    /// <summary>This function decreases the master volume of Winamp by four points (on a scale of 0 to 255.)</summary>
    new public void VolumeDown() {
      if (iVolumeInternal > 0) {
        if (bVolumeKnown)
          iVolumeInternal = Math.Max(0, iVolumeInternal - 4);
      }
      WinampInterface.VolumeDown();
    }

    /// <summary>This function sets the master volume of Winamp to a specific value.</summary>
    /// <param name="iVolume">The volume (between 0 and 255.)</param>
    new public void SetVolume(int iVolume) {
      if ((iVolume >= 0) && (iVolume <= 255)) {
        bVolumeKnown = true;
        iVolumeInternal = iVolume;
        WinampInterface.SetVolume(iVolume);
      }
    }

    /// <summary>This function gets the frequency with which this proxy polls Winamp. The default is 100 milliseconds.</summary>
    /// <returns>The number of milliseconds between polls.</returns>
    public int GetPollingFrequency() {
      if (timerProxy == null)
        return 100;  // default frequency
      else
        return (int) timerProxy.Interval;
    }

    /// <summary>
    /// This function sets the frequency with which this proxy polls Winamp. The default is 100 milliseconds.
    /// NOTE: Higher values result in more interprocess communication that can bog down the machine.
    /// </summary>
    /// <param name="iMilliseconds">The number of milliseconds between polls.</param>
    public void SetPollingFrequency(int iMilliseconds) {
      if (timerProxy != null) {
        timerProxy.Stop();
        timerProxy.Interval = iMilliseconds;
        timerProxy.Start();
      }
    }

    #region Stub Functions

    // These are here simply for convenience - so that a developer doesn't have to decide whether to invoke the
    // static methods of WinampInterface or the member methods of this class. The developer can just utilize this
    // class, which will pass back internal info if it has it (e.g., 

    new public bool IsWinampRunning() { return WinampInterface.IsWinampRunning(); }
    new public bool StartWinamp() { return WinampInterface.StartWinamp(); }
    new public void InitializeWinamp() { WinampInterface.InitializeWinamp(); }
    new public void StopWinamp() { WinampInterface.StopWinamp(); }
    new public float GetWinampVersion() { return WinampInterface.GetWinampVersion(); }
    new public WinampStatusEnum GetWinampStatus() { return WinampInterface.GetWinampStatus(); }
    new public void ShowWinamp() { WinampInterface.ShowWinamp(); }
    new public void HideWinamp() { WinampInterface.HideWinamp(); }
    new public void ClearPlaylist() { WinampInterface.ClearPlaylist(); }
    new public void EnqueueTrack(string strFilename) { WinampInterface.EnqueueTrack(strFilename); }
    new public int GetPlaylistCount() { return iPlaylistCountInternal; }
    new public List<Track> GetPlaylist() { return WinampInterface.GetPlaylist(); }
    new public int GetPlaylistPosition() { return iPlaylistTrackNumberInternal; }
    new public void SetPlaylistPosition(int position) { WinampInterface.SetPlaylistPosition(position); }
    new public int GetTrackLength() { return iCurrentTrackDurationInternal; }
    new public int GetTrackPosition() { return iCurrentTrackPositionInternal; }
    new public string GetTrackName() { return strCurrentTrackNameInternal; }
    new public string GetTrackFilename() { return WinampInterface.GetTrackFilename(); }
    new public TrackInfo GetTrackInfo() { return WinampInterface.GetTrackInfo(); }
    new public int CalculatePlaylistDuration(int iStart, int iEnd, bool bAnalyzeFiles) { return WinampInterface.CalculatePlaylistDuration(iStart, iEnd, bAnalyzeFiles); }
    new public int CalculatePlaylistDuration(bool bAnalyzeFiles) { return WinampInterface.CalculatePlaylistDuration(bAnalyzeFiles); }
    new public int CalculatePlaylistRemainderDuration(bool bAnalyzeFiles) { return WinampInterface.CalculatePlaylistRemainderDuration(bAnalyzeFiles); }
    new public void Play() { WinampInterface.Play(); }
    new public void Pause() { WinampInterface.Pause(); }
    new public void Stop() { WinampInterface.Stop(); }
    new public void PrevTrack() { WinampInterface.PrevTrack(); }
    new public void NextTrack() { WinampInterface.NextTrack(); }
    new public void JumpToTrackPosition(int iMilliseconds) { WinampInterface.JumpToTrackPosition(iMilliseconds); }
    new public void Forward5Sec() { WinampInterface.Forward5Sec(); }
    new public void Rewind5Sec() { WinampInterface.Rewind5Sec(); }
    new public void SetPanning(int iPanning) { WinampInterface.SetPanning(iPanning); }
    new public bool GetShuffle() { return WinampInterface.GetShuffle(); }
    new public void SetShuffle(bool bShuffle) { WinampInterface.SetShuffle(bShuffle); }
    new public bool GetRepeat() { return WinampInterface.GetRepeat(); }
    new public void SetRepeat(bool bRepeat) { WinampInterface.SetRepeat(bRepeat); }
    new public bool IsEqualizerActive() { return WinampInterface.IsEqualizerActive(); }
    new public void SetEqualizerActive(bool bActive) { WinampInterface.SetEqualizerActive(bActive); }
    new public int GetEqualizerBand(int iBand) { return WinampInterface.GetEqualizerBand(iBand); }
    new public void SetEqualizerBand(int iBand, int iValue) { WinampInterface.SetEqualizerBand(iBand, iValue); }

    #endregion

    #endregion

  }

}
