// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using live.asp.net.Models;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace live.asp.net.Formatters
{
    public class iCalendarOutputFormatter : OutputFormatter
    {
        private static Task _done = Task.FromResult(0);

        private bool _noNextShow = false;

        public iCalendarOutputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/calendar"));
        }

        protected override bool CanWriteType(Type declaredType, Type runtimeType)
        {
            return declaredType == typeof(LiveShowDetails) || runtimeType == typeof(LiveShowDetails);
        }

        public override void WriteResponseHeaders(OutputFormatterContext context)
        {
            var liveShowDetails = context.Object as LiveShowDetails;

            Debug.Assert(context.Object == null || liveShowDetails != null, $"Object to be formatted should be of type {nameof(LiveShowDetails)}");

            if (liveShowDetails == null || liveShowDetails.NextShowDateUtc == null)
            {
                _noNextShow = true;
                context.HttpContext.Response.StatusCode = StatusCodes.Status204NoContent;
            }
            else
            {
                base.WriteResponseHeaders(context);
            }
        }

        public override Task WriteResponseBodyAsync(OutputFormatterContext context)
        {
            var liveShowDetails = context.Object as LiveShowDetails;

            Debug.Assert(context.Object == null || liveShowDetails != null, $"Object to be formatted should be of type {nameof(LiveShowDetails)}");

            if (_noNextShow)
            {
                return _done;
            }

            return WriteLiveShowDetailsToResponseBody(liveShowDetails.NextShowDateUtc, context.HttpContext.Response);
        }
        
        private static string _dateTimeFormat = "yyyyMMddTHHmmssZ";

        private static Task WriteLiveShowDetailsToResponseBody(DateTime? nextShowDateUtc, HttpResponse response)
        {
            // Internet Calendaring and Scheduling Core Object Specification (iCalendar): https://tools.ietf.org/html/rfc5545
            /* BEGIN:VCALENDAR
               VERSION:2.0
               BEGIN:VEVENT
               UID:vscordovatools@microsoft.com
               DESCRIPTION:A live stream broadcast with the Visual Studio Tools for Apache Cordova team, answering your questions.
               DTSTART:20150804T170000Z
               DTEND:20150804T035959Z
               LOCATION:http://taco.visualstudio.com/live
               SUMMARY:Visual Studio Tools for Apache Cordova, Live Q&A
               END:VEVENT
               END:VCALENDAR */

            return response.WriteAsync(
                "BEGIN:VCALENDAR\r\n" +
                "VERSION:2.0\r\n" +
                "BEGIN:VEVENT\r\n" +
                "UID:vscordovatools@microsoft.com\r\n" +
                "DTSTART:" + nextShowDateUtc?.ToString(_dateTimeFormat) + "\r\n" +
                "DTEND:" + nextShowDateUtc?.AddMinutes(30).ToString(_dateTimeFormat) + "\r\n" +
                "SUMMARY:Visual Studio Tools for Apache Cordova, Live Q&A\r\n" +
                "DESCRIPTION:A live stream broadcast with the Visual Studio Tools for Apache Cordova team, answering your questions.\r\n" +
                "LOCATION:http://taco.visualstudio.com/live/\r\n" +
                "END:VEVENT\r\n" +
                "END:VCALENDAR\r\n");
        }
    }
}
