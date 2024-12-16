using System.Globalization;

partial class Program
{
    public interface ITranslator
    {
        Task<string> Translate(CultureInfo from, CultureInfo to, string value, CancellationToken cancellationToken);
    }
}
