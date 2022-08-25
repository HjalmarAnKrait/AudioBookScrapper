using HtmlAgilityPack;
using AudioBookScrapper.Model;
using System.Text.Json;

namespace AudioBookScrapper
{
    /// <summary>
    /// Парсер списка аудиокниг с сайта KnigaVUhe.
    /// </summary>
    public class AudioBookParser
    {
        /// <summary>
        /// Получить аудиокнигу с ссылке 
        /// </summary>
        /// <param name="url">Ссылка на книгу с "Книга в ухе"</param>
        /// <returns>Аудиокнига. Null, если ничего не найдено.</returns>
        public static async Task<AudioBook> ParseBookFromUrl(string url)
        {
            var gettedHtml = await CallUrl(url);
            var script = GetAudioBookPlayerScript(gettedHtml);
            var booksJson = ExctractArrayFromAudioBookPlayer(script);
            var jArray = DeserializeJsonArray(booksJson);
            var description = GetBookDescription(gettedHtml);
            var title = GetBookTitle(gettedHtml);

            var bookPlayList = new List<BookElement>();

            foreach (var item in jArray)
            {
                bookPlayList.Add(new BookElement
                {
                    Title = item.GetProperty("title").ToString(),
                    Url = item.GetProperty("url").ToString(),
                    Duration = item.GetProperty("duration").GetInt32(),
                    Id = item.GetProperty("id").GetInt32()
                });
            }

            if (bookPlayList.Count <= 0)
                return null;

            return new AudioBook
            {
                Description = description,
                Title = title,
                Playlist = bookPlayList,
                Url = url
            };
        }

        /// <summary>
        /// Получить описание книги.
        /// </summary>
        /// <param name="html">Страница</param>
        /// <returns>Описание книги</returns>
        private static string GetBookDescription(string html)
        {
            var startTag = "<div class=\"book_description\" itemprop=\"description\">";
            var description = html.Substring(html.IndexOf(startTag) + startTag.Length);
            description = description.Remove(description.IndexOf("</div>"));
            description = description.Replace("<br />", String.Empty);

            return description;
        }

        /// <summary>
        /// Получить название книги.
        /// </summary>
        /// <param name="html">Страница</param>
        /// <returns>Название книги.</returns>
        private static string GetBookTitle(string html)
        {
            var startTag = "<meta property=\"og:title\" content=";
            var title = html.Substring(html.IndexOf(startTag) + startTag.Length + 1);
            title = title.Remove(title.IndexOf(" />") - 1 );

            return title;
        }

        /// <summary>
        /// Получить содержимое страницы по url
        /// </summary>
        /// <param name="url">url страницы с книгой</param>
        /// <returns>Содержимое страницы.</returns>
        private static async Task<string> CallUrl(string url)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Получение JS-элемента с аудиокнигами
        /// </summary>
        /// <param name="html"></param>
        /// <returns>содержимое JS-элемента с плеером книг.</returns>
        private static string GetAudioBookPlayerScript(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var script = htmlDoc?.DocumentNode?
                .Descendants("script")?
                .ToList()?
                .FirstOrDefault(x => x.InnerText
                .Contains("new BookPlayer"))?.InnerText;

            return script;
        }

        /// <summary>
        /// Извлечь текстовый массив из скрипта с аудиокнигой.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        private static string ExctractArrayFromAudioBookPlayer(string script)
        {
            var booksAudioJsonArray = script.Substring(script.IndexOf("var player = new BookPlayer"));
            booksAudioJsonArray = booksAudioJsonArray.Substring(booksAudioJsonArray.IndexOf('['));
            booksAudioJsonArray = booksAudioJsonArray.Remove(booksAudioJsonArray.IndexOf(']') + 1);

            return booksAudioJsonArray;
        }

        /// <summary>
        /// Десереализация json-массива.
        /// </summary>
        /// <param name="jsonArray">json-массив</param>
        /// <returns>Десереализованный массив JsonElement</returns>
        /// <exception cref="Exception">Если не десереализовалось.</exception>
        private static List<JsonElement> DeserializeJsonArray(string jsonArray)
        {
            return JsonSerializer.Deserialize<List<JsonElement>>(jsonArray) ?? throw new Exception($"Cannot deserialize {jsonArray}");
        }
    }
}
