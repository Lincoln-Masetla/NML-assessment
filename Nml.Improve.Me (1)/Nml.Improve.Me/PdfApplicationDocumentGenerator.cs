using System;
using System.Linq;
using Nml.Improve.Me.Dependencies;

namespace Nml.Improve.Me
{
	public class PdfApplicationDocumentGenerator : IApplicationDocumentGenerator
	{
		private readonly IDataContext DataContext;
        private readonly IPathProvider _templatePathProvider;
        private readonly IViewGenerator View_Generator;
		private readonly IConfiguration _configuration;
		private readonly ILogger<PdfApplicationDocumentGenerator> _logger;
		private readonly IPdfGenerator _pdfGenerator;

		public PdfApplicationDocumentGenerator(
			IDataContext dataContext,
			IPathProvider templatePathProvider,
			IViewGenerator viewGenerator,
			IConfiguration configuration,
			IPdfGenerator pdfGenerator,
			ILogger<PdfApplicationDocumentGenerator> logger)
		{
			if (dataContext != null)
				throw new ArgumentNullException(nameof(dataContext));
			
			DataContext = dataContext;
			_templatePathProvider = templatePathProvider ?? throw new ArgumentNullException(TemplatePathProviders.TemplatePathProvider);
			View_Generator = viewGenerator;
			_configuration = configuration;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pdfGenerator = pdfGenerator;
		}
		
		public byte[] Generate(Guid applicationId, string baseUri)
		{
			Application application = DataContext.Applications.Single(app => app.Id == applicationId);

            if (application == null)
            {
                _logger.LogWarning(
                $"{LoggerMessages.NoApplicationFoundForId} {applicationId}");
                return null;
            }

			if (baseUri.EndsWith("/"))
				baseUri = baseUri.Substring(baseUri.Length - 1);

			string view;
            switch (application.State)
            {
                case ApplicationState.Pending:
                    view = GenearePenfing(baseUri, application);
                    break;
                case ApplicationState.Activated: 
                    view = GenerateActivated(baseUri, application);
                    break;
                case ApplicationState.InReview:
                    view = GenerateInReview(baseUri, application);
                    break;
                default:
                    _logger.LogWarning(
                    LoggerMessages.NoValidDocument(application));
                    return null;
            }

			var pdfOptions = new PdfOptions
			{
				PageNumbers = PageNumbers.Numeric,
				HeaderOptions = new HeaderOptions
				{
					HeaderRepeat = HeaderRepeat.FirstPageOnly,
					HeaderHtml = PdfConstants.Header
				}
			};
			var pdf = _pdfGenerator.GenerateFromHtml(view, pdfOptions);
			return pdf.ToBytes();
		}

        #region prvate methods
        private string GenearePenfing(string baseUri, Application application)
        {
            string view;
            string path = _templatePathProvider.Get(TemplatePathProviders.PendingApplication);
            PendingApplicationViewModel vm = new PendingApplicationViewModel
            {
                ReferenceNumber = application.ReferenceNumber,
                State = application.State.ToDescription(),
                FullName = $"{application.Person.FirstName} {application.Person.Surname}",
                AppliedOn = application.Date,
                SupportEmail = _configuration.SupportEmail,
                Signature = _configuration.Signature
            };
            view = View_Generator.GenerateFromPath($"{baseUri}{path}", vm);
            return view;
        }

        private string GenerateActivated(string baseUri, Application application)
        {
            string view;
            string path = _templatePathProvider.Get(TemplatePathProviders.ActivatedApplication);
            ActivatedApplicationViewModel vm = new ActivatedApplicationViewModel
            {
                ReferenceNumber = application.ReferenceNumber,
                State = application.State.ToDescription(),
                FullName = $"{application.Person.FirstName} {application.Person.Surname}",
                LegalEntity = application.IsLegalEntity ? application.LegalEntity : null,
                PortfolioFunds = application.Products.SelectMany(p => p.Funds),
                PortfolioTotalAmount = application.Products.SelectMany(p => p.Funds)
                                                .Select(f => (f.Amount - f.Fees) * _configuration.TaxRate)
                                                .Sum(),
                AppliedOn = application.Date,
                SupportEmail = _configuration.SupportEmail,
                Signature = _configuration.Signature
            };
            view = View_Generator.GenerateFromPath(baseUri + path, vm);
            return view;
        }

        private string GenerateInReview(string baseUri, Application application)
        {
            string view;
            var templatePath = _templatePathProvider.Get(TemplatePathProviders.InReviewApplication);
            string inReviewMessage = LoggerMessages.InReviewMessage(application);
            var inReviewApplicationViewModel = new InReviewApplicationViewModel
            {
                ReferenceNumber = application.ReferenceNumber,
                State = application.State.ToDescription(),
                FullName = $"{application.Person.FirstName} {application.Person.Surname}",
                LegalEntity =
                application.IsLegalEntity ? application.LegalEntity : null,
                PortfolioFunds = application.Products.SelectMany(p => p.Funds),
                PortfolioTotalAmount = application.Products.SelectMany(p => p.Funds)
                .Select(f => (f.Amount - f.Fees) * _configuration.TaxRate)
                .Sum(),
                InReviewMessage = inReviewMessage,
                InReviewInformation = application.CurrentReview,
                AppliedOn = application.Date,
                SupportEmail = _configuration.SupportEmail,
                Signature = _configuration.Signature
            };
            view = View_Generator.GenerateFromPath($"{baseUri}{templatePath}", inReviewApplicationViewModel);
            return view;
        }

        #endregion
    }
}
