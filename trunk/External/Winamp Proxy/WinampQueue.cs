// WinampQueue.cs
// Written by David J. Stein, Esq., 9/5/2008
// See "License.html" for terms and conditions of use.
//
// This class is an extension of the WinampProxy class. It adds some functionality by polling
// the state of the Winamp interface via an internal polling timer. The information can then be
// accessed on an as-needed basis by hosting applications. Moreover (and more interestingly), this
// class offers some subscribable events that are raised upon significant state changes of Winamp
// (track changes, start/stop, etc.)

#region Using declarations

using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Forms;

#endregion

namespace WinampProxyNS {

  public class WinampQueue : WinampProxy {

    #region Internal variables and initialization code

    private bool bQueueEndingPromptIssued = false;

    public WinampQueue() {
      this.timerProxy.Elapsed += new ElapsedEventHandler(timerQueue_Tick);
      this.eventEnqueuePrompt += new WinampProxyEventDelegate(MoveTrack);
    }

    #endregion

    #region Public variables and events

    /// <summary>
    /// This is the list of tracks to be fed to Winamp.
    /// NOTE: DO NOT LOCK THIS CLASS! If you do something like this -
    ///    foreach (Track track in lstTracks) { ... }
    /// - while the queue is playing, WinampProxy may try to remove a track from the list to feed to Winamp...
    /// and either Winamp or your application may blow up with an exception. If you need to iterate over the
    /// track list, do something like:
    ///    for (int i = 0; i [lessthan] lstTracks.Count; i++) {  Track track = lstTracks[i] ... }
    /// </summary>
    public List<Track> lstTracks = new List<Track>();

    /// <summary>
    /// This variable determines how long (in milliseconds) before the end of the playlist queue the
    /// eventQueueEndingPrompt is raised. By default, this is 30 seconds.
    /// </summary>
    public int iQueueEndingPromptMilliseconds = 30000;

    /// <summary>The number of tracks in the current Winamp playlist. (Note: This is overridden to include the number of enqueued tracks.)</summary>
    public new int iPlaylistCount { get { return iPlaylistCountInternal + lstTracks.Count; } }

    /// <summary>
    /// This event is raised when less than (iQueueEndingPromptMilliseconds) ms remain in the
    /// playlist that Winamp is playing.
    /// </summary>
    public event WinampProxyEventDelegate eventQueueEndingPrompt;

    #endregion

    #region Internal functions

    private void timerQueue_Tick(object o, ElapsedEventArgs args) {
      if (enumWinampStatus == WinampStatusEnum.Playing) {
        int iDurationRemaining = iQueueEndingPromptMilliseconds / 1000;
        for (int i = 0; i < lstTracks.Count; i++) {
          iDurationRemaining -= lstTracks[i].iDuration;
          if (iDurationRemaining < 0) {
            bQueueEndingPromptIssued = false;
            break;
          }
        }
        if (iDurationRemaining >= 0) {  // factor in the duration of the currently playing track
          iDurationRemaining -= (base.GetTrackLength() - base.GetTrackPosition()) / 1000;
          if (iDurationRemaining >= 0) {  // within bounds - raise event
            if (bQueueEndingPromptIssued == false) {
              bQueueEndingPromptIssued = true;
              if (eventQueueEndingPrompt != null)
                eventQueueEndingPrompt.Invoke();
            }
          } else
            bQueueEndingPromptIssued = false;
        }
      }
    }

    private void MoveTrack() {
      if (lstTracks.Count > 0) {
        base.EnqueueTrack(lstTracks[0].strFilename);
        lstTracks.RemoveAt(0);
      }
    }

    #endregion

    #region Stub methods

    // Explanation:
    // These functions are all identical in end-user behavior to the underlying functions of parent classes WinampInterface
    // and WinampProxy, so you shouldn't really have to mess with them. They are here because, since the queue is now
    // distributed over the proxy queue and the Winamp queue, some subtle changes have to be made - e.g.,
    // CalculatePlaylistDuration needs to take into account both the Winamp queue and the proxy queue. These functions below
    // are intended to replace the original queue-related functions. However, the WinampInterface-level functions are still
    // available by directly invoking the static functions from the lower class.

