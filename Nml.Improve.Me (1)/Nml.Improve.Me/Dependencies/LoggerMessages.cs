using System;
using System.Collections.Generic;
using System.Text;

namespace Nml.Improve.Me.Dependencies
{
    public static class LoggerMessages
    {
        public const string NoApplicationFoundForId = "No application found for id ";
        public static string NoValidDocument(Application application)
        {
            return $"The application is in state '{application.State}' and no valid document can be generated for it.";
        }

        public static string InReviewMessage(Application application)
        {
            return "Your application has been placed in review" +
                                application.CurrentReview.Reason switch
                                {
                                    { } reason when reason.Contains("address") =>
                                        " pending outstanding address verification for FICA purposes.",
                                    { } reason when reason.Contains("bank") =>
                                        " pending outstanding bank account verification.",
                                    _ =>
                                        " because of suspicious account behaviour. Please contact support ASAP."
                                };
        }
    }
}
