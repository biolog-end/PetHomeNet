using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Configuration;

namespace PetHome.Services
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }

    public class SmsService : ISmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _phoneNumber;

        public SmsService(IConfiguration configuration)
        {
            _accountSid = configuration["Twilio:AccountSid"];
            _authToken = configuration["Twilio:AuthToken"];
            _phoneNumber = configuration["Twilio:PhoneNumber"];

            TwilioClient.Init(_accountSid, _authToken);
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            var messageOptions = new CreateMessageOptions(new Twilio.Types.PhoneNumber(phoneNumber))
            {
                From = new Twilio.Types.PhoneNumber(_phoneNumber),
                Body = message
            };

            await MessageResource.CreateAsync(messageOptions);
        }
    }
}