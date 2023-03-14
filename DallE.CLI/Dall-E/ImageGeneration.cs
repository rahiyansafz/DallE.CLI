using System.Diagnostics;
using System.Text;
using System.Text.Json;

using DallE.CLI.Callback;

namespace DallE.CLI.Dall_E;
internal class ImageGeneration
{
    private const string _apiKey = Settings.APIKEY;
    private const string _modelName = Settings.MODEL_NAME;
    private const string _imagesFolderName = Settings.IMAGE_FOLDER_NAMEs;

    internal static async Task GenerateImage()
    {
        Console.WriteLine("Enter your prompt:");
        var userInput = Console.ReadLine();
        Console.WriteLine();
        Console.WriteLine("Enter the number of images you want:");
        var numImagesInput = Console.ReadLine();
        if (!int.TryParse(numImagesInput, out var numImages))
        {
            Console.WriteLine($"Invalid input: {numImagesInput} is not a valid number.");
            return;
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var requestBody = JsonSerializer.Serialize(new { model = _modelName, prompt = userInput, num_images = numImages });
        var requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

        Console.WriteLine();
        Console.WriteLine("Fetching image data...");
        var response = await client.PostAsync("https://api.openai.com/v1/images/generations", requestContent);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to call the API. Status code: {response.StatusCode}");
            return;
        }

        Console.WriteLine("Processing image data...");
        var responseContent = await response.Content.ReadAsStringAsync();
        DallEResponse? responseData = JsonSerializer.Deserialize<DallEResponse>(responseContent);

        if (responseData?.Data is not null)
        {
            var imagesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _imagesFolderName);
            if (!Directory.Exists(imagesFolderPath))
                Directory.CreateDirectory(imagesFolderPath);

            int savedImagesCount = 0;

            foreach (var data in responseData.Data)
            {
                using var client2 = new HttpClient();
                using var response2 = await client2.GetAsync(data.Url);
                if (response2.IsSuccessStatusCode)
                {
                    using var imageStream = await response2.Content.ReadAsStreamAsync();
                    var fileName = $"{DateTime.Now:yyyyMMddHHmmssfff}.png";
                    var filePath = Path.Combine(imagesFolderPath, fileName);
                    using var imageFile = new FileStream(filePath, FileMode.Create);
                    await imageStream.CopyToAsync(imageFile);
                    savedImagesCount++;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"{savedImagesCount} out of {numImages} images saved successfully in {imagesFolderPath}.");
            Console.WriteLine();

            Console.WriteLine("Do you want to open the folder now? (Press Enter to open or press Y and then Enter)");
            var keyInfo = Console.ReadKey();
            Console.WriteLine();

            if (keyInfo.Key == ConsoleKey.Enter || (keyInfo.Key == ConsoleKey.Y && keyInfo.Modifiers == 0))
                try
                {
                    Console.WriteLine($"Opening {imagesFolderPath}...");
                    Process.Start("explorer.exe", imagesFolderPath);
                }
                catch (Exception ex) { Console.WriteLine($"Failed to open the folder: {ex.Message}"); }

            Console.WriteLine();
        }
        else
            Console.WriteLine("No image data found in the API response.");
    }
}