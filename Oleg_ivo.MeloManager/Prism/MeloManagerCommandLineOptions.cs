namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerCommandLineOptions
    {
        /// <summary>
        /// �������� ������������
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// ����, �� �������� ������������� ���������, ������� ���������� �������� (��� ��������� "\")
        /// </summary>
        public string PlaylistsPath { get; set; }

        /// <summary>
        /// ���� (����� ";"), ���������� ����������� ����� (��� �������� "\")
        /// </summary>
        public string MusicFilesSource { get; set; }

        /// <summary>
        /// ����� �������������� �������. ��� ������� ������ ������� ��������� ����� ��������� <see cref="PlaylistsPath"/> � <see cref="MusicFilesSource"/>
        /// </summary>
        public bool RepairMode { get; set; }

        /// <summary>
        /// ����� ��������������
        /// </summary>
        public bool Mp3TagRenameMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Mp3TagRenamePreviewFileName { get; set; }
    }
}