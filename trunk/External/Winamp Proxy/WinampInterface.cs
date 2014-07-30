// WinampInterface.cs
// Written by David J. Stein, Esq., 9/5/2008
// See "License.html" for terms and conditions of use.
//
// This class is an extension of earlier work by Polis Pilavas (April 2005). In general, it is
// a straightforward implementation of the Winamp SDK in C#. However, many quirks to this API
// are addressed by this code to provide a more intuitive, consistent, and user-friendly interface.

#region Using declarations

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;

#endregion

namespace WinampProxyNS
{

    #region Helper classes

    /// <summary>
    /// This class contains the information about a track queued in the Winamp playlist.
    /// NOTE: The duration information may be incorrect.
    /// </summary>
    public class Track
    {
        public string strFilename = "";
        public int iDuration = 0;
        public Track(int iDuration, string strFilename)
        {
            this.iDuration = iDuration;
            this.strFilename = strFilename;
        }
    }

    /// <summary>This class contains some additional information about the structure of a track in the Winamp playlist.</summary>
    public class TrackInfo
    {
        public int iSampleRate = 0;
        public int iBitrate = 0;
        public int iChannels = 0;
    }

    public enum WinampStatusEnum { Unknown, Stopped, Playing, Paused };

    #endregion

    public class WinampInterface
    {

        #region DLL Imports, IPC Stuff, Consts, Etc. - Nothing Interesting Here, Move Along...

        protected struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        protected static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName);

        [DllImport("user32.dll")]
        protected static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        protected static extern int GetWindowModuleFileName(int hWnd, StringBuilder title, int size);

