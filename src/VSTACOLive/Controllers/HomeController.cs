// Copyright (c) .NET Foundation. All rights reserved. 
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using live.asp.net.Models;
using live.asp.net.Services;
using live.asp.net.ViewModels;
using Microsoft.AspNet.Mvc;
using System.Collections.Generic;

namespace live.asp.net.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILiveShowDetailsService _liveShowDetails;
        private readonly IShowsService _showsService;

        public HomeController(IShowsService showsService, ILiveShowDetailsService liveShowDetails)
        {
            _showsService = showsService;
            _liveShowDetails = liveShowDetails;
        }

        [Route("/")]
        public async Task<IActionResult> Index(bool? disableCache, bool? demoOnAir)
        {
            var liveShowDetails = await _liveShowDetails.LoadAsync();
            var showList = new ShowList() { Shows = new List<Show>() };// await _showsService.GetRecordedShowsAsync(User, disableCache ?? false);

            if (demoOnAir == true)
            {
                liveShowDetails = new LiveShowDetails();
                liveShowDetails.LiveShowEmbedUrl = "tbd";
            }

            return View(new HomeViewModel
            {
                AdminMessage = liveShowDetails?.AdminMessage,
                NextShowDateUtc = liveShowDetails?.NextShowDateUtc,
                LiveShowEmbedUrl = liveShowDetails?.LiveShowEmbedUrl,
                PreviousShows = showList.Shows,
                MoreShowsUrl = showList.MoreShowsUrl
            });
        }

        [HttpGet("/ical")]
        [Produces("text/calendar")]
        public async Task<LiveShowDetails> GetiCal()
        {
            var liveShowDetails = await _liveShowDetails.LoadAsync();

            return liveShowDetails;
        }

        [Route("/live/error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
