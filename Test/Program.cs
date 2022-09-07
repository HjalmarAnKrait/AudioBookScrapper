using AudioBookScrapper;
using AudioBookScrapper.Model;
using NAudio.Wave;
using System.Net;

var book = await AudioBookParser.ParseBookFromUrl("https://knigavuhe.org/book/stalker-pervyjj-stalker/");
Console.WriteLine(book.ToString());
//var a = new System.Windows.Forms
await DownloadAllBook(book);

while(book.Playlist.Any(x=> !x.IsDownloaded)) { }
static void PlayFile(AudioBook book)
{
    var bookPart = book.Playlist.First();

    using (var mf = new MediaFoundationReader(bookPart.Url))
    using (var wo = new WasapiOut())
    {
        wo.Init(mf);
        wo.Play();
        while (wo.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(1000);
            Console.Clear();
            Console.WriteLine($"Сейчас играет \"{bookPart.Title}\" {mf.CurrentTime.Seconds}:{mf.TotalTime.TotalSeconds}");
            Console.WriteLine(book.ToString());
        }
    }
}

static async Task DownloadAllBook(AudioBook audioBook)
{
    for(var i = 0; i < audioBook.Playlist.Count; i++)
    {
        Console.Clear();
        Console.WriteLine($"{audioBook.Title}");
        Console.WriteLine($"Загрузка файла {i+1} из {audioBook.Playlist.Count}");
        var book = audioBook.Playlist[i];
        await Download(book, audioBook.Title);
        book.IsDownloaded = true;
    }
}

static async Task Download(BookElement part, string bookTitle)
{
    using var client = new HttpClient();
    using var s = await client.GetStreamAsync(part.Url);

    string path = $"C:\\Users\\TarabrinEO\\Desktop\\books\\{bookTitle}\\";
    Directory.CreateDirectory(path);
    using var fs = new FileStream(Path.Combine(path, $"{part.Title}.mp3"), FileMode.OpenOrCreate);
    await s.CopyToAsync(fs);
}