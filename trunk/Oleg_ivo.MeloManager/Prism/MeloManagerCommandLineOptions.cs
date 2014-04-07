namespace Oleg_ivo.MeloManager.Prism
{
    public class MeloManagerCommandLineOptions
    {
        /// <summary>
        /// Название конфигурации
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// Путь, по которому располагаются плейлисты, которые необходимо починить (без конечного "\")
        /// </summary>
        public string PlaylistsPath { get; set; }

        /// <summary>
        /// Пути (через ";"), содержащие музыкальные файлы (без конечных "\")
        /// </summary>
        public string MusicFilesSource { get; set; }

        /// <summary>
        /// Режим автоматической починки. Для данного режима следует указывать также параметры <see cref="PlaylistsPath"/> и <see cref="MusicFilesSource"/>
        /// </summary>
        public bool RepairMode { get; set; }

        /// <summary>
        /// Режим переименования
        /// </summary>
        public bool Mp3TagRenameMode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Mp3TagRenamePreviewFileName { get; set; }
    }
}