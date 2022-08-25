using AudioBookScrapper.Model;
using System.Text;

namespace AudioBookScrapper.Model
{
    /// <summary>
    /// Аудиокнига.
    /// </summary>
    public class AudioBook
    {
        /// <summary>
        /// Ссылка на страницу с книгой.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Название книги.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Описание книги.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Список файлов на воспроизведение.
        /// </summary>
        public List<BookElement> Playlist { get; set; }

        /// <summary>
        /// Получить суммарную длительность книги
        /// </summary>
        /// <param name="timeUnit">Единицы измерения.</param>
        /// <returns>Длительность книги в заданных единицах измерения.</returns>
        public double GetTotalLength(TimeUnitEnum timeUnit)
        {
            var result = GetTotalLength();

            switch(timeUnit)
            {
                case TimeUnitEnum.MINUTES:
                    return Math.Round(result / 60, 2);
                case TimeUnitEnum.MILLISECONDS:
                    return Math.Round(result / 1000, 2);
                default:
                    return result;
            }
        }

        /// <summary>
        /// Получить суммарную длительность книги в секундах.
        /// </summary>
        /// <returns>Суммарная длительность книги в секундах</returns>
        public double GetTotalLength()
        {
            var result = 0D;

            foreach (var item in Playlist)
                result += item.Duration;

            return result;
        }
        
        public string ToString(bool addPlaylist = false)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"Название книги: {Title}");
            builder.AppendLine($"Описание: {Description}");
            builder.AppendLine($"Список воспроивзедения({GetTotalLength(TimeUnitEnum.MINUTES)} минут):");

            if(addPlaylist)
            {
                foreach (var item in Playlist.Select((x, i) => $"{i}: {x.Title}, Длительность:{x.GetDurationInMinutes()}"))
                {
                    builder.AppendLine(item);
                }
            }

            return builder.ToString();
        }
    }

    /// <summary>
    /// Элемент книги.
    /// </summary>
    public class BookElement
    {
        /// <summary>
        /// Ссылка на файл.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Название файла.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Была ли книга загружена.
        /// </summary>
        public bool IsDownloaded { get; set; } = false;

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Длительность файла в секундах.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Получить длительность файла в минутах
        /// </summary>
        /// <returns>Длительность файла в минутах</returns>
        public double GetDurationInMinutes()
        {
            return Math.Round(((double)Duration) / 60, 2);
        }

        /// <summary>
        /// Получить длительность файла в минутах
        /// </summary>
        /// <returns>Длительность файла в минутах</returns>
        public double GetDurationInSeconds()
        {
            return Duration;
        }
    }
}