    new public void InitializeWinamp() {
      lstTracks.Clear();
      WinampInterface.InitializeWinamp();
    }

    new public void EnqueueTrack(string strFilename) {
      if (WinampInterface.GetPlaylistCount() == 0)  // NOTE: we need to go straight to the source for this inquiry, in case a whole lot of tracks are enqueued at once.
        base.EnqueueTrack(strFilename);
      else {
        MP3Header h = new MP3Header();
        if (h.ReadMP3Information(strFilename) == true)
          lstTracks.Add(new Track(h.intLength, strFilename));
        else
          lstTracks.Add(new Track(0, strFilename));
      }
    }

    new public void Play() {
      if (WinampInterface.GetPlaylistCount() == 0) {
        if (lstTracks.Count > 0)
          MoveTrack();
        else {  // request user for tracks
          OpenFileDialog o = new OpenFileDialog();
          o.Filter = "MP3 Files (*.mp3)|*.mp3|All Files (*.*)|*.*";
          o.Multiselect = true;
          if (o.ShowDialog() == DialogResult.OK) {
            foreach (string strFilename in o.FileNames)
              EnqueueTrack(strFilename);
          }
        }
      }
      if (WinampInterface.GetPlaylistCount() > 0)
        base.Play();
    }

    new public void ClearPlaylist() {
      base.ClearPlaylist();
      lstTracks.Clear();
    }

    new public int GetPlaylistCount() {
      return base.GetPlaylistCount() + lstTracks.Count;
    }

    new public List<Track> GetPlaylist() {
      List<Track> lstPlaylistTracks = base.GetPlaylist();
      for (int i = 0; i < lstTracks.Count; i++)
        lstPlaylistTracks.Add(lstTracks[i]);
      return lstPlaylistTracks;
    }

    new public void SetPlaylistPosition(int position) {
      if (position < base.GetPlaylistCount() + lstTracks.Count) {
        while ((position >= base.GetPlaylistCount()) && (lstTracks.Count > 0))  // enqueue tracks until specified position
          MoveTrack();
        base.SetPlaylistPosition(position);
      }
    }

    new public int CalculatePlaylistDuration(int iStart, int iEnd, bool bAnalyzeFiles) {
      int iDuration = 0;
      int iWinampPlaylistCount = base.GetPlaylistCount();
      // measure Winamp playlist portion
      if (iStart < iWinampPlaylistCount)
        iDuration = base.CalculatePlaylistDuration(iStart, Math.Min(iEnd, iWinampPlaylistCount - 1), bAnalyzeFiles);
      // measure proxy playlist portion
      if ((iEnd >= iWinampPlaylistCount) && (bAnalyzeFiles)) {
        for (int i = 0; (i < iEnd - iWinampPlaylistCount) && (i < lstTracks.Count); i++)
          iDuration += lstTracks[i].iDuration;
      }
      return iDuration;
    }

    new public int CalculatePlaylistDuration(bool bAnalyzeFiles) {
      return CalculatePlaylistDuration(0, GetPlaylistCount(), bAnalyzeFiles);
    }

    new public int CalculatePlaylistRemainderDuration(bool bAnalyzeFiles) {
      int iDuration = base.CalculatePlaylistRemainderDuration(bAnalyzeFiles);
      if (bAnalyzeFiles) {
        for (int i = 0; i < lstTracks.Count; i++)
          iDuration += lstTracks[i].iDuration;
      }
      return iDuration;
    }

    new public void NextTrack() {
      if ((base.GetPlaylistPosition() == base.GetPlaylistCount() - 1))
        MoveTrack();
      base.NextTrack();
		}

    new public void JumpToTrackPosition(int iMilliseconds) {
      if ((base.GetTrackLength() - iMilliseconds < iEnqueuePromptMilliseconds) && (base.GetPlaylistPosition() == base.GetPlaylistCount() - 1))
        MoveTrack();
      base.JumpToTrackPosition(iMilliseconds);
    }

    new public void Forward5Sec() {
      if ((base.GetTrackPosition() + 5000 < iEnqueuePromptMilliseconds) && (base.GetPlaylistPosition() == base.GetPlaylistCount() - 1))
        MoveTrack();
      base.Forward5Sec();
    }

    #endregion

  }

}