        //For use with WM_COPYDATA and COPYDATASTRUCT
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        protected static extern int SendMessage(IntPtr hWnd, int wMsg, int wParam, ref COPYDATASTRUCT lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        protected static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, uint lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        protected static extern int GetWindowText(IntPtr hwnd, string lpString, int cch);

        [DllImport("kernel32.dll")]
        protected static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        protected static extern bool CloseHandle(IntPtr hHandle);

        [DllImport("psapi.dll", SetLastError = true)]
        [PreserveSig]
        protected static extern uint GetModuleFileNameEx([In]IntPtr hProcess, [In] IntPtr hModule, [Out] StringBuilder lpFilename, [In][MarshalAs(UnmanagedType.U4)]int nSize);

        [DllImport("user32.dll", SetLastError = true)]
        protected static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        protected const int WM_COPYDATA = 0X004A;
        protected const int WM_CLOSE = 0X0010;

        protected static int SendWindowsStringMessage(IntPtr hWnd, int dwData, int wParam, string msg)
        {
            // taken from this project: http://boycook.wordpress.com/2008/07/29/c-win32-messaging-with-sendmessage-and-wm_copydata/ - many thanks to Craig Cook for this function! :)
            int result = 0;
            if (hWnd != IntPtr.Zero)
            {
                byte[] sarr = Encoding.Default.GetBytes(msg);
                int len = sarr.Length;
                COPYDATASTRUCT cds;
                cds.dwData = (IntPtr)dwData;
                cds.lpData = msg;
                cds.cbData = len + 1;
                result = SendMessage(hWnd, WM_COPYDATA, (int)hWnd, ref cds);
            }
            return result;
        }

        protected static string GetAppNameFromHwnd(IntPtr hwnd)
        {
            // taken from this project: http://www.codeproject.com/KB/cs/Trayminimizer.aspx - many thanks to Giorgi Dalakishvili for this function! :)
            uint dwProcessId;
            GetWindowThreadProcessId(hwnd, out dwProcessId);
            IntPtr hProcess = OpenProcess(0x00000410, false, dwProcessId);
            StringBuilder path = new StringBuilder(1024);
            GetModuleFileNameEx(hProcess, IntPtr.Zero, path, 1024);
            CloseHandle(hProcess);
            return path.ToString();
        }

        protected static bool IsNumeric(string value)
        {
            double d;
            return double.TryParse(value, out d);
        }

        private const int WM_COMMAND = 0x111;  // To tell Winamp that we are sending it a WM_COMMAND it needs the hex code 0x111
        private const int WM_WA_IPC = 0x0400;  // To tell Winamp that we are sending it a WM_USER (WM_WA_IPC) it needs the hex code 0x0400

        private const string m_windowName = "Winamp v1.x";
        private const string strTtlEnd = " - Winamp";

        // commands
        private const int WA_NOTHING = 0;
        private const int WINAMP_OPTIONS_PREFS = 40012; // pops up the preferences
        private const int WINAMP_OPTIONS_AOT = 40019; // toggles always on top
        private const int WINAMP_FILE_PLAY = 40029; // pops up the load file(s) box
        private const int WINAMP_OPTIONS_EQ = 40036; // toggles the EQ window
        private const int WINAMP_OPTIONS_PLEDIT = 40040; // toggles the playlist window
        private const int WINAMP_HELP_ABOUT = 40041; // pops up the about box
        private const int WA_PREVTRACK = 40044; // plays previous track
        private const int WA_PLAY = 40045; // plays selected track
        private const int WA_PAUSE = 40046; // pauses/unpauses currently playing track
        private const int WA_STOP = 40047; // stops currently playing track
        private const int WA_NEXTTRACK = 40048; // plays next track
        private const int WA_VOLUMEUP = 40058; // turns volume up
        private const int WA_VOLUMEDOWN = 40059; // turns volume down
        private const int WINAMP_FFWD5S = 40060; // fast forwards 5 seconds
        private const int WINAMP_REW5S = 40061; // rewinds 5 seconds
        private const int WINAMP_BUTTON1_SHIFT = 40144; // fast-rewind 5 seconds
        private const int WINAMP_BUTTON4_SHIFT = 40147; // stop after current track
        private const int WINAMP_BUTTON5_SHIFT = 40148; // fast-forward 5 seconds
        private const int WINAMP_BUTTON1_CTRL = 40154; // start of playlist
        private const int WINAMP_BUTTON2_CTRL = 40155; // open URL dialog
        private const int WINAMP_BUTTON4_CTRL = 40157; // fadeout and stop
        private const int WINAMP_BUTTON5_CTRL = 40158; // end of playlist
        private const int WINAMP_FILE_DIR = 40187; // pops up the load directory box
        private const int ID_MAIN_PLAY_AUDIOCD1 = 40323; // starts playing the audio CD in the first CD reader
        private const int ID_MAIN_PLAY_AUDIOCD2 = 40323; // plays the 2nd
        private const int ID_MAIN_PLAY_AUDIOCD3 = 40323; // plays the 3rd
        private const int ID_MAIN_PLAY_AUDIOCD4 = 40323; // plays the 4th
        private const int IPC_PLAYFILE = 100;		       // Returns status of playback. Returns: 1 = playing, 3 = paused, 0 = not playing)
        private const int IPC_ISPLAYING = 104;		 // Returns status of playback. Returns: 1 = playing, 3 = paused, 0 = not playing)
        private const int IPC_GETVERSION = 0;	     // Returns Winamp version (0x2y0x for winamp 2.yx,  Versions previous to Winamp 2.0
        // typically (but not always) use 0x1zyx for 1.zx versions
        private const int IPC_DELETE = 101;		 // Clears Winamp internal playlist
        private const int IPC_CHDIR = 103;		         // Changes current Winamp directory
        private const int IPC_GETOUTPUTTIME = 105;		 // Returns the position in milliseconds of the 
        // current song (mode = 0), or the song length, in seconds (mode = 1). It 
        // returns: -1 if not playing or if there is an error.
        private const int IPC_JUMPTOTIME = 106;		 // Sets the position in milliseconds of the current song (approximately). It
        // returns -1 if not playing, 1 on eof, or 0 if successful. It requires Winamp v1.60+
        private const int IPC_WRITEPLAYLIST = 120;		 // Writes the current playlist to <winampdir>\\Winamp.m3u, and returns the current 
        // playlist position. It requires Winamp v1.666+
        private const int IPC_SETPLAYLISTPOS = 121;		 // Sets the playlist position
        private const int IPC_SETVOLUME = 122;		 // Sets the volume of Winamp (from 0-255)
        private const int IPC_SETPANNING = 123;		 // Sets the panning of Winamp (from 0 (left) to 255 (right))
        private const int IPC_GETLISTLENGTH = 124;		 // Returns the length of the current playlist in tracks
        private const int IPC_GETLISTPOS = 125;    // Returns the playlist position. A lot like IPC_WRITEPLAYLIST only faster since it 
        // doesn't have to write out the list. It requires Winamp v2.05+
        private const int IPC_GETINFO = 126;		 // Returns info about the current playing song (about Kb rate). The value it returns 
        // depends on the value of 'mode'. If mode == 0 then it returns the Samplerate (i.e. 44100), 
        // if mode == 1 then it returns the Bitrate  (i.e. 128), if mode == 2 then it returns the 
        // channels (i.e. 2)

        private const int IPC_GETEQDATA = 127;      // Queries the status of the EQ. The value it returns depends on what 'position' is set to. It
        // requires Winamp v2.05+
        // Value      Meaning
        // ------------------
        // 0-9        The 10 bands of EQ data. 0-63 (+20db - -20db)
        // 10         The preamp value. 0-63 (+20db - -20db)
        // 11         Enabled. zero if disabled, nonzero if enabled.
        // 12         Autoload. zero if disabled, nonzero if enabled.
        private const int IPC_SETEQDATA = 128;		 // Sets the value of the last position retrieved by IPC_GETEQDATA (integer eqPosition). It
        // requires Winamp v2.05+
        private const int IPC_GETSHUFFLE = 250;    // Gets the shuffle property (1 = set, 0 = not set.)
        private const int IPC_GETREPEAT = 251;    // Gets the repeat property (1 = set, 0 = not set.)
        private const int IPC_SETSHUFFLE = 252;    // Sets the shuffle property (1 = set, 0 = not set.)
        private const int IPC_SETREPEAT = 253;    // Sets the repeat property (1 = set, 0 = not set.)
        private const int IPC_GETWND = 260;   // Gets the hwnd of a submenu of Winamp (0 = equalizer window, 1 = playlist, 2 = minibrowser, 3 = video.)


        #endregion

        #region Winamp Instance and Status Info

        /// <summary>This function reports whether or not an instance of Winamp is currently running.</summary>
        /// <returns>True (yes ) or false (no.)</returns>
        public static bool IsWinampRunning()
        {
            return (FindWindow("Winamp v1.x", null) == IntPtr.Zero ? false : true);
        }

        /// <summary>This function invokes a new instance of Winamp if it's not yet started.</summary>
        /// <returns>True (success) or false (failure.)</returns>
        public static bool StartWinamp()
        {

            // check whether Winamp is already running
            if (IsWinampRunning() == false)
            {
                string strWinampPath = ((string)Registry.GetValue("HKEY_CLASSES_ROOT\\winamp\\shell\\open\\command", null, null)).Split(new char[] { '"' })[1];
                if (strWinampPath == null)
                    return false;  // don't know where to find Winamp :(

                // we have to start Winamp - note: there's usually a startup delay, so keep checking (block) until it's actually started
                Process.Start(strWinampPath);
                do
                {
                    IntPtr hWnd = FindWindow("Winamp v1.x", null);
                    if (hWnd != IntPtr.Zero)
                        break;  // already running
                    Thread.Sleep(50);
                } while (true);
            }
            return true;
        }

        /// <summary>
        /// This function initalizes the properties of Winamp (clears the playlist, sets volume to midlevel,
        /// zeroes the equalizer, etc.)
        /// </summary>
        public static void InitializeWinamp()
        {
            Stop();
            ClearPlaylist();
            SetShuffle(false);
            SetRepeat(false);
            SetVolume(128);  // set volume to midpoint
            SetPanning(0);
            SetEqualizerActive(true);
            for (int i = 0; i < 11; i++)
                SetEqualizerBand(i, 32);
        }

        /// <summary>This function stops one running instance of Winamp.</summary>
        /// <returns>True (success) or false (failure.)</returns>
        public static void StopWinamp()
        {

            // check whether Winamp is already running
            if (IsWinampRunning())
            {
                IntPtr hWnd = FindWindow("Winamp v1.x", null);
                SendMessage(hWnd, WM_CLOSE, 0, 0);
            }
        }

        /// <summary>This function returns the version number of Winamp.</summary>
        /// <returns>0.0 or more (Winamp version number) or -1 (Winamp not running or version unknown.)</returns>
        public static float GetWinampVersion()
        {

            float fVersion = -1;
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
            {
                int iVersionEncoded = 0;
                iVersionEncoded = SendMessage(hwnd, WM_WA_IPC, WA_NOTHING, IPC_GETVERSION);
                if ((iVersionEncoded & 0x2000) > 0)  // Winamp version 2.something - Winamp returns this encoded as "0x2y0z" for version 2.yz.
                    fVersion = 2.0f + (float)(((iVersionEncoded & 0x0f00) >> 8) / 10.0f) + (float)((iVersionEncoded & 0x000f) / 100.0f);
                else if ((iVersionEncoded & 0x1000) > 0)  // Winamp version 1.something - Winamp returns this encoded as "0x1wyz" for version 1.wyz.
                    fVersion = 2.0f + (float)((((iVersionEncoded & 0x0f00) >> 8) / 10.0f) + ((iVersionEncoded & 0x00f0) >> 4) / 100.0f) + (float)((iVersionEncoded & 0x000f) / 1000.0f);
            }
            return fVersion;
        }

        /// <summary>This function returns a WinampStatus enum value indicating the current status of Winamp.</summary>
        /// <returns>Enum value indicating the current status of Winamp: stopped, playing, paused, or unknown.</returns>
        public static WinampStatusEnum GetWinampStatus()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return WinampStatusEnum.Unknown;
            int iStatus = SendMessage(hwnd, WM_WA_IPC, WA_NOTHING, IPC_ISPLAYING);
            if (iStatus == 0)
                return WinampStatusEnum.Stopped;
            else if (iStatus == 1)
                return WinampStatusEnum.Playing;
            else if (iStatus == 3)
                return WinampStatusEnum.Paused;
            else
                return WinampStatusEnum.Unknown;
        }

