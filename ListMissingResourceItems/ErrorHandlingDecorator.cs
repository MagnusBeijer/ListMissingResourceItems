using System.Net;

namespace ListMissingResourceItems;
public static class ErrorHandlingDecorator
{
    public static async Task<T> ExecuteWithHandling<T>(Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode == (HttpStatusCode)456)
        {
            Console.WriteLine("Quota exceeded. Please try again later.");
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode == (HttpStatusCode)429)
        {
            Console.WriteLine("Too many requests. Please try again later.");
        }
        catch (HttpRequestException httpEx) when (httpEx.StatusCode == (HttpStatusCode)500)
        {
            Console.WriteLine("Internal server error. Please try again later.");
        }
        catch (HttpRequestException httpEx)
        {
            Console.WriteLine($"Network error: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return default(T); // or handle it as you need
    }
}
