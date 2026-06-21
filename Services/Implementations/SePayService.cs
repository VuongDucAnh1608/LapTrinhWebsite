using System.Security.Cryptography;
using System.Text;

namespace Website_QuanLyKhoHangThucPham.Services
{
    public class SePayService : ISePayService
    {
        private readonly IConfiguration _cfg;

        public SePayService(IConfiguration cfg) => _cfg = cfg;

        private string BankId      => _cfg["SePay:BankId"]      ?? "MB";
        private string AccountNo   => _cfg["SePay:AccountNo"]   ?? "0000000000";
        private string AccountName => _cfg["SePay:AccountName"] ?? "WAREHOUSE";
        private string Template    => _cfg["SePay:Template"]    ?? "compact2";
        private string Secret      => _cfg["SePay:WebhookSecret"] ?? "secret";


        public string GenerateQrUrl(string orderCode, long amount)
        {
            var content = GenerateTransferContent(orderCode);
            var encodedName    = Uri.EscapeDataString(AccountName);
            var encodedContent = Uri.EscapeDataString(content);
            return $"https://img.vietqr.io/image/{BankId}-{AccountNo}-{Template}.png" +
                   $"?amount={amount}&addInfo={encodedContent}&accountName={encodedName}";
        }

        public string GenerateTransferContent(string orderCode)
            => $"TT {orderCode}";

        public bool VerifyWebhook(string rawBody, string signature)
        {
            if (string.IsNullOrEmpty(signature)) return false;
            var key  = Encoding.UTF8.GetBytes(Secret);
            var data = Encoding.UTF8.GetBytes(rawBody);
            using var hmac = new HMACSHA256(key);
            var hash = BitConverter.ToString(hmac.ComputeHash(data))
                                   .Replace("-", "").ToLower();
            return hash == signature.ToLower();
        }
    }
}
