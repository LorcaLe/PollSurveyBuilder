namespace PollSurveyBuilder.Application.IServices
{
    public interface IQRCodeService
    {
        /// <summary>Returns a data:image/png;base64,... string for the given URL, ready to use in an &lt;img src&gt;.</summary>
        string GenerateBase64(string url);
    }
}
