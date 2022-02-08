using System;
using System.Threading.Tasks;
using Nml.Refactor.Me.Dependencies;
using Nml.Refactor.Me.MessageBuilders;

namespace Nml.Refactor.Me.Notifiers
{
    abstract class SmsNotifier : SmsApiClient, INotifier
    {

		private readonly IStringMessageBuilder _messageBuilder;
		private readonly IOptions _options;
		private readonly ILogger _logger = LogManager.For<SmsNotifier>();
        private readonly string _apiUrl;
        private readonly string _apiToken;

        public SmsNotifier(IStringMessageBuilder messageBuilder, IOptions options, string apiUrl, string apiToken) : base(apiUrl, apiToken)
        {
            _messageBuilder = messageBuilder ?? throw new ArgumentNullException(nameof(messageBuilder));
            _options = options ?? throw new ArgumentNullException(nameof(options));

        }

        public async Task Notify(NotificationMessage message)
		{
            //Complete after refactoring inheritance. Use "SmsApiClient"
            await new SmsApiClient(_options.Sms.ApiKey, _options.Sms.ApiUri).SendAsync(message: message.Body, mobileNumber: message.To);

        }
	}
}
