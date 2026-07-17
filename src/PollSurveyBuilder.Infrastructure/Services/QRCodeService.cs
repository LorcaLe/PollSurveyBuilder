using PollSurveyBuilder.Application.IServices;
using QRCoder;

namespace PollSurveyBuilder.Infrastructure.Services
{
    /// <summary>
    /// Generates QR codes in memory as base64 PNG data URIs - no files are written to
    /// disk, which keeps the API container stateless (important since it may run as
    /// multiple replicas behind the PaaS load balancer).
    /// </summary>
    public class QRCodeService : IQRCodeService
    {
        public string GenerateBase64(string url)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            var bytes = qr.GetGraphic(12);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }
    }
}