        /// <summary>This function shows the main Winamp window.</summary>
        public static void ShowWinamp()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                ShowWindow(hwnd, 1);
        }

        /// <summary>This function hides the main Winamp window and all accessory windows.</summary>
        public static void HideWinamp()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
            {
                // we also have to hide all of the other windows, if they're showing
                for (int i = 0; i <= 3; i++)
                {
                    IntPtr hwndSubmenu = new IntPtr(SendMessage(hwnd, WM_WA_IPC, i, IPC_GETWND));
                    if (hwndSubmenu != IntPtr.Zero)
                        ShowWindow(hwndSubmenu, 0);
                }
                ShowWindow(hwnd, 0);
            }
        }

        #endregion

        #region Track and Playlist Functions

        /// <summary>
        /// This function clears the Winamp playlist.
        /// Note: any track currently running will continue to run until completion - that's just how Winamp works.
        /// </summary>
        public static void ClearPlaylist()
        {

            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_WA_IPC, WA_NOTHING, IPC_DELETE);
        }

        /// <summary>This function enqueues the track specified by the filename.</summary>
        /// <param name="strFilename">The filename of the MP3 to enqueue.</param>
        public static void EnqueueTrack(string strFilename)
        {

            if (File.Exists(strFilename))
            {
                IntPtr hwnd = FindWindow(m_windowName, null);
                if (hwnd != IntPtr.Zero)
                {
                    string strData = strFilename + ((char)0);
                    SendWindowsStringMessage(hwnd, IPC_PLAYFILE, 0, strData);  // send simple info
                }
            }
        }

        /// <summary>This function gets the total number of tracks in the current Winamp playlist.</summary>
        /// <returns>0 or more (total number of queued tracks) or -1 (Winamp not running.)</returns>
        public static int GetPlaylistCount()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return -1;
            return SendMessage(hwnd, WM_WA_IPC, WA_NOTHING, IPC_GETLISTLENGTH);
        }

        /// <summary>This function gets the list of currently queued tracks.</summary>
        /// <returns>List<Track> (list of tracknames and durations) or null (Winamp not running or some kind of failure.)</returns>
        public static List<Track> GetPlaylist()
        {

            // This function is tricky. This function is really crude: it just writes a file called "Winamp.m3u" to whatever
            // directory Winamp is currently using.
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return null;

            // set path to temp directory
            string strModuleName = GetAppNameFromHwnd(hwnd);
            string strPath = Path.GetDirectoryName(strModuleName);
            SendWindowsStringMessage(hwnd, IPC_CHDIR, 0, strPath + "\0");
            // check and see if file already exists; if so, rename it
            string strPlaylistName = strPath + Path.DirectorySeparatorChar + "Winamp.m3u";
            string strTempName = "";
            if (File.Exists(strPlaylistName))
            {
                strTempName = Path.GetTempFileName();
                File.Delete(strTempName);
                try
                {
                    File.Move(strPlaylistName, strTempName);
                }
                catch { }
                if (File.Exists(strPlaylistName) == true) // we can't move the file for some reason, so abort
                    return null;
            }
            // write out the list
            SendMessage(hwnd, WM_WA_IPC, WA_NOTHING, IPC_WRITEPLAYLIST);
            // check to see if it exists
            var Roaming = Environment.ExpandEnvironmentVariables("%USERPROFILE%");
            strPlaylistName = Path.Combine(Roaming, @"AppData\Roaming\Winamp\Winamp.m3u8");
            if (File.Exists(strPlaylistName) == false)
                return null;
            // read it and clean up files
            string[] strTracks = File.ReadAllLines(strPlaylistName);
            File.Delete(strPlaylistName);
            if ((strTempName.Length > 0) && (File.Exists(strTempName)))
                File.Move(strTempName, strPlaylistName);
            // filter out everything that begins with a "#"
            List<Track> lstTracks = new List<Track>();
            for (int i = 1; i < strTracks.Length; i++)
            {
                if (strTracks[i].StartsWith("#") == false)
                {
                    if (strTracks[i].Trim().Length > 0)
                        lstTracks.Add(new Track(0, strTracks[i]));
                }
                else if (i < strTracks.Length - 1)
                {
                    // first line begins with # and contains duration info
                    int iComma = strTracks[i].IndexOf(',');
                    if (iComma < 8)
                        lstTracks.Add(new Track(0, strTracks[i + 1]));
                    else
                    {
                        string strDuration = strTracks[i].Substring(8, iComma - 8);
                        int iDuration = (strDuration.Length > 0 ? Int32.Parse(strDuration) : 0);
                        lstTracks.Add(new Track(iDuration, strTracks[i + 1]));
                        i++;
                    }
                }
            }
            // return array
            return lstTracks;
        }

        /// <summary>This function gets the index of the currently playing track in the Winamp playlist.</summary>
        /// <returns>0 or more (zero-based track index) or -1 (Winamp not running.)</returns>
        public static int GetPlaylistPosition()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return -1;
            return SendMessage(hwnd, WM_WA_IPC, WA_NOTHING, IPC_GETLISTPOS);
        }

        /// <summary>This function sets the index of the currently playing track in the Winamp playlist.</summary>
        /// <param name="position">The zero-based index of the track to be selected in the playlist.</param>
        public static void SetPlaylistPosition(int position)
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_WA_IPC, position, IPC_SETPLAYLISTPOS);
        }

        /// <summary>This function gets the *approximate* length, in milliseconds, of the currently playing track in the Winamp playlist.</summary>
        /// <returns>0 or more (milliseconds in current track) or -1 (Winamp not running or no tracks queued.)</returns>
        public static int GetTrackLength()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return -1;
            return SendMessage(hwnd, WM_WA_IPC, 1, IPC_GETOUTPUTTIME) * 1000;
        }

        /// <summary>This function gets the position, in milliseconds, of the currently playing track in the Winamp playlist.</summary>
        /// <returns>0 or more (milliseconds in current track) or -1 (Winamp not running or no tracks queued.)</returns>
        public static int GetTrackPosition()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return -1;
            return SendMessage(hwnd, WM_WA_IPC, WA_NOTHING, IPC_GETOUTPUTTIME);
        }

        /// <summary>This function gets the name of the currently playing track.</summary>
        /// <returns>string (name of playing track) or "" (Winamp not running or no track playing.)</returns>
        public static string GetTrackName()
        {
            string strName = "";
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
            {
                string lpText = new string((char)0, 100);
                int intLength = GetWindowText(hwnd, lpText, lpText.Length);
                if ((intLength > 0) && (intLength <= lpText.Length))
                {
                    strName = lpText.Substring(0, intLength);
                    int intName = strName.IndexOf(strTtlEnd);
                    int intLeft = strName.IndexOf("[");
                    int intRight = strName.IndexOf("]");
                    if ((intName >= 0) && (intLeft >= 0) && (intName < intLeft) && (intRight >= 0) && (intLeft + 1 < intRight))
                        strName = strName.Substring(intLeft + 1, intRight - intLeft - 1);
                    else
                    {
                        if ((strName.EndsWith(strTtlEnd)) && (strName.Length > strTtlEnd.Length))
                            strName = strName.Substring(0, strName.Length - strTtlEnd.Length);
                        int intDot = strName.IndexOf(".");
                        if ((intDot > 0) && IsNumeric(strName.Substring(0, intDot)))
                            strName = strName.Remove(0, intDot + 1);
                    }
                }
            }
            return strName;
        }

        /// <summary>This function gets the filename of the currently playing track.</summary>
        /// <returns>string (filename of playing track) or "" (Winamp not running or no track playing.)</returns>
        public static string GetTrackFilename()
        {
            string strFilename = "";
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
            {
                List<Track> t = GetPlaylist();
                int position = GetPlaylistPosition();
                if (position < t.Count)
                    strFilename = t[position].strFilename;
            }
            return strFilename;
        }

        /// <summary>This function returns a TrackInfo structure describing the currently playing track.</summary>
        /// <returns>TrackInfo structure (info about playing track) or null (Winamp not running or no track playing.)</returns>
        public static TrackInfo GetTrackInfo()
        {
            TrackInfo t = null;
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
            {
                t = new TrackInfo();
                t.iSampleRate = SendMessage(hwnd, WM_WA_IPC, 0, IPC_GETINFO);
                t.iBitrate = SendMessage(hwnd, WM_WA_IPC, 1, IPC_GETINFO);
                t.iChannels = SendMessage(hwnd, WM_WA_IPC, 2, IPC_GETINFO);
            }
            return t;
        }

        /// <summary>This function calculates the duration of a specified set of tracks in the playlist.</summary>
        /// <param name="iStart">The zero-based index of the first track in the playlist to be included in the duration calculation.</param>
        /// <param name="iEnd">The zero-based index of the last track in the playlist to be included in the duration calculation.</param>
        /// <param name="bAnalyzeFiles">Specifies whether, when Winamp doesn't have duration information for a track, WinampInterface analyzes the actual file to extract the duration information.
        /// Setting this parameter to true yields a more accurate calculation, but can also be more CPU- and bus-intensive.</param>
        /// <returns>The duration (in seconds) of the specified set of tracks in the playlist.</returns>
        public static int CalculatePlaylistDuration(int iStart, int iEnd, bool bAnalyzeFiles)
        {
            int iDuration = 0;
            List<Track> lstTracks = GetPlaylist();
            if (lstTracks != null)
            {
                for (int i = iStart; (i < iEnd) && (i < lstTracks.Count); i++)
                {
                    Track t = lstTracks[i];
                    if (t.iDuration > 0)
                        iDuration += t.iDuration;
                    else if (bAnalyzeFiles == true)
                    {
                        MP3Header h = new MP3Header();
                        if (h.ReadMP3Information(t.strFilename) == true)
                            iDuration += h.intLength;
                    }
                }
            }
            return iDuration;
        }

        /// <summary>This function calculates the duration of the entire playlist.</summary>
        /// <param name="bAnalyzeFiles">Specifies whether, when Winamp doesn't have duration information for a track, WinampInterface analyzes the actual file to extract the duration information.
        /// Setting this parameter to true yields a more accurate calculation, but can also be more CPU- and bus-intensive.</param>
        /// <returns>The duration (in seconds) of the playlist.</returns>
        public static int CalculatePlaylistDuration(bool bAnalyzeFiles)
        {
            return CalculatePlaylistDuration(0, GetPlaylistCount(), bAnalyzeFiles);
        }

        /// <summary>This function calculates the duration of remaining tracks in the playlist.</summary>
        /// <param name="bAnalyzeFiles">Specifies whether, when Winamp doesn't have duration information for a track, WinampInterface analyzes the actual file to extract the duration information.
        /// Setting this parameter to true yields a more accurate calculation, but can also be more CPU- and bus-intensive.</param>
        /// <returns>The duration (in seconds) of the remainder of playlist.</returns>
        public static int CalculatePlaylistRemainderDuration(bool bAnalyzeFiles)
        {
            int iDuration = (GetTrackLength() - GetTrackPosition()) / 1000;
            return iDuration + CalculatePlaylistDuration(GetPlaylistPosition() + 1, GetPlaylistCount(), bAnalyzeFiles);
        }

        #endregion

        #region Winamp Controls

        /// <summary>This function starts or resumes playback on Winamp.</summary>
        public static void Play()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WA_PLAY, WA_NOTHING);
        }

        /// <summary>This function pauses playback on Winamp.</summary>
        public static void Pause()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WA_PAUSE, WA_NOTHING);
        }

        /// <summary>This function stops playback on Winamp.</summary>
        public static void Stop()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WA_STOP, WA_NOTHING);
        }

        /// <summary>This function jumps backward one track on Winamp.</summary>
        public static void PrevTrack()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WA_PREVTRACK, WA_NOTHING);
        }

        /// <summary>This function jumps forward one track on Winamp.</summary>
        public static void NextTrack()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WA_NEXTTRACK, WA_NOTHING);
        }

        /// <summary>This function increases the master volume of Winamp by four points (on a scale of 0 to 255.)</summary>
        public static void VolumeUp()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WA_VOLUMEUP, WA_NOTHING);
        }

        /// <summary>This function decreases the master volume of Winamp by four points (on a scale of 0 to 255.)</summary>
        public static void VolumeDown()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WA_VOLUMEDOWN, WA_NOTHING);
        }

        /// <summary>This function jumps to an (approximate) millisecond position in the currently playing track.</summary>
        /// <param name="iMilliseconds">The (approximate) destination position, in milliseconds from the beginning of the track.</param>
        public static void JumpToTrackPosition(int iMilliseconds)
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_WA_IPC, iMilliseconds, IPC_JUMPTOTIME);
        }

        /// <summary>This function jumps forward five seconds in the currently playing track on Winamp.</summary>
        public static void Forward5Sec()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WINAMP_FFWD5S, WA_NOTHING);
        }

        /// <summary>This function jumps backward five seconds in the currently playing track on Winamp.</summary>
        public static void Rewind5Sec()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_COMMAND, WINAMP_REW5S, WA_NOTHING);
        }

        /// <summary>This function sets the master volume of Winamp to a specific value.
        /// IMPORTANT: If your application uses WinampProxy, don't use this static method of WinampInterface - use the SetVolume instance method of the WinampProxy object instead.</summary>
        /// <param name="iVolume">The volume (between 0 and 255.)</param>
        public static void SetVolume(int iVolume)
        {
            if ((iVolume >= 0) && (iVolume <= 255))
            {
                IntPtr hwnd = FindWindow(m_windowName, null);
                if (hwnd != IntPtr.Zero)
                    SendMessage(hwnd, WM_WA_IPC, iVolume, IPC_SETVOLUME);
            }
        }

        /// <summary>This function sets the left/right balance of Winamp to a specific value.</summary>
        /// <param name="iVolume">The balance (between -32 for far left and +32 for far right.)</param>
        public static void SetPanning(int iPanning)
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_WA_IPC, iPanning, IPC_SETPANNING);
        }

        /// <summary>This function gets the Shuffle value of Winamp.</summary>
        /// <returns>Boolean value indicating whether or not Shuffle is activated.</returns>
        public static bool GetShuffle()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return false;
            return (SendMessage(hwnd, WM_WA_IPC, 0, IPC_GETSHUFFLE) == 0 ? false : true);
        }

        /// <summary>This function sets the Shuffle value of Winamp.</summary>
        /// <param name="bShuffle">Boolean value indicating whether or not Shuffle is to be activated.</param>
        public static void SetShuffle(bool bShuffle)
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_WA_IPC, bShuffle == true ? 1 : 0, IPC_SETSHUFFLE);
        }

        /// <summary>This function gets the Repeat value of Winamp.</summary>
        /// <returns>Boolean value indicating whether or not Repeat is activated.</returns>
        public static bool GetRepeat()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return false;
            return (SendMessage(hwnd, WM_WA_IPC, 0, IPC_GETREPEAT) == 0 ? false : true);
        }

        /// <summary>This function sets the Repeat value of Winamp.</summary>
        /// <param name="bShuffle">Boolean value indicating whether or not Repeat is to be activated.</param>
        public static void SetRepeat(bool bRepeat)
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
                SendMessage(hwnd, WM_WA_IPC, bRepeat == true ? 1 : 0, IPC_SETREPEAT);
        }

        /// <summary>This function determines whether or not the Winamp equalizer is activated.</summary>
        /// <returns>Boolean value indicating whether or not the Winamp equalizer is activated.</returns>
        public static bool IsEqualizerActive()
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd == IntPtr.Zero)
                return false;
            return (SendMessage(hwnd, WM_WA_IPC, 11, IPC_GETEQDATA) == 0 ? false : true);
        }

        /// <summary>This function specifies whether or not the Winamp equalizer is to be activated.</summary>
        /// <param name="bActive">Boolean value indicating whether or not the Winamp equalizer is to be activated.</returns>
        public static void SetEqualizerActive(bool bActive)
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            int iStatus = SendMessage(hwnd, WM_WA_IPC, 11, IPC_GETEQDATA);
            if (((iStatus == 0) && (bActive == true)) || ((iStatus != 0) && (bActive == false)))  // must update
                SendMessage(hwnd, WM_WA_IPC, bActive ? 1 : 0, IPC_SETEQDATA);
        }

        /// <summary>This function gets the value of one of the ten equalizer bands in Winamp.</summary>
        /// <param name="iBand">The value (between 0 and 9, or 10 for the preamp) of the equalizer band to read.</param>
        /// <returns>The integer value of the specified equalizer band (between -32 and 32) of the equalizer band.</returns>
        public static int GetEqualizerBand(int iBand)
        {
            IntPtr hwnd = FindWindow(m_windowName, null);
            if ((hwnd == IntPtr.Zero) || (iBand < 0) || (iBand > 10))
                return -1;
            return SendMessage(hwnd, WM_WA_IPC, iBand, IPC_GETEQDATA);
        }

        /// <summary>This function sets the value of one of the ten equalizer bands in Winamp.</summary>
        /// <param name="iBand">The equalizer band (between 0 and 9, or 10 for the preamp) to be set.</param>
        /// <param name="iValue">The value of the equalizer band to be set.</param>
        public static void SetEqualizerBand(int iBand, int iValue)
        {

            // this function works kind of strangely...
            // first you have to read the band that you want, then you call this with the value to put into that band.
            IntPtr hwnd = FindWindow(m_windowName, null);
            if (hwnd != IntPtr.Zero)
            {
                SendMessage(hwnd, WM_WA_IPC, iBand, IPC_GETEQDATA);
                SendMessage(hwnd, WM_WA_IPC, iValue, IPC_SETEQDATA);
            }
        }

        #endregion

    }

}
